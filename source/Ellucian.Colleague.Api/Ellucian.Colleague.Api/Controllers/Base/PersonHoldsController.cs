// Copyright 2016 - 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Utility;
using System.Net.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides person holds data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PersonHoldsController : BaseCompressedApiController
    {
        private readonly IPersonHoldsService _personHoldsService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;        

       /// <summary>
       /// Person holds constructor
       /// </summary>
       /// <param name="adapterRegistry"></param>
       /// <param name="personHoldsService"></param>
       /// <param name="logger"></param>
        public PersonHoldsController(IAdapterRegistry adapterRegistry, IPersonHoldsService personHoldsService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _personHoldsService = personHoldsService;
            _logger = logger;
        }

        #region GET Methods
        /// <summary>
        /// Returns a list of all active restrictions recorded for any person in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetPersonsActiveHoldsAsync(Paging page)
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

                var pageOfItems = await _personHoldsService.GetPersonHoldsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonHold>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all active person holds for hold id
        /// </summary>
        /// <returns>PersonHold object for a person.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonHold> GetPersonsActiveHoldAsync([FromUri] string id)
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
                var personHold = await _personHoldsService.GetPersonHoldAsync(id);

                if (personHold != null)
                {

                    AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personHold.Id }));
                }


                return personHold;
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
            catch (ArgumentNullException e)
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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all active person holds for person id
        /// </summary>
        /// <returns>PersonHold object for a person.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        public async Task<IEnumerable<Dtos.PersonHold>> GetPersonsActiveHoldsByPersonIdAsync([FromUri] string person)
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
                if (person == null)
                {
                    return new List<Dtos.PersonHold>();
                }

                var personHolds = await _personHoldsService.GetPersonHoldsAsync(person, bypassCache);

                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              personHolds.Select(a => a.Id).ToList()));

                return personHolds;
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
            catch (ArgumentNullException e)
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

        #region PUT method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personHold"></param>
        /// <returns></returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PersonHold> PutPersonHoldAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonHold personHold)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person hold id cannot be null or empty");
                }

                if (personHold == null)
                {
                    throw new ArgumentNullException("personHold cannot be null or empty");
                }

                if (!id.Equals(personHold.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("id does not match id in personHold.");
                }

                if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }

                if (string.IsNullOrEmpty(personHold.Id))
                {
                    personHold.Id = id.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _personHoldsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _personHoldsService.ImportExtendedEthosData(await ExtractExtendedData(await _personHoldsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var personHoldReturn = await _personHoldsService.UpdatePersonHoldAsync(id,
                    await PerformPartialPayloadMerge(personHold, async () => await _personHoldsService.GetPersonHoldAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personHoldReturn; 

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (InvalidOperationException e)
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
        #endregion

        #region POST method
        /// <summary>
        /// Create new person hold
        /// </summary>
        /// <param name="personHold">personHold</param>
        /// <returns></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.PersonHold> PostPersonHoldAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonHold personHold)
        {
                          if (personHold == null)
                {
                    throw CreateHttpResponseException(new IntegrationApiException("Null PersonHolds argument",
                   IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
                }
            try
            {
                if (string.IsNullOrEmpty(personHold.Id))
                {
                    throw new ArgumentNullException("Null personHold id", "Id is a required property.");
                }

                if (personHold.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("personHoldsDto", "On a post you can not define a GUID");
                }
                //call import extend method that needs the extracted extension data and the config
                await _personHoldsService.ImportExtendedEthosData(await ExtractExtendedData(await _personHoldsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the person hold
                var personHoldCreate = await _personHoldsService.CreatePersonHoldAsync(personHold);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personHoldsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personHoldsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personHoldCreate.Id }));

                return personHoldCreate;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        #endregion
        
        #region DELETE method
        /// <summary>
        /// Deletes person hold based on id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeletePersonHoldAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Person hold id cannot be null or empty");
                }
                await _personHoldsService.DeletePersonHoldAsync(id);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
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
            //return new HttpResponseMessage(HttpStatusCode.OK);
        }
        #endregion
    }
}