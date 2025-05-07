using System;
using System.Data;
using System.Threading.Tasks;

namespace Basis.API.Contracts.Helpers
{
    public interface IDapperContext : IDisposable
    {
        IDbConnection Connection { get; }
        int TimeoutPeriod { get; set; }
        Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
    }
}
