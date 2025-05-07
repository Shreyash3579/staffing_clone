using Microservices.Common;
using Staffing.API.Contracts.Helpers;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Staffing.API.Core.Helpers
{
    [ExcludeFromCodeCoverage]
    public class DapperContext : IDapperContext
    {
        private IDbConnection _connection;
        protected string ConnectionString;

        public DapperContext()
        {
            ConnectionString = ConfigurationUtility.GetValue("ConnectionStrings:StaffingDatabase").Decrypt();
        }

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null) _connection = new SqlConnection(ConnectionString);
                if (_connection.State != ConnectionState.Open) _connection.Open();
                return _connection;
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

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}