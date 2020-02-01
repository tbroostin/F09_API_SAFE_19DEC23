// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Academic Period data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicPeriodsController : BaseCompressedApiController
    {
        private readonly IAcademicPeriodService _academicPeriodService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicPeriodsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicPeriodService">Repository of type <see cref="IAcademicPeriodService">IAcademicPeriodService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AcademicPeriodsController(IAdapterRegistry adapterRegistry, IAcademicPeriodService academicPeriodService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _academicPeriodService = academicPeriodService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all Academic Periods.
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicPeriod2">AcademicPeriod</see>objects.</returns>        
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicPeriod2>> GetAcademicPeriods2Async(QueryStringFilter criteria)
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
                var items = await _academicPeriodService.GetAcademicPeriods2Async(bypassCache);

                AddEthosContextProperties(
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all Academic Periods
        /// including census dates and registration status.
        /// </summary>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>All <see cref="Dtos.AcademicPeriod3">AcademicPeriod</see>objects.</returns>
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AcademicPeriod3)), FilteringFilter(IgnoreFiltering = true)]
        [HttpGet, EedmResponseFilter]
        public async Task<IEnumerable<Dtos.AcademicPeriod3>> GetAcademicPeriods3Async(QueryStringFilter criteria)
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

                string registration = string.Empty;

                var acadPeriod = GetFilterObject<Dtos.AcademicPeriod3>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.AcademicPeriod3>(new List<Dtos.AcademicPeriod3>());

                var registrationStatus = acadPeriod.RegistrationStatus != null ?

                acadPeriod.RegistrationStatus.ToString() : string.Empty;

                var items = await _academicPeriodService.GetAcademicPeriods3Async(bypassCache, registrationStatus);

                AddEthosContextProperties(
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM v16.0.0</remarks>
        /// <summary>
        /// Retrieves all Academic Periods
        /// including census dates and registration status.
        /// </summary>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>All <see cref="Dtos.AcademicPeriod4">AcademicPeriod</see>objects.</returns>
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.AcademicPeriodFilter)), FilteringFilter(IgnoreFiltering = true)]
        [HttpGet, EedmResponseFilter]
        public async Task<IEnumerable<Dtos.AcademicPeriod4>> GetAcademicPeriods4Async(QueryStringFilter criteria)
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

                string registration = string.Empty;

                var acadPeriod = GetFilterObject<Dtos.Filters.AcademicPeriodFilter>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.AcademicPeriod4>(new List<Dtos.AcademicPeriod4>());

                var filterQualifiers = GetFilterQualifiers(_logger);

                var registrationStatus = acadPeriod.RegistrationStatus != null ?
                    acadPeriod.RegistrationStatus.ToString() : string.Empty;
                var termCode = acadPeriod.Code != null ?
                    acadPeriod.Code : string.Empty;
                var category = acadPeriod.Category != null && acadPeriod.Category.Type != null ?
                    acadPeriod.Category.Type.ToString() : string.Empty;
                var startOn = acadPeriod.StartOn.HasValue ? acadPeriod.StartOn.Value : default(DateTime?);
                var endOn = acadPeriod.EndOn.HasValue ? acadPeriod.EndOn.Value : default(DateTime?);

                var items = await _academicPeriodService.GetAcademicPeriods4Async(bypassCache, registrationStatus, termCode, category, startOn, endOn, filterQualifiers);

                AddEthosContextProperties(
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves an academic period by id.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicPeriod2">AcademicPeriod</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicPeriod2> GetAcademicPeriodByGuid2Async(string id)
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
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicPeriodService.GetAcademicPeriodByGuid2Async(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves an academic period by id
        /// including census dates and registration status.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicPeriod3">AcademicPeriod</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicPeriod3> GetAcademicPeriodByGuid3Async(string id)
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
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicPeriodService.GetAcademicPeriodByGuid3Async(id, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM v16.0.0</remarks>
        /// <summary>
        /// Retrieves an academic period by id
        /// including census dates and registration status.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicPeriod4">AcademicPeriod</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicPeriod4> GetAcademicPeriodByGuid4Async(string id)
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
                  await _academicPeriodService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicPeriodService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicPeriodService.GetAcademicPeriodByGuid4Async(id, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Create an academic period
        /// </summary>
        /// <param name="academicPeriod">academicPeriod</param>
        /// <returns>A section object <see cref="Dtos.AcademicPeriod2"/> in HEDM format</returns>
        public async Task<Dtos.AcademicPeriod2> PostAcademicPeriodAsync([FromBody] Ellucian.Colleague.Dtos.AcademicPeriod2 academicPeriod)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update an academic period
        /// </summary>
        /// <param name="id">desired id for an academic period</param>
        /// <param name="academicPeriod">academicPeriod</param>
        /// <returns>A section object <see cref="Dtos.AcademicPeriod2"/> in HEDM format</returns>
        public async Task<Dtos.AcademicPeriod2> PutAcademicPeriodAsync([FromUri] string id, [FromBody] Dtos.AcademicPeriod2 academicPeriod)
        {
            //POST is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an academic period
        /// </summary>
        /// <param name="id">id to desired academic period</param>
        /// <returns>A section object <see cref="Dtos.AcademicPeriod2"/> in HEDM format</returns>
        [HttpDelete]
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriod2> DeleteAcademicPeriodByIdAsync(string id)
        {
            //Delete is not supported for Colleague but Hedm requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}