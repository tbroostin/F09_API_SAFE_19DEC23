//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to RegionIsoCodes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RegionIsoCodesController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IRegionIsoCodesService _regionIsoCodesService;

        /// <summary>
        /// Initializes a new instance of the RegionIsoCodesController class.
        /// </summary>
        /// <param name="regionIsoCodesService">Service of type <see cref="IRegionIsoCodesService">IRegionIsoCodesService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>      
        public RegionIsoCodesController(IRegionIsoCodesService regionIsoCodesService, ILogger logger)
        {
            this._logger = logger;
            this._regionIsoCodesService = regionIsoCodesService;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all region-iso-codes
        /// </summary>
        /// <returns>All region ISO codes<see cref="Dtos.RegionIsoCodes">RegionIsoCodes</see></returns>               
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.RegionIsoCodes))]
        //public async Task<IEnumerable<Ellucian.Colleague.Dtos.RegionIsoCodes>> GetRegionIsoCodesAsync()
        //public async Task<IHttpActionResult> GetRegionIsoCodesAsync(QueryStringFilter criteria)
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.RegionIsoCodes>> GetRegionIsoCodesAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaFilter = GetFilterObject<Dtos.RegionIsoCodes>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return null;
                //return new PagedHttpActionResult<IEnumerable<Dtos.RegionIsoCodes>>(new List<Dtos.RegionIsoCodes>(), page, 0, this.Request);


            try
            {
                var regionIsoCodes = await _regionIsoCodesService.GetRegionIsoCodesAsync(criteriaFilter, bypassCache);

                if (regionIsoCodes != null && regionIsoCodes.Any())
                {
                    AddEthosContextProperties(await _regionIsoCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _regionIsoCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              regionIsoCodes.Select(a => a.Id).ToList()));
                }
                return regionIsoCodes;
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
        /// Retrieve (GET) an existing region-iso-codes
        /// </summary>
        /// <param name="guid">GUID of the region-iso-codes to get</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.RegionIsoCodes> GetRegionIsoCodesByGuidAsync(string guid)
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
                   await _regionIsoCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _regionIsoCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync(guid);
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
        /// Create (POST) a new regionIsoCodes
        /// </summary>
        /// <param name="regionIsoCodes">DTO of the new regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.RegionIsoCodes> PostRegionIsoCodesAsync([FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID of the regionIsoCodes to update</param>
        /// <param name="regionIsoCodes">DTO of the updated regionIsoCodes</param>
        /// <returns>A regionIsoCodes object <see cref="Dtos.RegionIsoCodes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.RegionIsoCodes> PutRegionIsoCodesAsync([FromUri] string guid, [FromBody] Dtos.RegionIsoCodes regionIsoCodes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a regionIsoCodes
        /// </summary>
        /// <param name="guid">GUID to desired regionIsoCodes</param>
        [HttpDelete]
        public async Task DeleteRegionIsoCodesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}