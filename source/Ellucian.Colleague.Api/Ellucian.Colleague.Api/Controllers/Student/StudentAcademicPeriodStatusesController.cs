//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriodStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodStatusesController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodStatusesService _studentAcademicPeriodStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodStatusesController class.
        /// </summary>
        /// <param name="studentAcademicPeriodStatusesService">Service of type <see cref="IStudentAcademicPeriodStatusesService">IStudentAcademicPeriodStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAcademicPeriodStatusesController(IStudentAcademicPeriodStatusesService studentAcademicPeriodStatusesService, ILogger logger)
        {
            _studentAcademicPeriodStatusesService = studentAcademicPeriodStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicPeriodStatuses
        /// </summary>
        /// <returns>List of StudentAcademicPeriodStatuses <see cref="Dtos.StudentAcademicPeriodStatuses"/> objects representing matching studentAcademicPeriodStatuses</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriodStatuses)), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPeriodStatuses>> GetStudentAcademicPeriodStatusesAsync(QueryStringFilter criteria)
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
                var filter = GetFilterObject<Dtos.StudentAcademicPeriodStatuses>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                {
                    return new List<Dtos.StudentAcademicPeriodStatuses>(new List<Dtos.StudentAcademicPeriodStatuses>());
                }
                if (filter.Usages != null && filter.Usages.Count > 1)
                {
                    return new List<Dtos.StudentAcademicPeriodStatuses>(new List<Dtos.StudentAcademicPeriodStatuses>());
                }

                var studentAcademicPeriodStatuses = await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(bypassCache);

                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any())
                {
                    AddEthosContextProperties(await _studentAcademicPeriodStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _studentAcademicPeriodStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              studentAcademicPeriodStatuses.Select(a => a.Id).ToList()));
                }
                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any() && !string.IsNullOrEmpty(filter.Code))
                {
                    studentAcademicPeriodStatuses = studentAcademicPeriodStatuses.Where(saps => saps.Code.Equals(filter.Code, StringComparison.OrdinalIgnoreCase));
                }

                if (studentAcademicPeriodStatuses != null && studentAcademicPeriodStatuses.Any() 
                    && (filter.Usages != null) && (filter.Usages.Any()))
                {
                    studentAcademicPeriodStatuses =  studentAcademicPeriodStatuses
                         .Where(p => filter.Usages.Any(a => p.Usages != null && p.Usages.Contains(a)));
                }

                return studentAcademicPeriodStatuses;
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
        /// Read (GET) a studentAcademicPeriodStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAcademicPeriodStatuses> GetStudentAcademicPeriodStatusesByGuidAsync(string guid)
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
                   await _studentAcademicPeriodStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAcademicPeriodStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="studentAcademicPeriodStatuses">DTO of the new studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAcademicPeriodStatuses> PostStudentAcademicPeriodStatusesAsync([FromBody] Dtos.StudentAcademicPeriodStatuses studentAcademicPeriodStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicPeriodStatuses to update</param>
        /// <param name="studentAcademicPeriodStatuses">DTO of the updated studentAcademicPeriodStatuses</param>
        /// <returns>A studentAcademicPeriodStatuses object <see cref="Dtos.StudentAcademicPeriodStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAcademicPeriodStatuses> PutStudentAcademicPeriodStatusesAsync([FromUri] string guid, [FromBody] Dtos.StudentAcademicPeriodStatuses studentAcademicPeriodStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicPeriodStatuses
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriodStatuses</param>
        [HttpDelete]
        public async Task DeleteStudentAcademicPeriodStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}