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
    public class GetAllPendingHandler
    {
        private readonly IInventoryImportRepository _repository;
        private readonly IMapper _mapper;

        public GetAllPendingHandler(IInventoryImportRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<InventoryPendingResponseDto>> GetAllPendingInventoryImportsAsync()
        {
            var pendingImports = await _repository.GetAllPendingAsync();
            var responseDtos = _mapper.Map<List<InventoryPendingResponseDto>>(pendingImports);
            return responseDtos;
        }
    }
   
}
