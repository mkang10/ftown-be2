using Application.Interfaces; // Interface của UploadImageService
using Domain.DTO.Request;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateProductHandler
    {
        private readonly IProductRepos _productRepo;
        private readonly IProductVarRepos _variantRepo;
        private readonly IUploadImageService _uploadImageService;

        public CreateProductHandler(
            IProductRepos productRepo,
            IProductVarRepos variantRepo,
            IUploadImageService uploadImageService)
        {
            _productRepo = productRepo;
            _variantRepo = variantRepo;
            _uploadImageService = uploadImageService;
        }

        public async Task<int> CreateProductAsync(ProductCreateDto dto)
        {
            // Upload images sử dụng UploadImageService đã tách riêng
            var imageDtos = new List<ProductImageDto>();
            var imageUrls = await _uploadImageService.UploadImagesAsync(dto.Images);

            int count = 0;
            foreach (var url in imageUrls)
            {
                imageDtos.Add(new ProductImageDto
                {
                    ImagePath = url,
                    IsMain = count == 0   // Ảnh đầu tiên được đặt làm ảnh chính
                });
                count++;
            }

            // Map dữ liệu từ DTO sang entity Product
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Origin = dto.Origin,
                Model = dto.Model,
                Occasion = dto.Occasion,
                Style = dto.Style,
                Material = dto.Material,
                Status = dto.Status,
                ProductImages = imageDtos.Select(img => new ProductImage
                {
                    ImagePath = img.ImagePath,
                    IsMain = img.IsMain,
                    CreatedDate = DateTime.UtcNow
                }).ToList()
            };

            var created = await _productRepo.CreateAsync(product);
            return created.ProductId;
        }

        public async Task<int> CreateVariantAsync(ProductVariantCreateDto dto)
        {
            string? imagePath = null;
            if (dto.ImageFile != null)
            {
                // Upload ảnh variant qua UploadImageService
                imagePath = await _uploadImageService.UploadImageAsync(dto.ImageFile);
            }

            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                SizeId = dto.SizeId,
                ColorId = dto.ColorId,
                Price = dto.Price,
                ImagePath = imagePath,
                Sku = dto.Sku,
                Barcode = dto.Barcode,
                Weight = dto.Weight
            };

            var created = await _variantRepo.CreateAsync(variant);
            return created.VariantId;
        }
    }
}
