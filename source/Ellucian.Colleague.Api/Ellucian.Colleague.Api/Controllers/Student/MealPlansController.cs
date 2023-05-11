//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlans
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ResidenceLife)]
    public class MealPlansController : BaseCompressedApiController
    {
        private readonly IMealPlansService _mealPlansService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlansController class.
        /// </summary>
        /// <param name="mealPlansService">Service of type <see cref="IMealPlansService">IMealPlansService</see></param>
        /// <param name="logger">Interface to logger</param>
        public MealPlansController(IMealPlansService mealPlansService, ILogger logger)
        {
            _mealPlansService = mealPlansService;
            _logger = logger;
        }

        /// <summary>
        /// Return all mealPlans
        /// </summary>
        /// <returns>List of MealPlans <see cref="Dtos.MealPlans"/> objects representing matching mealPlans</returns>
        [HttpGet]       
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MealPlans>> GetMealPlansAsync()
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
                var plans = await _mealPlansService.GetMealPlansAsync(bypassCache);
                if (plans != null && plans.Any())
                {
                    AddEthosContextProperties(
                        await _mealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _mealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                         plans.Select(i => i.Id).ToList()));
                }
                return plans;
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
        /// Read (GET) a mealPlans using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.MealPlans> GetMealPlansByGuidAsync(string guid)
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

                var plan = await _mealPlansService.GetMealPlansByGuidAsync(guid);  // TODO: Honor bypassCache

                if (plan != null)
                {

                    AddEthosContextProperties(await _mealPlansService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlansService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { plan.Id }));
                }
                return plan;
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
        /// Create (POST) a new mealPlans
        /// </summary>
        /// <param name="mealPlans">DTO of the new mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.MealPlans> PostMealPlansAsync([FromBody] Dtos.MealPlans mealPlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mealPlans
        /// </summary>
        /// <param name="guid">GUID of the mealPlans to update</param>
        /// <param name="mealPlans">DTO of the updated mealPlans</param>
        /// <returns>A mealPlans object <see cref="Dtos.MealPlans"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.MealPlans> PutMealPlansAsync([FromUri] string guid, [FromBody] Dtos.MealPlans mealPlans)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a mealPlans
        /// </summary>
        /// <param name="guid">GUID to desired mealPlans</param>
        [HttpDelete]
        public async Task DeleteMealPlansAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}