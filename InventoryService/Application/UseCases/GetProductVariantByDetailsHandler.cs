using Application.DTO.Request;
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
    public class GetProductVariantByDetailsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;

        public GetProductVariantByDetailsHandler(IProductRepository productRepository,
                                                 IRedisCacheService cacheService,
                                                 IMapper mapper)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
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

            int stockQuantity = await _productRepository.GetProductVariantStockAsync(productVariant.VariantId);
            var variantResponse = _mapper.Map<ProductVariantResponse>(productVariant);
            variantResponse.StockQuantity = stockQuantity;

            return variantResponse;
        }

    }

}
