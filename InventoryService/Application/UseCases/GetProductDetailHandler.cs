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
    public class GetProductDetailHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public GetProductDetailHandler(IProductRepository productRepository, IRedisCacheService cacheService, IMapper mapper)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<ProductDetailResponse?> Handle(int productId)
        {
            string cacheKey = $"product:{productId}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedProduct = await _cacheService.GetCacheAsync<ProductDetailResponse>(cacheKey);
            if (cachedProduct != null)
                return cachedProduct;

            // ❌ Không có cache, truy vấn database
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;

            var productDetail = _mapper.Map<ProductDetailResponse>(product);

            // ✅ Lưu vào cache với TTL 30 phút
            await _cacheService.SetCacheAsync(cacheKey, productDetail, TimeSpan.FromMinutes(30));

            return productDetail;
        }
    }

}
