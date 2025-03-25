using AutoMapper;
using Domain.DTO.Response;
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

        public async Task<List<ProductVariantResponseDto>> GetAllProductVariantsAsync()
        {
            var variants = await _repository.GetAllAsync();
            return _mapper.Map<List<ProductVariantResponseDto>>(variants);
        }
    }
}
