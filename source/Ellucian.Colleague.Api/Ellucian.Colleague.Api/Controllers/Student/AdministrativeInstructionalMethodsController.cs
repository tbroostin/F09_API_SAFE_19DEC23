//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to AdministrativeInstructionalMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdministrativeInstructionalMethodsController : BaseCompressedApiController
    {
        private readonly IAdministrativeInstructionalMethodsService _administrativeInstructionalMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdministrativeInstructionalMethodsController class.
        /// </summary>
        /// <param name="administrativeInstructionalMethodsService">Service of type <see cref="IAdministrativeInstructionalMethodsService">IAdministrativeInstructionalMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdministrativeInstructionalMethodsController(IAdministrativeInstructionalMethodsService administrativeInstructionalMethodsService, ILogger logger)
        {
            _administrativeInstructionalMethodsService = administrativeInstructionalMethodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all administrativeInstructionalMethods
        /// </summary>
        /// <returns>List of AdministrativeInstructionalMethods <see cref="Dtos.AdministrativeInstructionalMethods"/> objects representing matching administrativeInstructionalMethods</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdministrativeInstructionalMethods>> GetAdministrativeInstructionalMethodsAsync()
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
                var administrativeInstructionalMethods = await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsAsync(bypassCache);

                if (administrativeInstructionalMethods != null && administrativeInstructionalMethods.Any())
                {
                    AddEthosContextProperties(
                      await _administrativeInstructionalMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _administrativeInstructionalMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          administrativeInstructionalMethods.Select(i => i.Id).ToList()));
                }
                return administrativeInstructionalMethods;
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
        /// Read (GET) a administrativeInstructionalMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired administrativeInstructionalMethods</param>
        /// <returns>A administrativeInstructionalMethods object <see cref="Dtos.AdministrativeInstructionalMethods"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdministrativeInstructionalMethods> GetAdministrativeInstructionalMethodsByGuidAsync(string guid)
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
                var administrativeInstrucationalMethod = await _administrativeInstructionalMethodsService.GetAdministrativeInstructionalMethodsByGuidAsync(guid);
                if (administrativeInstrucationalMethod != null)
                {

                    AddEthosContextProperties(await _administrativeInstructionalMethodsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _administrativeInstructionalMethodsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { administrativeInstrucationalMethod.Id }));
                }
                return administrativeInstrucationalMethod;
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
        /// Create (POST) a new administrativeInstructionalMethods
        /// </summary>
        /// <param name="administrativeInstructionalMethods">DTO of the new administrativeInstructionalMethods</param>
        /// <returns>A administrativeInstructionalMethods object <see cref="Dtos.AdministrativeInstructionalMethods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdministrativeInstructionalMethods> PostAdministrativeInstructionalMethodsAsync([FromBody] Dtos.AdministrativeInstructionalMethods administrativeInstructionalMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing administrativeInstructionalMethods
        /// </summary>
        /// <param name="guid">GUID of the administrativeInstructionalMethods to update</param>
        /// <param name="administrativeInstructionalMethods">DTO of the updated administrativeInstructionalMethods</param>
        /// <returns>A administrativeInstructionalMethods object <see cref="Dtos.AdministrativeInstructionalMethods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdministrativeInstructionalMethods> PutAdministrativeInstructionalMethodsAsync([FromUri] string guid, [FromBody] Dtos.AdministrativeInstructionalMethods administrativeInstructionalMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a administrativeInstructionalMethods
        /// </summary>
        /// <param name="guid">GUID to desired administrativeInstructionalMethods</param>
        [HttpDelete]
        public async Task DeleteAdministrativeInstructionalMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}