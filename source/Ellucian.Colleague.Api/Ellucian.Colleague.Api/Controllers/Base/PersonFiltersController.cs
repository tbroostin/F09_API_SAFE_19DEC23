// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Person Filters data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonFiltersController : BaseCompressedApiController
    {
        private readonly IDemographicService _demographicService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonFiltersController class.
        /// </summary>
        /// <param name="demographicService">Service of type <see cref="IDemographicService">IDemographicService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public PersonFiltersController(IDemographicService demographicService, ILogger logger)
        {
            _demographicService = demographicService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all person filters.
        /// </summary>
        /// <returns>All PersonFilters objects.</returns>
        [HttpGet, EedmResponseFilter, ValidateQueryStringFilter(new string[] {"code", "title" })]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonFilter))]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonFilter>> GetPersonFilters2Async(QueryStringFilter criteria)
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
                var filter = GetFilterObject<Dtos.PersonFilter>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.PersonFilter>(new List<Dtos.PersonFilter>());

                var personFiltersEntities = await _demographicService.GetPersonFiltersAsync(bypassCache);

                //apply filters
                if (filter != null && personFiltersEntities != null && personFiltersEntities.Any())
                {
                    if (!string.IsNullOrEmpty(filter.Code))
                    {
                        personFiltersEntities = personFiltersEntities.Where(pf => pf.Code.Equals(filter.Code, StringComparison.OrdinalIgnoreCase));
                    }
                    if ((personFiltersEntities.Any()) && (!string.IsNullOrEmpty(filter.Title)))
                    {
                        personFiltersEntities = personFiltersEntities.Where(pf => pf.Title.IndexOf(filter.Title, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }

                AddEthosContextProperties(
                        await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            personFiltersEntities.Select(pf => pf.Id).ToList()));

                return personFiltersEntities;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves all person filters.
        /// </summary>
        /// <returns>All PersonFilters objects.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonFilter>> GetPersonFiltersAsync()
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
                var personFiltersEntities = await _demographicService.GetPersonFiltersAsync(bypassCache);

                AddEthosContextProperties(
                        await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            personFiltersEntities.Select(pf => pf.Id).ToList()));

                return personFiltersEntities;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Retrieves a person filter by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonFilter">PersonFilter.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.PersonFilter> GetPersonFilterByIdAsync(string id)
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
                        await _demographicService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _demographicService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            new List<string>() { id }));

                return await _demographicService.GetPersonFilterByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a PersonFilter.
        /// </summary>
        /// <param name="personFilter"><see cref="PersonFilter">PersonFilter</see> to update</param>
        /// <returns>Newly updated <see cref="PersonFilter">PersonFilter</see></returns>
        [HttpPut]
        public async Task<Dtos.PersonFilter> PutPersonFilterAsync([FromBody] Dtos.PersonFilter personFilter)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a PersonFilter.
        /// </summary>
        /// <param name="personFilter"><see cref="PersonFilter">PersonFilter</see> to create</param>
        /// <returns>Newly created <see cref="PersonFilter">PersonFilter</see></returns>
        [HttpPost]
        public async Task<Dtos.PersonFilter> PostPersonFilterAsync([FromBody] Dtos.PersonFilter personFilter)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonFilter
        /// </summary>
        /// <param name="id">Id of the PersonFilter to delete</param>
        [HttpDelete]
        public async Task DeletePersonFilterAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
