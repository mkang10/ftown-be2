using Application.DTO.Response;
using AutoMapper;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetCustomerProfileHandler
    {
        private readonly IEditProfileRepository _editProfileRepository;
        private readonly IMapper _mapper;

        public GetCustomerProfileHandler(IEditProfileRepository editProfileRepository, IMapper mapper)
        {
            _editProfileRepository = editProfileRepository;
            _mapper = mapper;
        }

        public async Task<CustomerProfileResponse?> GetCustomerProfile(int accountId)
        {
            var (account, customerDetail) = await _editProfileRepository.GetCustomerProfileByAccountIdAsync(accountId);
            if (account == null || customerDetail == null)
            {
                return null;
            }

            // Mapping dữ liệu từ Entity -> DTO
            var response = _mapper.Map<CustomerProfileResponse>(account);
            _mapper.Map(customerDetail, response);

            return response;
        }
    }
}
