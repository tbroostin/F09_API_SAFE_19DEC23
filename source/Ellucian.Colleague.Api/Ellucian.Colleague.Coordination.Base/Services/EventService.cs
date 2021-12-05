// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.DataTypes;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using System.Threading.Tasks;
using Ical.Net.Serialization;

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
        public Dtos.Base.EventsICal GetFacultyEvents(IEnumerable<string> facultyIds, DateTime? startDate, DateTime? endDate)
        {
            var events = eventRepository.Get("FI", facultyIds, startDate, endDate);
            Domain.Base.Entities.EventsICal eventsICalEntity = EventListToEventsICal(events);
            Dtos.Base.EventsICal eventsICalDto = new Dtos.Base.EventsICal(eventsICalEntity.iCal);
            return eventsICalDto;
        }

        private Domain.Base.Entities.EventsICal EventListToEventsICal(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Event> events)
        {
            var ICalendar = new Calendar();
            foreach (var anEvent in events)
            {
                ICalendar.Events.Add(new Ical.Net.CalendarComponents.CalendarEvent
                {
                    Name = "VEVENT",
                    Summary = anEvent.Description,
                    Categories = new List<string>() { anEvent.Type },
                    Location = anEvent.Location,
                    DtStart = new CalDateTime(anEvent.StartTime.UtcDateTime, DateTimeKind.Utc.ToString()) { HasTime = true },
                    DtEnd = new CalDateTime(anEvent.EndTime.UtcDateTime, DateTimeKind.Utc.ToString()) { HasTime = true }
                });
            }
            string result = null;
            var ctx = new SerializationContext();
            var factory = new SerializerFactory();
            var serializer = factory.Build(ICalendar.GetType(), ctx) as IStringSerializer;
            if (serializer != null)
            {
                result = serializer.SerializeToString(ICalendar);
            }
            return new Domain.Base.Entities.EventsICal(result);
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
