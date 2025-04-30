using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ImportStoreRepos : IImportStoreRepos
    {
        private readonly FtownContext _context;
        public ImportStoreRepos(FtownContext context)
        {
            _context = context;
        }

        public void Add(ImportStoreDetail importStore)
        {
            _context.ImportStoreDetails.Add(importStore);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ImportStoreDetail> details)
       => await _context.ImportStoreDetails.AddRangeAsync(details);
    }

}
