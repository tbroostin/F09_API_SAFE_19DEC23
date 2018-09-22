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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to ExternalEmployments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ExternalEmploymentsController : BaseCompressedApiController
    {
        private readonly IExternalEmploymentsService _externalEmploymentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ExternalEmploymentsController class.
        /// </summary>
        /// <param name="externalEmploymentsService">Service of type <see cref="IExternalEmploymentsService">IExternalEmploymentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ExternalEmploymentsController(IExternalEmploymentsService externalEmploymentsService, ILogger logger)
        {
            _externalEmploymentsService = externalEmploymentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all externalEmployments
        /// </summary>
        
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
                /// <returns>List of ExternalEmployments <see cref="Dtos.ExternalEmployments"/> objects representing matching externalEmployments</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetExternalEmploymentsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _externalEmploymentsService.GetExternalEmploymentsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _externalEmploymentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEmploymentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.ExternalEmployments>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a externalEmployments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.ExternalEmployments> GetExternalEmploymentsByGuidAsync(string guid)
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
                    await _externalEmploymentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEmploymentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _externalEmploymentsService.GetExternalEmploymentsByGuidAsync(guid);
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
        /// Create (POST) a new externalEmployments
        /// </summary>
        /// <param name="externalEmployments">DTO of the new externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.ExternalEmployments> PostExternalEmploymentsAsync([FromBody] Dtos.ExternalEmployments externalEmployments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing externalEmployments
        /// </summary>
        /// <param name="guid">GUID of the externalEmployments to update</param>
        /// <param name="externalEmployments">DTO of the updated externalEmployments</param>
        /// <returns>A externalEmployments object <see cref="Dtos.ExternalEmployments"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ExternalEmployments> PutExternalEmploymentsAsync([FromUri] string guid, [FromBody] Dtos.ExternalEmployments externalEmployments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a externalEmployments
        /// </summary>
        /// <param name="guid">GUID to desired externalEmployments</param>
        [HttpDelete]
        public async Task DeleteExternalEmploymentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}