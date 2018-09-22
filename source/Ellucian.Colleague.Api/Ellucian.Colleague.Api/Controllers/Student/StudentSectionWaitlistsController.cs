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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentSectionWaitlists
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentSectionWaitlistsController : BaseCompressedApiController
    {
        private readonly IStudentSectionWaitlistsService _studentSectionWaitlistsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentSectionWaitlistsController class.
        /// </summary>
        /// <param name="studentSectionWaitlistsService">Service of type <see cref="IStudentSectionWaitlistsService">IStudentSectionWaitlistsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentSectionWaitlistsController(IStudentSectionWaitlistsService studentSectionWaitlistsService, ILogger logger)
        {
            _studentSectionWaitlistsService = studentSectionWaitlistsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentSectionWaitlists
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of StudentSectionWaitlists <see cref="Dtos.StudentSectionWaitlist"/> objects representing matching studentSectionWaitlists</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentSectionWaitlistsAsync(Paging page)
        {
            var bypassCache = true;
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

                var pageOfItems = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentSectionWaitlistsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentSectionWaitlistsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentSectionWaitlist>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a studentSectionWaitlists using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlist"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentSectionWaitlist> GetStudentSectionWaitlistsByGuidAsync(string guid)
        {
            var bypassCache = true;
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
                var item = await _studentSectionWaitlistsService.GetStudentSectionWaitlistsByGuidAsync(guid);

                if (item != null)
                {

                    AddEthosContextProperties(await _studentSectionWaitlistsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _studentSectionWaitlistsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return item;
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
        /// Create (POST) a new studentSectionWaitlists
        /// </summary>
        /// <param name="studentSectionWaitlists">DTO of the new studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlists"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentSectionWaitlist> PostStudentSectionWaitlistsAsync([FromBody] Dtos.StudentSectionWaitlist studentSectionWaitlists)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing studentSectionWaitlists
        /// </summary>
        /// <param name="guid">GUID of the studentSectionWaitlists to update</param>
        /// <param name="studentSectionWaitlists">DTO of the updated studentSectionWaitlists</param>
        /// <returns>A studentSectionWaitlists object <see cref="Dtos.StudentSectionWaitlists"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentSectionWaitlist> PutStudentSectionWaitlistsAsync([FromUri] string guid, [FromBody] Dtos.StudentSectionWaitlist studentSectionWaitlists)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a studentSectionWaitlists
        /// </summary>
        /// <param name="guid">GUID to desired studentSectionWaitlists</param>
        [HttpDelete]
        public async Task DeleteStudentSectionWaitlistsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}