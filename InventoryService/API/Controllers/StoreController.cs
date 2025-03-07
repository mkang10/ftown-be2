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
        private readonly UpdateStockAfterOrderHandler _updateStockHandler;
        private readonly GetStockQuantityHandler _getStockQuantityHandler;

        public StoreController(
            GetAllStoresHandler getAllStoresHandler,
            GetStoreByIdHandler getStoreByIdHandler,
            CreateStoreHandler createStoreHandler,
            UpdateStoreHandler updateStoreHandler,
            DeleteStoreHandler deleteStoreHandler,
            GetStoreStockByVariantHandler stockHandler,
            GetStockQuantityHandler getStockQuantityHandler,
            UpdateStockAfterOrderHandler updateStockHandler)
        {
            _getAllStoresHandler = getAllStoresHandler;
            _getStoreByIdHandler = getStoreByIdHandler;
            _createStoreHandler = createStoreHandler;
            _updateStoreHandler = updateStoreHandler;
            _deleteStoreHandler = deleteStoreHandler;
            _stockHandler = stockHandler;
            _getStockQuantityHandler = getStockQuantityHandler;
            _updateStockHandler = updateStockHandler;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO<List<StoreResponse>>>> GetAllStores()
        {
            var stores = await _getAllStoresHandler.Handle();
            var response = new ResponseDTO<List<StoreResponse>>(stores, true, "Lấy danh sách cửa hàng thành công");
            return Ok(response);
        }

        [HttpGet("{storeId}")]
        public async Task<ActionResult<ResponseDTO<StoreResponse>>> GetStoreById(int storeId)
        {
            var store = await _getStoreByIdHandler.Handle(storeId);
            if (store == null)
                return NotFound(new ResponseDTO<StoreResponse>(null, false, "Không tìm thấy cửa hàng"));

            var response = new ResponseDTO<StoreResponse>(store, true, "Lấy thông tin cửa hàng thành công");
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO<StoreResponse>>> CreateStore([FromBody] StoreRequest request)
        {
            var createdStore = await _createStoreHandler.Handle(request);
            var response = new ResponseDTO<StoreResponse>(createdStore, true, "Tạo cửa hàng thành công");
            return CreatedAtAction(nameof(GetStoreById), new { storeId = createdStore.StoreId }, response);
        }

        [HttpPut("{storeId}")]
        public async Task<ActionResult<ResponseDTO<StoreResponse>>> UpdateStore(int storeId, [FromBody] StoreRequest request)
        {
            var updatedStore = await _updateStoreHandler.Handle(storeId, request);
            if (updatedStore == null)
                return NotFound(new ResponseDTO<StoreResponse>(null, false, "Cửa hàng không tồn tại"));

            var response = new ResponseDTO<StoreResponse>(updatedStore, true, "Cập nhật cửa hàng thành công");
            return Ok(response);
        }

        [HttpDelete("{storeId}")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteStore(int storeId)
        {
            var success = await _deleteStoreHandler.Handle(storeId);
            if (!success)
                return NotFound(new ResponseDTO<bool>(false, false, "Xóa cửa hàng thất bại hoặc không tồn tại"));

            var response = new ResponseDTO<bool>(true, true, "Xóa cửa hàng thành công");
            return Ok(response);
        }

        /// <summary>
        /// Lấy tổng tồn kho của một variant trên toàn hệ thống.
        /// </summary>
        [HttpGet("variant/{variantId}/total-stock")]
        public async Task<ActionResult<ResponseDTO<int>>> GetTotalStock(int variantId)
        {
            int totalStock = await _stockHandler.HandleTotalStockAsync(variantId);
            var response = new ResponseDTO<int>(totalStock, true, "Lấy tổng tồn kho thành công");
            return Ok(response);
        }

        /// <summary>
        /// Lấy phân rã tồn kho của một variant theo từng cửa hàng.
        /// </summary>
        [HttpGet("variant/{variantId}/stock-breakdown")]
        public async Task<ActionResult<ResponseDTO<object>>> GetStockBreakdown(int variantId)
        {
            // Giả sử breakdown là một object, có thể thay thế bằng kiểu cụ thể nếu có
            var breakdown = await _stockHandler.HandleStockBreakdownAsync(variantId);
            var response = new ResponseDTO<object>(breakdown, true, "Lấy phân rã tồn kho thành công");
            return Ok(response);
        }
        [HttpGet("{storeId}/stock/{productVariantId}")]
        public async Task<IActionResult> GetStockQuantity(int storeId, int productVariantId)
        {
            var result = await _getStockQuantityHandler.HandleAsync(storeId, productVariantId);

            var response = new ResponseDTO<StockQuantityResponse>(
                result,
                result != null,
                result != null ? "Lấy số lượng tồn kho thành công." : "Không tìm thấy dữ liệu tồn kho."
            );

            if (result == null)
                return NotFound(response); // 404 Not Found nếu không có dữ liệu.

            return Ok(response); // 200 OK nếu có dữ liệu.
        }
        [HttpPost("update-after-order")]
        public async Task<ActionResult<bool>> UpdateStockAfterOrder([FromBody] StockUpdateRequest request)
        {
            bool success = await _updateStockHandler.HandleAsync(request);

            if (!success)
            {
                return BadRequest(false);
            }

            return Ok(true);
        }
    }
}
