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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AboutController : BaseCompressedApiController
    {
        /// <summary>
        /// Retrieves version information for the Colleague Web API.
        /// </summary>
        /// <returns>Version information.</returns>
        public async Task<IEnumerable<About>> GetAboutAsync()
        {
            var apiVersionCollection = new List<About>();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            About versionInfo = new About("Colleague Web Api", assemblyVersion.ToString(3));
            apiVersionCollection.Add(versionInfo);
            
            return apiVersionCollection;
        }
    }
}
