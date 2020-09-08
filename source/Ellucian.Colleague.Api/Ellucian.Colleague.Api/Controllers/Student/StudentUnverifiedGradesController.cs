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
    /// Provides access to StudentUnverifiedGrades
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentUnverifiedGradesController : BaseCompressedApiController
    {
        private readonly IStudentUnverifiedGradesService _studentUnverifiedGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentUnverifiedGradesController class.
        /// </summary>
        /// <param name="studentUnverifiedGradesService">Service of type <see cref="IStudentUnverifiedGradesService">IStudentUnverifiedGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentUnverifiedGradesController(IStudentUnverifiedGradesService studentUnverifiedGradesService, ILogger logger)
        {
            _studentUnverifiedGradesService = studentUnverifiedGradesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentUnverifiedGrades
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="section">filter section</param>
        /// <returns>List of StudentUnverifiedGrades <see cref="Dtos.StudentUnverifiedGrades"/> objects representing matching studentUnverifiedGrades</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentUnverifiedGrades))]
        [QueryStringFilterFilter("section", typeof(Dtos.Filters.StudentUnverifiedGradesFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetStudentUnverifiedGradesAsync(Paging page, QueryStringFilter criteria, QueryStringFilter section)
        {
            string student = string.Empty, sectionRegistration = string.Empty, sectionId = string.Empty;

            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaObj = GetFilterObject<Dtos.StudentUnverifiedGrades>(_logger, "criteria");
            if (criteriaObj != null)
            {
                student = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                sectionRegistration = criteriaObj.SectionRegistration != null ? criteriaObj.SectionRegistration.Id : string.Empty;
            }

            var sectionObj = GetFilterObject<Dtos.Filters.StudentUnverifiedGradesFilter>(_logger, "section");
            if (sectionObj != null)
            {
                sectionId = sectionObj.Section != null ? sectionObj.Section.Id : string.Empty;
            }
            

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentUnverifiedGrades>>(new List<Dtos.StudentUnverifiedGrades>(), page, 0, this.Request);

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentUnverifiedGradesService.GetStudentUnverifiedGradesAsync(page.Offset, page.Limit, student, sectionRegistration, sectionId, bypassCache);

                AddEthosContextProperties(
                  await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentUnverifiedGrades>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a studentUnverifiedGrades using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]    
        public async Task<Dtos.StudentUnverifiedGrades> GetStudentUnverifiedGradesByGuidAsync(string guid)
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
                   await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentUnverifiedGradesService.GetStudentUnverifiedGradesByGuidAsync(guid);
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
        /// Create (POST) a new studentUnverifiedGrades
        /// </summary>
        /// <param name="studentUnverifiedGrades">DTO of the new studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentUnverifiedGrades> PostStudentUnverifiedGradesAsync([FromBody] Dtos.StudentUnverifiedGrades studentUnverifiedGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentUnverifiedGrades
        /// </summary>
        /// <param name="guid">GUID of the studentUnverifiedGrades to update</param>
        /// <param name="studentUnverifiedGrades">DTO of the updated studentUnverifiedGrades</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentUnverifiedGrades> PutStudentUnverifiedGradesAsync([FromUri] string guid, [FromBody] Dtos.StudentUnverifiedGrades studentUnverifiedGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentUnverifiedGrades
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGrades</param>
        [HttpDelete]
        public async Task DeleteStudentUnverifiedGradesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}