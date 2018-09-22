// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Reflection;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific status information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class HealthCheckController : BaseCompressedApiController
    {
        /// <summary>
        /// Retrieves status information for the Colleague Web API.
        /// </summary>
        /// <returns>Status information.</returns>
        public async Task<IEnumerable<HealthCheck>> GetHealthCheckAsync()
        {
            var apiStatusCollection = new List<HealthCheck>();
            HealthCheck statusInfo = new HealthCheck(HealthCheckType.Available);
            apiStatusCollection.Add(statusInfo);

            return apiStatusCollection;
        }
    }
}
