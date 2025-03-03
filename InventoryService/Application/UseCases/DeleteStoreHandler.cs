using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DeleteStoreHandler
    {
        private readonly IStoreRepository _storeRepository;

        public DeleteStoreHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<bool> Handle(int storeId)
        {
            var store = await _storeRepository.GetStoreByIdAsync(storeId);
            if (store == null) return false;

            await _storeRepository.DeleteStoreAsync(storeId);
            return true;
        }
    }
}
