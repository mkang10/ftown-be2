using Application.DTO.Request;
using Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IExcelService
    {
        public Task<Pagination<ProductExcelRequestDTO>> GetAllProduct(PaginationParameter paginationParameter);

    }
}
