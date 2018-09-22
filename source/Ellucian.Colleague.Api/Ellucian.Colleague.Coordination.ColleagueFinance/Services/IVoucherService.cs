// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System; 
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

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
    }
}
