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
    public class CreateStoreHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public CreateStoreHandler(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<StoreResponse> Handle(StoreRequest request)
        {
            // Dùng AutoMapper để map StoreRequest -> Store Entity
            var store = _mapper.Map<Store>(request);

            // Nếu muốn set CreatedDate mặc định ở code, có thể làm thêm
            store.CreatedDate = DateTime.UtcNow;

            // Tạo store
            var created = await _storeRepository.CreateStoreAsync(store);

            // Map Store Entity -> StoreResponse
            var response = _mapper.Map<StoreResponse>(created);
            return response;
        }
    }
}
