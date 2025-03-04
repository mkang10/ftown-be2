using AutoMapper;
using Domain.DTO.Response;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class GetInventoryImportHandler
    {
        private readonly IInventoryImportRepos _repository;
        private readonly IMapper _mapper;

        public GetInventoryImportHandler(IInventoryImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<InventoryImportResponseDto>> GetAllInventoryImportsByCreatedByAsync(int createdBy)
        {
            var imports = await _repository.GetAllAsync(createdBy);
            var responseDtos = _mapper.Map<List<InventoryImportResponseDto>>(imports);
            return responseDtos;
        }
    }
}
