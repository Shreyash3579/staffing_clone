using Microservices.Common;
using Pipeline.API.Contracts.Helpers;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Pipeline.API.Core.Helpers
{
    [ExcludeFromCodeCoverage]
    public class DapperContext : IDapperContext
    {
        private IDbConnection _connection;
        private IDbConnection _cortexConnection;
        protected string ConnectionString;
        protected string CortexConnectionString;

        public DapperContext()
        {
            ConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:PipelineDatabase").Decrypt();
            CortexConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:CortexDatabase").Decrypt();
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

        public IDbConnection CortexConnection
        {
            get
            {
                if (_cortexConnection == null)
                {
                    _cortexConnection = new SqlConnection(CortexConnectionString);
                }

                if (_cortexConnection.State != ConnectionState.Open)
                { 
                    _cortexConnection.Open(); 
                }

                return _cortexConnection;
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
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " + ex.Message ?? "", ex);
            }
        }


        public async Task<T> WithCortexConnection<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                // SqlConnection's dispose method will close the connection for us
                using (var connection = new SqlConnection(CortexConnectionString))
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
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout). " + ex.Message ?? "", ex);
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }

            if (_cortexConnection != null && _cortexConnection.State == ConnectionState.Open)
            {
                _cortexConnection.Close();
            }
        }
    }
}
