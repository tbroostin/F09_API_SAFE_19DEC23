// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestNonAcademicEventRepository : INonAcademicEventRepository
    {
        public List<NonAcademicEvent> GetEventsByIdsTest(IEnumerable<string> eventIds)
        {
            List<NonAcademicEvent> events = new List<NonAcademicEvent>();

            foreach (var item in eventIds)
            {
                NonAcademicEvent eventEntity = new NonAcademicEvent(item, "Spring", ("Title of " + item));
                eventEntity.BuildingCode = "Building Code" + item;
                eventEntity.Date = new DateTime(2029, 1, 1);
                eventEntity.EndTime = new DateTimeOffset(new DateTime(2029, 1, 1, 10, 0, 0, DateTimeKind.Local));
                eventEntity.EventTypeCode = "Event Type";
                eventEntity.LocationCode = "Location Code";
                eventEntity.RoomCode = "Room Code";
                eventEntity.StartTime = new DateTimeOffset(new DateTime(2029, 1, 1, 9, 0, 0, DateTimeKind.Local));
                eventEntity.Venue = "Venue";
                eventEntity.Description = "Description";

                events.Add(eventEntity);
            }

            return events;
        }

        public async Task<IEnumerable<NonAcademicEvent>> GetEventsByIdsAsync(IEnumerable<string> eventIds)
        {
            List<NonAcademicEvent> events = new List<NonAcademicEvent>();

            foreach (var item in eventIds)
            {
                NonAcademicEvent eventEntity = new NonAcademicEvent(item, "Spring", ("Title of " + item));
                // Note: Start and end times are time of day times and currently events cannot cross multiple days.
                // That is why there is only one date on the contract.
                eventEntity.BuildingCode = "Building Code";
                eventEntity.Date = new DateTime(2029, 1, 1);
                eventEntity.EndTime = new DateTimeOffset(new DateTime(2029, 1, 1, 10, 0, 0));
                eventEntity.EventTypeCode = "Event Type";
                eventEntity.LocationCode = "Location Code";
                eventEntity.RoomCode = "Room Code";
                eventEntity.StartTime = new DateTimeOffset(new DateTime(2029, 1, 1, 9, 0, 0));
                eventEntity.Venue = "Venue";
                eventEntity.Description = "Description";

                events.Add(eventEntity);
            }

            return await Task.FromResult(events);
        }

        Task<IEnumerable<NonAcademicEvent>> INonAcademicEventRepository.GetEventsByIdsAsync(IEnumerable<string> eventIds)
        {
            throw new NotImplementedException();
        }
    }
}
