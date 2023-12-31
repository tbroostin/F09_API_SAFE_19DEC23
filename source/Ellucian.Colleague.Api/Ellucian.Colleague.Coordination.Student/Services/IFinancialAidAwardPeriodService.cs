﻿//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IFinancialAidAwardPeriodService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.FinancialAidAwardPeriod> GetFinancialAidAwardPeriodByGuidAsync(string guid);
    }
}