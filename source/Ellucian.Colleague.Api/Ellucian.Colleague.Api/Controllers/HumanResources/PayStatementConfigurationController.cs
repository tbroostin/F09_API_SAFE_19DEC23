// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Pay Statement Configuration Controller routes requests for configurations for pay statements
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PayStatementConfigurationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IPayStatementConfigurationService payStatementConfigurationService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        /// <param name="payStatementConfigurationService"></param>
        public PayStatementConfigurationController(IAdapterRegistry adapterRegistry, ILogger logger, IPayStatementConfigurationService payStatementConfigurationService)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.payStatementConfigurationService = payStatementConfigurationService;
        }

        /// <summary>
        /// Get the pay statement configuration for the environment.
        /// 
        /// A successful request will return a status code of 200 and a pay statement configuration object
        /// An unsuccessful request will return a status code of 400
        /// </summary>
        /// <returns>A PayStatementConfiguration object that can be used to govern Employee Self Service Earnings Statements</returns>
        public async Task<PayStatementConfiguration> GetPayStatementConfigurationAsync()
        {
            try
            {
                return await payStatementConfigurationService.GetPayStatementConfigurationAsync();
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}