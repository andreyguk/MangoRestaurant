using AutoMapper;
using Mango.Services.ProductAPI.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mango.Services.ProductAPI.Repository
{
    public class Repository<TEntity, TDto> : IRepository<TEntity, TDto> where TEntity : class, new() where TDto : class
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly DbSet<TEntity> _dbSet;
        public Repository(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _dbContext = applicationDbContext;
            _mapper = mapper;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public async Task<TDto> Create(TDto entityDto)
        {
            try
            {
                var entity = _mapper.Map<TDto, TEntity>(entityDto);
                _dbSet.Add(entity);
                await _dbContext.SaveChangesAsync();
                return _mapper.Map<TEntity, TDto>(entity);
            }
            catch (Exception)
            {

                return entityDto;
            }
        }

        public async Task<TDto> Update(TDto entityDto)
        {
            try
            {
                var entity = _mapper.Map<TDto, TEntity>(entityDto);
                _dbContext.Entry(entity).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return _mapper.Map<TEntity, TDto>(entity);
            }
            catch (Exception)
            {

                return entityDto;
            }
        }

        public async Task<bool> Delete(int entityId)
        {
            try
            {
                var entity = await _dbSet.FindAsync(entityId);
                if (entity == null)
                {
                    return false;
                }
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<TDto> GetAsync(int entityId)
        {
            var entity = await _dbSet.FindAsync(entityId);
            return _mapper.Map<TDto>(entity);
        }

        public async Task<IEnumerable<TDto>> GetAsync()
        {
            var entities = await _dbSet.ToArrayAsync();
            return _mapper.Map<List<TDto>>(entities);
        }

    }
}
