//Copyright 2017 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Coordination.Student.Services;
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

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionApplicationSupportingItems
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationSupportingItemsController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationSupportingItemsService _admissionApplicationSupportingItemsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationSupportingItemsController class.
        /// </summary>
        /// <param name="admissionApplicationSupportingItemsService">Service of type <see cref="IAdmissionApplicationSupportingItemsService">IAdmissionApplicationSupportingItemsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionApplicationSupportingItemsController(IAdmissionApplicationSupportingItemsService admissionApplicationSupportingItemsService, ILogger logger)
        {
            _admissionApplicationSupportingItemsService = admissionApplicationSupportingItemsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionApplicationSupportingItems
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of AdmissionApplicationSupportingItems <see cref="Dtos.AdmissionApplicationSupportingItems"/> objects representing matching admissionApplicationSupportingItems</returns>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAdmissionApplicationSupportingItemsAsync(Paging page)
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

                var pageOfItems = await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionApplicationSupportingItems>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a admissionApplicationSupportingItems using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItems</param>
        /// <returns>A admissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationSupportingItems> GetAdmissionApplicationSupportingItemsByGuidAsync(string guid)
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

                var returnval = await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
                AddEthosContextProperties(
                                        await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                        await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));
                return returnval;
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
        /// Update (PUT) an existing AdmissionApplicationSupportingItems
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationSupportingItems to update</param>
        /// <param name="admissionApplicationSupportingItems">DTO of the updated admissionApplicationSupportingItems</param>
        /// <returns>A AdmissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationSupportingItems> PutAdmissionApplicationSupportingItemsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (admissionApplicationSupportingItems == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationSupportingItems argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
            {
                admissionApplicationSupportingItems.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, admissionApplicationSupportingItems.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                await _admissionApplicationSupportingItemsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationSupportingItemsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var returnval =  await _admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(
                  await PerformPartialPayloadMerge(admissionApplicationSupportingItems, async () => await _admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid, true),
                  await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                  _logger));

                AddEthosContextProperties(
                                            await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                                            await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));
                return returnval;
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
        /// Create (POST) a new admissionApplicationSupportingItems
        /// </summary>
        /// <param name="admissionApplicationSupportingItems">DTO of the new admissionApplicationSupportingItems</param>
        /// <returns>A admissionApplicationSupportingItems object <see cref="Dtos.AdmissionApplicationSupportingItems"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationSupportingItems> PostAdmissionApplicationSupportingItemsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionApplicationSupportingItems admissionApplicationSupportingItems)
        {
            if (admissionApplicationSupportingItems == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid admissionApplicationSupportingItems.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(admissionApplicationSupportingItems.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null admissionApplicationSupportingItems id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (!admissionApplicationSupportingItems.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                await _admissionApplicationSupportingItemsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionApplicationSupportingItemsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var returnval = await _admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                AddEthosContextProperties(
                                           await _admissionApplicationSupportingItemsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                                           await _admissionApplicationSupportingItemsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { returnval.Id }));
                return returnval;
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
        /// Delete (DELETE) a admissionApplicationSupportingItems
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationSupportingItems</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationSupportingItemsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

    }
}