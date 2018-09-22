/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayrollDeductionArrangementRepository
    {
        /// <summary>
        /// Get the Host Country code from the INTL form parameter
        /// </summary>
        /// <returns>Returns a string with the host couuntry of USA or CANADA</returns>
        Task<string> GetHostCountryAsync();

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetIdFromGuidAsync(string guid);

        /// <summary>
        /// Get PayrollDeductionArrangement objects.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <param name="bypassCache">Bypass the cache and read directly from disk for all reads.</param>
        /// <param name="person">Employee for which this deduction applies.</param>
        /// <param name="contribution">Contribution reference from other system.</param>
        /// <param name="deductionType">Deduction Code used to identify the paroll deduction arrangement</param>
        /// <param name="statusType"></param>
        /// <returns>Tuple of Employee Entity objects <see cref="PayrollDeductionArrangements"/> and a count for paging.</returns>
        Task<Tuple<IEnumerable<PayrollDeductionArrangements>, int>> GetAsync(int offset, int limit, bool bypassCache = false, 
            string person = "", string contribution = "", string deductionType = "", string status = "");

        /// <summary>
        /// Get PayrollDeductionArrangement objects for all payroll deduction arrangements.
        /// </summary>   
        /// <param name="id">guid of the employees record.</param>
        /// <returns>PayrollDeductionArrangement Entity <see cref="PayrollDeductionArrangements"./></returns>
        Task<PayrollDeductionArrangements> GetByIdAsync(string id);
        
        /// <summary>
        /// Update an existing PERBEN record for an employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="payrollDeductionArrangement">Payroll Deduction Arrangement object</param>
        /// <returns>PayrollDeductionArrangement object <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/></returns>
        Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> UpdateAsync(string id, Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangement);
        
        /// <summary>
        /// Create a new PERBEN record for an employee
        /// </summary>
        /// <param name="payrollDeductionArrangement">Payroll Deduction Arrangement object</param>
        /// <returns>PayrollDeductionArrangement object <see cref="Domain.HumanResources.Entities.PayrollDeductionArrangements"/></returns>
        Task<Domain.HumanResources.Entities.PayrollDeductionArrangements> CreateAsync(Domain.HumanResources.Entities.PayrollDeductionArrangements payrollDeductionArrangement);
    }
}
