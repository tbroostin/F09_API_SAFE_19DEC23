/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestLeaveBalanceConfigurationRepository : ILeaveBalanceConfigurationRepository
    {
        private LeaveBalanceConfiguration BuildLeaveBalanceConfigurationItems()
        {
            LeaveBalanceConfiguration leaveBalanceConfiguration = new LeaveBalanceConfiguration()
            {
                ExcludedLeavePlanIds = new List<string>()// { "INAC", "CMPS" }
            };
            return leaveBalanceConfiguration;
        }

        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            return await Task.FromResult(BuildLeaveBalanceConfigurationItems());
        }
    }
}
