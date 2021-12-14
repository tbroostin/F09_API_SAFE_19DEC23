//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentTranscriptGrades
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTranscriptGradesController : BaseCompressedApiController
    {
        private readonly IStudentTranscriptGradesService _studentTranscriptGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTranscriptGradesController class.
        /// </summary>
        /// <param name="studentTranscriptGradesService">Service of type <see cref="IStudentTranscriptGradesService">IStudentTranscriptGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentTranscriptGradesController(IStudentTranscriptGradesService studentTranscriptGradesService, ILogger logger)
        {
            _studentTranscriptGradesService = studentTranscriptGradesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentTranscriptGrades
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">filter criteria</param>
        /// <returns>List of StudentTranscriptGrades <see cref="Dtos.StudentTranscriptGrades"/> objects representing matching studentTranscriptGrades</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentTranscriptGrades))]
        public async Task<IHttpActionResult> GetStudentTranscriptGradesAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            var criteriaFilter = GetFilterObject<Dtos.StudentTranscriptGrades>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentTranscriptGradesOptions>>(new List<Dtos.StudentTranscriptGradesOptions>(), page, 0, this.Request);

            try
            {
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentTranscriptGradesService.GetStudentTranscriptGradesAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                  await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentTranscriptGrades>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
            catch (InvalidOperationException e)
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
        /// Read (GET) a studentTranscriptGrades using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentTranscriptGrades, StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments })]
        public async Task<Dtos.StudentTranscriptGrades> GetStudentTranscriptGradesByGuidAsync(string guid)
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
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentTranscriptGradesService.GetStudentTranscriptGradesByGuidAsync(guid);
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
        /// Create (POST) a new studentTranscriptGrades
        /// </summary>
        /// <param name="studentTranscriptGrades">DTO of the new studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentTranscriptGrades> PostStudentTranscriptGradesAsync([FromBody] Dtos.StudentTranscriptGrades studentTranscriptGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing studentTranscriptGrades
        /// </summary>
        /// <param name="guid">GUID of the studentTranscriptGrades to update</param>
        /// <param name="studentTranscriptGrades">DTO of the updated studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGrades"/> in EEDM format</returns>
        [HttpPut]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentTranscriptGrades> PutStudentTranscriptGradesAsync([FromUri] string guid, [FromBody] Dtos.StudentTranscriptGrades studentTranscriptGrades)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentTranscriptGrades
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGrades</param>
        [HttpDelete]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task DeleteStudentTranscriptGradesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #region Adjustments

        /// <summary>
        /// Return all studentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentTranscriptGradesAdjustments <see cref="Dtos.StudentTranscriptGradesAdjustments"/> objects representing matching studentTranscriptGrades</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<IHttpActionResult> GetStudentTranscriptGradesAdjustmentsAsync(Paging page)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Get a studentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="guid">GUID to desired studentTranscriptGradesAdjustments</param>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task GetStudentTranscriptGradesAdjustmentsByGuidAsync(string guid)
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing StudentTranscriptGradesAdjustments
        /// </summary>
        /// <param name="guid">GUID of the studentTranscriptGradesAdjustments to update</param>
        /// <param name="studentTranscriptGradesAdjustments">DTO of the updated studentTranscriptGradesAdjustments</param>
        /// <returns>A StudentTranscriptGradesAdjustments object <see cref="Dtos.StudentTranscriptGradesAdjustments"/> in EEDM format</returns>
        [HttpPut] [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateStudentTranscriptGradesAdjustments)]
        public async Task<Dtos.StudentTranscriptGrades> PutStudentTranscriptGradesAdjustmentsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentTranscriptGradesAdjustments == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null studentTranscriptGradesAdjustments argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentTranscriptGradesAdjustments.Id))
            {
                studentTranscriptGradesAdjustments.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, studentTranscriptGradesAdjustments.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _studentTranscriptGradesService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _studentTranscriptGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                await _studentTranscriptGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentTranscriptGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var studentTranscriptGradesReturn = await _studentTranscriptGradesService.UpdateStudentTranscriptGradesAdjustmentsAsync(
                  await PerformPartialPayloadMerge(studentTranscriptGradesAdjustments, async () => await _studentTranscriptGradesService.GetStudentTranscriptGradesAdjustmentsByGuidAsync(guid, true),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _studentTranscriptGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentTranscriptGradesReturn;
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
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new studentTranscriptGrades
        /// </summary>
        /// <param name="studentTranscriptGradesAdjustments">DTO of the new studentTranscriptGrades</param>
        /// <returns>A studentTranscriptGrades object <see cref="Dtos.StudentTranscriptGradesAdjustments"/> in EEDM format</returns>
        [HttpPost]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentTranscriptGrades> PostStudentTranscriptGradesAdjustmentsAsync([FromBody] Dtos.StudentTranscriptGradesAdjustments studentTranscriptGradesAdjustments)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}