//Copyright 2019 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonExternalEducationCredentials
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonExternalEducationCredentialsController : BaseCompressedApiController
    {
        private readonly IPersonExternalEducationCredentialsService _personExternalEducationCredentialsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonExternalEducationCredentialsController class.
        /// </summary>
        /// <param name="personExternalEducationCredentialsService">Service of type <see cref="IPersonExternalEducationCredentialsService">IPersonExternalEducationCredentialsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonExternalEducationCredentialsController(IPersonExternalEducationCredentialsService personExternalEducationCredentialsService, ILogger logger)
        {
            _personExternalEducationCredentialsService = personExternalEducationCredentialsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personExternalEducationCredentials
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="personFilter">Person Filter filter options</param>
        /// <returns>List of PersonExternalEducationCredentials <see cref="Dtos.PersonExternalEducationCredentials"/> objects representing matching personExternalEducationCredentials</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]   
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPersonExternalEducationCredentialsAsync(Paging page, QueryStringFilter personFilter)
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
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                        if (string.IsNullOrEmpty(personFilterValue))
                        {
                            return new PagedHttpActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(new List<Dtos.PersonExternalEducationCredentials>(), page, 0, this.Request);
                        }
                    }
                }

                var pageOfItems = await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(page.Offset, page.Limit, personFilterValue, bypassCache);

                AddEthosContextProperties(
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonExternalEducationCredentials>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
   
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
        /// Read (GET) a personExternalEducationCredentials using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducationCredentials</param>
        /// <returns>A personExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonExternalEducationCredentials> GetPersonExternalEducationCredentialsByGuidAsync(string guid)
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
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personExternalEducationCredentialsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));
                return await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(guid);
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
        /// Update (PUT) an existing PersonExternalEducationCredentials
        /// </summary>
        /// <param name="guid">GUID of the personExternalEducationCredentials to update</param>
        /// <param name="personExternalEducationCredentials">DTO of the updated personExternalEducationCredentials</param>
        /// <returns>A PersonExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PersonExternalEducationCredentials> PutPersonExternalEducationCredentialsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (personExternalEducationCredentials == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personExternalEducationCredentials argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
            {
                personExternalEducationCredentials.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, personExternalEducationCredentials.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {          
                return await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(
                  await PerformPartialPayloadMerge(personExternalEducationCredentials, async () => await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(guid, true),
                  await _personExternalEducationCredentialsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
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
        /// Create (POST) a new personExternalEducationCredentials
        /// </summary>
        /// <param name="personExternalEducationCredentials">DTO of the new personExternalEducationCredentials</param>
        /// <returns>A personExternalEducationCredentials object <see cref="Dtos.PersonExternalEducationCredentials"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.PersonExternalEducationCredentials> PostPersonExternalEducationCredentialsAsync(Dtos.PersonExternalEducationCredentials personExternalEducationCredentials)
        {
            if (personExternalEducationCredentials == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid personExternalEducationCredentials.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(personExternalEducationCredentials.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null personExternalEducationCredentials id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (personExternalEducationCredentials.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Must provide nil GUID when creating a new Person External Education Credentials.",
                    IntegrationApiUtility.GetDefaultApiError("Must provide nil GUID for create.")));
            }

            try
            {
                return await _personExternalEducationCredentialsService.CreatePersonExternalEducationCredentialsAsync(personExternalEducationCredentials);
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
        /// Delete (DELETE) a personExternalEducationCredentials
        /// </summary>
        /// <param name="guid">GUID to desired personExternalEducationCredentials</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePersonExternalEducationCredentialsAsync([FromUri] string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        
    }
}