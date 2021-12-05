// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// The controller for student financial aid awards for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Authorize]
    public class StudentFinancialAidAwardsController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidAwardService studentFinancialAidAwardService;
        private readonly ILogger _logger;

        /// <summary>
        /// This constructor initializes the StudentFinancialAidAwardController object
        /// </summary>
        /// <param name="studentFinancialAidAwardService">student financial aid awards service object</param>
        /// <param name="logger">Logger object</param>
        public StudentFinancialAidAwardsController(IStudentFinancialAidAwardService studentFinancialAidAwardService, ILogger logger)
        {
            this.studentFinancialAidAwardService = studentFinancialAidAwardService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        public async Task<Dtos.StudentFinancialAidAward> GetByIdAsync([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetByIdAsync(id, false);
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
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentFinancialAidAward2> GetById2Async([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetById2Async(id, false, bypassCache);
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
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }              

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAsync(Paging page)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                
                var pageOfItems = await studentFinancialAidAwardService.GetAsync(page.Offset, page.Limit, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> Get2Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, this.Request);

                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, string.Empty, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.1.0.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the non-restricted version using student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IHttpActionResult> Get3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, this.Request);

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }


                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache, false);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        public async Task<Dtos.StudentFinancialAidAward> GetRestrictedByIdAsync([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await studentFinancialAidAwardService.GetByIdAsync(id, true);
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
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a specified student financial aid award for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns>A StudentFinancialAidAward DTO</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentFinancialAidAward2> GetRestrictedById2Async([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await studentFinancialAidAwardService.GetById2Async(id, true, bypassCache);
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
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }        

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 7
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetRestrictedAsync(Paging page)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                var pageOfItems = await studentFinancialAidAwardService.GetAsync(page.Offset, page.Limit, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IHttpActionResult> GetRestricted2Async(Paging page, QueryStringFilter criteria)
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, this.Request);

                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, string.Empty, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student financial aid awards for the data model version 11.1.0.
        /// There is a restricted and a non-restricted view of financial aid awards.  This
        /// is the restricted version using restricted-student-financial-aid-awards.
        /// </summary>
        /// <returns>A Collection of StudentFinancialAidAwards</returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewRestrictedStudentFinancialAidAwards)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAward2))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IHttpActionResult> GetRestricted3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
        {
            try
            {
                studentFinancialAidAwardService.ValidatePermissions(GetPermissionsMetaData());
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(new List<Dtos.StudentFinancialAidAward2>(), page, 0, this.Request);

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.StudentFinancialAidAward2>(_logger, "criteria");

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }


                var pageOfItems = await studentFinancialAidAwardService.Get2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache, true);

                AddEthosContextProperties(
                    await studentFinancialAidAwardService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await studentFinancialAidAwardService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAward2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                _logger.Error(e, "Unknown error getting student financial aid award");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
        
        /// <summary>
        /// Update a single student financial aid award for the data model version 7 or 11
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <param name="studentFinancialAidAwardDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentFinancialAidAward</returns>
        [HttpPut]
        public async Task<Dtos.StudentFinancialAidAward> UpdateAsync([FromUri] string id, [FromBody] Dtos.StudentFinancialAidAward2 studentFinancialAidAwardDto)
        {
            //PUT is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a single student financial aid award for the data model version 7 or 11
        /// </summary>
        /// <param name="studentFinancialAidAwardDto">Student Financial Aid Award DTO from Body of request</param>
        /// <returns>A single StudentFinancialAidAward</returns>
        [HttpPost]
        public async Task<Dtos.StudentFinancialAidAward> CreateAsync([FromBody] Dtos.StudentFinancialAidAward2 studentFinancialAidAwardDto)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete a single student financial aid award for the data model version 6
        /// </summary>
        /// <param name="id">The requested student financial aid award GUID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}