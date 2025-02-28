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
    public class GetStoreByIdHandler
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IMapper _mapper;

        public GetStoreByIdHandler(IStoreRepository storeRepository, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _mapper = mapper;
        }

        public async Task<StoreResponse?> Handle(int storeId)
        {
            var store = await _storeRepository.GetStoreByIdAsync(storeId);
            if (store == null) return null;

            // Map sang StoreResponse
            return _mapper.Map<StoreResponse>(store);
        }
    }

}
