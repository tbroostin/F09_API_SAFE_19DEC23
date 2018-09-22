/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes the AverageAwardPackage data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AverageAwardPackagesController : BaseCompressedApiController
    {
        private readonly IAverageAwardPackageService AverageAwardPackageService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Constructor for the AverageAwardPackageController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="averageAwardPackageService">averageAwardPackageService</param>
        /// <param name="logger">Logger</param>
        public AverageAwardPackagesController(IAdapterRegistry adapterRegistry, IAverageAwardPackageService averageAwardPackageService, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            AverageAwardPackageService = averageAwardPackageService;
            Logger = logger;
        }

        /// <summary>
        /// Get the list of award package averages for the predefined award categories.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The studentId for which to get the award package data</param>
        /// <returns>A list of average award packages that apply to the student</returns>
        public async Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null");
            }
            try
            {
                return await AverageAwardPackageService.GetAverageAwardPackagesAsync(studentId);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to AverageAwardPackages resource forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find AverageAwardPackages resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting AverageAwardPackages. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

    }
}