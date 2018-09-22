// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Reflection;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class VersionController : BaseCompressedApiController
    {
        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        public ApiVersion Get()
        {
            ApiVersion versionInfo = new ApiVersion();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            versionInfo.ProductVersion = assemblyVersion.ToString(3);

            return versionInfo;
        }
    }
}
