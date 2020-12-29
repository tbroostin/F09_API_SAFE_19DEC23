// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Health check
    /// </summary>
    public class HealthCheck
    {
        /// <summary>
        /// The validation check name
        /// </summary>
        public string Validation { get; set; }

        /// <summary>
        /// The result of the validation check
        /// </summary>
        public HealthCheckStatusType Status { get; set; }
    }
}