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
    /// Interface containing repository methods for Employee Current Benefits Retrieval
    /// </summary>
    public interface ICurrentBenefitsRepository
    {
        /// <summary>
        /// Returns Employee Current Benefits Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <returns>Employee Benefits Domain Entity containing Current Benefits details. </returns>
        Task<EmployeeBenefits> GetEmployeeCurrentBenefitsAsync(string effectivePersonId);
    }
}
