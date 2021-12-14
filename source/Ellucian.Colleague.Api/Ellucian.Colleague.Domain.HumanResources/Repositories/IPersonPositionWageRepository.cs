/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonPositionWageRepository
    {
        /// <summary>
        /// Retrieves person position wages for specified person ids and starting with the specified
        /// date. If no date is specified, all available information is returned
        /// </summary>
        /// <param name="personIds">person ids</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null, 
            IEnumerable<string> payCycleIds = null);
    }
}
