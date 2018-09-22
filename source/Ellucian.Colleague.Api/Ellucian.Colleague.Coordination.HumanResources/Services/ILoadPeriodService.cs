/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface ILoadPeriodService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.LoadPeriod>> GetLoadPeriodsByIdsAsync(IEnumerable<string> ids);
    }
}
