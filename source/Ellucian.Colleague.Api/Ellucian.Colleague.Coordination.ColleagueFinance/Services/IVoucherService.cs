// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a voucher service.
    /// </summary>
    public interface IVoucherService
    {
        /// <summary>
        /// Returns a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>A voucher DTO.</returns>
        [Obsolete("Obsolete as of 1.15. Use GetVoucher2Async.")]
        Task<Voucher> GetVoucherAsync(string voucherId);

        /// <summary>
        /// Returns a specified voucher.
        /// </summary>
        /// <param name="voucherId">ID of the requested voucher.</param>
        /// <returns>A voucher DTO.</returns>
        Task<Voucher2> GetVoucher2Async(string voucherId);

        /// <summary>
        /// Returns the list of voucher summary object for the user
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Voucher Summary DTOs</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.VoucherSummary>> GetVoucherSummariesAsync(string personId);
        
        /// <summary>
        /// Create/Update a voucher
        /// </summary>
        /// <param name="voucherCreateUpdateRequest"></param>
        /// <returns>The voucher create update response DTO</returns>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateResponse> CreateUpdateVoucherAsync(Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest);

        /// <summary>
        /// Gets a person payment address for voucher
        /// </summary>
        /// <returns> The payment address DTO</returns>
        Task<VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync();

        /// <summary>
        /// Void a voucher.
        /// </summary>
        /// <param name="voucherVoidRequest">The voucher void request DTO.</param>        
        /// <returns>The voucher void response DTO.</returns>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidResponse> VoidVoucherAsync(Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest voucherVoidRequest);
    }
}
