using Application.DTO.Request;
using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Order = Domain.Entities.Order;

namespace Application.UseCases
{
    public class FeedbackHandler : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        private const string CacheKey = "Data";
        private readonly IMapper _mapper;
        private readonly IConnectionMultiplexer _redis;
        private readonly HttpClient _httpClient;


        public FeedbackHandler(HttpClient httpClient, ICommentRepository commentRepository, IOrderDetailRepository orderDetailRepository , IMapper mapper, IConnectionMultiplexer redis)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _redis = redis;
            _httpClient = httpClient;
            _orderDetailRepository = orderDetailRepository;

        }

        public async Task<CreateFeedBackRequestDTO> Create(CreateFeedBackRequestDTO request)
        {
            try
            {
                // Kiểm tra xem OrderDetailId đã được cung cấp chưa.
                if (!request.orderDetailId.HasValue)
                    throw new Exception("OrderDetailId phải được cung cấp khi tạo feedback.");

                // Lấy thông tin OrderDetail dựa vào OrderDetailId.
                var orderDetail = await _orderDetailRepository.GetOrderDetailById(request.orderDetailId.Value);

                // Kiểm tra xem OrderDetail tồn tại và đơn hàng liên quan có trạng thái "completed".
                if (orderDetail == null || orderDetail.Order == null ||
                    !string.Equals(orderDetail.Order.Status, "completed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Feedback chỉ được tạo cho đơn hàng có trạng thái completed.");
                }

                // Map DTO sang entity Feedback.
                var feedbackEntity = _mapper.Map<Feedback>(request);

                // Tạo feedback qua repository.
                var createdFeedback = await _commentRepository.CreateFeedback(feedbackEntity);

                // Map entity vừa tạo về DTO và trả về.
                return _mapper.Map<CreateFeedBackRequestDTO>(createdFeedback);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<bool> Delete(int id)
        {
            try
            {
                var user = await _commentRepository.GetFeedBackById(id);
                if (user == null)
                {
                    throw new Exception($"Feedback {id} does not exist");
                }

                await _commentRepository.DeleteFeedback(user);
                // call redis and delete 1 of item in cache
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync("Data");

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Pagination<FeedbackRequestDTO>> GetAllFeedbackByProductId(int id, PaginationParameter paginationParameter)
        {
            try
            {
                var cacheKey = "Data";
                var db = _redis.GetDatabase();

                //// check cache null or not ?
                //var cachedData = await db.StringGetAsync(cacheKey);
                //if (cachedData.HasValue)
                //{
                //    // if not null , deserialize object
                //    var cachedResult = JsonConvert.DeserializeObject<Pagination<FeedbackRequestDTO>>(cachedData);
                //    return cachedResult;
                //}

                // if null cache, get data from db and write it down cache
                var trips = await _commentRepository.GettAllFeedbackByProductId(id, paginationParameter);
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                var tripModels = _mapper.Map<List<FeedbackRequestDTO>>(trips);

                // write down cache
                var paginationResult = new Pagination<FeedbackRequestDTO>(tripModels,
                    trips.TotalCount,
                    trips.CurrentPage,
                    trips.PageSize);
                await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(paginationResult), TimeSpan.FromMinutes(300));

                return paginationResult;
            }

            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        public async Task<FeedbackRequestDTO> GetById(int id)
        {
            try
            {
                var data = await _commentRepository.GetFeedBackById(id);
                if (data == null)
                {
                    throw new Exception("Feedback does not exsist!");
                }
                var dataModel = _mapper.Map<FeedbackRequestDTO>(data);

                return dataModel;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occur: " + ex.Message);
            }
        }

        public async Task<Pagination<FeedbackRequestDTO>> GettAllFeedbackByAccountId(int id, PaginationParameter paginationParameter)
        {
            try
            {
                var cacheKey = "Data";
                var db = _redis.GetDatabase();

                // Nếu có dữ liệu trong cache, có thể deserialize và trả về (đoạn này đang comment)
                // var cachedData = await db.StringGetAsync(cacheKey);
                // if (cachedData.HasValue)
                // {
                //     var cachedResult = JsonConvert.DeserializeObject<Pagination<FeedbackRequestDTO>>(cachedData);
                //     return cachedResult;
                // }

                // Lấy dữ liệu từ repository
                var trips = await _commentRepository.GettAllCommentByAccountId(id, paginationParameter);

                // Kiểm tra null cho trips và trips.Items
                if (trips == null || trips.Items == null || !trips.Items.Any())
                {
                    throw new Exception("No data!");
                }

                // Map danh sách Feedback sang FeedbackRequestDTO
                var tripModels = _mapper.Map<List<FeedbackRequestDTO>>(trips.Items);

                // Tạo đối tượng Pagination<FeedbackRequestDTO> mới dựa trên các thuộc tính của trips
                var paginationResult = new Pagination<FeedbackRequestDTO>(
                    tripModels,
                    trips.TotalCount,
                    trips.CurrentPage,
                    trips.PageSize);

                // Ghi cache kết quả vào Redis trong 300 phút
                await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(paginationResult), TimeSpan.FromMinutes(300));

                return paginationResult;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }


        public async Task<bool> Update(int id, UpdateFeedbackRequestDTO user)
        {
            try
            {
                var userData = await _commentRepository.GetFeedBackById(id);
                if (userData == null)
                {
                    throw new Exception("No data!");
                }

                _mapper.Map(user, userData);

                await _commentRepository.UpdateFeedback(userData);

                var db = _redis.GetDatabase();
                //delete old cache
                await db.KeyDeleteAsync("Data");
                var paginationParameter = new PaginationParameter();
                // call and wite new cache to redis
                var updatedUsers = await _commentRepository.GettAllFeedbackByProductId(user.ProductId ,paginationParameter);
                await db.StringSetAsync("Data", JsonConvert.SerializeObject(updatedUsers), TimeSpan.FromMinutes(300));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }
    }
}
