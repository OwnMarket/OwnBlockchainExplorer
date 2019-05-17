using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Infrastructure.Data.EF;

namespace Own.BlockchainExplorer.Infrastructure.Data
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly OwnDb _db;

        public Repository(OwnDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public void Insert(T entity)
        {
            _db.Set<T>().Add(entity);
        }

        public void Insert(IEnumerable<T> entities)
        {
            _db.Set<T>().AddRange(entities);
        }

        public void Update(T entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            if (_db.Entry(entity).State == EntityState.Detached)
                _db.Entry(entity).State = EntityState.Unchanged;
            _db.Set<T>().Remove(entity);
        }

        public void Delete(IEnumerable<T> entities)
        {
            foreach(var entity in entities)
            {
                Delete(entity);
            }
        }

        public T GetByKey(params object[] keyValues)
        {
            return _db.Set<T>().Find(keyValues);
        }

        public bool Exists(Expression<Func<T, bool>> whereCondition)
        {
            return _db.Set<T>().Any(whereCondition);
        }

        #region GetCount

        public int GetCount()
        {
            return _db.Set<T>().Count();
        }

        public int GetCount(Expression<Func<T, bool>> whereCondition)
        {
            return _db.Set<T>().Count(whereCondition);
        }

        #endregion

        #region GetAll

        public IEnumerable<T> GetAll()
        {
            return _db.Set<T>().ToList();
        }

        public IEnumerable<T> GetAll<TInclude1>(
            Expression<Func<T, TInclude1>> includeProperty1)
        {
            return this.Get(null, includeProperty1);
        }

        public IEnumerable<T> GetAll<TInclude1, TInclude2>(
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2)
        {
            return this.Get(null, includeProperty1, includeProperty2);
        }

        public IEnumerable<T> GetAll<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3)
        {
            return this.Get(null, includeProperty1, includeProperty2, includeProperty3);
        }

        #endregion

        #region Get

        public IEnumerable<T> Get(
            Expression<Func<T, bool>> whereCondition)
        {
            return this.Get<object>(whereCondition, null);
        }

        public IEnumerable<T> Get<TInclude1>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1)
        {
            return this.Get<TInclude1, object>(whereCondition, includeProperty1, null);
        }

        public IEnumerable<T> Get<TInclude1, TInclude2>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2)
        {
            return this.Get<TInclude1, TInclude2, object>(whereCondition, includeProperty1, includeProperty2, null);
        }

        public IEnumerable<T> Get<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3)
        {
            return this.Get<TInclude1, TInclude2, TInclude3, object>(
                whereCondition, 
                includeProperty1, 
                includeProperty2, 
                includeProperty3, 
                null)
                .ToList();
        }

        public IEnumerable<T> Get<TInclude1, TInclude2, TInclude3, TInclude4>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3,
            Expression<Func<T, TInclude4>> includeProperty4)
        {
            return this.Get<TInclude1, TInclude2, TInclude3, TInclude4, object>(
                whereCondition,
                includeProperty1,
                includeProperty2,
                includeProperty3,
                includeProperty4,
                null)
                .ToList();
        }

        public IEnumerable<T> Get<TInclude1, TInclude2, TInclude3, TInclude4, TInclude5>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3,
            Expression<Func<T, TInclude4>> includeProperty4,
            Expression<Func<T, TInclude5>> includeProperty5)
        {
            return this.Get<TInclude1, TInclude2, TInclude3, TInclude4, TInclude5, object>(
                whereCondition,
                includeProperty1,
                includeProperty2,
                includeProperty3,
                includeProperty4,
                includeProperty5,
                null)
                .ToList();
        }

        public IEnumerable<T> Get<TInclude1, TInclude2, TInclude3, TInclude4, TInclude5, TInclude6>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3,
            Expression<Func<T, TInclude4>> includeProperty4,
            Expression<Func<T, TInclude5>> includeProperty5,
            Expression<Func<T, TInclude6>> includeProperty6)
        {
            return this.GetQuery(
                whereCondition, 
                includeProperty1, 
                includeProperty2, 
                includeProperty3, 
                includeProperty4, 
                includeProperty5, 
                includeProperty6)
                .ToList();
        }

        public IEnumerable<T> GetDeep<TInclude1, TInclude2>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, TInclude2>> includeProperty2)
        {
            return this.GetDeepQuery<TInclude1, TInclude2>(whereCondition, includeProperty1, includeProperty2);
        }

        public IEnumerable<T> GetDeep<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, ICollection<TInclude2>>> includeProperty2,
            Expression<Func<TInclude2, TInclude3>> includeProperty3)
        {
            return this.GetDeepQuery(whereCondition, includeProperty1, includeProperty2, includeProperty3)
                .ToList();
        }

        #endregion

        public IEnumerable<TOutput> GetAs<TOutput>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TOutput>> mapFunction)
        {
            return _db.Set<T>()
                .Where(whereCondition)
                .Select(mapFunction)
                .ToList();
        }

        public IEnumerable<TOutput> GetLastAs<TOutput>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TOutput>> mapFunction,
            int count)
        {
            return _db.Set<T>()
                .Where(whereCondition)
                .OrderByDescending(mapFunction)
                .Take(count)
                .Select(mapFunction)
                .ToList();
        }

        public IEnumerable<TOutput> GetLastAs<TOutput>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TOutput>> orderFunction,
            Expression<Func<T, TOutput>> mapFunction,
            int takeCount,
            int skipCount)
        {
            return _db.Set<T>()
                .Where(whereCondition)
                .OrderByDescending(orderFunction)
                .Skip(skipCount)
                .Take(takeCount)
                .Select(mapFunction)
                .ToList();
        }

        // Breadth traversal of navigation properties
        private IQueryable<T> GetQuery<TInclude1, TInclude2, TInclude3, TInclude4, TInclude5, TInclude6>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3,
            Expression<Func<T, TInclude4>> includeProperty4,
            Expression<Func<T, TInclude5>> includeProperty5,
            Expression<Func<T, TInclude6>> includeProperty6)
        {
            var query = _db.Set<T>().AsQueryable();

            if (includeProperty1 != null)
                query = query.Include(includeProperty1);
            if (includeProperty2 != null)
                query = query.Include(includeProperty2);
            if (includeProperty3 != null)
                query = query.Include(includeProperty3);
            if (includeProperty4 != null)
                query = query.Include(includeProperty4);
            if (includeProperty5 != null)
                query = query.Include(includeProperty5);
            if (includeProperty6 != null)
                query = query.Include(includeProperty6);

            if (whereCondition != null)
                query = query.Where(whereCondition);

            return query;
        }

        // Depth traversal of navigation properties (2 levels)
        private IQueryable<T> GetDeepQuery<TInclude1, TInclude2>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, TInclude2>> includeProperty2)
        {
            var query = _db.Set<T>().AsQueryable();

            if (includeProperty1 != null)
                query = query.Include(includeProperty1);
            if (includeProperty2 != null)
                query = (query as IIncludableQueryable<T, ICollection<TInclude1>>).ThenInclude(includeProperty2);

            if (whereCondition != null)
                query = query.Where(whereCondition);

            return query;
        }

        // Depth traversal of navigation properties (3 levels)
        private IQueryable<T> GetDeepQuery<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, ICollection<TInclude2>>> includeProperty2,
            Expression<Func<TInclude2, TInclude3>> includeProperty3)
        {
            var query = _db.Set<T>().AsQueryable();

            if (includeProperty1 != null)
                query = query.Include(includeProperty1);
            if (includeProperty2 != null)
                query = (query as IIncludableQueryable<T, ICollection<TInclude1>>).ThenInclude(includeProperty2);
            if (includeProperty3 != null)
                query = (query as IIncludableQueryable<T, ICollection<TInclude2>>).ThenInclude(includeProperty3);

            if (whereCondition != null)
                query = query.Where(whereCondition);

            return query;
        }
    }
}
