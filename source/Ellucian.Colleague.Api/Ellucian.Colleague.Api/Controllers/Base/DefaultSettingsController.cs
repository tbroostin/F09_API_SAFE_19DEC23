//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to DefaultSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class DefaultSettingsController : BaseCompressedApiController
    {
        private readonly IDefaultSettingsService _defaultSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the DefaultSettingsController class.
        /// </summary>
        /// <param name="defaultSettingsService">Service of type <see cref="IDefaultSettingsService">IDefaultSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public DefaultSettingsController(IDefaultSettingsService defaultSettingsService, ILogger logger)
        {
            _defaultSettingsService = defaultSettingsService;
            _logger = logger;
        }

        #region GET default-settings
        /// <summary>
        /// Return all defaultSettings
        /// </summary>
        /// <returns>List of DefaultSettings <see cref="Dtos.DefaultSettings"/> objects representing matching defaultSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.DefaultSettings))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettings>> GetDefaultSettingsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.DefaultSettings>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.DefaultSettings>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsAsync(resourcesFilter, bypassCache);

                if (defaultSettings != null && defaultSettings.Any())
                {
                    AddEthosContextProperties(await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              defaultSettings.Select(a => a.Id).ToList()));
                }
                return defaultSettings;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a defaultSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DefaultSettings> GetDefaultSettingsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _defaultSettingsService.GetDefaultSettingsByGuidAsync(guid, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        #endregion

        #region GET default-settings-options
        /// <summary>
        /// Return all defaultSettings options
        /// </summary>
        /// <returns>List of DefaultSettings <see cref="Dtos.DefaultSettingsOptions"/> objects representing matching defaultSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.DefaultSettingsOptions))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsOptions>> GetDefaultSettingsOptionsAsync(QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            List<Dtos.DefaultSettingsEthos> resourcesFilter = new List<Dtos.DefaultSettingsEthos>();
            var criteriaObject = GetFilterObject<Dtos.DefaultSettingsOptions>(_logger, "criteria");
            if (CheckForEmptyFilterParameters())
                return new List<Dtos.DefaultSettingsOptions>();
            if (criteriaObject != null && criteriaObject.Ethos != null && criteriaObject.Ethos.Any())
            {
                resourcesFilter.AddRange(criteriaObject.Ethos);
            }
            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsOptionsAsync(resourcesFilter, bypassCache);

                if (defaultSettings != null && defaultSettings.Any())
                {
                    AddEthosContextProperties(await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              defaultSettings.Select(a => a.Id).ToList()));
                }
                return defaultSettings;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a defaultSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DefaultSettingsOptions> GetDefaultSettingsOptionsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                   await _defaultSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _defaultSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _defaultSettingsService.GetDefaultSettingsOptionsByGuidAsync(guid, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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

        #endregion

        #region GET default-settings-advanced-search-options
        /// <summary>
        /// Return all defaultSettings advanced search options
        /// </summary>
        /// <returns>List of DefaultSettingsAdvancedSearchOptions <see cref="Dtos.DefaultSettingsOptions"/> objects representing matching defaultSettingsAdvancedSearchOptions</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]        
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("advancedSearch", typeof(Dtos.Filters.DefaultSettingsFilter))]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsAdvancedSearchOptions>> GetDefaultSettingsAdvancedSearchOptionsAsync(QueryStringFilter advancedSearch)
        {
            if (advancedSearch == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("advancedSearch must be specified in the request URL when a GET operation is requested.")));
            }           

            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var advancedSearchFilter = GetFilterObject<Dtos.Filters.DefaultSettingsFilter>(_logger, "advancedSearch");

            if (string.IsNullOrEmpty(advancedSearchFilter.Keyword))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("keyword must be specified in the request URL when a GET operation is requested.")));
            }
            if (advancedSearchFilter.DefaultSettings == null || string.IsNullOrEmpty(advancedSearchFilter.DefaultSettings.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("defaultSettings must be specified in the request URL when a GET operation is requested.")));
            }

            try
            {
                var defaultSettings = await _defaultSettingsService.GetDefaultSettingsAdvancedSearchOptionsAsync(advancedSearchFilter, bypassCache);

                return defaultSettings;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a defaultSettingsAdvancedSearchOptions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettingsAdvancedSearchOptions</param>
        /// <returns>A defaultSettingsAdvancedSearchOptions object <see cref="Dtos.DefaultSettingsAdvancedSearchOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.DefaultSettingsAdvancedSearchOptions> GetDefaultSettingsAdvancedSearchOptionsByGuidAsync(string guid)
        {
            //Get by Guid is not supported for this alternate view.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Get by Guid operation is only available when requesting the default-settings representation."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        #endregion

        /// <summary>
        /// Update (PUT) an existing DefaultSettings
        /// </summary>
        /// <param name="guid">GUID of the defaultSettings to update</param>
        /// <param name="defaultSettings">DTO of the updated defaultSettings</param>
        /// <returns>A DefaultSettings object <see cref="Dtos.DefaultSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.DefaultSettings> PutDefaultSettingsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.DefaultSettings defaultSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (defaultSettings == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null defaultSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(defaultSettings.Id))
            {
                defaultSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, defaultSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {                
                var origDefaultSettings = await _defaultSettingsService.GetDefaultSettingsByGuidAsync(guid, true);
                var mergedSettings = await PerformPartialPayloadMerge(defaultSettings, origDefaultSettings,
                  await _defaultSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger);

                // Changed to look at the mergedSettings data instead of simply comparing original to the new request.
                if (origDefaultSettings != null && mergedSettings != null)
                {
                    IntegrationApiException exception = null;
                    
                    if (origDefaultSettings.Source != null && mergedSettings.Source != null
                        && origDefaultSettings.Source.Value.Equals(mergedSettings.Source.Value, StringComparison.OrdinalIgnoreCase)
                        && !origDefaultSettings.Source.Title.Equals(mergedSettings.Source.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Source Title cannot be changed for a default setting."));

                    }
                    if (!string.IsNullOrEmpty(origDefaultSettings.Title) && !origDefaultSettings.Title.Equals(mergedSettings.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a default setting."));
                    }
                    if (!string.IsNullOrEmpty(origDefaultSettings.Description) && !origDefaultSettings.Description.Equals(mergedSettings.Description, StringComparison.OrdinalIgnoreCase))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Description cannot be changed for a default setting."));
                    }
                    //HED-32899
                    /*
                     *Advance search is not null but either search type is null or min search length is null.
                    */
                    if (defaultSettings.AdvancedSearch != null)
                    {
                        if (!defaultSettings.AdvancedSearch.AdvanceSearchType.HasValue)
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The type of advanced search may not be changed for a default setting."));
                        }
                        if (!defaultSettings.AdvancedSearch.MinSearchLength.HasValue)
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The minimum search length needed for an advanced search may not be changed for a default setting."));
                        }
                    }
                    /*
                     *If the requested type does not match the existing IDS.SEARCH.TYPE (or the existing IDS.SEARCH.TYPE is blank), issue err13.
                    */
                    if ((origDefaultSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch != null && origDefaultSettings.AdvancedSearch.AdvanceSearchType != mergedSettings.AdvancedSearch.AdvanceSearchType)||
                        (origDefaultSettings.AdvancedSearch == null && mergedSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch.AdvanceSearchType.HasValue))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The type of advanced search may not be changed for a default setting."));
                    }
                    /*
                     * If the requested minSearchLength does not match the existing IDS.SEARCH.MIN.LENGTH (or IDS.SEARCH.MIN.LENGTH is blank), issue err14.
                    */
                    if (origDefaultSettings.AdvancedSearch != null && mergedSettings.AdvancedSearch != null)
                    {
                        if (origDefaultSettings.AdvancedSearch.MinSearchLength.HasValue && mergedSettings.AdvancedSearch.MinSearchLength.HasValue &&
                       !origDefaultSettings.AdvancedSearch.MinSearchLength.Value.Equals(mergedSettings.AdvancedSearch.MinSearchLength.Value) ||
                        (origDefaultSettings.AdvancedSearch == null && mergedSettings.AdvancedSearch.MinSearchLength.HasValue))
                        {
                            if (exception == null) exception = new IntegrationApiException();
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The minimum search length needed for an advanced search may not be changed for a default setting."));
                        }
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                }

                return await _defaultSettingsService.UpdateDefaultSettingsAsync(mergedSettings);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new defaultSettings
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettings"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.DefaultSettings> PostDefaultSettingsAsync(Dtos.DefaultSettings defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing defaultSettingsOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.DefaultSettingsOptions> PutDefaultSettingsOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.", 
                "The Update operation is only available when requesting the default-settings representation."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new defaultSettingsOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.DefaultSettingsOptions> PostDefaultSettingsOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing defaultSettingsAdvancedSearchOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.DefaultSettingsOptions> PutDefaultSettingsAdvancedSearchOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Put is not supported for Colleague but EEDM requires full crud support.
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation",
                "Invalid operation for alternate view of resource.",
                "The Update operation is only available when requesting the default-settings representation."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new defaultSettingsAdvancedSearchOptions
        /// </summary>
        /// <param name="defaultSettings">DTO of the new defaultSettings</param>
        /// <returns>A defaultSettings object <see cref="Dtos.DefaultSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.DefaultSettingsOptions> PostDefaultSettingsAdvancedSearchOptionsAsync(Dtos.DefaultSettingsOptions defaultSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a defaultSettings
        /// </summary>
        /// <param name="guid">GUID to desired defaultSettings</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteDefaultSettingsAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

    }
}