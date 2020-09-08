// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to PersonVisas data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class PersonVisasController : BaseCompressedApiController
    {
        private readonly IPersonVisasService _personVisasService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        #region ..ctor
        /// <summary>
        /// Initializes a new instance of the PersonVisasController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="personVisasService">Service of type <see cref="IPersonVisasService">IPersonVisasService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonVisasController(IAdapterRegistry adapterRegistry, IPersonVisasService personVisasService, ILogger logger)
        {
            _personVisasService = personVisasService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }
        #endregion

        #region GET

        /// <summary>
        /// Gets all person visa information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = false)]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        public async Task<IHttpActionResult> GetAllPersonVisasAsync(Paging page, [FromUri] string person = "")
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

                if (person == null)
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonVisa>>(new List<Dtos.PersonVisa>(), page, 0, this.Request);
                }

                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _personVisasService.GetAllAsync(page.Offset, page.Limit, person, bypassCache);

                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonVisa>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Gets all person visa information
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.PersonVisa))]
        public async Task<IHttpActionResult> GetAllPersonVisas2Async(Paging page, QueryStringFilter criteria)
        {
            string person = string.Empty, visaTypeDetail = string.Empty, visaTypeCategory = string.Empty;

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

                var criteriaObj = GetFilterObject<Dtos.PersonVisa>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    person = criteriaObj.Person != null ? criteriaObj.Person.Id : string.Empty;
                    visaTypeDetail = (criteriaObj.VisaType != null && criteriaObj.VisaType.Detail != null) ? criteriaObj.VisaType.Detail.Id : string.Empty;
                    visaTypeCategory = criteriaObj.VisaType != null ? 
                        (criteriaObj.VisaType.VisaTypeCategory.Equals(Dtos.VisaTypeCategory.Immigrant) ? 
                        "immigrant" : (criteriaObj.VisaType.VisaTypeCategory.Equals(Dtos.VisaTypeCategory.NonImmigrant) ? 
                        "nonImmigrant" : string.Empty)) : string.Empty;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PersonVisa>>(new List<Dtos.PersonVisa>(), page, 0, this.Request);
                
                var pageOfItems = await _personVisasService.GetAll2Async(page.Offset, page.Limit, person, visaTypeCategory, visaTypeDetail, bypassCache);

                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonVisa>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Retrieves a person visa by ID.
        /// </summary>
        /// <param name="id">Id of person visa to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonVisa">person visa</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.PersonVisa> GetPersonVisaByIdAsync([FromUri] string id)
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
                    throw new ArgumentNullException("Must provide an id for person-visas.");
                }

                var personVisa = await _personVisasService.GetPersonVisaByIdAsync(id);

                if (personVisa != null)
                {

                    AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personVisa.Id }));
                }


                return personVisa;
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
        /// Retrieves a person visa by ID.
        /// </summary>
        /// <param name="id">Id of person visa to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.PersonVisa">person visa</see></returns>
        [EedmResponseFilter, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Ellucian.Colleague.Dtos.PersonVisa> GetPersonVisaById2Async([FromUri] string id)
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
                    // throw new ArgumentNullException("Must provide an id for person-visas.");
                    var exception = new IntegrationApiException();
                    exception.AddError(new IntegrationApiError("Missing.Guid",
                            "The GUID must be provided in the URL.",
                                "Please provide a GUID in the URL."));
                    throw exception;
                }

                var personVisa = await _personVisasService.GetPersonVisaById2Async(id);

                if (personVisa != null)
                {

                    AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { personVisa.Id }));
                }


                return personVisa;
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

        #region POST
        /// <summary>
        /// Creates a PersonVisa.
        /// </summary>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonVisa">PersonVisa</see></returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.PersonVisa> PostPersonVisaAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                if (personVisa == null)
                {
                    throw new ArgumentNullException("Must provide a person-visas.");
                }

                if (string.IsNullOrEmpty(personVisa.Id))
                {
                    throw new ArgumentNullException("Must provide an id for person-visas in request body.");
                }

                if (personVisa.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("person-visas", "Nil GUID must be used in POST operation.");
                }

                Validate(personVisa);

                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn  = await _personVisasService.PostPersonVisaAsync(personVisa);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personVisaReturn.Id }));

                return personVisaReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a PersonVisa.
        /// </summary>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.PersonVisa">PersonVisa</see></returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.PersonVisa> PostPersonVisa2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
              
                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PostPersonVisa2Async(personVisa);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { personVisaReturn.Id }));

                return personVisaReturn;

            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }
        #endregion

        #region PUT
        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="id">id of the personVisa to update</param>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Updated <see cref="Dtos.PersonVisa">Dtos.PersonVisa</see></returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PersonVisa> PutPersonVisaAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Must provide an id for person-visas.");
                }

                if (personVisa == null)
                {
                    throw new ArgumentNullException("Must provide a person-visas.");
                }

                if (string.IsNullOrEmpty(personVisa.Id))
                {
                    throw new ArgumentNullException("Must provide an id for person-visas in request body.");
                }

                if (!id.Equals(personVisa.Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("id in URL is not the same as in request body.");
                }

                //Validate(personVisa);

                //get Data Privacy List
                var dpList = await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PutPersonVisaAsync(id,
                  await PerformPartialPayloadMerge(personVisa, async () => await _personVisasService.GetPersonVisaByIdAsync(id),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personVisaReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a person visa.
        /// </summary>
        /// <param name="id">id of the personVisa to update</param>
        /// <param name="personVisa"><see cref="Dtos.PersonVisa">personVisa</see> to create</param>
        /// <returns>Updated <see cref="Dtos.PersonVisa">Dtos.PersonVisa</see></returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.PersonVisa> PutPersonVisa2Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.PersonVisa personVisa)
        {
            try
            {
               
                //get Data Privacy List
                var dpList = await _personVisasService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _personVisasService.ImportExtendedEthosData(await ExtractExtendedData(await _personVisasService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var personVisaReturn = await _personVisasService.PutPersonVisa2Async(id,
                  await PerformPartialPayloadMerge(personVisa, async () => await _personVisasService.GetPersonVisaById2Async(id),
                  dpList, _logger));

                AddEthosContextProperties(dpList,
                    await _personVisasService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return personVisaReturn;
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }           
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        #endregion

        #region DELETE
        /// <summary>
        /// Delete (DELETE) an existing PersonVisa
        /// </summary>
        /// <param name="id">id of the PersonVisa to delete</param>
        [HttpDelete]
        public async Task DeletePersonVisaAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        #endregion

      
    }
}