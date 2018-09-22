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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationStatusTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationStatusTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionDecisionTypesService _admissionApplicationStatusTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationStatusTypesController class.
        /// </summary>
        /// <param name="admissionApplicationStatusTypesService">Service of type <see cref="IAdmissionDecisionTypesService">IAdmissionApplicationStatusTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionApplicationStatusTypesController(IAdmissionDecisionTypesService admissionApplicationStatusTypesService, ILogger logger)
        {
            _admissionApplicationStatusTypesService = admissionApplicationStatusTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationStatusTypes v6.
        /// </summary>
        /// <returns>List of AdmissionApplicationStatusTypes <see cref="Dtos.AdmissionApplicationStatusType"/> objects representing matching admissionApplicationStatusTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync()
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
                var statusTypeEntities = await _admissionApplicationStatusTypesService.GetAdmissionApplicationStatusTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _admissionApplicationStatusTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationStatusTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        statusTypeEntities.Select(i => i.Id).ToList()));

                return statusTypeEntities;
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
        /// Read (GET) a admissionApplicationStatusTypes using a GUID v6.
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationStatusType> GetAdmissionApplicationStatusTypesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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
                AddEthosContextProperties(
                    await _admissionApplicationStatusTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationStatusTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _admissionApplicationStatusTypesService.GetAdmissionApplicationStatusTypesByGuidAsync(guid);
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
        /// Create (POST) a new admissionApplicationStatusTypes
        /// </summary>
        /// <param name="admissionApplicationStatusTypes">DTO of the new admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionApplicationStatusType> PostAdmissionApplicationStatusTypesAsync([FromBody] Dtos.AdmissionApplicationStatusType admissionApplicationStatusTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationStatusTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationStatusTypes to update</param>
        /// <param name="admissionApplicationStatusTypes">DTO of the updated admissionApplicationStatusTypes</param>
        /// <returns>A admissionApplicationStatusTypes object <see cref="Dtos.AdmissionApplicationStatusType"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionApplicationStatusType> PutAdmissionApplicationStatusTypesAsync([FromUri] string guid, [FromBody] Dtos.AdmissionApplicationStatusType admissionApplicationStatusTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationStatusTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationStatusTypes</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationStatusTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}