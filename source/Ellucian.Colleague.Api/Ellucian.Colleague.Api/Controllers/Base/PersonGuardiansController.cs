// Copyright 2016 - 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides person guardian relationships data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonGuardiansController : BaseCompressedApiController
    {
        private readonly IPersonGuardianRelationshipService _personGuardianRelationshipService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;        

       /// <summary>
       /// Person guardian constructor
       /// </summary>
       /// <param name="adapterRegistry"></param>
       /// <param name="personGuardianRelationshipService"></param>
       /// <param name="logger"></param>
        public PersonGuardiansController(IAdapterRegistry adapterRegistry, IPersonGuardianRelationshipService personGuardianRelationshipService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _personGuardianRelationshipService = personGuardianRelationshipService;
            _logger = logger;
        }

        #region GET Methods
        
        /// <summary>
        /// Retrieves active personal guardian relationship for relationship id
        /// </summary>
        /// <returns>PersonGuardianRelationship object for a personal guardian relationship.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonGuardianRelationship> GetPersonGuardianRelationshipByIdAsync([FromUri] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Id cannot be null.");
                }
                 AddEthosContextProperties(
                 await _personGuardianRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _personGuardianRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));

                return await _personGuardianRelationshipService.GetPersonGuardianRelationshipByIdAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Returns all or filtered records
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="person"></param>
        /// <returns>Tuple containing list of PersonGuardianRelationships <see cref="Dtos.PersonGuardianRelationship"/> objects.</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        public async Task<IHttpActionResult> GetPersonGuardianRelationshipsAllAndFilterAsync(Paging page, [FromUri] string person = "")
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

                if (person == null)
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonGuardianRelationship>>(new List<Dtos.PersonGuardianRelationship>(), page, 0, this.Request);
                }

                var pageOfItems = await _personGuardianRelationshipService.GetPersonGuardianRelationshipsAllAndFilterAsync(page.Offset, page.Limit, person);
                AddEthosContextProperties(
                   await _personGuardianRelationshipService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personGuardianRelationshipService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonGuardianRelationship>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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

        #region PUT method
        /// <summary>
        /// Updates personal guardian relationship
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalGuardianRelationship">personGuardianRelationship</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.PersonalRelationship> PutPersonGuardianRelationshipAsync([FromUri] string id, [FromBody] Dtos.PersonGuardianRelationship personalGuardianRelationship)
        {
            //PUT is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region POST method
        /// <summary>
        /// Create new personal guardian relationship
        /// </summary>
        /// <param name="personGuardianRelationship">personGuardianRelationship</param>
        /// <returns></returns>
        //[HttpPost]
        public async Task<Dtos.PersonalRelationship> PostPersonGuardianRelationshipAsync([FromBody] Dtos.PersonGuardianRelationship personGuardianRelationship)
        {
            //POST is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion
        
        #region DELETE method
        /// <summary>
        /// Delete of personal guardian relationship is not supported
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePersonGuardianRelationshipAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}