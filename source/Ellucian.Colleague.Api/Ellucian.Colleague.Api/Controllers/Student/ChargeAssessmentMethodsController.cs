//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
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
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to ChargeAssessmentMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ChargeAssessmentMethodsController : BaseCompressedApiController
    {
        private readonly IChargeAssessmentMethodsService _chargeAssessmentMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ChargeAssessmentMethodsController class.
        /// </summary>
        /// <param name="chargeAssessmentMethodsService">Service of type <see cref="IChargeAssessmentMethodsService">IChargeAssessmentMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ChargeAssessmentMethodsController(IChargeAssessmentMethodsService chargeAssessmentMethodsService, ILogger logger)
        {
            _chargeAssessmentMethodsService = chargeAssessmentMethodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all chargeAssessmentMethods
        /// </summary>
        /// <returns>List of ChargeAssessmentMethods <see cref="Dtos.ChargeAssessmentMethods"/> objects representing matching chargeAssessmentMethods</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ChargeAssessmentMethods>> GetChargeAssessmentMethodsAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                AddDataPrivacyContextProperty((await _chargeAssessmentMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a chargeAssessmentMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.ChargeAssessmentMethods> GetChargeAssessmentMethodsByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddDataPrivacyContextProperty((await _chargeAssessmentMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new chargeAssessmentMethods
        /// </summary>
        /// <param name="chargeAssessmentMethods">DTO of the new chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ChargeAssessmentMethods> PostChargeAssessmentMethodsAsync([FromBody] Dtos.ChargeAssessmentMethods chargeAssessmentMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing chargeAssessmentMethods
        /// </summary>
        /// <param name="guid">GUID of the chargeAssessmentMethods to update</param>
        /// <param name="chargeAssessmentMethods">DTO of the updated chargeAssessmentMethods</param>
        /// <returns>A chargeAssessmentMethods object <see cref="Dtos.ChargeAssessmentMethods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ChargeAssessmentMethods> PutChargeAssessmentMethodsAsync([FromUri] string guid, [FromBody] Dtos.ChargeAssessmentMethods chargeAssessmentMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a chargeAssessmentMethods
        /// </summary>
        /// <param name="guid">GUID to desired chargeAssessmentMethods</param>
        [HttpDelete]
        public async Task DeleteChargeAssessmentMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}