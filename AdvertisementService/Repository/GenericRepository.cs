using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdvertisementService.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        internal AdvertisementContext _context;
        internal DbSet<T> dbSet;

        public GenericRepository(AdvertisementContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public void Delete(int id)
        {
            T entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }
        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return dbSet.SingleOrDefault(predicate);
        }
        public void Delete(T entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            return await dbSet.Where(predicate).ToListAsync();
        }

        public IEnumerable<T> Get(Pagination pagination, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;
            if (pagination == null)
            {
                pagination = new Pagination
                {
                    Total = 1
                };
            }
            else
            {
                pagination.Total = query.Count();
            }

            if (filter != null)
            {
                query = query.Where(filter).Skip((pagination.Offset - 1) * pagination.Limit).Take(pagination.Limit);
                pagination.Total = query.Count();
            }
            else
            {
                query = query.Skip((pagination.Offset - 1) * pagination.Limit).Take(pagination.Limit);

            }

            foreach (Expression<Func<T, object>> includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return query.ToList();
        }
        public async Task<List<T>> GetAsync(Pagination pagination, Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;
            if (pagination == null)
            {
                pagination = new Pagination();
                pagination.Total = 1;
            }
            else
            {
                pagination.Total = query.Count();
            }

            if (filter != null)
            {
                query = query.Where(filter).Skip((pagination.Offset - 1) * pagination.Limit).Take(pagination.Limit);
                pagination.Total = query.Count();
            }
            else
            {
                query = query.Skip((pagination.Offset - 1) * pagination.Limit).Take(pagination.Limit);

            }

            foreach (Expression<Func<T, object>> includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return await query.ToListAsync();
        }

        public T GetById(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return query.FirstOrDefault();
        }

        public T GetById(int id)
        {
            return dbSet.Find(id);
            //var res = dbSet.Find(id) as List<T>;
            //return res.FirstOrDefault();
        }

        public void Post(T entity)
        {
            dbSet.Add(entity);
        }
        public async Task PostAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Put(T entity)
        {
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
        public void Remove(T entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
        }

        public async Task DeleteAsync(int id)
        {
            T entityToDelete = await dbSet.FindAsync(id);
            Delete(entityToDelete);
        }
    }


}
