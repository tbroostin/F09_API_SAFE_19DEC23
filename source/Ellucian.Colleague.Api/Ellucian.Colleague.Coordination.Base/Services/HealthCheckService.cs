// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service to check the health of this application
    /// </summary>
    [RegisterType]
    public class HealthCheckService : BaseCoordinationService, IHealthCheckService
    {
        private readonly ColleagueSettings _colleagueSettings;

        /// <summary>
        /// Constructor for HealthCheckService
        /// </summary>
        /// <param name="adapterRegistry">Adaper registry</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="roleRepository">The role repository</param>
        /// <param name="logger">The logger</param>
        /// <param name="colleagueSettings">Colleague settings</param>
        public HealthCheckService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger, ColleagueSettings colleagueSettings)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _colleagueSettings = colleagueSettings;
        }

        /// <summary>
        /// Perform a detailed health check
        /// </summary>
        /// <returns>Detailed health check response</returns>
        public async Task<HealthCheckDetailedResponse> PerformDetailedHealthCheckAsync()
        {
            // test the app and das (if configured) listener connections to the Colleague environment
            var connectionService = new ColleagueConnectionTestService(logger);
            var connectionResult = await connectionService.TestColleagueConnectionsAsync(_colleagueSettings);

            // create the response
            var response = new HealthCheckDetailedResponse()
            {
                Name = "Colleague Web API",
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // add the health check(s) to the response
            StringBuilder healthCheckErrorMessage = null;
            var healthChecks = new List<HealthCheck>();
            foreach (var check in connectionResult.Checks)
            {
                // convert to the DTO check
                healthChecks.Add(new HealthCheck()
                {
                    Status = ConvertStatusToDto(check.Status),
                    Validation = check.Validation
                });

                // build up error messages to log
                if (!string.IsNullOrEmpty(check.ErrorMessage))
                {
                    if (healthCheckErrorMessage == null )
                        healthCheckErrorMessage = new StringBuilder("Error(s) occurred during the detailed health check:" + Environment.NewLine);
                    if (!string.IsNullOrEmpty(check.ErrorMessage))
                    {
                        healthCheckErrorMessage.AppendLine(string.Format("\t {0} health check returned: {1}", check.Validation, check.ErrorMessage));
                    }
                }
            }
            response.Checks = healthChecks;

            // set the overall status
            if (healthCheckErrorMessage == null)
            {
                response.Status = HealthCheckStatusType.Available;
            }
            else
            {
                response.Status = HealthCheckStatusType.Unavailable;
                logger.Error(healthCheckErrorMessage.ToString());
            }

            return response;
        }

        // convert the status string to health status enum
        private HealthCheckStatusType ConvertStatusToDto(ConnectionTestStatusType status)
        {
            switch(status)
            {
                case ConnectionTestStatusType.Available:
                    return HealthCheckStatusType.Available;
                default:
                    return HealthCheckStatusType.Unavailable;
            }
        }
    }
}