//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to BeneficiaryPreferenceTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BeneficiaryPreferenceTypesController : BaseCompressedApiController
    {
        private readonly IBeneficiaryPreferenceTypesService _beneficiaryPreferenceTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BeneficiaryPreferenceTypesController class.
        /// </summary>
        /// <param name="beneficiaryPreferenceTypesService">Service of type <see cref="IBeneficiaryPreferenceTypesService">IBeneficiaryPreferenceTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BeneficiaryPreferenceTypesController(IBeneficiaryPreferenceTypesService beneficiaryPreferenceTypesService, ILogger logger)
        {
            _beneficiaryPreferenceTypesService = beneficiaryPreferenceTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all beneficiaryPreferenceTypes
        /// </summary>
        /// <returns>List of BeneficiaryPreferenceTypes <see cref="Dtos.BeneficiaryPreferenceTypes"/> objects representing matching beneficiaryPreferenceTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes>> GetBeneficiaryPreferenceTypesAsync()
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
                AddDataPrivacyContextProperty((await _beneficiaryPreferenceTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesAsync(bypassCache);
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
        /// Read (GET) a beneficiaryPreferenceTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.BeneficiaryPreferenceTypes> GetBeneficiaryPreferenceTypesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _beneficiaryPreferenceTypesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _beneficiaryPreferenceTypesService.GetBeneficiaryPreferenceTypesByGuidAsync(guid);
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
        /// Create (POST) a new beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="beneficiaryPreferenceTypes">DTO of the new beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.BeneficiaryPreferenceTypes> PostBeneficiaryPreferenceTypesAsync([FromBody] Dtos.BeneficiaryPreferenceTypes beneficiaryPreferenceTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="guid">GUID of the beneficiaryPreferenceTypes to update</param>
        /// <param name="beneficiaryPreferenceTypes">DTO of the updated beneficiaryPreferenceTypes</param>
        /// <returns>A beneficiaryPreferenceTypes object <see cref="Dtos.BeneficiaryPreferenceTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.BeneficiaryPreferenceTypes> PutBeneficiaryPreferenceTypesAsync([FromUri] string guid, [FromBody] Dtos.BeneficiaryPreferenceTypes beneficiaryPreferenceTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a beneficiaryPreferenceTypes
        /// </summary>
        /// <param name="guid">GUID to desired beneficiaryPreferenceTypes</param>
        [HttpDelete]
        public async Task DeleteBeneficiaryPreferenceTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}