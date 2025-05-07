using System;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Helpers
{
    public interface IDapperContext : IDisposable
    {
        IDbConnection Connection { get; }
        IDbConnection BasisConnection { get; }
        IDbConnection PipelineConnection { get; }
        IDbConnection AnalyticsConnection { get; }
        int TimeoutPeriod { get; set; }
        Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
    }
}
