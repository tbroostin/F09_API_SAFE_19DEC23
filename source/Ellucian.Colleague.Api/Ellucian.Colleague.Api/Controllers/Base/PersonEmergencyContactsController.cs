//Copyright 2021 Ellucian Company L.P. and its affiliates.

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
using System.Configuration;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonEmergencyContacts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonEmergencyContactsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _personEmergencyContactsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEmergencyContactsController class.
        /// </summary>
        /// <param name="personEmergencyContactsService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonEmergencyContactsController(IEmergencyInformationService personEmergencyContactsService, ILogger logger)
        {
            _personEmergencyContactsService = personEmergencyContactsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personEmergencyContacts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        ///  <param name="criteria">Selection criteria</param>
        ///  <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of PersonEmergencyContacts <see cref="Dtos.PersonEmergencyContacts"/> objects representing matching personEmergencyContacts</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact })]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonEmergencyContacts))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        public async Task<IHttpActionResult> GetPersonEmergencyContactsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(new List<Dtos.PersonEmergencyContacts>(), page, 0, this.Request);

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var criteriaObj = GetFilterObject<Dtos.PersonEmergencyContacts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(new List<Dtos.PersonEmergencyContacts>(), page, 0, this.Request);

                var pageOfItems = await _personEmergencyContactsService.GetPersonEmergencyContacts2Async(page.Offset, page.Limit, criteriaObj, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personEmergencyContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personEmergencyContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonEmergencyContacts>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a personEmergencyContacts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personEmergencyContacts</param>
        /// <returns>A personEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { BasePermissionCodes.ViewAnyPersonContact, BasePermissionCodes.UpdatePersonContact })]
        public async Task<Dtos.PersonEmergencyContacts> GetPersonEmergencyContactsByGuidAsync(string guid)
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
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _personEmergencyContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personEmergencyContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personEmergencyContactsService.GetPersonEmergencyContactsByGuid2Async(guid);
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
        /// Update (PUT) an existing PersonEmergencyContacts
        /// </summary>
        /// <param name="guid">GUID of the personEmergencyContacts to update</param>
        /// <param name="personEmergencyContacts">DTO of the updated personEmergencyContacts</param>
        /// <returns>A PersonEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter, PermissionsFilter(BasePermissionCodes.UpdatePersonContact)]
        public async Task<Dtos.PersonEmergencyContacts> PutPersonEmergencyContactsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonEmergencyContacts personEmergencyContacts)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personEmergencyContacts == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personEmergencyContacts argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personEmergencyContacts.Id))
            {
                personEmergencyContacts.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personEmergencyContacts.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personEmergencyContactsService.UpdatePersonEmergencyContactsAsync(
                  await PerformPartialPayloadMerge(personEmergencyContacts, async () => await _personEmergencyContactsService.GetPersonEmergencyContactsByGuid2Async(guid, true),
                  await _personEmergencyContactsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));
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
        /// Create (POST) a new personEmergencyContacts
        /// </summary>
        /// <param name="personEmergencyContacts">DTO of the new personEmergencyContacts</param>
        /// <returns>A personEmergencyContacts object <see cref="Dtos.PersonEmergencyContacts"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, PermissionsFilter(BasePermissionCodes.UpdatePersonContact)]
        public async Task<Dtos.PersonEmergencyContacts> PostPersonEmergencyContactsAsync(Dtos.PersonEmergencyContacts personEmergencyContacts)
        {
            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                return await _personEmergencyContactsService.CreatePersonEmergencyContactsAsync(personEmergencyContacts);
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a personEmergencyContacts
        /// </summary>
        /// <param name="guid">GUID to desired personEmergencyContacts</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete, PermissionsFilter(BasePermissionCodes.DeletePersonContact)]
        public async Task<HttpResponseMessage> DeletePersonEmergencyContactsAsync([FromUri] string guid)
        {
            try
            {
                _personEmergencyContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await _personEmergencyContactsService.DeletePersonEmergencyContactsAsync(guid);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

    }
}