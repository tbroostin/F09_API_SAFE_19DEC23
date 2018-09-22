//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CourseTopics
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseTopicsController : BaseCompressedApiController
    {
        private readonly ICourseTopicsService _courseTopicsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseTopicsController class.
        /// </summary>
        /// <param name="courseTopicsService">Service of type <see cref="ICourseTopicsService">ICourseTopicsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CourseTopicsController(ICourseTopicsService courseTopicsService, ILogger logger)
        {
            _courseTopicsService = courseTopicsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all courseTopics
        /// </summary>
        /// <returns>List of CourseTopics <see cref="Dtos.CourseTopics"/> objects representing matching courseTopics</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseTopics>> GetCourseTopicsAsync()
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
                var items = await _courseTopicsService.GetCourseTopicsAsync(bypassCache);

                AddEthosContextProperties(
                    await _courseTopicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseTopicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
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
        /// Read (GET) a courseTopics using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired courseTopics</param>
        /// <returns>A courseTopics object <see cref="Dtos.CourseTopics"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CourseTopics> GetCourseTopicsByGuidAsync(string guid)
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
                    await _courseTopicsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseTopicsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _courseTopicsService.GetCourseTopicsByGuidAsync(guid);
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
        /// Create (POST) a new courseTopics
        /// </summary>
        /// <param name="courseTopics">DTO of the new courseTopics</param>
        /// <returns>A courseTopics object <see cref="Dtos.CourseTopics"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.CourseTopics> PostCourseTopicsAsync([FromBody] Dtos.CourseTopics courseTopics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing courseTopics
        /// </summary>
        /// <param name="guid">GUID of the courseTopics to update</param>
        /// <param name="courseTopics">DTO of the updated courseTopics</param>
        /// <returns>A courseTopics object <see cref="Dtos.CourseTopics"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.CourseTopics> PutCourseTopicsAsync([FromUri] string guid, [FromBody] Dtos.CourseTopics courseTopics)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a courseTopics
        /// </summary>
        /// <param name="guid">GUID to desired courseTopics</param>
        [HttpDelete]
        public async Task DeleteCourseTopicsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}