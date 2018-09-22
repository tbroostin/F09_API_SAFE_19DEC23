//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmployeeLeaveTransactions services
    /// </summary>
    public interface IEmployeeLeaveTransactionsService : IBaseService
    {
        /// <summary>
        /// Gets all employee-leave-transactions
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="EmployeeLeaveTransactions">EmployeeLeaveTransactions</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions>, int>> GetEmployeeLeaveTransactionsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a EmployeeLeaveTransactions by guid.
        /// </summary>
        /// <param name="guid">Guid of the EmployeeLeaveTransactions in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="EmployeeLeaveTransactions">EmployeeLeaveTransactions</see></returns>
        Task<Ellucian.Colleague.Dtos.EmployeeLeaveTransactions> GetEmployeeLeaveTransactionsByGuidAsync(string guid, bool bypassCache = false);


    }
}
