/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    public interface IOrganizationalChartDomainService
    {
        Task<IEnumerable<OrgChartEmployee>> GetOrganizationalChartEmployeesAsync(string rootEmployeeId);
        Task<OrgChartEmployee> GetOrganizationalChartEmployeeAsync(string rootEmployeeId);

    }
}
