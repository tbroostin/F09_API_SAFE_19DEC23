// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
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
    /// Provides access to CommodityCodes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class CommodityCodesController : BaseCompressedApiController
    {
        private readonly ICommodityCodesService _commodityCodesService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommodityCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="commodityCodesService">Service of type <see cref="ICommodityCodesService">ICommodityCodesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CommodityCodesController(IAdapterRegistry adapterRegistry, ICommodityCodesService commodityCodesService, ILogger logger)
        {
            _commodityCodesService = commodityCodesService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all commodity codes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All commodity codes objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityCode>> GetCommodityCodesAsync()
        {
            bool bypassCache = false; 
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var items = await _commodityCodesService.GetCommodityCodesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _commodityCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a commodity code by ID.
        /// </summary>
        /// <param name="id">Id of commodity code to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CommodityCode">commodity code.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CommodityCode> GetCommodityCodeByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

            try
            {
                var item = await _commodityCodesService.GetCommodityCodeByIdAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _commodityCodesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityCodesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a CommodityCode.
        /// </summary>
        /// <param name="commodityCode"><see cref="Dtos.CommodityCode">CommodityCode</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CommodityCode">CommodityCode</see></returns>
        [HttpPost]
        public async Task<Dtos.CommodityCode> PostCommodityCodeAsync([FromBody] Dtos.CommodityCode commodityCode)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a commodity code.
        /// </summary>
        /// <param name="id">Id of the CommodityCode to update</param>
        /// <param name="commodityCode"><see cref="Dtos.CommodityCode">CommodityCode</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CommodityCode">CommodityCode</see></returns>
        [HttpPut]
        public async Task<Dtos.CommodityCode> PutCommodityCodeAsync([FromUri] string id, [FromBody] Dtos.CommodityCode commodityCode)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing commodityCode
        /// </summary>
        /// <param name="id">Id of the commodityCode to delete</param>
        [HttpDelete]
        public async Task DeleteCommodityCodeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Returns all CommodityCodes
        /// </summary>
        /// <returns>List of CommodityCodes DTO objects </returns>
        /// <accessComments>
        /// No permission is needed.
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>> GetAllCommodityCodesAsync()
        {
            try
            {
                var dtos = await _commodityCodesService.GetAllCommodityCodesAsync();
                return dtos;
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get Commodity Codes.", HttpStatusCode.BadRequest);
            }
        }
    }
}