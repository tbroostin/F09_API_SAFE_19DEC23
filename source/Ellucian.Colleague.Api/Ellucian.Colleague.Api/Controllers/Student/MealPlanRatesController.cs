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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlanRates
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealPlanRatesController : BaseCompressedApiController
    {
        private readonly IMealPlanRatesService _mealPlanRatesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlanRatesController class.
        /// </summary>
        /// <param name="mealPlanRatesService">Service of type <see cref="IMealPlanRatesService">IMealPlanRatesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public MealPlanRatesController(IMealPlanRatesService mealPlanRatesService, ILogger logger)
        {
            _mealPlanRatesService = mealPlanRatesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all mealPlanRates
        /// </summary>
        /// <returns>List of MealPlanRates <see cref="Dtos.MealPlanRates"/> objects representing matching mealPlanRates</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlanRates>> GetMealPlanRatesAsync()
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
                var rates = await _mealPlanRatesService.GetMealPlanRatesAsync(bypassCache);
                if (rates != null && rates.Any())
                {
                    AddEthosContextProperties(
                        await _mealPlanRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _mealPlanRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         rates.Select(i => i.Id).ToList()));
                }
                return rates;

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
        /// Read (GET) a mealPlanRates using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.MealPlanRates> GetMealPlanRatesByGuidAsync(string guid)
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
                var rate = await _mealPlanRatesService.GetMealPlanRatesByGuidAsync(guid);  // TODO: Honor bypassCache

                if (rate != null)
                {

                    AddEthosContextProperties(await _mealPlanRatesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlanRatesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { rate.Id }));
                }
                return rate;
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
        /// Create (POST) a new mealPlanRates
        /// </summary>
        /// <param name="mealPlanRates">DTO of the new mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.MealPlanRates> PostMealPlanRatesAsync([FromBody] Dtos.MealPlanRates mealPlanRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mealPlanRates
        /// </summary>
        /// <param name="guid">GUID of the mealPlanRates to update</param>
        /// <param name="mealPlanRates">DTO of the updated mealPlanRates</param>
        /// <returns>A mealPlanRates object <see cref="Dtos.MealPlanRates"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.MealPlanRates> PutMealPlanRatesAsync([FromUri] string guid, [FromBody] Dtos.MealPlanRates mealPlanRates)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a mealPlanRates
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRates</param>
        [HttpDelete]
        public async Task DeleteMealPlanRatesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}