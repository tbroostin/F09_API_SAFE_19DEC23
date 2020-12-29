// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Course data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CoursesController : BaseCompressedApiController
    {
        private readonly ICourseService _courseService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CoursesController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="ICourseService">ICourseService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public CoursesController(ICourseService service, ILogger logger)
        {
            _courseService = service;
            _logger = logger;
        }

        /// <summary>
        /// Performs a search of courses in Colleague that are available for registration. 
        /// The criteria supplies a keyword, requirement, and various filters which may be used to search and narrow a list of courses.
        /// </summary>
        /// <param name="criteria"><see cref="CourseSearchCriteria">Course search criteria</see></param>
        /// <param name="pageSize">integer page size</param>
        /// <param name="pageIndex">integer page index</param>
        /// <returns>A <see cref="CoursePage">page</see> of courses matching criteria with totals and filter information</returns>
        // [CacheControlFilter(MaxAgeHours = 1, Public = true, Revalidate = true)]
        [Obsolete("Obsolete as of API version 1.3, use version 2 of this API")]
        public async Task<CoursePage> PostSearchAsync([FromBody]CourseSearchCriteria criteria, int pageSize, int pageIndex)
        {
            criteria.Keyword = criteria.Keyword != null ? criteria.Keyword.Replace("_~", "/") : null;
            if (criteria.RequirementGroup != null)
            {
                if (!string.IsNullOrEmpty(criteria.RequirementGroup.RequirementCode))
                {
                    criteria.RequirementGroup.RequirementCode = criteria.RequirementGroup.RequirementCode.Replace("_~", "/");
                }
            }
            if (!string.IsNullOrEmpty(criteria.RequirementCode))
            {
                criteria.RequirementCode = criteria.RequirementCode.Replace("_~", "/");
            }

            try
            {
                // Logging the timings for monitoring
                _logger.Info("Call Course Search Service from Courses controller... ");
                var watch = new Stopwatch();
                watch.Start();
                CoursePage coursesPage = await _courseService.SearchAsync(criteria, pageSize, pageIndex);
                watch.Stop();
                _logger.Info("Course search returned in: " + watch.ElapsedMilliseconds.ToString());
                return coursesPage;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Performs a search of courses in Colleague that are available for registration. 
        /// The criteria supplies a keyword, requirement, and various filters which may be used to search and narrow a list of courses.
        /// </summary>
        /// <param name="criteria"><see cref="CourseSearchCriteria">Course search criteria</see></param>
        /// <param name="pageSize">integer page size</param>
        /// <param name="pageIndex">integer page index</param>
        /// <returns>A <see cref="CoursePage">page</see> of courses matching criteria with totals and filter information.</returns>
        public async Task<CoursePage2> PostSearch2Async([FromBody]CourseSearchCriteria criteria, int pageSize, int pageIndex)
        {
            criteria.Keyword = criteria.Keyword != null ? criteria.Keyword.Replace("_~", "/") : null;
            if (criteria.RequirementGroup != null)
            {
                if (!string.IsNullOrEmpty(criteria.RequirementGroup.RequirementCode))
                {
                    criteria.RequirementGroup.RequirementCode = criteria.RequirementGroup.RequirementCode.Replace("_~", "/");
                }
            }
            if (!string.IsNullOrEmpty(criteria.RequirementCode))
            {
                criteria.RequirementCode = criteria.RequirementCode.Replace("_~", "/");
            }

            try
            {
                // Logging the timings for monitoring
                _logger.Info("Call Course Search Service from Courses controller... ");
                var watch = new Stopwatch();
                watch.Start();
                CoursePage2 coursesPage = await _courseService.Search2Async(criteria, pageSize, pageIndex);
                watch.Stop();
                _logger.Info("Course search returned in: " + watch.ElapsedMilliseconds.ToString());
                return coursesPage;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString() + ex.StackTrace);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Performs a search of sections that are available for Instant Enrollment only.
        /// The criteria supplied is keyword and various filters which may be used to search and narrow list of sections.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        /// <accessComments>Any authenticated user can perform search on sections and view Instant Enrollment catalog.</accessComments>
        public async Task<SectionPage> PostInstantEnrollmentCourseSearchAsync([FromBody]InstantEnrollmentCourseSearchCriteria criteria, int pageSize=10, int pageIndex=0)
        {
            try
            {
                // Logging the timings for monitoring
                _logger.Info("Call Course Search Service from Courses controller... ");
                var watch = new Stopwatch();
                watch.Start();
                SectionPage sectionPage = await _courseService.InstantEnrollmentSearchAsync(criteria, pageSize, pageIndex);
                watch.Stop();
                _logger.Info("Course search returned in: " + watch.ElapsedMilliseconds.ToString());
                return sectionPage;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString() + ex.StackTrace);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// For each course ID specified, this API will return the sections for this course that are offered in terms "open for registration". 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned from the repository.
        /// </summary>
        /// <param name="courseIds">a string of course Ids separated by commas</param>
        /// <returns>A list of <see cref="Section">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task< IEnumerable<Section>> GetCourseSectionsAsync(string courseIds)
        {
            List<string> courseList = new List<string>();
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                if (!String.IsNullOrEmpty(courseIds))
                {
                    string[] courseArray = courseIds.Split(new char[] { ',' });
                    foreach (var crs in courseArray)
                    {
                        courseList.Add(crs);
                    }
                }

                var privacyWrapper = await _courseService.GetSectionsAsync(courseList, useCache);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return sectionDtos;
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// For each course Id specified, this API will return the sections for this course that are offered in terms "open for registration". 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned from the repository.
        /// </summary>
        /// <param name="courseIds">a string of course Ids separated by commas</param>
        /// <returns>A list of <see cref="Section2">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 3 of this API")]
        public async Task<IEnumerable<Section2>> GetCourseSections2Async(string courseIds)
        {
            List<string> courseList = new List<string>();
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                if (!String.IsNullOrEmpty(courseIds))
                {
                    string[] courseArray = courseIds.Split(new char[] { ',' });
                    foreach (var crs in courseArray)
                    {
                        courseList.Add(crs);
                    }
                }

                var privacyWrapper = await _courseService.GetSections2Async(courseList, useCache);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return sectionDtos;
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// For each course Id specified, this API will return the sections for this course that are offered in terms "open for registration". 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned from the repository.
        /// </summary>
        /// <param name="courseIds">a string of course Ids separated by commas</param>
        /// <returns>A list of <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        public async Task<IEnumerable<Section3>> GetCourseSections3Async(string courseIds)
        {
            List<string> courseList = new List<string>();
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                if (!String.IsNullOrEmpty(courseIds))
                {
                    string[] courseArray = courseIds.Split(new char[] { ',' });
                    foreach (var crs in courseArray)
                    {
                        courseList.Add(crs);
                    }
                }

                var privacyWrapper = await _courseService.GetSections3Async(courseList, useCache);
                var sectionDtos = privacyWrapper.Dto as IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return sectionDtos;
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns courses given the list of course ids.
        /// </summary>
        /// <param name="id">course ID</param>
        /// <returns>A list of <see cref="Course">Courses</see></returns>
        [Obsolete("Obsolete as of API version 1.2, use version 2 of this API")]
        public async Task<List<Course>> GetAsync(string id)
        {

            List<Course> courseList = new List<Course>();
            if (!String.IsNullOrEmpty(id))
            {
                string[] courseIds = id.Split(new char[] { ',' });
                foreach (var crs in courseIds)
                {
                    if (!string.IsNullOrEmpty(crs))
                    {
                        courseList.Add(await _courseService.GetCourseByIdAsync(crs));
                    }
                }
            }
            return courseList;
        }

        /// <summary>
        /// Returns a course for the provided course id.
        /// </summary>
        /// <param name="courseId">course ID</param>
        /// <returns>The requested <see cref="Course">Course</see></returns>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task<Course> GetCourseAsync(string courseId)
        {
            return await _courseService.GetCourseByIdAsync(courseId);
        }

        /// <summary>
        /// Returns a course for the provided course id.
        /// </summary>
        /// <param name="courseId">course ID</param>
        /// <returns>The requested <see cref="Course">Course</see></returns>
        public async Task< Course2> GetCourse2Async(string courseId)
        {
            return await _courseService.GetCourseById2Async(courseId);
        }      

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a course for the provided Id.
        /// </summary>
        /// <param name="id">The Id of the course</param>
        /// <returns>The requested <see cref="Dtos.Course3">Course.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course3> GetHedmCourse3ByIdAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(id, "Course id cannot be null or empty");
            }
            try
            {
                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));
                return await _courseService.GetCourseByGuid3Async(id);
                
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a course for the provided Id.
        /// </summary>
        /// <param name="id">The GUID of the course</param>
        /// <returns>The requested <see cref="Dtos.Course4">Course.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course4> GetHedmCourse4ByIdAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(id, "Course id cannot be null or empty");
            }
            
            try
            {
                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));
                return await _courseService.GetCourseByGuid4Async(id);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves a course for the provided Id. v16
        /// </summary>
        /// <param name="id">The GUID of the course</param>
        /// <returns>The requested <see cref="Dtos.Course5">Course.</see></returns>
        [EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Ellucian.Colleague.Dtos.Course5> GetHedmCourse5ByIdAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(id, "Course id cannot be null or empty");
            }

            try
            {
                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));
                return await _courseService.GetCourseByGuid5Async(id);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Returns all or filtered list of courses
        /// </summary>
        /// <param name="page"></param>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevels"></param>
        /// <param name="owningInstitutionUnits"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="schedulingStartOn"></param>
        /// <param name="schedulingEndOn"></param>
        /// <returns>Filtered <see cref="Dtos.Course3">Courses.</see></returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "subject", "number", "academicLevels", "owningInstitutionUnits", "title", "instructionalMethods", "schedulingStartOn", "schedulingEndOn" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAllAndFilteredCourses3Async(Paging page, [FromUri] string subject = "", [FromUri] string number = "", [FromUri] string academicLevels = "", 
            [FromUri] string owningInstitutionUnits = "", [FromUri] string title = "", [FromUri] string instructionalMethods = "", [FromUri] string schedulingStartOn = "", [FromUri] string schedulingEndOn = "")
        {
            string criteria = string.Concat(subject, number, academicLevels, owningInstitutionUnits, title, instructionalMethods, schedulingStartOn, schedulingEndOn);

            //valid query parameter but empty argument
            if (!string.IsNullOrEmpty(criteria) && (string.IsNullOrEmpty(criteria.Replace("\"", "")) || string.IsNullOrEmpty(criteria.Replace("'", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Course3>>(new List<Dtos.Course3>(), page, 0, this.Request);
            }
            if (subject == null || subject == "null" || number == null || number == "null" || academicLevels == null ||
                academicLevels == "null" || owningInstitutionUnits == null || owningInstitutionUnits == "null" || 
                title == null || title == "null" || instructionalMethods == null || instructionalMethods == "null" || 
                schedulingStartOn == null || schedulingStartOn == "null" || schedulingEndOn == null || schedulingEndOn == "null")
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedHttpActionResult<IEnumerable<Dtos.Course3>>(new List<Dtos.Course3>(), page, 0, this.Request);

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
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _courseService.GetCourses3Async(page.Offset, page.Limit, bypassCache, subject, number, academicLevels, owningInstitutionUnits, title, instructionalMethods, schedulingStartOn, schedulingEndOn);
                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedHttpActionResult<IEnumerable<Dtos.Course3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns all or filtered list of courses
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"> Can contain: </param>
        /// subject, number, academicLevels, owningInstitutionUnits, title, instructionalMethods
        /// schedulingStartOn, schedulingEndOn
        /// <returns>Filtered <see cref="Dtos.Course4">Courses.</see></returns>
        [HttpGet]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Course4))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAllAndFilteredCourses4Async(Paging page, QueryStringFilter criteria)
        {
            string subject = string.Empty, number = string.Empty,  
                   title = string.Empty,  schedulingStartOn = string.Empty, schedulingEndOn = string.Empty;

            List<string> academicLevels = new List<string>(), owningInstitutionUnits = new List<string>(),
                instructionalMethods = new List<string>();

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
                    page = new Paging(200, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.Course4>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Course4>>(new List<Dtos.Course4>(), page, 0, this.Request);

                if (criteriaObj != null)
                {
                    subject = criteriaObj.Subject != null ? criteriaObj.Subject.Id : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    schedulingStartOn = criteriaObj.EffectiveStartDate != null && criteriaObj.EffectiveStartDate != default(DateTime)
                       ? criteriaObj.EffectiveStartDate.ToShortDateString() : string.Empty;
                    schedulingEndOn = criteriaObj.EffectiveEndDate != null ?
                        Convert.ToDateTime(criteriaObj.EffectiveEndDate).ToShortDateString() : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    instructionalMethods = criteriaObj.InstructionMethods != null ? ConvertGuidObject2ListToStringList(criteriaObj.InstructionMethods) : new List<string>();
                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningInstitutionUnits = organizations;
                    }
                }

                var pageOfItems = await _courseService.GetCourses4Async(page.Offset, page.Limit, bypassCache, subject, number, academicLevels, owningInstitutionUnits, title, instructionalMethods, schedulingStartOn, schedulingEndOn);
                
                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Course4>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns all or filtered list of courses v16
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"> Can contain: </param>
        /// subject, number, academicLevels, owningInstitutionUnits, title, instructionalMethods
        /// schedulingStartOn, schedulingEndOn
        ///  <param name="activeOn">named query</param>
        /// <returns>Filtered <see cref="Dtos.Course5">Courses.</see></returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Course5))]
        [QueryStringFilterFilter("activeOn", typeof(Dtos.Filters.ActiveOnFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAllAndFilteredCourses5Async(Paging page, QueryStringFilter criteria, QueryStringFilter activeOn)
        {
            string subject = string.Empty, number = string.Empty, topic = string.Empty,
                   title = string.Empty, schedulingStartOn = string.Empty, schedulingEndOn = string.Empty, activeOnDate = string.Empty; 

            List<string> academicLevels = new List<string>(), owningInstitutionUnits = new List<string>(),
                instructionalMethods = new List<string>(), titles = new List<string>(), categories = new List<string>();

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
                    page = new Paging(200, 0);
                }

                var activeOnObj = GetFilterObject<Dtos.Filters.ActiveOnFilter>(_logger, "activeOn");
                if (activeOnObj != null)
                {
                    activeOnDate = activeOnObj.ActiveOn != null ?
                                            activeOnObj.ActiveOn.Value.ToShortDateString() : string.Empty;
                }

                var criteriaObj = GetFilterObject<Dtos.Course5>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Course5>>(new List<Dtos.Course5>(), page, 0, this.Request);

                if (criteriaObj != null)
                {
                    if (criteriaObj.AdministrativePeriod != null || (criteriaObj.AdministrativePeriod != null && !string.IsNullOrEmpty(criteriaObj.AdministrativePeriod.Id)))
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.Course5>>(new List<Dtos.Course5>(), page, 0, this.Request);
                    }
                    subject = criteriaObj.Subject != null && !string.IsNullOrEmpty(criteriaObj.Subject.Id) ? criteriaObj.Subject.Id : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    topic = criteriaObj.Topic != null && !string.IsNullOrEmpty(criteriaObj.Topic.Id) ? criteriaObj.Topic.Id : string.Empty;
                    schedulingStartOn = criteriaObj.EffectiveStartDate != null ?
                        criteriaObj.EffectiveStartDate.Value.ToShortDateString() : string.Empty;
                    schedulingEndOn = criteriaObj.EffectiveEndDate != null ?
                        criteriaObj.EffectiveEndDate.Value.ToShortDateString() : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    categories = criteriaObj.Categories != null ? ConvertGuidObject2ListToStringList(criteriaObj.Categories) : new List<string>();
                    if (criteriaObj.InstructionalMethodDetails != null && criteriaObj.InstructionalMethodDetails.Any())
                    {
                        foreach(var method in criteriaObj.InstructionalMethodDetails)
                        {
                            if (method.InstructionalMethod != null && !string.IsNullOrEmpty(method.InstructionalMethod.Id))
                            {
                                instructionalMethods.Add(method.InstructionalMethod.Id);
                            }
                        }  
                    }
                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningInstitutionUnits = organizations;
                    }
                    if (criteriaObj.Titles != null && criteriaObj.Titles.Any())
                    {
                        foreach (var singleTitle in criteriaObj.Titles)
                        {
                            if (!string.IsNullOrEmpty(singleTitle.Value))
                            {
                                titles.Add(singleTitle.Value);
                            }
                        }
                    }
                }

                var pageOfItems = await _courseService.GetCourses5Async(page.Offset, page.Limit, bypassCache, subject, number, academicLevels, owningInstitutionUnits, titles, instructionalMethods, schedulingStartOn, schedulingEndOn, topic, categories, activeOnDate);

                AddEthosContextProperties(
                    await _courseService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Course5>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post the course
        /// </summary>
        /// <param name="course">The course to be created</param>
        /// <returns>The created <see cref="Dtos.Course3">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to create the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>
        /// 
        [HttpPost,EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course3> PostCourse3Async([ModelBinder(typeof(EedmModelBinder))] Ellucian.Colleague.Dtos.Course3 course)
        {
            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(course.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null course id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (course.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Non-empty course id not allowed in POST operation",
                    IntegrationApiUtility.GetDefaultApiError("You cannot update an existing course via POST.")));
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(await ExtractExtendedData(await _courseService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the course
                var courseReturn = await _courseService.CreateCourse3Async(course);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { courseReturn.Id }));

                return courseReturn;
            }
            catch (InvalidOperationException inex)
            {
                _logger.Error(inex.ToString());
                throw CreateHttpResponseException(inex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post the course
        /// </summary>
        /// <param name="course">The course to be created</param>
        /// <returns>The created <see cref="Dtos.Course4">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to create the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>
        
        [HttpPost,EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course4> PostCourse4Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Course4 course)
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(course.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null course id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (course.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Non-empty course id not allowed in POST operation",
                    IntegrationApiUtility.GetDefaultApiError("You cannot update an existing course via POST.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(await ExtractExtendedData(await _courseService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the course
                var courseReturn = await _courseService.CreateCourse4Async(course, false);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { courseReturn.Id }));

                return courseReturn;
                
            }
            catch (InvalidOperationException inex)
            {
                _logger.Error(inex.ToString());
                throw CreateHttpResponseException(inex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Post the course v16
        /// </summary>
        /// <param name="course">The course to be created</param>
        /// <returns>The created <see cref="Dtos.Course5">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to create the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>

        [HttpPost, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course5> PostCourse5Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Course5 course)
        {
            var bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(course.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null course id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (course.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Non-empty course id not allowed in POST operation",
                    IntegrationApiUtility.GetDefaultApiError("You cannot update an existing course via POST.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(await ExtractExtendedData(await _courseService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the course
                var courseReturn = await _courseService.CreateCourse5Async(course, bypassCache);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _courseService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { courseReturn.Id }));

                return courseReturn;

            }
            catch (InvalidOperationException inex)
            {
                _logger.Error(inex.ToString());
                throw CreateHttpResponseException(inex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ConfigurationException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
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
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update the course PUT v6
        /// </summary>
        /// <param name="id">Id for the course to be updated</param>
        /// <param name="course">The course to be updated</param>
        /// <returns>The updated <see cref="Dtos.Course3">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to update the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>
        [EedmResponseFilter,HttpPut]
        public async Task<Ellucian.Colleague.Dtos.Course3> PutCourse3Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Course3 course)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("id in URL was not supplied.", HttpStatusCode.BadRequest);
            }
            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(course.Id))
            {
                course.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != course.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException("id in URL is not the same as in request body.", HttpStatusCode.BadRequest);
            }

            try
            {
                //get Data Privacy List
                var dpList = await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _courseService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(course, async () => await _courseService.GetCourseByGuid3Async(id), dpList, _logger);
                var courseReturn = await _courseService.UpdateCourse3Async(partialmerged);

                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _courseService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return courseReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Update the course PUT v8
        /// </summary>
        /// <param name="id">Id for the course to be updated</param>
        /// <param name="course">The course to be updated</param>
        /// <returns>The updated <see cref="Dtos.Course4">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to update the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course4> PutCourse4Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Course4 course)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("id in URL was not supplied.", HttpStatusCode.BadRequest);
            }
            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(course.Id))
            {
                course.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != course.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException("id in URL is not the same as in request body.", HttpStatusCode.BadRequest);
            }

            try
            {

                //get Data Privacy List
                var dpList = await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _courseService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(course, async () => await _courseService.GetCourseByGuid4Async(id), dpList, _logger);
                var courseReturn = await _courseService.UpdateCourse4Async(partialmerged, false);
                
                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _courseService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return courseReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Update the course PUT v16
        /// </summary>
        /// <param name="id">Id for the course to be updated</param>
        /// <param name="course">The course to be updated</param>
        /// <returns>The updated <see cref="Dtos.Course4">Course</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to update the course</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the course is not provided.</exception>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.Course5> PutCourse5Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Course5 course)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException("id in URL was not supplied.", HttpStatusCode.BadRequest);
            }
            if (course == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Course.", HttpStatusCode.BadRequest);
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(course.Id))
            {
                course.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != course.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException("id in URL is not the same as in request body.", HttpStatusCode.BadRequest);
            }

            try
            {
                //get Data Privacy List
                var dpList = await _courseService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _courseService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _courseService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(course, async () => await _courseService.GetCourseByGuid5Async(id), dpList, _logger);
                var courseReturn = await _courseService.UpdateCourse5Async(partialmerged, bypassCache);

                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _courseService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return courseReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
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
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Accepts a list of course Id strings to post a query against courses.
        /// </summary>
        /// <param name="criteria"><see cref="CourseQueryCriteria">Query Criteria</see> including the list of Course Ids to use to retrieve courses.</param>
        /// <returns>List of <see cref="Course2">Course2</see> objects. </returns>
        [HttpPost]
        public async Task<IEnumerable<Course2>> QueryCoursesByPost2Async([FromBody]CourseQueryCriteria criteria)
        {
            try
            {
                return await _courseService.GetCourses2Async(criteria);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete (DELETE) a course
        /// </summary>
        /// <param name="id">Id to desired course</param>
        /// <returns>A course object <see cref="Dtos.Course2"/> in HeDM format</returns>
        [HttpDelete]
        public async Task<Ellucian.Colleague.Dtos.Course2> DeleteHedmCourseByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}