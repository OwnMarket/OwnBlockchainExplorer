using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IRepository<T>
        where T : class
    {
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);

        T GetByKey(params object[] keyValues);

        bool Exists(Expression<Func<T, bool>> whereCondition);

        #region GetCount

        int GetCount();
        int GetCount(Expression<Func<T, bool>> whereCondition);

        #endregion

        #region GetAll

        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll<TInclude1>(
            Expression<Func<T, TInclude1>> includeProperty1);

        IEnumerable<T> GetAll<TInclude1, TInclude2>(
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2);

        IEnumerable<T> GetAll<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3);

        #endregion

        #region Get

        IEnumerable<T> Get(
            Expression<Func<T, bool>> whereCondition);

        IEnumerable<T> Get<TInclude1>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1);

        IEnumerable<T> Get<TInclude1, TInclude2>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2);

        IEnumerable<T> Get<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TInclude1>> includeProperty1,
            Expression<Func<T, TInclude2>> includeProperty2,
            Expression<Func<T, TInclude3>> includeProperty3);

        IEnumerable<T> GetDeep<TInclude1, TInclude2>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, TInclude2>> includeProperty2);

        IEnumerable<T> GetDeep<TInclude1, TInclude2, TInclude3>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, ICollection<TInclude1>>> includeProperty1,
            Expression<Func<TInclude1, ICollection<TInclude2>>> includeProperty2,
            Expression<Func<TInclude2, TInclude3>> includeProperty3);

        #endregion

        IEnumerable<TOutput> GetAs<TOutput>(
            Expression<Func<T, bool>> whereCondition,
            Expression<Func<T, TOutput>> mapFunction);
    }
}
