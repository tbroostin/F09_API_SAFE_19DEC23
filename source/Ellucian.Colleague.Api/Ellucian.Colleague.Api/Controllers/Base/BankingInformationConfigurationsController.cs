/*Copyright 2015-2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Exposes methods to interact with Configuration objects for Colleague Self Service Banking Information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BankingInformationConfigurationsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IBankingInformationConfigurationService bankingInformationConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="bankingInformationConfigurationService"></param>
        public BankingInformationConfigurationsController(ILogger logger, IAdapterRegistry adapterRegistry, IBankingInformationConfigurationService bankingInformationConfigurationService)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.bankingInformationConfigurationService = bankingInformationConfigurationService;
        }

        /// <summary>
        /// Get the Configuration object for Colleague Self Service Banking Information
        /// </summary>
        /// <returns>Returns a single banking information configuration object</returns>
        [HttpGet]
        public async Task<BankingInformationConfiguration> GetAsync()
        {
            try
            {
                var bankingInformationConfiguration = await bankingInformationConfigurationService.GetBankingInformationConfigurationAsync();
                return bankingInformationConfiguration;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfe)
            {
                var message = "Banking information configuration record does not exist";
                logger.Error(knfe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred getting banking information configuration");
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}