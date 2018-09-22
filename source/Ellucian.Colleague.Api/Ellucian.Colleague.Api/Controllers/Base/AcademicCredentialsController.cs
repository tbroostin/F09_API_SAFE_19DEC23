// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Web.Http.Filters;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Controller for Academic Credentials
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AcademicCredentialsController : BaseCompressedApiController
    {
        private readonly IAcademicCredentialService _academicCredentialService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the Academic Credentials Controller class.
        /// </summary>
        /// <param name="academicCredentialService">Service of type <see cref="IAcademicCredentialService">IAcademicCredentialService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AcademicCredentialsController(IAcademicCredentialService academicCredentialService, ILogger logger)
        {
            _academicCredentialService = academicCredentialService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Academic Credentials
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicCredential">Academic Credentials.</see></returns>
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicCredential>> GetAcademicCredentialsAsync()
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
                var items = await _academicCredentialService.GetAcademicCredentialsAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _academicCredentialService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _academicCredentialService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  items.Select(a => a.Id).ToList()));
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Academic Credential by GUID.
        /// </summary>
        /// <returns>A <see cref="Dtos.AcademicCredential">Academic Credential.</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.AcademicCredential> GetAcademicCredentialByGuidAsync(string guid)
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
                var item = await _academicCredentialService.GetAcademicCredentialByGuidAsync(guid);

                if (item != null)
                {
                    AddEthosContextProperties(await _academicCredentialService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                        await _academicCredentialService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), 
                        new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>        
        /// Creates an Academic Credential
        /// </summary>
        /// <param name="academicCredential"><see cref="Dtos.AcademicCredential">AcademicCredential</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicCredential">AcademicCredential</see></returns>
        [HttpPost]
        public async Task <Dtos.AcademicCredential> PostAcademicCredentialAsync([FromBody] Dtos.AcademicCredential academicCredential)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an AcademicCredential.
        /// </summary>
        /// <param name="id">Id of the Academic Credential to update</param>
        /// <param name="academicCredential"><see cref="Dtos.AcademicCredential">AcademicCredential</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicCredential">AcademicCredential</see></returns>
        [HttpPut]
        public async Task<Dtos.AcademicCredential> PutAcademicCredentialAsync([FromUri] string id, [FromBody] Dtos.AcademicCredential academicCredential)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Academic Credential
        /// </summary>
        /// <param name="id">Id of the Academic Credential to delete</param>
        [HttpDelete]
        public async Task DeleteAcademicCredentialAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}