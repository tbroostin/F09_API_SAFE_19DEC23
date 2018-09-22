// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This is the definition of the methods that must be implemented
    /// for a recurring voucher service
    /// </summary>
    public interface IRecurringVoucherService
    {
        /// <summary>
        /// Returns a specified recurring voucher
        /// </summary>
        /// <param name="recurringVoucherId">The requested recurring voucher ID</param>
        /// <returns>A recurring voucher DTO.</returns>
        Task<RecurringVoucher> GetRecurringVoucherAsync(string recurringVoucherId);
    }
}
