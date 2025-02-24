using Application.DTO.Request;
using Application.DTO.Response;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/stores")]
    public class StoreController : ControllerBase
    {
        private readonly GetAllStoresHandler _getAllStoresHandler;
        private readonly GetStoreByIdHandler _getStoreByIdHandler;
        private readonly CreateStoreHandler _createStoreHandler;
        private readonly UpdateStoreHandler _updateStoreHandler;
        private readonly DeleteStoreHandler _deleteStoreHandler;
        private readonly GetStoreStockByVariantHandler _stockHandler;

        public StoreController(
            GetAllStoresHandler getAllStoresHandler,
            GetStoreByIdHandler getStoreByIdHandler,
            CreateStoreHandler createStoreHandler,
            UpdateStoreHandler updateStoreHandler,
            DeleteStoreHandler deleteStoreHandler,
            GetStoreStockByVariantHandler stockHandler)
        {
            _getAllStoresHandler = getAllStoresHandler;
            _getStoreByIdHandler = getStoreByIdHandler;
            _createStoreHandler = createStoreHandler;
            _updateStoreHandler = updateStoreHandler;
            _deleteStoreHandler = deleteStoreHandler;
            _stockHandler = stockHandler;
        }

        [HttpGet]
        public async Task<ActionResult<List<StoreResponse>>> GetAllStores()
        {
            var result = await _getAllStoresHandler.Handle();
            return Ok(result);
        }

        [HttpGet("{storeId}")]
        public async Task<ActionResult<StoreResponse>> GetStoreById(int storeId)
        {
            var store = await _getStoreByIdHandler.Handle(storeId);
            if (store == null) return NotFound();
            return Ok(store);
        }

        [HttpPost]
        public async Task<ActionResult<StoreResponse>> CreateStore([FromBody] StoreRequest request)
        {
            var createdStore = await _createStoreHandler.Handle(request);
            return CreatedAtAction(nameof(GetStoreById), new { storeId = createdStore.StoreId }, createdStore);
        }

        [HttpPut("{storeId}")]
        public async Task<ActionResult<StoreResponse>> UpdateStore(int storeId, [FromBody] StoreRequest request)
        {
            var updatedStore = await _updateStoreHandler.Handle(storeId, request);
            if (updatedStore == null) return NotFound();
            return Ok(updatedStore);
        }

        [HttpDelete("{storeId}")]
        public async Task<IActionResult> DeleteStore(int storeId)
        {
            var success = await _deleteStoreHandler.Handle(storeId);
            if (!success) return NotFound();
            return NoContent();
        }
        /// <summary>
        /// Lấy tổng tồn kho của một variant trên toàn hệ thống.
        /// </summary>
        [HttpGet("variant/{variantId}/total-stock")]
        public async Task<IActionResult> GetTotalStock(int variantId)
        {
            int totalStock = await _stockHandler.HandleTotalStockAsync(variantId);
            return Ok(new { VariantId = variantId, TotalStock = totalStock });
        }

        /// <summary>
        /// Lấy phân rã tồn kho của một variant theo từng cửa hàng.
        /// </summary>
        [HttpGet("variant/{variantId}/stock-breakdown")]
        public async Task<IActionResult> GetStockBreakdown(int variantId)
        {
            var breakdown = await _stockHandler.HandleStockBreakdownAsync(variantId);
            return Ok(breakdown);
        }
    }
}
