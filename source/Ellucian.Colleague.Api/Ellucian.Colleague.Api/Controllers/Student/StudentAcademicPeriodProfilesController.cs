// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using System.Threading.Tasks;
using System;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriodProfiles data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodProfilesController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodProfilesService _studentAcademicPeriodProfilesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodProfilesController class.
        /// </summary>
        /// <param name="studentAcademicPeriodProfilesService">Repository of type <see cref="IStudentAcademicPeriodProfilesService">IStudentAcademicPeriodProfilesService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentAcademicPeriodProfilesController(IStudentAcademicPeriodProfilesService studentAcademicPeriodProfilesService, ILogger logger)
        {
            _studentAcademicPeriodProfilesService = studentAcademicPeriodProfilesService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an Student Academic Period Profiles by ID.
        /// </summary>
        /// <returns>An <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<StudentAcademicPeriodProfiles> GetStudentAcademicPeriodProfileByGuidAsync(string id)
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
                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfileByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Return a list of StudentAcademicPeriodProfiles objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="person">Id (GUID) A reference to link a student to the common HEDM persons entity</param>     
        /// <param name="academicPeriod">Id (GUID) A term within an academic year (for example, Semester).</param>
        /// <returns>List of StudentAcademicPeriodProfiles <see cref="StudentAcademicPeriodProfiles"/> objects representing matching Student Academic Period Profiles</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 50), EedmResponseFilter]
        [ValidateQueryStringFilter(new string[] { "person", "academicPeriod" }, false, true)]
        public async Task<IHttpActionResult> GetStudentAcademicPeriodProfilesAsync(Paging page, [FromUri] string person = "", [FromUri] string academicPeriod = "")
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
                    page = new Paging(50, 0);
                }                

                var pageOfItems = await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(page.Offset, page.Limit, bypassCache, person, academicPeriod);

                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<StudentAcademicPeriodProfiles>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            { 
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Return a list of StudentAcademicPeriodProfiles objects based on selection criteria.
        /// </summary>
        ///  <param name="page">page</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>List of StudentAcademicPeriodProfiles <see cref="StudentAcademicPeriodProfiles"/> objects representing matching Student Academic Period Profiles</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 50), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriodProfiles))]
        public async Task<IHttpActionResult> GetStudentAcademicPeriodProfiles2Async(Paging page, QueryStringFilter criteria = null)
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
                    page = new Paging(50, 0);
                }
                string person = string.Empty, academicPeriod = string.Empty;

                var criteriaObj = GetFilterObject<Dtos.StudentAcademicPeriodProfiles>(_logger, "criteria");

                if (criteriaObj != null)
                {
                    person = criteriaObj.Person != null && !string.IsNullOrEmpty(criteriaObj.Person.Id) ? criteriaObj.Person.Id : string.Empty;

                    academicPeriod = criteriaObj.AcademicPeriod != null && !string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Id) ?
                        criteriaObj.AcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicPeriodProfiles>>(new List<Dtos.StudentAcademicPeriodProfiles>(), page, 0, this.Request);

                var pageOfItems = await _studentAcademicPeriodProfilesService.GetStudentAcademicPeriodProfilesAsync(page.Offset, page.Limit, bypassCache, person, academicPeriod);

                AddEthosContextProperties((await _studentAcademicPeriodProfilesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)),
                    await _studentAcademicPeriodProfilesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<StudentAcademicPeriodProfiles>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Creates a Student Academic Period Profile.
        /// </summary>
        /// <param name="studentAcademicPeriodProfiles"><see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see> to create</param>
        /// <returns>Newly created <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see></returns>
        [HttpPost]
        public async Task<StudentAcademicPeriodProfiles> CreateStudentAcademicPeriodProfilesAsync([ModelBinder(typeof(EedmModelBinder))] StudentAcademicPeriodProfiles studentAcademicPeriodProfiles)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Student Academic Period Profile.
        /// </summary>
        /// <param name="id">Id of the Student Academic Period Profile to update</param>
        /// <param name="studentAcademicPeriodProfiles"><see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see> to create</param>
        /// <returns>Updated <see cref="StudentAcademicPeriodProfiles">StudentAcademicPeriodProfiles</see></returns>
        [HttpPut]
        public async Task<StudentAcademicPeriodProfiles> UpdateStudentAcademicPeriodProfilesAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] StudentAcademicPeriodProfiles studentAcademicPeriodProfiles)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Student Academic Period Profiles.
        /// </summary>
        /// <param name="id">ID of the Student Academic Period Profile to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteStudentAcademicPeriodProfilesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}