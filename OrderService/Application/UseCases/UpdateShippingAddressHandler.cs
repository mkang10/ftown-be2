using Application.DTO.Request;
using Application.DTO.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class UpdateShippingAddressHandler
    {
        private readonly IShippingAddressRepository _repository;
        private readonly IMapper _mapper;

        public UpdateShippingAddressHandler(IShippingAddressRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ResponseDTO<ShippingAddress>> Handle(int id, UpdateShippingAddressRequest request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResponseDTO<ShippingAddress>(null, true, "Không có địa chỉ đó tồn tại");
            }

            _mapper.Map(request, existing);

            await _repository.UpdateAsync(existing);

            return new ResponseDTO<ShippingAddress>(existing, true, "Cập nhật địa chỉ thành công");
        }
    }


}
