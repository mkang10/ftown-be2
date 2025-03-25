using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{


    public class GetImportDetailHandler
    {
        private readonly IImportRepos _inventoryRepository;
        private readonly IMapper _mapper;

        public GetImportDetailHandler(IImportRepos inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }

        public async Task<InventoryImportDetailDto> GetInventoryDetailAsync(int importId)
        {
            var inventory = await _inventoryRepository.GetImportByIdAsync(importId);
            if (inventory == null)
            {
                throw new Exception("Không tìm thấy phiếu nhập kho có Id: " + importId);
            }

            // Sử dụng AutoMapper để mapping dữ liệu sang DTO
            return _mapper.Map<InventoryImportDetailDto>(inventory);
        }
    }
}
