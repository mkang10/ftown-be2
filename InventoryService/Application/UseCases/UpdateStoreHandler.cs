using Application.DTO.Request;
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
    public class UpdateStoreHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public UpdateStoreHandler(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<StoreResponse?> Handle(int storeId, StoreRequest request)
        {
            var store = await _storeRepository.GetStoreByIdAsync(storeId);
            if (store == null) return null;

            // Map từ request sang entity sẵn có
            // Lưu ý: AutoMapper mặc định map sang 1 object mới
            // => Muốn "map lên" object cũ, ta dùng Overload thứ 2: _mapper.Map(source, destination)
            _mapper.Map(request, store);

            // store.CreatedDate = store.CreatedDate; // Giữ nguyên, nếu muốn
            // store.StoreId = store.StoreId; // Giữ nguyên
            // => .ForMember(..., opt => opt.Ignore()) trong profile

            await _storeRepository.UpdateStoreAsync(store);

            // Trả về response
            return _mapper.Map<StoreResponse>(store);
        }
    }

}
