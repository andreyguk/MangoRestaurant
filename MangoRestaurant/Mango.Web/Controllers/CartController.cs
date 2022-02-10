using Mango.Web.Models.Dtos;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(IProductService productService, ICartService cartService, ICouponService couponService)
        {
            _productService = productService;
            _cartService = cartService;
            _couponService = couponService;
        }
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartByLoggedUser());
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var usertId = User.Claims.Where(u => u.Type == "sid")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.ApplyCouppon<ResponseDto>(cartDto, accessToken);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }
        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            var usertId = User.Claims.Where(u => u.Type == "sid")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.RemoveCoupon<ResponseDto>(cartDto.CartHeader.UsertId, accessToken);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        public async Task<IActionResult> Remove(int cardDetailId)
        {
            var usertId = User.Claims.Where(u => u.Type == "sid")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.RemoveFromAsync<ResponseDto>(cardDetailId, accessToken);
            var cartDto = new CartDto();

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }

            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartByLoggedUser());
        }

        [HttpPost]   
        public async Task<IActionResult> Checkout(CartDto  cartDto)
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var response = await _cartService.Checkout<ResponseDto>(cartDto.CartHeader, accessToken);
                if(!response.IsSuccess)
                {
                    TempData["Error"] = response.DisplayMassage;
                    return RedirectToAction(nameof(Checkout));
                }
                return RedirectToAction(nameof(Confirmation));
            }
            catch (Exception)
            {

                return View(cartDto);
            }
        }
        public async Task<IActionResult> Confirmation()
        {
            return View();
        }

        private async Task<CartDto> LoadCartByLoggedUser()
        {
            var usertId = User.Claims.Where(u => u.Type == "sid")?.FirstOrDefault()?.Value;
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.GetCartByUserIdAsync<ResponseDto>(usertId, accessToken);
            var cartDto = new CartDto();

            if (response != null && response.IsSuccess)
            {
                cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
            }
            if (cartDto.CartHeader != null)
            {

                if (!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                {
                    var coupon = await _couponService.GetCouponAsync<ResponseDto>(cartDto.CartHeader.CouponCode, accessToken);
                    if (coupon != null && coupon.IsSuccess)
                    {
                        var couponObj = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(coupon.Result));
                        if (couponObj != null)
                        {
                            cartDto.CartHeader.DiscountTotal = couponObj.Discount;
                        }

                    }
                }

                foreach (var detail in cartDto.CartDetails)
                {
                    cartDto.CartHeader.OrderTotal += (detail.Product.Price * detail.Count);
                }
                cartDto.CartHeader.OrderTotal -= cartDto.CartHeader.DiscountTotal;
            }
            return cartDto;
        }
    }
}
