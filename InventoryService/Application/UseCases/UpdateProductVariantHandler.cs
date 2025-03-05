using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    //public class UpdateProductVariantHandler
    //{
    //    private readonly IProductRepository _productRepository;
    //    private readonly IRedisCacheService _cacheService;

    //    public UpdateProductVariantHandler(IProductRepository productRepository, IRedisCacheService cacheService)
    //    {
    //        _productRepository = productRepository;
    //        _cacheService = cacheService;
    //    }

    //    public async Task<bool> Handle(ProductVariant productVariant)
    //    {
    //        await _productRepository.UpdateProductVariantAsync(productVariant);

    //        // 🔴 Xóa cache chi tiết biến thể sản phẩm
    //        string cacheKey = $"variant:{productVariant.VariantId}";
    //        await _cacheService.RemoveCacheAsync(cacheKey);

    //        return true;
    //    }
    //}


}
