using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateOrderHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly GetShippingAddressHandler _getShippingAddressHandler;
        private readonly IMapper _mapper;
        private readonly GetSelectedCartItemsHandler _getSelectedCartItemsHandler;
        private readonly ILogger<CreateOrderHandler> _logger;
        private readonly ShippingCostHandler _shippingCostHandler;
        private readonly AuditLogHandler _auditLogHandler;
        private readonly INotificationClient _notificationClient;
        public CreateOrderHandler(
            IDistributedCache cache,
            IOrderRepository orderRepository,
            ICustomerServiceClient customerServiceClient,
            IInventoryServiceClient inventoryServiceClient,
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IShippingAddressRepository shippingAddressRepository,
            GetShippingAddressHandler getShippingAddressHandler,
            IMapper mapper,
            GetSelectedCartItemsHandler getSelectedCartItemsHandler,
            ShippingCostHandler shippingCostHandler,
            ILogger<CreateOrderHandler> logger,
            AuditLogHandler auditLogHandler,
            INotificationClient notificationClient)
        {
            _cache = cache;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _customerServiceClient = customerServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _shippingAddressRepository = shippingAddressRepository;
            _getShippingAddressHandler = getShippingAddressHandler;
            _getSelectedCartItemsHandler = getSelectedCartItemsHandler;
            _shippingCostHandler = shippingCostHandler;
            _logger = logger;
            _auditLogHandler = auditLogHandler;
            _notificationClient = notificationClient;
        }

        public async Task<OrderResponse?> Handle(CreateOrderRequest request)
        {
            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Lấy thông tin phiên checkout từ Redis bằng CheckOutSessionId
                var cacheKey = $"checkout:{request.CheckOutSessionId}";
                var checkoutDataJson = await _cache.GetStringAsync(cacheKey);
                if (string.IsNullOrEmpty(checkoutDataJson))
                {
                    await _unitOfWork.RollbackAsync();
                    return null; // Hoặc có thể fallback lấy thông tin từ các service
                }
                var checkoutData = JsonConvert.DeserializeObject<CheckOutData>(checkoutDataJson);
                if (checkoutData == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // Xác định ShippingAddressId cần dùng:
                // Nếu request có ShippingAddressId mới thì ưu tiên dùng nó, nếu không dùng ShippingAddressId lưu trong phiên checkout.
                int shippingAddressId = request.ShippingAddressId ?? checkoutData.ShippingAddressId;

                // Lấy thông tin địa chỉ giao hàng dựa trên shippingAddressId được xác định
                var shippingAddress = await _getShippingAddressHandler.HandleAsync(shippingAddressId, request.AccountId);
                if (shippingAddress == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }
                decimal shippingCost;
                if (request.ShippingAddressId.HasValue && request.ShippingAddressId.Value != checkoutData.ShippingAddressId)
                {
                    shippingCost = _shippingCostHandler.CalculateShippingCost(shippingAddress.City, shippingAddress.District);
                }
                else
                {
                    shippingCost = checkoutData.ShippingCost;
                }

                // Lấy lại danh sách sản phẩm đã chọn từ phiên checkout (để đảm bảo số lượng, giá cả chưa thay đổi)
                var orderItems = checkoutData.Items;
                if (orderItems == null || !orderItems.Any())
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // Lấy tổng tiền và phí vận chuyển đã tính từ phiên checkout
                var totalAmount = checkoutData.OrderTotal;
                if (totalAmount <= 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }
         
                // Lấy WarehouseId từ appsettings.json, nếu không có thì dùng giá trị mặc định (1)
                int warehouseId = int.TryParse(_configuration["Warehouse:OnlineWarehouseId"], out var wId) ? wId : 1;


                // Tạo Order và copy snapshot thông tin từ ShippingAddress
                var newOrder = _mapper.Map<Order>(shippingAddress);
                newOrder.AccountId = request.AccountId;
                newOrder.CreatedDate = DateTime.UtcNow;
                newOrder.OrderTotal = totalAmount;
                newOrder.ShippingCost = shippingCost;
                newOrder.ShippingAddressId = shippingAddress.AddressId;

                // Thiết lập trạng thái ban đầu tùy theo phương thức thanh toán (sẽ cập nhật sau)
                newOrder.Status = request.PaymentMethod == "PAYOS" ? "Pending Payment" : "Pending Confirmed";
                newOrder.WareHouseId = warehouseId;

                // Lưu Order (chưa commit)
                await _orderRepository.CreateOrderAsync(newOrder);
                await _unitOfWork.SaveChangesAsync();

                // Tạo OrderDetails từ orderItems
                var orderDetails = CreateOrderDetails(newOrder, orderItems);

                // Xử lý theo PaymentMethod
                if (request.PaymentMethod == "PAYOS")
                {
                    // Gọi PayOS tạo link thanh toán
                    var paymentUrl = await _payOSService.CreatePayment(newOrder.OrderId, totalAmount + shippingCost, request.PaymentMethod);
                    if (string.IsNullOrEmpty(paymentUrl))
                    {
                        await _unitOfWork.RollbackAsync();
                        return null;
                    }

                    // Tạo đối tượng Payment (chưa commit)
                    var payment = new Payment
                    {
                        OrderId = newOrder.OrderId,
                        PaymentMethod = request.PaymentMethod,
                        PaymentStatus = "Pending",
                        Amount = totalAmount + shippingCost,
                        TransactionDate = DateTime.UtcNow
                    };
                    await _paymentRepository.SavePaymentAsync(payment);

                    // Lưu OrderDetails
                    await _orderRepository.SaveOrderDetailsAsync(orderDetails);
                    newOrder.OrderDetails = orderDetails;

                    // Sau khi đặt hàng thành công, xóa sản phẩm khỏi giỏ hàng
                    var productVariantIds = orderItems.Select(i => i.ProductVariantId).ToList();
                    var cartCleared = await _customerServiceClient.ClearCartAfterOrderAsync(request.AccountId, productVariantIds);
                    if (!cartCleared)
                    {
                        _logger.LogWarning("Không thể xóa sản phẩm khỏi giỏ hàng sau khi đặt hàng. AccountId: {AccountId}", request.AccountId);
                    }

                    // Commit transaction
                    await _unitOfWork.CommitAsync();

                    var orderResponse = _mapper.Map<OrderResponse>(newOrder);
                    orderResponse.PaymentMethod = request.PaymentMethod;
                    orderResponse.PaymentUrl = paymentUrl;
                    return orderResponse;
                }
                else if (request.PaymentMethod == "COD")
                {
                    var payment = new Payment
                    {
                        OrderId = newOrder.OrderId,
                        PaymentMethod = request.PaymentMethod,
                        PaymentStatus = "Pending",
                        Amount = totalAmount + shippingCost,
                        TransactionDate = DateTime.UtcNow
                    };
                    await _paymentRepository.SavePaymentAsync(payment);

                    // Lưu OrderDetails
                    await _orderRepository.SaveOrderDetailsAsync(orderDetails);

                    newOrder.OrderDetails = orderDetails;

                    // Cập nhật tồn kho ngay lập tức
                    var updateStockSuccess = await _inventoryServiceClient.UpdateStockAfterOrderAsync(warehouseId, orderDetails);
                    if (!updateStockSuccess)
                    {
                        await _unitOfWork.RollbackAsync();
                        return null;
                    }

                    // Xóa sản phẩm đã đặt khỏi giỏ hàng
                    var productVariantIds = orderItems.Select(i => i.ProductVariantId).ToList();
                    var cartCleared = await _customerServiceClient.ClearCartAfterOrderAsync(request.AccountId, productVariantIds);
                    if (!cartCleared)
                    {
                        _logger.LogWarning("Không thể xóa sản phẩm khỏi giỏ hàng sau khi đặt hàng. AccountId: {AccountId}", request.AccountId);
                    }

                    // Ghi lại lịch sử đơn hàng
                    await _auditLogHandler.LogOrderStatusChangeAsync(
                                newOrder.OrderId,
                                "Pending Confirmed",
                                request.AccountId,
                                "Đơn hàng đang chờ xác nhận."
                            );
                    var notificationRequest = new SendNotificationRequest
                    {
                        AccountId = newOrder.AccountId,
                        Title = "Đơn hàng mới",
                        Message = $"Đơn hàng #{newOrder.OrderId} đã được tạo thành công!",
                        NotificationType = "Order",
                        TargetId = newOrder.OrderId,
                        TargetType = "Order"
                    };

                    await _notificationClient.SendNotificationAsync(notificationRequest);

                    await _unitOfWork.CommitAsync();

                    var orderResponse = _mapper.Map<OrderResponse>(newOrder);
                    orderResponse.PaymentMethod = request.PaymentMethod;
                    return orderResponse;
                }

                await _unitOfWork.RollbackAsync();
                return null;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }


        ///////////////////////////////////////////////////////
        private List<OrderDetail> CreateOrderDetails(Order newOrder, List<OrderItemRequest> orderItems)
        {
            return orderItems.Select(item => new OrderDetail
            {
                OrderId = newOrder.OrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.DiscountedPrice,
                DiscountApplied = item.Price-item.DiscountedPrice
            }).ToList();
        }

        /// 📌 Tính tổng tiền đơn hàng
        private async Task<decimal> CalculateTotalAmountAsync(List<OrderItemRequest> cartItems)
        {
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var productVariant = await _inventoryServiceClient.GetProductVariantByIdAsync(item.ProductVariantId);
                if (productVariant == null)
                    return -1; // Nếu không tìm thấy sản phẩm, trả về lỗi

                totalAmount += productVariant.Price * item.Quantity;
            }

            return totalAmount;
        }

        /// 📌 Tạo đơn hàng trong DB và lưu OrderDetails 
        private async Task<Order> CreateOrderAsync(CreateOrderRequest request, decimal totalAmount, decimal shippingCost, List<OrderItemRequest> orderItems, ShippingAddress shippingAddress)
        {
            var order = new Order
            {
                AccountId = request.AccountId,
                CreatedDate = DateTime.UtcNow,
                Status = "Pending",
                OrderTotal = totalAmount,
                ShippingCost = shippingCost,
                ShippingAddressId = shippingAddress.AddressId, // Lưu liên kết đến ShippingAddress

                // Snapshot thông tin giao hàng từ ShippingAddress
                FullName = shippingAddress.RecipientName,
                Email = shippingAddress.Email ?? string.Empty,
                PhoneNumber = shippingAddress.RecipientPhone,
                Address = shippingAddress.Address,
                City = shippingAddress.City ?? string.Empty,
                District = shippingAddress.District ?? string.Empty,
                Country = shippingAddress.Country,
                Province = shippingAddress.Province
            };

            await _orderRepository.CreateOrderAsync(order);

            // Lưu danh sách sản phẩm (OrderDetails)
            var orderDetails = orderItems.Select(item => new OrderDetail
            {
                OrderId = order.OrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.Price,
                DiscountApplied = 0
            }).ToList();

            await _orderRepository.SaveOrderDetailsAsync(orderDetails);

            return order;
        }
        
    }
}
