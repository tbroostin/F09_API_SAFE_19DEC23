//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Filters;
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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to PaymentTransactions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class PaymentTransactionsController : BaseCompressedApiController
    {
        private readonly IPaymentTransactionsService _paymentTransactionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PaymentTransactionsController class.
        /// </summary>
        /// <param name="paymentTransactionsService">Service of type <see cref="IPaymentTransactionsService">IPaymentTransactionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PaymentTransactionsController(IPaymentTransactionsService paymentTransactionsService, ILogger logger)
        {
            _paymentTransactionsService = paymentTransactionsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all paymentTransactions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="document">Named query</param>
        /// <returns>List of PaymentTransactions <see cref="Dtos.PaymentTransactions"/> objects representing matching paymentTransactions</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("document", typeof(DocumentFilter)), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPaymentTransactionsAsync(Paging page, QueryStringFilter document) 
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
               
                string documentGuid = string.Empty;
                var documentTypeValue = InvoiceTypes.NotSet;

                var documentFilter = GetFilterObject<DocumentFilter>(_logger, "document");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.PaymentTransactions>>(new List<Dtos.PaymentTransactions>(), page, 0, this.Request);
                if (documentFilter.Document != null)
                {
                    documentGuid = documentFilter.Document.Id;
                    documentTypeValue = documentFilter.Document.Type;
                    if (documentTypeValue != InvoiceTypes.NotSet && string.IsNullOrEmpty(documentGuid))
                    {
                        throw new ArgumentException("documentGuid", "Id is required when requesting a document");
                    }
                    if (documentTypeValue == InvoiceTypes.NotSet && !string.IsNullOrEmpty(documentGuid))
                    {
                        throw new ArgumentException("documentType", "Type is required when requesting a document");
                    }
                }
             
                var pageOfItems = await _paymentTransactionsService.GetPaymentTransactionsAsync(page.Offset, page.Limit, documentGuid, documentTypeValue, bypassCache);

                AddEthosContextProperties(
                    await _paymentTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _paymentTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PaymentTransactions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a paymentTransactions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PaymentTransactions> GetPaymentTransactionsByGuidAsync(string guid)
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
                    await _paymentTransactionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _paymentTransactionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _paymentTransactionsService.GetPaymentTransactionsByGuidAsync(guid);
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
        /// Create (POST) a new paymentTransactions
        /// </summary>
        /// <param name="paymentTransactions">DTO of the new paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PaymentTransactions> PostPaymentTransactionsAsync([FromBody] Dtos.PaymentTransactions paymentTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing paymentTransactions
        /// </summary>
        /// <param name="guid">GUID of the paymentTransactions to update</param>
        /// <param name="paymentTransactions">DTO of the updated paymentTransactions</param>
        /// <returns>A paymentTransactions object <see cref="Dtos.PaymentTransactions"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PaymentTransactions> PutPaymentTransactionsAsync([FromUri] string guid, [FromBody] Dtos.PaymentTransactions paymentTransactions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a paymentTransactions
        /// </summary>
        /// <param name="guid">GUID to desired paymentTransactions</param>
        [HttpDelete]
        public async Task DeletePaymentTransactionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}