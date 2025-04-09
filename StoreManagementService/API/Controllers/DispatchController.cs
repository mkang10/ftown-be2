using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response.Domain.DTO.Response;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using static Domain.DTO.Request.StoreExportStoreDetailReq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DispatchController : ControllerBase
    {
        
        private readonly DispatchDoneHandler _dispatchDoneHandler;
        private readonly GetAllDispatchHandler _getAllDispatchHandler;
        private readonly AssignStaffHandler _assignStaff;
        private readonly GetAllStaffDispatchHandler _getDispatchByStaff;


        public DispatchController(GetAllStaffDispatchHandler getDispatchByStaff ,AssignStaffHandler assignStaff, DispatchDoneHandler dispatchDoneHandler, GetAllDispatchHandler getAllDispatchHandler)
        {

            _dispatchDoneHandler = dispatchDoneHandler;
            _getAllDispatchHandler = getAllDispatchHandler;
            _assignStaff = assignStaff;
            _getDispatchByStaff = getDispatchByStaff;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDispatch([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] DispatchFilterDto filter = null)
        {
            var result = await _getAllDispatchHandler.HandleAsync(page, pageSize, filter);
            return Ok(result);
        }

        [HttpPut("{dispatchId}/assign-staff")]
        public async Task<IActionResult> AssignStaffDetail(int dispatchId, [FromQuery] int staffDetailId)
        {
            var response = await _assignStaff.AssignStaffDispatchAccountAsync(dispatchId, staffDetailId);
            if (!response.Status)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }



        [HttpPost("{dispatchid}/done")]
        public async Task<IActionResult> ProcessImportDone(int dispatchid, int staffId, [FromBody] List<UpdateStoreDetailDto> confirmations)
        {
            if (confirmations == null || confirmations.Count == 0)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Danh sách xác nhận không được để trống");
                return BadRequest(errorResponse);
            }

            try
            {
                await _dispatchDoneHandler.ProcessDispatchDoneAsync(dispatchid, staffId, confirmations);
                var response = new ResponseDTO<string>("Cập nhật Done thành công", true, "Success");
                return Ok(response);
            }
            catch (ArgumentException argEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, argEx.Message);
                return BadRequest(errorResponse);
            }
            catch (InvalidOperationException invOpEx)
            {
                var errorResponse = new ResponseDTO<string>(null, false, invOpEx.Message);
                return Conflict(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ResponseDTO<string>(null, false, "Đã có lỗi xảy ra, vui lòng thử lại sau");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("by-staff")]
        public async Task<IActionResult> GetExportDetailsByStaffDetail([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] StoreExportStoreDetailFilterDto filter = null)
        {
            var paged = await _getDispatchByStaff.HandleAsync(page, pageSize, filter);
            var response = new ResponseDTO<PaginatedResponseDTO<ExportDetailDto>>(
                paged, true, "Lấy danh sách export store details thành công");
            return Ok(response);
        }
    }
}
