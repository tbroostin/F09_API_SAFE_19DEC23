/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonPositionRepository
    {
        /// <summary>
        /// Retrieves person positions for specified person ids and start date.
        /// If not start date is specified, all available positions for specified persons
        /// are returned
        /// </summary>
        /// <param name="personIds">person ids</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null);
    }
}
