using Application.DTO.Request;
using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var db = _redis.GetDatabase();

                // check cache null or not ?
                var cachedData = await db.StringGetAsync(cacheKey);
                if (cachedData.HasValue)
                {
                    // if not null , deserialize object
                    var cachedResult = JsonConvert.DeserializeObject<Pagination<ProductExcelRequestDTO>>(cachedData);
                    return cachedResult;
                }

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
    }
}
