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
    }
}
