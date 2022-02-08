using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mango.Services.ProductAPI.Repository
{
    public interface IRepository<TEntity, TDto> where TEntity : class, new() where TDto : class
    {
        Task<IEnumerable<TDto>> GetAsync();
        Task<TDto> GetAsync(int entityId);       
        Task<TDto> Create(TDto entity);
        Task<TDto> Update(TDto entity);
        Task<bool> Delete(int entityId);


    }
}
