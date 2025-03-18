using Application.DTO.Response;
using AutoMapper;
using Domain.Interfaces;
using Newtonsoft.Json;
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
        private readonly IPromotionRepository _promotionRepository;

        public GetProductDetailHandler(IProductRepository productRepository, IRedisCacheService cacheService, IMapper mapper, IPromotionRepository promotionRepository)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
        }

        public async Task<ProductDetailResponse?> Handle(int productId)
        {
            string instanceName = "ProductInstance";
            string cacheKey = $"{instanceName}:product:{productId}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedProduct = await _cacheService.GetCacheAsync<ProductDetailResponse>(cacheKey);
            if (cachedProduct != null)
                return cachedProduct;

            // ❌ Không có cache, truy vấn database
            var product = await _productRepository.GetProductByIdAsync(productId);
            if (product == null) return null;

            // 🔹 Lấy danh sách khuyến mãi áp dụng cho sản phẩm
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            // ⚡ Dùng AutoMapper để chuyển đổi Entity -> DTO
            var productDetail = _mapper.Map<ProductDetailResponse>(product);

            // 🔥 Kiểm tra sản phẩm có khuyến mãi không
            var applicablePromotion = promotions.FirstOrDefault(p =>
                !string.IsNullOrEmpty(p.ApplyValue) &&
                JsonConvert.DeserializeObject<List<int>>(p.ApplyValue).Contains(productId));

            foreach (var variant in productDetail.Variants)
            {
                decimal discountedPrice = variant.Price; // Giá mặc định

                if (applicablePromotion != null)
                {
                    if (applicablePromotion.DiscountType == "PERCENTAGE")
                    {
                        decimal discountAmount = (variant.Price * applicablePromotion.DiscountValue) / 100;
                        discountedPrice = Math.Max(variant.Price - discountAmount, 0);
                    }
                    else if (applicablePromotion.DiscountType == "FIXED_AMOUNT")
                    {
                        discountedPrice = Math.Max(variant.Price - applicablePromotion.DiscountValue, 0);
                    }

                    variant.DiscountedPrice = discountedPrice;
                    variant.PromotionTitle = applicablePromotion.Title;
                }
                else
                {
                    variant.DiscountedPrice = variant.Price;
                    variant.PromotionTitle = null;
                }
            }

            // ✅ Lưu vào cache với TTL 30 phút
            await _cacheService.SetCacheAsync(cacheKey, productDetail, TimeSpan.FromMinutes(30));

            return productDetail;
        }

    }

}
