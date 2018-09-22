//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentFinancialAidAcademicProgressStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidAcademicProgressStatusesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidAcademicProgressStatusesService _studentFinancialAidAcademicProgressStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentFinancialAidAcademicProgressStatusesController class.
        /// </summary>
        /// <param name="studentFinancialAidAcademicProgressStatusesService">Service of type <see cref="IStudentFinancialAidAcademicProgressStatusesService">IStudentFinancialAidAcademicProgressStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentFinancialAidAcademicProgressStatusesController(IStudentFinancialAidAcademicProgressStatusesService studentFinancialAidAcademicProgressStatusesService, ILogger logger)
        {
            _studentFinancialAidAcademicProgressStatusesService = studentFinancialAidAcademicProgressStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>        
        /// <param name="criteria">StudentFinancialAidAcademicProgressStatuses search criteria in JSON format.</param>  
        /// <returns>List of StudentFinancialAidAcademicProgressStatuses <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> objects representing matching studentFinancialAidAcademicProgressStatuses</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentFinancialAidAcademicProgressStatuses))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetStudentFinancialAidAcademicProgressStatusesAsync(Paging page, QueryStringFilter criteria)
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

                var criteriaObject = GetFilterObject<Dtos.StudentFinancialAidAcademicProgressStatuses>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>>(new List<Dtos.StudentFinancialAidAcademicProgressStatuses>(),
                        page, 0, this.Request);

                
                var pageOfItems = await _studentFinancialAidAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesAsync(page.Offset, page.Limit, criteriaObject, bypassCache);

                AddEthosContextProperties(
                  await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _studentFinancialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentFinancialAidAcademicProgressStatuses>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Read (GET) a studentFinancialAidAcademicProgressStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentFinancialAidAcademicProgressStatuses> GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _studentFinancialAidAcademicProgressStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _studentFinancialAidAcademicProgressStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _studentFinancialAidAcademicProgressStatusesService.GetStudentFinancialAidAcademicProgressStatusesByGuidAsync(guid);
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Create (POST) a new studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="studentFinancialAidAcademicProgressStatuses">DTO of the new studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentFinancialAidAcademicProgressStatuses> PostStudentFinancialAidAcademicProgressStatusesAsync([FromBody] Dtos.StudentFinancialAidAcademicProgressStatuses studentFinancialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID of the studentFinancialAidAcademicProgressStatuses to update</param>
        /// <param name="studentFinancialAidAcademicProgressStatuses">DTO of the updated studentFinancialAidAcademicProgressStatuses</param>
        /// <returns>A studentFinancialAidAcademicProgressStatuses object <see cref="Dtos.StudentFinancialAidAcademicProgressStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentFinancialAidAcademicProgressStatuses> PutStudentFinancialAidAcademicProgressStatusesAsync([FromUri] string guid, [FromBody] Dtos.StudentFinancialAidAcademicProgressStatuses studentFinancialAidAcademicProgressStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentFinancialAidAcademicProgressStatuses
        /// </summary>
        /// <param name="guid">GUID to desired studentFinancialAidAcademicProgressStatuses</param>
        [HttpDelete]
        public async Task DeleteStudentFinancialAidAcademicProgressStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}