/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IEmployeeLeaveTransactionsRepository
    {
        /// <summary>
        /// Get EmployeeLeaveTransactions objects for all EmployeeLeaveTransactions bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        
        /// <returns>Tuple of PerleaveDetails Entity objects <see cref="PerleaveDetails"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<PerleaveDetails>, int>> GetEmployeeLeaveTransactionsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get EmployeeLeaveTransactions objects for a specific Id.
        /// </summary>   
        /// <param name="id">guid of the EmployeeLeaveTransactions record.</param>
        /// <returns>PerleaveDetails Entity <see cref="PerleaveDetails"./></returns>
        Task<PerleaveDetails> GetEmployeeLeaveTransactionsByIdAsync(string id);

      
    }
}
