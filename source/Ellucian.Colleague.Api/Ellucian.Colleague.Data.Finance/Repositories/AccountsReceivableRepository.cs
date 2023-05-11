// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Text.RegularExpressions;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    /// <summary>
    /// Repository for accounts receivable
    /// </summary>
    [RegisterType]
    public class AccountsReceivableRepository : PersonRepository, IAccountsReceivableRepository
    {

        private readonly string _colleagueTimeZone;
        /// <summary>
        /// Constructor for accounts receivable repository
        /// </summary>
        /// <param name="cacheProvider">Cache provider</param>
        /// <param name="transactionFactory">Transaction factory</param>
        /// <param name="logger">Logger</param>
        public AccountsReceivableRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger, apiSettings)
        {
            // Override the default cache value - unless otherwise specified, only cache data for 1 minute
            CacheTimeout = 1;
            if (apiSettings != null)
            {
                _colleagueTimeZone = apiSettings.ColleagueTimeZone;
            }
        }

        #region Receivable Type Codes (AR Types)

        /// <summary>
        /// List of accounts receivable types
        /// </summary>
        public IEnumerable<ReceivableType> ReceivableTypes
        {
            get
            {
                // Cache this entry for 1 day
                return GetCodeItem<ArTypes, ReceivableType>("AllAccountTypes", "AR.TYPES",
                    t => new ReceivableType(t.Recordkey, t.ArtDesc), Level1CacheTimeoutValue);
            }
        }

        #endregion

        #region Invoice Types

        /// <summary>
        /// List of invoice types
        /// </summary>
        public IEnumerable<InvoiceType> InvoiceTypes
        {
            get
            {
                // Cache this entry for 1 day
                return GetValcode<InvoiceType>("ST", "INVOICE.TYPES",
                    t => new InvoiceType(t.ValInternalCodeAssocMember, t.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
            }
        }

        #endregion

        #region External Systems

        /// <summary>
        /// List of external systems
        /// </summary>
        public IEnumerable<ExternalSystem> ExternalSystems
        {
            get
            {
                // Cache this entry for 1 day
                return GetValcode<ExternalSystem>("ST", "RES.LIFE.EXTL.ID.SOURCE",
                    t => new ExternalSystem(t.ValInternalCodeAssocMember, t.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);
            }
        }

        #endregion

        #region Charge Codes (AR Codes)

        /// <summary>
        /// List of charge codes
        /// </summary>
        public IEnumerable<ChargeCode> ChargeCodes
        {
            get
            {
                // Cache this entry for 1 day
                return GetCodeItem<ArCodes, ChargeCode>("AllChargeCodes", "AR.CODES",
                    t => new ChargeCode(t.Recordkey, t.ArcDesc, t.ArcPriority), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// List of charge codes
        /// </summary>
        public async Task<IEnumerable<ChargeCode>> GetChargeCodesAsync()
        {
            // Cache this entry for 1 day
            return await GetCodeItemAsync<ArCodes, ChargeCode>("AllChargeCodes", "AR.CODES",
                t => new ChargeCode(t.Recordkey, t.ArcDesc, t.ArcPriority), Level1CacheTimeoutValue);
        }

        #endregion

        #region Deposit Types

        /// <summary>
        /// List of deposit types
        /// </summary>
        public IEnumerable<DepositType> DepositTypes
        {
            get
            {
                // Cache this entry for 1 day
                return GetCodeItem<ArDepositTypes, DepositType>("AllDepositTypes", "AR.DEPOSIT.TYPES",
                    t => new DepositType(t.Recordkey, t.ArdtDesc), Level1CacheTimeoutValue);
            }
        }

        #endregion

        #region Get Account Holder

        /// <summary>
        /// Get an accountholder
        /// </summary>
        /// <param name="personId">Accountholder ID</param>
        /// <returns>Accountholder data</returns>
        public async Task<AccountHolder> GetAccountHolderAsync(string personId, bool bypassCache = false)
        {
            // Make sure we have an ID passed in
            if (String.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            try
            {
                AccountHolder accountHolder = await base.GetAsync<AccountHolder>(personId,
                        person =>
                        {
                            AccountHolder personAccountHolder = new AccountHolder(person.Recordkey, person.LastName, person.PrivacyFlag);
                            personAccountHolder.AddDepositsDue(GetDepositsDue(personId));
                            return personAccountHolder;
                        }
                    , bypassCache);

                return accountHolder;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session got expired while retrieving account holder.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occured while retrieving account holder.");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        public async Task<IEnumerable<AccountHolder>> SearchAccountHoldersByKeywordAsync(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("criteria");
            }

            List<AccountHolder> accountHolders = new List<AccountHolder>();
            List<string> personIds = new List<string>();

            // If search string is a numeric ID, add only that ID to the list 
            int personId;
            bool isId = int.TryParse(criteria, out personId);
            if (isId)
            {
                //Format the id according to the existing configuration
                string id = await PadIdPerPid2ParamsAsync(criteria);
                personIds.Add(id);
            }
            // If search string is alphanumeric, parse names from query and add matching persons to list
            else
            {
                string lastName = "";
                string firstName = "";
                string middleName = "";
                ParseNames(criteria, ref lastName, ref firstName, ref middleName);
                if (string.IsNullOrEmpty(firstName))
                {
                    throw new ArgumentException("Either an id or a first and last name must be supplied.");
                }
                personIds.AddRange(await base.SearchByNameAsync(lastName, firstName, middleName));
            }
            // If there are no persons, return the empty set
            if (personIds == null || !personIds.Any()) return accountHolders;

            // Filter out corporations from the set of persons
            personIds = (await FilterOutCorporationsAsync(personIds)).ToList();

            // Build account holder objects for each non-corporation person
            foreach (string id in personIds)
            {
                AccountHolder holder = await GetAsync<AccountHolder>(id,
                   person =>
                   {
                       AccountHolder personAccountHolder = new AccountHolder(person.Recordkey, person.LastName, person.PrivacyFlag);
                       personAccountHolder.AddDepositsDue(GetDepositsDue(id));
                       return personAccountHolder;
                   });
                accountHolders.Add(holder);
            }
            return accountHolders;
        }

         /// <summary>
        /// Searches for account holders for the specified person ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Set of FinancialAidPerson entities</returns>
        public async Task<IEnumerable<AccountHolder>> SearchAccountHoldersByIdsAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids");
            }

            List<AccountHolder> accountHolders = new List<AccountHolder>();
            IEnumerable<string> personIds = new List<string>();

            // Filter out corporations from the set of persons
            personIds = await FilterOutCorporationsAsync(ids);

            if (personIds.Any())
            {
                var persons = await GetPersonsBaseAsync(personIds);
                foreach (var person in persons)
                {
                    accountHolders.Add(new AccountHolder(person.Id, person.LastName, person.PrivacyStatusCode)
                    {
                        MiddleName = person.MiddleName,
                        FirstName = person.FirstName
                    });
                }
            }
            else
            {
                string message = string.Format("Could not locate account holders for specified ids");
                logger.Warn(message);
                throw new ApplicationException(message);
            }
            return accountHolders;
        }

        #endregion

        #region Get Invoice(s)

        /// <summary>
        /// Get an invoice by ID
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>The invoice info</returns>
        public Invoice GetInvoice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Invoice ID must be specified.");
            }

            try
            {
                ArInvoices arInvoice = DataReader.ReadRecord<ArInvoices>(id);
                if (arInvoice == null)
                {
                    throw new ArgumentOutOfRangeException("Invoice ID " + id + " is not valid.");
                }

                return BuildInvoice(arInvoice);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a group of invoices by ID
        /// </summary>
        /// <param name="ids">List of invoice IDs</param>
        /// <returns>List of invoices</returns>
        public IEnumerable<Invoice> GetInvoices(IEnumerable<string> ids)
        {
            try
            {
                if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "At least one invoice ID must be specified.");
            }

            Collection<ArInvoices> items = DataReader.BulkReadRecord<ArInvoices>(ids.ToArray());
            if (items == null || items.Count != ids.Count())
            {
                throw new ArgumentOutOfRangeException("ids", "Some invoices could not be retrieved from the database.");
            }

            
                var invoices = new List<Invoice>();
                foreach (var inv in items)
                {
                    invoices.Add(BuildInvoice(inv));
                }

                return invoices;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session got expired  while retrieving invoices.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occured while retrieving invoices.");
                throw;
            }
        }

        /// <summary>
        /// Build an invoice entity from a Colleague invoice
        /// </summary>
        /// <param name="arInvoice">Colleague AR.INVOICES record</param>
        /// <returns>Invoice entity</returns>
        private Invoice BuildInvoice(ArInvoices arInvoice)
        {
            var invoiceItems = GetCharges(arInvoice.InvInvoiceItems);
            var invoice = new Invoice(arInvoice.Recordkey,
                arInvoice.InvPersonId,
                arInvoice.InvArType,
                arInvoice.InvTerm,
                arInvoice.InvNo,
                arInvoice.InvDate.Value,
                arInvoice.InvDueDate.Value,
                arInvoice.InvBillingStartDate.Value,
                arInvoice.InvBillingEndDate.Value,
                arInvoice.InvDesc, invoiceItems)
            {
                Archived = !String.IsNullOrEmpty(arInvoice.InvArchive),
                AdjustmentToInvoice = arInvoice.InvAdjToInvoice
            };
            if (arInvoice.InvDueDate.HasValue)
            {
                invoice.DueDateOffset = arInvoice.InvDueDate.ToPointInTimeDateTimeOffset(arInvoice.InvDueDate.Value, _colleagueTimeZone).GetValueOrDefault();
            }
            if (arInvoice.InvAdjByInvoices != null && arInvoice.InvAdjByInvoices.Count > 0)
            {
                foreach (var adj in arInvoice.InvAdjByInvoices)
                {
                    invoice.AddAdjustingInvoice(adj);
                }
            }
            return invoice;
        }
        /// <summary>
        /// Get an invoice by ID
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>The invoice info</returns>
        public ReceivableInvoice GetReceivableInvoice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Invoice ID must be specified.");
            }

            try
            {
                ArInvoices arInvoice = DataReader.ReadRecord<ArInvoices>(id);
                if (arInvoice == null)
                {
                    throw new KeyNotFoundException("Invoice ID " + id + " is not valid.");
                }

                return BuildReceivableInvoice(arInvoice);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a group of invoices by ID
        /// </summary>
        /// <param name="ids">List of invoice IDs</param>
        /// <returns>List of invoices</returns>
        public IEnumerable<ReceivableInvoice> GetReceivableInvoices(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "At least one invoice ID must be specified.");
            }

            try
            {
                Collection<ArInvoices> items = DataReader.BulkReadRecord<ArInvoices>(ids.ToArray());
                if (items == null || items.Count != ids.Count())
                {
                    throw new KeyNotFoundException("Failed to retrieve one or more of invoice ids " + String.Join(", ", ids.ToArray()) + " from the database.");
                }

                var invoices = new List<ReceivableInvoice>();
                foreach (var inv in items)
                {
                    invoices.Add(BuildReceivableInvoice(inv));
                }

                return invoices;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

        }

        /// <summary>
        /// Build an invoice entity from a Colleague invoice
        /// </summary>
        /// <param name="arInvoice">Colleague AR.INVOICES record</param>
        /// <returns>ReceivableInvoice entity</returns>
        private ReceivableInvoice BuildReceivableInvoice(ArInvoices arInvoice)
        {
            var invoiceItems = GetReceivableCharges(arInvoice.InvInvoiceItems);
            var invoice = new ReceivableInvoice(arInvoice.Recordkey,
                arInvoice.InvNo,
                arInvoice.InvPersonId,
                arInvoice.InvArType,
                arInvoice.InvTerm,
                arInvoice.InvDate.HasValue ? arInvoice.InvDate.Value : DateTime.Today,
                arInvoice.InvDueDate.HasValue ? arInvoice.InvDueDate.Value : DateTime.Today,
                arInvoice.InvBillingStartDate.HasValue ? arInvoice.InvBillingStartDate.Value : DateTime.Today,
                arInvoice.InvBillingEndDate.HasValue ? arInvoice.InvBillingEndDate.Value : DateTime.Today,
                arInvoice.InvDesc, invoiceItems)
            {
                InvoiceType = arInvoice.InvType,
                IsArchived = !string.IsNullOrEmpty(arInvoice.InvArchive),
                AdjustmentToInvoice = arInvoice.InvAdjToInvoice,
                Location = arInvoice.InvLocation
            };
            if (arInvoice.InvAdjByInvoices != null && arInvoice.InvAdjByInvoices.Count > 0)
            {
                foreach (var adj in arInvoice.InvAdjByInvoices)
                {
                    invoice.AddAdjustingInvoice(adj);
                }
            }
            if (!string.IsNullOrEmpty(arInvoice.InvExternalSystem) && !string.IsNullOrEmpty(arInvoice.InvExternalId))
            {
                invoice.AddExternalSystemAndId(arInvoice.InvExternalSystem, arInvoice.InvExternalId);
            }

            return invoice;
        }

        #endregion

        #region Get Payment(s)

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>The payment info</returns>
        public ReceivablePayment GetPayment(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment ID must be specified.");
            }
            try
            {
                ArPayments arPayment = DataReader.ReadRecord<ArPayments>(id);
                if (arPayment == null)
                {
                    throw new ArgumentOutOfRangeException("Payment ID " + id + " is not valid.");
                }

                return BuildPayment(arPayment);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a group of payments by ID
        /// </summary>
        /// <param name="ids">List of payment IDs</param>
        /// <returns>List of payments</returns>
        public IEnumerable<ReceivablePayment> GetPayments(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "At least one payment ID must be specified.");
            }

            try
            {
                Collection<ArPayments> pmts = DataReader.BulkReadRecord<ArPayments>(ids.ToArray());

                if (pmts == null || pmts.Count != ids.Count())
                {
                    throw new ArgumentOutOfRangeException("ids", "Some payments could not be retrieved from the database.");
                }

                foreach (var rev in GetReversingPayments(pmts))
                {
                    pmts.Add(rev);
                }

                var payments = new List<ReceivablePayment>();
                foreach (var pmt in pmts)
                {
                    payments.Add(BuildPayment(pmt));
                }

                return payments;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Build a receivable payment entity from a Colleague payment
        /// </summary>
        /// <param name="arInvoice">Colleague AR.PAYMENTS record</param>
        /// <returns>PaymentReceipt entity</returns>
        private ReceivablePayment BuildPayment(ArPayments arPayment)
        {
            try
            {
                // Cash Receipt
                if (!string.IsNullOrEmpty(arPayment.ArpCashRcpt))
                {
                    string refNo = null;
                    CashRcpts cr = DataReader.ReadRecord<CashRcpts>(arPayment.ArpCashRcpt);
                    refNo = cr != null ? cr.RcptNo : null;

                    var crPayment = new ReceiptPayment(arPayment.Recordkey,
                    refNo,
                    arPayment.ArpPersonId,
                    arPayment.ArpArType,
                    arPayment.ArpTerm,
                    arPayment.ArpDate.HasValue ? arPayment.ArpDate.Value : DateTime.Today,
                    arPayment.ArpAmt.GetValueOrDefault() - arPayment.ArpReversalAmt.GetValueOrDefault(),
                    arPayment.ArpCashRcpt)
                    {
                        Location = arPayment.ArpLocation,
                        IsArchived = !String.IsNullOrEmpty(arPayment.ArpArchive),
                    };

                    // TODO: Net the payment amount along with any associated reversals/reversals of reversals

                    return crPayment;
                }
                else
                {
                    // TODO: Add logic to build/return other types of payments (FA, SB, Deposit Item, AR Transfer, Payroll, System-Gen, Refund)
                    throw new NotImplementedException("Non-CR payments are not implemented in IPC.");
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the amount of the payment that reverses an AR.PAYMENT, if one exists
        /// </summary>
        /// <param name="reversingPaymentId">ID of the reversing AR.PAYMENTS record</param>
        /// <returns>Reversal amount</returns>
        private Collection<ArPayments> GetReversingPayments(Collection<ArPayments> pmts)
        {
            Collection<ArPayments> reversals = new Collection<ArPayments>();

            try
            {
                foreach (var pmt in pmts)
                {
                    if (!string.IsNullOrEmpty(pmt.ArpReversedByPayment))
                    {
                        reversals.Add(DataReader.ReadRecord<ArPayments>(pmt.ArpReversedByPayment));
                    }
                }

                if (reversals.Any(x => !string.IsNullOrEmpty(x.ArpReversedByPayment)))
                {
                    var moreReversals = GetReversingPayments(reversals);
                    foreach (var rev in moreReversals)
                    {
                        reversals.Add(rev);
                    }
                }

                return reversals;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        #endregion

        #region Get Charges

        /// <summary>
        /// Get a charge by ID
        /// </summary>
        /// <param name="id">Charge ID</param>
        /// <returns>The charge</returns>
        public Charge GetCharge(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Charge ID must be specified");
            }

            var charge = DataReader.ReadRecord<ArInvoiceItems>(id);
            if (charge == null)
            {
                throw new KeyNotFoundException("ID not found: " + id);
            }

            return BuildCharge(charge);
        }

        /// <summary>
        /// Get a list of charges by ID
        /// </summary>
        /// <param name="ids">List of charge IDs</param>
        /// <returns>List of charges</returns>
        public IEnumerable<Charge> GetCharges(IEnumerable<String> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "Invoice Item IDs must be specified");
            }

            Collection<ArInvoiceItems> items = DataReader.BulkReadRecord<ArInvoiceItems>(ids.ToArray());
            if (items == null || items.Count != ids.Count())
            {
                throw new ArgumentOutOfRangeException("ids", "Failed to retrieve one or more of invoice items " + String.Join(", ", ids.ToArray()) + " from the database.");
            }

            var charges = items.Select(x => BuildCharge(x)).ToList();
            return charges;
        }

        /// <summary>
        /// Build a charge entity from a Colleague invoice item
        /// </summary>
        /// <param name="item">Colleague AR.INVOICE.ITEMS record</param>
        /// <returns>Charge entity</returns>
        private Charge BuildCharge(ArInvoiceItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "An invoice item cannot be null");
            }

            var charge = new Charge(item.Recordkey, item.InviInvoice, item.InviDesc.Split(DmiString._VM), item.InviArCode,
                item.InviExtChargeAmt.GetValueOrDefault() - item.InviExtCrAmt.GetValueOrDefault());

            // Calculate the total tax, if any
            decimal tax = 0;
            if (item.InviArCodeTaxDistrs != null && item.InviArCodeTaxDistrs.Count > 0)
            {
                var taxes = DataReader.BulkReadRecord<ArCodeTaxGlDistr>(item.InviArCodeTaxDistrs.ToArray());
                tax += taxes.Sum(taxItem => taxItem.ArctdGlTaxAmt.GetValueOrDefault());
            }
            charge.TaxAmount = tax;

            // Add any associated payment plan IDs to charges
            if (item.InviArPayPlans != null && item.InviArPayPlans.Count > 0)
            {
                foreach (var planId in item.InviArPayPlans)
                {
                    charge.AddPaymentPlan(planId);
                }
            }

            return charge;
        }

        /// <summary>
        /// Get a charge by ID
        /// </summary>
        /// <param name="id">Charge ID</param>
        /// <returns>The charge</returns>
        public ReceivableCharge GetReceivableCharge(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Charge ID must be specified");
            }

            return GetReceivableCharges(new List<String>() { id }).FirstOrDefault();
        }

        /// <summary>
        /// Get a list of charges by ID
        /// </summary>
        /// <param name="ids">List of charge IDs</param>
        /// <returns>List of ReceivableCharges</returns>
        public IEnumerable<ReceivableCharge> GetReceivableCharges(IEnumerable<String> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "Invoice Item IDs must be specified");
            }

            Collection<ArInvoiceItems> items = DataReader.BulkReadRecord<ArInvoiceItems>(ids.ToArray(), false);
            if (items == null || items.Count != ids.Count())
            {
                throw new KeyNotFoundException("Failed to retrieve one or more of invoice items " + String.Join(", ", ids.ToArray()) + " from the database.");
            }

            List<ReceivableCharge> charges = new List<ReceivableCharge>();
            foreach (var item in items)
            {
                charges.Add(BuildReceivableCharge(item));
            }

            return charges;
        }

        /// <summary>
        /// Build a charge entity from a Colleague invoice item
        /// </summary>
        /// <param name="item">Colleague AR.INVOICE.ITEMS record</param>
        /// <returns>ReceivableCharge entity</returns>
        private ReceivableCharge BuildReceivableCharge(ArInvoiceItems item)
        {
            if (item == null)
            {
                throw new ArgumentException("item", "An invoice item cannot be null");
            }

            var charge = new ReceivableCharge(item.Recordkey, item.InviInvoice, item.InviDesc.Split(DmiString._VM), item.InviArCode,
                item.InviExtChargeAmt.GetValueOrDefault() - item.InviExtCrAmt.GetValueOrDefault());

            // Calculate the total tax, if any
            decimal tax = 0;
            if (item.InviArCodeTaxDistrs != null && item.InviArCodeTaxDistrs.Count > 0)
            {
                Collection<ArCodeTaxGlDistr> taxes = DataReader.BulkReadRecord<ArCodeTaxGlDistr>(item.InviArCodeTaxDistrs.ToArray());
                foreach (var taxItem in taxes)
                {
                    tax += taxItem.ArctdGlTaxAmt.GetValueOrDefault();
                }
            }
            charge.TaxAmount = tax;

            // Add any associated payment plan IDs to charges
            if (item.InviArPayPlans != null && item.InviArPayPlans.Count > 0)
            {
                foreach (var planId in item.InviArPayPlans)
                {
                    charge.AddPaymentPlan(planId);
                }
            }

            return charge;
        }
        #endregion

        #region Get Deposits Due

        /// <summary>
        /// Get the deposits due for a person
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <returns>List of deposits due</returns>
        public List<DepositDue> GetDepositsDue(string personId)
        {
            // Make sure we have an ID passed in
            if (String.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            // Get the deposits due for the specified student
            var result = new List<DepositDue>();
            string template = "WITH ARDD.PERSON.ID = '{0}'";
            string ddCriteria = String.Format(template, personId);
            Collection<ArDepositsDue> arDepositsDue = DataReader.BulkReadRecord<ArDepositsDue>(ddCriteria);
            if (arDepositsDue == null || !arDepositsDue.Any())
            {
                return result;
            }

            foreach (var arDepositDue in arDepositsDue)
            {
                // Create a deposit due using the data returned from Colleague
                var depositDue = new DepositDue(
                    arDepositDue.Recordkey,
                    arDepositDue.ArddPersonId,
                    arDepositDue.ArddAmount.GetValueOrDefault(),
                    arDepositDue.ArddDepositType,
                    arDepositDue.ArddDueDate.GetValueOrDefault())
                        {
                            TermId = arDepositDue.ArddTerm
                        };
                if (arDepositDue.ArddDueDate.HasValue)
                {
                    depositDue.DueDateOffsetCTZS = arDepositDue.ArddDueDate.ToPointInTimeDateTimeOffset(arDepositDue.ArddDueDate, _colleagueTimeZone).GetValueOrDefault();
                }
                var depositType = DepositTypes.Where(dt => dt.Code == depositDue.DepositType).FirstOrDefault();
                depositDue.DepositTypeDescription = depositType != null ? depositType.Description : string.Empty;

                // Get the deposits for this deposit due
                string depCriteria = String.Format("WITH ARD.DEPOSITS.DUE = '{0}' WITH ARD.AR.POSTED.FLAG = 'Y'", arDepositDue.Recordkey);
                Collection<ArDeposits> arDeposits = DataReader.BulkReadRecord<ArDeposits>(depCriteria);
                foreach (var arDeposit in arDeposits)
                {
                    decimal netAmount = arDeposit.ArdAmt.GetValueOrDefault() - arDeposit.ArdReversalAmt.GetValueOrDefault();
                    // Don't add a deposit if it has a zero amount; otherwise, create the
                    // deposit and add it to the deposit due
                    if (netAmount != 0)
                    {
                        depositDue.AddDeposit(new Deposit(
                            arDeposit.Recordkey,
                            arDeposit.ArdPersonId,
                            arDeposit.ArdDate.GetValueOrDefault(),
                            arDeposit.ArdDepositType,
                            netAmount)
                            {
                                TermId = arDeposit.ArdTerm
                            });
                    }
                }
                // Add this deposit due to the returned list
                result.Add(depositDue);
            }
            // Sort the deposits due by ascending due date
            return result.OrderBy(x => x.DueDate).ToList();
        }

        #endregion

        #region Get Distribution

        /// <summary>
        /// Get the distribution for an account
        /// </summary>
        /// <param name="studentId">Person ID</param>
        /// <param name="accountType">Receivable type</param>
        /// <param name="callingProcess">ID of calling process</param>
        /// <returns>Distribution code</returns>
        public string GetDistribution(string studentId, string accountType, string callingProcess)
        {
            return GetDistributions(studentId, new List<string>() { accountType }, callingProcess).FirstOrDefault();
        }

        /// <summary>
        /// Get the distributions for a group of accounts
        /// </summary>
        /// <param name="studentId">Person ID</param>
        /// <param name="accountTypes">List of receivable types</param>
        /// <param name="paymentProcess">ID of payment process</param>
        /// <returns>List of distribution codes</returns>
        public IEnumerable<string> GetDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess)
        {
            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            var request = new TxDetermineGlDistributionRequest()
                {
                    InPersonId = studentId,
                    InArType = accountTypes.ToList(),
                    InCallingProcess = paymentProcess
                };
            TxDetermineGlDistributionResponse response = transactionInvoker.Execute<TxDetermineGlDistributionRequest, TxDetermineGlDistributionResponse>(request);

            return response.OutDistribution;
        }

        #endregion

        #region PostReceivableInvoice
        /// <summary>
        /// Create a receivable invoice
        /// </summary>
        /// <param name="source">The invoice to create</param>
        /// <returns>The created invoice</returns>
        public ReceivableInvoice PostReceivableInvoice(ReceivableInvoice source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The invoice to create must be specified.");
            }

            try
            {
                // build list of ChargeGroups
                var items = new List<ChargeGroup>();
                foreach (var charge in source.Charges)
                {
                    items.Add(new ChargeGroup()
                    {
                        ChargeCodes = charge.Code,
                        ChargeDescriptions = String.Join(Ellucian.Dmi.Runtime.DmiString.sSM, charge.Description.ToArray()),
                        ChargeAmounts = charge.BaseAmount
                    });
                }

                // Execute the CreateReceivableInvoice transaction
                var request = new CreateReceivableInvoiceRequest()
                {
                    PersonId = source.PersonId,
                    ReceivableType = source.ReceivableType,
                    InvoiceDescription = source.Description,
                    InvoiceType = source.InvoiceType,
                    ExternalInvoiceId = source.ExternalIdentifier,
                    ExternalSystem = source.ExternalSystem,
                    InvoiceDate = source.Date,
                    TermCode = source.TermId,
                    InvoiceBillingStartDate = source.BillingStart,
                    InvoiceBillingEndDate = source.BillingEnd,
                    InvoiceDueDate = source.DueDate,
                    Location = source.Location,
                    ChargeGroup = items
                };
                var response = transactionInvoker.Execute<CreateReceivableInvoiceRequest, CreateReceivableInvoiceResponse>(request);

                if (response.ErrorMessage != null && response.ErrorMessage.Count > 0)
                {
                    // Update failed
                    logger.Error(response.ErrorMessage.ToString());
                    throw new InvalidOperationException(String.Join("\n", response.ErrorMessage));
                }

                return GetReceivableInvoice(response.InvoiceId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }
        #endregion

        #region GetDeposit

        /// <summary>
        /// Get a deposit
        /// </summary>
        /// <param name="id">The ID of the deposit to retrieve</param>
        /// <returns>The retrieved deposit</returns>
        public Deposit GetDeposit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(id);
            }

            var ids = new List<string>() { id };
            var deposits = GetDeposits(ids);
            return deposits.First();
        }

        /// <summary>
        /// Get a list of deposits
        /// </summary>
        /// <param name="ids">The list of ids of the deposits to retrieve</param>
        /// <returns>The list of retrieved deposits</returns>
        public IEnumerable<Deposit> GetDeposits(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids", "Deposits must be specified.");
            }

            try
            {
                Collection<ArDeposits> items = DataReader.BulkReadRecord<ArDeposits>(ids.ToArray());
                if (items == null || items.Count != ids.Count())
                {
                    throw new KeyNotFoundException("Failed to retrieve one or more of deposit ids " + String.Join(", ", ids.ToArray()) + " from the database.");
                }

                var response = items.Select(x => BuildDeposit(x)).ToList();

                return response;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private Deposit BuildDeposit(ArDeposits deposit)
        {
            decimal amt = deposit.ArdAmt.GetValueOrDefault() - deposit.ArdReversalAmt.GetValueOrDefault();
            var response = new Deposit(deposit.Recordkey, deposit.ArdPersonId, deposit.ArdDate.GetValueOrDefault(),
                deposit.ArdDepositType, amt)
                {
                    TermId = deposit.ArdTerm,
                    ReceiptId = deposit.ArdCashRcpt
                };
            if (!string.IsNullOrEmpty(deposit.ArdExternalSystem) || !string.IsNullOrEmpty(deposit.ArdExternalId))
            {
                response.AddExternalSystemAndId(deposit.ArdExternalSystem, deposit.ArdExternalId);
            }
            return response;
        }

        #endregion

        /// <summary>
        /// Get a group of invoice payments by ID
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of invoice Payments</returns>
        public async Task<IEnumerable<InvoicePayment>> QueryInvoicePaymentsAsync(IEnumerable<string> invoiceIds, InvoiceDataSubset invoiceDataSubsetType)
        {
            if (invoiceIds == null || invoiceIds.Count() == 0)
            {
                throw new ArgumentNullException("invoiceIds", "At least one invoice ID must be specified.");
            }
            try
            {
                // Get the information about the AR.INVOICES
                Collection<ArInvoices> arInvoices = await DataReader.BulkReadRecordAsync<ArInvoices>(invoiceIds.ToArray());
                // Since this is a qapi we will not throw an error if not all are retrieved but will log this information.
                if (arInvoices == null || arInvoices.Count != invoiceIds.Count())
                {
                    logger.Info("ERROR: Failed to retrieve all invoices from the database.");
                    if (arInvoices != null)
                    {
                        logger.Info("Id count " + invoiceIds.Count() + " Invoices retrieved count " + arInvoices.Count());
                        var missingIds = invoiceIds.Except(arInvoices.Select(c => c.Recordkey)).Distinct().ToList();
                        logger.Info("   Missing Ids :" + string.Join(",", missingIds));
                    }
                }
                // Get AR.INVOICE.ITEMS for these invoices
                var arInvoiceItemsIds = arInvoices.Where(inv => inv.InvInvoiceItems != null && inv.InvInvoiceItems.Any()).SelectMany(ii => ii.InvInvoiceItems).Distinct().ToList();
                Collection<ArInvoiceItems> arInvoiceItems = new Collection<ArInvoiceItems>();
                Collection<ArCodeTaxGlDistr> taxGlDistributions = new Collection<ArCodeTaxGlDistr>();
                if (arInvoiceItemsIds != null && arInvoiceItemsIds.Any())
                {
                    arInvoiceItems = await DataReader.BulkReadRecordAsync<ArInvoiceItems>(arInvoiceItemsIds.ToArray());
                    // Get AR.CODE.TAX.GL.DISTRs for these ARItems.

                    if (arInvoiceItems != null && arInvoiceItems.Any())
                    {
                        var arCodeTaxGlDistrIds = arInvoiceItems.Where(invi => invi.InviArCodeTaxDistrs != null && invi.InviArCodeTaxDistrs.Any()).SelectMany(ii => ii.InviArCodeTaxDistrs).Distinct().ToList();
                        taxGlDistributions = await DataReader.BulkReadRecordAsync<ArCodeTaxGlDistr>(arCodeTaxGlDistrIds.ToArray());
                    }
                }
                // Get paid amounts for these invoices - if appropriate.
                List<InvoicePaymentItems> invoicePaymentItems = new List<InvoicePaymentItems>();
                if (invoiceDataSubsetType == InvoiceDataSubset.InvoicePayment)
                {
                    // Next retrieve the additional payment information for each invoice using Colleague TX
                    GetInvoicePaymentAmountsRequest paymentAmountsRequest = new GetInvoicePaymentAmountsRequest();
                    paymentAmountsRequest.InvoiceIds = invoiceIds.ToList();
                    GetInvoicePaymentAmountsResponse paymentAmountsResponse = await transactionInvoker.ExecuteAsync<GetInvoicePaymentAmountsRequest, GetInvoicePaymentAmountsResponse>(paymentAmountsRequest);
                    invoicePaymentItems = paymentAmountsResponse != null ? paymentAmountsResponse.InvoicePaymentItems : new List<InvoicePaymentItems>();
                }

                return BuildInvoicePayments(arInvoices, arInvoiceItems, taxGlDistributions, invoicePaymentItems);
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Timeout exception has occurred while requesting InvoicePayment objects.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An exception occurred while requesting InvoicePayment objects.");
                throw;
            }
        }

        #region private methods
        /// <summary>
        /// Build invoice payment entities from a Colleague AR invoice data contracts and InvoicePaymentItem Colleague TX results
        /// </summary>
        /// <param name="arInvoices">Collection of Colleague AR.INVOICES records</param>
        /// <param name="invoicePaymentItems">List that has amount paid for each invoice.</param>
        /// <returns>Invoice entity</returns>
        private IEnumerable<InvoicePayment> BuildInvoicePayments(Collection<ArInvoices> arInvoices, Collection<ArInvoiceItems> arInvoiceItems, Collection<ArCodeTaxGlDistr> taxDistributions, List<InvoicePaymentItems> invoicePaymentItems)
        {
            List<InvoicePayment> invoicePayments = new List<InvoicePayment>();
            foreach (var arInvoice in arInvoices)
            {
                try
                {
                    InvoicePaymentItems invoicePaymentItem = invoicePaymentItems.Where(ip => ip.InvoicePaymentId == arInvoice.Recordkey).FirstOrDefault();
                    decimal amountPaid = invoicePaymentItem == null || !invoicePaymentItem.InvoicePaymentAmount.HasValue ? 0 : invoicePaymentItem.InvoicePaymentAmount.Value;
                    decimal balanceAmount = invoicePaymentItem == null || !invoicePaymentItem.AlInvoiceBalanceAmount.HasValue ? 0 : invoicePaymentItem.AlInvoiceBalanceAmount.Value;
                    // Get the invoice items specific to this invoice
                    var specificInvoiceItems = arInvoiceItems.Where(aii => aii.InviInvoice == arInvoice.Recordkey);
                    var invoiceItems = specificInvoiceItems.Select(x => BuildCharge(x, taxDistributions)).ToList();
                    var invoicePayment = new InvoicePayment(arInvoice.Recordkey,
                        arInvoice.InvPersonId,
                        arInvoice.InvArType,
                        arInvoice.InvTerm,
                        arInvoice.InvNo,
                        arInvoice.InvDate.Value,
                        arInvoice.InvDueDate.Value,
                        arInvoice.InvBillingStartDate.Value,
                        arInvoice.InvBillingEndDate.Value,
                        arInvoice.InvDesc,
                        invoiceItems,
                        amountPaid, balanceAmount)
                    {
                        Archived = !String.IsNullOrEmpty(arInvoice.InvArchive),
                        AdjustmentToInvoice = arInvoice.InvAdjToInvoice
                    };
                    if (arInvoice.InvAdjByInvoices != null && arInvoice.InvAdjByInvoices.Count > 0)
                    {
                        foreach (var adj in arInvoice.InvAdjByInvoices)
                        {
                            invoicePayment.AddAdjustingInvoice(adj);
                        }
                    }
                    invoicePayments.Add(invoicePayment);
                }
                catch (Exception ex)
                {
                    // If unable to build all the invoices consider this a fatal error.

                    LogDataError("Invoice", arInvoice.Recordkey, arInvoice, ex);
                    throw new ApplicationException("Unable to build invoice for AR.INVOICE with ID " + arInvoice.Recordkey, ex);
                }
            }

            return invoicePayments;

        }

        /// <summary>
        /// Build a charge entity from a Colleague invoice item
        /// </summary>
        /// <param name="item">Colleague AR.INVOICE.ITEMS record</param>
        /// <returns>Charge entity</returns>
        private Charge BuildCharge(ArInvoiceItems item, Collection<ArCodeTaxGlDistr> taxDistributions)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "An invoice item cannot be null");
            }

            var charge = new Charge(item.Recordkey, item.InviInvoice, item.InviDesc.Split(DmiString._VM), item.InviArCode,
                item.InviExtChargeAmt.GetValueOrDefault() - item.InviExtCrAmt.GetValueOrDefault());

            // Calculate the total tax, if any
            decimal tax = 0;
            if (taxDistributions != null && taxDistributions.Any())
            {
                var specificTaxDistributions = taxDistributions.Where(t => t.ArctdArInvoiceItem == item.Recordkey).ToList();
                tax += specificTaxDistributions.Sum(taxItem => taxItem.ArctdGlTaxAmt.GetValueOrDefault());
            }
            charge.TaxAmount = tax;

            // Add any associated payment plan IDs to charges
            if (item.InviArPayPlans != null && item.InviArPayPlans.Count > 0)
            {
                foreach (var planId in item.InviArPayPlans)
                {
                    charge.AddPaymentPlan(planId);
                }
            }

            return charge;
        }

        private void ParseNames(string criteria, ref string lastName, ref string firstName, ref string middleName)
        {
            // Regular expression for all punctuation and numbers to remove from name string
            Regex regexNotPunc = new Regex(@"[!-&(-,.-@[-`{-~]");
            Regex regexNotSpace = new Regex(@"\s");

            var nameStrings = criteria.Split(',');
            // If there was a comma, set the first item to last name
            if (nameStrings.Count() > 1)
            {
                lastName = nameStrings.ElementAt(0).Trim();
                if (nameStrings.Count() >= 2)
                {
                    // parse the two items after the comma using a space. Ignore anything else
                    var nameStrings2 = nameStrings.ElementAt(1).Trim().Split(' ');
                    if (nameStrings2.Count() >= 1) { firstName = nameStrings2.ElementAt(0).Trim(); }
                    if (nameStrings2.Count() >= 2) { middleName = nameStrings2.ElementAt(1).Trim(); }
                }
            }
            else
            {
                // Parse entry using spaces, assume entered (last) or (first last) or (first middle last). 
                // Blank values don't hurt anything.
                nameStrings = criteria.Split(' ');
                switch (nameStrings.Count())
                {
                    case 1:
                        lastName = nameStrings.ElementAt(0).Trim();
                        break;
                    case 2:
                        firstName = nameStrings.ElementAt(0).Trim();
                        lastName = nameStrings.ElementAt(1).Trim();
                        break;
                    default:
                        firstName = nameStrings.ElementAt(0).Trim();
                        middleName = nameStrings.ElementAt(1).Trim();
                        lastName = nameStrings.ElementAt(2).Trim();
                        break;
                }
            }
            // Remove characters that won't make sense for each name part, including all punctuation and numbers 
            if (lastName != null)
            {
                lastName = regexNotPunc.Replace(lastName, "");
                lastName = regexNotSpace.Replace(lastName, "");
            }
            if (firstName != null)
            {
                firstName = regexNotPunc.Replace(firstName, "");
                firstName = regexNotSpace.Replace(firstName, "");
            }
            if (middleName != null)
            {
                middleName = regexNotPunc.Replace(middleName, "");
                middleName = regexNotSpace.Replace(middleName, "");
            }

        }

        #endregion
    }

}
