//Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestColleagueFinanceWebConfigurationsRepository : IColleagueFinanceWebConfigurationsRepository
    {
        public Task<ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurations()
        {
            return Task.FromResult<ColleagueFinanceWebConfiguration>(new ColleagueFinanceWebConfiguration()
                {
                   DefaultEmailType ="PRI",
                   PurchasingDefaults= new PurchasingDefaults()
                   {
                       DefaultShipToCode= "MC"
                   }
                });

        }

    }
}
