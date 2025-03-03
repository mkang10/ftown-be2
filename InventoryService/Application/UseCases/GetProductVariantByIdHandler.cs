using Application.DTO.Response;
using AutoMapper;
using Azure;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetProductVariantByIdHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public GetProductVariantByIdHandler(IProductRepository productRepository, IRedisCacheService cacheService, IMapper mapper)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<ProductVariantResponse?> Handle(int variantId)
        {
            string cacheKey = $"variant:{variantId}";

            // 🔍 Kiểm tra cache trước khi gọi database
            //var cachedVariant = await _cacheService.GetCacheAsync<ProductVariantResponse>(cacheKey);
            //if (cachedVariant != null)
            //    return cachedVariant;

            // ❌ Không có cache, truy vấn database
            var productVariant = await _productRepository.GetProductVariantByIdAsync(variantId);
            if (productVariant == null)
                return null;
            int stockQuantity = await _productRepository.GetProductVariantStockAsync(variantId);
            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;
            // ✅ Lưu vào cache với TTL 30 phút
            await _cacheService.SetCacheAsync(cacheKey, variantResponse, TimeSpan.FromMinutes(30));

            return variantResponse;
        }
    }

}
