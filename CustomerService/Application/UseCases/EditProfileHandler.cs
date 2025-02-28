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
        private readonly IMapper _mapper;

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

            // Cập nhật Account
            account.FullName = request.FullName;
            account.Email = request.Email;
            account.PhoneNumber = request.PhoneNumber;
            account.Address = request.Address;
            account.ImagePath = request.ImagePath;

            // Cập nhật CustomerDetail
            customerDetail.LoyaltyPoints = request.LoyaltyPoints;
            customerDetail.MembershipLevel = request.MembershipLevel;
            customerDetail.DateOfBirth = request.DateOfBirth;
            customerDetail.Gender = request.Gender;
            customerDetail.CustomerType = request.CustomerType;
            customerDetail.PreferredPaymentMethod = request.PreferredPaymentMethod;

            // Lưu vào database
            await _editProfileRepository.UpdateAccountAsync(account);
            await _editProfileRepository.UpdateCustomerDetailAsync(customerDetail);

            return new EditProfileResponse { Success = true, Message = "Profile updated successfully" };
        }
    }
}
