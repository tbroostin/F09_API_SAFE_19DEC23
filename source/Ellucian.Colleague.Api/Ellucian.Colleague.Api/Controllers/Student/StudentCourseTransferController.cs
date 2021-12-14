//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to StudentCourseTransfer
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentCourseTransferController : BaseCompressedApiController
    {
        private readonly IStudentCourseTransferService _StudentCourseTransferService;
        private readonly ILogger _logger;
        private int offset, limit;

        /// <summary>
        /// Initializes a new instance of the StudentCourseTransferController class.
        /// </summary>
        /// <param name="StudentCourseTransferService">Service of type <see cref="IStudentCourseTransferService">IStudentCourseTransferService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentCourseTransferController(IStudentCourseTransferService StudentCourseTransferService, ILogger logger)
        {
            _StudentCourseTransferService = StudentCourseTransferService;
            _logger = logger;
        }

        /// <summary>
        /// Return all StudentCourseTransfer
        /// </summary>
        /// <returns>List of StudentCourseTransfer <see cref="Dtos.StudentCourseTransfer"/> objects representing matching StudentCourseTransfer</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [EedmResponseFilter, PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentCourseTransfersAsync(Paging page, bool ignoreCache = false)
        {
            var bypassCache = false;
            

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            
            if (page == null)
            {
                offset = 0;
                limit = 100;
            }
            else
            {
                offset = page.Offset;
                limit = page.Limit;
            }
            try
            {
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _StudentCourseTransferService.GetStudentCourseTransfersAsync(offset, limit, bypassCache);

                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentCourseTransfer>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);


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
        /// Read (GET) a StudentCourseTransfer using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        public async Task<Dtos.StudentCourseTransfer> GetStudentCourseTransferByGuidAsync(string guid)
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
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _StudentCourseTransferService.GetStudentCourseTransferByGuidAsync(guid, bypassCache);
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
        /// Return all StudentCourseTransfer
        /// </summary>
        /// <returns>List of StudentCourseTransfer <see cref="Dtos.StudentCourseTransfer"/> objects representing matching StudentCourseTransfer</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        [EedmResponseFilter, PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentCourseTransfers2Async(Paging page, bool ignoreCache = false)
        {
            var bypassCache = false;


            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                offset = 0;
                limit = 100;
            }
            else
            {
                offset = page.Offset;
                limit = page.Limit;
            }
            try
            {
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                var pageOfItems = await _StudentCourseTransferService.GetStudentCourseTransfers2Async(offset, limit, bypassCache);

                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentCourseTransfer>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);


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
        /// Read (GET) a StudentCourseTransfer using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewStudentCourseTransfers })]
        public async Task<Dtos.StudentCourseTransfer> GetStudentCourseTransfer2ByGuidAsync(string guid)
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
                _StudentCourseTransferService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _StudentCourseTransferService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _StudentCourseTransferService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _StudentCourseTransferService.GetStudentCourseTransfer2ByGuidAsync(guid, bypassCache);
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
        /// Create (POST) a new StudentCourseTransfer
        /// </summary>
        /// <param name="StudentCourseTransfer">DTO of the new StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentCourseTransfer> PostStudentCourseTransferAsync([FromBody] Dtos.StudentCourseTransfer StudentCourseTransfer)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentCourseTransfer
        /// </summary>
        /// <param name="guid">GUID of the StudentCourseTransfer to update</param>
        /// <param name="StudentCourseTransfer">DTO of the updated StudentCourseTransfer</param>
        /// <returns>A StudentCourseTransfer object <see cref="Dtos.StudentCourseTransfer"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentCourseTransfer> PutStudentCourseTransferAsync([FromUri] string guid, [FromBody] Dtos.StudentCourseTransfer StudentCourseTransfer)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a StudentCourseTransfer
        /// </summary>
        /// <param name="guid">GUID to desired StudentCourseTransfer</param>
        [HttpDelete]
        public async Task DeleteStudentCourseTransferAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}