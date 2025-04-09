using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class GetShippingAddressHandler
    {
        private readonly IShippingAddressRepository _shippingAddressRepository;
        private readonly IRedisCacheService _redisCacheService;

        public GetShippingAddressHandler(
            IShippingAddressRepository shippingAddressRepository,
            IRedisCacheService redisCacheService)
        {
            _shippingAddressRepository = shippingAddressRepository;
            _redisCacheService = redisCacheService;
        }

        // Hàm tạo key cho cache dựa trên ShippingAddressId
        private string GetCacheKey(int shippingAddressId) => $"shippingaddress:{shippingAddressId}";
        private string GetAllAddressesCacheKey(int accountId) => $"shippingaddresses:account:{accountId}";
        /// <summary>
        /// Lấy thông tin địa chỉ giao hàng cho tài khoản từ cache, nếu không có thì lấy từ DB và cập nhật cache.
        /// </summary>
        /// <param name="shippingAddressId">ID của địa chỉ giao hàng</param>
        /// <param name="accountId">ID của tài khoản để kiểm tra tính hợp lệ</param>
        /// <returns>ShippingAddress nếu tồn tại và thuộc về tài khoản, ngược lại trả về null.</returns>
        public async Task<ShippingAddress?> HandleAsync(int shippingAddressId, int accountId)
        {

            var cacheKey = GetCacheKey(shippingAddressId);

            // Kiểm tra dữ liệu trên Redis
            var cachedAddress = await _redisCacheService.GetCacheAsync<ShippingAddress>(cacheKey);
            if (cachedAddress != null)
            {
                // Kiểm tra xem địa chỉ có thuộc tài khoản yêu cầu không
                if (cachedAddress.AccountId == accountId)
                    return cachedAddress;
                else
                    return null;
            }

            // Nếu không có trong cache, lấy từ Database
            var shippingAddress = await _shippingAddressRepository.GetByIdAsync(shippingAddressId);
            if (shippingAddress != null && shippingAddress.AccountId == accountId)
            {
                // Lưu vào cache với thời gian hết hạn (ví dụ: 1 giờ)
                await _redisCacheService.SetCacheAsync(cacheKey, shippingAddress, TimeSpan.FromHours(1));
            }

            return shippingAddress;
        }

        /// <summary>
        /// Lấy danh sách tất cả địa chỉ giao hàng của tài khoản.
        /// </summary>
        /// <param name="accountId">ID tài khoản</param>
        /// <returns>Danh sách địa chỉ giao hàng</returns>
        public async Task<List<ShippingAddress>> GetAllByAccountIdAsync(int accountId)
        {
            var cacheKey = GetAllAddressesCacheKey(accountId);

            // Thử lấy từ cache trước
            var cachedList = await _redisCacheService.GetCacheAsync<List<ShippingAddress>>(cacheKey);
            if (cachedList != null)
                return cachedList;

            // Nếu không có cache, truy vấn DB
            var addresses = await _shippingAddressRepository.GetShippingAddressesByAccountIdAsync(accountId);

            if (addresses != null && addresses.Any())
            {
                await _redisCacheService.SetCacheAsync(cacheKey, addresses, TimeSpan.FromHours(1));
            }

            return addresses ?? new List<ShippingAddress>();
        }
    }
}
