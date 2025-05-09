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
    public interface IUserManagementService
    {
        public Task<Pagination<UserRequestDTO>> GetAllUserAscyn(int id, PaginationParameter paginationParameter);
        public  Task<Pagination<UserRequestDTO>> GetUserByGmailHandler(string gmail, PaginationParameter paginationParameter);

        public Task<UserRequestDTO> createUser(CreateUserRequestWithPasswordDTO user);
        public Task<bool> deleteUser(int id);
        public Task<bool> updateUser(int id, UpdateAccountShopManagerStaffRequest user);
        public Task<bool> banUser(int id);



        public Task<CreateShopmanagerDetailRequest> CreateShopmanagerDetail(CreateShopmanagerDetailRequest user);
        public Task<bool> UpdateShopmanagerDetail(int id, UpdateShopmanagerDetailRequest user);
        public Task<CreateShopmanagerDetailRequest> getShopmanagerDetaibyid(int id);



        public Task<UserRequestDTO> getAccountInfoById(int id);


    }
}
