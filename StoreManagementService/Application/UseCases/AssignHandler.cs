using AutoMapper;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.OrderAssigmentRes;
using static Domain.DTO.Response.OrderDTO;

namespace Application.UseCases
{
    public class AssignStaffHandler
    {
        private readonly IImportRepos _invenRepos;
        private readonly IStaffDetailRepository _staffDetailRepos;
        private readonly IDispatchRepos _dispatchRepos;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public AssignStaffHandler(IMapper mapper, IOrderRepository orderRepository ,IImportRepos invenRepos,
                                      IStaffDetailRepository staffDetailRepos, IDispatchRepos dispatchRepos)
        {
            _invenRepos = invenRepos;
            _staffDetailRepos = staffDetailRepos;
            _dispatchRepos = dispatchRepos;
            _orderRepository = orderRepository;
            _mapper = mapper;
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

        public async Task<ResponseDTO<bool>> AssignStaffDispatchAccountAsync(int dispatchId, int staffId)
        {
            // Lấy dispach theo importId (bao gồm quan hệ từ Detail -> StoreDetail -> StaffDetail -> Account)
            var dispatch = await _dispatchRepos.GetByIdDispatchAssignAsync(dispatchId);
            if (dispatch == null)
            {
                return new ResponseDTO<bool>(false, false, "Dispatch not found");
            }

            // Lấy StaffDetail dựa trên accountId
            var staffDetail = await _staffDetailRepos.GetByIdAsync(staffId);
            if (staffDetail == null)
            {
                return new ResponseDTO<bool>(false, false, "Staff detail not found for the given staff id");
            }

            // Gán StaffDetail cho các StoreDetail trong từng InventoryImportDetail
            foreach (var detail in dispatch.DispatchDetails)
            {
                foreach (var storeDetail in detail.StoreExportStoreDetails)
                {
                    storeDetail.StaffDetailId = staffDetail.StaffDetailId;
                    storeDetail.Status = "Processing"; // Cập nhật status của store detail
                }
            }

            dispatch.Status = "Processing";

            await _dispatchRepos.UpdateAsync(dispatch);
            return new ResponseDTO<bool>(true, true, "Staff assigned and statuses updated successfully");
        }

        public async Task<ResponseDTO<OrderAssignmentResponseDTO>> AssignStaffAsync(AssignStaffDTO dto)
        {
            var assignment = await _orderRepository.GetByOrderIdAsync(dto.OrderId);
            if (assignment == null)
            {
                return new ResponseDTO<OrderAssignmentResponseDTO>(
                    null!,
                    false,
                    $"Không tìm thấy phân công nào cho OrderId = {dto.OrderId}"
                );
            }

            _mapper.Map(dto, assignment);
            assignment.Order.Status = "Confirmed";

            await _orderRepository.SaveChangesAsync();

            var responseDto = _mapper.Map<OrderAssignmentResponseDTO>(assignment);
            return new ResponseDTO<OrderAssignmentResponseDTO>(
                responseDto,
                true,
                "Gán nhân viên thành công."
            );
        }

        

    }
}
