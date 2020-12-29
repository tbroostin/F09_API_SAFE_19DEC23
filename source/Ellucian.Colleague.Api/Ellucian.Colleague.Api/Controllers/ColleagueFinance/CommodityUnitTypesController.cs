// Copyright 2016 - 2019 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to CommodityUnitTypes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class CommodityUnitTypesController : BaseCompressedApiController
    {
        private readonly ICommodityUnitTypesService _commodityUnitTypesService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommodityUnitTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="commodityUnitTypesService">Service of type <see cref="ICommodityUnitTypesService">ICommodityUnitTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CommodityUnitTypesController(IAdapterRegistry adapterRegistry, ICommodityUnitTypesService commodityUnitTypesService, ILogger logger)
        {
            _commodityUnitTypesService = commodityUnitTypesService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all commodity unit types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All commodity unit types objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetCommodityUnitTypesAsync()
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
                var items = await _commodityUnitTypesService.GetCommodityUnitTypesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _commodityUnitTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityUnitTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Retrieves a commodity unit type by ID.
        /// </summary>
        /// <param name="id">Id of commodity unit type to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.CommodityUnitType">commodity unit type.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CommodityUnitType> GetCommodityUnitTypeByIdAsync(string id)
        {
            var bypassCache = false;
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
                var item = await _commodityUnitTypesService.GetCommodityUnitTypeByIdAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _commodityUnitTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _commodityUnitTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a CommodityUnitType.
        /// </summary>
        /// <param name="commodityUnitType"><see cref="Dtos.CommodityUnitType">CommodityUnitType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.CommodityUnitType">CommodityUnitType</see></returns>
        [HttpPost]
        public async Task<Dtos.CommodityUnitType> PostCommodityUnitTypeAsync([FromBody] Dtos.CommodityUnitType commodityUnitType)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a commodity unit type.
        /// </summary>
        /// <param name="id">Id of the CommodityUnitType to update</param>
        /// <param name="commodityUnitType"><see cref="Dtos.CommodityUnitType">CommodityUnitType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.CommodityUnitType">CommodityUnitType</see></returns>
        [HttpPut]
        public async Task<Dtos.CommodityUnitType> PutCommodityUnitTypeAsync([FromUri] string id, [FromBody] Dtos.CommodityUnitType commodityUnitType)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing commodityUnitType
        /// </summary>
        /// <param name="id">Id of the commodityUnitType to delete</param>
        [HttpDelete]
        public async Task DeleteCommodityUnitTypeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Retrieves all commodity unit types.
        /// </summary>
        /// <returns>All commodity unit types objects.</returns>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetAllCommodityUnitTypesAsync()
        {       
            try
            {
                var items = await _commodityUnitTypesService.GetAllCommodityUnitTypesAsync();
                
                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get Unit typess.", HttpStatusCode.BadRequest);
            }
        }
    }
}