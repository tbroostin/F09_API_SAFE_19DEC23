/*Copyright 2019-2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to benefits enrollment configuration items
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BenefitsEnrollmentConfigurationController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IBenefitsEnrollmentConfigurationService benefitsEnrollmentConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        public BenefitsEnrollmentConfigurationController(IBenefitsEnrollmentConfigurationService benefitsEnrollmentConfigurationService, ILogger logger)
        {
            this.benefitsEnrollmentConfigurationService = benefitsEnrollmentConfigurationService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the configurations for benefits enrollment
        /// </summary>
        /// <returns>BenefitsEnrollmentConfiguration</returns>
        /// <accessComments>Any authenticated user can get this resource</accessComments>
        [HttpGet]
        public async Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync()
        {
            try
            {
                return await benefitsEnrollmentConfigurationService.GetBenefitsEnrollmentConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.InternalServerError);
            }
        }
    }
}