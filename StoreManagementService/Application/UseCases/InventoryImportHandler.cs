using Application.DTO.Request;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class InventoryImportHandler
    {
        private readonly IInventoryImportRepos _repository;
        private readonly IMapper _mapper;

        public InventoryImportHandler(IInventoryImportRepos repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<int> CreateInventoryImportAsync(CreateInventoryImportDto dto)
        {
            // Kiểm tra tổng số lượng phân bổ cho từng chi tiết nhập hàng
            foreach (var detail in dto.Details)
            {
                int totalAllocated = detail.StoreAllocations.Sum(sa => sa.AllocatedQuantity);
                if (totalAllocated != detail.Quantity)
                {
                    throw new Exception($"Tổng số lượng phân bổ ({totalAllocated}) không khớp với số lượng nhập ({detail.Quantity}) của sản phẩm variant {detail.ProductVariantId}.");
                }
            }

            // Ánh xạ DTO sang entity, bao gồm InventoryImportHistories, Details và StoreAllocations
            var import = _mapper.Map<InventoryImport>(dto);

            // Gọi repository để lưu dữ liệu. Trong repository, sẽ attach đối tượng Account cho mỗi History.
            var createdImport = await _repository.AddAsync(import);

            return createdImport.ImportId;
        }

    }
}
