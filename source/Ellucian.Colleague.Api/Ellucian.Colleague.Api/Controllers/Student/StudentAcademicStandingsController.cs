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
using Ellucian.Colleague.Coordination.Student;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAcademicStandings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicStandingsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicStandingsService _studentAcademicStandingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicStandingsController class.
        /// </summary>
        /// <param name="studentAcademicStandingsService">Service of type <see cref="IStudentAcademicStandingsService">IStudentAcademicStandingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAcademicStandingsController(IStudentAcademicStandingsService studentAcademicStandingsService, ILogger logger)
        {
            _studentAcademicStandingsService = studentAcademicStandingsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicStandings
        /// </summary>
        /// <returns>List of StudentAcademicStandings <see cref="Dtos.StudentAcademicStandings"/> objects representing matching studentAcademicStandings</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentAcademicStandingsAsync(Paging page)
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

                var pageOfItems = await _studentAcademicStandingsService.GetStudentAcademicStandingsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAcademicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAcademicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicStandings>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a studentAcademicStandings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAcademicStandings> GetStudentAcademicStandingsByGuidAsync(string guid)
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
                    await _studentAcademicStandingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAcademicStandingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAcademicStandingsService.GetStudentAcademicStandingsByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicStandings
        /// </summary>
        /// <param name="studentAcademicStandings">DTO of the new studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAcademicStandings> PostStudentAcademicStandingsAsync([FromBody] Dtos.StudentAcademicStandings studentAcademicStandings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicStandings
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicStandings to update</param>
        /// <param name="studentAcademicStandings">DTO of the updated studentAcademicStandings</param>
        /// <returns>A studentAcademicStandings object <see cref="Dtos.StudentAcademicStandings"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAcademicStandings> PutStudentAcademicStandingsAsync([FromUri] string guid, [FromBody] Dtos.StudentAcademicStandings studentAcademicStandings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicStandings
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicStandings</param>
        [HttpDelete]
        public async Task DeleteStudentAcademicStandingsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}