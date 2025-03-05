using Application.DTO.Response;
using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllProductsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper, IRedisCacheService cacheService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<List<ProductListResponse>> Handle(int page, int pageSize)
        {
            string cacheKey = $"products:view-all:page:{page}:size:{pageSize}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedProducts = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            // ❌ Nếu cache không có, gọi Repository để lấy dữ liệu từ database
            var products = await _productRepository.GetPagedProductsWithVariantsAsync(page, pageSize);

            if (products == null || !products.Any())
                return new List<ProductListResponse>();

            // ⚡ Dùng AutoMapper để chuyển đổi Entity -> DTO
            var productList = _mapper.Map<List<ProductListResponse>>(products);

            // ✅ Lưu vào cache với TTL 10 phút
            await _cacheService.SetCacheAsync(cacheKey, productList, TimeSpan.FromMinutes(10));

            return productList;
        }
    }

}
