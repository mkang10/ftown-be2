using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Request
{
    public class RoleRequestDTO
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public string? Description { get; set; }
    }
    public class RoleCreateRequestDTO
    {

        public string RoleName { get; set; } = null!;

        public string? Description { get; set; }
    }
}
