//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to ExternalEmploymentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ExternalEmploymentStatusesController : BaseCompressedApiController
    {
        private readonly IExternalEmploymentStatusesService _externalEmploymentStatusesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ExternalEmploymentStatusesController class.
        /// </summary>
        /// <param name="externalEmploymentStatusesService">Service of type <see cref="IExternalEmploymentStatusesService">IExternalEmploymentStatusesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ExternalEmploymentStatusesController(IExternalEmploymentStatusesService externalEmploymentStatusesService, ILogger logger)
        {
            _externalEmploymentStatusesService = externalEmploymentStatusesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all externalEmploymentStatuses
        /// </summary>
        /// <returns>List of ExternalEmploymentStatuses <see cref="Dtos.ExternalEmploymentStatuses"/> objects representing matching externalEmploymentStatuses</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmploymentStatuses>> GetExternalEmploymentStatusesAsync()
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
                var externalEmploymentStatuses = await _externalEmploymentStatusesService.GetExternalEmploymentStatusesAsync(bypassCache);

                if (externalEmploymentStatuses != null && externalEmploymentStatuses.Any())
                {
                    AddEthosContextProperties(await _externalEmploymentStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _externalEmploymentStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              externalEmploymentStatuses.Select(a => a.Id).ToList()));
                }

                return externalEmploymentStatuses;
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
        /// Read (GET) a externalEmploymentStatuses using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.ExternalEmploymentStatuses> GetExternalEmploymentStatusesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                    await _externalEmploymentStatusesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _externalEmploymentStatusesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _externalEmploymentStatusesService.GetExternalEmploymentStatusesByGuidAsync(guid);
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
        /// Create (POST) a new externalEmploymentStatuses
        /// </summary>
        /// <param name="externalEmploymentStatuses">DTO of the new externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ExternalEmploymentStatuses> PostExternalEmploymentStatusesAsync([FromBody] Dtos.ExternalEmploymentStatuses externalEmploymentStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing externalEmploymentStatuses
        /// </summary>
        /// <param name="guid">GUID of the externalEmploymentStatuses to update</param>
        /// <param name="externalEmploymentStatuses">DTO of the updated externalEmploymentStatuses</param>
        /// <returns>A externalEmploymentStatuses object <see cref="Dtos.ExternalEmploymentStatuses"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ExternalEmploymentStatuses> PutExternalEmploymentStatusesAsync([FromUri] string guid, [FromBody] Dtos.ExternalEmploymentStatuses externalEmploymentStatuses)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a externalEmploymentStatuses
        /// </summary>
        /// <param name="guid">GUID to desired externalEmploymentStatuses</param>
        [HttpDelete]
        public async Task DeleteExternalEmploymentStatusesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}