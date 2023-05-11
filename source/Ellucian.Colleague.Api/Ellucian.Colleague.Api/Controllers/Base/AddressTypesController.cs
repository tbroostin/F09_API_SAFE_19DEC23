// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AddressType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AddressTypesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAddressTypeService _addressTypeService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the AddressTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="addressTypeService">Service of type <see cref="IAddressTypeService">IAddressTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AddressTypesController(IAdapterRegistry adapterRegistry, IAddressTypeService addressTypeService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _addressTypeService = addressTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all address types.
        /// </summary>
        /// <returns>All AddressType objects.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>> GetAddressTypesAsync()
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
                var addressType = await _addressTypeService.GetAddressTypesAsync(bypassCache);

                if (addressType != null && addressType.Any())
                {
                    AddEthosContextProperties(await _addressTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _addressTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              addressType.Select(a => a.Id).ToList()));
                }

                return await _addressTypeService.GetAddressTypesAsync(bypassCache);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves an address type by GUID.
        /// </summary>
        /// /// <param name="id">Unique ID representing the Address Type to get</param>
        /// <returns>An AddressType object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.AddressType2> GetAddressTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _addressTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _addressTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _addressTypeService.GetAddressTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete an existing Address type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Address Type to delete</param>
        [HttpDelete]
        public async Task DeleteAddressTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update a Address Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="AddressType"><see cref="AddressType2">AddressType</see> to update</param>
        [HttpPut]
        public async Task<Ellucian.Colleague.Dtos.AddressType2> PutAddressTypesAsync([FromBody] Ellucian.Colleague.Dtos.AddressType2 AddressType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a Address Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="AddressType"><see cref="AddressType2">AddressType</see> to create</param>
        [HttpPost]
        public async Task<Ellucian.Colleague.Dtos.AddressType2> PostAddressTypesAsync([FromBody] Ellucian.Colleague.Dtos.AddressType2 AddressType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
