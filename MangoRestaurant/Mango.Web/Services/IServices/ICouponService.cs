using Mango.Web.Models.Dtos;
using System.Threading.Tasks;

namespace Mango.Web.Services.IServices
{
    public interface ICouponService
    {
        Task<T> GetCouponAsync<T>(string couponCode, string token = null);
       
    }
}
