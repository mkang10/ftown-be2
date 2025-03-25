using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class AssignStaffHandler
    {
        private readonly IImportRepos _invenRepos;
        private readonly IStaffDetailRepository _staffDetailRepos;

        public AssignStaffHandler(IImportRepos invenRepos,
                                      IStaffDetailRepository staffDetailRepos)
        {
            _invenRepos = invenRepos;
            _staffDetailRepos = staffDetailRepos;
        }


        public async Task<ResponseDTO<bool>> AssignStaffAccountAsync(int importId, int staffId)
        {
            // Lấy InventoryImport theo importId (bao gồm quan hệ từ Detail -> StoreDetail -> StaffDetail -> Account)
            var inventoryImport = await _invenRepos.GetByIdAssignAsync(importId);
            if (inventoryImport == null)
            {
                return new ResponseDTO<bool>(false, false, "Inventory import not found");
            }

            // Lấy StaffDetail dựa trên accountId
            var staffDetail = await _staffDetailRepos.GetByIdAsync(staffId);
            if (staffDetail == null)
            {
                return new ResponseDTO<bool>(false, false, "Staff detail not found for the given staff id");
            }

            // Gán StaffDetail cho các StoreDetail trong từng InventoryImportDetail
            foreach (var detail in inventoryImport.ImportDetails)
            {
                foreach (var storeDetail in detail.ImportStoreDetails)
                {
                    storeDetail.StaffDetailId = staffDetail.StaffDetailId;
                    storeDetail.Status = "Processing"; // Cập nhật status của store detail
                }
            }

            // Cập nhật status của InventoryImport
            inventoryImport.Status = "Processing";

            await _invenRepos.UpdateAsync(inventoryImport);
            return new ResponseDTO<bool>(true, true, "Staff assigned and statuses updated successfully");
        }

    }
}
