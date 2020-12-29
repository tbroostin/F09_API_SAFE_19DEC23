/*Copyright 2016-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPayCycleService : IBaseService
    {
        /// <summary>
        /// Get all Pay Cycles
        /// </summary>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns></returns>
        Task<IEnumerable<PayCycle>> GetPayCyclesAsync(DateTime? lookbackDate = null);

        Task<IEnumerable<Ellucian.Colleague.Dtos.PayCycles>> GetPayCyclesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.PayCycles> GetPayCyclesByGuidAsync(string id);
    }
}
