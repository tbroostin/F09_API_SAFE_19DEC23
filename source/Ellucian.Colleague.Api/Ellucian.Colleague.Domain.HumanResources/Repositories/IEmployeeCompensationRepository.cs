/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Interface containing methods for Employee Compensation Retrieval 
    /// </summary>
     public interface IEmployeeCompensationRepository
    {
        /// <summary>
        /// Returns Employee Compensation Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <param name="salaryAmount">Estimated Annual Salary amount used in compensation re-calculation</param>
        /// <returns>Employee Compensation Domain Entity containing Benefit-Deductions,Taxes and Stipends. </returns>
        Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId , decimal? salaryAmount,bool isAdminView);
    }
}
