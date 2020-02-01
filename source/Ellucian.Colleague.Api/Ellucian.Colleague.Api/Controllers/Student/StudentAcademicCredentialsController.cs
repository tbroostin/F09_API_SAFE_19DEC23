//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
    /// Provides access to StudentAcademicCredentials
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAcademicCredentialsController : BaseCompressedApiController
    {
        private readonly IStudentAcademicCredentialsService _studentAcademicCredentialsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAcademicCredentialsController class.
        /// </summary>
        /// <param name="studentAcademicCredentialsService">Service of type <see cref="IStudentAcademicCredentialsService">IStudentAcademicCredentialsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAcademicCredentialsController(IStudentAcademicCredentialsService studentAcademicCredentialsService, ILogger logger)
        {
            _studentAcademicCredentialsService = studentAcademicCredentialsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAcademicCredentials
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>        
        /// <param name="criteria">Filtering Criteria</param>
        /// <param name="personFilter">personFilter Named Query</param>
        /// <param name="academicPrograms">academicPrograms Named Query</param>
        /// <returns>List of StudentAcademicCredentials <see cref="Dtos.StudentAcademicCredentials"/> objects representing matching studentAcademicCredentials</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAcademicCredentials))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("academicPrograms", typeof(Dtos.Filters.AcademicProgramsFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetStudentAcademicCredentialsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter, 
                                                                                QueryStringFilter academicPrograms)
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

                var filterCriteria = GetFilterObject<Dtos.StudentAcademicCredentials>(_logger, "criteria");
                var personFilterFilter = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                var academicProgramsFilter = GetFilterObject<Dtos.Filters.AcademicProgramsFilter>(_logger, "academicPrograms");

                if (CheckForEmptyFilterParameters())
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicCredentials>>(new List<Dtos.StudentAcademicCredentials>(), page, 0, this.Request);
                }

                Dictionary<string, string> filterQualifiers = GetFilterQualifiers(_logger);

                var pageOfItems = await _studentAcademicCredentialsService.GetStudentAcademicCredentialsAsync(page.Offset, page.Limit, filterCriteria, personFilterFilter,
                    academicProgramsFilter, filterQualifiers, bypassCache);

                AddEthosContextProperties(
                  await _studentAcademicCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentAcademicCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAcademicCredentials>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a studentAcademicCredentials using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicCredentials</param>
        /// <returns>A studentAcademicCredentials object <see cref="Dtos.StudentAcademicCredentials"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAcademicCredentials> GetStudentAcademicCredentialsByGuidAsync(string guid)
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
                   await _studentAcademicCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentAcademicCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentAcademicCredentialsService.GetStudentAcademicCredentialsByGuidAsync(guid);
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
        /// Create (POST) a new studentAcademicCredentials
        /// </summary>
        /// <param name="studentAcademicCredentials">DTO of the new studentAcademicCredentials</param>
        /// <returns>A studentAcademicCredentials object <see cref="Dtos.StudentAcademicCredentials"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAcademicCredentials> PostStudentAcademicCredentialsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicCredentials studentAcademicCredentials)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentAcademicCredentials
        /// </summary>
        /// <param name="guid">GUID of the studentAcademicCredentials to update</param>
        /// <param name="studentAcademicCredentials">DTO of the updated studentAcademicCredentials</param>
        /// <returns>A studentAcademicCredentials object <see cref="Dtos.StudentAcademicCredentials"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAcademicCredentials> PutStudentAcademicCredentialsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAcademicCredentials studentAcademicCredentials)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentAcademicCredentials
        /// </summary>
        /// <param name="guid">GUID to desired studentAcademicCredentials</param>
        [HttpDelete]
        public async Task DeleteStudentAcademicCredentialsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}