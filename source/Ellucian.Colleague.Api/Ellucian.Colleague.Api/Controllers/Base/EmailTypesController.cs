﻿// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using System.Threading.Tasks;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to EmailType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EmailTypesController : BaseCompressedApiController
    {
        private readonly IEmailTypeService _emailTypeService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Initializes a new instance of the EmailTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="emailTypeService">Service of type <see cref="IEmailTypeService">IEmailTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public EmailTypesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, IEmailTypeService emailTypeService, ILogger logger)
        {
            _emailTypeService = emailTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all email types.
        /// </summary>
        /// <returns>All <see cref="Dtos.EmailType">EmailType</see> objects.</returns>
        /// 
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmailType>> GetEmailTypesAsync()
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
                var emailTypes = await _emailTypeService.GetEmailTypesAsync(bypassCache);

                if (emailTypes != null && emailTypes.Any())
                {
                    AddEthosContextProperties(await _emailTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _emailTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              emailTypes.Select(a => a.Id).ToList()));
                }
                return emailTypes;

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves an email type by GUID.
        /// </summary>
        /// <returns>An <see cref="Dtos.EmailType">EmailType</see> object.</returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.EmailType> GetEmailTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _emailTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _emailTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _emailTypeService.GetEmailTypeByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves email types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.EmailType">EmailType</see> objects></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.EmailType>> GetAsync()
        {
            try
            {
                return await _emailTypeService.GetBaseEmailTypesAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(unexpectedGenericErrorMessage);
            }
        }
        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete an existing Email type in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Unique ID representing the Email Type to delete</param>
         [HttpDelete]
        public async Task<Ellucian.Colleague.Dtos.EmailType> DeleteEmailTypesAsync(string id)
         {
             //Delete is not supported for Colleague but HeDM requires full crud support.
             throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Update an Email Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="EmailType"><see cref="EmailType">EmailType</see> to update</param>
        [HttpPut]
         public async Task<Ellucian.Colleague.Dtos.EmailType> PutEmailTypesAsync([FromBody] Ellucian.Colleague.Dtos.EmailType EmailType)
         {
             //Update is not supported for Colleague but HeDM requires full crud support.
             throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
         }
        #endregion

        #region Post Methods
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Create an Email Type Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="EmailType"><see cref="EmailType">EmailType</see> to create</param>
        [HttpPost]
        public async Task<Ellucian.Colleague.Dtos.EmailType> PostEmailTypesAsync([FromBody] Ellucian.Colleague.Dtos.EmailType EmailType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}
