using Application.DTO.Request;
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
    public class EditProfileHandler
    {
        private readonly IEditProfileRepository _editProfileRepository;
        private readonly IMapper _mapper; // Inject AutoMapper

        public EditProfileHandler(IEditProfileRepository editProfileRepository, IMapper mapper)
        {
            _editProfileRepository = editProfileRepository;
            _mapper = mapper;
        }

        public async Task<EditProfileResponse> EditProfile(int accountId, EditProfileRequest request)
        {
            var account = await _editProfileRepository.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return new EditProfileResponse { Success = false, Message = "Account not found" };
            }

            var customerDetail = await _editProfileRepository.GetCustomerDetailByAccountIdAsync(accountId);
            if (customerDetail == null)
            {
                return new EditProfileResponse { Success = false, Message = "Customer details not found" };
            }

            // Sử dụng AutoMapper để cập nhật Account và CustomerDetail
            _mapper.Map(request, account);
            _mapper.Map(request, customerDetail);

            // Lưu vào database
            await _editProfileRepository.UpdateAccountAsync(account);
            await _editProfileRepository.UpdateCustomerDetailAsync(customerDetail);

            return new EditProfileResponse { Success = true, Message = "Profile updated successfully" };
        }
    }

}
