﻿using Ellucian.Colleague.Coordination.Base.Services;
/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
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
        Task<IEnumerable<PayCycle>> GetPayCyclesAsync();

        Task<IEnumerable<Ellucian.Colleague.Dtos.PayCycles>> GetPayCyclesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.PayCycles> GetPayCyclesByGuidAsync(string id);
    }
}
