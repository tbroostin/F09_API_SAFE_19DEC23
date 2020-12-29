//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FixedAssetDesignations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetDesignationsController : BaseCompressedApiController
    {
        private readonly IFixedAssetDesignationsService _fixedAssetDesignationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetDesignationsController class.
        /// </summary>
        /// <param name="fixedAssetDesignationsService">Service of type <see cref="IFixedAssetDesignationsService">IFixedAssetDesignationsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FixedAssetDesignationsController(IFixedAssetDesignationsService fixedAssetDesignationsService, ILogger logger)
        {
            _fixedAssetDesignationsService = fixedAssetDesignationsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all fixedAssetDesignations
        /// </summary>
        /// <returns>List of FixedAssetDesignations <see cref="Dtos.FixedAssetDesignations"/> objects representing matching fixedAssetDesignations</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetDesignations>> GetFixedAssetDesignationsAsync()
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
                var fixedAssetDesignations = await _fixedAssetDesignationsService.GetFixedAssetDesignationsAsync(bypassCache);

                if (fixedAssetDesignations != null && fixedAssetDesignations.Any())
                {
                    AddEthosContextProperties(await _fixedAssetDesignationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _fixedAssetDesignationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              fixedAssetDesignations.Select(a => a.Id).ToList()));
                }
                return fixedAssetDesignations;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a fixedAssetDesignations using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FixedAssetDesignations> GetFixedAssetDesignationsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _fixedAssetDesignationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _fixedAssetDesignationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _fixedAssetDesignationsService.GetFixedAssetDesignationsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new fixedAssetDesignations
        /// </summary>
        /// <param name="fixedAssetDesignations">DTO of the new fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FixedAssetDesignations> PostFixedAssetDesignationsAsync([FromBody] Dtos.FixedAssetDesignations fixedAssetDesignations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssetDesignations
        /// </summary>
        /// <param name="guid">GUID of the fixedAssetDesignations to update</param>
        /// <param name="fixedAssetDesignations">DTO of the updated fixedAssetDesignations</param>
        /// <returns>A fixedAssetDesignations object <see cref="Dtos.FixedAssetDesignations"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FixedAssetDesignations> PutFixedAssetDesignationsAsync([FromUri] string guid, [FromBody] Dtos.FixedAssetDesignations fixedAssetDesignations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssetDesignations
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetDesignations</param>
        [HttpDelete]
        public async Task DeleteFixedAssetDesignationsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}