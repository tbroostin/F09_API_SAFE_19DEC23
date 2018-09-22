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
    /// Provides access to FinancialAidYears data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidYearsController : BaseCompressedApiController
    {
        private readonly IFinancialAidYearService _financialAidYearService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FinancialAidYearsController class.
        /// </summary>
        /// <param name="financialAidYearService">Repository of type <see cref="IFinancialAidYearService">IFinancialAidYearService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FinancialAidYearsController(IFinancialAidYearService financialAidYearService, ILogger logger)
        {
            _financialAidYearService = financialAidYearService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid years.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidYear objects.</returns>
        [HttpGet, ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidYear>> GetFinancialAidYearsAsync()
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
                var items = await _financialAidYearService.GetFinancialAidYearsAsync(bypassCache);

                AddEthosContextProperties(
                    await _financialAidYearService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidYearService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
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
        /// Retrieves an Financial Aid Years by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidYear">FinancialAidYear</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<FinancialAidYear> GetFinancialAidYearByIdAsync(string id)
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
                    await _financialAidYearService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _financialAidYearService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await _financialAidYearService.GetFinancialAidYearByGuidAsync(id);
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
        /// Creates a Financial Aid Year.
        /// </summary>
        /// <param name="financialAidYear"><see cref="Dtos.FinancialAidYear">FinancialAidYear</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidYear">FinancialAidYear</see></returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidYear> PostFinancialAidYearAsync([FromBody] Dtos.FinancialAidYear financialAidYear)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Year.
        /// </summary>
        /// <param name="id">Id of the Financial Aid Year to update</param>
        /// <param name="financialAidYear"><see cref="Dtos.FinancialAidYear">FinancialAidYear</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidYear">FinancialAidYear</see></returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidYear> PutFinancialAidYearAsync([FromUri] string id, [FromBody] Dtos.FinancialAidYear financialAidYear)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Year.
        /// </summary>
        /// <param name="id">ID of the Financial Aid Year to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteFinancialAidYearAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}