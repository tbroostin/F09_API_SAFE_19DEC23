//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Data.Colleague;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPayPeriodsRepository
    {
       

        /// <summary>
        /// Get a collection of PayPeriod
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of PayPeriod</returns>
        Task<Tuple<IEnumerable<PayPeriod>, int>> GetPayPeriodsAsync(int offset, int limit, string payCycleCode = "", 
            string convertedStartOn = "", string convertedEndOn = "", bool bypassCache = false);
        
        /// <summary>
        /// Returns a review for a specified Pay Periods key.
        /// </summary>
        /// <param name="ids">Key to Pay Periods to be returned</param>
        /// <returns>PayPeriod Objects</returns>
        Task<PayPeriod> GetPayPeriodByIdAsync(string id);
    }
}
