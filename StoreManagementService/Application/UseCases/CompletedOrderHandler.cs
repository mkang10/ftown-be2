using AutoMapper;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UseCases.CompletedOrderHandler;
using static Domain.DTO.Response.OrderDoneRes;

namespace Application.UseCases
{
    
        public class CompletedOrderHandler 
        {
            private readonly IOrderRepository _orderRepo;
            private readonly IMapper _mapper;

            public CompletedOrderHandler(IOrderRepository orderRepo, IMapper mapper)
            {
                _orderRepo = orderRepo;
                _mapper = mapper;
            }

            public async Task<ResponseDTO<OrderResponseDTO>> CompleteOrderAsync(int orderId)
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    return new ResponseDTO<OrderResponseDTO>(
                        data: null!,
                        status: false,
                        message: $"Không tìm thấy Order với ID = {orderId}"
                    );
                }

                // Cập nhật trạng thái
                order.Status = "Completed";

                // Lưu vào DB
                await _orderRepo.SaveChangesAsync();

                // Map về DTO
                var dto = _mapper.Map<OrderResponseDTO>(order);
                return new ResponseDTO<OrderResponseDTO>(
                    data: dto,
                    status: true,
                    message: "Order đã được cập nhật thành Completed."
                );
            }
        
    }
}
