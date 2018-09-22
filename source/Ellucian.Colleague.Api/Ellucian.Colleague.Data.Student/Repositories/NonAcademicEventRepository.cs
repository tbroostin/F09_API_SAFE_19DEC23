// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class NonAcademicEventRepository : BaseColleagueRepository, INonAcademicEventRepository
    {
        private string colleagueTimeZone;

        public NonAcademicEventRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apisettings) : 
            base(cacheProvider, transactionFactory, logger)
        {
            colleagueTimeZone = apisettings.ColleagueTimeZone;
        }
        /// <summary>
        /// Returns nonacademic events for a list of Ids
        /// </summary>
        /// <param name="eventIds">Ids requested</param>
        /// <returns>NonAcademicEvents</returns>
        public async Task<IEnumerable<NonAcademicEvent>> GetEventsByIdsAsync(IEnumerable<string> eventIds)
        {

            var nonacademicEventResult = new List<NonAcademicEvent>();
            if (eventIds == null || !eventIds.Any())
            {
                return nonacademicEventResult;
            }
            logger.Info("In NonAcademicEventRepository.GetEventsByIdsAsync for Ids " + string.Join(", ", eventIds));
            Collection<NaaEvents> eventData = await DataReader.BulkReadRecordAsync<NaaEvents>(eventIds.ToArray());
            if (eventData == null)
            {
                throw new ApplicationException("Unable to read NAA.EVENTS for IDs: " + string.Join(", ", eventIds));
            }
            return BuildEvents(eventData.ToList());
        }

        private List<NonAcademicEvent> BuildEvents(List<NaaEvents> eventData)
        {
            // Get all event details
            List<NonAcademicEvent> eventList = new List<NonAcademicEvent>();
            if (eventData.Any())
            {
                foreach (var eventContract in eventData)
                {
                    try
                    {
                        NonAcademicEvent eventItem = new NonAcademicEvent(eventContract.Recordkey, eventContract.NaaevTerm, eventContract.NaaevTitle);
                        eventItem.LocationCode = eventContract.NaaevLocation;
                        eventItem.RoomCode = eventContract.NaaevRoom;
                        eventItem.StartTime = eventContract.NaaevStartTime.HasValue ? eventContract.NaaevStartTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                        eventItem.Venue = eventContract.NaaevVenue;
                        eventItem.BuildingCode = eventContract.NaaevBuilding;
                        eventItem.Date = eventContract.NaaevStartDate;
                        eventItem.EndTime = eventContract.NaaevEndTime.HasValue ? eventContract.NaaevEndTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                        eventItem.EventTypeCode = eventContract.NaaevType;
                        eventItem.Description = eventContract.NaaevDescription;
                        
                        eventList.Add(eventItem);
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Event", eventContract.Recordkey, eventData, ex);
                    }
                }
                logger.Info("Count of NonAcademicEvent entities retreived is " + eventList.Count());
            }
            return eventList;
        }
    }
}
