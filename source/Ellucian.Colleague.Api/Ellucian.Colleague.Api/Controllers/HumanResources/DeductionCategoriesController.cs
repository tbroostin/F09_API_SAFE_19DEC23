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
    /// Provides access to DeductionCategories
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class DeductionCategoriesController : BaseCompressedApiController
    {
        private readonly IDeductionCategoriesService _deductionCategoriesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DeductionCategoriesController class.
        /// </summary>
        /// <param name="deductionCategoriesService">Service of type <see cref="IDeductionCategoriesService">IDeductionCategoriesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public DeductionCategoriesController(IDeductionCategoriesService deductionCategoriesService, ILogger logger)
        {
            _deductionCategoriesService = deductionCategoriesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all deductionCategories
        /// </summary>
                /// <returns>List of DeductionCategories <see cref="Dtos.DeductionCategories"/> objects representing matching deductionCategories</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DeductionCategories>> GetDeductionCategoriesAsync()
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
            {                   AddDataPrivacyContextProperty((await _deductionCategoriesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());               
                return await _deductionCategoriesService.GetDeductionCategoriesAsync(bypassCache);
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
        /// Read (GET) a deductionCategories using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DeductionCategories> GetDeductionCategoriesByGuidAsync(string guid)
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
               AddDataPrivacyContextProperty((await _deductionCategoriesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
               return await _deductionCategoriesService.GetDeductionCategoriesByGuidAsync(guid);
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
        /// Create (POST) a new deductionCategories
        /// </summary>
        /// <param name="deductionCategories">DTO of the new deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.DeductionCategories> PostDeductionCategoriesAsync([FromBody] Dtos.DeductionCategories deductionCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing deductionCategories
        /// </summary>
        /// <param name="guid">GUID of the deductionCategories to update</param>
        /// <param name="deductionCategories">DTO of the updated deductionCategories</param>
        /// <returns>A deductionCategories object <see cref="Dtos.DeductionCategories"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.DeductionCategories> PutDeductionCategoriesAsync([FromUri] string guid, [FromBody] Dtos.DeductionCategories deductionCategories)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a deductionCategories
        /// </summary>
        /// <param name="guid">GUID to desired deductionCategories</param>
        [HttpDelete]
        public async Task DeleteDeductionCategoriesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}