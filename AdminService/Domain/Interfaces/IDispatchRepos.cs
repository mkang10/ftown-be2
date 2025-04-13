using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDispatchRepos { 
        void Add(Dispatch dispatch);
        Task SaveChangesAsync();

        // duc anh
        public Task<Dispatch> GetJSONDispatchById(int id);

        Task<StoreExportStoreDetail> GetStoreExportStoreDetailById(int importId);
        //====

    }
}
