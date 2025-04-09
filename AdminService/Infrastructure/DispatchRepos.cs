using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DispatchRepos : IDispatchRepos { 
    
        
    private readonly FtownContext _context;
        public DispatchRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(Dispatch dispatch)
        {
            _context.Dispatches.Add(dispatch);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }

}
