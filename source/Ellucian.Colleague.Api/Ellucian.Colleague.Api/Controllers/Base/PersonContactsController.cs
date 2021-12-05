// Copyright 2021 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to person data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonContactsController : BaseCompressedApiController
    {
        private readonly IEmergencyInformationService _emergencyInformationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonContactsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="emergencyInformationService">Service of type <see cref="IPersonService">IPersonService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PersonContactsController(IAdapterRegistry adapterRegistry, IEmergencyInformationService emergencyInformationService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _emergencyInformationService = emergencyInformationService;
            _logger = logger;
        }

        /// <summary>
        /// Gets persons emergency contacts information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="person">Person id filter.</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(BasePermissionCodes.ViewAnyPersonContact)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "person"}, false, true)]
        public async Task<IHttpActionResult> GetPersonEmergencyContactsAsync(Paging page, [FromUri] string person = "")
        {
            string criteria = person;

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, this.Request);
            }
            if (person == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, this.Request);
            }

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
                _emergencyInformationService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _emergencyInformationService.GetPersonEmergencyContactsAsync(page.Offset, page.Limit, bypassCache, person);

                AddEthosContextProperties(
                    await _emergencyInformationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _emergencyInformationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonContactSubject>>(pageOfItems.Item1, page, pageOfItems.Item2, Request);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
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
        /// Gets persons emergency contact information
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.PersonContactSubject</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(BasePermissionCodes.ViewAnyPersonContact)]
        public async Task<Dtos.PersonContactSubject> GetPersonEmergencyContactsByIdAsync([FromUri] string id)
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
                _emergencyInformationService.ValidatePermissions(GetPermissionsMetaData());
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person contact id is required");
                }

                AddEthosContextProperties(
                    await _emergencyInformationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache), 
                    await _emergencyInformationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return await _emergencyInformationService.GetPersonEmergencyContactByIdAsync(id);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (IntegrationApiException e)
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
        /// Create a new person contact
        /// </summary>
        /// <param name="personContactSubject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Dtos.PersonContactSubject> PostPersonContactAsync([FromBody] Dtos.PersonContactSubject personContactSubject)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update an existing person contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personContactSubject"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.PersonContactSubject> PutPersonContactAsync([FromUri] string id, [FromBody] Dtos.PersonContactSubject personContactSubject)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete a person contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeletePersonContactAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}