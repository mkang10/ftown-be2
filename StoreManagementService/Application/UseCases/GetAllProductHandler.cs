using AutoMapper;
using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAllProductHandler
    {
        private readonly IImportRepos _repository;
        private readonly IMapper _mapper;

        public GetAllProductHandler(IImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDTO<ProductVariantResponseDto>> GetAllProductVariantsAsync(int page, int pageSize)
        {
            var pagedResult = await _repository.GetAllAsync(page, pageSize);
            var dtos = _mapper.Map<List<ProductVariantResponseDto>>(pagedResult.Data);
            return new PaginatedResponseDTO<ProductVariantResponseDto>(dtos, pagedResult.TotalRecords, pagedResult.Page, pagedResult.PageSize);
        }
    }
}
