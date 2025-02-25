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
    public class GetAllStoresHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public GetAllStoresHandler(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<List<StoreResponse>> Handle()
        {
            var stores = await _storeRepository.GetAllStoresAsync();
            // Dùng AutoMapper: map List<Store> -> List<StoreResponse>
            var result = _mapper.Map<List<StoreResponse>>(stores);
            return result;
        }
    }

}
