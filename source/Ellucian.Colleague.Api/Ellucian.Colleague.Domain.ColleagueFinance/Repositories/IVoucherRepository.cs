// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a voucher repository.
    /// </summary>
    public interface IVoucherRepository
    {
        /// <summary>
        /// Get a specific voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher ID.</param>
        /// <param name="personId">Person ID of user.</param>
        /// <param name="glAccessLevel">GL Access level of user.</param>
        /// <param name="glAccessAccounts">Set of GL Accounts to which the user has access.</param>
        /// <param name="versionNumber">Voucher API version number.</param>
        /// <returns>Voucher domain entity.</returns>
        Task<Voucher> GetVoucherAsync(string voucherId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts, int versionNumber);

        /// <summary>
        /// Get all vouchers requested for a given person.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>List of voucher domain entity list</returns>
        Task<IEnumerable<VoucherSummary>> GetVoucherSummariesByPersonIdAsync(string personId);

        /// <summary>
        /// Create a new voucher
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns>VoucherCreateUpdateResponse</returns>
        Task<VoucherCreateUpdateResponse> CreateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest);

        /// <summary>
        /// Get person payment address
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        Task<VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync(string personId);

        /// <summary>
        /// Update a voucher
        /// </summary>
        /// <param name="createUpdateRequest"></param>
        /// <returns>VoucherCreateUpdateResponse</returns>
        Task<VoucherCreateUpdateResponse> UpdateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest, Voucher originalVoucher);

        /// <summary>
        /// Void a voucher.
        /// </summary>
        /// <param name="voucherVoidRequest">The voucher void request DTO.</param>        
        /// <returns>The voucher void response DTO.</returns>
        Task<VoucherVoidResponse> VoidVoucherAsync(VoucherVoidRequest voucherVoidRequest);

        /// <summary>
        /// Get the list of voucher's by vendor id and invoice number.
        /// </summary>
        /// <param name="vendorId">Vendor Id</param>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>List of <see cref="Voucher2">Vouchers</see></returns> 
        Task<IEnumerable<Voucher>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo);

        /// <summary>
        /// Get Voucher summary list for the given user
        /// </summary>
        /// <param name="criteria">procurement filter criteria</param>      
        /// <returns>list of voucher summary domain entity objects</returns>
        Task<IEnumerable<VoucherSummary>> QueryVoucherSummariesAsync(ProcurementDocumentFilterCriteria criteria);

    }
}
