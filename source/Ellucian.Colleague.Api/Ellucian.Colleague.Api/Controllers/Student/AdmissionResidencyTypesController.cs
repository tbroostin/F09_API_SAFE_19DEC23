//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
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
    /// Provides access to AdmissionResidencyTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionResidencyTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionResidencyTypesService _admissionResidencyTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionResidencyTypesController class.
        /// </summary>
        /// <param name="admissionResidencyTypesService">Service of type <see cref="IAdmissionResidencyTypesService">IAdmissionResidencyTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionResidencyTypesController(IAdmissionResidencyTypesService admissionResidencyTypesService, ILogger logger)
        {
            _admissionResidencyTypesService = admissionResidencyTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionResidencyTypes
        /// </summary>
        /// <returns>List of AdmissionResidencyTypes <see cref="Dtos.AdmissionResidencyTypes"/> objects representing matching admissionResidencyTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionResidencyTypes>> GetAdmissionResidencyTypesAsync()
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
                var pageOfItems = await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(bypassCache);

                AddEthosContextProperties(
                  await _admissionResidencyTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionResidencyTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Select(i => i.Id).Distinct().ToList()));

                return pageOfItems;
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
        /// Read (GET) a admissionResidencyTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionResidencyTypes> GetAdmissionResidencyTypesByGuidAsync(string guid)
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
                  await _admissionResidencyTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionResidencyTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync(guid);
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
        /// Create (POST) a new admissionResidencyTypes
        /// </summary>
        /// <param name="admissionResidencyTypes">DTO of the new admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionResidencyTypes> PostAdmissionResidencyTypesAsync([FromBody] Dtos.AdmissionResidencyTypes admissionResidencyTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionResidencyTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionResidencyTypes to update</param>
        /// <param name="admissionResidencyTypes">DTO of the updated admissionResidencyTypes</param>
        /// <returns>A admissionResidencyTypes object <see cref="Dtos.AdmissionResidencyTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionResidencyTypes> PutAdmissionResidencyTypesAsync([FromUri] string guid, [FromBody] Dtos.AdmissionResidencyTypes admissionResidencyTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionResidencyTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionResidencyTypes</param>
        [HttpDelete]
        public async Task DeleteAdmissionResidencyTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}