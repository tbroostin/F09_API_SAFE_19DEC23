//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes deduction types data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class DeductionTypesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IDeductionTypesService _deductionTypesService;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deductionTypesService"></param>
        public DeductionTypesController(ILogger logger, IDeductionTypesService deductionTypesService)
        {
            this.logger = logger;
            this._deductionTypesService = deductionTypesService;
        }

        /// <summary>
        /// Returns all deduction types.
        /// </summary>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.DeductionType>> GetAllDeductionTypesAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var items = await _deductionTypesService.GetDeductionTypesAsync(bypassCache);

                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting deduction types.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Returns all deduction types.
        /// </summary>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.DeductionType2>> GetAllDeductionTypes2Async()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var items = await _deductionTypesService.GetDeductionTypes2Async(bypassCache);

                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns a deduction type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DeductionType2> GetDeductionTypeById2Async([FromUri] string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                  await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _deductionTypesService.GetDeductionTypeById2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Returns a deduction type.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DeductionType> GetDeductionTypeByIdAsync([FromUri] string id)
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
                AddEthosContextProperties(
                    await _deductionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _deductionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _deductionTypesService.GetDeductionTypeByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e, "No deduction types was found for guid " + id + ".");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting deduction type.");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// PutDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.DeductionType> PutDeductionTypeAsync([FromBody] Dtos.DeductionType deductionType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PutDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.DeductionType2> PutDeductionType2Async([FromBody] Dtos.DeductionType2 deductionType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// PostDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Dtos.DeductionType> PostDeductionTypeAsync([FromBody] Dtos.DeductionType deductionType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PostDeductionTypeAsync
        /// </summary>
        /// <param name="deductionType"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Dtos.DeductionType2> PostDeductionType2Async([FromBody] Dtos.DeductionType2 deductionType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// DeleteDeductionTypeAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteDeductionTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}