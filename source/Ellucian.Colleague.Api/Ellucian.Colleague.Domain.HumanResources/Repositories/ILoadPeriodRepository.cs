/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface ILoadPeriodRepository
    {
        /// <summary>
        /// Gets a list of Load Periods for the given IDs
        /// </summary>
        /// <param name="ids">Load Period IDs</param>
        /// <returns>Returns a collection of Load Periods</returns>
        Task<IEnumerable<LoadPeriod>> GetLoadPeriodsByIdsAsync(IEnumerable<string> ids);
        /// <summary>
        /// Gets a list of all Load Periods
        /// </summary>
        /// <returns>Returns a collection of LoadPeriods</returns>
        Task<IEnumerable<LoadPeriod>> GetLoadPeriodsAsync();
    }
}
