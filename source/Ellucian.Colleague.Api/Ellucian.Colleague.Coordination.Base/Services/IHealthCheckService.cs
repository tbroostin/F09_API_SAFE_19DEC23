// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for a health check service
    /// </summary>
    public interface IHealthCheckService
    {
        /// <summary>
        /// Perform a detailed health check
        /// </summary>
        /// <returns>Detailed health check response</returns>
        Task<HealthCheckDetailedResponse> PerformDetailedHealthCheckAsync();
    }
}