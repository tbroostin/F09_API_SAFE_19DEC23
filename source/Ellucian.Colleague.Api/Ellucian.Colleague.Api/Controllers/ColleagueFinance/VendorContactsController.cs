//Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to VendorContacts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorContactsController : BaseCompressedApiController
    {
        private readonly IVendorContactsService _vendorContactsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorContactsController class.
        /// </summary>
        /// <param name="vendorContactsService">Service of type <see cref="IVendorContactsService">IVendorContactsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public VendorContactsController(IVendorContactsService vendorContactsService, ILogger logger)
        {
            _vendorContactsService = vendorContactsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all vendorContacts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria"></param>
        /// <returns>List of VendorContacts <see cref="Dtos.VendorContacts"/> objects representing matching vendorContacts</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(VendorContacts))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetVendorContactsAsync(Paging page, QueryStringFilter criteria)
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
                _vendorContactsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.VendorContacts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.VendorContacts>>(new List<Dtos.VendorContacts>(), page, 0, this.Request);

                var pageOfItems = await _vendorContactsService.GetVendorContactsAsync(page.Offset, page.Limit, criteriaObj, bypassCache);

                AddEthosContextProperties(
                  await _vendorContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _vendorContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.VendorContacts>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a vendorContacts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(ColleagueFinancePermissionCodes.ViewVendorContacts)]
        public async Task<Dtos.VendorContacts> GetVendorContactsByGuidAsync(string guid)
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
                _vendorContactsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                   await _vendorContactsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _vendorContactsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _vendorContactsService.GetVendorContactsByGuidAsync(guid);
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
        /// Create (POST) a new vendorContacts
        /// </summary>
        /// <param name="vendorContacts">DTO of the new vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.VendorContacts> PostVendorContactsAsync([FromBody] Dtos.VendorContacts vendorContacts)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Update (PUT) an existing vendorContacts
        /// </summary>
        /// <param name="guid">GUID of the vendorContacts to update</param>
        /// <param name="vendorContacts">DTO of the updated vendorContacts</param>
        /// <returns>A vendorContacts object <see cref="Dtos.VendorContacts"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.VendorContacts> PutVendorContactsAsync([FromUri] string guid, [FromBody] Dtos.VendorContacts vendorContacts)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) a vendorContacts
        /// </summary>
        /// <param name="guid">GUID to desired vendorContacts</param>
        [HttpDelete]
        public async Task DeleteVendorContactsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #region vendor-contact-initiation-process v1.0.0

        /// <summary>
        /// Create (POST) a new vendor-contact-initiation-Process.
        /// </summary>
        /// <param name="vendorContactInitiationProcess"></param>
        /// <returns></returns>
        [HttpPost, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<object> PostVendorContactInitiationProcessAsync([ModelBinder(typeof(EedmModelBinder))] VendorContactInitiationProcess vendorContactInitiationProcess)
        {
            try
            {
                var returnObject = await _vendorContactsService.CreateVendorContactInitiationProcessAsync(vendorContactInitiationProcess);

                var resourceName = string.Empty;
                var resourceGuid = string.Empty;
                var version = string.Empty;

                var type = returnObject.GetType();
                if (type == typeof(Dtos.VendorContacts))
                {
                    resourceName = "vendor-contacts";
                    resourceGuid = (returnObject as Dtos.VendorContacts).Id;
                    version = "1.0.0";
                }
                else
                {
                    resourceName = "person-matching-requests";
                    resourceGuid = (returnObject as Dtos.PersonMatchingRequests).Id;
                    version = "1.0.0";
                }
                string customMediaType = string.Format(IntegrationCustomMediaType, resourceName, version);
                CustomMediaTypeAttributeFilter.SetCustomMediaType(customMediaType);

                //store dataprivacy list and get the extended data to store 
                var resource = new Web.Http.EthosExtend.EthosResourceRouteInfo()
                {
                    ResourceName = resourceName,
                    ResourceVersionNumber = version,
                    BypassCache = true
                };

                AddEthosContextProperties(await _vendorContactsService.GetDataPrivacyListByApi(resourceName, true),
                   await _vendorContactsService.GetExtendedEthosDataByResource(resource, new List<string>() { resourceGuid }));

                return returnObject;
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
        /// Update a Vendor Contact Initiation Process in Colleague (Not Supported).
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="vendorContactsDto"></param>
        /// <returns></returns>
        [HttpPut]
        public Ellucian.Colleague.Dtos.VendorContacts PutVendorContactInitiationProcess([FromUri] string guid, [FromBody] VendorContacts vendorContactsDto)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Get a Vendor Contact Initiation Process in Colleague (Not Supported).
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        public Ellucian.Colleague.Dtos.VendorContacts GetVendorContactInitiationProcess([FromUri] string guid = null)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Vendor Contact Initiation Process.
        /// </summary>
        /// <param name="guid"></param>
        [HttpDelete]
        public void DeleteVendorContactInitiationProcess([FromUri] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}