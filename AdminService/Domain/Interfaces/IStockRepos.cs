using Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IStockRepos
    {
        /// <summary>
        /// Lấy bản ghi Stock theo kho và variant. Trả về null nếu không tìm thấy.
        /// </summary>
        Task<WareHousesStock> GetByWarehouseAndVariantAsync(int warehouseId, int variantId);
            }
}
