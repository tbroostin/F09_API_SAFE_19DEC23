// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides personal relationships data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonalRelationshipsController : BaseCompressedApiController
    {
        private readonly IPersonalRelationshipsService _personalRelationshipsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;        

       /// <summary>
       /// Personal relationships constructor
       /// </summary>
       /// <param name="adapterRegistry"></param>
       /// <param name="personalRelationshipsService"></param>
       /// <param name="logger"></param>
        public PersonalRelationshipsController(IAdapterRegistry adapterRegistry, IPersonalRelationshipsService personalRelationshipsService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _personalRelationshipsService = personalRelationshipsService;
            _logger = logger;
        }

        #region GET Methods
        
        /// <summary>
        /// Retrieves all active personal relationships for relationship id
        /// </summary>
        /// <returns>PersonalRelationship object for a personal relationship.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonalRelationship> GetPersonalRelationshipByIdAsync([FromUri] string id)
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

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Id cannot be null.");
                }

               AddEthosContextProperties(
                    await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _personalRelationshipsService.GetPersonalRelationshipByIdAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
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
        /// Return a list of Personal Relationships objects based on selection criteria.
        /// </summary>
        /// <param name="page">Personal Relationship page Contains ...page...</param>
        /// <param name="subjectPerson">Personal Relationship subjectPerson Contains ...subjectPerson...</param>
        /// <param name="relatedPerson">Personal Relationship relatedPerson Contains ...relatedPerson...</param>
        /// <param name="directRelationshipType">Personal Relationship directRelationship Contains ...directRelationship...</param>
        /// <param name="directRelationshipDetailId">Personal Relationship reciprocalRelationship Contains ...reciprocalRelationship...</param>
        // <returns>List of PersonalRelationship <see cref="Dtos.PersonalRelationship"/> objects representing matching personal relationships</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(new string[] { "subjectPerson", "relatedPerson", "directRelationshipType", "directRelationshipDetailId" }, false, true)]
        public async Task<IHttpActionResult> GetPersonalRelationshipsAsync(Paging page, [FromUri] string subjectPerson = "",
            [FromUri] string relatedPerson = "", [FromUri] string directRelationshipType = "", [FromUri] string directRelationshipDetailId = "")
        {
            if (subjectPerson == null || relatedPerson == null || directRelationshipType == null || directRelationshipDetailId == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.PersonalRelationship>>(new List<Dtos.PersonalRelationship>(), page, 0, this.Request);
            }

            string criteria = string.Concat(subjectPerson,relatedPerson,directRelationshipType,directRelationshipDetailId);
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

            Tuple<IEnumerable<Dtos.PersonalRelationship>, int> pageOfItems = null;

            try
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    pageOfItems = await _personalRelationshipsService.GetAllPersonalRelationshipsAsync(page.Offset, page.Limit, bypassCache);

                }
                else
                {
                    //valid query parameter but empty argument
                    if ((string.IsNullOrEmpty(criteria.Replace("\"", ""))) || (string.IsNullOrEmpty(criteria.Replace("'", ""))))
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.PersonalRelationship>>(new List<Dtos.PersonalRelationship>(), page, 0, this.Request);
                    }

                    if ((!string.IsNullOrEmpty(directRelationshipType)) && (!ValidEnumerationValue(typeof(PersonalRelationshipType), directRelationshipType)))
                    {
                        throw new Exception(string.Concat("'", directRelationshipType, "' is an invalid enumeration value. "));
                    }
                   
                    pageOfItems = await _personalRelationshipsService.GetPersonalRelationshipsByFilterAsync(page.Offset, page.Limit, subjectPerson, relatedPerson, directRelationshipType, directRelationshipDetailId);
                }

                AddEthosContextProperties(
                   await _personalRelationshipsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personalRelationshipsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedHttpActionResult<IEnumerable<Dtos.PersonalRelationship>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }    

        #endregion

        #region PUT method
        /// <summary>
        /// Updates personal relationship
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalRelationship"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<Dtos.PersonalRelationship> PutPersonalRelationshipAsync([FromUri] string id, [FromBody] Dtos.PersonalRelationship personalRelationship)
        {
            //PUT is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

        #region POST method
        /// <summary>
        /// Create new personal relationship
        /// </summary>
        /// <param name="personRelationship">personRelationship</param>
        /// <returns></returns>
        //[HttpPost]
        public async Task<Dtos.PersonalRelationship> PostPersonalRelationshipAsync([FromBody] Dtos.PersonalRelationship personRelationship)
        {
            //POST is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion
        
        #region DELETE method
        /// <summary>
        /// Delete of personal relationship is not supported
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeletePersonRelationshipAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion
    }
}