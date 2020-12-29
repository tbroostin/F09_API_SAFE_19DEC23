// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Social Media Types
    /// </summary>
     [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SocialMediaTypesController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the  Social Media Types Controller class.
        /// </summary>
        /// <param name="socialMediaTypeService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public SocialMediaTypesController(IDemographicService socialMediaTypeService, ILogger logger)
        {
            _demographicService = socialMediaTypeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Social Media Types
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All <see cref="Dtos.SocialMediaType">Social Media Types.</see></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.SocialMediaType>> GetSocialMediaTypesAsync()
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
                var socialMediaTypes = await _demographicService.GetSocialMediaTypesAsync(bypassCache);

                if (socialMediaTypes != null && socialMediaTypes.Any())
                {
                    AddEthosContextProperties(await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              socialMediaTypes.Select(a => a.Id).ToList()));
                }
                return socialMediaTypes;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Social Media Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.SocialMediaType">Social Media Type.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SocialMediaType> GetSocialMediaTypeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _demographicService.GetSocialMediaTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>        
        /// Creates an Social Media Type
        /// </summary>
        /// <param name="socialMediaType"><see cref="Dtos.SocialMediaType">SocialMediaType</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.SocialMediaType">SocialMediaType</see></returns>
        [HttpPost]
        public async Task <Dtos.SocialMediaType> PostSocialMediaTypeAsync([FromBody] Dtos.SocialMediaType socialMediaType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an SocialMediaType.
        /// </summary>
        /// <param name="id">Id of the Social Media Type to update</param>
        /// <param name="socialMediaType"><see cref="Dtos.SocialMediaType">SocialMediaType</see> to create</param>
        /// <returns>Updated <see cref="Dtos.SocialMediaType">SocialMediaType</see></returns>
        [HttpPut]
        public async Task<Dtos.SocialMediaType> PutSocialMediaTypeAsync([FromUri] string id, [FromBody] Dtos.SocialMediaType socialMediaType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Social Media Type
        /// </summary>
        /// <param name="id">Id of the  Social Media Type to delete</param>
        [HttpDelete]
        public async Task DeleteSocialMediaTypeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}