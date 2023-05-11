// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PhoneType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhoneTypesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IPhoneTypeService _phoneTypeService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the PhoneTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="phoneTypeService">Service of type<see cref="IPhoneTypeService"> IPhoneTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PhoneTypesController(IAdapterRegistry adapterRegistry, IPhoneTypeService phoneTypeService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
             _phoneTypeService = phoneTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all phone types.
        /// </summary>
        /// <returns>All <see cref="Dtos.PhoneType2">PhoneType</see> objects.</returns>
        /// 
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PhoneType2>> GetPhoneTypesAsync()
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
                var phoneTypes = await _phoneTypeService.GetPhoneTypesAsync(bypassCache);

                if (phoneTypes != null && phoneTypes.Any())
                {
                    AddEthosContextProperties(await _phoneTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _phoneTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              phoneTypes.Select(a => a.Id).ToList()));
                }
                return phoneTypes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an phone type by ID.
        /// </summary>
        /// <param name="id">Unique ID representing the Phone Type to get</param>
        /// <returns>An <see cref="Dtos.PhoneType2">PhoneType</see> object.</returns>
        public async Task<Ellucian.Colleague.Dtos.PhoneType2> GetPhoneTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _phoneTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _phoneTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _phoneTypeService.GetPhoneTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }


        /// <summary>
        /// Retrieves all phone types.
        /// </summary>
        /// <returns>All <see cref="Dtos.PhoneType">PhoneType </see>objects.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneType>> GetAsync()
        {
            try
            {
                return await _phoneTypeService.GetBasePhoneTypesAsync();
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

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Phone type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Phone Type to delete</param>
        [HttpDelete]
        public async Task DeletePhoneTypesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update a Phone Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="PhoneType"><see cref="PhoneType2">PhoneType</see> to update</param>
        [HttpPut]
        public async Task<Ellucian.Colleague.Dtos.PhoneType2> PutPhoneTypesAsync([FromBody] Ellucian.Colleague.Dtos.PhoneType2 PhoneType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create a Phone Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="PhoneType"><see cref="PhoneType">PhoneType</see> to create</param>
        [HttpPost]
        public async Task<Ellucian.Colleague.Dtos.PhoneType2> PostPhoneTypesAsync([FromBody] Ellucian.Colleague.Dtos.PhoneType2 PhoneType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
