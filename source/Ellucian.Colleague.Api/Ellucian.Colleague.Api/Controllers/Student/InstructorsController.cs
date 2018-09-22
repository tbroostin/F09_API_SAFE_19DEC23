//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Instructors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructorsController : BaseCompressedApiController
    {
        private readonly IInstructorsService _instructorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructorsController class.
        /// </summary>
        /// <param name="instructorsService">Service of type <see cref="IInstructorsService">IInstructorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public InstructorsController(IInstructorsService instructorsService, ILogger logger)
        {
            _instructorsService = instructorsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all instructors
        /// </summary>
        /// <returns>List of Instructors <see cref="Dtos.Instructor"/> objects representing matching instructors</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructorFilter2)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter]
        public async Task<IHttpActionResult> GetInstructorsAsync(Paging page, QueryStringFilter criteria = null)
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
                    page = new Paging(200, 0);
                }
                
                var criteriaObj= GetFilterObject<Dtos.Filters.InstructorFilter2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters()) return new PagedHttpActionResult<IEnumerable<Dtos.Instructor>>(new List<Dtos.Instructor>(), page, 0, this.Request);

                string primaryLocationGuid = "";
                if (criteriaObj.PrimaryLocation != null)
                {
                    primaryLocationGuid = !string.IsNullOrEmpty(criteriaObj.PrimaryLocation.Id) ? criteriaObj.PrimaryLocation.Id : "";
                }

                string instructorGuid = "";
                if (criteriaObj.Instructor != null)
                {
                     instructorGuid = !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : "";
                }
                
                var pageOfItems = await _instructorsService.GetInstructorsAsync(page.Offset, page.Limit, instructorGuid, primaryLocationGuid, bypassCache);

                AddEthosContextProperties(
                                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Instructor>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentException e)
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
        /// Return all instructors
        /// </summary>
        /// <returns>List of Instructors <see cref="Dtos.Instructor2"/> objects representing matching instructors</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructorFilter2)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter]
        public async Task<IHttpActionResult> GetInstructors2Async(Paging page, QueryStringFilter criteria = null)
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
                    page = new Paging(200, 0);
                }

                var criteriaObj = GetFilterObject<Dtos.Filters.InstructorFilter2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters()) return new PagedHttpActionResult<IEnumerable<Dtos.Instructor2>>(new List<Dtos.Instructor2>(), page, 0, this.Request);

                string primaryLocationGuid = "";
                if (criteriaObj.PrimaryLocation != null)
                {
                    primaryLocationGuid = !string.IsNullOrEmpty(criteriaObj.PrimaryLocation.Id) ? criteriaObj.PrimaryLocation.Id : "";
                }

                string instructorGuid = "";
                if (criteriaObj.Instructor != null)
                {
                    instructorGuid = !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : "";
                }
                                
                var pageOfItems = await _instructorsService.GetInstructors2Async(page.Offset, page.Limit, instructorGuid, primaryLocationGuid, bypassCache);

                AddEthosContextProperties(
                  await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Instructor2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (ArgumentException e)
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
        /// Read (GET) a instructors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Instructor> GetInstructorsByGuidAsync(string guid)
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
                var instructor = await _instructorsService.GetInstructorByGuidAsync(guid);

                AddEthosContextProperties(
                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return instructor;
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a instructors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor2"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Instructor2> GetInstructorsByGuid2Async(string guid)
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
                
                var instructor = await _instructorsService.GetInstructorByGuid2Async(guid);

                AddEthosContextProperties(
                    await _instructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _instructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return instructor;
                
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new instructors
        /// </summary>
        /// <param name="instructor">DTO of the new instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructor"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.Instructor> PostInstructorsAsync([FromBody] Dtos.Instructor instructor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing instructors
        /// </summary>
        /// <param name="guid">GUID of the instructors to update</param>
        /// <param name="instructor">DTO of the updated instructors</param>
        /// <returns>A instructors object <see cref="Dtos.Instructors"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.Instructor> PutInstructorsAsync([FromUri] string guid, [FromBody] Dtos.Instructor instructor)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a instructors
        /// </summary>
        /// <param name="guid">GUID to desired instructors</param>
        [HttpDelete]
        public async Task DeleteInstructorsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}