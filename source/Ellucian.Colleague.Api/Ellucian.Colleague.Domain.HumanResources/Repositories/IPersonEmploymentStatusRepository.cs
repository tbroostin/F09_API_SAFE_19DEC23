/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPersonEmploymentStatusRepository
    {
        /// <summary>
        /// Retrieves person employment statuses for all specified person ids and beginning with the
        /// specified start date. If no date is specified, all available information is returned
        /// </summary>
        /// <param name="personIds">person ids</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null);
    }
}
