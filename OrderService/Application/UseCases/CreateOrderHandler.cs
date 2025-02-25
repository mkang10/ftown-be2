using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerServiceClient _customerServiceClient;
        private readonly IInventoryServiceClient _inventoryServiceClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AutoSelectStoreHandler _autoSelectStoreHandler;
        private readonly IShippingAddressRepository _shippingAddressRepository;

        public CreateOrderHandler(
            IOrderRepository orderRepository,
            ICustomerServiceClient customerServiceClient,
            IInventoryServiceClient inventoryServiceClient,
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IConfiguration configuration,
            AutoSelectStoreHandler autoSelectStoreHandler,
            IUnitOfWork unitOfWork,
            IShippingAddressRepository shippingAddressRepository)
        {
            _orderRepository = orderRepository;
            _customerServiceClient = customerServiceClient;
            _inventoryServiceClient = inventoryServiceClient;
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _configuration = configuration;
            _autoSelectStoreHandler = autoSelectStoreHandler;
            _unitOfWork = unitOfWork;
            _shippingAddressRepository = shippingAddressRepository;
        }

        public async Task<OrderResponse?> Handle(CreateOrderRequest request)
        {
            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Lấy thông tin địa chỉ giao hàng dựa trên ShippingAddressId
                var shippingAddress = await _shippingAddressRepository.GetByIdAsync(request.ShippingAddressId);
                if (shippingAddress == null || shippingAddress.AccountId != request.AccountId)
                {
                    await _unitOfWork.RollbackAsync();
                    return null; // Không tìm thấy địa chỉ hoặc không thuộc tài khoản
                }

                // 2. Lấy giỏ hàng từ CustomerService
                var cart = await _customerServiceClient.GetCartAsync(request.AccountId) ?? new List<CartItem>();

                var orderItems = new List<OrderItemRequest>();
                foreach (var item in cart)
                {
                    // Lấy thông tin sản phẩm từ InventoryService
                    var productVariant = await _inventoryServiceClient.GetProductVariantByIdAsync(item.ProductVariantId);
                    if (productVariant == null)
                        continue;

                    orderItems.Add(new OrderItemRequest
                    {
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        Price = productVariant.Price
                    });
                }

                // 3. Tính tổng tiền đơn hàng
                var totalAmount = await CalculateTotalAmountAsync(orderItems);
                if (totalAmount <= 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // 4. Tính phí vận chuyển dựa vào thông tin từ ShippingAddress
                decimal shippingCost = GetShippingCost(shippingAddress.City, shippingAddress.District);

                // 5. Xác định cửa hàng (Store)
                int storeId = request.StoreId ?? await _autoSelectStoreHandler.AutoSelectStoreAsync(orderItems, shippingAddress.City, shippingAddress.District);
                if (storeId == 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return null;
                }

                // 6. Tạo Order và copy snapshot thông tin địa chỉ từ ShippingAddress
                var newOrder = new Order
                {
                    AccountId = request.AccountId,
                    CreatedDate = DateTime.UtcNow,
                    Status = "Pending", // Trạng thái ban đầu
                    OrderTotal = totalAmount,
                    ShippingCost = shippingCost,
                    ShippingAddressId = shippingAddress.AddressId, // Giữ liên kết với ShippingAddress

                    // Snapshot thông tin địa chỉ giao hàng từ ShippingAddress
                    Address = shippingAddress.Address,
                    City = shippingAddress.City ?? string.Empty,
                    District = shippingAddress.District ?? string.Empty,
                    Country = shippingAddress.Country,
                    Province = shippingAddress.Province,

                    // Snapshot thông tin người nhận
                    FullName = shippingAddress.RecipientName,
                    Email = shippingAddress.Email ?? string.Empty,
                    PhoneNumber = shippingAddress.RecipientPhone
                };

                // Lưu Order (chưa commit)
                await _orderRepository.CreateOrderAsync(newOrder);
                await _unitOfWork.SaveChangesAsync();

                // 7. Xử lý theo PaymentMethod
                if (request.PaymentMethod == "PAYOS")
                {
                    newOrder.Status = "Pending Payment";
                    newOrder.StoreId = storeId;

                    // Gọi PayOS tạo link thanh toán
                    var paymentUrl = await _payOSService.CreatePayment(newOrder.OrderId, totalAmount + shippingCost, request.PaymentMethod);
                    if (string.IsNullOrEmpty(paymentUrl))
                    {
                        await _unitOfWork.RollbackAsync();
                        return null;
                    }

                    // Tạo payment (chưa lưu commit)
                    var payment = new Payment
                    {
                        OrderId = newOrder.OrderId,
                        PaymentMethod = request.PaymentMethod,
                        PaymentStatus = "Pending",
                        Amount = totalAmount + shippingCost,
                        TransactionDate = DateTime.UtcNow
                    };
                    await _paymentRepository.SavePaymentAsync(payment);

                    // Xóa giỏ hàng sau khi đặt đơn
                    await _customerServiceClient.ClearCartAfterOrderAsync(request.AccountId);

                    // Commit transaction
                    await _unitOfWork.CommitAsync();

                    // Trả về kết quả OrderResponse
                    return new OrderResponse
                    {
                        OrderId = newOrder.OrderId,
                        Status = newOrder.Status,
                        OrderTotal = newOrder.OrderTotal ?? 0,
                        ShippingCost = newOrder.ShippingCost ?? 0,
                        PaymentMethod = "PAYOS",
                        PaymentUrl = paymentUrl,
                        StoreId = storeId
                    };
                }
                else if (request.PaymentMethod == "COD")
                {
                    // Tạo OrderDetails dựa trên giỏ hàng
                    var orderDetails = orderItems.Select(item => new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = item.Price,
                        DiscountApplied = 0
                    }).ToList();
                    await _orderRepository.SaveOrderDetailsAsync(orderDetails);

                    // Cập nhật trạng thái đơn hàng
                    newOrder.Status = "Confirmed";
                    newOrder.StoreId = storeId;

                    // Xóa giỏ hàng sau khi đặt đơn
                    await _customerServiceClient.ClearCartAfterOrderAsync(request.AccountId);

                    // Commit transaction
                    await _unitOfWork.CommitAsync();

                    return new OrderResponse
                    {
                        OrderId = newOrder.OrderId,
                        Status = newOrder.Status,
                        OrderTotal = newOrder.OrderTotal ?? 0,
                        ShippingCost = newOrder.ShippingCost ?? 0,
                        PaymentMethod = "COD",
                        StoreId = storeId,
                        Items = orderDetails.Select(od => new OrderItemResponse
                        {
                            ProductVariantId = od.ProductVariantId,
                            Quantity = od.Quantity,
                            PriceAtPurchase = od.PriceAtPurchase,
                            DiscountApplied = od.DiscountApplied ?? 0
                        }).ToList()
                    };
                }

                // Nếu PaymentMethod không khớp với PAYOS hoặc COD
                await _unitOfWork.RollbackAsync();
                return null;
            }
            catch
            {
                // Xảy ra lỗi, rollback transaction
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }



        ///////////////////////////////////////////////////////


        /// 📌 Tính phí vận chuyển
        private decimal GetShippingCost(string city, string district)
        {
             var urbanAreas = new List<string> { "Hà Nội", "Hồ Chí Minh", "Đà Nẵng" };
             decimal urbanFee = decimal.TryParse(_configuration["ShippingFees:Urban"], out var uFee) ? uFee : 20000;
             decimal suburbanFee = decimal.TryParse(_configuration["ShippingFees:Suburban"], out var sFee) ? sFee : 35000;

             return urbanAreas.Contains(city) ? urbanFee : suburbanFee;
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
