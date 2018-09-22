/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayCycleRepository
    {
        Task<IEnumerable<PayCycle>> GetPayCyclesAsync();
    }
}
