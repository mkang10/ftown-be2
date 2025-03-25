using Application.DTO.Response;
using AutoMapper;
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
    public class GetAllProductsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;

        public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper, IRedisCacheService cacheService, IPromotionRepository promotionRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _promotionRepository = promotionRepository;
        }

        public async Task<List<ProductListResponse>> Handle(int page, int pageSize)
        {
            string instanceName = "ProductInstance"; // 🔹 Định nghĩa instance name (có thể lấy từ config)
            string cacheKey = $"{instanceName}:products:view-all:page:{page}:size:{pageSize}";

            // 🔍 Kiểm tra cache trước khi gọi database
            var cachedProducts = await _cacheService.GetCacheAsync<List<ProductListResponse>>(cacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            // ❌ Nếu cache không có, gọi Repository để lấy dữ liệu từ database
            var products = await _productRepository.GetPagedProductsWithVariantsAsync(page, pageSize);

            if (products == null || !products.Any())
                return new List<ProductListResponse>();

            // 🔹 Lấy danh sách khuyến mãi đang hoạt động
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            // ⚡ Dùng AutoMapper để chuyển đổi Entity -> DTO
            var productList = _mapper.Map<List<ProductListResponse>>(products);

            // 🔥 Tính giá sau khuyến mãi
            foreach (var product in productList)
            {
                var applicablePromotion = promotions.FirstOrDefault(p =>
                    !string.IsNullOrEmpty(p.ApplyValue) &&
                    JsonConvert.DeserializeObject<List<int>>(p.ApplyValue).Contains(product.ProductId));

                if (applicablePromotion != null)
                {
                    decimal discountedPrice = product.Price;

                    if (applicablePromotion.DiscountType == "PERCENTAGE")
                    {
                        decimal discountAmount = (product.Price * applicablePromotion.DiscountValue) / 100;
                        discountedPrice = Math.Max(product.Price - discountAmount, 0);
                    }
                    else if (applicablePromotion.DiscountType == "FIXED_AMOUNT")
                    {
                        discountedPrice = Math.Max(product.Price - applicablePromotion.DiscountValue, 0);
                    }

                    product.DiscountedPrice = discountedPrice;
                    product.PromotionTitle = applicablePromotion.Title;
                }
                else
                {
                    product.DiscountedPrice = product.Price;
                    product.PromotionTitle = null;
                }
            }

            // ✅ Lưu vào cache với TTL 10 phút
            await _cacheService.SetCacheAsync(cacheKey, productList, TimeSpan.FromMinutes(10));

            return productList;
        }

    }

}
