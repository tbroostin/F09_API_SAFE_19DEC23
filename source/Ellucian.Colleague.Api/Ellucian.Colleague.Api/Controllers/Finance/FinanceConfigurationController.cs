// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get student finance parameters and settings.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class FinanceConfigurationController : BaseCompressedApiController
    {
        private readonly IFinanceConfigurationService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// FinanceConfigurationController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IFinanceConfigurationService">IFinanceConfigurationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinanceConfigurationController(IFinanceConfigurationService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the configuration information for Student Finance.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>The <see cref="FinanceConfiguration">Finance Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the required setup is not complete</exception>
        public FinanceConfiguration Get()
        {
            FinanceConfiguration configurationDto = null;
            try
            {
                configurationDto = _service.GetFinanceConfiguration();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information for Immediate Payment Control.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>The <see cref="ImmediatePaymentControl">Immediate Payment Control</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the required setup is not complete</exception>
        public ImmediatePaymentControl GetImmediatePaymentControl()
        {
            try
            {
                return _service.GetImmediatePaymentControl();
            }
            catch (ColleagueSessionExpiredException csee)
            {
                _logger.Error(csee, csee.Message);
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
