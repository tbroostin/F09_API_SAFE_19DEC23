// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
   public interface IColleagueFinanceWebConfigurationsService
    {
        /// <summary>
        /// Gets all CF Web Configuration parameters
        /// </summary>
        Task<Ellucian.Colleague.Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurationsAsync();
    }
}
