using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.Dtos;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class ShoppingCartAPIController : ControllerBase
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IMessageBus _messageBus;
        private readonly ResponseDto _response;
        public ShoppingCartAPIController(IShoppingCartRepository shoppingCartRepository, ICouponRepository couponRepository, IMessageBus messageBus)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _couponRepository = couponRepository;
            _messageBus = messageBus;
            _response = new ResponseDto();
        }
        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                var cart = await _shoppingCartRepository.GetCartByUserId(userId);
                _response.Result = cart;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPost("AddCart")]
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                var cart = await _shoppingCartRepository.CreateUpdateCart(cartDto);
                _response.Result = cart;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                var cart = await _shoppingCartRepository.CreateUpdateCart(cartDto);
                _response.Result = cart;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                var cart = await _shoppingCartRepository.RemoveFromCart(cartId);
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cart = await _shoppingCartRepository.ApplyCoupon(cartDto.CartHeader.UsertId, cartDto.CartHeader.CouponCode);
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string usertId)
        {
            try
            {
                var cart = await _shoppingCartRepository.RemoveCoupon(usertId);
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkoutHeader)
        {
            try
            {
                var cart = await _shoppingCartRepository.GetCartByUserId(checkoutHeader.UsertId);
                if (cart == null) 
                {
                    return BadRequest();
                }
                if(!string.IsNullOrEmpty(checkoutHeader.CouponCode))
                {
                    var coupon = await _couponRepository.GetCouponByCode(checkoutHeader.CouponCode);
                    if(checkoutHeader.DiscountTotal !=coupon.Discount)
                    {
                        _response.IsSuccess=false;
                        _response.ErrorMessages = new List<string>() { "Coupon Price has changes, please confirm." };
                        _response.DisplayMassage = "Coupon Price has changes, please confirm.";
                        return _response;
                    }
                }
                checkoutHeader.CartDetails = cart.CartDetails;
                //await _messageBus.PublishMessage(checkoutHeader, "checkoutmessagetopic");
                await _messageBus.PublishMessage(checkoutHeader, "checkoutqueue");                
                await _shoppingCartRepository.ClearCart(checkoutHeader.UsertId);
               
            }
            catch (Exception ex)
            {

                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
