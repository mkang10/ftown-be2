using Application.DTO.Request;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ProcessPaymentHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPayOSService _payOSService;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessPaymentHandler(
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IPayOSService payOSService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _payOSService = payOSService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Xử lý thanh toán theo PaymentRequest và trả về PaymentUrl (với PAYOS) hoặc thông báo thành công (với COD)
        /// </summary>
        public async Task<string?> Handle(PaymentRequest request)
        {
            // 1️⃣ Lấy đơn hàng từ DB
            var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
            if (order == null || order.OrderTotal <= 0)
                return null;

            // 2️⃣ Phân nhánh xử lý theo phương thức thanh toán
            return request.PaymentMethod switch
            {
                "PAYOS" => await ProcessPayOSPayment(order, request),
                "COD" => await ProcessCODPayment(order),
                _ => null
            };
        }

        /// <summary>
        /// Xử lý thanh toán qua PAYOS: tạo giao dịch thanh toán, lưu thông tin payment, cập nhật trạng thái đơn hàng thành "Pending Payment"
        /// </summary>
        private async Task<string?> ProcessPayOSPayment(Order order, PaymentRequest request)
        {
            // Tính tổng tiền bao gồm phí vận chuyển
            var totalAmount = (order.OrderTotal ?? 0) + (order.ShippingCost ?? 0);

            // Gọi dịch vụ PayOS tạo giao dịch thanh toán
            var paymentUrl = await _payOSService.CreatePayment(order.OrderId, totalAmount, request.PaymentMethod);
            if (string.IsNullOrEmpty(paymentUrl))
                return null;

            // Lưu thông tin thanh toán với trạng thái "Pending"
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending",
                Amount = totalAmount,
                TransactionDate = DateTime.UtcNow
            };
            await _paymentRepository.SavePaymentAsync(payment);

            // Cập nhật trạng thái đơn hàng sang "Pending Payment"
            order.Status = "Pending Payment";
            await _orderRepository.UpdateOrderAsync(order);

            return paymentUrl;
        }

        /// <summary>
        /// Xử lý thanh toán COD: cập nhật trạng thái đơn hàng và lưu thông tin thanh toán ngay lập tức
        /// </summary>
        private async Task<string?> ProcessCODPayment(Order order)
        {
            // Cập nhật trạng thái đơn hàng ngay, với COD đơn hàng được xác nhận luôn
            order.Status = "Completed"; // Hoặc "Confirmed" / "Processing" tùy nghiệp vụ
            await _orderRepository.UpdateOrderAsync(order);

            // Tạo thông tin thanh toán cho COD với trạng thái "Completed"
            var codPayment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = "COD",
                PaymentStatus = "Completed",
                Amount = order.OrderTotal ?? 0,
                TransactionDate = DateTime.UtcNow
            };
            await _paymentRepository.SavePaymentAsync(codPayment);

            return "COD_PAYMENT_SUCCESS";
        }

        /// <summary>
        /// Xử lý callback từ PayOS khi thanh toán được thực hiện thành công hay thất bại
        /// </summary>
        public async Task<bool> HandleCallback(PayOSCallbackRequest callback)
        {
            // Mở transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(callback.OrderId);
                if (order == null)
                    return false;

                var payment = await _paymentRepository.GetPaymentByOrderIdAsync(callback.OrderId);
                if (payment == null)
                    return false;

                if (callback.Status == "SUCCESS")
                {
                    payment.PaymentStatus = "Completed";
                    order.Status = "Completed";
                }
                else
                {
                    payment.PaymentStatus = "Failed";
                    order.Status = "Failed";
                }

                // Cập nhật các entity, không gọi SaveChangesAsync ở đây
                await _paymentRepository.UpdatePaymentAsync(payment);
                await _orderRepository.UpdateOrderAsync(order);

                // Commit => save thay đổi + commit transaction
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                // Nếu có lỗi, rollback transaction
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        //public async Task<bool> HandlePaymentWebhook(PaymentWebhookRequest request)
        //{
        //    var payment = await _paymentRepository.GetPaymentByOrderIdAsync(request.OrderId);
        //    if (payment == null)
        //    {
        //        return false;
        //    }

        //    if (request.PaymentStatus == "Paid")
        //    {
        //        payment.PaymentStatus = "Paid";
        //        await _paymentRepository.UpdatePaymentAsync(payment);

        //        // Lấy đơn hàng liên quan
        //        var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
        //        if (order == null)
        //        {
        //            return false;
        //        }

        //        order.Status = "Confirmed"; // ✅ Đơn hàng chuyển sang trạng thái đã xác nhận

        //        // Lấy danh sách sản phẩm trong đơn hàng
        //        var orderDetails = await _orderRepository.GetOrderDetailsByOrderIdAsync(request.OrderId);

        //        // ✅ Cập nhật tồn kho khi thanh toán hoàn tất
        //        var updateStockSuccess = await _inventoryServiceClient.UpdateStockAfterOrderAsync(order.StoreId, orderDetails);
        //        if (!updateStockSuccess)
        //        {
        //            return false; // Không thể cập nhật tồn kho
        //        }

        //        await _unitOfWork.SaveChangesAsync();
        //        return true;
        //    }

        //    return false;
        //}

    }
}
