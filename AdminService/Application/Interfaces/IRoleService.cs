using Application.DTO.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRoleService
    {
        public Task<List<RoleRequestDTO>> GetAll();
        public Task<RoleRequestDTO> Create(RoleRequestDTO role);
        public Task<bool> Delete(int id);
        public Task<bool> Update(int id, RoleRequestDTO role);
    }
}
