//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to EducationalGoals
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EducationalGoalsController : BaseCompressedApiController
    {
        private readonly IEducationalGoalsService _educationalGoalsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EducationalGoalsController class.
        /// </summary>
        /// <param name="educationalGoalsService">Service of type <see cref="IEducationalGoalsService">IEducationalGoalsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EducationalGoalsController(IEducationalGoalsService educationalGoalsService, ILogger logger)
        {
            _educationalGoalsService = educationalGoalsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all educationalGoals
        /// </summary>
        /// <returns>List of EducationalGoals <see cref="Dtos.EducationalGoals"/> objects representing matching educationalGoals</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalGoals>> GetEducationalGoalsAsync()
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
                var educationalGoals = await _educationalGoalsService.GetEducationalGoalsAsync(bypassCache);

                if (educationalGoals != null && educationalGoals.Any())
                {
                    AddEthosContextProperties(await _educationalGoalsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _educationalGoalsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              educationalGoals.Select(a => a.Id).ToList()));
                }
                return educationalGoals;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a educationalGoals using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired educationalGoals</param>
        /// <returns>A educationalGoals object <see cref="Dtos.EducationalGoals"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EducationalGoals> GetEducationalGoalsByGuidAsync(string guid)
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
                   await _educationalGoalsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _educationalGoalsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _educationalGoalsService.GetEducationalGoalsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new educationalGoals
        /// </summary>
        /// <param name="educationalGoals">DTO of the new educationalGoals</param>
        /// <returns>A educationalGoals object <see cref="Dtos.EducationalGoals"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EducationalGoals> PostEducationalGoalsAsync([FromBody] Dtos.EducationalGoals educationalGoals)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing educationalGoals
        /// </summary>
        /// <param name="guid">GUID of the educationalGoals to update</param>
        /// <param name="educationalGoals">DTO of the updated educationalGoals</param>
        /// <returns>A educationalGoals object <see cref="Dtos.EducationalGoals"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EducationalGoals> PutEducationalGoalsAsync([FromUri] string guid, [FromBody] Dtos.EducationalGoals educationalGoals)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a educationalGoals
        /// </summary>
        /// <param name="guid">GUID to desired educationalGoals</param>
        [HttpDelete]
        public async Task DeleteEducationalGoalsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}