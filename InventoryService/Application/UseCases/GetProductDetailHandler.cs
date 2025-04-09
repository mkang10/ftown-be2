using Application.DTO.Response;
using Application.Interfaces;
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
    public class GetProductDetailHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisCacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IWareHousesStockRepository _wareHousesStockRepository;
		private readonly IPromotionService _promotionService;
		public GetProductDetailHandler(IProductRepository productRepository,
                                       IRedisCacheService cacheService, 
                                       IMapper mapper,
                                       IPromotionRepository promotionRepository,
                                       IWareHousesStockRepository wareHousesStockRepository,
                                       IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _promotionRepository = promotionRepository;
            _wareHousesStockRepository = wareHousesStockRepository;
            _promotionService = promotionService;
        }

		public async Task<ProductDetailResponse?> Handle(int productId, int? accountId = null)
		{
			string instanceName = "ProductInstance";
			string cacheKey = $"{instanceName}:product:{productId}";

			var cachedProduct = await _cacheService.GetCacheAsync<ProductDetailResponse>(cacheKey);
			if (cachedProduct != null)
			{
				cachedProduct.IsFavorite = false;

				if (accountId.HasValue)
				{
					var isFavorite = await _productRepository.IsProductFavoriteAsync(accountId.Value, productId);
					cachedProduct.IsFavorite = isFavorite;
				}

				return cachedProduct;
			}

			var product = await _productRepository.GetProductByIdAsync(productId);
			if (product == null) return null;

			var promotions = await _promotionRepository.GetActiveProductPromotionsAsync();

			var productDetail = _mapper.Map<ProductDetailResponse>(product);

			foreach (var variant in productDetail.Variants)
			{
				_promotionService.ApplyPromotion(
					productId,
					variant.Price,
					promotions,
					out var discountedPrice,
					out var promotionTitle);

				variant.DiscountedPrice = discountedPrice;
				variant.PromotionTitle = promotionTitle;
			}

			productDetail.IsFavorite = false;

			if (accountId.HasValue)
			{
				productDetail.IsFavorite = await _productRepository.IsProductFavoriteAsync(accountId.Value, productId);
			}

			productDetail.IsFavorite = false;
			await _cacheService.SetCacheAsync(cacheKey, productDetail, TimeSpan.FromMinutes(30));

			return productDetail;
		}


	}

}
