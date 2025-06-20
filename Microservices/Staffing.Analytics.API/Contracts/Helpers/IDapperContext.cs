﻿using System;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Helpers
{
    public interface IDapperContext : IDisposable
    {
        IDbConnection Connection { get; }
        int TimeoutPeriod { get; set; }
        Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData);
    }
}
