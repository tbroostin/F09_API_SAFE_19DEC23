// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Newtonsoft.Json;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides specific status information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class HealthController : BaseCompressedApiController
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly ILogger _logger;
        private readonly ApiSettings _apiSettings;

        /// <summary>
        /// Initializes a new instance of the HealthCheckController class.
        /// </summary>
        /// <param name="healthCheckService">Service of type <see cref="IHealthCheckService">IHealthCheckService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings">API settings</param>
        public HealthController(IHealthCheckService healthCheckService, ILogger logger, ApiSettings apiSettings)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
            _apiSettings = apiSettings;
        }

        /// <summary>
        /// Retrieves status information for the Colleague Web API.
        /// </summary>
        /// <returns>Status information.</returns>
        public async Task<object> GetApplicationHealthCheckAsync([FromUri(Name = "level")] string level = null)
        {
            if (level == "detailed")
            {
                if (_apiSettings != null && _apiSettings.DetailedHealthCheckApiEnabled)
                {
                    // detailed health check meant for reporting/monitoring without automated actions.  Check the
                    // application's connections to its dependencies
                    try
                    {
                        var healthCheckResult = await _healthCheckService.PerformDetailedHealthCheckAsync();
                        if (healthCheckResult.Status == Dtos.Base.HealthCheckStatusType.Unavailable)
                        {
                            // an error occurred during the health check, change the status code but return the JSON still
                            var response = new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.ServiceUnavailable,
                                Content = new StringContent(JsonConvert.SerializeObject(healthCheckResult))
                            };
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            return response;
                        }
                        else
                        {
                            return healthCheckResult;
                        }
                    }
                    catch (Exception e)
                    {
                        string error = "Exception occurred during detailed health check";
                        _logger.Error(e, error);
                        throw CreateHttpResponseException(error, HttpStatusCode.ServiceUnavailable);
                    }
                }
                else
                {
                    // detailed health check not enabled
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Forbidden
                    };
                }
            }
            else if (level == null)
            {
                // basic health check meant for load balancers.  Return available to confirm the application
                // is running and accessible
                return new Dtos.Base.HealthCheckResponse()
                {
                    Status = Dtos.Base.HealthCheckStatusType.Available
                };
            }
            else
            {
                throw CreateHttpResponseException("Invalid health check level");
            }
        }
    }
}