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
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItemStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AdmissionApplicationSupportingItemStatusesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemStatusesService _admissionApplicationSupportingItemStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemStatusesController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemStatusesService">Service of type <see cref="IAdmissionApplicationSupportingItemStatusesService">IAdmissionApplicationSupportingItemStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionApplicationSupportingItemStatusesController(IAdmissionApplicationSupportingItemStatusesService admissionApplicationSupportingItemStatusesService, ILogger logger)
        {
            _admissionApplicationSupportingItemStatusesService = admissionApplicationSupportingItemStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItemStatuses
        /// </summary>
        /// <returns>List of AdmissionApplicationSupportingItemStatuses <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> objects representing matching admissionApplicationSupportingItemStatuses</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemStatus>> GetAdmissionApplicationSupportingItemStatusesAsync()
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
                AddDataPrivacyContextProperty((await _admissionApplicationSupportingItemStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusesAsync(bypassCache);
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
        /// Read (GET) a admissionApplicationSupportingItemStatus using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationSupportingItemStatus> GetAdmissionApplicationSupportingItemStatusByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _admissionApplicationSupportingItemStatusesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusByGuidAsync(guid);
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
        /// Create (POST) a new admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="admissionApplicationSupportingItemStatus">DTO of the new admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionApplicationSupportingItemStatus> PostAdmissionApplicationSupportingItemStatusAsync([FromBody] Dtos.AdmissionApplicationSupportingItemStatus admissionApplicationSupportingItemStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItemStatus to update</param>
        /// <param name="admissionApplicationSupportingItemStatus">DTO of the updated admissionApplicationSupportingItemStatus</param>
        /// <returns>A admissionApplicationSupportingItemStatus object <see cref="Dtos.AdmissionApplicationSupportingItemStatus"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionApplicationSupportingItemStatus> PutAdmissionApplicationSupportingItemStatusAsync([FromUri] string guid, [FromBody] Dtos.AdmissionApplicationSupportingItemStatus admissionApplicationSupportingItemStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationSupportingItemStatus
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemStatus</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationSupportingItemStatusAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}