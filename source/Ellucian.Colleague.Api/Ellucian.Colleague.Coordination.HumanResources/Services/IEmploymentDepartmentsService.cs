//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentDepartments services
    /// </summary>
    public interface IEmploymentDepartmentsService : IBaseService
    {
          
        Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentDepartments>> GetEmploymentDepartmentsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentDepartments> GetEmploymentDepartmentsByGuidAsync(string id);
    }
}
