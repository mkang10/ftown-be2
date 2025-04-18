﻿using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductVarRepos
    {
        Task<ProductVariant> CreateAsync(ProductVariant variant);

        Task<int?> GetVariantIdAsync(int productId, int sizeId, int colorId);
        Task<ProductVariant?> GetByProductSizeColorAsync(int productId, int sizeId, int colorId);
        Task<Product?> GetByIdWithVariantsAsync(int productId);
        Task<int[]> GetAllVariantIdsByProductIdAsync(int productId);
        Task<ProductVariant[]> GetAllVariantsByProductIdAsync(int productId);

    }

}
