//Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicPeriods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicPeriodsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicPeriodsService _studentAcademicPeriodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicPeriodsController class.
        /// </summary>
        /// <param name="studentAcademicPeriodsService">Service of type <see cref="IStudentAcademicPeriodsService">IStudentAcademicPeriodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAcademicPeriodsController(IStudentAcademicPeriodsService studentAcademicPeriodsService, ILogger logger)
        {
            _studentAcademicPeriodsService = studentAcademicPeriodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicPeriods
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of StudentAcademicPeriods <see cref="Dtos.StudentAcademicPeriods"/> objects representing matching studentAcademicPeriods</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriods)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicPeriods))]
        public async Task<IHttpActionResult> GetStudentAcademicPeriodsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _studentAcademicPeriodsService.ValidatePermissions(GetPermissionsMetaData());
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }
                
                var filters = GetFilterObject<Dtos.StudentAcademicPeriods>(_logger, "criteria");
               
                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicPeriods>>(new List<Dtos.StudentAcademicPeriods>(), page, 0, this.Request);

                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _studentAcademicPeriodsService.GetStudentAcademicPeriodsAsync(page.Offset, page.Limit, 
                    personFilterValue, filters, bypassCache);

                AddEthosContextProperties(
                  await _studentAcademicPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentAcademicPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicPeriods>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a studentAcademicPeriods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewStudentAcademicPeriods)]
        public async Task<Dtos.StudentAcademicPeriods> GetStudentAcademicPeriodsByGuidAsync(string guid)
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
                _studentAcademicPeriodsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _studentAcademicPeriodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAcademicPeriodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAcademicPeriodsService.GetStudentAcademicPeriodsByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicPeriods
        /// </summary>
        /// <param name="studentAcademicPeriods">DTO of the new studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAcademicPeriods> PostStudentAcademicPeriodsAsync([FromBody] Dtos.StudentAcademicPeriods studentAcademicPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicPeriods
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicPeriods to update</param>
        /// <param name="studentAcademicPeriods">DTO of the updated studentAcademicPeriods</param>
        /// <returns>A studentAcademicPeriods object <see cref="Dtos.StudentAcademicPeriods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAcademicPeriods> PutStudentAcademicPeriodsAsync([FromUri] string guid, [FromBody] Dtos.StudentAcademicPeriods studentAcademicPeriods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicPeriods
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicPeriods</param>
        [HttpDelete]
        public async Task DeleteStudentAcademicPeriodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}