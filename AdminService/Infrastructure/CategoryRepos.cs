using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class CategoryRepos : ICategoryRepos
    {
        private readonly FtownContext _context;

        public CategoryRepos(FtownContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _context.Categories.AsNoTracking().ToListAsync();

        public async Task<Category?> GetByIdAsync(int id)
            => await _context.Categories.FindAsync(id);

        public async Task AddAsync(Category category)
            => await _context.Categories.AddAsync(category);

        public void Update(Category category)
            => _context.Categories.Update(category);

        public void Delete(Category category)
            => _context.Categories.Remove(category);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
