//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPayrollDeductionArrangementService : IBaseService
    {
        /// <summary>
        /// Accept requests from external systems for new employee deductions in the authoritative HR system.
        /// </summary>
        /// <param name="offset">Page offset for paging.</param>
        /// <param name="limit">Page limit for paging.</param>
        /// <param name="bypassCache">Bypass use of cache and read directly from disk.</param>
        /// <param name="person">Person GUID filter.</param>
        /// <param name="contribution">Contribution ID filter.</param>
        /// <param name="deductionType">Deduction Type filter.</param>
        /// <param name="statusType">Status Type filter.</param>
        /// <returns>IEnumerable of Objects of type <see cref="Dtos.PayrollDeductionArrangements"/></returns>
        Task<Tuple<IEnumerable<Dtos.PayrollDeductionArrangements>, int>> GetPayrollDeductionArrangementsAsync(int offset, int limit, bool bypassCache = false,
            string person = "", string contribution = "", string deductionType = "", string status = "");
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangementsByGuidAsync(string id, bool bypassCache = false);
        
        /// <summary>
        /// Update a payroll deduction arrangement by guid
        /// </summary>
        /// <param name="id">guid for the payroll deduction arrangement</param>
        /// <returns>PayrollDeductionArrangement DTO Object</returns>
        Task<Dtos.PayrollDeductionArrangements> UpdatePayrollDeductionArrangementsAsync(string id, Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto);

        /// <summary>
        /// Create a new payroll deduction arrangement
        /// </summary>
        /// <param name="id">guid for the address</param>
        /// <returns>Addresses DTO Object</returns>
        Task<Dtos.PayrollDeductionArrangements> CreatePayrollDeductionArrangementsAsync(Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto);

        /// <summary>
        /// Gets all payroll-deduction-arrangements
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="person">Person GUID filter.</param>
        /// <param name="contribution">Contribution ID filter.</param>
        /// <param name="deductionType">Deduction Type filter.</param>
        /// <param name="statusType">Status Type filter.</param>
        /// <returns>Collection of <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PayrollDeductionArrangements>, int>> GetPayrollDeductionArrangements2Async(int offset, int limit, bool bypassCache = false, string person = "", string contribution = "", string deductionType = "", string status = "");

        /// <summary>
        /// Get a payrollDeductionArrangements by guid.
        /// </summary>
        /// <param name="id">Guid of the payrollDeductionArrangements in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see></returns>
        Task<Ellucian.Colleague.Dtos.PayrollDeductionArrangements> GetPayrollDeductionArrangements2ByIdAsync(string id, bool bypassCache = false);

        /// <summary>
        /// Update a payrollDeductionArrangements.
        /// </summary>
        /// <param name="payrollDeductionArrangements">The <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see></returns>
        Task<Ellucian.Colleague.Dtos.PayrollDeductionArrangements> UpdatePayrollDeductionArrangements2Async(string id, Ellucian.Colleague.Dtos.PayrollDeductionArrangements payrollDeductionArrangements);

        /// <summary>
        /// Create a payrollDeductionArrangements.
        /// </summary>
        /// <param name="payrollDeductionArrangements">The <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="PayrollDeductionArrangements">payrollDeductionArrangements</see></returns>
        Task<Ellucian.Colleague.Dtos.PayrollDeductionArrangements> CreatePayrollDeductionArrangements2Async(Ellucian.Colleague.Dtos.PayrollDeductionArrangements payrollDeductionArrangements);
    }
}
