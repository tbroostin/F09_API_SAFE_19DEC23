//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to CourseStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseStatusesController : BaseCompressedApiController
    {
        private readonly ICourseStatusesService _courseStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseStatusesController class.
        /// </summary>
        /// <param name="courseStatusesService">Service of type <see cref="ICourseStatusesService">ICourseStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CourseStatusesController(ICourseStatusesService courseStatusesService, ILogger logger)
        {
            _courseStatusesService = courseStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all courseStatuses
        /// </summary>
        /// <returns>List of CourseStatuses <see cref="Dtos.CourseStatuses"/> objects representing matching courseStatuses</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseStatuses>> GetCourseStatusesAsync()
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
                var courseStatuses = await _courseStatusesService.GetCourseStatusesAsync(bypassCache);

                if (courseStatuses != null && courseStatuses.Any())
                {
                    AddEthosContextProperties(await _courseStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _courseStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              courseStatuses.Select(a => a.Id).ToList()));
                }
                return courseStatuses;
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
        /// Read (GET) a courseStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired courseStatuses</param>
        /// <returns>A courseStatuses object <see cref="Dtos.CourseStatuses"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CourseStatuses> GetCourseStatusesByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _courseStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _courseStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _courseStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _courseStatusesService.GetCourseStatusesByGuidAsync(guid);
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
        /// Create (POST) a new courseStatuses
        /// </summary>
        /// <param name="courseStatuses">DTO of the new courseStatuses</param>
        /// <returns>A courseStatuses object <see cref="Dtos.CourseStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.CourseStatuses> PostCourseStatusesAsync([FromBody] Dtos.CourseStatuses courseStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing courseStatuses
        /// </summary>
        /// <param name="guid">GUID of the courseStatuses to update</param>
        /// <param name="courseStatuses">DTO of the updated courseStatuses</param>
        /// <returns>A courseStatuses object <see cref="Dtos.CourseStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.CourseStatuses> PutCourseStatusesAsync([FromUri] string guid, [FromBody] Dtos.CourseStatuses courseStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a courseStatuses
        /// </summary>
        /// <param name="guid">GUID to desired courseStatuses</param>
        [HttpDelete]
        public async Task DeleteCourseStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}