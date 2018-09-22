// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
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
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AccountReceivableTypes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AccountReceivableTypesController : BaseCompressedApiController
    {
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountReceivableTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AccountReceivableTypesController(IAdapterRegistry adapterRegistry, ICurriculumService curriculumService, ILogger logger)
        {
            _curriculumService = curriculumService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all account receivable types.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All account receivable types objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountReceivableType>> GetAccountReceivableTypesAsync()
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
                var items =  await _curriculumService.GetAccountReceivableTypesAsync(bypassCache);

                AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
              items.Select(a => a.Id).ToList()));


                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a account receivable type by ID.
        /// </summary>
        /// <param name="id">Id of account receivable type to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AccountReceivableType">account receivable type.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.AccountReceivableType> GetAccountReceivableTypeByIdAsync(string id)
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
                var item = await _curriculumService.GetAccountReceivableTypeByIdAsync(id);

                if (item != null)
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
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
        /// Creates a AccountReceivableType.
        /// </summary>
        /// <param name="accountReceivableType"><see cref="Dtos.AccountReceivableType">AccountReceivableType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AccountReceivableType">AccountReceivableType</see></returns>
        [HttpPost]
        public async Task<Dtos.AccountReceivableType> PostAccountReceivableTypeAsync([FromBody] Dtos.AccountReceivableType accountReceivableType)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a account receivable type.
        /// </summary>
        /// <param name="id">Id of the AccountReceivableType to update</param>
        /// <param name="accountReceivableType"><see cref="Dtos.AccountReceivableType">AccountReceivableType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AccountReceivableType">AccountReceivableType</see></returns>
        [HttpPut]
        public async Task<Dtos.AccountReceivableType> PutAccountReceivableTypeAsync([FromUri] string id, [FromBody] Dtos.AccountReceivableType accountReceivableType)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing accountingReceivableType
        /// </summary>
        /// <param name="id">Id of the accountReceivableType to delete</param>
        [HttpDelete]
        public async Task DeleteAccountReceivableTypeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}