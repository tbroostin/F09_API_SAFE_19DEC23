// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
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
    /// Provides access to FinancialAidFundCategories data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidFundCategoriesController : BaseCompressedApiController
    {
        private readonly IFinancialAidFundCategoryService _financialAidFundCategoryService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidFundCategoriesController class.
        /// </summary>
        /// <param name="financialAidFundCategoryService">Repository of type <see cref="IFinancialAidFundCategoryService">IFinancialAidFundCategoryService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinancialAidFundCategoriesController(IFinancialAidFundCategoryService financialAidFundCategoryService, ILogger logger)
        {
            _financialAidFundCategoryService = financialAidFundCategoryService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid fund categories.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidFundCategory objects.</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var categories = await _financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync(bypassCache);

                if (categories != null && categories.Any()) {
                    AddEthosContextProperties(
                      await _financialAidFundCategoryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _financialAidFundCategoryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                          categories.Select(i => i.Id).ToList()));
                }
                return categories;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Financial Aid FundCategories by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<FinancialAidFundCategory> GetFinancialAidFundCategoryByIdAsync(string id)
        {
            bool bypassCache = false;
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
                     await _financialAidFundCategoryService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                     await _financialAidFundCategoryService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         new List<string>() { id }));

                return await _financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Creates a Financial Aid Fund Category.
        /// </summary>
        /// <param name="financialAidFundCategory"><see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see></returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidFundCategory> PostFinancialAidFundCategoryAsync([FromBody] Dtos.FinancialAidFundCategory financialAidFundCategory)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Fund Category.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Fund Category to update</param>
        /// <param name="financialAidFundCategory"><see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidFundCategory">FinancialAidFundCategory</see></returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidFundCategory> PutFinancialAidFundCategoryAsync([FromUri] string id, [FromBody] Dtos.FinancialAidFundCategory financialAidFundCategory)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Fund Category.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Fund Category to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteFinancialAidFundCategoryAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}