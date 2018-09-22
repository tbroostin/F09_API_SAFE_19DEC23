// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.iCal;
using DDay.iCal.Serialization;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Representation of an event service
    /// </summary>
    [RegisterType]
    public class EventService : BaseCoordinationService, IEventService 
    {
        private readonly IEventRepository eventRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="eventRepository">The event repository.</param>
        public EventService(IEventRepository eventRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IAdapterRegistry adapterRegistry,
            ILogger logger) 
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            
            this.eventRepository = eventRepository;
        }

        /// <summary>
        /// Gets the section events.
        /// </summary>
        /// <param name="sectionIds">The section ids.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public Ellucian.Colleague.Dtos.Base.EventsICal GetSectionEvents(IEnumerable<string> sectionIds, DateTime? startDate, DateTime? endDate)
        {
            var events = eventRepository.Get("CS", sectionIds, startDate, endDate);
            Ellucian.Colleague.Domain.Base.Entities.EventsICal eventsICalEntity = EventListToEventsICal(events);
            Ellucian.Colleague.Dtos.Base.EventsICal eventsICalDto = new Dtos.Base.EventsICal(eventsICalEntity.iCal);
            return eventsICalDto;
        }

        /// <summary>
        /// Gets the faculty events.
        /// </summary>
        /// <param name="facultyIds">The faculty ids.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public Ellucian.Colleague.Dtos.Base.EventsICal GetFacultyEvents(IEnumerable<string> facultyIds, DateTime? startDate, DateTime? endDate)
        {
            var events = eventRepository.Get("FI", facultyIds, startDate, endDate);
            Ellucian.Colleague.Domain.Base.Entities.EventsICal eventsICalEntity = EventListToEventsICal(events);
            Ellucian.Colleague.Dtos.Base.EventsICal eventsICalDto = new Dtos.Base.EventsICal(eventsICalEntity.iCal);
            return eventsICalDto;
        }

        private Ellucian.Colleague.Domain.Base.Entities.EventsICal EventListToEventsICal(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> events) {
            var iCal = new iCalendar();
            foreach (var anEvent in events) {
                DDay.iCal.Event evt = iCal.Create<DDay.iCal.Event>();
                evt.Name = "VEVENT";
                evt.Summary = anEvent.Description;
                evt.Categories = new List<string>() { anEvent.Type };
                evt.Location = anEvent.Location;
                evt.DTStart = new iCalDateTime(anEvent.StartTime.UtcDateTime);
                evt.DTStart.IsUniversalTime = true;
                evt.DTEnd = new iCalDateTime(anEvent.EndTime.UtcDateTime);
                evt.DTEnd.IsUniversalTime = true;
                //Retain the time component of DTStart and DTEnd. Fix bug in iCal which considers midnight UTC as all-day event and drops the time portion
                evt.DTStart.HasTime = true;
                evt.DTEnd.HasTime = true;
            }
            string result = null;
            var ctx = new SerializationContext();
            var factory = new DDay.iCal.Serialization.iCalendar.SerializerFactory();
            var serializer = factory.Build(iCal.GetType(), ctx) as IStringSerializer;
            if (serializer != null) {
                result = serializer.SerializeToString(iCal);
            }
            return new Ellucian.Colleague.Domain.Base.Entities.EventsICal(result);
        }

        public async Task<IEnumerable<Dtos.Base.CampusCalendar>> GetCampusCalendarsAsync()
        {
            //get calendars
            var campusCalendarEntities = await eventRepository.GetCalendarsAsync();
            if (campusCalendarEntities == null)
            {
                return new List<Dtos.Base.CampusCalendar>();
            }
            //get adapter
            var campusCalendarAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.CampusCalendar, Dtos.Base.CampusCalendar>();

            //map
            var dtos = campusCalendarEntities.Select(cc => campusCalendarAdapter.MapToType(cc));

            return dtos;
        }
    }
}
