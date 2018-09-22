// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Bargaining Units data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BargainingUnitsController : BaseCompressedApiController
    {
        private readonly IBargainingUnitsService _bargainingUnitsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BargainingUnitsController class.
        /// </summary>
        /// <param name="bargainingUnitsService">Service of type <see cref="IBargainingUnitsService">IBargainingUnitsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public BargainingUnitsController(IBargainingUnitsService bargainingUnitsService, ILogger logger)
        {
            _bargainingUnitsService = bargainingUnitsService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves all bargaining units.
        /// </summary>
        /// <returns>All BargainingUnit objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BargainingUnit>> GetBargainingUnitsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var items = await _bargainingUnitsService.GetBargainingUnitsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _bargainingUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _bargainingUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Retrieves a bargaining unit by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.BargainingUnit">BargainingUnit.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.BargainingUnit> GetBargainingUnitByIdAsync(string id)
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
                var item = await _bargainingUnitsService.GetBargainingUnitsByGuidAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _bargainingUnitsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _bargainingUnitsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Updates a BargainingUnit.
        /// </summary>
        /// <param name="bargainingUnit"><see cref="BargainingUnit">BargainingUnit</see> to update</param>
        /// <returns>Newly updated <see cref="BargainingUnit">BargainingUnit</see></returns>
        [HttpPut]
        public async Task<Dtos.BargainingUnit> PutBargainingUnitAsync([FromBody] Dtos.BargainingUnit bargainingUnit)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a BargainingUnit.
        /// </summary>
        /// <param name="bargainingUnit"><see cref="BargainingUnit">BargainingUnit</see> to create</param>
        /// <returns>Newly created <see cref="BargainingUnit">BargainingUnit</see></returns>
        [HttpPost]
        public async Task<Dtos.BargainingUnit> PostBargainingUnitAsync([FromBody] Dtos.BargainingUnit bargainingUnit)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing BargainingUnit
        /// </summary>
        /// <param name="id">Id of the BargainingUnit to delete</param>
        [HttpDelete]
        public async Task DeleteBargainingUnitAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
