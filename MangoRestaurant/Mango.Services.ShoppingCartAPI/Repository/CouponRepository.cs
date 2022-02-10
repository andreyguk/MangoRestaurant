using Mango.Services.ShoppingCartAPI.Models.Dtos;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient _httpClient;
        public CouponRepository(HttpClient httpClien)
        {
            _httpClient = httpClien;
        }
        public async Task<CouponDto> GetCouponByCode(string couponCode)
        {
            var response = await _httpClient.GetAsync($"/api/coupon/{ couponCode}");
            var apiContent = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if (responseDto != null && responseDto.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(responseDto.Result));
            }
            return new CouponDto();
        }
    }
}
