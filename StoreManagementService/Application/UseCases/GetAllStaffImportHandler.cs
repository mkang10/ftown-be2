using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response.Application.Imports.Dto;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.GetAllStaffImportHandler;

namespace Application.UseCases
{
  
        public class GetAllStaffImportHandler 
        {
            private readonly IImportRepos _repository;
            private readonly IMapper _mapper;

            public GetAllStaffImportHandler(IImportRepos repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

        public async Task<ResponseDTO<PaginatedResponseDTO<InventoryImportStoreDetailDto>>> GetStoreDetailsByStaffDetailAsync(ImportStoreDetailFilterDto filter)
        {
            var pagedResult = await _repository.GetStoreDetailsByStaffDetailAsync(filter);
            var dtos = _mapper.Map<List<InventoryImportStoreDetailDto>>(pagedResult.Data);
            var response = new PaginatedResponseDTO<InventoryImportStoreDetailDto>(dtos, pagedResult.TotalRecords, pagedResult.Page, pagedResult.PageSize);
            return new ResponseDTO<PaginatedResponseDTO<InventoryImportStoreDetailDto>>(response, true, "Data fetched successfully");
        }

    }

}
