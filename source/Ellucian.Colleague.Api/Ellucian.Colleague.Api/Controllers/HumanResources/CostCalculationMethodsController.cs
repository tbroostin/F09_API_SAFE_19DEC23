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
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to CostCalculationMethods
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class CostCalculationMethodsController : BaseCompressedApiController
    {
        private readonly ICostCalculationMethodsService _costCalculationMethodsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CostCalculationMethodsController class.
        /// </summary>
        /// <param name="costCalculationMethodsService">Service of type <see cref="ICostCalculationMethodsService">ICostCalculationMethodsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public CostCalculationMethodsController(ICostCalculationMethodsService costCalculationMethodsService, ILogger logger)
        {
            _costCalculationMethodsService = costCalculationMethodsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all costCalculationMethods
        /// </summary>
                /// <returns>List of CostCalculationMethods <see cref="Dtos.CostCalculationMethods"/> objects representing matching costCalculationMethods</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CostCalculationMethods>> GetCostCalculationMethodsAsync()
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
            {                   AddDataPrivacyContextProperty((await _costCalculationMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());               
                return await _costCalculationMethodsService.GetCostCalculationMethodsAsync(bypassCache);
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
        /// Read (GET) a costCalculationMethods using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired costCalculationMethods</param>
        /// <returns>A costCalculationMethods object <see cref="Dtos.CostCalculationMethods"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.CostCalculationMethods> GetCostCalculationMethodsByGuidAsync(string guid)
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
               AddDataPrivacyContextProperty((await _costCalculationMethodsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
               return await _costCalculationMethodsService.GetCostCalculationMethodsByGuidAsync(guid);
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
        /// Create (POST) a new costCalculationMethods
        /// </summary>
        /// <param name="costCalculationMethods">DTO of the new costCalculationMethods</param>
        /// <returns>A costCalculationMethods object <see cref="Dtos.CostCalculationMethods"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.CostCalculationMethods> PostCostCalculationMethodsAsync([FromBody] Dtos.CostCalculationMethods costCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing costCalculationMethods
        /// </summary>
        /// <param name="guid">GUID of the costCalculationMethods to update</param>
        /// <param name="costCalculationMethods">DTO of the updated costCalculationMethods</param>
        /// <returns>A costCalculationMethods object <see cref="Dtos.CostCalculationMethods"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.CostCalculationMethods> PutCostCalculationMethodsAsync([FromUri] string guid, [FromBody] Dtos.CostCalculationMethods costCalculationMethods)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a costCalculationMethods
        /// </summary>
        /// <param name="guid">GUID to desired costCalculationMethods</param>
        [HttpDelete]
        public async Task DeleteCostCalculationMethodsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}