//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentMealPlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class StudentMealPlansController : BaseCompressedApiController
    {
        private readonly IStudentMealPlansService _studentMealPlansService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentMealPlansController class.
        /// </summary>
        /// <param name="studentMealPlansService">Service of type <see cref="IStudentMealPlansService">IStudentMealPlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentMealPlansController(IStudentMealPlansService studentMealPlansService, ILogger logger)
        {
            _studentMealPlansService = studentMealPlansService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentMealPlans
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of StudentMealPlans <see cref="Dtos.StudentMealPlans"/> objects representing matching studentMealPlans</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentMealPlans))]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentMealPlansAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.StudentMealPlans>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentMealPlans>>(new List<Dtos.StudentMealPlans>(), page, 0, this.Request);
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentMealPlansService.GetStudentMealPlansAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentMealPlans>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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

        #region 16.0.0

        /// <summary>
        /// Return all studentMealPlans
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of StudentMealPlans <see cref="Dtos.StudentMealPlans2"/> objects representing matching studentMealPlans</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentMealPlans2))]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentMealPlans2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.StudentMealPlans2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentMealPlans2>>(new List<Dtos.StudentMealPlans2>(), page, 0, this.Request);

            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _studentMealPlansService.GetStudentMealPlans2Async(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentMealPlans2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a studentMealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        public async Task<Dtos.StudentMealPlans2> GetStudentMealPlansByGuid2Async(string guid)
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
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                var mealPlans = await _studentMealPlansService.GetStudentMealPlansByGuid2Async(guid);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { mealPlans.Id }));
                }
                return mealPlans;
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
        /// Update (PUT) an existing studentMealPlans
        /// </summary>
        /// <param name="guid">GUID of the studentMealPlans to update</param>
        /// <param name="studentMealPlans2">DTO of the updated studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        public async Task<Dtos.StudentMealPlans2> PutStudentMealPlans2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans2 studentMealPlans2)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentMealPlans2 == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null  studentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentMealPlans2.Id))
            {
                studentMealPlans2.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(studentMealPlans2.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != studentMealPlans2.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.StudentMealPlans2 existingMealPlan = null;
                try
                {
                    existingMealPlan = await _studentMealPlansService.GetStudentMealPlansByGuid2Async(guid);
                    if (studentMealPlans2.Consumption != null)
                    {
                        existingMealPlan.Consumption = studentMealPlans2.Consumption;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An exception occurred while reading the Student Meal Plan.");
                }

                //do update with partial logic
                var studentMealPlanReturn = await _studentMealPlansService.PutStudentMealPlans2Async(guid,
                    await PerformPartialPayloadMerge(studentMealPlans2, existingMealPlan,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentMealPlanReturn;

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
        /// Create (POST) a new studentMealPlans
        /// </summary>
        /// <param name="studentMealPlans2">DTO of the new studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter( ErrorContentType = IntegrationErrors2 )]
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        public async Task<Dtos.StudentMealPlans2> PostStudentMealPlans2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans2 studentMealPlans2)
        {
            if (studentMealPlans2 == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null StudentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student meal plan
                var mealPlanReturn = await _studentMealPlansService.PostStudentMealPlans2Async(studentMealPlans2);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

                return mealPlanReturn;
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

        #endregion

        /// <summary>
        /// Read (GET) a studentMealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewMealPlanAssignment, StudentPermissionCodes.CreateMealPlanAssignment })]
        public async Task<Dtos.StudentMealPlans> GetStudentMealPlansByGuidAsync(string guid)
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
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                var mealPlans = await _studentMealPlansService.GetStudentMealPlansByGuidAsync(guid);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { mealPlans.Id }));
                }
                return mealPlans;
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
        /// Create (POST) a new studentMealPlans
        /// </summary>
        /// <param name="studentMealPlans">DTO of the new studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans2"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        public async Task<Dtos.StudentMealPlans> PostStudentMealPlansAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans studentMealPlans)
        {
            if (studentMealPlans == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null StudentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the student meal plan
                var mealPlanReturn = await _studentMealPlansService.PostStudentMealPlansAsync(studentMealPlans);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

                return mealPlanReturn;
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
        /// Update (PUT) an existing studentMealPlans
        /// </summary>
        /// <param name="guid">GUID of the studentMealPlans to update</param>
        /// <param name="studentMealPlans">DTO of the updated studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateMealPlanAssignment)]
        public async Task<Dtos.StudentMealPlans> PutStudentMealPlansAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans studentMealPlans)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentMealPlans == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null  studentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(studentMealPlans.Id))
            {
                studentMealPlans.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(studentMealPlans.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != studentMealPlans.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                _studentMealPlansService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                Dtos.StudentMealPlans existingMealPlan = null;
                try
                {
                    existingMealPlan = await _studentMealPlansService.GetStudentMealPlansByGuidAsync(guid);
                    if (studentMealPlans.Consumption != null)
                    {
                        existingMealPlan.Consumption = studentMealPlans.Consumption;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An exception occurred while reading the Student Meal Plan.");
                }

                //do update with partial logic
                var studentMealPlanReturn = await _studentMealPlansService.PutStudentMealPlansAsync(guid,
                    await PerformPartialPayloadMerge(studentMealPlans, existingMealPlan,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _studentMealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentMealPlanReturn;

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
        /// Delete (DELETE) a studentMealPlans
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        [HttpDelete]
        public async Task DeleteStudentMealPlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}