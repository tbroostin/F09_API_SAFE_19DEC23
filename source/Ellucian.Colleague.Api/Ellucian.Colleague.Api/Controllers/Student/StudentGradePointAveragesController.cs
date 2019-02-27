//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
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
    /// Provides access to StudentGradePointAverages
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
    public class StudentGradePointAveragesController : BaseCompressedApiController
    {
        private readonly IStudentGradePointAveragesService _studentGradePointAveragesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentGradePointAveragesController class.
        /// </summary>
        /// <param name="studentGradePointAveragesService">Service of type <see cref="IStudentGradePointAveragesService">IStudentGradePointAveragesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentGradePointAveragesController(IStudentGradePointAveragesService studentGradePointAveragesService, ILogger logger)
        {
            _studentGradePointAveragesService = studentGradePointAveragesService;
            _logger = logger;
        }


        /// <summary>
        /// Return all studentGradePointAverages
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="gradeDate"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentGradePointAverages))]
        [QueryStringFilterFilter("gradeDate", typeof(Dtos.Filters.GradeDateFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetStudentGradePointAveragesAsync(Paging page, QueryStringFilter criteria, //QueryStringFilter academicPeriod, 
            QueryStringFilter gradeDate)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentGradePointAverages>(_logger, "criteria");

                //academicPeriod
                string academicPeriodFilterValue = string.Empty;
                var academicPeriodFilterObj = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>(_logger, "academicPeriod");
                if (academicPeriodFilterObj != null && academicPeriodFilterObj.AcademicPeriod != null && !string.IsNullOrEmpty(academicPeriodFilterObj.AcademicPeriod.Id))
                {
                    academicPeriodFilterValue = academicPeriodFilterObj.AcademicPeriod.Id != null ? academicPeriodFilterObj.AcademicPeriod.Id : null;
                }

                //gradeDate
                string gradeDateFilterValue = string.Empty;
                var gradeDateFilterObj = GetFilterObject<Dtos.Filters.GradeDateFilter>(_logger, "gradeDate");
                if (gradeDateFilterObj != null && gradeDateFilterObj.GradeDate.HasValue )
                {
                    gradeDateFilterValue = gradeDateFilterObj.GradeDate.Value.ToString();
                }


                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentGradePointAverages>>(new List<Dtos.StudentGradePointAverages>(), page, 0, this.Request);


                var pageOfItems = await _studentGradePointAveragesService.GetStudentGradePointAveragesAsync(page.Offset, page.Limit, criteriaObj, gradeDateFilterValue, bypassCache);

                if (pageOfItems != null && pageOfItems.Item1.Any())
                {
                    AddEthosContextProperties(
                      await _studentGradePointAveragesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _studentGradePointAveragesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          pageOfItems.Item1.Select(i => i.Id).ToList()));
                }

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentGradePointAverages>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a studentGradePointAverages using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentGradePointAverages> GetStudentGradePointAveragesByGuidAsync(string guid)
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
                   await _studentGradePointAveragesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentGradePointAveragesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentGradePointAveragesService.GetStudentGradePointAveragesByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new studentGradePointAverages
        /// </summary>
        /// <param name="studentGradePointAverages">DTO of the new studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.StudentGradePointAverages> PostStudentGradePointAveragesAsync([FromBody] Dtos.StudentGradePointAverages studentGradePointAverages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentGradePointAverages
        /// </summary>
        /// <param name="guid">GUID of the studentGradePointAverages to update</param>
        /// <param name="studentGradePointAverages">DTO of the updated studentGradePointAverages</param>
        /// <returns>A studentGradePointAverages object <see cref="Dtos.StudentGradePointAverages"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.StudentGradePointAverages> PutStudentGradePointAveragesAsync([FromUri] string guid, [FromBody] Dtos.StudentGradePointAverages studentGradePointAverages)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a studentGradePointAverages
        /// </summary>
        /// <param name="guid">GUID to desired studentGradePointAverages</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteStudentGradePointAveragesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}