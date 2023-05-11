// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Data.Colleague;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Data.Colleague.DataContracts;
using slf4net;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using System.Diagnostics;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Repository for events
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EventRepository : BaseColleagueRepository, IEventRepository
    {
        private ApplValcodes eventTypes;
        private DataContracts.IntlParams internationalParameters;
        private readonly string _colleagueTimeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRepository"/> class.
        /// </summary>
        /// <param name="settings">API settings</param>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public EventRepository(ApiSettings settings, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = settings.ColleagueTimeZone;
            CacheTimeout = 60;
        }

        /// <summary>
        /// Gets the specified event identifier.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">id;Calendar Schedule ID may not be null or empty</exception>
        public Event Get(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException("id", "Calendar Schedule ID may not be null or empty");
            }
            CalendarSchedules calsched = DataReader.ReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", eventId);
            var eventsList = BuildEvents(new Collection<CalendarSchedules>() { calsched });
            return eventsList.Where(s => s.Id == eventId).FirstOrDefault();
        }

        /// <summary>
        /// Gets the specified calendar schedule type.
        /// </summary>
        /// <param name="calendarScheduleType">Type of the calendar schedule.</param>
        /// <param name="calendarSchedulePointers">The calendar schedule pointers.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// calendarScheduleType;Calendar Schedule Type may not be null or empty
        /// or
        /// calendarSchedulePointers;Calendar Schedule Associated Record Pointers may not be null
        /// </exception>
        /// <exception cref="System.ArgumentException">At least one Calendar Schedule Pointer to an Associated Record is required</exception>
        public IEnumerable<Event> Get(string calendarScheduleType, IEnumerable<string> calendarSchedulePointers, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(calendarScheduleType))
            {
                throw new ArgumentNullException("calendarScheduleType", "Calendar Schedule Type may not be null or empty");
            }
            if (calendarSchedulePointers == null)
            {
                throw new ArgumentNullException("calendarSchedulePointers", "Calendar Schedule Associated Record Pointers may not be null");
            }
            else
            {
                if (calendarSchedulePointers.Count() < 1)
                {
                    throw new ArgumentException("At least one Calendar Schedule Pointer to an Associated Record is required");
                }
            }
            if (startDate.HasValue || endDate.HasValue)
            {
                internationalParameters = GetInternationalParameters();
            }
            string startDatePart = null;
            string endDatePart = null;
            if (startDate.HasValue)
            {
                startDatePart = string.Format("AND WITH CALS.DATE GE '{0}'", UniDataFormatter.UnidataFormatDate(startDate.Value, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter));
            }
            if (endDate.HasValue)
            {
                endDatePart = string.Format("AND WITH CALS.DATE LE '{0}'", UniDataFormatter.UnidataFormatDate(endDate.Value, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter));
            }
            Collection<CalendarSchedules> calsData = new Collection<CalendarSchedules>();
            foreach (var ptr in calendarSchedulePointers)
            {
                string criteria = null;
                if (startDatePart != null && endDatePart != null)
                {
                    criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} {3} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, ptr, startDatePart, endDatePart);
                }
                else if (startDatePart != null)
                {
                    criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, ptr, startDatePart);
                }
                else if (endDatePart != null)
                {
                    criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, ptr, endDatePart);
                }
                else
                {
                    criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, ptr);
                }
                Collection<CalendarSchedules> cals = DataReader.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", criteria);
                foreach (var cal in cals)
                {
                    calsData.Add(cal);
                }
            }
            return BuildEvents(calsData);
        }

        /// <summary>
        /// Gets a campus calendar
        /// </summary>
        /// <returns>Campus calendar</returns>
        public Ellucian.Colleague.Domain.Base.Entities.CampusCalendar GetCalendar(string calendarId)
        {
            if (string.IsNullOrEmpty(calendarId))
            {
                throw new ArgumentNullException(calendarId);
            }
            return GetOrAddToCache<Ellucian.Colleague.Domain.Base.Entities.CampusCalendar>("CampusCalendar_" + calendarId,
                () => 
                {
                    Domain.Base.Entities.CampusCalendar calendar = null;
                    DataContracts.CampusCalendar calendarData = DataReader.ReadRecord<DataContracts.CampusCalendar>(calendarId);
                    if (calendarData == null)
                    {
                        throw new KeyNotFoundException("Calendar record not found for ID " + calendarId);
                    }

                    var calendarScheduleIds = calendarData.CmpcSchedules == null ? new string[0] : calendarData.CmpcSchedules.ToArray();
                    var calendarScheduleData = DataReader.BulkReadRecord<DataContracts.CalendarSchedules>(calendarScheduleIds);

                    var specialDayIds = calendarData.CmpcSpecialDays == null ? new string[0] : calendarData.CmpcSpecialDays.ToArray();
                    var specialDayData = DataReader.BulkReadRecord<DataContracts.CampusSpecialDay>(specialDayIds);

                    var calendarDayTypesValcode = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES");

                    try
                    {
                        calendar = BuildCampusCalendar(calendarData, specialDayData, calendarScheduleData, calendarDayTypesValcode);                        
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Campus Calendar", calendarId, calendar, ex);
                    }

                    return calendar;
                });
        }

        /// <summary>
        /// Get all calendars
        /// </summary>
        /// <returns>An enumerable of CampusCalendar objects</returns>
        public async Task<IEnumerable<Domain.Base.Entities.CampusCalendar>> GetCalendarsAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<Domain.Base.Entities.CampusCalendar>>("AllCampusCalendars", async () =>
            {
                var campusCalendarRecords = await DataReader.BulkReadRecordAsync<DataContracts.CampusCalendar>("");
                if (campusCalendarRecords == null || !campusCalendarRecords.Any())
                {
                    logger.Error("No Campus.Calendar records retrieved from the database");
                    return new List<Domain.Base.Entities.CampusCalendar>();
                }

                var calendarScheduleIds = campusCalendarRecords.SelectMany(cc => cc.CmpcSchedules).ToArray();
                var calendarScheduleRecords = await DataReader.BulkReadRecordAsync<DataContracts.CalendarSchedules>(calendarScheduleIds);

                var specialDayIds = campusCalendarRecords.SelectMany(cc => cc.CmpcSpecialDays).ToArray();
                var specialDayRecords = await DataReader.BulkReadRecordAsync<DataContracts.CampusSpecialDay>(specialDayIds);

                var calendarDayTypesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES");

                var campusCalendarEntities = new List<Domain.Base.Entities.CampusCalendar>();
                foreach (var campusCalendarRecord in campusCalendarRecords)
                {
                    try
                    {
                        var campusCalendarEntity = BuildCampusCalendar(campusCalendarRecord, specialDayRecords, calendarScheduleRecords, calendarDayTypesValcode);
                        campusCalendarEntities.Add(campusCalendarEntity);

                    }
                    catch (Exception e)
                    {
                        LogDataError("CAMPUS.CALENDAR", campusCalendarRecord.Recordkey, campusCalendarRecord, e, "Unable to build CampusCalendar from CampusCalendarRecord");
                    }
                }

                return campusCalendarEntities;
            }, Level1CacheTimeoutValue);
        }



        /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="meetingDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="building">Building code used to filter rooms</param>
        /// <param name="allBuildingsFromLocation">Collection of building codes from a location used as a filter</param>
        /// <returns>Collection of room IDs</returns>
        public IEnumerable<string> GetRoomIdsWithConflicts(DateTimeOffset startTime, DateTimeOffset endTime, IEnumerable<DateTime> meetingDates, string building = "", IEnumerable<string> allBuildingsFromLocation = null)
        {
            if (meetingDates == null || meetingDates.Count() == 0)
            {
                throw new ArgumentNullException("meetingDates", "A list of meeting dates must be provided.");
            }

            if (DateTimeOffset.Compare(startTime, endTime) > 0)
            {
                throw new ArgumentOutOfRangeException("Start time cannot be later than end time");
            }

            meetingDates = meetingDates.OrderBy(o => o.Date).ToList();
            var startDate = meetingDates.First();
            var endDate = meetingDates.Last();

            startTime = startTime.ToLocalDateTime(_colleagueTimeZone);
            endTime = endTime.ToLocalDateTime(_colleagueTimeZone);

            var internationalParameters = GetInternationalParameters();

            string[] buildingsSitesIds = null;
            // Filter for building

            if (building != string.Empty)
            {
                var selectBuilding = string.Format("WITH CALS.BUILDINGS EQ '{0}'", building);
                buildingsSitesIds = DataReader.Select("CALENDAR.SCHEDULES", selectBuilding);
            }
            else if ((allBuildingsFromLocation != null) && (allBuildingsFromLocation.Count() > 0))
                buildingsSitesIds = DataReader.Select("CALENDAR.SCHEDULES", "WITH CALS.BUILDINGS EQ '?'", allBuildingsFromLocation.ToArray());


            // check for potential calendar schedules conflicts
            var roomsWithConflicts = new List<string>();
            var potentialConflicts = new Collection<CalendarSchedules>();
            var selectBetweenStartEndDate = string.Format("WITH CALS.DATE GE '{0}' AND CALS.DATE LE '{1}'",
                UniDataFormatter.UnidataFormatDate(startDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter),
                UniDataFormatter.UnidataFormatDate(endDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter));

            string[] potentialConflictIds = null;
            if ((buildingsSitesIds == null) || (buildingsSitesIds.Count() == 0))
                potentialConflictIds = DataReader.Select("CALENDAR.SCHEDULES", selectBetweenStartEndDate);
            else
                potentialConflictIds = DataReader.Select("CALENDAR.SCHEDULES", buildingsSitesIds, selectBetweenStartEndDate);

            if (potentialConflictIds == null || potentialConflictIds.Count() == 0)
            {
                return roomsWithConflicts;
            }
            else
            {
                // splitting these next two selects and unioning the results based on an issue in UniData where MIOSEL would fail to
                // parse the query correctly when both conditions were included with an OR condition and parenthesis 

                var selectOutsideStartEndTime = string.Format("WITH CALS.BLDG.ROOM.IDX NE '' AND CALS.START.TIME LE '{0}' AND CALS.END.TIME GT '{0}'",
                    startTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                var outsideStartEndTimeResults = DataReader.Select("CALENDAR.SCHEDULES", potentialConflictIds, selectOutsideStartEndTime);
                if (outsideStartEndTimeResults == null) outsideStartEndTimeResults = new string[] { };

                var selectBetweenStartEndTime = string.Format("WITH CALS.BLDG.ROOM.IDX NE '' AND CALS.START.TIME GT '{0}' AND CALS.START.TIME LT '{1}'",
                    startTime.TimeOfDay.ToString(@"hh\:mm\:ss"),
                    endTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                var betweenStartEndTimeResults = DataReader.Select("CALENDAR.SCHEDULES", potentialConflictIds, selectBetweenStartEndTime);
                if (betweenStartEndTimeResults == null) betweenStartEndTimeResults = new string[] { };

                // combine the results and read the potential conflicting calendar schedules
                var potentialSchedConflicts = outsideStartEndTimeResults.Union(betweenStartEndTimeResults).ToList();
                if (potentialSchedConflicts != null && potentialSchedConflicts.Count() > 0)
                {
                    potentialConflicts = DataReader.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES", potentialSchedConflicts.ToArray());
                }
            }

            if (potentialConflicts == null || potentialConflicts.Count == 0)
            {
                return roomsWithConflicts;
            }

            var conflicts = potentialConflicts.Where(x => meetingDates.Contains(x.CalsDate.Value)).ToList();
            if (conflicts == null || conflicts.Count == 0)
            {
                return roomsWithConflicts;
            }

            foreach (var conflict in conflicts)
            {
                if (conflict.CalsBldgRoomEntityAssociation == null || conflict.CalsBldgRoomEntityAssociation.Count == 0)
                {
                    break;
                }
                foreach (var room in conflict.CalsBldgRoomEntityAssociation)
                {
                    roomsWithConflicts.Add(room.CalsBuildingsAssocMember + "*" + room.CalsRoomsAssocMember);
                }
            }
            roomsWithConflicts = roomsWithConflicts.Distinct().ToList();
            return roomsWithConflicts;
        }

        /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="meetingDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="rooms">Collection of filtered rooms</param>     
        /// <returns>Collection of room IDs</returns>
        public IEnumerable<string> GetRoomIdsWithConflicts2(DateTimeOffset startTime, DateTimeOffset endTime,
            IEnumerable<DateTime> meetingDates, IEnumerable<string> rooms)
        {

            if (meetingDates == null || !meetingDates.Any())
            {
                throw new ArgumentNullException("meetingDates", "A list of meeting dates must be provided.");
            }

            if (DateTimeOffset.Compare(startTime, endTime) > 0)
            {
                throw new ArgumentOutOfRangeException("Start time cannot be later than end time");
            }

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
            }

            meetingDates = meetingDates.OrderBy(o => o.Date).ToList();
            var startDate = meetingDates.First();
            var endDate = meetingDates.Last();

            startTime = startTime.ToLocalDateTime(_colleagueTimeZone);
            endTime = endTime.ToLocalDateTime(_colleagueTimeZone);

            var internationalParameters = GetInternationalParameters();

            string[] buildingsSitesIds = null;

            if ((rooms != null) && (rooms.Any()))
            {

                if (logger.IsInfoEnabled)
                {
                    logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Filter by rooms started");
                    watch.Restart();
                }

                buildingsSitesIds = DataReader.Select("CALENDAR.SCHEDULES", "WITH CALS.BLDG.ROOM.IDX EQ  '?'",
                    rooms.ToArray());

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Filter by rooms completed in " +
                                watch.ElapsedMilliseconds.ToString() + " ms");
                }
                if (logger.IsDebugEnabled)
                {
                    logger.Info(
                        "Event Query: (GetRoomIdsWithConflicts2) _CALENDAR.SCHEDULES WITH CALS.BLDG.ROOM.IDX EQ " +
                        string.Join(" ", rooms));
                }
            }
            // check for potential calendar schedules conflicts
            var roomsWithConflicts = new List<string>();
            var potentialConflicts = new Collection<CalendarSchedules>();
            string[] potentialConflictIds = null;

            var selectBetweenStartEndDate = string.Format("WITH CALS.DATE GE '{0}' AND CALS.DATE LE '{1}'",
                UniDataFormatter.UnidataFormatDate(startDate, internationalParameters.HostShortDateFormat,
                    internationalParameters.HostDateDelimiter),
                UniDataFormatter.UnidataFormatDate(endDate, internationalParameters.HostShortDateFormat,
                    internationalParameters.HostDateDelimiter));

            if (logger.IsInfoEnabled)
            {
                logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Filter by dates started");
                watch.Restart();
            }


            if ((buildingsSitesIds == null) || (!buildingsSitesIds.Any()))
                potentialConflictIds = DataReader.Select("CALENDAR.SCHEDULES", selectBetweenStartEndDate);
            else
                potentialConflictIds = DataReader.Select("CALENDAR.SCHEDULES", buildingsSitesIds,
                    selectBetweenStartEndDate);

            if (logger.IsInfoEnabled)
            {
                watch.Stop();
                logger.Info("Event Timing: (GetRoomIdsWithConflicts2) __Filter by dates completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }
            if (logger.IsDebugEnabled)
            {
                logger.Info("Event Query: (GetRoomIdsWithConflicts2) _CALENDAR.SCHEDULES " + selectBetweenStartEndDate);
            }

            if (potentialConflictIds == null || !potentialConflictIds.Any())
            {
                return roomsWithConflicts;
            }
            else
            {
                if (logger.IsInfoEnabled)
                {
                    logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Filter by time started");
                    watch.Restart();
                }

                var selectBetweenStartEndTime =
                    string.Format(
                        "WITH CALS.START.TIME GE '{0}' AND CALS.END.TIME LE '{1}' OR CALS.END.TIME GE '{0}' AND CALS.END.TIME LE '{1}'",
                        startTime.TimeOfDay.ToString(@"hh\:mm\:ss"),
                        endTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                var betweenStartEndTimeResults = DataReader.Select("CALENDAR.SCHEDULES", potentialConflictIds,
                    selectBetweenStartEndTime) ?? new string[] { };
                var potentialSchedConflicts = betweenStartEndTimeResults.Distinct().ToList();

                if (potentialSchedConflicts != null && potentialSchedConflicts.Any())
                {
                    potentialConflicts = DataReader.BulkReadRecord<CalendarSchedules>("CALENDAR.SCHEDULES",
                        potentialSchedConflicts.ToArray());
                }

                if (logger.IsInfoEnabled)
                {
                    watch.Stop();
                    logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Filter by time completed in " +
                                watch.ElapsedMilliseconds.ToString() + " ms");
                }
                if (logger.IsDebugEnabled)
                {
                    logger.Info("Event Query: (GetRoomIdsWithConflicts2) _CALENDAR.SCHEDULES " +
                                selectBetweenStartEndTime);
                }
            }

            if (potentialConflicts == null || !potentialConflicts.Any())
            {
                return roomsWithConflicts;
            }

            var conflicts = potentialConflicts.Where(x => meetingDates.Contains(x.CalsDate.Value)).ToList();
            if (conflicts == null || conflicts.Count == 0)
            {
                return roomsWithConflicts;
            }

            if (logger.IsInfoEnabled)
            {
                logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Sort conflicts started");
                watch.Restart();
            }

            foreach (var conflict in conflicts)
            {
                if (conflict.CalsBldgRoomEntityAssociation == null || conflict.CalsBldgRoomEntityAssociation.Count == 0)
                {
                    break;
                }
                foreach (var room in conflict.CalsBldgRoomEntityAssociation)
                {
                    roomsWithConflicts.Add(room.CalsBuildingsAssocMember + "*" + room.CalsRoomsAssocMember);
                }
            }
            roomsWithConflicts = roomsWithConflicts.Distinct().ToList();

            if (!logger.IsInfoEnabled) return roomsWithConflicts;
            watch.Stop();
            logger.Info("Event Timing: (GetRoomIdsWithConflicts2) _Sort conflicts completed in " +
                        watch.ElapsedMilliseconds.ToString() + " ms");
            return roomsWithConflicts;
        }

        /// <summary>
        /// Gets a list of IDs for rooms with conflicts for a collection of dates and times
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="meetingDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="rooms">Collection of filtered rooms</param>     
        /// <returns>Collection of room IDs</returns>
        public async Task<IEnumerable<string>> GetRoomIdsWithConflicts3Async(DateTimeOffset startDateTime, DateTimeOffset endDateTime,
            IEnumerable<DateTime> meetingDates, IEnumerable<string> rooms, bool isMidnight = false)
        {
            if (meetingDates == null || !meetingDates.Any())
            {
                throw new ArgumentNullException("meetingDates", "A list of meeting dates must be provided.");
            }

            if (DateTimeOffset.Compare(startDateTime, endDateTime) > 0)
            {
                throw new ArgumentOutOfRangeException("Start time cannot be later than end time");
            }

            string[] betweenStartEndTimeResults = null;
            var roomsWithConflicts = new List<string>();

            internationalParameters = GetInternationalParameters();
            meetingDates = meetingDates.OrderBy(o => o.Date).ToList();
            var startTime = startDateTime.ToLocalDateTime(_colleagueTimeZone);
            var endTime = isMidnight ? DateTime.Today.AddDays(1).AddTicks(-1)
                    : endDateTime.ToLocalDateTime(_colleagueTimeZone);

            if (DateTimeOffset.Compare(startTime, endTime) == 0)
            {
                throw new RepositoryException("Start Time and End Time are equal : " + startTime.ToString());
            }

            var formattedDates = meetingDates.Select(key => UniDataFormatter.UnidataFormatDate(key, internationalParameters.HostShortDateFormat,
                internationalParameters.HostDateDelimiter)).ToArray();

            // Get conflicts.  Check for the combined size of all rooms and dates.  If it greater then the queryAttributeLimit then chunk.
            betweenStartEndTimeResults = ExceedsQueryAttributeLimit(rooms, formattedDates)
                ? (await GetPotentialConflictsExceedsLimitAsync(rooms, formattedDates, startTime, endTime)).ToArray()
                : (await GetPotentialConflictsAsync(rooms, formattedDates, startTime, endTime)).ToArray();

            var potentialSchedConflicts = betweenStartEndTimeResults.Distinct().ToList();
            var potentialConflicts = await DataReader.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES",
                potentialSchedConflicts.ToArray());

            if (potentialConflicts == null || !potentialConflicts.Any())
            {
                return roomsWithConflicts;
            }
            var conflicts = potentialConflicts.Where(x => x.CalsDate != null && meetingDates.Contains(x.CalsDate.Value)).ToList();
            if (!conflicts.Any())
            {
                return roomsWithConflicts;
            }
            foreach (var conflict in conflicts)
            {
                if (conflict.CalsBldgRoomEntityAssociation == null || conflict.CalsBldgRoomEntityAssociation.Count == 0)
                {
                    break;
                }
                roomsWithConflicts.AddRange(conflict.CalsBldgRoomEntityAssociation.Select(room => room.CalsBuildingsAssocMember + "*" + room.CalsRoomsAssocMember));
            }
            return roomsWithConflicts.Distinct().ToList();
        }


        private IEnumerable<Event> BuildEvents(Collection<CalendarSchedules> calsData)
        {
            var cals = new List<Event>();
            if (calsData != null)
            {
                foreach (var cal in calsData)
                {
                    try
                    {
                        // Calculate the start/end datetimeoffset value based on the Colleague time zone for the given date
                        if (!cal.CalsDate.HasValue || cal.CalsDate == new DateTime(1968, 1, 1))
                        {
                            throw new ColleagueWebApiException("Calendar item must have at least a date.");
                        }
                        DateTimeOffset startDateTime = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(cal.CalsStartTime, cal.CalsDate, _colleagueTimeZone).GetValueOrDefault();
                        DateTimeOffset endDateTime = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(cal.CalsEndTime, cal.CalsDate, _colleagueTimeZone).GetValueOrDefault();
                        var calEvent = new Event(cal.Recordkey,
                            cal.CalsDescription,
                            cal.CalsType,
                            cal.CalsLocation,
                            cal.CalsPointer,
                            startDateTime,
                            endDateTime);
                        if (cal.CalsBldgRoomEntityAssociation != null && cal.CalsBldgRoomEntityAssociation.Count > 0)
                        {
                            for (int i = 0; i < cal.CalsBldgRoomEntityAssociation.Count; i++)
                            {
                                calEvent.AddRoom(cal.CalsBuildings[i] + "*" + cal.CalsRooms[i]);
                            }
                        }
                        if (cal.CalsPeople != null && cal.CalsPeople.Count > 0)
                        {
                            foreach (var person in cal.CalsPeople)
                            {
                                calEvent.AddPerson(person);
                            }
                        }
                        cals.Add(calEvent);
                    }
                    catch (Exception ex)
                    {
                        var calString = "Calendar Schedule Id: " + cal.Recordkey + "Type: " + cal.CalsType + "Pointer: " + cal.CalsPointer + " Description: " + cal.CalsDescription;
                        LogDataError("Calendar Schedule", cal.Recordkey, cal, ex, calString);
                    }
                }
            }
            return cals;
        }

        private ApplValcodes GetEventTypes()
        {
            if (eventTypes != null)
            {
                return eventTypes;
            }
            eventTypes = GetOrAddToCache<ApplValcodes>("EventTypes",
                () =>
                    {
                        ApplValcodes eventTypesTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "EVENT.TYPES");
                        if (eventTypesTable == null)
                        {
                            var errorMessage = "Unable to access EVENT.TYPES valcode table.";
                            logger.Info(errorMessage);
                            throw new ColleagueWebApiException(errorMessage);
                        }
                        return eventTypesTable;
                    }, 240);
            return eventTypes;
        }

        private string GetActionCodeForExternalCode(string internalCode)
        {
            string result = "";
            if (!string.IsNullOrEmpty(internalCode))
            {
                if (eventTypes != null)
                {
                    var codeAssoc = eventTypes.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == internalCode).FirstOrDefault();
                    if (codeAssoc != null)
                    {
                        result = codeAssoc.ValActionCode1AssocMember;
                    }
                }
            }
            return result;
        }

        private DataContracts.IntlParams GetInternationalParameters()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = GetOrAddToCache<DataContracts.IntlParams>("InternationalParameters",
                () =>
                {
                    DataContracts.IntlParams intlParams = DataReader.ReadRecord<DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new ColleagueWebApiException(errorMessage);
                        DataContracts.IntlParams newIntlParams = new DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        /// <summary>
        /// Build a campus calendar object
        /// </summary>
        /// <param name="campusCalendarRecord">the record from which to build a CampusCalendar</param>
        /// <param name="specialDayRecords">method will join the special day records based on the special day ids in the campusCalendarRecord</param>
        /// <param name="calendarScheduleRecords">method will join the calendar schedule records based on the calendar schedule ids in the campusCalendarRecord</param>
        /// <param name="calendarDayTypesValcode">the CORE CALENDAR.DAY.TYPES valcode</param>
        /// <returns></returns>
        private Domain.Base.Entities.CampusCalendar BuildCampusCalendar(DataContracts.CampusCalendar campusCalendarRecord,
            IEnumerable<DataContracts.CampusSpecialDay> specialDayRecords,
            IEnumerable<DataContracts.CalendarSchedules> calendarScheduleRecords,
            ApplValcodes calendarDayTypesValcode)
        {
            //check arguments
            if (campusCalendarRecord == null)
            {
                throw new ArgumentNullException("campusCalendarRecord");
            }
            if (specialDayRecords == null)
            {
                specialDayRecords = new List<DataContracts.CampusSpecialDay>();
            }
            if (calendarScheduleRecords == null)
            {
                calendarScheduleRecords = new List<DataContracts.CalendarSchedules>();
            }

            //check time values which are required attrs
            if (!campusCalendarRecord.CmpcDayStartTime.HasValue)
            {
                throw new ArgumentException("campusCalendarRecord must have a CmpcDayStartTime value", "campusCalendarRecord");
            }
            if (!campusCalendarRecord.CmpcDayEndTime.HasValue)
            {
                throw new ArgumentException("campusCalendarRecord must have a CmpcDayEndTime value", "campusCalendarRecord");
            }

            //build the calendar object
            var calendar = new Domain.Base.Entities.CampusCalendar(campusCalendarRecord.Recordkey,
                campusCalendarRecord.CmpcDesc,
                campusCalendarRecord.CmpcDayStartTime.Value.TimeOfDay,
                campusCalendarRecord.CmpcDayEndTime.Value.TimeOfDay);


            //parse out the BookPastNumberOfDays value
            int days = 0;
            if (int.TryParse(campusCalendarRecord.CmpcBookPastNoDays, out days))
            {
                calendar.BookPastNumberOfDays = days;
            }

            //build the booked events
            if (campusCalendarRecord.CmpcSchedules != null && campusCalendarRecord.CmpcSchedules.Any())
            {
                //find the records with ids listed in the calendar reocrd
                var calsData = (from calendarScheduleId in campusCalendarRecord.CmpcSchedules
                                join calendarSchedule in calendarScheduleRecords on calendarScheduleId equals calendarSchedule.Recordkey
                                select calendarSchedule).ToList();

                //Each CalendarSchedule in CmpcSchedule is a bookedEvent.
                foreach (var cals in calsData)
                {
                    try
                    {
                        calendar.AddBookedEventDate(cals.CalsDate.Value);

                    }
                    catch (Exception e)
                    {
                        LogDataError("CalendarSchedule", cals.Recordkey, cals, e, "Error building BookedEvent and BookedEventDate from CalendarSchedule record ");
                    }
                }
            }

            //build the Special Days
            if (campusCalendarRecord.CmpcSpecialDays != null && campusCalendarRecord.CmpcSpecialDays.Any())
            {
                var cmsdData = (from campusSpecialDayId in campusCalendarRecord.CmpcSpecialDays
                                join specialDay in specialDayRecords on campusSpecialDayId equals specialDay.Recordkey
                                select specialDay).ToList();

                foreach (var cmsd in cmsdData)
                {
                    try
                    {
                        var specialDay = BuildSpecialDay(cmsd, calendarDayTypesValcode);

                        calendar.AddSpecialDay(specialDay);
                    }
                    catch (Exception e)
                    {
                        LogDataError("CampusSpecialDay", cmsd.Recordkey, cmsd, e, "Error building SpecialDay from CampusSpecialDay record");
                    }
                }
            }

            return calendar;
        }

        /// <summary>
        /// Build a SpecialDay entity to add to a CampusCalendar
        /// </summary>
        /// <param name="specialDayRecord">the CampusSpecialDay record</param>
        /// <param name="calendarDayTypesValcode">The CORE valcode CALENDAR.DAY.TYPES</param>
        /// <returns></returns>
        private Domain.Base.Entities.SpecialDay BuildSpecialDay(DataContracts.CampusSpecialDay specialDayRecord,
            ApplValcodes calendarDayTypesValcode)
        {
            //check arguments
            if (specialDayRecord == null)
            {
                throw new ArgumentNullException("specialDayRecord");
            }
            if (calendarDayTypesValcode == null)
            {
                throw new ArgumentNullException("calendarDayTypesValcode");
            }

            //check start and end dates which are required fields
            if (!specialDayRecord.CmsdStartDate.HasValue)
            {
                throw new ArgumentNullException("specialDayRecord must have a start date value");
            }
            if (!specialDayRecord.CmsdEndDate.HasValue)
            {
                throw new ArgumentNullException("specialDayRecord must have an end date value");
            }

            //get the entry of the calendarDayTypes valcode (CORE) based on the CmsdType
            var dayType = calendarDayTypesValcode.ValsEntityAssociation.FirstOrDefault(vals =>
                vals.ValInternalCodeAssocMember.Equals(specialDayRecord.CmsdType, StringComparison.CurrentCultureIgnoreCase));

            //its a holiday if the valcode's special action code 1 equals HO
            var isHoliday = dayType != null && dayType.ValActionCode1AssocMember != null &&
                dayType.ValActionCode1AssocMember.Equals("HO", StringComparison.CurrentCultureIgnoreCase);

            //its a payroll holiday if the valcode's special action code 3 equals HO
            var isPayrollHoliday = dayType != null && dayType.ValActionCode3AssocMember != null &&
                dayType.ValActionCode3AssocMember.Equals("HO", StringComparison.CurrentCultureIgnoreCase);


            bool isFullDay;
            DateTimeOffset startDateTime;
            DateTimeOffset endDateTime;
            if (specialDayRecord.CmsdStartTime.HasValue && specialDayRecord.CmsdEndTime.HasValue)
            {
                //not a full day when a start and end time are specified
                isFullDay = false;

                //convert to offset
                startDateTime = specialDayRecord.CmsdStartTime.ToPointInTimeDateTimeOffset(specialDayRecord.CmsdStartDate, _colleagueTimeZone).Value;
                endDateTime = specialDayRecord.CmsdEndTime.ToPointInTimeDateTimeOffset(specialDayRecord.CmsdEndDate, _colleagueTimeZone).Value;
            }
            else
            {
                //its a full day if either the start time or the end time are null
                isFullDay = true;

                //just get the Date portions
                startDateTime = new DateTimeOffset(specialDayRecord.CmsdStartDate.Value.Date);
                endDateTime = new DateTimeOffset(specialDayRecord.CmsdEndDate.Value.Date);
            }

            var specialDay = new SpecialDay(specialDayRecord.Recordkey,
                specialDayRecord.CmsdDesc,
                specialDayRecord.CmsdCampusCalendar,
                specialDayRecord.CmsdType,
                isHoliday,
                isFullDay,
                startDateTime,
                endDateTime,
                isPayrollHoliday);

            return specialDay;
        }

        /// <summary>
        /// Determine if the combined record count exceeds the queryAttributeLimit
        /// </summary>
        /// <param name="rooms"></param>
        /// <param name="formattedDates"></param>
        /// <returns></returns>
        private bool ExceedsQueryAttributeLimit(IEnumerable<string> rooms, IEnumerable<string> formattedDates)
        {
            var queryAttributeLimit = Ellucian.Colleague.Configuration.ColleagueSDKParameters.QueryAttributeLimit;
            if (queryAttributeLimit == 0) queryAttributeLimit = 100;

            int count = 0;

            if ((rooms != null) && (rooms.Any()))
            {
                count += rooms.Count();
            }
            if ((formattedDates != null) && (formattedDates.Any()))
            {
                count += formattedDates.Count();
            }
            if (count > queryAttributeLimit)
                return true;

            return false;
        }

        /// <summary>
        ///  Retrieve a list of keys containing CALENDAR.SCHEDULES entries with conflicts.   
        ///  Combined record count exceeds QueryAttributeLimit 
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="formattedDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="rooms">Collection of filtered rooms</param> 
        /// <returns>A list of keys containing CALENDAR.SCHEDULES entries with conflicts</returns>
        private async Task<IEnumerable<string>> GetPotentialConflictsExceedsLimitAsync(IEnumerable<string> rooms, IEnumerable<string> formattedDates, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            string[] betweenStartEndTimeResults = null;

            var potentialConflictRooms = await GetCalendarSchedulesElementsAsync(rooms.ToArray(), "WITH CALS.BLDG.ROOM.IDX EQ ");
            var potentialConflictDates = await GetCalendarSchedulesElementsAsync(formattedDates.ToArray(), "WITH CALS.DATE EQ ", potentialConflictRooms);

            if (startTime != DateTimeOffset.MinValue && endTime != DateTimeOffset.MinValue)
            {
                var selectBetweenStartEndTime =
                    string.Format(
                         // "WITH CALS.START.TIME GE '{0}' AND CALS.END.TIME LE '{1}' OR CALS.END.TIME GE '{0}' AND CALS.END.TIME LE '{1}'",
                         "WITH CALS.START.TIME GE '{0}' AND CALS.END.TIME LE '{1}'",
                         startTime.TimeOfDay.ToString(@"hh\:mm\:ss"),
                        endTime.TimeOfDay.ToString(@"hh\:mm\:ss"));

                if (selectBetweenStartEndTime.Any())
                {
                    betweenStartEndTimeResults = await DataReader.SelectAsync("CALENDAR.SCHEDULES", potentialConflictDates.Distinct().ToArray(), selectBetweenStartEndTime);
                }
                else
                {
                    betweenStartEndTimeResults = potentialConflictDates.Distinct().ToArray();
                }
            }
            return betweenStartEndTimeResults;
        }

        /// <summary>
        /// Retrieve a list of keys containing CALENDAR.SCHEDULES entries with conflicts.
        /// Combined record count does not exceeds QueryAttributeLimit 
        /// </summary>
        /// <param name="startTime">Time of day at which to start checking for potential conflicts</param>
        /// <param name="endTime">Time of day at which to stop checking for potential conflicts</param>
        /// <param name="formattedDates">Collection of meeting dates for which conflicts can exist</param>
        /// <param name="rooms">Collection of filtered rooms</param> 
        /// <returns>A list of keys containing CALENDAR.SCHEDULES entries with conflicts</returns>
        private async Task<IEnumerable<string>> GetPotentialConflictsAsync(IEnumerable<string> rooms, IEnumerable<string> formattedDates, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            string[] betweenStartEndTimeResults = null;
            var criteria = string.Empty;

            if ((rooms != null) && (rooms.Any()))
            {
                criteria = "WITH CALS.BLDG.ROOM.IDX EQ '" + (string.Join(" ", rooms.ToArray())).Replace(" ", "' '") + "'";
            }
            // check for potential calendar schedules conflicts         
            if (formattedDates.Any())
            {
                var selectDates = "WITH CALS.DATE EQ '" + (string.Join(" ", formattedDates.ToArray())).Replace(" ", "' '") + "'";
                criteria = string.IsNullOrEmpty(criteria) ? selectDates : string.Concat(criteria, " ", selectDates);
            }

            if (startTime != DateTimeOffset.MinValue && endTime != DateTimeOffset.MinValue)
            {
                var selectBetweenStartEndTime =
                    string.Format(
                        "WITH CALS.START.TIME GE '{0}' AND CALS.END.TIME LE '{1}'",
                        startTime.TimeOfDay.ToString(@"hh\:mm\:ss"),
                        endTime.TimeOfDay.ToString(@"hh\:mm\:ss"));

                if (selectBetweenStartEndTime.Any())
                {
                    criteria = string.IsNullOrEmpty(criteria) ? selectBetweenStartEndTime : string.Concat(criteria, " ", selectBetweenStartEndTime);
                }
            }
            if (!(string.IsNullOrEmpty(criteria)))
            {
                betweenStartEndTimeResults = await DataReader.SelectAsync("CALENDAR.SCHEDULES", criteria);
            }
            return betweenStartEndTimeResults;
        }

        /// <summary>
        /// The maximum number of attributes permitted in each query is determined by the QueryAttributeLimit.
        /// If the number of attributes is greater than the QueryAttributeLimit, perform multiple subqueries
        /// </summary>
        /// <param name="limitingList"></param>
        /// <param name="dataElements"></param>
        /// <param name="queryPrefix"></param>
        /// <returns></returns>
        private async Task<string[]> GetCalendarSchedulesElementsAsync(IReadOnlyCollection<string> dataElements, string queryPrefix, string[] limitingList = null)
        {
            if ((dataElements == null) || (!dataElements.Any()))
            {
                throw new ArgumentNullException("dataElements");
            }
            if (string.IsNullOrEmpty(queryPrefix))
            {
                throw new ArgumentNullException("queryPrefix");
            }

            var queryAttributeLimit = Configuration.ColleagueSDKParameters.QueryAttributeLimit;
            if (queryAttributeLimit == 0) queryAttributeLimit = 100;
            string[] potentialConflictIds = null;

            for (var i = 0; i < (dataElements.Count / queryAttributeLimit) + 1; i++)
            {
                var dataToQuery = string.Empty;

                // Retrieve the range of attributes
                var filteredElements = dataElements.Take(queryAttributeLimit * (i + 1)).Skip(i * queryAttributeLimit).ToArray();

                // Concatenate the list of attributes in the specified range
                dataToQuery = filteredElements.Aggregate(dataToQuery, (current, element) => current + string.Concat("'", element, "'"));

                if ((potentialConflictIds == null) || (!potentialConflictIds.Any()))
                    potentialConflictIds = await DataReader.SelectAsync("CALENDAR.SCHEDULES", limitingList, string.Concat(queryPrefix, " ", dataToQuery));
                else
                    potentialConflictIds = potentialConflictIds.Concat(await DataReader.SelectAsync("CALENDAR.SCHEDULES", limitingList, string.Concat(queryPrefix, " ", dataToQuery))).ToArray();
            }
            return potentialConflictIds;
        }

    }
}