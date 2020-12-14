// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Base health check response
    /// </summary>
    public class HealthCheckResponseBase
    {
        /// <summary>
        /// Overall status of this application
        /// </summary>
        public HealthCheckStatusType Status { get; set; }
    }
}