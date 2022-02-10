using Mango.Services.CouponAPI.Dtos;
using Mango.Services.CouponAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [ApiController]
    [Route("api/coupon")]
    public class CouponAPIController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;
        protected ResponseDto _responseDto;
        public CouponAPIController(ICouponRepository shoppingCartRepository)
        {
            _couponRepository = shoppingCartRepository;
            this._responseDto = new ResponseDto();
        }
     
        [HttpGet("{code}")]
        public async Task<object> GetGetDiscountForCode(string code)
        {
            try
            {
                var cart = await _couponRepository.GetCouponByCode(code);
                _responseDto.Result = cart;
            }
            catch (Exception ex)
            {

                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }
       
    }
}
