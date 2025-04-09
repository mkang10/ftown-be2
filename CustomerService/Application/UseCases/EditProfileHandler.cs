using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interfaces;
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
		private readonly ICustomerProfileDataService _customerProfileDataService;
		private readonly IProfileRepository _profileRepository;
		private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public EditProfileHandler(
			ICustomerProfileDataService customerProfileDataService,
			IProfileRepository profileRepository,
			IMapper mapper,
            ICloudinaryService cloudinaryService)
		{
			_customerProfileDataService = customerProfileDataService;
			_profileRepository = profileRepository;
			_mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

		public async Task<EditProfileResponse> EditProfile(int accountId, EditProfileRequest request)
		{
			var (account, customerDetail) = await _customerProfileDataService.GetAccountAndDetailAsync(accountId);
			if (account == null)
			{
				return new EditProfileResponse { Success = false, Message = "Account not found" };
			}
			if (customerDetail == null)
			{
				return new EditProfileResponse { Success = false, Message = "Customer details not found" };
			}

			_mapper.Map(request, account);
			_mapper.Map(request, customerDetail);

			await _profileRepository.UpdateAccountAsync(account);
			await _profileRepository.UpdateCustomerDetailAsync(customerDetail);

			return new EditProfileResponse { Success = true, Message = "Profile updated successfully" };
		}
	}


}
