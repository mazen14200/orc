using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace orc.core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        T? GetById(string id);
        List<T>? GetAll();
        Task<List<T>?> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        List<T>? FindAll(Expression<Func<T, bool>> match);
        Task<List<T>?> FindAllAsync(Expression<Func<T, bool>> match);

        T Add(T entity);
        Task<T> AddAsync(T entity);
        T Update(T entity);
        IEnumerable<T> UpdateRange(IEnumerable<T> entities);
        bool Delete(T entity);
        bool DeleteRange(IEnumerable<T> entities);

        int Complete();
        Task<int> CompleteAsync();

    }
}
