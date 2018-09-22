/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
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
    /// Exposes student-specific PROFILE Application data 
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ProfileApplicationsController : BaseCompressedApiController
    {
        private readonly IProfileApplicationService profileApplicationService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for ProfileApplicationsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="profileApplicationService">ProfileApplicationService</param>
        /// <param name="logger">Logger</param>
        public ProfileApplicationsController(
            IAdapterRegistry adapterRegistry,
            IProfileApplicationService profileApplicationService,
            ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.profileApplicationService = profileApplicationService;
            this.logger = logger;
        }

        /// <summary>
        /// Get a student's profile applications for all award years
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">Colleague PERSON id of the student for whom to get ProfileApplications</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of the given student's profile applications for all award years</returns>
        [HttpGet]
        public async Task<IEnumerable<ProfileApplication>> GetProfileApplicationsAsync([FromUri]string studentId, [FromUri]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId is required in Uri");
            }

            try
            {
                return await profileApplicationService.GetProfileApplicationsAsync(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("User does not have access rights to student {0}", studentId);
                logger.Error(pex, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred getting profile applications resources");
                throw CreateHttpResponseException("Unknown error occurred getting profile applications resources");
            }
        }
    }
}