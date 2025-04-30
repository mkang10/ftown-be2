using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StoreExportRepos : IStoreExportRepos
    {
        private readonly FtownContext _context;

        public StoreExportRepos(FtownContext context)
        {
            _context = context;
        }
        public async Task AddRangeAndSaveAsync(IEnumerable<StoreExportStoreDetail> entities)
        {
            await _context.Set<StoreExportStoreDetail>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
       
    }
}
