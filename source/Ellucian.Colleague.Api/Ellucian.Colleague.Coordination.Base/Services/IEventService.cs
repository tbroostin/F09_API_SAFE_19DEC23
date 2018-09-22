// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services 
{
    /// <summary>
    /// Interface for event services
    /// </summary>
    public interface IEventService 
    {
        /// <summary>
        /// Gets the section events.
        /// </summary>
        /// <param name="sectionIds">The section ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        Ellucian.Colleague.Dtos.Base.EventsICal GetSectionEvents(IEnumerable<string> sectionIds, DateTime? start, DateTime? end);
        
        /// <summary>
        /// Gets the faculty events.
        /// </summary>
        /// <param name="facultyIds">The faculty ids.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        Ellucian.Colleague.Dtos.Base.EventsICal GetFacultyEvents(IEnumerable<string> facultyIds, DateTime? start, DateTime? end);


        /// <summary>
        /// Get all campus calendars
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Dtos.Base.CampusCalendar>> GetCampusCalendarsAsync();
    }
}
