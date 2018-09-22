//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to FixedAssetTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class FixedAssetTypesController : BaseCompressedApiController
    {
        private readonly IFixedAssetTypesService _fixedAssetTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FixedAssetTypesController class.
        /// </summary>
        /// <param name="fixedAssetTypesService">Service of type <see cref="IFixedAssetTypesService">IFixedAssetTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FixedAssetTypesController(IFixedAssetTypesService fixedAssetTypesService, ILogger logger)
        {
            _fixedAssetTypesService = fixedAssetTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all fixedAssetTypes
        /// </summary>
        /// <returns>List of FixedAssetTypes <see cref="Dtos.FixedAssetType"/> objects representing matching fixedAssetTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetType>> GetFixedAssetTypesAsync()
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
                var items = await _fixedAssetTypesService.GetFixedAssetTypesAsync(bypassCache);

                AddEthosContextProperties(
                 await _fixedAssetTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _fixedAssetTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     items.Select(i => i.Id).Distinct().ToList()));

                return items;
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
        /// Read (GET) a fixedAssetTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FixedAssetType> GetFixedAssetTypesByGuidAsync(string guid)
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
                   await _fixedAssetTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _fixedAssetTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _fixedAssetTypesService.GetFixedAssetTypesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new fixedAssetTypes
        /// </summary>
        /// <param name="fixedAssetTypes">DTO of the new fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.FixedAssetType> PostFixedAssetTypesAsync([FromBody] Dtos.FixedAssetType fixedAssetTypes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing fixedAssetTypes
        /// </summary>
        /// <param name="guid">GUID of the fixedAssetTypes to update</param>
        /// <param name="fixedAssetTypes">DTO of the updated fixedAssetTypes</param>
        /// <returns>A fixedAssetTypes object <see cref="Dtos.FixedAssetType"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.FixedAssetType> PutFixedAssetTypesAsync([FromUri] string guid, [FromBody] Dtos.FixedAssetType fixedAssetTypes)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a fixedAssetTypes
        /// </summary>
        /// <param name="guid">GUID to desired fixedAssetTypes</param>
        [HttpDelete]
        public async Task DeleteFixedAssetTypesAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }       
    }
}