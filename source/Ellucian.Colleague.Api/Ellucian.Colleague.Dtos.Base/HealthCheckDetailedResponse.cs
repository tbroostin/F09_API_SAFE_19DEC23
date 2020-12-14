// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Detailed health check response
    /// </summary>
    public class HealthCheckDetailedResponse : HealthCheckResponseBase
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version of the application
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// UTC formatted date/time
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Array of validation operations performed for the health check
        /// </summary>
        public IEnumerable<HealthCheck> Checks { get; set; }
    }
}