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
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class FeedbackHandler : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private const string CacheKey = "Data";
        private readonly IMapper _mapper;
        private readonly IConnectionMultiplexer _redis;

        public FeedbackHandler(ICommentRepository commentRepository, IMapper mapper, IConnectionMultiplexer redis)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _redis = redis;
        }

        public async Task<CreateFeedBackRequestDTO> Create(CreateFeedBackRequestDTO user)
        {
            try
            {
                var map = _mapper.Map<Feedback>(user);
                var userCreate = await _commentRepository.CreateFeedback(map);
                var result = _mapper.Map<CreateFeedBackRequestDTO>(userCreate);
                return result;
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

                //// check cache null or not ?
                //var cachedData = await db.StringGetAsync(cacheKey);
                //if (cachedData.HasValue)
                //{
                //    // if not null , deserialize object
                //    var cachedResult = JsonConvert.DeserializeObject<Pagination<FeedbackRequestDTO>>(cachedData);
                //    return cachedResult;
                //}

                // if null cache, get data from db and write it down cache
                var trips = await _commentRepository.GettAllCommentByAccountId(id ,paginationParameter);
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
