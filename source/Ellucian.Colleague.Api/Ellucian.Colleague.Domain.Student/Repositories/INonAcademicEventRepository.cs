using Ellucian.Colleague.Domain.Student.Entities;
// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface INonAcademicEventRepository
    {
        /// <summary>
        /// Returns nonacademic events for a list of Ids
        /// </summary>
        /// <param name="eventIds">Ids requested</param>
        /// <returns>NonacademicEvents</returns>
        Task<IEnumerable<NonAcademicEvent>> GetEventsByIdsAsync(IEnumerable<string> eventIds);
    }
}
