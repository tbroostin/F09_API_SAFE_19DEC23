// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Finance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IAccountsReceivableService 
    {
        /// <summary>
        /// Get all receivable types
        /// </summary>
        /// <returns>List of receivable types</returns>
        IEnumerable<ReceivableType> GetReceivableTypes();

        /// <summary>
        /// Get all deposit types
        /// </summary>
        /// <returns>List of deposit types</returns>
        IEnumerable<DepositType> GetDepositTypes();

        /// <summary>
        /// Get an accountholder by ID
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>The accountholder</returns>
        AccountHolder GetAccountHolder(string id);

        /// <summary>
        /// Get a privacy-restricted accountholder by ID
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>The accountholder</returns>
        PrivacyWrapper<AccountHolder> GetAccountHolder2(string id);

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
        Task<IEnumerable<Dtos.Finance.AccountHolder>> SearchAccountHoldersAsync(string id);

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
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
        /// <param name="criteria">a query keyword such as a Person ID or a first and last name.  
        /// A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        [Obsolete("Obsolete as of API version 1.16. Use SearchAccountHolders3Async.")]
        Task<PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>>> SearchAccountHoldersAsync2(string criteria);

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
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
        /// <param name="criteria">either a list of person ids or a query keyword such as 
        /// a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        Task<PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>>> SearchAccountHolders3Async(AccountHolderQueryCriteria criteria);

        /// <summary>
        /// Get an invoice by ID
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>The invoice</returns>
        Invoice GetInvoice(string invoiceId);

        /// <summary>
        /// Get a list of invoices by ID
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of invoices</returns>
        [Obsolete("Obsolete as of API version 1.12, use QueryInvoicesAsync instead.")]
        IEnumerable<Invoice> GetInvoices(IEnumerable<string> invoiceIds);

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>The payment</returns>
        ReceivablePayment GetPayment(string paymentId);

        /// <summary>
        /// Get a list of payments by ID
        /// </summary>
        /// <param name="paymentIds">List of payment IDs</param>
        /// <returns>List of payments</returns>
        IEnumerable<ReceivablePayment> GetPayments(IEnumerable<string> paymentIds);

        /// <summary>
        /// Get the deposits due for a person
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>List of deposits due</returns>
        IEnumerable<DepositDue> GetDepositsDue(string id);

        /// <summary>
        /// Get the distributions for a set of accounts
        /// </summary>
        /// <param name="studentId">Person ID</param>
        /// <param name="accountTypes">List of receivable types</param>
        /// <param name="paymentProcess">ID of the payment process</param>
        /// <returns>List of distribution codes</returns>
        IEnumerable<string> GetDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess);

        /// <summary>
        /// Create a receivable invoice
        /// </summary>
        /// <param name="source">Invoice to create</param>
        /// <returns></returns>
        ReceivableInvoice PostReceivableInvoice(Ellucian.Colleague.Dtos.Finance.ReceivableInvoice source);

        /// <summary>
        /// Retrieve a deposit
        /// </summary>
        /// <param name="depositId">Identifier of the deposit to retrieve</param>
        /// <returns>The desired deposit</returns>
        Dtos.Finance.Deposit GetDeposit(string depositId);

        /// <summary>
        /// Get a list of deposits
        /// </summary>
        /// <param name="ids">The list of ids of the deposits to retrieve</param>
        /// <returns>The list of retrieved deposits</returns>
        IEnumerable<Dtos.Finance.Deposit> GetDeposits(IEnumerable<string> ids);

        /// <summary>
        /// Get a group of invoice from a list of invoice IDs asyncronously
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of Invoice DTOs</returns>
        Task<IEnumerable<Dtos.Finance.Invoice>> QueryInvoicesAsync(IEnumerable<string> invoiceIds);

        /// <summary>
        /// Get a group of invoice payments from a list of invoice IDs
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of InvoicePayment DTOs</returns>
        Task<IEnumerable<Dtos.Finance.InvoicePayment>> QueryInvoicePaymentsAsync(IEnumerable<string> invoiceIds);

        /// <summary>
        /// Get all charge codes
        /// </summary>
        /// <returns>List of charge codes</returns>
        Task<IEnumerable<ChargeCode>> GetChargeCodesAsync();
    }
}
