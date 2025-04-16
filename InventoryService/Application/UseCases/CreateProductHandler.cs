using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
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
        private readonly ICloudinaryService _cloudinaryService;
        public CreateProductHandler(IProductRepository productRepository, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

		public async Task<ProductDetailResponse> CreateProductAsync(CreateProductRequest request)
		{
			var product = _mapper.Map<Product>(request);
			product.Status = "Draft";

			// Upload ảnh (nếu có)
			if (request.ImageFiles != null && request.ImageFiles.Any())
			{
				var (mainImagePath, productImages) = await UploadProductImagesAsync(request.ImageFiles);
				product.ImagePath = mainImagePath; // Lưu ảnh chính vào Product
				product.ProductImages = productImages;
			}

			await _productRepository.AddProductAsync(product);
			return _mapper.Map<ProductDetailResponse>(product);
		}


        // Tạo nhiều sản phẩm cùng lúc
        public async Task<List<ProductDetailResponse>> CreateMultipleProductsAsync(List<CreateProductRequest> requests)
        {
            var result = new List<Product>();

            for (int i = 0; i < requests.Count; i++)
            {
                var product = _mapper.Map<Product>(requests[i]);
                product.Status = "Draft";

                if (requests[i].ImageFiles != null && requests[i].ImageFiles.Any())
                {
                    var (mainImagePath, productImages) = await UploadProductImagesAsync(requests[i].ImageFiles);
                    product.ImagePath = mainImagePath;
                    product.ProductImages = productImages;
                }

                result.Add(product);
            }

            await _productRepository.AddProductsAsync(result);
            return _mapper.Map<List<ProductDetailResponse>>(result);
        }

        private async Task<(string mainImagePath, List<ProductImage>)> UploadProductImagesAsync(List<IFormFile> files)
        {
            var productImages = new List<ProductImage>();
            string mainImagePath = null;

            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine("wwwroot/uploads/products", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (mainImagePath == null)
                    mainImagePath = $"/uploads/products/{fileName}";

                productImages.Add(new ProductImage
                {
                    ImagePath = $"/uploads/products/{fileName}"
                });
            }

            return (mainImagePath, productImages);
        }

    }
}
