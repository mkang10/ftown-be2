using Application.DTO.Response;
using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetOrderHistoryHandler
    {
        private readonly IOrderHistoryRepository _orderHistoryRepository;
        private readonly IMapper _mapper;

        public GetOrderHistoryHandler(IOrderHistoryRepository orderHistoryRepository, IMapper mapper)
        {
            _orderHistoryRepository = orderHistoryRepository;
            _mapper = mapper;
        }

        public async Task<List<OrderHistoryResponse>> HandleAsync(int orderId)
        {
            var orderHistories = await _orderHistoryRepository.GetOrderHistoryByOrderIdAsync(orderId);
            return _mapper.Map<List<OrderHistoryResponse>>(orderHistories);
        }
    }

}
