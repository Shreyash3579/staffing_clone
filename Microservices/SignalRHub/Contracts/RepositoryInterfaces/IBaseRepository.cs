using SignalRHub.Contracts.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SignalRHub.Contracts.RepositoryInterfaces
{
    public interface IBaseRepository<T> where T : class
    {
        IDapperContext Context { set; get; }

        #region Upserts

        Task<T> UpsertAsync(string storedProcedure, object dynamicParameters);

        #endregion

        #region Gets

        // Single object
        T GetById(Guid id, string storedProcedure);
        T GetById(int id, string storedProcedure);
        T GetById(string id, string storedProcedure);
        T GetByDynamic(object dynamicParameters, string storedProcedure);
        Task<T> GetByIdAsync(string id, string storedProcedure);
        Task<T> GetByDynamicAsync(object dynamicParameters, string storedProcedure);

        // Lists
        IEnumerable<T> GetAll(string storedProcedure);
        IEnumerable<T> GetAll(object dynamicParameters, string storedProcedure);
        Task<IEnumerable<T>> GetAllAsync(string storedProcedure);
        Task<IEnumerable<T>> GetAllAsync(object dynamicParameters, string storedProcedure);

        #endregion

        #region Inserts

        T Insert(T entity, string storedProcedure);
        T Insert(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction);
        T Insert(T entity, string storedProcedure, object dynamicParameters);
        IEnumerable<T> Insert(object dynamicParameters, string storedProcedure);

        T Insert(IDbConnection connection, T entity, string storedProcedure, object dynamicParameters,
            IDbTransaction transaction);

        void Insert(string storedProcedure, object dynamicParameters);
        Task InsertAsync(string storedProcedure, object dynamicParameters);
        Task<T> InsertAsync(object dynamicParameters, string storedProcedure);

        #endregion

        #region Updates

        T Update(T entity, string storedProcedure);
        T Update(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction);
        T Update(T entity, string storedProcedure, object dynamicParameters);
        IEnumerable<T> Update(object dynamicParameters, string storedProcedure);

        T Update(IDbConnection connection, T entity, string storedProcedure, object dynamicParameters,
            IDbTransaction transaction);

        void Update(string storedProcedure, object dynamicParameters);
        Task UpdateAsync(string storedProcedure, object dynamicParameters);
        Task<T> UpdateAsync(object dynamicParameters, string storedProcedure);

        #endregion

        #region Deletes

        void Delete(T entity, string storedProcedure);
        void Delete(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction);
        void Delete(Guid id, string storedProcedure);
        void Delete(string storedProcedure, object dynamicParameters);
        Task DeleteAsync(string storedProcedure, object dynamicParameters);
        IEnumerable<T> Delete(object dynamicParameters, string storedProcedure);
        void Delete(IDbConnection connection, Guid id, string storedProcedure, IDbTransaction transaction);

        void Delete(IDbConnection connection, object dynamicParameters, string storedProcedure,
            IDbTransaction transaction);

        Task<T> DeleteAsync(object dynamicParameters, string storedProcedure);

        #endregion
    }
}