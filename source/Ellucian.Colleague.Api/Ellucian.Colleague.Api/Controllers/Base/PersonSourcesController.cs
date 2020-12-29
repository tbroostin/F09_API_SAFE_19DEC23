//Copyright 2019 Ellucian Company L.P. and its affiliates.
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
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonSources
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonSourcesController : BaseCompressedApiController
    {
        private readonly IPersonSourcesService _personSourcesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonSourcesController class.
        /// </summary>
        /// <param name="personSourcesService">Service of type <see cref="IPersonSourcesService">IPersonSourcesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonSourcesController(IPersonSourcesService personSourcesService, ILogger logger)
        {
            _personSourcesService = personSourcesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personSources
        /// </summary>
        /// <returns>List of PersonSources <see cref="Dtos.PersonSources"/> objects representing matching personSources</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonSources>> GetPersonSourcesAsync()
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
                var personSources = await _personSourcesService.GetPersonSourcesAsync(bypassCache);

                if (personSources != null && personSources.Any())
                {
                    AddEthosContextProperties(await _personSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _personSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personSources.Select(a => a.Id).ToList()));
                }
                return personSources;
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
        /// Read (GET) a personSources using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personSources</param>
        /// <returns>A personSources object <see cref="Dtos.PersonSources"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonSources> GetPersonSourcesByGuidAsync(string guid)
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
                   await _personSourcesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personSourcesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personSourcesService.GetPersonSourcesByGuidAsync(guid);
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
        /// Create (POST) a new personSources
        /// </summary>
        /// <param name="personSources">DTO of the new personSources</param>
        /// <returns>A personSources object <see cref="Dtos.PersonSources"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonSources> PostPersonSourcesAsync([FromBody] Dtos.PersonSources personSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personSources
        /// </summary>
        /// <param name="guid">GUID of the personSources to update</param>
        /// <param name="personSources">DTO of the updated personSources</param>
        /// <returns>A personSources object <see cref="Dtos.PersonSources"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonSources> PutPersonSourcesAsync([FromUri] string guid, [FromBody] Dtos.PersonSources personSources)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personSources
        /// </summary>
        /// <param name="guid">GUID to desired personSources</param>
        [HttpDelete]
        public async Task DeletePersonSourcesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}