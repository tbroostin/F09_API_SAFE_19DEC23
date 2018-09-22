// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for purchase orders
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class PurchaseOrdersController : BaseCompressedApiController
    {
        private readonly IPurchaseOrderService purchaseOrderService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the PurchaseOrdersController object
        /// </summary>
        /// <param name="purchaseOrderService">Purchase Order service object</param>
        /// <param name="logger">Logger object</param>
        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger logger)
        {
            this.purchaseOrderService = purchaseOrderService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified purchase order
        /// </summary>
        /// <param name="purchaseOrderId">The requested purchase order ID</param>
        /// <returns>Purchase Order DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.PURCHASE.ORDER, and requires access to at least one of the
        /// general ledger numbers on the purchase order line items.
        /// </accessComments>
        public async Task<PurchaseOrder> GetPurchaseOrderAsync(string purchaseOrderId)
        {
            if (string.IsNullOrEmpty(purchaseOrderId))
            {
                string message = "A Purchase Order ID must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var purchaseOrder = await purchaseOrderService.GetPurchaseOrderAsync(purchaseOrderId);
                return purchaseOrder;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the purchase order.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ApplicationException aex)
            {
                logger.Error(aex, aex.Message);
                throw CreateHttpResponseException("Invalid data in record.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException("Unable to get the purchase order.", HttpStatusCode.BadRequest);
            }
        }
        
        #region EEDM V11
        /// <summary>
        /// Return all purchaseOrders
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of PurchaseOrders <see cref="Dtos.PurchaseOrders"/> objects representing matching purchaseOrders</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetPurchaseOrdersAsync2(Paging page)
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

                var pageOfItems = await purchaseOrderService.GetPurchaseOrdersAsync2(page.Offset, page.Limit, bypassCache);
                AddEthosContextProperties(
                    await purchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PurchaseOrders2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a purchaseOrders using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PurchaseOrders2> GetPurchaseOrdersByGuidAsync2(string guid)
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
                    await purchaseOrderService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await purchaseOrderService.GetPurchaseOrdersByGuidAsync2(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new purchaseOrders
        /// </summary>
        /// <param name="purchaseOrders">DTO of the new purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.PurchaseOrders2> PostPurchaseOrdersAsync2([ModelBinder(typeof(EedmModelBinder))] Dtos.PurchaseOrders2 purchaseOrders)
        {
            if (purchaseOrders == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null purchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            
            try
            {
                if (purchaseOrders.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("purchaseOrdersDto", "Nil GUID must be used in POST operation.");
                }
                ValidatePO2(purchaseOrders);

                //call import extend method that needs the extracted extension data and the config
                await purchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await purchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                // Create Purchase Order
                var purchaseOrderReturn = await purchaseOrderService.PostPurchaseOrdersAsync2(purchaseOrders);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await purchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { purchaseOrderReturn.Id }));

                return purchaseOrderReturn;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing purchaseOrders
        /// </summary>
        /// <param name="guid">GUID of the purchaseOrders to update</param>
        /// <param name="purchaseOrders">DTO of the updated purchaseOrders</param>
        /// <returns>A purchaseOrders object <see cref="Dtos.PurchaseOrders"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.PurchaseOrders2> PutPurchaseOrdersAsync2([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.PurchaseOrders2 purchaseOrders)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (purchaseOrders == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null purchaseOrders argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(purchaseOrders.Id))
            {
                purchaseOrders.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(purchaseOrders.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != purchaseOrders.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                // get Data Privacy List
                var dpList = await purchaseOrderService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await purchaseOrderService.ImportExtendedEthosData(await ExtractExtendedData(await purchaseOrderService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var mergedPurchaseOrder = await PerformPartialPayloadMerge(purchaseOrders,
                            async () => await purchaseOrderService.GetPurchaseOrdersByGuidAsync2(guid),
                            dpList, logger);

                ValidatePO2(mergedPurchaseOrder);

                var purchaseOrderReturn = await purchaseOrderService.PutPurchaseOrdersAsync2(guid, mergedPurchaseOrder);

                //store dataprivacy list and get the extended data to store

                AddEthosContextProperties(dpList,
                    await purchaseOrderService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return purchaseOrderReturn;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                if (e.Errors == null || e.Errors.Count <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Helper method to validate Purchase Order PUT/POST.
        /// </summary>
        /// <param name="po">Purchase Order DTO object of type <see cref="Dtos.PurchaseOrders"/></param>
        private void ValidatePO2(Dtos.PurchaseOrders2 po)
        {
            var defaultCurrency = new CurrencyIsoCode?();
            if (po.Vendor == null)
            {
                throw new ArgumentNullException("purchaseOrders.Vendor", "The vendor is required when submitting a purchase order. ");
            }
            if (po.Vendor != null && po.Vendor.ExistingVendor != null 
                && po.Vendor.ExistingVendor.Vendor != null && string.IsNullOrEmpty(po.Vendor.ExistingVendor.Vendor.Id))
            {
                throw new ArgumentNullException("purchaseOrders.Vendor", "The vendor id is required when submitting a purchase order. ");
            }
            if (po.OrderedOn == default(DateTime))
            {
                throw new ArgumentNullException("purchaseOrders.OrderOn.", "OrderOn is a required field");
            }

            if (po.TransactionDate == default(DateTime))
            {
                throw new ArgumentNullException("purchaseOrders.TransactionDate.", "TransactionDate is a required field");
            }

            if (po.OrderedOn > po.TransactionDate)
            {
                throw new ArgumentNullException("purchaseOrders.TransactionDate.", "TransactionDate cannot before OrderOn date.");
            }

            if (po.DeliveredBy != default(DateTime) && po.OrderedOn > po.DeliveredBy)
            {
                throw new ArgumentNullException("purchaseOrders.DeliveredBy.", "DeliveredBy date cannot be before the OrderOn date");
            }
            if (po.StatusDate != default(DateTime) && po.OrderedOn > po.StatusDate && po.Status == PurchaseOrdersStatus.Voided)
            {
                throw new ArgumentNullException("purchaseOrders.StatusDate.", "StatusDate date cannot be before the OrderOn date when Voiding the purchase order");
            }
            
            if (po.OverrideShippingDestination != null && po.OverrideShippingDestination.Place != null)
            {
                if (po.OverrideShippingDestination.Place.Country != null && po.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.CAN && po.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.USA)
                {
                    throw new ArgumentNullException("purchaseOrders.OverrideShippingDestination.Country.", "Country code can only be CAN or USA");
                }
                if (po.OverrideShippingDestination.Contact != null && po.OverrideShippingDestination.Contact.Extension.Length > 4)
                {
                    throw new ArgumentNullException("purchaseOrders.OverrideShippingDestination.Contact.Extension", "The Extension cannot be greater then 4 in length.");
                }
            }
            if (po.Vendor.ManualVendorDetails != null && po.Vendor.ManualVendorDetails.Place != null)
            {
                if (po.Vendor.ManualVendorDetails.Place.Country != null &&
                    po.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.CAN && po.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.USA)
                {
                    throw new ArgumentNullException("purchaseOrders.Vendor.Country.", "Country code can only be CAN or USA");
                }
            }

            if (po.PaymentSource == null)
            {
                throw new ArgumentNullException("purchaseOrders.PaymentSource.", "PaymentSource is a required field for Colleague");
            }
            if (po.Comments != null)
            {
                foreach (var comments in po.Comments)
                {
                    if (comments.Comment == null)
                    {
                        throw new ArgumentNullException("purchaseOrders.Comments", "Comments require a comment");
                    }
                    if (comments.Type == CommentTypes.NotSet)
                    {
                        throw new ArgumentNullException("purchaseOrders.Comments", "Type is require for a comment");
                    }
                    if (comments.Type == null)
                    {
                        throw new ArgumentNullException("purchaseOrders.Comments", "Type is require for a comment");
                    }
                }
            }
            if (po.Buyer != null && po.Buyer.Id == null)
            {
                throw new ArgumentNullException("purchaseOrders.Buyer", "Buyer Id is require for a Buyer object.");
            }
            if (po.Initiator != null && po.Initiator.Detail != null && string.IsNullOrWhiteSpace(po.Initiator.Detail.Id))
            {
                throw new ArgumentNullException("purchaseOrders.Initiator.Detail", "The Initiator detail Id is require for an Initiator detail object.");
            }
            if (po.PaymentTerms != null && string.IsNullOrWhiteSpace(po.PaymentTerms.Id))
            {
                throw new ArgumentNullException("purchaseOrders.PaymentTerms", "The PaymentTerms Id is require for a PaymentTerms object.");
            }
            if (po.Classification != null && string.IsNullOrWhiteSpace(po.Classification.Id))
            {
                throw new ArgumentNullException("purchaseOrders.Classification", "The Classification Id is require for a Classification object.");
            }
            if (po.SubmittedBy != null && string.IsNullOrWhiteSpace(po.SubmittedBy.Id))
            {
                throw new ArgumentNullException("purchaseOrders.SubmittedBy", "The SubmittedBy Id is require for a SubmittedBy object.");
            }


            if (po.LineItems == null)
            {
                throw new ArgumentNullException("purchaseOrders.LineItems.", "At least one line item must be provided when submitting an accounts-payable-invoice. ");
            }
            if (po.LineItems != null)
            {
                foreach (var lineItem in po.LineItems)
                {
                    if (lineItem.CommodityCode != null && string.IsNullOrWhiteSpace(lineItem.CommodityCode.Id))
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.CommodityCode", "The commodity code id is required when submitting a commodity code. ");
                    }
                    if (lineItem.UnitOfMeasure != null && string.IsNullOrWhiteSpace(lineItem.UnitOfMeasure.Id))
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.UnitofMeasure", "The UnitofMeasure id is required when submitting a UnitofMeasure. ");
                    }
                    if (lineItem.UnitPrice != null && (!lineItem.UnitPrice.Value.HasValue || lineItem.UnitPrice.Currency == null))
                    {
                        throw new ArgumentNullException("purchaseOrders.Taxes.UnitPrice.", "Both the unit price amount value and currency are required when submitting a line item unit price. ");
                    }
                    if (lineItem.UnitPrice != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, lineItem.UnitPrice.Currency);
                    }
                    if (lineItem.AdditionalAmount != null && (!lineItem.AdditionalAmount.Value.HasValue || lineItem.AdditionalAmount.Currency == null))
                    {
                        throw new ArgumentNullException("purchaseOrders.AdditionalAmount.UnitPrice", "The additional amount value and currency are required when submitting a line item additional price. ");
                    }
                    if (lineItem.AdditionalAmount != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, lineItem.AdditionalAmount.Currency);
                    }
                    if (lineItem.TaxCodes != null)
                    {
                        foreach (var lineItemTaxes in lineItem.TaxCodes)
                        {
                            if (lineItemTaxes.Id == null)
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.Taxes.TaxCode", "The Taxes.TaxCode is required when submitting a line item Tax Code. ");
                            }
                        }
                    }

                    if (lineItem.TradeDiscount != null)
                    {
                        if (lineItem.TradeDiscount.Amount != null && lineItem.TradeDiscount.Percent != null)
                        {
                            throw new ArgumentNullException("purchaseOrders.LineItems.TradeDiscount", "TradeDiscount cannot contain both an Amount and Percentage");
                        }
                        if (lineItem.TradeDiscount.Amount != null && (!lineItem.TradeDiscount.Amount.Value.HasValue || lineItem.TradeDiscount.Amount.Currency == null))
                        {
                            throw new ArgumentNullException("purchaseOrders.LineItems.TradeDiscount.Amount", "TradeDiscount amount requires both an Amount and Currency");
                        }
                        if (lineItem.AdditionalAmount != null)
                        {
                            defaultCurrency = checkCurrency(defaultCurrency, lineItem.AdditionalAmount.Currency);
                        }
                    }

                    try
                    {
                        if (lineItem.Reference != null)
                        {
                            var referenceDoc = lineItem.Reference;
                            // Check to see if the reference line item differ, If they do then we have to make sure that the are the same Item number
                            if (referenceDoc.lineItemNumber != lineItem.LineItemNumber)
                            {
                                if (!string.IsNullOrEmpty(referenceDoc.lineItemNumber) && string.IsNullOrEmpty(lineItem.LineItemNumber))
                                {
                                    lineItem.LineItemNumber = referenceDoc.lineItemNumber;
                                }
                                else
                                {
                                    lineItem.Reference.lineItemNumber = lineItem.LineItemNumber;
                                    referenceDoc.lineItemNumber = lineItem.LineItemNumber;
                                }
                            }

                            if (referenceDoc.Document.PurchasingArrangement != null)
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.Reference.Document", "The Document of Purchasing Arrangment is not acceptable in this system.");
                            }

                            if (referenceDoc.Document.Requisition.Id == null)
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.Reference.Document", "The requisition ID is a required field");
                            }

                            if (string.IsNullOrEmpty(referenceDoc.lineItemNumber))
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.Reference.LineItemNumber", "The Line number is a required field for a reference to requisitions");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("An unexpected error occurred: " + ex.Message);
                    }




                    if (lineItem.AccountDetail != null)
                    {
                        foreach (var accountDetail in lineItem.AccountDetail)
                        {
                            if (accountDetail.AccountingString == null)
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetails.AccountingString", "The AccountingString is required when submitting a line item account detail.");

                            }
                            if (accountDetail.Allocation != null)
                            {
                                var allocation = accountDetail.Allocation;
                                if (allocation.Allocated != null)
                                {
                                    var allocated = allocation.Allocated;
                                    if (allocated.Amount != null && (!allocated.Amount.Value.HasValue || allocated.Amount.Currency == null))
                                    {
                                        throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetail.Allocation.Allocated",
                                            "The Allocation.Allocated value and currency are required when submitting a line item AccountDetail.Allocation.Allocated. ");
                                    }
                                    if (allocated.Amount != null)
                                    {
                                        defaultCurrency = checkCurrency(defaultCurrency, allocated.Amount.Currency);
                                    }
                                }
                                if (allocation.TaxAmount != null && (!allocation.TaxAmount.Value.HasValue || allocation.TaxAmount.Currency == null))
                                {
                                    throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetail.Allocation.TaxAmount",
                                        "The tax amount value and currency are required when submitting a line item account detail allocation tax amount. ");
                                }
                                if (allocation.TaxAmount != null)
                                {
                                    defaultCurrency = checkCurrency(defaultCurrency, allocation.TaxAmount.Currency);
                                }
                                if (allocation.AdditionalAmount != null && (!allocation.AdditionalAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetail.Allocation.AdditionalAmount",
                                        "The additional amount value and currency are required when submitting a line item account detail allocation additional amount. ");
                                }
                                if (allocation.AdditionalAmount != null)
                                {
                                    defaultCurrency = checkCurrency(defaultCurrency, allocation.AdditionalAmount.Currency);
                                }
                                if (allocation.DiscountAmount != null && (!allocation.DiscountAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetails.DiscountAmount.Allocation.AdditionalAmount",
                                        "The discount amount value and currency are required when submitting a line item account detail allocation discount amount. ");
                                }
                                if (allocation.DiscountAmount != null)
                                {
                                    defaultCurrency = checkCurrency(defaultCurrency, allocation.DiscountAmount.Currency);
                                }

                            }
                            if (accountDetail.SubmittedBy != null && string.IsNullOrEmpty(accountDetail.SubmittedBy.Id))
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetails.SubmittedBy", "The SubmittedBy id is required when submitting a line item account detail SubmittedBy. ");
                            }
                            if (string.IsNullOrEmpty(accountDetail.AccountingString))
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetails.AccountingString", "The AccountingString id is required when submitting a line item account detail AccountingString. ");
                            }
                            if (accountDetail.Allocation == null)
                            {
                                throw new ArgumentNullException("purchaseOrders.LineItems.AccountDetails.Allocation", "The Allocation is required when submitting a line item account detail. ");
                            }
                        }

                    }
                    if (string.IsNullOrEmpty(lineItem.Description))
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.Description", "The Description is required when submitting a line item. ");
                    }
                    if (lineItem.Quantity == 0)
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.Quantity", "The Quantity is required when submitting a line item. ");
                    }
                    if (lineItem.UnitPrice == null)
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.UnitPrice", "The UnitPrice is required when submitting a line item. ");
                    }
                    else
                    {
                        if (lineItem.UnitPrice.Currency != null)
                        {
                            defaultCurrency = checkCurrency(defaultCurrency, lineItem.UnitPrice.Currency);
                        }
                        else
                        {
                            throw new ArgumentNullException("purchaseOrders.LineItems.UnitPrice", "The UnitPrice currency is a required when submitting a line item. ");
                        }
                        if (!lineItem.UnitPrice.Value.HasValue)
                        {
                            throw new ArgumentNullException("purchaseOrders.LineItems.UnitPrice", "The UnitPrice value is required when submitting a line item. ");
                        }
                    }
                    if (lineItem.PartNumber != null && lineItem.PartNumber.Length > 11)
                    {
                        throw new ArgumentNullException("purchaseOrders.LineItems.PartNumber", "The PartNumber cannot exceed 11 characters in length.");
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Delete (DELETE) a purchaseOrders
        /// </summary>
        /// <param name="guid">GUID to desired purchaseOrders</param>
        [HttpDelete]
        public async Task DeletePurchaseOrdersAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        private CurrencyIsoCode? checkCurrency(CurrencyIsoCode? defaultValue, CurrencyIsoCode? newValue)
        {
            if (defaultValue != null && defaultValue != newValue && newValue != null)
            {
                throw new ArgumentException("purchaseOrders.Currency", "All currency codes in the request must be the same and cannot differ.");
            }
            CurrencyIsoCode? cc = newValue == null ? defaultValue : newValue;
            return cc;
        }


    }
}