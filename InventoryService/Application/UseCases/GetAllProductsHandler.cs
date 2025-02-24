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
    public class GetAllProductsHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductsHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<ProductListResponse>> Handle()
        {
            var products = await _productRepository.GetAllProductsWithVariantsAsync();

            return products.Select(p => new ProductListResponse
            {
                ProductId = p.ProductId,
                Name = p.Name,
                ImagePath = p.ImagePath,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                MinPrice = p.ProductVariants.Min(v => v.Price),
                MaxPrice = p.ProductVariants.Max(v => v.Price),
                Colors = p.ProductVariants.Select(v => v.Color).Distinct().ToList()
            }).ToList();
        }
    }
}
