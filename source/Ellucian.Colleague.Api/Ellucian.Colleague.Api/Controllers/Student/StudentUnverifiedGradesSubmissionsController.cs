﻿//Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

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
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Net.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentUnverifiedGradesSubmissions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentUnverifiedGradesSubmissionsController : BaseCompressedApiController
    {
        private readonly IStudentUnverifiedGradesService _studentUnverifiedGradesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentUnverifiedGradesSubmissionsController class.
        /// </summary>
        /// <param name="studentUnverifiedGradesService">Service of type <see cref="IStudentUnverifiedGradesService">IStudentUnverifiedGradesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentUnverifiedGradesSubmissionsController(IStudentUnverifiedGradesService studentUnverifiedGradesService, ILogger logger)
        {
            _studentUnverifiedGradesService = studentUnverifiedGradesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentUnverifiedGradesSubmissions <see cref="Dtos.StudentUnverifiedGradesSubmissions"/> objects representing matching studentUnverifiedGradesSubmissions</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetStudentUnverifiedGradesSubmissionsAsync(Paging page)
        {
            //Get is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Read (GET) a studentUnverifiedGradesSubmissions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGradesSubmissions</param>
        /// <returns>A studentUnverifiedGradesSubmissions object <see cref="Dtos.StudentUnverifiedGradesSubmissions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.StudentUnverifiedGradesSubmissions> GetStudentUnverifiedGradesSubmissionsByGuidAsync(string guid)
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
                return await _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(guid);
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
        /// Update (PUT) an existing StudentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="guid">GUID of the studentUnverifiedGradesSubmissions to update</param>
        /// <param name="studentUnverifiedGradesSubmissions">DTO of the updated studentUnverifiedGradesSubmissions</param>
        /// <returns>A StudentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions)]
        public async Task<Dtos.StudentUnverifiedGrades> PutStudentUnverifiedGradesSubmissionsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
           
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentUnverifiedGradesSubmissions == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null studentUnverifiedGradesSubmissions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentUnverifiedGradesSubmissions.Id))
            {
                studentUnverifiedGradesSubmissions.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, studentUnverifiedGradesSubmissions.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                var dpList = await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Save incoming Last Attendance info for later comparison after partial put logic
                DateTime? submittedLastAttendanceDate = null;
                Dtos.EnumProperties.StudentUnverifiedGradesStatus submittedLastAttendanceStatus = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet;
                if (studentUnverifiedGradesSubmissions.LastAttendance != null)
                {
                    if (studentUnverifiedGradesSubmissions.LastAttendance.Date != null)
                    {
                        submittedLastAttendanceDate = studentUnverifiedGradesSubmissions.LastAttendance.Date;
                    }
                    if (studentUnverifiedGradesSubmissions.LastAttendance.Status == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended)
                    {
                        submittedLastAttendanceStatus = Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended;
                    }
                }
                
                var studentUnverifiedGradesSubmissionsDto = _studentUnverifiedGradesService.GetStudentUnverifiedGradesSubmissionsByGuidAsync(guid, true);
                var studentUnverifiedGradesDto = (await PerformPartialPayloadMerge(studentUnverifiedGradesSubmissions, async () => await studentUnverifiedGradesSubmissionsDto, dpList, _logger));

                // If we received a last attend date and no last attend status, clear out the status if it already exists.
                if ((submittedLastAttendanceDate != null) && (submittedLastAttendanceStatus == Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet))
                {
                    if (studentUnverifiedGradesDto.LastAttendance != null)
                    {
                        if (studentUnverifiedGradesDto.LastAttendance.Status == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended)
                        {
                            studentUnverifiedGradesDto.LastAttendance.Status = Dtos.EnumProperties.StudentUnverifiedGradesStatus.NotSet;
                        }
                    }
                }
                // If we received a last attend status and no last attend date, clear the date if it already exists.
                if ((submittedLastAttendanceStatus == Dtos.EnumProperties.StudentUnverifiedGradesStatus.Neverattended) && submittedLastAttendanceDate == null)
                {
                    if (studentUnverifiedGradesDto.LastAttendance != null)
                    {
                        if (studentUnverifiedGradesDto.LastAttendance.Date != null)
                        {
                            studentUnverifiedGradesDto.LastAttendance.Date = null;
                        }
                    }
                }

                await _studentUnverifiedGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentUnverifiedGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var studentUnverifiedGradesReturn = await _studentUnverifiedGradesService.UpdateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesDto);

                AddEthosContextProperties(dpList,
                    await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return studentUnverifiedGradesReturn;
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
        /// Create (POST) a new studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="studentUnverifiedGradesSubmissions">DTO of the new studentUnverifiedGradesSubmissions</param>
        /// <returns>A studentUnverifiedGrades object <see cref="Dtos.StudentUnverifiedGrades"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, PermissionsFilter(StudentPermissionCodes.ViewStudentUnverifiedGradesSubmissions)]
        public async Task<Dtos.StudentUnverifiedGrades> PostStudentUnverifiedGradesSubmissionsAsync(Dtos.StudentUnverifiedGradesSubmissions studentUnverifiedGradesSubmissions)
        {
            
            if (studentUnverifiedGradesSubmissions == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid studentUnverifiedGradesSubmissions.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(studentUnverifiedGradesSubmissions.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null studentUnverifiedGradesSubmissions id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (studentUnverifiedGradesSubmissions.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                _studentUnverifiedGradesService.ValidatePermissions(GetPermissionsMetaData());
                await _studentUnverifiedGradesService.ImportExtendedEthosData(await ExtractExtendedData(await _studentUnverifiedGradesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                var response = await _studentUnverifiedGradesService.CreateStudentUnverifiedGradesSubmissionsAsync(studentUnverifiedGradesSubmissions);
                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _studentUnverifiedGradesService.GetDataPrivacyListByApi(GetRouteResourceName(), true),

                await _studentUnverifiedGradesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { response.Id }));

                return response;
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a studentUnverifiedGradesSubmissions
        /// </summary>
        /// <param name="guid">GUID to desired studentUnverifiedGradesSubmissions</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteStudentUnverifiedGradesSubmissionsAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}