using Application.DTO.Response;
using Application.UseCases;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

[ApiController]
[Route("api/[controller]")]
public class TransferController : ControllerBase
{
    private readonly TransferHandler _transferHandler;
    private readonly GetAllTransferHandler _gethandler;

    public TransferController(TransferHandler transferHandler, GetAllTransferHandler gethandler)
    {
        _transferHandler = transferHandler;
        _gethandler = gethandler;
    }

    [HttpPost("create-transfer-fullflow")]
    public async Task<IActionResult> CreateTransferFullFlow([FromBody] CreateTransferFullFlowDto request)
    {
        try
        {
            if (request == null || request.TransferDetails == null || !request.TransferDetails.Any())
                return BadRequest(new ResponseDTO<object>(null, false, "Dữ liệu chuyển hàng không hợp lệ!"));

            var response = await _transferHandler.CreateTransferFullFlowAsync(request);
            return Ok(response);
        }
        catch (ArgumentException argEx)
        {
            return BadRequest(new ResponseDTO<object>(null, false, argEx.Message));
        }
        catch (UnauthorizedAccessException uaEx)
        {
            return Unauthorized(new ResponseDTO<object>(null, false, uaEx.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDTO<object>(null, false, $"Lỗi server: {ex.Message}"));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] string? filter = null,
           CancellationToken cancellationToken = default)
    {
        var result = await _gethandler.HandleAsync(page, pageSize, filter, cancellationToken);
        if (!result.Status)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("transfer/{id}")]
    public async Task<IActionResult> GetJSONTransferByIdController(int id)
    {
        try
        {
            var result = await _transferHandler.GetJSONTransferById(id);
            if (result == null)
            {
                var notFoundResponse = new MessageRespondDTO<JSONTransferDispatchImportGet>(null, false, "Transfer not found!");
                return NotFound(notFoundResponse);
            }
            var successResponse = new MessageRespondDTO<JSONTransferDispatchImportGet>(result, true, "Success!");
            return Ok(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = new MessageRespondDTO<object>(null, false, "An error occurred: " + ex.Message);
            return BadRequest(errorResponse);
        }
    }
}
