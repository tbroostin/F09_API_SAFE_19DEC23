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
using Ellucian.Colleague.Domain.Base.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonRelationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonRelationshipsController : BaseCompressedApiController
    {
        private readonly IPersonRelationshipsService _personRelationshipsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonRelationshipsController class.
        /// </summary>
        /// <param name="personRelationshipsService">Service of type <see cref="IPersonRelationshipsService">IPersonRelationshipsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonRelationshipsController(IPersonRelationshipsService personRelationshipsService, ILogger logger)
        {
            _personRelationshipsService = personRelationshipsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personRelationships
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="person">Person Filter</param>
        /// <param name="relationshipType">relatuonship type Filter</param>
        /// <returns>List of PersonRelationships <see cref="Dtos.PersonRelationships"/> objects representing matching personRelationships</returns>
        /// <accessComments>
        /// Only the current user can get their own person relationships.
        /// </accessComments>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        public async Task<IHttpActionResult> GetPersonRelationshipsAsync(Paging page, [FromUri] string person = "", [FromUri] string relationshipType = "")
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

                string personValue = string.Empty, relationshipTypeValue = string.Empty;
                if (!string.IsNullOrEmpty(person))
                {
                    var personObject = JObject.Parse(person);
                    if (personObject != null)
                    {
                        personValue = GetValueFromJsonObjectToken("person.id", personObject);
                    }
                }
                if (!string.IsNullOrEmpty(relationshipType))
                {
                    var relationshipTypeObject = JObject.Parse(relationshipType);
                    if (relationshipTypeObject != null)
                    {
                        relationshipTypeValue = GetValueFromJsonObjectToken("relationshipType.id", relationshipTypeObject);
                    }
                }
                
                var pageOfItems = await _personRelationshipsService.GetPersonRelationshipsAsync(page.Offset, page.Limit, personValue, relationshipTypeValue, bypassCache);

                AddEthosContextProperties(
                  await _personRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonRelationships>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a personRelationships using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.PersonRelationships"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonRelationships> GetPersonRelationshipsByGuidAsync(string guid)
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
                   await _personRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _personRelationshipsService.GetPersonRelationshipsByGuidAsync(guid);
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
        /// Update (PUT) an existing PersonRelationships
        /// </summary>
        /// <param name="guid">GUID of the personRelationships to update</param>
        /// <param name="personRelationships">DTO of the updated personRelationships</param>
        /// <returns>A PersonRelationships object <see cref="Dtos.PersonRelationships"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PersonRelationships> PutPersonRelationshipsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonRelationships personRelationships)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personRelationships == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personRelationships argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personRelationships.Id))
            {
                personRelationships.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personRelationships.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                return await _personRelationshipsService.UpdatePersonRelationshipsAsync(
                  await PerformPartialPayloadMerge(personRelationships, async () => await _personRelationshipsService.GetPersonRelationshipsByGuidAsync(guid, true),
                  await _personRelationshipsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));
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
        /// Create (POST) a new personRelationships
        /// </summary>
        /// <param name="personRelationships">DTO of the new personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.PersonRelationships"/> in HeDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonRelationships> PostPersonRelationshipsAsync(Dtos.PersonRelationships personRelationships)
        {
            if (personRelationships == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid personRelationships.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personRelationships.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personRelationships id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (personRelationships.Id != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("PersonRelationships", "Nil GUID must be used in POST operation.");
            }

            try
            {
                return await _personRelationshipsService.CreatePersonRelationshipsAsync(personRelationships);
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
        /// Delete (DELETE) a personRelationships
        /// </summary>
        /// <param name="guid">GUID to desired personRelationships</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpDelete]
        public async Task DeletePersonRelationshipsAsync([FromUri] string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await _personRelationshipsService.DeletePersonRelationshipsAsync(guid);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
       }

    }
}