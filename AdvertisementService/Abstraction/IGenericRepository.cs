using AdvertisementService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);
        int? FindMax(Expression<Func<T, int?>> predicate);
        void Delete(int id);
        Task DeleteAsync(int id);
        Task<List<T>> GetAsync(Pagination pagination, Expression<Func<T, bool>> filter = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                    params Expression<Func<T, object>>[] includeProperties);
        IEnumerable<T> Get(Pagination pagination, Expression<Func<T, bool>> filter = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                    params Expression<Func<T, object>>[] includeProperties);

        T GetById(int id);
        T GetById(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeProperties);
        T SingleOrDefault(Expression<Func<T, bool>> predicate);
        void Post(T entity);
        Task PostAsync(T entity);
        void Put(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Remove(T entity);
    }
}
