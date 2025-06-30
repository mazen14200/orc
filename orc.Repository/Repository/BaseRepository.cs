using Microsoft.EntityFrameworkCore;
using orc.core.Interfaces;
using orc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace orc.Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected ApplicationDbContext _dbContext;
        public BaseRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public T? GetById(string id)
        {
            return _dbContext.Set<T>().Find(id);
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public List<T>? GetAll()
        {
            return _dbContext.Set<T>().ToList<T>();
        }
        public async Task<List<T>?> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync<T>();

        }


        public List<T>? FindAll(Expression<Func<T, bool>> match)
        {
            return _dbContext.Set<T>().Where(match).ToList<T>();
        }
        public async Task<List<T>?> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().Where(match).ToListAsync<T>();
        }

        public T Add(T entity)
        {
            try
            {
                _dbContext.Set<T>().Add(entity);
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                await _dbContext.Set<T>().AddAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public T Update(T entity)
        {
            try
            {
                _dbContext.Set<T>().Update(entity);
                return entity;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            _dbContext.UpdateRange(entities);
            return entities;
        }

        public bool Delete(T entity)
        {
            try
            {
                _dbContext.Set<T>().Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteRange(IEnumerable<T> entities)
        {
            try
            {
                _dbContext.Set<T>().RemoveRange(entities);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int Complete()
        {
            return _dbContext.SaveChanges();
        }

        public Task<int> CompleteAsync()
        {
            return _dbContext.SaveChangesAsync();
        }


    }
}
