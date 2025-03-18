using Application.DTO.Request;
using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infrastructure
{
    public class ExcelHandler : IExcelService

    {
        private readonly IExcelRepo _excelService;
        private readonly IConnectionMultiplexer _redis;
        private readonly IMapper _mapper;
        private const string CacheKey = "UserAccounts";

        public ExcelHandler(IExcelRepo excelService, IConnectionMultiplexer redis, IMapper mapper)
        {
            _excelService = excelService;
            _redis = redis;
            _mapper = mapper;
        }

        public async Task<Pagination<ProductExcelRequestDTO>> GetAllProduct(PaginationParameter paginationParameter)
        {
            try
            {
                var cacheKey = "UserAccounts";
                var db = _redis.GetDatabase(); // Ensure db is of type IDatabase

                // if null cache, get data from db and write it down cache
                var trips = await _excelService.GetAllProduct(paginationParameter);
                if (!trips.Any())
                {
                    throw new Exception("No data!");
                }

                var tripModels = _mapper.Map<List<ProductExcelRequestDTO>>(trips);

                // write down cache
                var paginationResult = new Pagination<ProductExcelRequestDTO>(tripModels,
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

        public async Task<List<CreateProductDTORequest>> CreateProduct(List<CreateProductDTORequest> user)
        {
            try
            {
                var map = _mapper.Map<List<Product>>(user);
                var userCreate = await _excelService.CreateProduct(map);
                var result = _mapper.Map<List<CreateProductDTORequest>>(userCreate);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
