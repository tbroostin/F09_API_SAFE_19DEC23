/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for Current Benefits services
    /// </summary>
    public interface ICurrentBenefitsService: IBaseService
    {
        /// <summary>
        /// Returns Employee Current Benefits Details
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user</param>
        /// <returns>EmployeeBenefits DTO containing list of employee's current benefits. </returns>
        Task<EmployeeBenefits> GetEmployeesCurrentBenefitsAsync(string effectivePersonId);
    }
}
