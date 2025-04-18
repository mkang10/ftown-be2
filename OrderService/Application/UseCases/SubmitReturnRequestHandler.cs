﻿using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class SubmitReturnRequestHandler
    {
        private readonly IDistributedCache _cache;
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public SubmitReturnRequestHandler(IDistributedCache cache, IReturnOrderRepository returnOrderRepository, ICloudinaryService cloudinaryService)
        {
            _cache = cache;
            _returnOrderRepository = returnOrderRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<SubmitReturnResponse?> Handle(SubmitReturnRequest request)
        {
            var cacheKey = $"return-checkout:{request.ReturnCheckoutSessionId}";
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData == null) return null;

            var json = Encoding.UTF8.GetString(cachedData);
            var returnCheckoutData = JsonConvert.DeserializeObject<ReturnCheckoutData>(json);
            if (returnCheckoutData == null || !returnCheckoutData.Items.Any()) return null;

            // ✅ 1️⃣ Tải hình ảnh/video lên Cloudinary và lưu danh sách URL
            var mediaUrls = new List<string>();
            foreach (var file in request.MediaFiles)
            {
                var mediaUrl = await _cloudinaryService.UploadMediaAsync(file);
                if (!string.IsNullOrEmpty(mediaUrl))
                {
                    mediaUrls.Add(mediaUrl);
                }
            }

            // ✅ 2️⃣ Tạo đối tượng `ReturnOrder`
            var returnOrder = new ReturnOrder
            {
                OrderId = returnCheckoutData.OrderId,
                AccountId = returnCheckoutData.AccountId,
                Email = request.Email,
                TotalRefundAmount = returnCheckoutData.TotalRefundAmount,
                ReturnReason = request.ReturnReason,
                ReturnOption = request.ReturnOption,
                RefundMethod = request.RefundMethod,
                ReturnDescription = request.ReturnDescription,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                ReturnImages = mediaUrls.Any() ? JsonConvert.SerializeObject(mediaUrls) : null // ✅ Lưu URL ảnh/video vào JSON
            };

            // ✅ 3️⃣ Nếu chọn phương thức hoàn tiền qua ngân hàng, lưu thông tin ngân hàng
            var refundMethodLower = request.RefundMethod.Trim().ToLower();
            if (refundMethodLower == "Hoàn tiền")
            {
                returnOrder.BankName = request.BankName;
                returnOrder.BankAccountNumber = request.BankAccountNumber;
                returnOrder.BankAccountName = request.BankAccountName;
            }

            // ✅ 4️⃣ Lưu đơn đổi trả vào DB
            await _returnOrderRepository.CreateReturnOrderAsync(returnOrder);

            // ✅ 5️⃣ Lưu danh sách sản phẩm đổi trả vào `ReturnOrderItem`
            var returnOrderItems = returnCheckoutData.Items.Select(item => new ReturnOrderItem
            {
                ReturnOrderId = returnOrder.ReturnOrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                RefundPrice = item.Price * item.Quantity // ✅ Tính số tiền hoàn lại cho sản phẩm
            }).ToList();

            await _returnOrderRepository.AddReturnOrderItemsAsync(returnOrderItems);

            // ✅ 6️⃣ Xóa dữ liệu cache sau khi hoàn tất
            await _cache.RemoveAsync(cacheKey);

            return new SubmitReturnResponse
            {
                ReturnOrderId = returnOrder.ReturnOrderId,
                Status = "Pending",
            };
        }

    }


}
