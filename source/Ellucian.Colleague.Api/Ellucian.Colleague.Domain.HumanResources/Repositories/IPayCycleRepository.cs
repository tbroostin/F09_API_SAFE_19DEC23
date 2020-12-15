/* Copyright 2020 Ellucian Company L.P. and its affiliates. */
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
        /// <summary>
        /// Get all PayCycle objects, built from database data
        /// </summary>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns></returns>
        Task<IEnumerable<PayCycle>> GetPayCyclesAsync(DateTime? lookbackDate = null);
    }
}
