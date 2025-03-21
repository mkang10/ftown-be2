using Application.DTO.Request;
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
    public class GetProductVariantByDetailsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IWareHousesStockRepository _wareHousesStockRepository;
        private readonly IPromotionRepository _promotionRepository;

        public GetProductVariantByDetailsHandler(IProductRepository productRepository,
                                                 IRedisCacheService cacheService,
                                                 IMapper mapper,
                                                 IWareHousesStockRepository wareHousesStockRepository,
                                                 IPromotionRepository promotionRepository)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _wareHousesStockRepository = wareHousesStockRepository;
            _promotionRepository = promotionRepository;
        }

        public async Task<ProductVariantResponse?> HandleAsync(GetProductVariantByDetailsRequest request)
        {
            string instanceName = "ProductInstance"; // 🔹 Lấy từ config nếu cần
            string cacheKey = $"{instanceName}:product:{request.ProductId}"; // 🔹 Trùng với cacheKey của ProductDetail

            // 🔍 Kiểm tra cache trong Redis trước
            var cachedProduct = await _cacheService.GetCacheAsync<ProductDetailResponse>(cacheKey);
            if (cachedProduct != null && cachedProduct.Variants != null)
            {
                // 🏎️ Tìm nhanh biến thể cần lấy trong danh sách đã cache
                var variant = cachedProduct.Variants.FirstOrDefault(v =>
                    v.Size.Trim().Equals(request.Size.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    v.Color.Trim().Equals(request.Color.Trim(), StringComparison.OrdinalIgnoreCase));

                if (variant != null)
                    return variant;
            }

            // ❌ Nếu cache không có, truy vấn DB như bình thường
            var productVariant = await _productRepository.GetProductVariantByDetailsAsync(request.ProductId, request.Size, request.Color);
            if (productVariant == null)
                return null;

            int stockQuantity = await _wareHousesStockRepository.GetStockQuantityAsync(2, productVariant.VariantId);

            // 🔹 Lấy danh sách khuyến mãi áp dụng cho sản phẩm
            var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;
            // 🔥 Kiểm tra biến thể có khuyến mãi không
            var applicablePromotion = promotions.FirstOrDefault(p =>
                !string.IsNullOrEmpty(p.ApplyValue) &&
                JsonConvert.DeserializeObject<List<int>>(p.ApplyValue).Contains(request.ProductId));

            decimal discountedPrice = variantResponse.Price; // Giá mặc định

            if (applicablePromotion != null)
            {
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

            return variantResponse;
        }

    }

}
