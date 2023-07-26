/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IOrganizationalChartRepository
    {
        Task<IEnumerable<OrgChartNode>> GetActiveOrgChartEmployeesAsync(string rootEmployeeId);
        Task<OrgChartNode> GetActiveOrgChartEmployeeAsync(string rootEmployeeId);
    }
}
