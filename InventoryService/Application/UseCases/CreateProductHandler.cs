using Application.DTO.Request;
using Application.DTO.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CreateProductHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductDetailResponse> Handle(CreateProductRequest request)
        {
            // ✅ Kiểm tra xem danh sách biến thể có ít nhất 1 cái không
            if (request.Variants == null || request.Variants.Count == 0)
                throw new ArgumentException("Sản phẩm phải có ít nhất một biến thể!");

            // ✅ Tạo đối tượng Product từ DTO
            var product = _mapper.Map<Product>(request);

            // ✅ Thêm vào database
            await _productRepository.AddProductAsync(product);

            // ✅ Lưu biến thể sản phẩm (cần gán ProductId trước)
            var productVariants = request.Variants.Select(variantDto => new ProductVariant
            {
                ProductId = product.ProductId, // Lấy ID sau khi tạo Product
                Price = variantDto.Price,
                ImagePath = variantDto.ImagePath,
                Sku = variantDto.Sku,
                Barcode = variantDto.Barcode,
                Weight = variantDto.Weight,
                SizeId = variantDto.SizeId,
                ColorId = variantDto.ColorId
            }).ToList();

            await _productRepository.AddProductVariantsAsync(productVariants);

            // ✅ Lưu ảnh sản phẩm (nếu có)
            if (request.ProductImages != null && request.ProductImages.Any())
            {
                var productImages = request.ProductImages.Select(imgPath => new ProductImage
                {
                    ProductId = product.ProductId,
                    ImagePath = imgPath,
                    IsMain = false, // Ảnh chính có thể xác định sau
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                await _productRepository.AddProductImagesAsync(productImages);
            }

            // ✅ Trả về thông tin sản phẩm vừa tạo
            return _mapper.Map<ProductDetailResponse>(product);
        }
    }

}
