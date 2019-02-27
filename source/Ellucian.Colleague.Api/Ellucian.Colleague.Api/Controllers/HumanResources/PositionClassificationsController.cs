//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PositionClassifications
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PositionClassificationsController : BaseCompressedApiController
    {
        private readonly IPositionClassificationsService _positionClassificationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PositionClassificationsController class.
        /// </summary>
        /// <param name="positionClassificationsService">Service of type <see cref="IPositionClassificationsService">IPositionClassificationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PositionClassificationsController(IPositionClassificationsService positionClassificationsService, ILogger logger)
        {
            _positionClassificationsService = positionClassificationsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all positionClassifications
        /// </summary>
        /// <returns>List of PositionClassifications <see cref="Dtos.PositionClassification"/> objects representing matching positionClassifications</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PositionClassification>> GetPositionClassificationsAsync()
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
                AddDataPrivacyContextProperty((await _positionClassificationsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _positionClassificationsService.GetPositionClassificationsAsync(bypassCache);
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
        /// Read (GET) a positionClassifications using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PositionClassification> GetPositionClassificationsByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _positionClassificationsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _positionClassificationsService.GetPositionClassificationsByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new positionClassifications
        /// </summary>
        /// <param name="positionClassifications">DTO of the new positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PositionClassification> PostPositionClassificationsAsync([FromBody] Dtos.PositionClassification positionClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing positionClassifications
        /// </summary>
        /// <param name="guid">GUID of the positionClassifications to update</param>
        /// <param name="positionClassifications">DTO of the updated positionClassifications</param>
        /// <returns>A positionClassifications object <see cref="Dtos.PositionClassification"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PositionClassification> PutPositionClassificationsAsync([FromUri] string guid, [FromBody] Dtos.PositionClassification positionClassifications)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a positionClassifications
        /// </summary>
        /// <param name="guid">GUID to desired positionClassifications</param>
        [HttpDelete]
        public async Task DeletePositionClassificationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}