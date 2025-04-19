using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.ProductDetailDTO;

namespace Application.UseCases
{
    public class GetProductDetailHandler
    {
        private readonly IProductRepos _productRepo;
        private readonly IProductVarRepos _variantRepo;
        private readonly IMapper _mapper;

        public GetProductDetailHandler(IProductRepos productRepo, IProductVarRepos variantRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<ProductWithVariantsDto>> GetProductWithVariantsAsync(int productId)
        {
            var product = await _productRepo.GetByIdWithVariantsAsync(productId);
            if (product == null)
                return new ResponseDTO<ProductWithVariantsDto>(null, false, "Không tìm thấy sản phẩm");

            var variantIds = await _variantRepo.GetAllVariantIdsByProductIdAsync(productId);
            var productDto = _mapper.Map<ProductDto>(product);

            // Lấy image chính (IsMain) giống GetAllProductsAsync
            productDto.ImagePath = product.ProductImages
                                        .FirstOrDefault(pi => pi.IsMain)
                                        ?.ImagePath;

            var variantDtos = _mapper.Map<List<ProductVariantDto>>(product.ProductVariants);

            var dto = new ProductWithVariantsDto
            {
                Product = productDto,
                VariantIds = variantIds,
                Variants = variantDtos
            };

            return new ResponseDTO<ProductWithVariantsDto>(dto, true, "Thành công");
        }

    }
}
