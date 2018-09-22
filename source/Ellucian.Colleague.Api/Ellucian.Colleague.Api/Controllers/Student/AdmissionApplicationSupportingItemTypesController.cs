//Copyright 2017-18 Ellucian Company L.P. and its affiliates.

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
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItemTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationSupportingItemTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemTypesService _admissionApplicationSupportingItemTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemTypesController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemTypesService">Service of type <see cref="IAdmissionApplicationSupportingItemTypesService">IAdmissionApplicationSupportingItemTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionApplicationSupportingItemTypesController(IAdmissionApplicationSupportingItemTypesService admissionApplicationSupportingItemTypesService, ILogger logger)
        {
            _admissionApplicationSupportingItemTypesService = admissionApplicationSupportingItemTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItemTypes
        /// </summary>
        /// <returns>List of AdmissionApplicationSupportingItemTypes <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> objects representing matching admissionApplicationSupportingItemTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>> GetAdmissionApplicationSupportingItemTypesAsync()
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
                var items = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesAsync(bypassCache);

                AddEthosContextProperties(await _admissionApplicationSupportingItemTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionApplicationSupportingItemTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return items;
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
        /// Read (GET) a admissionApplicationSupportingItemTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationSupportingItemTypes> GetAdmissionApplicationSupportingItemTypesByGuidAsync(string guid)
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
                var admissionApplication = await _admissionApplicationSupportingItemTypesService.GetAdmissionApplicationSupportingItemTypesByGuidAsync(guid);

                if (admissionApplication != null)
                {

                    AddEthosContextProperties(await _admissionApplicationSupportingItemTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionApplicationSupportingItemTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { admissionApplication.Id }));
                }

                return admissionApplication;
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
        /// Create (POST) a new admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="admissionApplicationSupportingItemTypes">DTO of the new admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionApplicationSupportingItemTypes> PostAdmissionApplicationSupportingItemTypesAsync([FromBody] Dtos.AdmissionApplicationSupportingItemTypes admissionApplicationSupportingItemTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItemTypes to update</param>
        /// <param name="admissionApplicationSupportingItemTypes">DTO of the updated admissionApplicationSupportingItemTypes</param>
        /// <returns>A admissionApplicationSupportingItemTypes object <see cref="Dtos.AdmissionApplicationSupportingItemTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionApplicationSupportingItemTypes> PutAdmissionApplicationSupportingItemTypesAsync([FromUri] string guid, [FromBody] Dtos.AdmissionApplicationSupportingItemTypes admissionApplicationSupportingItemTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationSupportingItemTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItemTypes</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationSupportingItemTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}