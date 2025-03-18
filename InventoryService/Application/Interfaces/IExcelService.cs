using Application.DTO.Request;
using Domain.Commons;
using Domain.Entities;
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
        public Task<List<CreateProductDTORequest>> CreateProduct(List<CreateProductDTORequest> user);

    }
}
