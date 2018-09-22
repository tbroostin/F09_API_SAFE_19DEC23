//Copyright 2017-18 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to MealPlanRequests
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class MealPlanRequestsController : BaseCompressedApiController
    {
        private readonly IMealPlanRequestsService _mealPlanRequestsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MealPlanRequestsController class.
        /// </summary>
        /// <param name="mealPlanRequestsService">Service of type <see cref="IMealPlanRequestsService">IMealPlanRequestsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public MealPlanRequestsController(IMealPlanRequestsService mealPlanRequestsService, ILogger logger)
        {
            _mealPlanRequestsService = mealPlanRequestsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all mealPlanRequests
        /// </summary>
        /// <returns>List of MealPlanRequests <see cref="Dtos.MealPlanRequests"/> objects representing matching mealPlanRequests</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetMealPlanRequestsAsync(Paging page)
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
                    page = new Paging(200, 0);
                }

                var pageOfItems =  await _mealPlanRequestsService.GetMealPlanRequestsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.MealPlanRequests>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a mealPlanRequests using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.MealPlanRequests> GetMealPlanRequestsByGuidAsync(string guid)
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
                var mealPlans = await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(guid);

                if (mealPlans != null)
                {

                    AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { mealPlans.Id }));
                }


                return mealPlans;
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
        /// Create (POST) a new mealPlanRequests
        /// </summary>
        /// <param name="mealPlanRequests">DTO of the new mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.MealPlanRequests> PostMealPlanRequestsAsync([FromBody] Dtos.MealPlanRequests mealPlanRequests)
        {
            if (mealPlanRequests == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null meal plan request argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(mealPlanRequests.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _mealPlanRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _mealPlanRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the meal plan request
                var mealPlanReturn = await _mealPlanRequestsService.PostMealPlanRequestsAsync(mealPlanRequests);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _mealPlanRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { mealPlanReturn.Id }));

                return mealPlanReturn;
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
        /// Update (PUT) an existing mealPlanRequests
        /// </summary>
        /// <param name="guid">GUID of the mealPlanRequests to update</param>
        /// <param name="mealPlanRequests">DTO of the updated mealPlanRequests</param>
        /// <returns>A mealPlanRequests object <see cref="Dtos.MealPlanRequests"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.MealPlanRequests> PutMealPlanRequestsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.MealPlanRequests mealPlanRequests)
        {
            if (mealPlanRequests == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null meal plan request argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (!guid.Equals(mealPlanRequests.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                //get Data Privacy List
                var dpList = await _mealPlanRequestsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _mealPlanRequestsService.ImportExtendedEthosData(await ExtractExtendedData(await _mealPlanRequestsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var mealPlanRequestReturn = await _mealPlanRequestsService.PutMealPlanRequestsAsync(guid,
                    await PerformPartialPayloadMerge(mealPlanRequests, async () => await _mealPlanRequestsService.GetMealPlanRequestsByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _mealPlanRequestsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return mealPlanRequestReturn;  

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
        /// Delete (DELETE) a mealPlanRequests
        /// </summary>
        /// <param name="guid">GUID to desired mealPlanRequests</param>
        [HttpDelete]
        public async Task DeleteMealPlanRequestsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}