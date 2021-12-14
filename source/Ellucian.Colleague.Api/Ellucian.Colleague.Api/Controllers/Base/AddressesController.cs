// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Address data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AddressesController : BaseCompressedApiController
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IAddressService _addressService;

        /// <summary>
        /// Initializes a new instance of the AddressesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="addressService">Service of type <see cref="IAddressService">IAddressService</see></param>
        /// <param name="addressRepository">Repository of type <see cref="IAddressRepository">IAddressRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AddressesController(IAdapterRegistry adapterRegistry, IAddressService addressService, IAddressRepository addressRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _addressService = addressService;
            _addressRepository = addressRepository;
            _logger = logger;
        }

        #region Get Methods
        /// <summary>
        /// Get all current addresses for a person
        /// </summary>
        /// <param name="personId">Person to get addresses for</param>
        /// <returns>List of Address Objects <see cref="Ellucian.Colleague.Dtos.Base.Address">Address</see></returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.Address> GetPersonAddresses(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                _logger.Error("Invalid personId parameter");
                throw CreateHttpResponseException("The personId is required.", HttpStatusCode.BadRequest);
            }
            try
            {
                var addressDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Address>();
                var addressCollection = _addressRepository.GetPersonAddresses(personId);
                // Get the right adapter for the type mapping
                var addressDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>();
                // Map the Address entity to the Address DTO
                foreach (var address in addressCollection)
                {
                    addressDtoCollection.Add(addressDtoAdapter.MapToType(address));
                }

                return addressDtoCollection;
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Get a list of Addresses from a list of Person keys
        /// </summary>
        /// <param name="criteria">Address Query Criteria including PersonIds list.</param>
        /// <returns>List of Address Objects <see cref="Ellucian.Colleague.Dtos.Base.Address">Address</see></returns>
        /// <accessComments>User must have VIEW.ADDRESS permission or search for their own address(es)</accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Address>> QueryAddressesAsync(AddressQueryCriteria criteria)
        {
            if (criteria.PersonIds == null || criteria.PersonIds.Count() <= 0)
            {
                _logger.Error("Invalid personIds parameter: null or empty.");
                throw CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                await _addressService.QueryAddressPermissionAsync(criteria.PersonIds);

                var addressDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Address>();
                var addressCollection = _addressRepository.GetPersonAddressesByIds(criteria.PersonIds.ToList());
                // Get the right adapter for the type mapping
                var addressDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>();
                // Map the Address entity to the Address DTO
                foreach (var address in addressCollection)
                {
                    addressDtoCollection.Add(addressDtoAdapter.MapToType(address));
                }

                return addressDtoCollection;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message, "QueryAddresses error");
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryAddresses error");
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Read (GET) a Address using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired Address</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [EedmResponseFilter]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        public async Task<Dtos.Addresses> GetAddressByGuidAsync(string guid)
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
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var address = await _addressService.GetAddressesByGuidAsync(guid);

                if (address != null)
                {

                    AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { address.Id }));
                }


                return address;
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Read (GET) a Address using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired Address</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        public async Task<Dtos.Addresses> GetAddressByGuid2Async(string guid)
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
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var address = await _addressService.GetAddressesByGuid2Async(guid);

                if (address != null)
                {

                    AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { address.Id }));
                }
                return address;
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Get all addresses with paging
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAddressesAsync(Paging page)
        {
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var bypassCache = false;
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

                var pageOfItems = await _addressService.GetAddressesAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Addresses>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Get all addresses with paging
        /// </summary>
        /// <param name="page"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns></returns>
        [HttpGet, PermissionsFilter(new string[] { BasePermissionCodes.ViewAddress, BasePermissionCodes.UpdateAddress })]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAddresses2Async(Paging page, QueryStringFilter personFilter)
        {
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                var bypassCache = false;
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

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Addresses>>(new List<Dtos.Addresses>(), page, 0, this.Request);

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var pageOfItems = await _addressService.GetAddresses2Async(page.Offset, page.Limit, personFilterValue, bypassCache);

                AddEthosContextProperties(await _addressService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Addresses>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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

        #endregion

        #region Put Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Guid to Address in Colleague</param>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to update</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(BasePermissionCodes.UpdateAddress)]
        public async Task<Dtos.Addresses> PutAddressAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Addresses address)
        {
            if (address == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (id != address.Id)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Incorrect id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID in the URL doesn't match the GUID in the body of the request.")));
            }
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _addressService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _addressService.ImportExtendedEthosData(await ExtractExtendedData(await _addressService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var addressReturn = await _addressService.PutAddressesAsync(id,
                    await PerformPartialPayloadMerge(address, async () => await _addressService.GetAddressesByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return addressReturn;
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Update a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="id">Guid to Address in Colleague</param>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to update</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter, PermissionsFilter(BasePermissionCodes.UpdateAddress)]
        public async Task<Dtos.Addresses> PutAddress2Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Addresses address)
        {
            if (address == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null request body",
                    IntegrationApiUtility.GetDefaultApiError("The request body must be specified in the request.")));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (id != address.Id)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Incorrect id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID in the URL doesn't match the GUID in the body of the request.")));
            }
            try
            {
                _addressService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _addressService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _addressService.ImportExtendedEthosData(await ExtractExtendedData(await _addressService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var addressReturn = await _addressService.PutAddresses2Async(id,
                    await PerformPartialPayloadMerge(address, async () => await _addressService.GetAddressesByGuid2Async(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _addressService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return addressReturn;
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        #endregion

        #region Post Methods


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create a Address Record in Colleague (Not Supported)
        /// </summary>
        /// <param name="address"><see cref="Dtos.Addresses">Address</see> to create</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        [HttpPost]
        public async Task<Dtos.Addresses> PostAddressAsync([FromBody] Dtos.Addresses address)
        {        
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));        
        }
        #endregion

        #region Delete Methods
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Delete an existing Address in Colleague 
        /// </summary>
        /// <param name="id">Unique ID representing the Address to delete</param>
        [HttpDelete]
        public async Task DeleteAddressAsync(string id)
        {
            try
            {
                await _addressService.DeleteAddressesAsync(id);
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
    }
}