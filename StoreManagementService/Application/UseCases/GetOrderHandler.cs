using AutoMapper;
using Domain.DTO.Response.Domain.DTO.Response;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.DTO.Response.OrderDTO;

namespace Application.UseCases
{
  
        public class GetOrderHandler
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IMapper _mapper;
            public GetOrderHandler(IOrderRepository orderRepository, IMapper mapper)
            {
                _orderRepository = orderRepository;
                _mapper = mapper;
            }

        public async Task<ResponseDTO<PaginatedResponseDTO<OrderWithBuyerDTO>>> GetAllOrdersWithAssignmentsAsync(
         int page,
         int pageSize,
         string? orderStatus = null,
         System.DateTime? orderStartDate = null,
         System.DateTime? orderEndDate = null,
         int? shopManagerId = null,
         int? staffId = null,
         System.DateTime? assignmentStartDate = null,
         System.DateTime? assignmentEndDate = null)
        {
            // Gọi repository để lấy danh sách Order theo filter và phân trang
            var paginatedOrders = await _orderRepository.GetAllOrdersWithAssignmentsAsync(
                page,
                pageSize,
                orderStatus,
                orderStartDate,
                orderEndDate,
                shopManagerId,
                staffId,
                assignmentStartDate,
                assignmentEndDate);

            // Sử dụng AutoMapper ánh xạ từ Order entity sang OrderDTO
            var orderDTOs = _mapper.Map<List<OrderWithBuyerDTO>>(paginatedOrders.Data);

            // Đóng gói lại kết quả phân trang sử dụng DTO
            var paginatedResult = new PaginatedResponseDTO<OrderWithBuyerDTO>(
                orderDTOs,
                paginatedOrders.TotalRecords,
                paginatedOrders.Page,
                paginatedOrders.PageSize);

            return new ResponseDTO<PaginatedResponseDTO<OrderWithBuyerDTO>>(
                paginatedResult, true, "Lấy dữ liệu thành công");
        }
    }
}
