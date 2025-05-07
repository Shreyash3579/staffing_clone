using System;
using System.Data;
using System.Threading.Tasks;

namespace Pipeline.API.Contracts.Helpers
{
    public interface IDapperContext : IDisposable
    {
        IDbConnection Connection { get; }
        IDbConnection CortexConnection { get; }
        int TimeoutPeriod { get; set; }
        Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
        Task<T> WithCortexConnection<T>(Func<IDbConnection, Task<T>> getData);
    }
}
