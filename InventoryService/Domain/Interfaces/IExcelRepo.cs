using Domain.Commons;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IExcelRepo
    {
        public Task<Pagination<Product>> GetAllProduct(PaginationParameter paginationParameter);
        public Task<List<Product>> CreateProduct(List<Product> user);

    }
}
