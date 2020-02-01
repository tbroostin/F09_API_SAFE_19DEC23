// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Vendors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class VendorsController : BaseCompressedApiController
    {
        private readonly IVendorsService _vendorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the VendorsController class.
        /// </summary>
        /// <param name="vendorsService">Service of type <see cref="IVendorsService">IVendorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public VendorsController(IVendorsService vendorsService, ILogger logger)
        {
            _vendorsService = vendorsService;
            _logger = logger;
        }
        #region EEDM vendors v8
        /// <summary>
        /// Return all vendors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        /// <returns>List of Vendors <see cref="Vendors"/> objects representing matching vendors</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Vendors))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetVendorsAsync(Paging page, QueryStringFilter criteria)
        {
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
                page = new Paging(100, 0);
            }

            var criteriaObj = GetFilterObject<Vendors>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Vendors>>(new List<Vendors>(), page, this.Request);

            var criteriaValue = new Dtos.Filters.VendorFilter();
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Institution != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Institution.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Institution.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Organization != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Organization.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Organization.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Person!= null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Person.Id))
                criteriaValue.vendorDetail = criteriaObj.VendorDetail.Person.Id;
            if (criteriaObj.Classifications != null && criteriaObj.Classifications.Any() && !string.IsNullOrEmpty(criteriaObj.Classifications.FirstOrDefault().Id))
            {
                criteriaValue.classifications = criteriaObj.Classifications.FirstOrDefault().Id;
            }
            if (criteriaObj.Statuses != null && criteriaObj.Statuses.Any())
            {
                criteriaValue.statuses = new List<string>();
                foreach (var status in criteriaObj.Statuses)
                {
                    criteriaValue.statuses.Add(status.ToString());
                }
            }
            if (criteriaObj.relatedReference != null && criteriaObj.relatedReference.Any())
            {
                // Not supported in Colleague therefore always return an empty set.
                return new PagedHttpActionResult<IEnumerable<Vendors>>(new List<Vendors>(), page, this.Request);
            }

            try
            {
                var pageOfItems = await _vendorsService.GetVendorsAsync(page.Offset, page.Limit, criteriaValue, bypassCache);

                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Vendors>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a vendor using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Vendors> GetVendorsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                var vendor = await _vendorsService.GetVendorsByGuidAsync(guid);

                if (vendor != null)
                {

                    AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { vendor.Id }));
                }


                return vendor;

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
        /// Create (POST) a new vendor
        /// </summary>
        /// <param name="vendor">DTO of the new vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Vendors> PostVendorsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors vendor)
        {
            if (vendor == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null vendor argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                ValidateVendor(vendor);

                var vendorDetail = vendor.VendorDetail;

                if ((vendorDetail.Institution != null) && ((string.IsNullOrEmpty(vendorDetail.Institution.Id))
                     || (string.Equals(vendorDetail.Institution.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Institution", "The institution id is required when submitting a vendorDetail institution. ");
                }
                if ((vendorDetail.Organization != null) && ((string.IsNullOrEmpty(vendorDetail.Organization.Id))
                     || (string.Equals(vendorDetail.Organization.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Organization", "The organization id is required when submitting a vendorDetail organization. ");
                }
                if ((vendorDetail.Person != null) && ((string.IsNullOrEmpty(vendorDetail.Person.Id))
                    || (string.Equals(vendorDetail.Person.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Person", "The person id is required when submitting a vendorDetail person. ");
                }

                //call import extend method that needs the extracted extension data and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var vendorReturn = await _vendorsService.PostVendorAsync(vendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { vendorReturn.Id }));

                return vendorReturn;
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
        /// Update (PUT) an existing vendor
        /// </summary>
        /// <param name="guid">GUID of the vendor to update</param>
        /// <param name="vendor">DTO of the updated vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Vendors> PutVendorsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors vendor)
        {
            ValidateUpdateRequest(guid, vendor);

            try
            {
                //get Data Privacy List
                var dpList = await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var mergedVendor = 
                    await PerformPartialPayloadMerge(vendor, async () => await _vendorsService.GetVendorsByGuidAsync(guid), 
                    dpList, _logger);

                if (vendor.VendorDetail == null)
                {
                    throw new ArgumentNullException("The vendorDetail is required when submitting a vendor.");
                }

                if (vendor.VendorDetail.Institution != null || mergedVendor.VendorDetail.Institution != null)
                {
                    if (vendor.VendorDetail.Institution == null || mergedVendor.VendorDetail.Institution == null || vendor.VendorDetail.Institution.Id != mergedVendor.VendorDetail.Institution.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    } 
                }

                if (vendor.VendorDetail.Organization != null || mergedVendor.VendorDetail.Organization != null)
                {
                    if (vendor.VendorDetail.Organization == null || mergedVendor.VendorDetail.Organization == null || vendor.VendorDetail.Organization.Id != mergedVendor.VendorDetail.Organization.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                if ( vendor.VendorDetail.Person != null || mergedVendor.VendorDetail.Person != null)
                {
                    if (vendor.VendorDetail.Person == null || mergedVendor.VendorDetail.Person == null || vendor.VendorDetail.Person.Id != mergedVendor.VendorDetail.Person.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                ValidateVendor(mergedVendor);

                var vendorReturn = await _vendorsService.PutVendorAsync(guid, mergedVendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return vendorReturn;
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
        /// Delete (DELETE) a vendor
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        [HttpDelete]
        public async Task DeleteVendorsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Helper method to validate vendors PUT/POST.
        /// </summary>
        /// <param name="vendor">Vendors DTO object of type <see cref="Dtos.Vendors"/></param>

        private void ValidateVendor(Vendors vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("Vendor", "The id is required when submitting a vendor. ");
            }

            if (vendor.EndOn != null)
            {
                throw new ArgumentNullException("Vendor.EndOn", "The endOn date can not be updated when submitting a vendor. ");
            }

            if (vendor.VendorDetail == null)
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "The vendorDetail is required when submitting a vendor. ");
            }

            var vendorDetail = vendor.VendorDetail;
            if ((vendorDetail.Institution == null) && (vendorDetail.Organization == null) && (vendorDetail.Person == null))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Either a Institution, Organizatation, or Person is required when submitting a vendorDetail. ");
            }

            if ((vendorDetail.Organization != null) && ((vendorDetail.Person != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Person != null) && ((vendorDetail.Organization != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Institution != null) && ((vendorDetail.Person != null) || (vendorDetail.Organization != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }


            if (vendor.Classifications != null)
            {
                foreach (var classification in vendor.Classifications)
                {
                    if (string.IsNullOrEmpty(classification.Id))
                        throw new ArgumentNullException("Vendor.Classification", "The classification id is required when submitting classifications. ");
                }
            }

            if (vendor.PaymentTerms != null)
            {
                foreach (var paymentTerm in vendor.PaymentTerms)
                {
                    if (string.IsNullOrEmpty(paymentTerm.Id))
                        throw new ArgumentNullException("Vendor.PaymentTerms", "The paymentTerms id is required when submitting paymentTerms. ");
                }
            }

            if (vendor.VendorHoldReasons != null)
            {
                foreach (var vendorHoldReason in vendor.VendorHoldReasons)
                {
                    if (string.IsNullOrEmpty(vendorHoldReason.Id))
                    {
                        throw new ArgumentNullException("Vendor.VendorHoldReasons", "The vendorHoldReason id is required when submitting vendorHoldReasons. ");
                    }
                }
            }

        }

        
        /// <summary>
        /// Validate the request on Put meets conditions for guid consistency 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="request"></param>
        private void ValidateUpdateRequest(string guid, BaseModel2 request)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (request == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                request.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(request.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != request.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
        }

        #endregion

        #region EEDM Vendors v11

        /// <summary>
        /// Return all vendors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">The default named query implementation for filtering</param>
        /// <returns>List of Vendors <see cref="Dtos.Vendors2"/> objects representing matching vendors</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Vendors2))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetVendorsAsync2(Paging page, QueryStringFilter criteria)
        {
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
                page = new Paging(100, 0);
            }

            string vendorDetails = "";
            List<string> relatedReferences = null, statuses = null, classifications = null, types = null;

            var criteriaObj = GetFilterObject<Vendors2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Vendors2>>(new List<Vendors2>(), page, this.Request);

            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Institution != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Institution.Id))
                vendorDetails = criteriaObj.VendorDetail.Institution.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Organization != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Organization.Id))
                vendorDetails = criteriaObj.VendorDetail.Organization.Id;
            if (criteriaObj.VendorDetail != null && criteriaObj.VendorDetail.Person != null && !string.IsNullOrEmpty(criteriaObj.VendorDetail.Person.Id))
                vendorDetails = criteriaObj.VendorDetail.Person.Id;
            if (criteriaObj.Classifications != null && criteriaObj.Classifications.Any())
            {
                classifications = criteriaObj.Classifications.Select(vc => vc.Id).ToList();
            }
            if (criteriaObj.Statuses != null && criteriaObj.Statuses.Any())
            {
                statuses = new List<string>();
                foreach (var status in criteriaObj.Statuses)
                {
                    statuses.Add(status.ToString());
                }
            }
            if (criteriaObj.relatedReference != null && criteriaObj.relatedReference.Any())
            {
                relatedReferences = criteriaObj.relatedReference.Select(rr => rr.Type.ToString()).ToList();
                // We don't support "paymentVendor" therefore return an empty set.
                if (relatedReferences.Contains("PaymentVendor"))
                    return new PagedHttpActionResult<IEnumerable<Vendors2>>(new List<Vendors2>(), page, this.Request);
            }
            if (criteriaObj.Types != null && criteriaObj.Types.Any())
            {
                types = new List<string>();
                foreach (var type in criteriaObj.Types)
                {
                    types.Add(type.ToString());
                }
            }

            try
            {
                var pageOfItems = await _vendorsService.GetVendorsAsync2(page.Offset, page.Limit, vendorDetails, classifications,
                    statuses, relatedReferences, types, bypassCache);

                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Vendors2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a vendor using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors2"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Vendors2> GetVendorsByGuidAsync2(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                var vendor = await _vendorsService.GetVendorsByGuidAsync2(guid);

                if (vendor != null)
                {

                    AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { vendor.Id }));
                }


                return vendor;
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
        /// Create (POST) a new vendor
        /// </summary>
        /// <param name="vendor">DTO of the new vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Vendors2> PostVendorsAsync2([ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors2 vendor)
        {
            if (vendor == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null vendor argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                ValidateVendor2(vendor);

                var vendorDetail = vendor.VendorDetail;

                if ((vendorDetail.Institution != null) && ((string.IsNullOrEmpty(vendorDetail.Institution.Id))
                     || (string.Equals(vendorDetail.Institution.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Institution", "The institution id is required when submitting a vendorDetail institution. ");
                }
                if ((vendorDetail.Organization != null) && ((string.IsNullOrEmpty(vendorDetail.Organization.Id))
                     || (string.Equals(vendorDetail.Organization.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Organization", "The organization id is required when submitting a vendorDetail organization. ");
                }
                if ((vendorDetail.Person != null) && ((string.IsNullOrEmpty(vendorDetail.Person.Id))
                    || (string.Equals(vendorDetail.Person.Id, Guid.Empty.ToString()))))
                {
                    throw new ArgumentNullException("Vendor.VendorDetail.Person", "The person id is required when submitting a vendorDetail person. ");
                }


                //call import extend method that needs the extracted extension data and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var vendorReturn = await _vendorsService.PostVendorAsync2(vendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { vendorReturn.Id }));

                return vendorReturn;
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
        /// Update (PUT) an existing vendor
        /// </summary>
        /// <param name="guid">GUID of the vendor to update</param>
        /// <param name="vendor">DTO of the updated vendor</param>
        /// <returns>A vendor object <see cref="Dtos.Vendors2"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Vendors2> PutVendorsAsync2([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Vendors2 vendor)
        {
            ValidateUpdateRequest2(guid, vendor);

            try
            {
                //get Data Privacy List
                var dpList = await _vendorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _vendorsService.ImportExtendedEthosData(await ExtractExtendedData(await _vendorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var mergedVendor =
                    await PerformPartialPayloadMerge(vendor, async () => await _vendorsService.GetVendorsByGuidAsync2(guid),
                    dpList, _logger);

                if(vendor.VendorDetail == null)
                {
                    throw new ArgumentNullException("The vendorDetail is required when submitting a vendor.");
                }

                if (vendor.VendorDetail.Institution != null || mergedVendor.VendorDetail.Institution != null)
                {
                    if (vendor.VendorDetail.Institution == null || mergedVendor.VendorDetail.Institution == null || vendor.VendorDetail.Institution.Id != mergedVendor.VendorDetail.Institution.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                if (vendor.VendorDetail.Organization != null || mergedVendor.VendorDetail.Organization != null)
                {
                    if (vendor.VendorDetail.Organization == null || mergedVendor.VendorDetail.Organization == null || vendor.VendorDetail.Organization.Id != mergedVendor.VendorDetail.Organization.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                if (vendor.VendorDetail.Person != null || mergedVendor.VendorDetail.Person != null)
                {
                    if (vendor.VendorDetail.Person == null || mergedVendor.VendorDetail.Person == null || vendor.VendorDetail.Person.Id != mergedVendor.VendorDetail.Person.Id)
                    {
                        throw new ArgumentException("Updates to vendorDetail are not permitted.");
                    }
                }

                ValidateVendor2(mergedVendor);
                var vendorReturn = await _vendorsService.PutVendorAsync2(guid, mergedVendor);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await _vendorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return vendorReturn;
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
        /// Helper method to validate vendors PUT/POST.
        /// </summary>
        /// <param name="vendor">Vendors DTO object of type <see cref="Dtos.Vendors"/></param>

        private void ValidateVendor2(Vendors2 vendor)
        {
            if (vendor == null)
            {
                throw new ArgumentNullException("Vendor", "The id is required when submitting a vendor. ");
            }

            if (vendor.EndOn != null)
            {
                throw new ArgumentNullException("Vendor.EndOn", "The endOn date can not be updated when submitting a vendor. ");
            }

            if (vendor.VendorDetail == null)
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "The vendorDetail is required when submitting a vendor. ");
            }

            var vendorDetail = vendor.VendorDetail;
            if ((vendorDetail.Institution == null) && (vendorDetail.Organization == null) && (vendorDetail.Person == null))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Either a Institution, Organizatation, or Person is required when submitting a vendorDetail. ");
            }

            if ((vendorDetail.Organization != null) && ((vendorDetail.Person != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Person != null) && ((vendorDetail.Organization != null) || (vendorDetail.Institution != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }
            if ((vendorDetail.Institution != null) && ((vendorDetail.Person != null) || (vendorDetail.Organization != null)))
            {
                throw new ArgumentNullException("Vendor.VendorDetail", "Only one of either an organization, person or institution can be specified as a vendor. ");
            }


            if (vendor.Classifications != null)
            {
                foreach (var classification in vendor.Classifications)
                {
                    if (string.IsNullOrEmpty(classification.Id))
                        throw new ArgumentNullException("Vendor.Classification", "The classification id is required when submitting classifications. ");
                }
            }

            if (vendor.PaymentTerms != null)
            {
                foreach (var paymentTerm in vendor.PaymentTerms)
                {
                    if (string.IsNullOrEmpty(paymentTerm.Id))
                        throw new ArgumentNullException("Vendor.PaymentTerms", "The paymentTerms id is required when submitting paymentTerms. ");
                }
            }

            if (vendor.VendorHoldReasons != null)
            {
                foreach (var vendorHoldReason in vendor.VendorHoldReasons)
                {
                    if (string.IsNullOrEmpty(vendorHoldReason.Id))
                    {
                        throw new ArgumentNullException("Vendor.VendorHoldReasons", "The vendorHoldReason id is required when submitting vendorHoldReasons. ");
                    }
                }
            }

        }


        /// <summary>
        /// Validate the request on Put meets conditions for guid consistency 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="request"></param>
        private void ValidateUpdateRequest2(string guid, BaseModel2 request)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (request == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                request.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(request.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != request.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
        }

        #endregion

        /// <summary>
        /// Get the list of vendors based on keyword search.
        /// </summary>
        /// <param name="searchCriteria"> The search criteria containing keyword for vendor search.</param>
        /// <returns> The vendor search results</returns>      
        /// <accessComments>
        /// Requires at least one of the permissions VIEW.VENDOR, CREATE.UPDATE.REQUISITION and CREATE.UPDATE.PURCHASE.ORDER.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<VendorSearchResult>> QueryVendorsByPostAsync(VendorSearchCriteria searchCriteria)
        {
            if (searchCriteria==null)
            {
                string message = "Vendor search criteria must be specified.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(searchCriteria.QueryKeyword))
            {
                string message = "query keyword is required to query.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var vendorSearchResults = await _vendorsService.QueryVendorsByPostAsync(searchCriteria);
                return vendorSearchResults;
            }            
            catch (ArgumentNullException anex)
            {
                _logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to search vendors", HttpStatusCode.BadRequest);
            }
        }
    }

}