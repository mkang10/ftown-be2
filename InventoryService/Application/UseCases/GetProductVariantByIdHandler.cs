using Application.DTO.Response;
using AutoMapper;
using Azure;
using Domain.Entities;
using Domain.Interfaces;
using Newtonsoft.Json;
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
        private readonly IPromotionRepository _promotionRepository;
        public GetProductVariantByIdHandler(IProductRepository productRepository, IRedisCacheService cacheService, IMapper mapper, IPromotionRepository promotionRepository)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
        }

        public async Task<ProductVariantResponse?> Handle(int variantId)
        {
            string instanceName = "ProductInstance";
            string cacheKey = $"{instanceName}:variant:{variantId}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedVariant = await _cacheService.GetCacheAsync<ProductVariantResponse>(cacheKey);
            if (cachedVariant != null)
                return cachedVariant;

            // ❌ Không có cache, truy vấn database
            var productVariant = await _productRepository.GetProductVariantByIdAsync(variantId);
            if (productVariant == null)
                return null;

            int stockQuantity = await _productRepository.GetProductVariantStockAsync(variantId);
            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;

            // 🔹 Lấy danh sách khuyến mãi đang hoạt động
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            // 🔥 Kiểm tra sản phẩm có khuyến mãi không
            var applicablePromotion = promotions.FirstOrDefault(p =>
                !string.IsNullOrEmpty(p.ApplyValue) &&
                JsonConvert.DeserializeObject<List<int>>(p.ApplyValue).Contains(productVariant.ProductId));

            if (applicablePromotion != null)
            {
                decimal discountedPrice = variantResponse.Price;

                if (applicablePromotion.DiscountType == "PERCENTAGE")
                {
                    decimal discountAmount = (variantResponse.Price * applicablePromotion.DiscountValue) / 100;
                    discountedPrice = Math.Max(variantResponse.Price - discountAmount, 0);
                }
                else if (applicablePromotion.DiscountType == "FIXED_AMOUNT")
                {
                    discountedPrice = Math.Max(variantResponse.Price - applicablePromotion.DiscountValue, 0);
                }

                variantResponse.DiscountedPrice = discountedPrice;
                variantResponse.PromotionTitle = applicablePromotion.Title;
            }
            else
            {
                variantResponse.DiscountedPrice = variantResponse.Price;
                variantResponse.PromotionTitle = null;
            }

            // ✅ Lưu vào cache với TTL 30 phút
            await _cacheService.SetCacheAsync(cacheKey, variantResponse, TimeSpan.FromMinutes(30));

            return variantResponse;
        }

    }

}
