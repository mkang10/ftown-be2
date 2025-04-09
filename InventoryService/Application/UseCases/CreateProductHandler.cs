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
			var products = _mapper.Map<List<Product>>(requests);

			for (int i = 0; i < products.Count; i++)
			{
				products[i].Status = "Draft";

				if (requests[i].ImageFiles != null && requests[i].ImageFiles.Any())
				{
					var (mainImagePath, productImages) = await UploadProductImagesAsync(requests[i].ImageFiles);
					products[i].ImagePath = mainImagePath;
					products[i].ProductImages = productImages;
				}
			}

			await _productRepository.AddProductsAsync(products);
			return _mapper.Map<List<ProductDetailResponse>>(products);
		}

		private async Task<(string mainImagePath, List<ProductImage> images)> UploadProductImagesAsync(List<IFormFile> imageFiles)
		{
			var images = new List<ProductImage>();
			string mainImagePath = string.Empty;

			for (int i = 0; i < imageFiles.Count; i++)
			{
				var imageUrl = await _cloudinaryService.UploadMediaAsync(imageFiles[i]);

				var productImage = new ProductImage
				{
					ImagePath = imageUrl,
					IsMain = (i == 0),
					CreatedDate = DateTime.UtcNow
				};

				if (i == 0)
					mainImagePath = imageUrl;

				images.Add(productImage);
			}

			return (mainImagePath, images);
		}

	}
}
