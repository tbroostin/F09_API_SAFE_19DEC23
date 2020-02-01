// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IColleagueFinanceWebConfigurationsRepository
    {
        Task<ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurations();
        
    }
}
