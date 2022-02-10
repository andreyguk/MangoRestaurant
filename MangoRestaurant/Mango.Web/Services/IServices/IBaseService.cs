using Mango.Web.Models;
using Mango.Web.Models.Dtos;
using System;
using System.Threading.Tasks;

namespace Mango.Web.Services.IServices
{
    public interface IBaseService : IDisposable
    {
        ResponseDto responseModel { get; set; }
        Task<T> SendAsync<T>(ApiRequest apiRequest);
    }
}
