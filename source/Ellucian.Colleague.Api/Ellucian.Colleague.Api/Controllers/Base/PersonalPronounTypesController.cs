// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonalPronounType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalPronounTypesController : BaseCompressedApiController
    {
        private readonly IPersonalPronounTypeService _personalPronounTypeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonalPronounTypesController class.
        /// </summary>
        /// <param name="personalPronounTypeService">Service of type <see cref="IPersonalPronounTypeService">IPersonalPronounTypeService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonalPronounTypesController(IPersonalPronounTypeService personalPronounTypeService, ILogger logger)
        {
            _personalPronounTypeService = personalPronounTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves personal pronoun types
        /// </summary>
        /// <returns>A list of <see cref="Dtos.Base.PersonalPronounType">PersonalPronounType</see> objects></returns>
        [HttpGet]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonalPronounType>> GetAsync()
        {
            try
            {
                bool ignoreCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        ignoreCache = true;
                    }
                }
                return await _personalPronounTypeService.GetBasePersonalPronounTypesAsync(ignoreCache);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving personal pronoun types";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Return all personalPronouns
        /// </summary>
        /// <returns>List of PersonalPronouns <see cref="Dtos.PersonalPronouns"/> objects representing matching personalPronouns</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalPronouns>> GetPersonalPronounsAsync()
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
                var personalPronouns = await _personalPronounTypeService.GetPersonalPronounsAsync(bypassCache);

                if (personalPronouns != null && personalPronouns.Any())
                {
                    AddEthosContextProperties(await _personalPronounTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _personalPronounTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personalPronouns.Select(a => a.Id).ToList()));
                }
                return personalPronouns;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Read (GET) a personalPronouns using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonalPronouns> GetPersonalPronounsByGuidAsync(string guid)
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
                   await _personalPronounTypeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personalPronounTypeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personalPronounTypeService.GetPersonalPronounsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
        /// Create (POST) a new personalPronouns
        /// </summary>
        /// <param name="personalPronouns">DTO of the new personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonalPronouns> PostPersonalPronounsAsync([FromBody] Dtos.PersonalPronouns personalPronouns)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personalPronouns
        /// </summary>
        /// <param name="guid">GUID of the personalPronouns to update</param>
        /// <param name="personalPronouns">DTO of the updated personalPronouns</param>
        /// <returns>A personalPronouns object <see cref="Dtos.PersonalPronouns"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonalPronouns> PutPersonalPronounsAsync([FromUri] string guid, [FromBody] Dtos.PersonalPronouns personalPronouns)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personalPronouns
        /// </summary>
        /// <param name="guid">GUID to desired personalPronouns</param>
        [HttpDelete]
        public async Task DeletePersonalPronounsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}