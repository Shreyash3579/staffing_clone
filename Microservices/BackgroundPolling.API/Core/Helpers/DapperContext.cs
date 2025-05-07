using BackgroundPolling.API.Contracts.Helpers;
using Microservices.Common;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Helpers
{
    [ExcludeFromCodeCoverage]
    public class DapperContext : IDapperContext
    {
        private IDbConnection _connection;
        private IDbConnection _basisConnection;
        private IDbConnection _pipelineConnection;
        private IDbConnection _analyticsConnection;
        protected string ConnectionString;
        protected string BasisConnectionString;
        protected string PipelineConnectionString;
        protected string AnalyticsConnectionString;

        public DapperContext()
        {
            ConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:StaffingDatabase").Decrypt();
            BasisConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:BasisDatabase").Decrypt();
            PipelineConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:PipelineDatabase").Decrypt();
            AnalyticsConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:StaffingAnalyticsDatabase").Decrypt();
        }
        public DapperContext(string connectionString)
        {            
            ConnectionString = connectionString.Decrypt();
        }

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(ConnectionString);
                }
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        public IDbConnection BasisConnection
        {
            get
            {
                if (_basisConnection == null)
                {
                    _basisConnection = new SqlConnection(BasisConnectionString);
                }
                if (_basisConnection.State != ConnectionState.Open)
                {
                    _basisConnection.Open();
                }
                return _basisConnection;
            }
        }

        public IDbConnection PipelineConnection
        {
            get
            {
                if (_pipelineConnection == null)
                {
                    _pipelineConnection = new SqlConnection(PipelineConnectionString);
                }
                if (_pipelineConnection.State != ConnectionState.Open)
                {
                    _pipelineConnection.Open();
                }
                return _pipelineConnection;
            }
        }
        

        public IDbConnection AnalyticsConnection
        {
            get
            {
                if (_analyticsConnection == null) 
                {
                    _analyticsConnection = new SqlConnection(AnalyticsConnectionString); 
                }
                if (_analyticsConnection.State != ConnectionState.Open)
                {
                    _analyticsConnection.Open();
                }
                return _analyticsConnection;
            }
        }

        public int TimeoutPeriod { get; set; }

        public async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                // SqlConnection's dispose method will close the connection for us
                using (var connection = new SqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " +
                    ex.Message ?? "", ex);
            }
        }

        public async Task<T> WithBasisConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                // SqlConnection's dispose method will close the connection for us
                using (var connection = new SqlConnection(BasisConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " +
                    ex.Message ?? "", ex);
            }
        }

        public async Task<T> WithPipelineConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                // SqlConnection's dispose method will close the connection for us
                using (var connection = new SqlConnection(PipelineConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " +
                    ex.Message ?? "", ex);
            }
        }
        public async Task<T> WithAnalyticsConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                // SqlConnection's dispose method will close the connection for us
                using (var connection = new SqlConnection(AnalyticsConnectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " +
                    ex.Message ?? "", ex);
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
            if (_basisConnection != null && _basisConnection.State == ConnectionState.Open)
                _basisConnection.Close();
            if (_analyticsConnection != null && _analyticsConnection.State == ConnectionState.Open)
                _analyticsConnection.Close();
        }
    }
}