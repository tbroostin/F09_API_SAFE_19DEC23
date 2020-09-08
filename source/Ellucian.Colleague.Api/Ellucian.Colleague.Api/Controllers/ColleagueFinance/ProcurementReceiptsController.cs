//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to ProcurementReceipts
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class ProcurementReceiptsController : BaseCompressedApiController
    {
        private readonly IProcurementReceiptsService _procurementReceiptsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ProcurementReceiptsController class.
        /// </summary>
        /// <param name="procurementReceiptsService">Service of type <see cref="IProcurementReceiptsService">IProcurementReceiptsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ProcurementReceiptsController(IProcurementReceiptsService procurementReceiptsService, ILogger logger)
        {
            _procurementReceiptsService = procurementReceiptsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all ProcurementReceipts
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">QueryStringFilter</param>
        /// <returns>List of ProcurementReceipts <see cref="Dtos.ProcurementReceipts"/> objects representing matching procurementReceipts</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.ProcurementReceipts))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetProcurementReceiptsAsync(Paging page, QueryStringFilter criteria)
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

                var criteriaValues = GetFilterObject<Dtos.ProcurementReceipts>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.ProcurementReceipts>>(new List<Dtos.ProcurementReceipts>(), page, this.Request);

                var pageOfItems = await _procurementReceiptsService.GetProcurementReceiptsAsync(page.Offset, page.Limit, criteriaValues, bypassCache);

                AddEthosContextProperties(
                  await _procurementReceiptsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.ProcurementReceipts>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a ProcurementReceipts using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.ProcurementReceipts> GetProcurementReceiptsByGuidAsync(string guid)
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
                   await _procurementReceiptsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _procurementReceiptsService.GetProcurementReceiptsByGuidAsync(guid);
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
        /// Create (POST) a new procurementReceipts
        /// </summary>
        /// <param name="procurementReceipts">DTO of the new procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.ProcurementReceipts> PostProcurementReceiptsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.ProcurementReceipts procurementReceipts)
        {
            if (procurementReceipts == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid procurementReceipts.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(procurementReceipts.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null procurementReceipts id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (procurementReceipts.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _procurementReceiptsService.ImportExtendedEthosData(await ExtractExtendedData(await _procurementReceiptsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var procurementReceipt=  await _procurementReceiptsService.CreateProcurementReceiptsAsync(procurementReceipts);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _procurementReceiptsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _procurementReceiptsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { procurementReceipt.Id }));

                return procurementReceipt;
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
        /// Update (PUT) an existing procurementReceipts
        /// </summary>
        /// <param name="guid">GUID of the procurementReceipts to update</param>
        /// <param name="procurementReceipts">DTO of the updated procurementReceipts</param>
        /// <returns>A procurementReceipts object <see cref="Dtos.ProcurementReceipts"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.ProcurementReceipts> PutProcurementReceiptsAsync([FromUri] string guid, [FromBody] Dtos.ProcurementReceipts procurementReceipts)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a procurementReceipts
        /// </summary>
        /// <param name="guid">GUID to desired procurementReceipts</param>
        [HttpDelete]
        public async Task DeleteProcurementReceiptsAsync(string guid)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}