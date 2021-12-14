//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class PaymentTransactionsService : BaseCoordinationService, IPaymentTransactionsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPaymentTransactionsRepository _paymentTransactionsRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IColleagueFinanceReferenceDataRepository _colleagueFinanceReferenceDataRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IPersonRepository _personRepository;


        public PaymentTransactionsService(
            IPaymentTransactionsRepository paymentTransactionsRepository,
            IPersonRepository personRepository,
            IReferenceDataRepository referenceDataRepository,
            IInstitutionRepository institutionRepository,
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IAddressRepository addressRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _paymentTransactionsRepository = paymentTransactionsRepository;
            _referenceDataRepository = referenceDataRepository;
            _institutionRepository = institutionRepository;
            _colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            _addressRepository = addressRepository;
            _personRepository = personRepository;
        }

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<Domain.Base.Entities.State> _states = null;
        private async Task<IEnumerable<Domain.Base.Entities.State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }

        #region GET EEDM Version 12

        /// <summary>
        /// Gets all payment-transactions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="documentGuid">guid for accounts-payable-invoices</param>
        /// <param name="documentTypeValue">invoice or refund</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PaymentTransactions">paymentTransactions</see> objects</returns>          
        public async Task<Tuple<IEnumerable<Dtos.PaymentTransactions>, int>> GetPaymentTransactionsAsync(int offset, int limit,
            string documentGuid, InvoiceTypes documentTypeValue, Dtos.PaymentTransactions criteriaFilter, bool bypassCache = false)
        {
           
            var docNumber = string.Empty;
            var refPoDoc = new List<string>();
            var refBpoDoc = new List<string>();
            var refRecDoc = new List<string>();
            if (criteriaFilter != null)
            {
                docNumber = criteriaFilter.DocumentNumber;
                if (criteriaFilter.PaymentsFor != null && criteriaFilter.PaymentsFor.Any())
                {
                    foreach (var doc in criteriaFilter.PaymentsFor)
                    {
                        if (doc.Document != null && doc.Document.ReferenceDocument != null && doc.Document.ReferenceDocument.PurchaseOrder != null && !string.IsNullOrEmpty(doc.Document.ReferenceDocument.PurchaseOrder.Id))
                        {
                            var poId = await _paymentTransactionsRepository.GetIdFromGuidAsync(doc.Document.ReferenceDocument.PurchaseOrder.Id, "PURCHASE.ORDERS");
                            if (!string.IsNullOrEmpty(poId))
                                refPoDoc.Add(poId);
                            else
                                return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);
                        }
                        if (doc.Document != null && doc.Document.ReferenceDocument != null && doc.Document.ReferenceDocument.BlanketPurchaseOrder != null && !string.IsNullOrEmpty(doc.Document.ReferenceDocument.BlanketPurchaseOrder.Id))
                        {
                            var bpoId = await _paymentTransactionsRepository.GetIdFromGuidAsync(doc.Document.ReferenceDocument.BlanketPurchaseOrder.Id, "BPO");
                            if (!string.IsNullOrEmpty(bpoId))
                                refBpoDoc.Add(bpoId);
                            else
                                return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);

                        }
                        if (doc.Document != null && doc.Document.ReferenceDocument != null && !string.IsNullOrEmpty(doc.Document.ReferenceDocument.RecurringVoucher))
                        {
                            refRecDoc.Add(doc.Document.ReferenceDocument.RecurringVoucher);
                        }
                        if (doc.Document != null && doc.Document.ReferenceDocument != null && doc.Document.ReferenceDocument.PurchasingArrangement != null && !string.IsNullOrEmpty(doc.Document.ReferenceDocument.PurchasingArrangement.Id))
                        {
                            return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);
                        }
                    }
                }

            }
            try
            {

                var paymentTransactionsCollection = new List<Ellucian.Colleague.Dtos.PaymentTransactions>();
                var totalRecords = 0;

                var documentId = string.Empty;
                var invoiceOrRefund = InvoiceOrRefund.NotSet;
                Tuple<IEnumerable<PaymentTransaction>, int> paymentTransactionsEntities = null;
                if (!string.IsNullOrEmpty(documentGuid))
                {
                    documentId = await _paymentTransactionsRepository.GetIdFromGuidAsync(documentGuid, "VOUCHERS");
                    if (string.IsNullOrEmpty(documentId))
                    {
                        return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);
                    }
                    if (documentTypeValue == InvoiceTypes.NotSet)
                    {
                        return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);
                    }
                    invoiceOrRefund = ConvertInvoiceTypesDtoEnumToInvoiceOrRefundEnum(documentTypeValue);
                    if (invoiceOrRefund == InvoiceOrRefund.NotSet)
                    {
                        return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(new List<Dtos.PaymentTransactions>(), 0);
                    }
                }

                try
                {
                    paymentTransactionsEntities = await _paymentTransactionsRepository.GetPaymentTransactionsAsync(offset, limit, documentId, invoiceOrRefund, docNumber, refPoDoc, refBpoDoc, refRecDoc);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                }
                if (paymentTransactionsEntities != null)
                {

                    totalRecords = paymentTransactionsEntities.Item2;

                    if (paymentTransactionsEntities != null && paymentTransactionsEntities.Item1 != null)
                    {
                        var institutionIds = paymentTransactionsEntities.Item1.Select(x => x.Vendor).ToArray();

                        var institutions = await _institutionRepository.GetInstitutionsFromListAsync(institutionIds);

                        var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(paymentTransactionsEntities.Item1.Select(x => x.Vendor).ToArray());

                        var associatedVouchers = paymentTransactionsEntities.Item1.SelectMany(x => x.Vouchers);
                        Dictionary<string, string> poGuidCollection = null;
                        Dictionary<string, string> bpoGuidCollection = null;
                        if (associatedVouchers != null && associatedVouchers.Any())
                        {
                            var PoIds = associatedVouchers.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                            poGuidCollection = await _paymentTransactionsRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");
                            var BpoIds = associatedVouchers.Where(cv => !string.IsNullOrEmpty(cv.BlanketPurchaseOrderId)).Select(cd => cd.BlanketPurchaseOrderId).Distinct();
                            bpoGuidCollection = await _paymentTransactionsRepository.GetGuidsCollectionAsync(BpoIds.ToArray(), "BPO");
                        }

                        foreach (var paymentTransactionEntity in paymentTransactionsEntities.Item1)
                        {
                            if (paymentTransactionEntity.Guid != null)
                            {
                                var paymentTransactionDto = await ConvertPaymentTransactionsEntityToDtoAsync(paymentTransactionEntity, institutions, personGuidCollection, poGuidCollection, bpoGuidCollection, bypassCache);
                                paymentTransactionsCollection.Add(paymentTransactionDto);
                            }
                        }

                    }
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return new Tuple<IEnumerable<Dtos.PaymentTransactions>, int>(paymentTransactionsCollection, totalRecords);
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a paymentTransactions by guid.
        /// </summary>
        /// <param name="guid">Guid of the paymentTransactions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PaymentTransactions">paymentTransactions</see></returns>
        public async Task<Dtos.PaymentTransactions> GetPaymentTransactionsByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Payment Transaction.");
            }
           
            try
            {
                var paymentTransactions = await _paymentTransactionsRepository.GetPaymentTransactionsByGuidAsync(guid);
                Dictionary<string, string> personGuidCollection = null;
                var institutions = new List<Institution>();
                Dictionary<string, string> poGuidCollection = null;
                Dictionary<string, string> bpoGuidCollection = null;
                if (paymentTransactions != null)
                {
                    if (!string.IsNullOrEmpty(paymentTransactions.Vendor))
                    {
                        personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new string[] { paymentTransactions.Vendor });
                        institutions = (await _institutionRepository.GetInstitutionsFromListAsync(new string[] { paymentTransactions.Vendor })).ToList();
                    }

                    var associatedVouchers = paymentTransactions.Vouchers;

                    if (associatedVouchers != null && associatedVouchers.Any())
                    {
                        var PoIds = associatedVouchers.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                        poGuidCollection = await _paymentTransactionsRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");
                        var BpoIds = associatedVouchers.Where(cv => !string.IsNullOrEmpty(cv.BlanketPurchaseOrderId)).Select(cd => cd.BlanketPurchaseOrderId).Distinct();
                        bpoGuidCollection = await _paymentTransactionsRepository.GetGuidsCollectionAsync(BpoIds.ToArray(), "BPO");
                    }
                }
                var response = await ConvertPaymentTransactionsEntityToDtoAsync(paymentTransactions, institutions, personGuidCollection, poGuidCollection, bpoGuidCollection, bypassCache);

                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return response;
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No payment-transactions was found for guid  " + guid, ex);

                //IntegrationApiExceptionAddError("No payment-transactions was found for GUID " + guid, "GUID.Not.Found", guid, string.Empty, System.Net.HttpStatusCode.NotFound);
                //throw IntegrationApiException;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error", guid);

                throw IntegrationApiException;
            }
        }

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Checks domain entity to its corresponding PaymentTransactions DTO
        /// </summary>
        /// <param name="source">Checks domain entity</param>
        /// <returns>PaymentTransactions DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PaymentTransactions> ConvertPaymentTransactionsEntityToDtoAsync(Domain.ColleagueFinance.Entities.PaymentTransaction source,
            IEnumerable<Domain.Base.Entities.Institution> institutions, Dictionary<string, string> personGuidCollection, Dictionary<string, string> poGuidCollection, Dictionary<string, string> bpoGuidCollection, bool bypassCache = false)
        {
            var paymentTransactions = new Ellucian.Colleague.Dtos.PaymentTransactions();
            CurrencyIsoCode currency = GetCurrencyIsoCode(source.CurrencyCode, source.HostCountry);
            if (string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError("Could not find a GUID for payment transactions entity.", "GUID.Not.Found", id: source.Id);
            }
            else
            {
                paymentTransactions.Id = source.Guid;
            }
            paymentTransactions.DocumentNumber = source.Id;
            if (!(string.IsNullOrEmpty(source.ReferenceNumber)))
            {
                paymentTransactions.ReferenceNumber = source.ReferenceNumber;
            }
            paymentTransactions.PaymentMethod = ConvertPaymentMethodDomainEnumToPaymentMethodDtoEnum(source.PaymentMethod);
            if (paymentTransactions.PaymentMethod == Dtos.EnumProperties.PaymentMethod.NotSet)
            {
                IntegrationApiExceptionAddError("Invalid payment method.", "Bad.Data", source.Guid, source.Id);
            }
            paymentTransactions.PaymentDate = source.Date;

            paymentTransactions.Status = ConvertPaymentTransactionsStatusDomainEnumToPaymentTransactionsStatusDtoEnum(source.VoucherStatus);

            if (paymentTransactions.Status == PaymentTransactionsStatus.Void)
            {
                var paymentTransactionsVoid = new PaymentVoidDtoProperty()
                {
                    Type = PaymentTransactionsType.Payablereestablished,
                    Date = !source.VoidDate.HasValue ? source.Date : Convert.ToDateTime(source.VoidDate)
                };

                paymentTransactions.Void = paymentTransactionsVoid;
            }

            var paymentsFor = new List<PaymentsForDtoProperty>();
            if (source.PaymentMethod == Domain.ColleagueFinance.Entities.PaymentMethod.Check || source.PaymentMethod == Domain.ColleagueFinance.Entities.PaymentMethod.Directdeposit)
            {
                if (source.Vouchers != null && source.Vouchers.Any())
                {
                    foreach (var vou in source.Vouchers)
                    {
                        var id = vou.Guid;
                        if (string.IsNullOrEmpty(id))
                        {
                            IntegrationApiExceptionAddError("Unable to find guid for voucher id '" + vou.Id + "'.", "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            var amountAndCurrency = vou.VoucherInvoiceAmt;
                            if ((!string.IsNullOrEmpty(id)) && (amountAndCurrency != null))
                            {
                                var paymentsForDtoProperty = new PaymentsForDtoProperty();
                                paymentsForDtoProperty.Document = new InvoiceDtoProperty();

                                paymentsForDtoProperty.Document.Invoice = new GuidObject2(id);

                                paymentsForDtoProperty.Amount = new Amount2DtoProperty()
                                {
                                    Currency = GetCurrencyIsoCode(amountAndCurrency.Currency.ToString(), source.HostCountry),
                                    Value = amountAndCurrency.Value
                                };
                                if (!string.IsNullOrEmpty(vou.PurchaseOrderId))
                                {
                                    var poGuid = string.Empty;
                                    if (poGuidCollection == null)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Purchase Order guid not found for purchase order Id: '", vou.PurchaseOrderId, "'"), "GUID.Not.Found"
                                            , source.Guid, source.Id);
                                    }
                                    else
                                    {
                                        poGuidCollection.TryGetValue(vou.PurchaseOrderId, out poGuid);
                                        if (string.IsNullOrEmpty(poGuid))
                                        {
                                            IntegrationApiExceptionAddError(string.Concat("Purchase Order guid not found for purchase order Id: '", vou.PurchaseOrderId, "'"), "GUID.Not.Found"
                                                , source.Guid, source.Id);
                                        }
                                        else
                                        {
                                            var refdoc = new LineItemReferenceDocumentDtoProperty2();
                                            refdoc.PurchaseOrder = new GuidObject2(poGuid);
                                            paymentsForDtoProperty.Document.ReferenceDocument = refdoc;
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(vou.BlanketPurchaseOrderId))
                                {
                                    var bpoGuid = string.Empty;
                                    if (bpoGuidCollection == null)
                                    {
                                        IntegrationApiExceptionAddError(string.Concat("Blanket Purchase Order guid not found for BPO Id: '", vou.BlanketPurchaseOrderId, "'"), "GUID.Not.Found"
                                            , source.Guid, source.Id);
                                    }
                                    else
                                    {
                                        bpoGuidCollection.TryGetValue(vou.BlanketPurchaseOrderId, out bpoGuid);
                                        if (string.IsNullOrEmpty(bpoGuid))
                                        {
                                            IntegrationApiExceptionAddError(string.Concat("Blanket Purchase Order guid not found for BPO Id: '", vou.BlanketPurchaseOrderId, "'"), "GUID.Not.Found"
                                                , source.Guid, source.Id);
                                        }
                                        else
                                        {
                                            var refdoc = new LineItemReferenceDocumentDtoProperty2();
                                            refdoc.BlanketPurchaseOrder = new GuidObject2(bpoGuid);
                                            paymentsForDtoProperty.Document.ReferenceDocument = refdoc;
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(vou.RecurringVoucherId))
                                {
                                    var refdoc = new LineItemReferenceDocumentDtoProperty2();
                                    refdoc.RecurringVoucher = vou.RecurringVoucherId;
                                    paymentsForDtoProperty.Document.ReferenceDocument = refdoc;

                                }
                                // add line items detail
                                if (vou.LineItems != null & vou.LineItems.Any())
                                {
                                    var itemDtoList = new List<PaymentTransactionsLineItemDtoProperty>();
                                    foreach (var item in vou.LineItems)
                                    {
                                        var itemDto = new PaymentTransactionsLineItemDtoProperty();
                                        itemDto.LineItemNumber = item.Id;
                                        itemDto.Amount = new Amount2DtoProperty()
                                        {
                                            Currency = GetCurrencyIsoCode(amountAndCurrency.Currency.ToString(), source.HostCountry),
                                            Value = item.ExtendedPrice
                                        };
                                        itemDtoList.Add(itemDto);
                                    }
                                    paymentsForDtoProperty.Document.LineItems = itemDtoList;
                                }
                                paymentsFor.Add(paymentsForDtoProperty);
                            }
                        }
                    }
                }
            }
            else
            {
                if (source.Vouchers != null && source.Vouchers.Any())
                {
                    foreach (var vou in source.Vouchers)
                    {
                        var id = vou.Guid;
                        if (string.IsNullOrEmpty(id))
                        {
                            IntegrationApiExceptionAddError("Unable to find guid for voucher id '" + vou.Id + "'.", "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            var amountAndCurrency = vou.VoucherInvoiceAmt;
                            var paymentsForDtoProperty = new PaymentsForDtoProperty();
                            paymentsForDtoProperty.Document = new InvoiceDtoProperty();

                            paymentsForDtoProperty.Document.Refund = new GuidObject2(id);

                            paymentsForDtoProperty.Amount = new Amount2DtoProperty()
                            {
                                Currency = currency,
                                Value = source.PaymentAmount
                            };
                            // this is commented out as the voucher refund does not have reference document property yet in the model but could be added in the future. 
                            //if (!string.IsNullOrEmpty(vou.PurchaseOrderId))
                            //{
                            //    var poGuid = string.Empty;
                            //    if (poGuidCollection == null)
                            //    {
                            //        IntegrationApiExceptionAddError(string.Concat("Purchase Order guid not found for purchase order Id: '", vou.PurchaseOrderId, "'"), "GUID.Not.Found"
                            //            , source.Guid, source.Id);
                            //    }
                            //    else
                            //    {
                            //        poGuidCollection.TryGetValue(vou.PurchaseOrderId, out poGuid);
                            //        if (string.IsNullOrEmpty(poGuid))
                            //        {
                            //            IntegrationApiExceptionAddError(string.Concat("Purchase Order guid not found for purchase order Id: '", vou.PurchaseOrderId, "'"), "GUID.Not.Found"
                            //                , source.Guid, source.Id);
                            //        }
                            //        else
                            //        {
                            //            var refdoc = new LineItemReferenceDocumentDtoProperty2();
                            //            refdoc.PurchaseOrder = new GuidObject2(poGuid);
                            //            paymentsForDtoProperty.Document.ReferenceDocument = refdoc;
                            //        }
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(vou.BlanketPurchaseOrderId))
                            //{
                            //    var bpoGuid = string.Empty;
                            //    if (bpoGuidCollection == null)
                            //    {
                            //        IntegrationApiExceptionAddError(string.Concat("Blanket Purchase Order guid not found for BPO Id: '", vou.BlanketPurchaseOrderId, "'"), "GUID.Not.Found"
                            //            , source.Guid, source.Id);
                            //    }
                            //    else
                            //    {
                            //        bpoGuidCollection.TryGetValue(vou.BlanketPurchaseOrderId, out bpoGuid);
                            //        if (string.IsNullOrEmpty(bpoGuid))
                            //        {
                            //            IntegrationApiExceptionAddError(string.Concat("Blanket Purchase Order guid not found for BPO Id: '", vou.BlanketPurchaseOrderId, "'"), "GUID.Not.Found"
                            //                , source.Guid, source.Id);
                            //        }
                            //        else
                            //        {
                            //            var refdoc = new LineItemReferenceDocumentDtoProperty2();
                            //            refdoc.BlanketPurchaseOrder = new GuidObject2(bpoGuid);
                            //            paymentsForDtoProperty.Document.ReferenceDocument = refdoc;
                            //        }
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(vou.RecurringVoucherId))
                            //{
                            //    var refdoc = new LineItemReferenceDocumentDtoProperty2();
                            //    refdoc.RecurringVoucher = vou.RecurringVoucherId;
                            //    paymentsForDtoProperty.Document.ReferenceDocument = refdoc;

                            //}
                            //// add line items detail
                            //if (vou.LineItems != null & vou.LineItems.Any())
                            //{
                            //    var itemDtoList = new List<PaymentTransactionsLineItemDtoProperty>();
                            //    foreach (var item in vou.LineItems)
                            //    {
                            //        var itemDto = new PaymentTransactionsLineItemDtoProperty();
                            //        itemDto.LineItemNumber = item.Id;
                            //        itemDto.Amount = new Amount2DtoProperty()
                            //        {
                            //            Currency = GetCurrencyIsoCode(amountAndCurrency.Currency.ToString(), source.HostCountry),
                            //            Value = item.ExtendedPrice
                            //        };
                            //        itemDtoList.Add(itemDto);
                            //    }
                            //    paymentsForDtoProperty.Document.LineItems = itemDtoList;
                            //}
                            paymentsFor.Add(paymentsForDtoProperty);
                        }
                    }
                }

            }



            if (paymentsFor != null && paymentsFor.Any())
                paymentTransactions.PaymentsFor = paymentsFor;

            paymentTransactions.Amount = new Amount2DtoProperty()
            {
                Currency = currency,
                Value = source.PaymentAmount
            };

            if (!(string.IsNullOrEmpty(source.Vendor)))
            {
                var manualVendorDetails = new PayeeDetailsDtoProperty();
                var personGuid = string.Empty;
                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(string.Concat("Person guid not found for Person Id: '", source.Vendor, "'"), "GUID.Not.Found"
                        , source.Guid, source.Id);
                }
                else
                {
                    personGuidCollection.TryGetValue(source.Vendor, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Person guid not found for Person Id: '", source.Vendor, "'"), "GUID.Not.Found"
                            , source.Guid, source.Id);
                    }
                    else
                    {
                        var ledgerActivityReference = new LedgerActivityReference();
                        Institution institution = null;
                        if (institutions != null && institutions.Any())
                            institution = institutions.FirstOrDefault(i => i.Id.Equals(source.Vendor));

                        if (source.IsOrganization && institution == null)
                        {
                            ledgerActivityReference.Organization = new GuidObject2(personGuid);
                        }
                        else if (institution != null)
                        {
                            ledgerActivityReference.Institution = new GuidObject2(personGuid);
                        }
                        else
                        {
                            ledgerActivityReference.Person = new GuidObject2(personGuid);
                        }
                        manualVendorDetails.Payee = ledgerActivityReference;
                        if ((source.MiscName != null) && (source.MiscName.Any()))
                        {
                            string name = string.Empty;
                            foreach (var vouName in source.MiscName)
                            {
                                if (!string.IsNullOrEmpty(name))
                                {
                                    name = string.Concat(name, " ");
                                }
                                name = string.Concat(name, vouName);
                            }

                            manualVendorDetails.Name = name;
                        }
                        if ((source.Address != null) && (source.Address.Any()))
                        {
                            manualVendorDetails.AddressLines = source.Address;
                            try
                            {
                                manualVendorDetails.Place = await BuildAddressPlaceDtoAsync(source);
                            }
                            catch (Exception ex)
                            {
                                {
                                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data",
                                        source.Guid, source.Id);
                                }
                            }
                        }

                        paymentTransactions.PayeeDetails = manualVendorDetails;
                    }
                }
            }
            return paymentTransactions;
        }

        private static CurrencyIsoCode GetCurrencyIsoCode(string currencyCode, string hostCountry = "USD")
        {
            Dtos.EnumProperties.CurrencyIsoCode currency = Dtos.EnumProperties.CurrencyIsoCode.USD;
            try
            {
                if (!(string.IsNullOrEmpty(currencyCode)))
                {
                    currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), currencyCode);
                }
                else
                {
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                        CurrencyIsoCode.USD;
                }
            }
            catch
            {
                currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                    CurrencyIsoCode.USD;
            }

            return currency;
        }

        /// <summary>
        /// Build a Addresses DTO object from an Address entity
        /// </summary>
        /// <param name="source">Address Entty Object</param>
        /// <returns>An address object <see cref="Dtos.Addresses"/> in HeDM format</returns>
        private async Task<Dtos.AddressPlace> BuildAddressPlaceDtoAsync(Domain.ColleagueFinance.Entities.PaymentTransaction source, bool bypassCache = false)
        {
            var addressDto = new Dtos.AddressPlace();
            Dtos.AddressCountry addressCountry = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;

            var countries = await GetAllCountriesAsync(bypassCache);

            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(source.Country))
            {
                country = countries.FirstOrDefault(x => x.Code == source.Country);
                if (country == null)
                {
                    country = countries.FirstOrDefault(x => x.Description.ToUpper() == source.Country.ToUpper());
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(source.State))
                {
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.State);
                    if ((states != null) && (!string.IsNullOrEmpty(states.CountryCode)))
                    {
                        country = countries.FirstOrDefault(x => x.Code == states.CountryCode);
                    }
                }
                if (country == null)
                {
                    var hostCountry = await _addressRepository.GetHostCountryAsync();
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = countries.FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = countries.FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(source.Country))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + source.Country);
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + (await _addressRepository.GetHostCountryAsync()));
            }

            switch (country.IsoAlpha3Code)
            {
                case "USA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.USA;
                    addressCountry.PostalTitle = "UNITED STATES OF AMERICA";
                    break;
                case "CAN":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.CAN;
                    addressCountry.PostalTitle = "CANADA";
                    break;
                case "AUS":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.AUS;
                    addressCountry.PostalTitle = "AUSTRALIA";
                    break;
                case "BRA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.BRA;
                    addressCountry.PostalTitle = "BRAZIL";
                    break;
                case "MEX":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.MEX;
                    addressCountry.PostalTitle = "MEXICO";
                    break;
                case "NLD":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.NLD;
                    addressCountry.PostalTitle = "NETHERLANDS";
                    break;
                case "GBR":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.GBR;
                    addressCountry.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                    break;
                default:
                    try
                    {
                        addressCountry.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Concat(ex.Message, "For the Country: '", source.Country, "' .ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountry.PostalTitle = country.Description.ToUpper();
                    break;
            }

            if (!string.IsNullOrEmpty(source.State))
            {
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.State);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
            }
            if (region != null)
            {
                addressCountry.Region = region;
            }
            if (!string.IsNullOrEmpty(source.City))
            {
                addressCountry.Locality = source.City;
            }

            addressCountry.PostalCode = source.Zip;
            if (addressCountry.PostalCode == string.Empty) addressCountry.PostalCode = null;

            if (country != null)
                addressCountry.Title = country.Description;

            if (addressCountry != null
                && (!string.IsNullOrEmpty(addressCountry.Locality)
                || !string.IsNullOrEmpty(addressCountry.PostalCode)
                || addressCountry.Region != null
                || addressCountry.SubRegion != null
                || !string.IsNullOrEmpty(source.Country)))

                addressDto = new Dtos.AddressPlace() { Country = addressCountry };

            return addressDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PaymentMethods domain enumeration value to its corresponding PaymentMethod DTO enumeration value
        /// </summary>
        /// <param name="source">PaymentMethod domain enumeration value</param>
        /// <returns>PaymentMethod DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.PaymentMethod ConvertPaymentMethodDomainEnumToPaymentMethodDtoEnum(Domain.ColleagueFinance.Entities.PaymentMethod source)
        {
            switch (source)
            {

                case Domain.ColleagueFinance.Entities.PaymentMethod.Check:
                    return Dtos.EnumProperties.PaymentMethod.Check;
                case Domain.ColleagueFinance.Entities.PaymentMethod.Directdeposit:
                    return Dtos.EnumProperties.PaymentMethod.Directdeposit;
                case Domain.ColleagueFinance.Entities.PaymentMethod.Wire:
                    return Dtos.EnumProperties.PaymentMethod.Wire;
                case Domain.ColleagueFinance.Entities.PaymentMethod.Echeck:
                    return Dtos.EnumProperties.PaymentMethod.Echeck;
                case Domain.ColleagueFinance.Entities.PaymentMethod.Creditcard:
                    return Dtos.EnumProperties.PaymentMethod.Creditcard;
                case Domain.ColleagueFinance.Entities.PaymentMethod.Debitcard:
                    return Dtos.EnumProperties.PaymentMethod.Debitcard;
                default:
                    return Dtos.EnumProperties.PaymentMethod.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PaymentTransactionsStatus domain enumeration value to its corresponding PaymentTransactionsStatus DTO enumeration value
        /// </summary>
        /// <param name="source">PaymentTransactionsStatus domain enumeration value</param>
        /// <returns>PaymentTransactionsStatus DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.PaymentTransactionsStatus ConvertPaymentTransactionsStatusDomainEnumToPaymentTransactionsStatusDtoEnum(VoucherStatus? source)
        {
            switch (source)
            {
                case VoucherStatus.Paid:
                    return Dtos.EnumProperties.PaymentTransactionsStatus.Outstanding;
                case VoucherStatus.Reconciled:
                    return Dtos.EnumProperties.PaymentTransactionsStatus.Reconciled;
                case VoucherStatus.Outstanding:
                    return Dtos.EnumProperties.PaymentTransactionsStatus.Outstanding;
                case VoucherStatus.Voided:
                    return Dtos.EnumProperties.PaymentTransactionsStatus.Void;
                default:
                    return Dtos.EnumProperties.PaymentTransactionsStatus.Void;
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a InvoiceTypes dto enumeration value to its corresponding InvoiceOrRefund domain enumeration value
        /// </summary>
        /// <param name="source">InvoiceTypes dto enumeration value</param>
        /// <returns>InvoiceOrRefund domain enumeration value</returns>
        private InvoiceOrRefund ConvertInvoiceTypesDtoEnumToInvoiceOrRefundEnum(InvoiceTypes? source)
        {
            if (source == null)
                return InvoiceOrRefund.NotSet;

            switch (source)
            {
                case InvoiceTypes.Invoice:
                    return InvoiceOrRefund.Invoice;
                case InvoiceTypes.Refund:
                    return InvoiceOrRefund.Refund;
                default:
                    return InvoiceOrRefund.NotSet;
            }
        }

        /// <summary>
        /// Converts entity to reference dto.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<LedgerActivityReference> ConvertEntityToReferenceDtoAsync(List<Domain.Base.Entities.Person> _people, string source, bool bypassCache)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            LedgerActivityReference reference = null;

            if (_people != null && _people.Any())
            {
                var person = _people.FirstOrDefault(p => p.Guid.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (person == null)
                {
                    throw new KeyNotFoundException(string.Format("Person not found for id: {0}.", source));
                }

                if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && !person.PersonCorpIndicator.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    reference = new LedgerActivityReference()
                    {
                        Person = new GuidObject2(person.Guid)
                    };
                    return reference;
                }

                //var institution = (await GetInstitutionsAsync(bypassCache)).FirstOrDefault(i => i.Id.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                var institution = _institutionRepository.GetInstitutionAsync(person.Id);
                if (institution != null)
                {
                    reference = new LedgerActivityReference()
                    {
                        Institution = new GuidObject2(person.Guid)
                    };
                    return reference;
                }

                if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    reference = new LedgerActivityReference()
                    {
                        Organization = new GuidObject2(person.Guid)
                    };
                    return reference;
                }
            }
            return reference;
        }
    }
}