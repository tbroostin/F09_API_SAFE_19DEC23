﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to NonPersonRelationships
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class NonPersonRelationshipsController : BaseCompressedApiController
    {
        private readonly INonPersonRelationshipsService _nonPersonRelationshipsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NonPersonRelationshipsController class.
        /// </summary>
        /// <param name="personRelationshipsService">Service of type <see cref="INonPersonRelationshipsService">INonPersonRelationshipsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public NonPersonRelationshipsController(INonPersonRelationshipsService personRelationshipsService, ILogger logger)
        {
            _nonPersonRelationshipsService = personRelationshipsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personRelationships
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="organization">organization Filter</param>
        /// <param name="institution">institution Filter</param>
        /// <param name="criteria">NonPerson Filter</param>
        /// <param name="relationshipType">relatuonship type Filter</param>
        /// <returns>List of NonPersonRelationships <see cref="Dtos.NonPersonRelationships"/> objects representing matching personRelationships</returns>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200)]
        public async Task<IHttpActionResult> GetNonPersonRelationshipsAsync(Paging page, [FromUri] string organization = "", [FromUri] string institution = "", [FromUri] string criteria = "", [FromUri] string relationshipType = "")
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
                string organizationValue = string.Empty, institutionValue = string.Empty;
                if (!string.IsNullOrEmpty(organization))
                {
                    var organizationObject = JObject.Parse(organization);
                    if (organizationObject != null)
                    {
                        organizationValue = GetValueFromJsonObjectToken("organization.id", organizationObject);
                    }
                }

                if (!string.IsNullOrEmpty(institution))
                {
                    var institutionObject = JObject.Parse(institution);
                    if (institutionObject != null)
                    {
                        institutionValue = GetValueFromJsonObjectToken("institution.id", institutionObject);
                    }
                }

                if (!string.IsNullOrWhiteSpace(criteria))
                {
                    var criteriaObject = JObject.Parse(criteria);
                    if (criteriaObject != null)
                    {
                         personValue = GetValueFromJsonObjectToken("related.person.id", criteriaObject);
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
                
                var pageOfItems = await _nonPersonRelationshipsService.GetNonPersonRelationshipsAsync(page.Offset, page.Limit, organizationValue, institutionValue, personValue, relationshipTypeValue, bypassCache);

                AddEthosContextProperties(
                  await _nonPersonRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _nonPersonRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.NonPersonRelationships>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.NonPersonRelationships> GetNonPersonRelationshipsByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _personRelationshipsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                   await _nonPersonRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _nonPersonRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _nonPersonRelationshipsService.GetNonPersonRelationshipsByGuidAsync(guid);
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
        /// Create (POST) a new personRelationships
        /// </summary>
        /// <param name="personRelationships">DTO of the new personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.NonPersonRelationships> PostNonPersonRelationshipsAsync([FromBody] Dtos.NonPersonRelationships personRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personRelationships
        /// </summary>
        /// <param name="guid">GUID of the personRelationships to update</param>
        /// <param name="personRelationships">DTO of the updated personRelationships</param>
        /// <returns>A personRelationships object <see cref="Dtos.NonPersonRelationships"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.NonPersonRelationships> PutNonPersonRelationshipsAsync([FromUri] string guid, [FromBody] Dtos.NonPersonRelationships personRelationships)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personRelationships
        /// </summary>
        /// <param name="guid">GUID to desired personRelationships</param>
        [HttpDelete]
        public async Task DeleteNonPersonRelationshipsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
     }
}