// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Accounts Receivable repository
    /// </summary>
    public interface IAccountsReceivableRepository : IPersonRepository
    {
        /// <summary>
        /// Account Types
        /// </summary>
        IEnumerable<ReceivableType> ReceivableTypes { get; }

        /// <summary>
        /// Deposit Types
        /// </summary>
        IEnumerable<DepositType> DepositTypes { get; }

        /// <summary>
        /// Charge Codes
        /// </summary>
        IEnumerable<ChargeCode> ChargeCodes { get; }

        /// <summary>
        /// List of charge codes
        /// </summary>
        Task<IEnumerable<ChargeCode>> GetChargeCodesAsync();

        /// <summary>
        /// Invoice Types
        /// </summary>
        IEnumerable<InvoiceType> InvoiceTypes { get; }

        /// <summary>
        /// External systems that feed into Accounts Receivable
        /// </summary>
        IEnumerable<ExternalSystem> ExternalSystems { get; }
            
        /// <summary>
        /// Get an account holder
        /// </summary>
        /// <param name="personId">Person ID of the account holder</param>
        /// <returns>The account holder</returns>
        AccountHolder GetAccountHolder(string personId);

        /// <summary>
        /// Retrieves the information for a dingle accountholder if an id is provided,
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
        Task<IEnumerable<AccountHolder>> SearchAccountHoldersByKeywordAsync(string criteria);

        /// <summary>
        /// Searches for account holders for the specified person ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>Set of FinancialAidPerson entities</returns>
        Task<IEnumerable<AccountHolder>> SearchAccountHoldersByIdsAsync(IEnumerable<string> ids);

        /// <summary>
        /// Get a specified invoice
        /// </summary>
        /// <param name="id">ID of the invoice</param>
        /// <returns>The invoice</returns>
        Invoice GetInvoice(string id);

        /// <summary>
        /// Get a collection of invoices
        /// </summary>
        /// <param name="ids">Collection of invoice IDs</param>
        /// <returns></returns>
        IEnumerable<Invoice> GetInvoices(IEnumerable<string> ids);

        /// <summary>
        /// Get a specified AR payment
        /// </summary>
        /// <param name="id">ID of the payment</param>
        /// <returns>The AR payment</returns>
        ReceivablePayment GetPayment(string id);

        /// <summary>
        /// Get a collection of AR payments
        /// </summary>
        /// <param name="ids">Collection of payment IDs</param>
        /// <returns>List of AR Payments</returns>
        IEnumerable<ReceivablePayment> GetPayments(IEnumerable<string> ids);

        /// <summary>
        /// Get the deposits due for an account holder
        /// </summary>
        /// <param name="personId">Person ID of the account holder</param>
        /// <returns>List of the deposits due</returns>
        List<DepositDue> GetDepositsDue(string personId);

        /// <summary>
        /// Get the distribution for a payment
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountType">AR Type of account (optional)</param>
        /// <param name="callingProcess">Code for process being called</param>
        /// <returns>Distribution code</returns>
        string GetDistribution(string studentId, string accountType, string callingProcess);

        /// <summary>
        /// Get the distribution for a student and a list of AR types
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountTypes">List of AR types</param>
        /// <param name="callingProcess">Code for payment process being used</param>
        /// <returns></returns>
        IEnumerable<string> GetDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess);

        /// <summary>
        /// Create a receivable invoice
        /// </summary>
        /// <param name="source">A ReceivableInvoice object with null invoice and charge ids</param>
        /// <returns>A ReceivableInvoice object reflecting a newly created invoice, with id fields populated</returns>
        ReceivableInvoice PostReceivableInvoice(ReceivableInvoice source);

        /// <summary>
        /// Get a receivable invoice
        /// </summary>
        /// <param name="id">Id of an existing invoice to retrieve</param>
        /// <returns>The indicated invoice</returns>
        ReceivableInvoice GetReceivableInvoice(string id);

        /// <summary>
        /// Get a deposit
        /// </summary>
        /// <param name="id">Id of the existing deposit to retrieve</param>
        /// <returns>The indicated deposit</returns>
        Deposit GetDeposit(string id);

        /// <summary>
        /// Get a list of deposits
        /// </summary>
        /// <param name="ids">Ids of the deposits to retrieve</param>
        /// <returns>The list of indicated deposits</returns>
        IEnumerable<Deposit> GetDeposits(IEnumerable<string> ids);

        /// <summary>
        /// Get a group of invoice payments by ID
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of invoice Payments</returns>
        Task<IEnumerable<InvoicePayment>> QueryInvoicePaymentsAsync(IEnumerable<string> invoiceIds, InvoiceDataSubset invoiceDataSubsetType);
    }
}
