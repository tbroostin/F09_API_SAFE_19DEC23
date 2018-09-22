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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAdvisorRelationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAdvisorRelationshipsController : BaseCompressedApiController
    {
        private readonly IStudentAdvisorRelationshipsService _studentAdvisorRelationshipsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAdvisorRelationshipsController class.
        /// </summary>
        /// <param name="studentAdvisorRelationshipsService">Service of type <see cref="IStudentAdvisorRelationshipsService">IStudentAdvisorRelationshipsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAdvisorRelationshipsController(IStudentAdvisorRelationshipsService studentAdvisorRelationshipsService, ILogger logger)
        {
            _studentAdvisorRelationshipsService = studentAdvisorRelationshipsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAdvisorRelationships
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filters to be used within this API. They must be in JSON and contain the following fields: student,advisor, advisorType and startAcademicPeriod</param>
        /// <returns>List of StudentAdvisorRelationships <see cref="Dtos.StudentAdvisorRelationships"/> objects representing matching studentAdvisorRelationships</returns>
        [HttpGet, EedmResponseFilter, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.StudentAdvisorRelationshipFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetStudentAdvisorRelationshipsAsync(Paging page, QueryStringFilter criteria)
        {
            string student = string.Empty, advisor = string.Empty, advisorType = string.Empty, startAcademicPeriod = string.Empty;

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

                var criteriaObj = GetFilterObject<Dtos.Filters.StudentAdvisorRelationshipFilter>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    student = criteriaObj.Student != null ? criteriaObj.Student.Id : string.Empty;
                    advisor = criteriaObj.Advisor != null ? criteriaObj.Advisor.Id : string.Empty;
                    advisorType = criteriaObj.AdvisorType != null ? criteriaObj.AdvisorType.Id : string.Empty;
                    startAcademicPeriod = criteriaObj.StartAcademicPeriod != null ? criteriaObj.StartAcademicPeriod.Id : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAdvisorRelationships>>(new List<Dtos.StudentAdvisorRelationships>(), page, 0, this.Request);

                var pageOfItems = await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsAsync(page.Offset, page.Limit, bypassCache,
                    student, advisor, advisorType, startAcademicPeriod);

                AddEthosContextProperties(
                  await _studentAdvisorRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentAdvisorRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAdvisorRelationships>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a studentAdvisorRelationships using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAdvisorRelationships> GetStudentAdvisorRelationshipsByGuidAsync(string guid)
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
                   await _studentAdvisorRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAdvisorRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAdvisorRelationshipsService.GetStudentAdvisorRelationshipsByGuidAsync(guid);
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
        /// Create (POST) a new studentAdvisorRelationships
        /// </summary>
        /// <param name="studentAdvisorRelationships">DTO of the new studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAdvisorRelationships> PostStudentAdvisorRelationshipsAsync([FromBody] Dtos.StudentAdvisorRelationships studentAdvisorRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAdvisorRelationships
        /// </summary>
        /// <param name="guid">GUID of the studentAdvisorRelationships to update</param>
        /// <param name="studentAdvisorRelationships">DTO of the updated studentAdvisorRelationships</param>
        /// <returns>A studentAdvisorRelationships object <see cref="Dtos.StudentAdvisorRelationships"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAdvisorRelationships> PutStudentAdvisorRelationshipsAsync([FromUri] string guid, [FromBody] Dtos.StudentAdvisorRelationships studentAdvisorRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAdvisorRelationships
        /// </summary>
        /// <param name="guid">GUID to desired studentAdvisorRelationships</param>
        [HttpDelete]
        public async Task DeleteStudentAdvisorRelationshipsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}