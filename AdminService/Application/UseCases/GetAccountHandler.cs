using Application.DTO.Request;
using Application.Interfaces;
using AutoMapper;
using Domain.Commons;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetAccountHandler : IUserManagementService
    {
        private readonly IUserManagementRepository _userManagementRepository;
        private readonly IMapper _mapper;

        public GetAccountHandler(IUserManagementRepository userManagementRepository, IMapper mapper)
        {
            _userManagementRepository = userManagementRepository;
            _mapper = mapper;
        }

        public async Task<UserRequestDTO> createUser(UserRequestDTO user)
        {
            try
            {
                var map = _mapper.Map<Account>(user);
                var userCreate = await _userManagementRepository.CreateUser(map);
                var resutl = _mapper.Map<UserRequestDTO>(userCreate);
                return resutl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> deleteUser(int id)
        {
            try
            {
                var user = await _userManagementRepository.GetUserById(id);
                if (user == null)
                {
                    throw new Exception($"User {id} does not exist");
                }

                await _userManagementRepository.DeleteUser(user);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task<Account> getAccountInfoByAccountName(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Account> getAccountInfoByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<Account> getAccountInfoById(int id)
        {
            var data = await _userManagementRepository.GetUserById(id);

            return data;
        }

        public async Task<Pagination<UserRequestDTO>> GetAllUserAscyn(PaginationParameter paginationParameter)
        {
            var trips = await _userManagementRepository.GetAllUser(paginationParameter);
            if (!trips.Any())
            {
                return null;
            }
            var tripModels = _mapper.Map<List<UserRequestDTO>>(trips);

            return new Pagination<UserRequestDTO>(tripModels,
                trips.TotalCount,
                trips.CurrentPage,
                trips.PageSize);
        }

        public async Task<bool> updateUser(int id, UserRequestDTO user)
        {
            try
            {
                var userData = await _userManagementRepository.GetUserById(id);
                if (userData == null)
                {
                    return false;
                }

                _mapper.Map(user, userData);
                await _userManagementRepository.UpdateUser(userData);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Fail to update user info {ex.Message}");
                return false;
            }
        }
    }
}
