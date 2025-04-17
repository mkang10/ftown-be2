using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetAllProductHandler;

namespace Application.UseCases
{
    
        public class GetAllProductHandler 
        {
            private readonly IProductRepos _repo;
            public GetAllProductHandler(IProductRepos repo)
            {
                _repo = repo;
            }

        public async Task<PaginatedResponseDTO<ProductDto>>
        GetAllProductsAsync(
            string? nameFilter,
            string? descriptionFilter,
            int? categoryFilter,
            string? originFilter,
            string? modelFilter,
            string? occasionFilter,
            string? styleFilter,
            string? materialFilter,
            string? statusFilter,
            int page,
            int pageSize)
        {
            var all = await _repo.GetAllAsync();

            var queried = all.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                ImagePath = p.ProductImages.FirstOrDefault(pi => pi.IsMain)?.ImagePath,
                Origin = p.Origin,
                Model = p.Model,
                Occasion = p.Occasion,
                Style = p.Style,
                Material = p.Material,
                Status = p.Status
            });

            // Apply filters
            if (!string.IsNullOrEmpty(nameFilter))
                queried = queried.Where(d => d.Name.ToLower().Contains(nameFilter.ToLower()));
            if (!string.IsNullOrEmpty(descriptionFilter))
                queried = queried.Where(d => d.Description?.ToLower().Contains(descriptionFilter.ToLower()) == true);
            if (categoryFilter.HasValue)
                queried = queried.Where(d => d.CategoryId == categoryFilter.Value);
            if (!string.IsNullOrEmpty(originFilter))
                queried = queried.Where(d => d.Origin?.ToLower().Contains(originFilter.ToLower()) == true);
            if (!string.IsNullOrEmpty(modelFilter))
                queried = queried.Where(d => d.Model?.ToLower().Contains(modelFilter.ToLower()) == true);
            if (!string.IsNullOrEmpty(occasionFilter))
                queried = queried.Where(d => d.Occasion?.ToLower().Contains(occasionFilter.ToLower()) == true);
            if (!string.IsNullOrEmpty(styleFilter))
                queried = queried.Where(d => d.Style?.ToLower().Contains(styleFilter.ToLower()) == true);
            if (!string.IsNullOrEmpty(materialFilter))
                queried = queried.Where(d => d.Material?.ToLower().Contains(materialFilter.ToLower()) == true);
            if (!string.IsNullOrEmpty(statusFilter))
                queried = queried.Where(d => d.Status?.ToLower().Contains(statusFilter.ToLower()) == true);

            var total = queried.Count();
            var items = queried
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResponseDTO<ProductDto>(items, total, page, pageSize);
        }
    }
}
