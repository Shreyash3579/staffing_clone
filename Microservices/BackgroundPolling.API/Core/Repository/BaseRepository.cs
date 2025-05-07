using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    [ExcludeFromCodeCoverage]
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private IDapperContext _context;

        public BaseRepository(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Connection = Context.Connection;
        }

        private IDbConnection Connection { get; }

        public IDapperContext Context
        {
            get
            {
                if (_context == null) throw new Exception("context has not been set!");

                return _context;
            }

            set => _context = value;
        }

        #region Upserts

        public async Task<T> UpsertAsync(string storedProcedure, object dynamicParameters)
        {
            return
                await
                    Context.WithConnection(async c =>
                        await
                            c.QueryFirstOrDefaultAsync<T>(
                                storedProcedure,
                                dynamicParameters,
                                commandType: CommandType.StoredProcedure,
                                commandTimeout: Context.TimeoutPeriod
                            )
                    );
        }

        #endregion

        #region Gets

        public async Task<T> GetByIdAsync(string id, string storedProcedure)
        {
            return await Context.WithConnection(async c =>
                await c.QueryFirstAsync<T>(
                    storedProcedure,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod
                )
            );
        }

        public async Task<T> GetByDynamicAsync(object dynamicParameters, string storedProcedure)
        {
            return await Context.WithConnection(async c =>
                await c.QueryFirstOrDefaultAsync<T>(
                    storedProcedure,
                    dynamicParameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod
                )
            );
        }

        public IEnumerable<T> GetAll(string storedProcedure)
        {
            IEnumerable<T> result;

            using (var localContext = Context)
            {
                result = localContext.Connection.Query<T>(storedProcedure, new { Id = (Guid?)null },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod);
            }

            return result;
        }

        public IEnumerable<T> GetAll(object dynamicParameters, string storedProcedure)
        {
            IEnumerable<T> result;

            using (var localContext = Context)
            {
                result = localContext.Connection.Query<T>(storedProcedure, dynamicParameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod);
            }

            return result;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string storedProcedure)
        {
            return await Context.WithConnection(async c =>
                await c.QueryAsync<T>(
                    storedProcedure,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod
                )
            );
        }

        public async Task<IEnumerable<T>> GetAllAsync(object dynamicParameters, string storedProcedure)
        {
            return await Context.WithConnection(async c =>
                await c.QueryAsync<T>(
                    storedProcedure,
                    dynamicParameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod
                )
            );
        }

        public T GetById(Guid id, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, new { Id = id },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        public T GetById(int id, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, new { Id = id },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        public T GetById(string id, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, new { Id = id },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        public T GetByDynamic(object dynamicParameters, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, dynamicParameters,
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        protected IEnumerable<T> GetAllDynamic(string storedProcedure)
        {
            List<T> results;

            using (var localContext = Context)
            {
                results = localContext.Connection.Query<T>(storedProcedure,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod).ToList();
            }

            return results;
        }

        #endregion

        #region Inserts

        /// <summary>
        ///     Generic insert
        /// </summary>
        public T Insert(T entity, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, entity, commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        /// <summary>
        ///     Generic insert with dynamic parameters
        /// </summary>
        public T Insert(T entity, string storedProcedure, object dynamicParameters)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, dynamicParameters,
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        /// <summary>
        ///     Generic insert with dynamic parameters
        /// </summary>
        /// <returns>A list of the inserted objects</returns>
        public IEnumerable<T> Insert(object dynamicParameters, string storedProcedure)
        {
            IEnumerable<T> results;

            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    results = Connection.Query<T>(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: Context.TimeoutPeriod);

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }

            return results;
        }

        /// <summary>
        ///     Generic insert with dynamic parameters which does not return data
        /// </summary>
        public void Insert(string storedProcedure, object dynamicParameters)
        {
            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    Context.Connection.Execute(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: Context.TimeoutPeriod);

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }
        }

        /// <summary>
        ///     Async insert with dynamic parameters which does not return data
        /// </summary>
        public async Task InsertAsync(string storedProcedure, object dynamicParameters)
        {
            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    await Task.Run(() => scope.Connection.Execute(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: scope.Connection.ConnectionTimeout));
                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }
        }

        /// <summary>
        ///     Async insert with dynamic parameters which return inserted object
        /// </summary>
        public async Task<T> InsertAsync(object dynamicParameters, string storedProcedure)
        {
            return
                await
                    Context.WithConnection(async c =>
                        await
                            c.QueryFirstOrDefaultAsync<T>(
                                storedProcedure,
                                dynamicParameters,
                                commandType: CommandType.StoredProcedure,
                                commandTimeout: Context.TimeoutPeriod
                            )
                    );
        }

        /// <summary>
        ///     Generic insert in a scope of a transaction
        /// </summary>
        public T Insert(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction)
        {
            return
                connection.Query<T>(
                        storedProcedure, entity,
                        commandType: CommandType.StoredProcedure,
                        transaction: transaction,
                        commandTimeout: Context.TimeoutPeriod)
                    .SingleOrDefault();
        }

        /// <summary>
        ///     Generic insert with dynamic parameters in the scope of a transaction
        /// </summary>
        public T Insert(IDbConnection connection, T entity, string storedProcedure, object dynamicParameters,
            IDbTransaction transaction)
        {
            return
                connection.Query<T>(storedProcedure, dynamicParameters, commandType: CommandType.StoredProcedure,
                    transaction: transaction, commandTimeout: Context.TimeoutPeriod).SingleOrDefault();
        }

        #endregion

        #region Updates

        /// <summary>
        ///     Generic update
        /// </summary>
        public T Update(T entity, string storedProcedure)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, entity, commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        /// <summary>
        ///     Generic update in the scope of a transaction
        /// </summary>
        public T Update(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction)
        {
            return
                connection.Query<T>(storedProcedure, entity, commandType: CommandType.StoredProcedure,
                        transaction: transaction, commandTimeout: Context.TimeoutPeriod)
                    .SingleOrDefault();
        }

        /// <summary>
        ///     Generic update with dynamic parameters
        /// </summary>
        public T Update(T entity, string storedProcedure, object dynamicParameters)
        {
            T results;

            using (var localContext = Context)
            {
                results =
                    localContext.Connection.Query<T>(storedProcedure, dynamicParameters,
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: Context.TimeoutPeriod)
                        .SingleOrDefault();
            }

            return results;
        }

        /// <summary>
        ///     Generic update with dynamic parameters
        /// </summary>
        /// <returns>A list of the updated objects</returns>
        public IEnumerable<T> Update(object dynamicParameters, string storedProcedure)
        {
            IEnumerable<T> results;

            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    results = Connection.Query<T>(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope, commandTimeout: Context.TimeoutPeriod);

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }

            return results;
        }

        /// <summary>
        ///     Generic update with dynamic parameters in the scope of a transaction
        /// </summary>
        public T Update(IDbConnection connection, T entity, string storedProcedure, object dynamicParameters,
            IDbTransaction transaction)
        {
            return
                connection.Query<T>(storedProcedure, dynamicParameters, commandType: CommandType.StoredProcedure,
                    transaction: transaction, commandTimeout: Context.TimeoutPeriod).SingleOrDefault();
        }

        /// <summary>
        ///     Generic Update with dynamic parameters which does not return data
        /// </summary>
        public void Update(string storedProcedure, object dynamicParameters)
        {
            using (var localContext = Context)
            {
                localContext.Connection.Execute(storedProcedure, dynamicParameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod);
            }
        }

        /// <summary>
        ///     Async Update with dynamic parameters which does not return data
        /// </summary>
        public async Task UpdateAsync(string storedProcedure, object dynamicParameters)
        {
            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    await Task.Run(() => scope.Connection.Execute(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: scope.Connection.ConnectionTimeout));

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }
        }

        /// <summary>
        ///     Async update with dynamic parameters which return updated object
        /// </summary>
        public async Task<T> UpdateAsync(object dynamicParameters, string storedProcedure)
        {
            return
                await
                    Context.WithConnection(async c =>
                        await
                            c.QueryFirstOrDefaultAsync<T>(
                                storedProcedure,
                                dynamicParameters,
                                commandType: CommandType.StoredProcedure,
                                commandTimeout: Context.TimeoutPeriod
                            )
                    );
        }

        #endregion

        #region Deletes

        /// <summary>
        ///     Generic delete
        /// </summary>
        public void Delete(T entity, string storedProcedure)
        {
            using (var localContext = Context)
            {
                localContext.Connection.Execute(storedProcedure, entity, commandTimeout: Context.TimeoutPeriod);
            }
        }

        /// <summary>
        ///     Generic delete in the scope of a transaction
        /// </summary>
        public void Delete(IDbConnection connection, T entity, string storedProcedure, IDbTransaction transaction)
        {
            connection.Execute(storedProcedure, entity, transaction, Context.TimeoutPeriod);
        }

        /// <summary>
        ///     Generic delete with uniqueidentifier as the id
        /// </summary>
        public void Delete(Guid id, string storedProcedure)
        {
            using (var localContext = Context)
            {
                localContext.Connection.Execute(storedProcedure, new { Id = id },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: Context.TimeoutPeriod);
            }
        }

        /// <summary>
        ///     Generic delete with dynamic parameters
        /// </summary>
        public void Delete(string storedProcedure, object dynamicParameters)
        {
            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    scope.Connection.Execute(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: Context.TimeoutPeriod);

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }
        }

        /// <summary>
        ///     Async Delete with dynamic parameters which does not return data
        /// </summary>
        public async Task DeleteAsync(string storedProcedure, object dynamicParameters)
        {
            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    await Task.Run(() => scope.Connection.Execute(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: scope.Connection.ConnectionTimeout));

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }
        }

        /// <summary>
        ///     Generic delete with dynamic parameters
        /// </summary>
        /// <returns>A list of the deleted objects</returns>
        public IEnumerable<T> Delete(object dynamicParameters, string storedProcedure)
        {
            IEnumerable<T> results;

            var scope = Context.Connection.BeginTransaction();

            try
            {
                using (scope)
                {
                    results = Connection.Query<T>(
                        storedProcedure,
                        dynamicParameters,
                        commandType: CommandType.StoredProcedure,
                        transaction: scope,
                        commandTimeout: Context.TimeoutPeriod);

                    scope.Commit();
                }
            }
            catch (Exception)
            {
                scope.Rollback();

                throw;
            }
            finally
            {
                Context.Dispose();
            }

            return results;
        }

        /// <summary>
        ///     Generic delete with uniqueidentifier as the id, in the scope of a transaction
        /// </summary>
        public void Delete(IDbConnection connection, Guid id, string storedProcedure, IDbTransaction transaction)
        {
            connection.Execute(storedProcedure, new { Id = id }, commandType: CommandType.StoredProcedure,
                transaction: transaction, commandTimeout: Context.TimeoutPeriod);
        }

        /// <summary>
        ///     Generic delete with dynamic parameters in the scope of a transaction
        /// </summary>
        public void Delete(IDbConnection connection, object dynamicParameters, string storedProcedure,
            IDbTransaction transaction)
        {
            connection.Execute(storedProcedure, dynamicParameters, commandType: CommandType.StoredProcedure,
                transaction: transaction, commandTimeout: Context.TimeoutPeriod);
        }

        /// <summary>
        ///     Async Delete with dynamic parameters which return deleted object
        /// </summary>
        public async Task<T> DeleteAsync(object dynamicParameters, string storedProcedure)
        {
            return
                await
                    Context.WithConnection(async c =>
                        await
                            c.QueryFirstOrDefaultAsync<T>(
                                storedProcedure,
                                dynamicParameters,
                                commandType: CommandType.StoredProcedure,
                                commandTimeout: Context.TimeoutPeriod
                            )
                    );
        }

        #endregion
    }
}