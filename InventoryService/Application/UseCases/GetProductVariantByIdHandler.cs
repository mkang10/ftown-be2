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
    public class GetProductVariantByIdHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductVariantByIdHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductVariantResponse?> Handle(int variantId)
        {
            var productVariant = await _productRepository.GetProductVariantByIdAsync(variantId);

            if (productVariant == null)
                return null;

            return _mapper.Map<ProductVariantResponse>(productVariant);
        }
    }
}
