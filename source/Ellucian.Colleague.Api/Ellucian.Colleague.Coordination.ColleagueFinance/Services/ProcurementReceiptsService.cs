// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IProcurementReceiptservice interface
    /// </summary>
    [RegisterType]
    public class ProcurementReceiptsService : BaseCoordinationService, IProcurementReceiptsService
    {
        private IPurchaseOrderReceiptRepository purchaseOrderReceiptsRepository;
        private IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository;
        IPurchaseOrderRepository purchaseOrderRepository;
        private readonly IPersonRepository personRepository;
        private readonly IConfigurationRepository configurationRepository;

        // Constructor to initialize the private attributes
        public ProcurementReceiptsService(IPurchaseOrderReceiptRepository purchaseOrderReceiptsRepository,
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IPersonRepository personRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.purchaseOrderReceiptsRepository = purchaseOrderReceiptsRepository;
            this.colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            this.purchaseOrderRepository = purchaseOrderRepository;
            this.personRepository = personRepository;
            this.configurationRepository = configurationRepository;
        }


        private IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod> _shippingMethod = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod>> GetShippingMethodsAsync(bool bypassCache)
        {
            if (_shippingMethod == null)
            {
                _shippingMethod = await colleagueFinanceReferenceDataRepository.GetShippingMethodsAsync(bypassCache);
            }
            return _shippingMethod;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> _currencyConv = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion>> GetAllCurrencyConversionAsync()
        {
            if (_currencyConv == null)
            {
                _currencyConv = await colleagueFinanceReferenceDataRepository.GetCurrencyConversionAsync();
            }
            return _currencyConv;
        }

        /// <summary>
        /// Gets all procurement-receipts
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="filters">Filters</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="ProcurementReceipts">procurementReceipts</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.ProcurementReceipts>, int>> GetProcurementReceiptsAsync(int offset, int limit, Dtos.ProcurementReceipts filters, bool bypassCache = false)
        {
            CheckViewProcurementReceiptsPermission();

            var procurementReceiptsCollection = new List<Ellucian.Colleague.Dtos.ProcurementReceipts>();

            var purchaseOrderID = string.Empty;
            if (filters != null && filters.PurchaseOrder != null && !string.IsNullOrEmpty(filters.PurchaseOrder.Id))
            {
                try
                {
                    purchaseOrderID = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(filters.PurchaseOrder.Id);
                    if (string.IsNullOrEmpty(purchaseOrderID))
                    {
                        return new Tuple<IEnumerable<Dtos.ProcurementReceipts>, int>(procurementReceiptsCollection, 0);
                    }
                }
                catch (Exception)
                {
                    return new Tuple<IEnumerable<Dtos.ProcurementReceipts>, int>(procurementReceiptsCollection, 0);
                }
            }

            var procurementReceiptsEntities =
                     await purchaseOrderReceiptsRepository.GetPurchaseOrderReceiptsAsync(offset, limit, purchaseOrderID);
        

            var totalRecords = procurementReceiptsEntities.Item2;
            if (procurementReceiptsEntities != null && procurementReceiptsEntities.Item1.Any())
            {
                var ids = procurementReceiptsEntities.Item1
                    .Where(x => (!string.IsNullOrEmpty(x.ReceivedBy)))
                    .Select(x => x.ReceivedBy).Distinct().ToList();

                var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync(ids);

                foreach (var procurementReceiptsEntity in procurementReceiptsEntities.Item1)
                {
                    if (procurementReceiptsEntity.Guid != null)
                    {
                        procurementReceiptsCollection.Add(await this.ConvertPurchaseOrderReceiptEntityToDtoAsync(procurementReceiptsEntity, personGuidCollection, bypassCache));
                    }
                }
                return new Tuple<IEnumerable<Dtos.ProcurementReceipts>, int>(procurementReceiptsCollection, totalRecords);
            }
            return new Tuple<IEnumerable<Dtos.ProcurementReceipts>, int>(new List<Dtos.ProcurementReceipts>(), 0);
        }

        /// <summary>
        /// Get a procurementReceipts by guid.
        /// </summary>
        /// <param name="guid">Guid of the procurementReceipts in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="ProcurementReceipts">procurementReceipts</see></returns>
        public async Task<Ellucian.Colleague.Dtos.ProcurementReceipts> GetProcurementReceiptsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Procurement Receipt.");
            }
             CheckViewProcurementReceiptsPermission();

            try
            {
                var purchaseOrderReceiptEntity = await purchaseOrderReceiptsRepository.GetPurchaseOrderReceiptByGuidAsync(guid);
                var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync
               (new List<string>() { purchaseOrderReceiptEntity.ReceivedBy });
                return await ConvertPurchaseOrderReceiptEntityToDtoAsync(purchaseOrderReceiptEntity, personGuidCollection, bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No Procurement Receipt was found for guid  " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No Procurement Receipt was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException("No Procurement Receipt was found for guid  " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("No Procurement Receipt was found for guid  " + guid, ex);
            }
        }

        /// <summary>
        /// Create a procurementReceipts.
        /// </summary>
        /// <param name="procurementReceipts">The <see cref="ProcurementReceipts">procurementReceipts</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="ProcurementReceipts">procurementReceipts</see></returns>
        public async Task<Ellucian.Colleague.Dtos.ProcurementReceipts> CreateProcurementReceiptsAsync(Ellucian.Colleague.Dtos.ProcurementReceipts procurementReceipts)
        {
            if (procurementReceipts == null)
                throw new ArgumentNullException("ProcurementReceipts", "Must provide a ProcurementReceipts for update");
            if (string.IsNullOrEmpty(procurementReceipts.Id))
                throw new ArgumentNullException("ProcurementReceipts", "Must provide a guid for ProcurementReceipts update");

            Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderReceipt createdProcurementReceipt = null;
            Dtos.ProcurementReceipts dtoProcurementReceipts = null;
            // verify the user has the permission to create a ProcurementReceipts
            CheckCreateProcurementReceiptsPermission();

            try
            {
                var procurementReceiptsEntity
                         = await ConvertProcurementReceiptsDtoToEntityAsync(procurementReceipts);

                // create a ProcurementReceipts entity in the database
                createdProcurementReceipt =
                    await purchaseOrderReceiptsRepository.CreatePurchaseOrderReceiptAsync(procurementReceiptsEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            if (createdProcurementReceipt != null)
            {
                var personGuidCollection = await personRepository.GetPersonGuidsCollectionAsync
                   (new List<string>() { createdProcurementReceipt.ReceivedBy });
                dtoProcurementReceipts = await this.ConvertPurchaseOrderReceiptEntityToDtoAsync(createdProcurementReceipt,
                   personGuidCollection, true);
            }

            // return the newly created ProcurementReceipts
            return dtoProcurementReceipts;
        }

        /// <summary>
        /// Converts an ProcurementReceipts domain entity to its corresponding ProcurementReceipts DTO
        /// </summary>
        /// <param name="source">ProcurementReceipts domain entity</param>
        /// <param name="personGuidCollection"> personGuid Collection</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.ProcurementReceipts">ProcurementReceipts</see></returns>
        private async Task<Dtos.ProcurementReceipts> ConvertPurchaseOrderReceiptEntityToDtoAsync(Domain.ColleagueFinance.Entities.PurchaseOrderReceipt source,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source is required to obtain a Procurement Receipt.");
            }
            var dto = new Dtos.ProcurementReceipts();
            dto.Id = source.Guid;
            if (!string.IsNullOrEmpty(source.PoId))
            {
                var purchaseOrderID = await this.purchaseOrderRepository.GetGuidFromIdAsync(source.PoId, "PURCHASE.ORDERS");
                if (purchaseOrderID == null)
                {
                    throw new KeyNotFoundException(string.Concat("PurchaseOrder, ", "No purchase order was found for GUID: '", source.PoId, "'"));
                }
                dto.PurchaseOrder = new Dtos.GuidObject2(purchaseOrderID);
            }

            if (!string.IsNullOrEmpty(source.PackingSlip))
            {
                dto.PackingSlipNumber = source.PackingSlip;
            }

            if (!string.IsNullOrEmpty(source.PoNo) && !string.IsNullOrEmpty(source.Recordkey))
            {
                dto.ReceiptNumber = string.Format("{0}*{1}", source.PoNo, source.Recordkey);
            }

            if (source.ReceiptItems != null && source.ReceiptItems.Any())
            {
                var lineItems = new List<Dtos.ProcurementReceiptsLineItems>();
                foreach (var line in source.ReceiptItems)
                {
                    var lineItem = new Dtos.ProcurementReceiptsLineItems();
                    lineItem.LineItemNumber = line.Id;
                    if ((line.ReceivedAmt.HasValue) || (line.ReceivedQty.HasValue))
                    {
                        var amount = new Dtos.DtoProperties.QuantityAmount2DtoProperty()  { };
                        if (line.ReceivedAmt.HasValue)
                        {
                            amount.Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                                Value = line.ReceivedAmt,
                                Currency = GetCurrencyIsoCode(line.ReceivedAmtCurrency)
                            };
                        }
                        amount.Quantity = line.ReceivedQty;
                        lineItem.Received = amount;
                    }

                    if ((line.RejectedAmt.HasValue) || (line.RejectedQty.HasValue))
                    {
                        var amount = new Dtos.DtoProperties.QuantityAmount2DtoProperty() { };
                        if (line.RejectedAmt.HasValue)
                        {
                            amount.Cost = new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                                Value = line.RejectedAmt,
                                Currency = GetCurrencyIsoCode(line.RejectedAmtCurrency)
                            };
                        }
                        amount.Quantity = line.RejectedQty;
                        lineItem.Rejected = amount;
                    }
                    
                    if (!string.IsNullOrEmpty(line.ReceivingComments))
                    {
                        lineItem.Comment = line.ReceivingComments;
                    }
                    lineItems.Add(lineItem);
                }
                if (lineItems != null && lineItems.Any())
                {
                    dto.LineItems = lineItems;
                }
            }

            if (!string.IsNullOrEmpty(source.ArrivedVia))
            {
                var shippingMethods = await this.GetShippingMethodsAsync(bypassCache);
                if (shippingMethods == null || !shippingMethods.Any())
                {
                    throw new InvalidOperationException("Shipping Methods not found.");
                }
           
                var shippingMethod = shippingMethods.FirstOrDefault(sm => sm.Code == source.ArrivedVia);
                if (shippingMethod == null)
                {
                    throw new KeyNotFoundException(string.Concat("ShippingMethod, ", "Shipping Method not found for : '", source.ArrivedVia, "'"));
                }
                dto.ShippingMethod = new Dtos.GuidObject2(shippingMethod.Guid);
            }    
            
            if (source.ReceivedDate.HasValue)
                dto.ReceivedOn = source.ReceivedDate;

            if ((!string.IsNullOrEmpty(source.ReceivedBy)) && (personGuidCollection != null) && (personGuidCollection.Any()))
            {
                var receivedByGuid = string.Empty;
                personGuidCollection.TryGetValue(source.ReceivedBy, out receivedByGuid);
                if (string.IsNullOrEmpty(receivedByGuid))
                {
                    throw new KeyNotFoundException(string.Concat("ReceivedBy, ", "Unable to locate guid for : '", source.ReceivedBy, "'"));
                }
                dto.ReceivedBy = new Dtos.GuidObject2(receivedByGuid);
                
            }

            if (!string.IsNullOrEmpty(source.ReceivingComments))
            {
                dto.Comment = source.ReceivingComments;
            }
            return dto;
        }

        /// <summary>
        /// Convert a DTO to a entity
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private async Task<PurchaseOrderReceipt> ConvertProcurementReceiptsDtoToEntityAsync(Dtos.ProcurementReceipts dto, bool bypassCache = false)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id))
                throw new ArgumentNullException("ProcurementReceipts", "Must provide guid for ProcurementReceipts");

            if ((dto.PurchaseOrder == null) || (string.IsNullOrEmpty(dto.PurchaseOrder.Id)))
            {
                throw new ArgumentNullException("purchaseOrder", "Must provide purchaseOrder for ProcurementReceipts");
            }
            if (dto.LineItems == null || !dto.LineItems.Any())
            {
                throw new ArgumentNullException("lineItems", "Must provide at least one lineItem for ProcurementReceipts");
            }
            if ((dto.ReceivedBy == null) || (string.IsNullOrEmpty(dto.ReceivedBy.Id)))
            {
                throw new ArgumentNullException("receivedBy", "Must provide receivedBy for ProcurementReceipts");
            }

            CurrencyIsoCode? currency = null;
            var procurementReceiptsEntity = new PurchaseOrderReceipt(dto.Id);

            // Since we only support POST, the entity record key will always be null;
            // If support is added for PUT, then setting the record key should be part of the entity constructor
            // procurementReceiptsEntity.Recordkey = await purchaseOrderReceiptsRepository.GetPurchaseOrderReceiptIdFromGuidAsync(guid);

            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrdersByGuidAsync(dto.PurchaseOrder.Id);
            if (purchaseOrder == null)
            {
                throw new Exception(string.Concat("Purchase Order GUID is not found: ", dto.PurchaseOrder.Id));
            }
            if (purchaseOrder.Status == PurchaseOrderStatus.InProgress || purchaseOrder.Status == PurchaseOrderStatus.NotApproved)
            {
                throw new Exception("You cannot accept items on an unapproved or unfinished purchase order.");
            }
            if (((purchaseOrder.OutstandingItemsId == null) || !purchaseOrder.OutstandingItemsId.Any())
                && ((purchaseOrder.AcceptedItemsId == null) || !purchaseOrder.AcceptedItemsId.Any()))
            {
                throw new Exception("There are no outstanding items to accept, and no accepted items to adjust. No receiving actions are possible on this purchase order at this time.");
            }

            procurementReceiptsEntity.PoId = purchaseOrder.Id;

            if (!(string.IsNullOrEmpty(purchaseOrder.CurrencyCode)))
            {
                try
                {
                    currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), purchaseOrder.CurrencyCode);
                }
                catch (ArgumentException)
                {
                    // if we are unable to convert the purchare order currency code then GetCurrencyCodeFromCurrencyIsoCodeAsync will use the hostCountry
                }
            }
            var purchaseOrderCurrency = await this.GetCurrencyCodeFromCurrencyIsoCodeAsync(currency, purchaseOrder.HostCountry);

            var receiptItems = new List<PurchaseOrderReceiptItem>();
            foreach (var lineItem in dto.LineItems)
            {
                var receiptItem = GetPurchaseOrderReceiptItem(purchaseOrder, purchaseOrderCurrency, lineItem);
                if (receiptItem != null)
                {
                    receiptItems.Add(receiptItem);
                }
            }
            if (receiptItems.Any())
            {
                procurementReceiptsEntity.ReceiptItems = receiptItems;
            }

            if (!string.IsNullOrEmpty(dto.PackingSlipNumber))
            {
                procurementReceiptsEntity.PackingSlip = dto.PackingSlipNumber;
            }

            if ((dto.ShippingMethod != null) && (!string.IsNullOrEmpty(dto.ShippingMethod.Id)))
            {
                var shippingMethods = await this.GetShippingMethodsAsync(bypassCache);
                if (shippingMethods == null)
                {
                    throw new InvalidOperationException("Shipping Methods not found.");
                }
                var shippingMethod = shippingMethods.FirstOrDefault(sm => sm.Guid == dto.ShippingMethod.Id);
                if (shippingMethod == null)
                {
                    throw new Exception(string.Concat("ShippingMethod GUID is not found: ", dto.ShippingMethod.Id));
                }
                procurementReceiptsEntity.ArrivedVia = shippingMethod.Code;
            }
         
            var submittedById = await personRepository.GetPersonIdFromGuidAsync(dto.ReceivedBy.Id);
            if (string.IsNullOrEmpty(submittedById))
            {
                throw new Exception(string.Concat("ReceivedBy GUID is not found: ", dto.ReceivedBy.Id));
            }
            procurementReceiptsEntity.ReceivedBy = submittedById;

            if ((dto.ReceivedOn != default(DateTime)))
                procurementReceiptsEntity.ReceivedDate = dto.ReceivedOn;

            if (!string.IsNullOrEmpty(dto.Comment))
            {
                procurementReceiptsEntity.ReceivingComments = dto.Comment;
            }

            return procurementReceiptsEntity;
        }

        private PurchaseOrderReceiptItem GetPurchaseOrderReceiptItem(PurchaseOrder purchaseOrder, string purchaseOrderCurrency, Dtos.ProcurementReceiptsLineItems lineItem)
        {
            if (string.IsNullOrEmpty(lineItem.LineItemNumber))
            {
                throw new ArgumentNullException("lineItem.LineItemNumber", "LineItemNumber is required for ProcurementReceipts line items");
            }
            if (lineItem.Received == null)
            {
                throw new ArgumentNullException("lineItem.Received", "Received is required for ProcurementReceipts line items");
            }
            if (!purchaseOrder.LineItems.Any(x => x.Id == lineItem.LineItemNumber))
            {
                throw new Exception(string.Concat("The requested item ID ", lineItem.LineItemNumber, " is not associated with purchase order ", purchaseOrder.Number));
            }
            if ((!lineItem.Received.Quantity.HasValue) && (lineItem.Received.Cost == null))
            {
                throw new ArgumentNullException("lineItem.Received", "Quantity or amount is required for ProcurementReceipts received line items");
            }
            if ((lineItem.Received.Quantity.HasValue) && (lineItem.Received.Cost != null))
            {
                throw new ArgumentNullException("lineItem.Received", "Either quantity or amount is required for ProcurementReceipts received line items");
            }

            var receiptItem = new PurchaseOrderReceiptItem(lineItem.LineItemNumber)
            {
                ReceivingComments = lineItem.Comment
            };
            receiptItem.ReceivedQty = lineItem.Received.Quantity;
            if (lineItem.Received.Cost != null)
            {
                if ((!lineItem.Received.Cost.Value.HasValue) || (lineItem.Received.Cost.Currency == null))
                {
                    throw new ArgumentNullException("lineItem.Received.Amount", "Both value and currency are required for ProcurementReceipts received line item Amounts");
                }
                receiptItem.ReceivedAmt = lineItem.Received.Cost.Value;
                var receivedAmtCurrency = lineItem.Received.Cost.Currency.ToString(); // await GetCurrencyCodeFromCurrencyIsoCodeAsync(lineItem.Received.Cost.Currency, purchaseOrder.HostCountry);
                if (receivedAmtCurrency != purchaseOrderCurrency)
                {
                    throw new Exception("Dollar-based receiving can only be done in the same currency as the purchase order itself.");
                }
                receiptItem.ReceivedAmtCurrency = receivedAmtCurrency;
            }

            if (lineItem.Rejected != null)
            {
                if ((!lineItem.Rejected.Quantity.HasValue) && (lineItem.Rejected.Cost == null))
                {
                    throw new ArgumentNullException("lineItem.Rejected", "Quantity or amount is required for ProcurementReceipts rejected line items");
                }
                if ((lineItem.Rejected.Quantity.HasValue) && (lineItem.Rejected.Cost != null))
                {
                    throw new ArgumentNullException("lineItem.Rejected", "Either Quantity or amount are required for ProcurementReceipts rejected line items");
                }


                if ((lineItem.Rejected.Quantity.HasValue) && (lineItem.Received.Quantity.HasValue))
                {
                    //If rejected.quantity and received.quantity are both non-null, then
                    // if (rejected.quantity is positive and received.quantity is negative) 
                    // or (rejected.quantity is negative and received.quantity is positive) then abort with an error 
                    if (((lineItem.Rejected.Quantity > 0) && (lineItem.Received.Quantity < 0))
                        || ((lineItem.Rejected.Quantity < 0) && (lineItem.Received.Quantity > 0)))
                    {
                        throw new ArgumentException("Cannot combine receiving and unreceiving in a single transaction. Received quantity/amount and rejected quantity/amount must both be negative or must both be positive.");
                    }
                    // If both rejected.quantity and received.quantity are non-null and the 
                    // absolute value of the rejected quantity is greater than the absolute 
                    // value of the received quantity, abort with an error 
                    if (Math.Abs(Convert.ToDecimal(lineItem.Rejected.Quantity)) > Math.Abs(Convert.ToDecimal(lineItem.Received.Quantity)))
                    {
                        throw new ArgumentException("Received quantity/amount includes the rejected quantity/amount; therefore, rejected quantity/amount may not be larger than the received quantity/amount.");
                    }
                }

                if ((lineItem.Rejected.Quantity.HasValue) && (lineItem.Received.Cost != null))
                {
                    // If rejected.quantity and received.amount.value are both non-null, 
                    // if (rejected.quantity is positive and received.amount.value is negative)
                    // or(rejected.quantity is negative and received.amount.value is positive) then abort with an error 
                    if (((lineItem.Rejected.Quantity > 0) && (lineItem.Received.Cost.Value < 0))
                       || ((lineItem.Rejected.Quantity < 0) && (lineItem.Received.Cost.Value > 0)))
                    {
                        throw new ArgumentException("Cannot combine receiving and unreceiving in a single transaction. Received quantity/amount and rejected quantity/amount must both be negative or must both be positive.");
                    }
                }

                if ((lineItem.Rejected.Cost != null) && (lineItem.Rejected.Cost.Value != null) && 
                    (lineItem.Rejected.Cost.Value.HasValue))
                {
                    if (lineItem.Received.Quantity.HasValue)
                    {
                        // If rejected.amount.value and received.quantity are both non-null, 
                        // if (rejected.amount.value is positive and received.quantity is negative) 
                        // or(rejected.amount.value is negative and received.quantity is positive) then abort with an error
                        if (((lineItem.Rejected.Cost.Value > 0) && (lineItem.Received.Quantity < 0))
                            || ((lineItem.Rejected.Cost.Value < 0) && (lineItem.Received.Quantity > 0)))
                        {
                            throw new ArgumentException("Cannot combine receiving and unreceiving in a single transaction. Received quantity/amount and rejected quantity/amount must both be negative or must both be positive.");
                        }
                    }
                    if (lineItem.Received.Cost != null && lineItem.Received.Cost.Value.HasValue)
                    {
                        // If rejected.amount.value and received.amount.value are both non-null, then 
                        // if (rejected.amount.value is positive and received.amount.value is negative) 
                        // or(rejected.amount.value is negative and received.amount.value is positive) 
                        // then abort with an error
                        if (((lineItem.Received.Cost.Value > 0) && (lineItem.Rejected.Cost.Value < 0))
                            || ((lineItem.Received.Cost.Value > 0) && (lineItem.Rejected.Cost.Value < 0)))
                        {
                            throw new ArgumentException("Cannot combine receiving and unreceiving in a single transaction. Received quantity/amount and rejected quantity/amount must both be negative or must both be positive.");
                        }
                        // If both rejected.amount.value and received.amount.value are non-null and the 
                        // absolute value of the rejected amount is greater than the absolute value of 
                        //the received amount, abort with an error
                        if (Math.Abs(Convert.ToDecimal(lineItem.Rejected.Cost.Value)) > Math.Abs(Convert.ToDecimal(lineItem.Received.Cost.Value)))
                        {
                            throw new ArgumentException("Received quantity/amount includes the rejected quantity/amount; therefore, rejected quantity/amount may not be larger than the received quantity/amount.");
                        }
                    }
                }

                receiptItem.RejectedQty = lineItem.Rejected.Quantity;
                if (lineItem.Rejected.Cost != null)
                {
                    if ((!lineItem.Rejected.Cost.Value.HasValue) || (lineItem.Rejected.Cost.Currency == null))
                    {
                        throw new ArgumentNullException("lineItem.Received.Amount", "Both value and currency are required for ProcurementReceipts received line item amounts");
                    }
                    receiptItem.RejectedAmt = lineItem.Rejected.Cost.Value;

                    var rejectedAmtCurrency = lineItem.Rejected.Cost.Currency.ToString(); 
                    if (rejectedAmtCurrency != purchaseOrderCurrency)
                    {
                        throw new Exception("Dollar-based receiving can only be done in the same currency as the purchase order itself.");
                    }
                    receiptItem.RejectedAmtCurrency = rejectedAmtCurrency;
                }
            }

            return receiptItem;
        }

        private async Task<string> GetCurrencyCodeFromCurrencyIsoCodeAsync(CurrencyIsoCode? currencyCode, string hostCountry = "USA")
        {
            if (currencyCode != null)
            {
                var currencyIsoCode = ConvertCurrencyIsoCodeToCurrencyCode(currencyCode);
                var currencyCodes = (await GetAllCurrencyConversionAsync());
                if (currencyCodes == null)
                {
                    throw new KeyNotFoundException("Unable to extract currency codes from CURRENCY.CONV");
                }
                var curCode = currencyCodes.FirstOrDefault(x => x.CurrencyCode == currencyIsoCode);
                if (curCode == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to locate currency code: ", currencyIsoCode));
                }
                return curCode.Code;
            }
            else if (!string.IsNullOrEmpty(hostCountry))
            {
                switch (hostCountry)
                {
                    case "US":
                    case "USA":
                        return "USD";
                    case "CAN":
                    case "CANADA":
                        return "CAD";

                }
            }
            return string.Empty;
        }

        /// <summary>
        ///  Get Currency ISO Code
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="hostCountry"></param>
        /// <returns><see cref="CurrencyIsoCode"></returns>
        private CurrencyIsoCode GetCurrencyIsoCode(string currencyCode, string hostCountry = "USA")
        {
            var currency = CurrencyIsoCode.USD;

            try
            {
                if (!string.IsNullOrEmpty(currencyCode))
                {
                    currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), currencyCode);
                }
                else
                {
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                        CurrencyIsoCode.USD;
                }
            }
            catch (Exception)
            {
                currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                    CurrencyIsoCode.USD;
            }
            return currency;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to purchase orders that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewProcurementReceiptsPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewProcurementReceipts) || HasPermission(ColleagueFinancePermissionCodes.CreateProcurementReceipts);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view procurement-receipts.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a POST operation. This API will integrate information related to purchase orders that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateProcurementReceiptsPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateProcurementReceipts);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create or update procurement-receipts.");
            }
        }

        /// <summary>
        /// Convert CurrencyIsoCode domain enumeration to CurrencyCode DTO enumeration
        /// </summary>
        /// <param name="code">CurrencyIsoCode domain enumeration</param>
        /// <returns>CurrencyCode DTO enumeration</returns>
        private Domain.ColleagueFinance.Entities.CurrencyCodes? ConvertCurrencyIsoCodeToCurrencyCode(CurrencyIsoCode? code)
        {
            if (code == null)
                return null;

            switch (code)
            {
                case CurrencyIsoCode.CAD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.CAD;
                case CurrencyIsoCode.EUR:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.EUR;
                case CurrencyIsoCode.USD:
                    return Domain.ColleagueFinance.Entities.CurrencyCodes.USD;              
                default:
                    return null;
            }
        }
    }
}
 