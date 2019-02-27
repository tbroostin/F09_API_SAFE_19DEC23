// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person hold types
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonHoldTypesController :BaseCompressedApiController
    {
        private readonly IPersonHoldTypeService _personHoldTypeService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// PersonHoldTypesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personHoldTypeService">Service of type <see cref="IPersonHoldTypeService">IPersonHoldTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonHoldTypesController(IAdapterRegistry adapterRegistry, IPersonHoldTypeService personHoldTypeService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _personHoldTypeService = personHoldTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4.5</remarks>
        /// <summary>
        /// Retrieves all person hold types
        /// </summary>
        /// <returns>All PersonHoldTypes objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonHoldType>> GetPersonHoldTypesAsync()
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
                var items = await _personHoldTypeService.GetPersonHoldTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _personHoldTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personHoldTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4.5</remarks>
        /// <summary>
        /// Retrieves a person hold type by ID
        /// </summary>
        /// <returns>A PersonHoldType object.</returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.PersonHoldType> GetPersonHoldTypeByIdAsync([FromUri] string id)
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
                return await _personHoldTypeService.GetPersonHoldTypeByGuid2Async(id, bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonHoldType
        /// </summary>
        /// <param name="id">Id of the PersonHoldType to delete</param>
        [HttpDelete]
        public async Task<Dtos.PersonHoldType> DeletePersonHoldTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to update</param>
        /// <returns>Newly updated <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPut]
        public async Task<Ellucian.Colleague.Dtos.PersonHoldType> PutPersonHoldTypesAsync([FromBody] Ellucian.Colleague.Dtos.PersonHoldType personHoldType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a PersonHoldType.
        /// </summary>
        /// <param name="personHoldType"><see cref="PersonHoldType">PersonHoldType</see> to create</param>
        /// <returns>Newly created <see cref="PersonHoldType">PersonHoldType</see></returns>
        [HttpPost]
        public async Task<Dtos.PersonHoldType> PostPersonHoldTypesAsync([FromBody] Dtos.PersonHoldType personHoldType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}