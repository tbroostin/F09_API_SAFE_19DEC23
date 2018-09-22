// Copyright 2014-2018 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonsController : BaseCompressedApiController
    {
        private readonly IPersonRestrictionTypeService _personRestrictionTypeService;
        private readonly IPersonService _personService;
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;        

        /// <summary>
        /// Initializes a new instance of the PersonsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personService">Service of type <see cref="IPersonService">IPersonService</see></param>
        /// <param name="personRestrictionTypeService">Service of type <see cref="IPersonRestrictionTypeService">IPersonRestrictionTypeService</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IEmergencyInformationService">IEmergencyInformationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PersonsController(IAdapterRegistry adapterRegistry, IPersonService personService, IPersonRestrictionTypeService personRestrictionTypeService, IEmergencyInformationService emergencyInformationService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _personService = personService;
            _personRestrictionTypeService = personRestrictionTypeService;
            _emergencyInformationService = emergencyInformationService;
            _logger = logger;
        }

        #region Get Methods
        
        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetPersonCredentialsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _personService.GetAllPersonCredentialsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonCredential>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
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
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetPersonCredentials2Async(Paging page)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _personService.GetAllPersonCredentials2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonCredential2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException rex)
            {
                _logger.Error(rex.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(rex));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Gets a subset of person credentials.
        /// </summary>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonCredential2))]
        //public async Task<IHttpActionResult> GetPersonCredentials3Async(Paging page, [FromUri] string criteria = "")
        public async Task<IHttpActionResult> GetPersonCredentials3Async(Paging page, QueryStringFilter criteria)
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
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.PersonCredential2>(_logger, "criteria");

                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case CredentialType2.BannerId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."));
                            case CredentialType2.BannerSourcedId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."));
                            case CredentialType2.BannerUdcId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."));
                            case CredentialType2.BannerUserName:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."));
                            case CredentialType2.Ssn:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."));
                            case CredentialType2.Sin:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."));
                        }
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonCredential2>>(new List<Dtos.PersonCredential2>(), page, 0, this.Request);

                var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>();


                var personCredentials = criteriaObject.Credentials != null ?

                criteriaObject.Credentials.ToString() : string.Empty;

                var pageOfItems = await _personService.GetAllPersonCredentials3Async(page.Offset, page.Limit, criteriaObject, bypassCache);
                
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonCredential2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException rex)
            {
                _logger.Error(rex.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(rex));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential">PersonsCredentials</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.PersonCredential> GetPersonCredentialByGuidAsync(string id)
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
                var credential = await _personService.GetPersonCredentialByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateNotFoundException("person", id);
            }
        }

        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential2">PersonsCredentials</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.PersonCredential2> GetPersonCredential2ByGuidAsync(string id)
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
                var credential = await _personService.GetPersonCredential2ByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateNotFoundException("person", id);
            }
        }

        /// <summary>
        /// Get a subset of person's data, including only their credentials.
        /// </summary>
        /// <param name="id">A global identifier of a person.</param>
        /// <returns>The requested <see cref="Dtos.PersonCredential2">PersonsCredentials</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.PersonCredential2> GetPersonCredential3ByGuidAsync(string id)
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
                var credential = await _personService.GetPersonCredential3ByGuidAsync(id);

                if (credential != null)
                {

                    AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { credential.Id }));
                }


                return credential;

            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateNotFoundException("person", id);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves all active person restriction types for a student
        /// </summary>
        /// <returns>PersonRestrictionType object for a student.</returns>
        [Obsolete("Obsolete as of HeDM Version 4.5, use person-holds API instead.")]
        public async Task<List<Dtos.GuidObject>> GetActivePersonRestrictionTypesAsync(string guid)
        {
            try
            {
                return await _personRestrictionTypeService.GetActivePersonRestrictionTypesAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Get a person's profile information.
        /// </summary>
        /// <param name="personId">Id of the person to get</param>
        /// <returns>The requested <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user or their proxies can view their profile.
        /// </accessComments>
        public async Task<Dtos.Base.Profile> GetProfileAsync(string personId)
        {
            try
            {
                bool useCache = true;
                if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        useCache = false;
                    }
                }
                return await _personService.GetProfileAsync(personId, useCache);
            }
            catch (ArgumentNullException anex)
            {
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateNotFoundException("person", personId);
            }
        }

        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="personId">Pass in a person ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        /// <accessComments>
        /// Only the current user can get their emergency information.
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.16. Use GetEmergencyInformation2Async instead.")]
        public async Task<Ellucian.Colleague.Dtos.Base.EmergencyInformation> GetEmergencyInformationAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw CreateHttpResponseException("Invalid person ID", HttpStatusCode.BadRequest);
            }

            try
            {
                return await _emergencyInformationService.GetEmergencyInformationAsync(personId);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("person", personId);
            }
        }

        /// <summary>
        /// Get all the emergency information for a person.
        /// </summary>
        /// <param name="personId">Pass in a person ID</param>
        /// <returns>Returns all the emergency information for the specified person</returns>
        /// <accessComments>
        /// Only the current user can access their emergency information.
        /// </accessComments>
        public async Task<Ellucian.Colleague.Dtos.Base.EmergencyInformation> GetEmergencyInformation2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw CreateHttpResponseException("Invalid person ID", HttpStatusCode.BadRequest);
            }

            try
            {
                var privacyWrappedEmerInfo = await _emergencyInformationService.GetEmergencyInformation2Async(personId);
                var emergencyInformation = privacyWrappedEmerInfo.Dto;
                if (privacyWrappedEmerInfo.HasPrivacyRestrictions)
                {
                    SetContentRestrictedHeader("partial");
                }
                return emergencyInformation;
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("person", personId);
            }
        }

        #endregion

        #region Get Methods for HEDM v6

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person2">Person</see></returns>
        [EedmResponseFilter]
        public async Task<Person2> GetPerson2ByGuidAsync(string guid)
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
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                return await _personService.GetPerson2ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="title">Person Title Contains ...title...</param>
        /// <param name="firstName">Person First Name equal to</param>
        /// <param name="middleName">Person Middle Name equal to</param>
        /// <param name="lastNamePrefix">Person Last Name begins with 'code...'</param>
        /// <param name="lastName">Person Last Name equal to</param>
        /// <param name="pedigree">Person Suffixe Contains ...pedigree... (guid)</param>
        /// <param name="preferredName">Person Preferred Name equal to (guid)</param>
        /// <param name="role">Person Role equal to (guid)</param>
        /// <param name="credentialType">Person Credential Type (colleagueId or ssn)</param>
        /// <param name="credentialValue">Person Credential equal to</param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person2 <see cref="Dtos.Person2"/> objects representing matching persons</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "title", "firstName", "middleName", "lastNamePrefix", "lastName", "pedigree", "preferredName", "role", "credentialType", "credentialValue", "personFilter" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetPerson2Async(Paging page, [FromUri] string title = "", [FromUri] string firstName = "", [FromUri] string middleName = "",
            [FromUri] string lastNamePrefix = "", [FromUri] string lastName = "", [FromUri] string pedigree = "", [FromUri] string preferredName = "",
            [FromUri] string role = "", [FromUri] string credentialType = "", [FromUri] string credentialValue = "", [FromUri] string personFilter = "")
        {
            if ( title == null || firstName == null || middleName == null || lastNamePrefix == null || lastName == null || pedigree == null || preferredName == null || role == null || credentialType == null || credentialValue == null || personFilter == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, this.Request);
            }
            string criteria = string.Concat(title, firstName, middleName, lastNamePrefix, lastName, pedigree, preferredName,
                    role, credentialType, credentialValue, personFilter);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, this.Request);
            }
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            //validate the crendentials
            if (!string.IsNullOrEmpty(credentialType) && string.IsNullOrEmpty(credentialValue))
            {
                throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType");
            }
            if (string.IsNullOrEmpty(credentialType) && !string.IsNullOrEmpty(credentialValue))
            {
                throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
            }
            if (!string.IsNullOrEmpty(credentialType))
            {
                var credentialTypeValue = GetEnumFromEnumMemberAttribute(credentialType, Dtos.EnumProperties.CredentialType.NotSet);
                if (credentialTypeValue == Dtos.EnumProperties.CredentialType.NotSet)
                {
                    throw new ArgumentException("credentialValue", "credentialType is not valid");
                }
                if (credentialTypeValue == Dtos.EnumProperties.CredentialType.Sin || credentialTypeValue == Dtos.EnumProperties.CredentialType.Ssn || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerId
                            || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerSourcedId || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerUdcId || credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerSourcedId ||
                            credentialTypeValue == Dtos.EnumProperties.CredentialType.BannerUserName )
                {
                    throw new ArgumentException("credentialType", string.Concat("Credential Type filter of '", credentialTypeValue, "' is not supported."));
                }
            }
            //validate role
            if (!string.IsNullOrEmpty(role))
            {
                var roleTypeValue = GetEnumFromEnumMemberAttribute(role, Dtos.EnumProperties.PersonRoleType.NotSet);
                if (roleTypeValue == Dtos.EnumProperties.PersonRoleType.NotSet)
                      throw new ArgumentException("role", string.Concat(role, " is not a valid role."));
            }
            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.Person2>>(new List<Dtos.Person2>(), page, 0, this.Request);

            try
            {
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _personService.GetPerson2NonCachedAsync(page.Offset, page.Limit, bypassCache,
                    title, firstName, middleName, lastNamePrefix, lastName, pedigree, preferredName,
                    role, credentialType, credentialValue, personFilter);

                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Person2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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

        #endregion

        #region Get Methods for HEDM v8

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person3">Person</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.Person3> GetPersonByGuid3Async(string guid)
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
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));
                
                return await _personService.GetPerson3ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="criteria">Person search criteria in JSON format
        /// <param name="personFilter">Person filter search criteria</param>
        /// <param name="preferredName">Person filter search criteria</param>
        /// Can contain:
        /// title - Person title equal to
        /// firstName - Person First Name equal to
        /// middleName - Person Middle Name equal to
        /// lastNamePrefix - Person Last Name begins with 'code...'
        /// lastName- -Person Last Name equal to
        /// pedigree- -Person Suffix Contains pedigree (guid)
        /// preferredName - Person Preferred Name equal to (guid)
        /// role - Person Role equal to (guid)
        /// credentialType - Person Credential Type (colleagueId - SSN/SIN not supported here)
        /// credentialValue - Person Credential equal to
        /// personFilter - Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person2 <see cref="Dtos.Person3"/> objects representing matching persons</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.PersonFilter))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter))]
        [QueryStringFilterFilter("preferredName", typeof(Dtos.Filters.PreferredNameFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetPerson3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter, QueryStringFilter preferredName)
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
                    page = new Paging(100, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    personFilterValue = personFilterObj.personFilterId != null ? personFilterObj.personFilterId : null;
                }

                string preferredNameValue = string.Empty;
                var preferredNameFilterObj = GetFilterObject<Dtos.Filters.PreferredNameFilter>(_logger, "preferredName");
                if (preferredNameFilterObj != null)
                {
                    preferredNameValue = preferredNameFilterObj.PreferredName != null ? preferredNameFilterObj.PreferredName : null;
                }

                var criteriaObj = GetFilterObject<Dtos.Filters.PersonFilter>(_logger, "criteria");
                
                if (criteriaObj != null)
                {
                    //check for old filter and convert them to new format
                    if (!string.IsNullOrEmpty(criteriaObj.Title) || !string.IsNullOrEmpty(criteriaObj.FirstName) || !string.IsNullOrEmpty(criteriaObj.MiddleName) || !string.IsNullOrEmpty(criteriaObj.LastName)
                        || !string.IsNullOrEmpty(criteriaObj.LastNamePrefix) || !string.IsNullOrEmpty(criteriaObj.Pedigree))
                    {
                        var personName = new PersonNameDtoProperty();
                        personName.Title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                        personName.FirstName = criteriaObj.FirstName != null ? criteriaObj.FirstName : string.Empty;
                        personName.MiddleName = criteriaObj.MiddleName != null ? criteriaObj.MiddleName : string.Empty;
                        personName.LastNamePrefix = criteriaObj.LastNamePrefix != null ? criteriaObj.LastNamePrefix : string.Empty;
                        personName.LastName = criteriaObj.LastName != null ? criteriaObj.LastName : string.Empty;
                        personName.Pedigree = criteriaObj.Pedigree != null ? criteriaObj.Pedigree : string.Empty;
                        criteriaObj.PersonNames = new List<PersonNameDtoProperty> { personName };
                    }
                    if (!string.IsNullOrEmpty(criteriaObj.PreferredName))
                    {
                        preferredNameValue = criteriaObj.PreferredName;
                    }
                    if (!string.IsNullOrEmpty(criteriaObj.PersonFilterFilter))
                    {
                        personFilterValue = criteriaObj.PersonFilterFilter;
                    }
                    if (criteriaObj.Role != null)
                    {
                        criteriaObj.Roles = new List<PersonRoleDtoProperty> { new PersonRoleDtoProperty { RoleType = criteriaObj.Role } };
                    }
                    if ((criteriaObj.CredentialType != null) || !string.IsNullOrEmpty(criteriaObj.CredentialValue))
                    {
                        if (!string.IsNullOrEmpty(criteriaObj.CredentialType.ToString()) && string.IsNullOrEmpty(criteriaObj.CredentialValue))
                        {
                            throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType");
                        }
                        if (string.IsNullOrEmpty(criteriaObj.CredentialType.ToString()) && !string.IsNullOrEmpty(criteriaObj.CredentialValue))
                        {
                            throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
                        }
                        criteriaObj.Credentials = new List<CredentialDtoProperty2> { new CredentialDtoProperty2 { Type = criteriaObj.CredentialType, Value = criteriaObj.CredentialValue } };
                    }

                    // do filter validation
                    //we need to validate the credentials
                    if (criteriaObj.Credentials != null && criteriaObj.Credentials.Any())
                    {
                        foreach (var cred in criteriaObj.Credentials)
                        {
                            if (cred.Type == CredentialType2.Sin || cred.Type == CredentialType2.Ssn || cred.Type == CredentialType2.BannerId
                                || cred.Type == CredentialType2.BannerSourcedId || cred.Type == CredentialType2.BannerUdcId || cred.Type == CredentialType2.BannerSourcedId ||
                                cred.Type == CredentialType2.BannerUserName || cred.Type == CredentialType2.ColleagueUserName || cred.Type == CredentialType2.NotSet)
                            {
                                throw new ArgumentException("credentialType", string.Concat("Credential Type filter of '", cred.Type, "' is not supported."));
                            }
                            if (!string.IsNullOrEmpty(cred.Type.ToString()) && string.IsNullOrEmpty(cred.Value))
                            {
                                throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType");
                            }
                            if (string.IsNullOrEmpty(cred.Type.ToString()) && !string.IsNullOrEmpty(cred.Value))
                            {
                                throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
                            }
                        }
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Person3>>(new List<Dtos.Person3>(), page, 0, this.Request);

                var pageOfItems = await _personService.GetPerson3NonCachedAsync(page.Offset, page.Limit, bypassCache, criteriaObj, personFilterValue, preferredNameValue);

                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Person3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (JsonReaderException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
            }
            catch
                (KeyNotFoundException e)
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

        #endregion

        #region METHODS for EEDM V12

        /// <summary>
        /// Get a person.
        ///  If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="guid">Guid of the person to get</param>
        /// <returns>The requested <see cref="Person4">Person</see></returns>
        [EedmResponseFilter]
        public async Task<Dtos.Person4> GetPerson4ByIdAsync(string guid)
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
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _personService.GetPerson4ByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Return a list of Person objects based on selection criteria.
        /// </summary>
        /// <param name="page">Person page to retrieve</param>
        /// <param name="personFilter">Person filter search criteria</param>
        /// <param name="criteria">Person search criteria in JSON format
        /// Can contain:
        /// title - Person title equal to
        /// firstName - Person First Name equal to
        /// middleName - Person Middle Name equal to
        /// lastNamePrefix - Person Last Name begins with 'code...'
        /// lastName- -Person Last Name equal to
        /// role - Person Role equal to (guid)
        /// credentialType - Person Credential Type (colleagueId - SSN/SIN not supported here)
        /// credentialValue - Person Credential equal to
        /// personFilter - Selection from SaveListParms definition or person-filters</param>
        /// <returns>List of Person3 <see cref="Dtos.Person3"/> objects representing matching persons</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Person4))]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetPerson4Async(Paging page, QueryStringFilter personFilter, QueryStringFilter criteria)
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
                    page = new Paging(100, 0);
                }
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    personFilterValue = personFilterObj.personFilterId != null ? personFilterObj.personFilterId : null;
                }
                var criteriaObject = GetFilterObject<Dtos.Person4>(_logger, "criteria");
                 if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Person4>>(new List<Dtos.Person4>(), page, 0, this.Request);
                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    foreach (var cred in criteriaObject.Credentials)
                    {
                        switch (cred.Type)
                        {
                            case Credential3Type.BannerId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerId' is not supported."));
                            case Credential3Type.BannerSourcedId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerSourcedId' is not supported."));
                            case Credential3Type.BannerUdcId:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUdcId' is not supported."));
                            case Credential3Type.BannerUserName:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'bannerUserName' is not supported."));
                            case Credential3Type.Ssn:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'ssn' is not supported."));
                            case Credential3Type.Sin:
                                throw new ArgumentException("credentialType", string.Concat("credentials.type filter of 'sin' is not supported."));
                        }
                    }
                }
                
                var pageOfItems = await _personService.GetPerson4NonCachedAsync(page.Offset, page.Limit, bypassCache, criteriaObject, personFilterValue);
                AddEthosContextProperties(
                    await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Person4>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch(JsonSerializationException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
            }
            catch (JsonReaderException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                   IntegrationApiUtility.GetDefaultApiError("Error parsing JSON person search request.")));
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

        #endregion

        #region Post Methods

        /// <summary>
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person2"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Person2> PostPerson2Async([ModelBinder(typeof(EedmModelBinder))] Person2 person)
        {
            if (person == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson2Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person3"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Person3> PostPerson3Async([ModelBinder(typeof(EedmModelBinder))] Person3 person)
        {
            if (person == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson3Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Create (POST) a new person
        /// </summary>
        /// <param name="person">DTO of the new person</param>
        /// <returns>A person object <see cref="Person4"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Person4> PostPerson4Async([ModelBinder(typeof(EedmModelBinder))] Person4 person)
        {
            if (person == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Person.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (person.Id != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("person", "Nil GUID must be used in POST operation.");
            }

            try
            {
                // Check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(person.CitizenshipStatus, person.CitizenshipCountry, null, null);

                //call import extend method that needs the extracted extension data and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person
                var personReturn = await _personService.CreatePerson4Async(person);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personReturn.Id }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Create person credentials.
        /// </summary>
        /// <param name="personCredential"><see cref="Dtos.PersonCredential">PersonCredential</see> to update</param>
        /// <returns>Newly created <see cref="Dtos.PersonCredential">PersonCredential</see></returns>
        [HttpPost]
        public async Task<Dtos.PersonCredential> PostPersonCredentialAsync([FromBody] Dtos.PersonCredential personCredential)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region Put Methods

        /// <summary>
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person2"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Person2> PutPerson2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person2 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                // do partial merge
                var originalPerson = new Dtos.Person2();
                try
                {
                    originalPerson = await _personService.GetPerson2ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }

                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);


                var personReturn = await _personService.UpdatePerson2Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person3"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Person3> PutPerson3Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person3 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));



                // do partial merge
                var originalPerson = new Dtos.Person3();
                try
                {
                    originalPerson = await _personService.GetPerson3ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }

                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }
                                
                // check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);

                var personReturn = await _personService.UpdatePerson3Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Update (PUT) an existing person
        /// </summary>
        /// <param name="guid">GUID of the person to update</param>
        /// <param name="person">DTO of the updated person</param>
        /// <returns>A Person2 object <see cref="Dtos.Person4"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Person4> PutPerson4Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Person4 person)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (person == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null person argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(person.Id))
            {
                person.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, person.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
                        
            try
            {
                //get Data Privacy List
                var dpList = await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personService.ImportExtendedEthosData(await ExtractExtendedData(await _personService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                // Check for {} sent in for Citizenship Status
                if (person.CitizenshipStatus != null)
                {
                    if (person.CitizenshipStatus.Category == null && (person.CitizenshipStatus.Detail == null || string.IsNullOrEmpty(person.CitizenshipStatus.Detail.Id)))
                    {
                        person.CitizenshipStatus = null;
                    }
                }
                if (string.IsNullOrEmpty(person.CitizenshipCountry)) person.CitizenshipCountry = null;

                // do partial merge
                var originalPerson = new Dtos.Person4();
                try
                {
                    originalPerson = await _personService.GetPerson4ByGuidAsync(guid, true);
                }
                catch (KeyNotFoundException)
                {
                    originalPerson = null;
                }
                
                var mergedPerson = await PerformPartialPayloadMerge(person, originalPerson, dpList, _logger);

                PersonCitizenshipDtoProperty originalPersonCitizenshipStatus = null;
                string originalPersonCitizenshipCountry = null;
                if (originalPerson != null)
                {                    
                    originalPersonCitizenshipStatus = originalPerson.CitizenshipStatus;
                    originalPersonCitizenshipCountry = originalPerson.CitizenshipCountry;
                }
 
                // Check citizenship fields
                var xx = await _personService.CheckCitizenshipfields(mergedPerson.CitizenshipStatus, mergedPerson.CitizenshipCountry, originalPersonCitizenshipStatus, originalPersonCitizenshipCountry);


                //do update with partial logic
                var personReturn = await _personService.UpdatePerson4Async(mergedPerson);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return personReturn;
            }
            catch (ApplicationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
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
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="personId">The ID of the person profile to update.</param>
        /// <param name="profile"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user can update their profile.
        /// </accessComments>
        [HttpPut]
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this action instead.")]
        public async Task<Dtos.Base.Profile> PutProfileAsync([FromUri] string personId, [FromBody] Dtos.Base.Profile profile)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("PersonsController-PutProfileAsync: Must provide a person id in the request uri");
                    throw new Exception();
                }
                if (profile == null)
                {
                    _logger.Error("PersonsController-PutProfileAsync: Must provide a profile in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(profile.Id))
                {
                    _logger.Error("PersonsController-PutProfileAsync: Must provide a person Id in the request body");
                    throw new Exception();
                }
                if (personId != profile.Id)
                {
                    _logger.Error("PersonsController-PutProfileAsync: PersonID in URL is not the same as in request body");
                    throw new Exception();
                }

                return await _personService.UpdateProfileAsync(profile);
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateHttpResponseException("Unable to update profile information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates certain person profile information: AddressConfirmationDateTime, EmailAddressConfirmationDateTime,
        /// PhoneConfirmationDateTime, EmailAddresses, Personal Phones and Addresses. LastChangedDateTime must match the last changed timestamp on the database
        /// Person record to ensure updates not occurring from two different sources at the same time. If no changes are found, a NotModified Http status code
        /// is returned. If required by configuration, users must be set up with permissions to perform these updates: UPDATE.OWN.EMAIL, UPDATE.OWN.PHONE, and 
        /// UPDATE.OWN.ADDRESS. 
        /// </summary>
        /// <param name="personId">The ID of the person profile to update.</param>
        /// <param name="profile"><see cref="Dtos.Base.Profile">Profile</see> to use to update</param>
        /// <returns>Newly updated <see cref="Dtos.Base.Profile">Profile</see></returns>
        /// <accessComments>
        /// Only the current user can update their profile.
        /// </accessComments>
        [HttpPut]
        public async Task<Dtos.Base.Profile> PutProfile2Async([FromUri] string personId, [FromBody] Dtos.Base.Profile profile)
        {
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    _logger.Error("PersonsController-PutProfile2Async: Must provide a person id in the request uri");
                    throw new Exception();
                }
                if (profile == null)
                {
                    _logger.Error("PersonsController-PutProfile2Async: Must provide a profile in the request body");
                    throw new Exception();
                }
                if (string.IsNullOrEmpty(profile.Id))
                {
                    _logger.Error("PersonsController-PutProfile2Async: Must provide a person Id in the request body");
                    throw new Exception();
                }
                if (personId != profile.Id)
                {
                    _logger.Error("PersonsController-PutProfile2Async: PersonID in URL is not the same as in request body");
                    throw new Exception();
                }

                return await _personService.UpdateProfile2Async(profile);
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateHttpResponseException("Unable to update profile information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a PersonCredential.
        /// </summary>
        /// <param name="personCredential"><see cref="Dtos.PersonCredential">PersonCredential</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonCredential">PersonCredential</see></returns>
        [HttpPut]
        public async Task<Dtos.PersonCredential> PutPersonCredentialAsync([FromBody] Dtos.PersonCredential personCredential)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update a person's emergency information.
        /// </summary>
        /// <param name="emergencyInformation">An emergency information object</param>
        /// <returns>The updated emergency information object</returns>
        /// <accessComments>
        /// Only the current user can update their own emergency information.
        /// </accessComments>
        [HttpPut]
        public Dtos.Base.EmergencyInformation PutEmergencyInformation(Dtos.Base.EmergencyInformation emergencyInformation)
        {
            if (emergencyInformation == null)
            {
                throw CreateHttpResponseException("Request missing emergency information", HttpStatusCode.BadRequest);
            }
            try
            {
                var updatedEmergencyInformation = _emergencyInformationService.UpdateEmergencyInformation(emergencyInformation);

                return updatedEmergencyInformation;
            }
            catch (PermissionsException permissionException)
            {
                throw CreateHttpResponseException(permissionException.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateHttpResponseException("Unable to update emergency information", HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing Person
        /// </summary>
        /// <param name="id">Id of the Person to delete</param>
        [HttpDelete]
        public async Task DeletePersonAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing PersonCredential
        /// </summary>
        /// <param name="id">Id of the PersonCredential to delete</param>
        [HttpDelete]
        public async Task DeletePersonCredentialAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region Query Methods

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person2">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person2">persons</see></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<IEnumerable<Person2>> QueryPerson2ByPostAsync([FromBody] Person2 person)
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
                AddDataPrivacyContextProperty((await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _personService.QueryPerson2ByPostAsync(person);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person3">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person3">persons</see></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<IEnumerable<Person3>> QueryPerson3ByPostAsync([FromBody] Person3 person)
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
                AddDataPrivacyContextProperty((await _personService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _personService.QueryPerson3ByPostAsync(person);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries a person by post.
        /// </summary>
        /// <param name="person"><see cref="Person4">Person</see> to use for querying</param>
        /// <returns>List of matching <see cref="Person4">persons</see></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<IEnumerable<Person4>> QueryPerson4ByPostAsync([FromBody] Person4 person)
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
                var personDtos = await _personService.QueryPerson4ByPostAsync(person, bypassCache);

                AddEthosContextProperties(await _personService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personDtos.Select(a => a.Id).ToList()));

                return personDtos;
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query person by criteria and return the results of the matching algorithm
        /// </summary>
        /// <param name="criteria">The <see cref="Dtos.Base.PersonMatchCriteria">criteria</see> to query by.</param>
        /// <returns>List of matching <see cref="Dtos.Base.PersonMatchResult">results</see></returns>
        [HttpPost]
        public async Task<IEnumerable<Dtos.Base.PersonMatchResult>> QueryPersonMatchResultsByPostAsync([FromBody] Dtos.Base.PersonMatchCriteria criteria)
        {
            try
            {
                return await _personService.QueryPersonMatchResultsByPostAsync(criteria);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves the matching Persons for the ids provided or searches keyword
        /// for the matching Persons if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can be either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        [HttpPost]
        public async Task<IEnumerable<Dtos.Base.Person>> QueryPersonNamesByPostAsync([FromBody] Dtos.Base.PersonNameQueryCriteria criteria)
        {
            try
            {
                return await _personService.QueryPersonNamesByPostAsync(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            
        }

        #endregion
    }
}