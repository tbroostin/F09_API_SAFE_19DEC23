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
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

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
        /// <returns>List of StudentMealPlans <see cref="Dtos.StudentMealPlans"/> objects representing matching studentMealPlans</returns>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentMealPlansAsync(Paging page)
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

                var pageOfItems = await _studentMealPlansService.GetStudentMealPlansAsync(page.Offset, page.Limit, bypassCache);

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
        /// Read (GET) a studentMealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
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
        /// Create (POST) a new studentMealPlans
        /// </summary>
        /// <param name="studentMealPlans">DTO of the new studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentMealPlans> PostStudentMealPlansAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentMealPlans studentMealPlans)
        {
            if (studentMealPlans == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null StudentMealPlans argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
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
        /// Update (PUT) an existing studentMealPlans
        /// </summary>
        /// <param name="guid">GUID of the studentMealPlans to update</param>
        /// <param name="studentMealPlans">DTO of the updated studentMealPlans</param>
        /// <returns>A studentMealPlans object <see cref="Dtos.StudentMealPlans"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
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
                //get Data Privacy List
                var dpList = await _studentMealPlansService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _studentMealPlansService.ImportExtendedEthosData(await ExtractExtendedData(await _studentMealPlansService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var studentMealPlanReturn = await _studentMealPlansService.PutStudentMealPlansAsync(guid,
                    await PerformPartialPayloadMerge(studentMealPlans, async () => await _studentMealPlansService.GetStudentMealPlansByGuidAsync(guid),
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