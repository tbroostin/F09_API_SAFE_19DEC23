//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PayPeriods services
    /// </summary>
    public interface IPayPeriodsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.PayPeriods>, int>> GetPayPeriodsAsync(int offset, int limit, string payCycle, string startOn, string endOn, bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.PayPeriods> GetPayPeriodsByGuidAsync(string id);
    }
}
