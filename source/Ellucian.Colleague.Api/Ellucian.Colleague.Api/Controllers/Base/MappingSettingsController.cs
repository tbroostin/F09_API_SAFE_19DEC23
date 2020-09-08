﻿//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using System.Configuration;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to MappingSettings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class MappingSettingsController : BaseCompressedApiController
    {
        private readonly IMappingSettingsService _mappingSettingsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MappingSettingsController class.
        /// </summary>
        /// <param name="mappingSettingsService">Service of type <see cref="IMappingSettingsService">IMappingSettingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public MappingSettingsController(IMappingSettingsService mappingSettingsService, ILogger logger)
        {
            _mappingSettingsService = mappingSettingsService;
            _logger = logger;
        }

        #region GET mapping-settings
        /// <summary>
        /// Return all mappingSettings
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Mapping Setting search criteria in JSON format</param>
        /// <returns>List of MappingSettings <see cref="Dtos.MappingSettings"/> objects representing matching mappingSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.MappingSettings))]
        public async Task<IHttpActionResult> GetMappingSettingsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaFilter = GetFilterObject<Dtos.MappingSettings>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.MappingSettings>>(new List<Dtos.MappingSettings>(), page, 0, this.Request);
            
            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _mappingSettingsService.GetMappingSettingsAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                  await _mappingSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _mappingSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.MappingSettings>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a mappingSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mappingSettings</param>
        /// <returns>A mappingSettings object <see cref="Dtos.MappingSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.MappingSettings> GetMappingSettingsByGuidAsync(string guid)
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
                   await _mappingSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _mappingSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _mappingSettingsService.GetMappingSettingsByGuidAsync(guid);
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

        #region GET mapping-settings-options
        /// <summary>
        /// Return all mappingSettingsOptions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Mapping Setting search criteria in JSON format</param>
        /// <returns>List of MappingSettings <see cref="Dtos.MappingSettings"/> objects representing matching mappingSettings</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.MappingSettings))]
        public async Task<IHttpActionResult> GetMappingSettingsOptionsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            var criteriaFilter = GetFilterObject<Dtos.MappingSettingsOptions>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.MappingSettingsOptions>>(new List<Dtos.MappingSettingsOptions>(), page, 0, this.Request);

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _mappingSettingsService.GetMappingSettingsOptionsAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(
                  await _mappingSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _mappingSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.MappingSettingsOptions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a mappingSettings using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired mappingSettings</param>
        /// <returns>A mappingSettings object <see cref="Dtos.MappingSettingsOptions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.MappingSettingsOptions> GetMappingSettingsOptionsByGuidAsync(string guid)
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
                   await _mappingSettingsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _mappingSettingsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _mappingSettingsService.GetMappingSettingsOptionsByGuidAsync(guid, bypassCache);
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

        #region PUT mapping-settings
        /// <summary>
        /// Update (PUT) an existing MappingSettings
        /// </summary>
        /// <param name="guid">GUID of the mappingSettings to update</param>
        /// <param name="mappingSettings">DTO of the updated mappingSettings</param>
        /// <returns>A MappingSettings object <see cref="Dtos.MappingSettings"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.MappingSettings> PutMappingSettingsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.MappingSettings mappingSettings)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (mappingSettings == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null mappingSettings argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(mappingSettings.Id))
            {
                mappingSettings.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, mappingSettings.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                var origMappingSettings = await _mappingSettingsService.GetMappingSettingsByGuidAsync(guid, true);
                var mergedMappingSettings = await PerformPartialPayloadMerge(mappingSettings, origMappingSettings,
                     await _mappingSettingsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                     _logger);
                if (origMappingSettings != null && mergedMappingSettings != null)
                {
                    IntegrationApiException exception = null;
                    
                    if (origMappingSettings.Title != mergedMappingSettings.Title)
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The title cannot be changed for a mapping setting."));
                    }
                    var origResource = string.Empty;
                    var origPropertyName = string.Empty;
                    if (origMappingSettings.Ethos != null && origMappingSettings.Ethos.Resources != null && origMappingSettings.Ethos.Resources.Any())
                    {
                        foreach (var origResourceObject in origMappingSettings.Ethos.Resources)
                        {
                            if (!string.IsNullOrEmpty(origResourceObject.Resource))
                            {
                                origResource = origResourceObject.Resource;
                            }
                            if (!string.IsNullOrEmpty(origResourceObject.PropertyName))
                            {
                                origPropertyName = origResourceObject.PropertyName;
                            }
                        }
                    }
                    var resource = string.Empty;
                    var propertyName = string.Empty;
                    if (mergedMappingSettings.Ethos != null && mergedMappingSettings.Ethos.Resources != null && mergedMappingSettings.Ethos.Resources.Any())
                    {
                        foreach (var resourceObject in mergedMappingSettings.Ethos.Resources)
                        {
                            if (!string.IsNullOrEmpty(resourceObject.Resource))
                            {
                                resource = resourceObject.Resource;
                            }
                            if (!string.IsNullOrEmpty(resourceObject.PropertyName))
                            {
                                propertyName = resourceObject.PropertyName;
                            }
                        }
                    }
                    if ((!string.IsNullOrEmpty(resource)) && (!string.IsNullOrEmpty(origResource)) && (resource != origResource))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        {
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Ethos Resource cannot be changed for a mapping setting."));
                        }
                    }
                    if ((!string.IsNullOrEmpty(propertyName)) && (!string.IsNullOrEmpty(origPropertyName)) && (propertyName != origPropertyName))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        {
                            exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The Ethos Property Name cannot be changed for a mapping setting."));
                        }
                    }

                    if (origMappingSettings.Ethos != null && !string.IsNullOrEmpty(origMappingSettings.Ethos.Enumeration) && mergedMappingSettings.Ethos != null && string.IsNullOrEmpty(mergedMappingSettings.Ethos.Enumeration))
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The requested enumeration may not be unset, only changed to a different value."));
                    }

                    if (origMappingSettings.Source != null && !string.IsNullOrEmpty(origMappingSettings.Source.Title) && mergedMappingSettings.Source != null && origMappingSettings.Source.Title != mergedMappingSettings.Source.Title)
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The source title cannot be changed for a mapping setting."));
                    }

                    if (mergedMappingSettings.Source == null)
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The source cannot be changed for a mapping setting."));
                    }

                    if (origMappingSettings.Source != null && !string.IsNullOrEmpty(origMappingSettings.Source.Value) && mergedMappingSettings.Source != null && origMappingSettings.Source.Value != mergedMappingSettings.Source.Value)
                    {
                        if (exception == null) exception = new IntegrationApiException();
                        exception.AddError(new IntegrationApiError("Validation.Exception", "An error occurred attempting to validate data.", "The source value cannot be changed for a mapping setting."));
                    }

                    if (exception != null)
                    {
                        throw exception;
                    }          
                }
                return await _mappingSettingsService.UpdateMappingSettingsAsync(mergedMappingSettings);


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
        #endregion

        /// <summary>
        /// Create (POST) a new mappingSettings
        /// </summary>
        /// <param name="mappingSettings">DTO of the new mappingSettings</param>
        /// <returns>A mappingSettings object <see cref="Dtos.MappingSettings"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.MappingSettings> PostMappingSettingsAsync([FromBody] Dtos.MappingSettings mappingSettings)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing mappingSettingsOptions
        /// </summary>
        /// <param name="guid">GUID of the mapping settings options to update</param>
        /// <param name="mappingSettings">DTO of the new mappingSettings</param>
        /// <returns>A mappingSettings object <see cref="Dtos.MappingSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.MappingSettingsOptions> PutMappingSettingsOptionsAsync([FromUri] string guid, [FromBody] Dtos.MappingSettingsOptions mappingSettings)
        {
            var exception = new IntegrationApiException();
            exception.AddError(new IntegrationApiError("Invalid.Operation", "Invalid operation for alternate view of resource.",
                "The Update operation is only available when requesting the mapping-settings representation."));
            throw CreateHttpResponseException(exception, HttpStatusCode.NotAcceptable);
        }

        /// <summary>
        /// Create (POST) a new mappingSettingsOptions
        /// </summary>
        /// <param name="mappingSettings">DTO of the new mappingSettings</param>
        /// <returns>A mappingSettings object <see cref="Dtos.MappingSettingsOptions"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.MappingSettingsOptions> PostMappingSettingsOptionsAsync([FromBody] Dtos.MappingSettingsOptions mappingSettings)
        {
            //Post is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <summary>
        /// Delete (DELETE) a mappingSettings
        /// </summary>
        /// <param name="guid">GUID to desired mappingSettings</param>
        [HttpDelete]
        public async Task DeleteMappingSettingsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}