// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Ellucian.Web.Http.Configuration;
using System.Xml.Schema;
using Ellucian.Colleague.Data.Student.Transactions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Portal Repository 
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]

    public class PortalRepository : BaseColleagueRepository, IPortalRepository
    {
        private static char _SM = Convert.ToChar(DynamicArray.SM);
        private string colleagueTimeZone;

        public PortalRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Use default cache timeout value
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
        }

        /// <summary>
        /// This returns the Course Ids from COURSES file that are applicable for deletion from the Portal. 
        /// </summary>
        public async Task<PortalDeletedCoursesResult> GetCoursesForDeletionAsync()
        {
            PortalDeletedCoursesResult result = null;
            try
            {
                var request = new PortalGetCoursesForDeletionRequest();
                var response = await transactionInvoker.ExecuteAsync<PortalGetCoursesForDeletionRequest, PortalGetCoursesForDeletionResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.GET.COURSES.FOR.DEL transaction";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    if (response.TotalCourses.HasValue)
                    {
                        int totalCourses = response.TotalCourses.Value;
                        if (totalCourses < 0)
                        {
                            logger.Warn(string.Format("Total Courses {0} retrieved for deletion for Portal is less than 0", totalCourses));
                        }
                        else if (response.CourseIds != null && totalCourses < response.CourseIds.Count)
                        {
                            logger.Warn(string.Format("Total Courses {0} retrieved for deletion for Portal is less than the actual count of list of courses retrieved {1}", totalCourses, response.CourseIds.Count));
                        }
                        else if (response.CourseIds != null && totalCourses > response.CourseIds.Count)
                        {
                            logger.Info(string.Format("Total Courses {0} retrieved for deletion for Portal is more than the actual count of list of courses retrieved {1}, hence there could be more courses applicable for deletion for Portal", totalCourses, response.CourseIds.Count));
                        }
                    }
                    else
                    {
                        logger.Info(string.Format("Total Courses retrieved for deletion for Portal is null"));
                    }
                    result = new PortalDeletedCoursesResult(response.TotalCourses, response.CourseIds);
                }
                return result;
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.GET.COURSES.FOR.DEL transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (RepositoryException ex)
            {
                string error = "An exception occured while accessing repository to retrieve and sync deleted courses for Portal";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving deleted courses for Portal at repository level";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// This returns the Sections from COURSE.SECTIONS file that are applicable for update from the Portal. 
        /// </summary>
        public async Task<PortalUpdatedSectionsResult> GetSectionsForUpdateAsync()
        {
            try
            {
                var request = new PortalGetSectionsForUpdateRequest();
                var response = await transactionInvoker.ExecuteAsync<PortalGetSectionsForUpdateRequest, PortalGetSectionsForUpdateResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.GET.SECTIONS.FOR.UPDT transaction";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    if (response.TotalSections.HasValue)
                    {
                        int totalSections = response.TotalSections.Value;
                        if (totalSections < 0)
                        {
                            logger.Warn(string.Format("Total Sections {0} retrieved for update for Portal is less than 0", totalSections));
                        }
                        else if (response.PortalUpdatedSections != null && totalSections < response.PortalUpdatedSections.Count)
                        {
                            logger.Warn(string.Format("Total Sections {0} retrieved for update for Portal is less than the actual count of list of sections retrieved {1}", totalSections, response.PortalUpdatedSections.Count));
                        }
                        else if (response.PortalUpdatedSections != null && totalSections > response.PortalUpdatedSections.Count)
                        {
                            logger.Info(string.Format("Total Sections {0} retrieved for update for Portal is more than the actual count of list of sections retrieved {1}, hence there could be more sections applicable for update for Portal", totalSections, response.PortalUpdatedSections.Count));
                        }
                    }
                    else
                    {
                        logger.Info(string.Format("Total Sections retrieved for update for Portal is null"));
                    }

                    //subroutine used by CTX Converts Sub - value marks for books into something readable but still parsable.
                    //Books information uses "...; " and Prices will use just a semi - colon.
                    string[] bookDataSeparator = { "...;" };
                    char bookCostSeparator = ';';

                    var portalSections = new List<PortalSection>();

                    List<PortalSectionMeeting> portalSectionMeetings = null;
                    List<PortalBookInformation> portalBooks = null;


                    if (response.PortalUpdatedSections == null)
                        return new PortalUpdatedSectionsResult(shortDateFormat: response.HostShortDateFormat, totalSections: response.TotalSections, sections: portalSections);

                    foreach (var section in response.PortalUpdatedSections)
                    {
                        if (section == null)
                            continue;

                        // meeting information
                        var buildings = section.CsmBldg != null ? section.CsmBldg.Split(_SM).ToList() : new List<string>();
                        var rooms = section.CsmRoom != null ? section.CsmRoom.Split(_SM).ToList() : new List<string>();
                        var instructionalMethod = section.CsmInstrMethod != null ? section.CsmInstrMethod.Split(_SM).ToList() : new List<string>();
                        var days = section.CsmDays != null ? section.CsmDays.Split(_SM).ToList() : new List<string>();
                        var starttime = section.CsmStartTime != null ? section.CsmStartTime.Split(_SM).ToList() : new List<string>();
                        var endtime = section.CsmEndTime != null ? section.CsmEndTime.Split(_SM).ToList() : new List<string>();

                        portalSectionMeetings = new List<PortalSectionMeeting>();
                        for (int i = 0; i < instructionalMethod.Count(); i++)
                        {
                            if (string.IsNullOrWhiteSpace(instructionalMethod[i]))
                                continue;

                            DateTime? stime = null;
                            DateTime? etime = null;
                            DateTimeOffset? stimeOffset = null;
                            DateTimeOffset? etimeOffset = null;

                            if (!string.IsNullOrWhiteSpace(starttime[i]))
                            {
                                stime = new DateTime(DmiString.PickTimeToDateTime(Int32.Parse(starttime[i])).Ticks);
                                stimeOffset = stime.ToPointInTimeDateTimeOffset(DateTime.MinValue, colleagueTimeZone);
                            }

                            if (!string.IsNullOrWhiteSpace(endtime[i]))
                            {
                                etime = new DateTime(DmiString.PickTimeToDateTime(Int32.Parse(endtime[i])).Ticks);
                                etimeOffset = etime.ToPointInTimeDateTimeOffset(DateTime.MinValue, colleagueTimeZone);
                            }

                            var daysofweek = !string.IsNullOrWhiteSpace(days[i]) ? ConvertSectionMeetingDaysStringToList(days[i], ' ') : new List<DayOfWeek>();

                            portalSectionMeetings.Add(new PortalSectionMeeting(
                                building: buildings[i],
                                room: rooms[i],
                                instructionalMethod: instructionalMethod[i],
                                daysOfWeek: daysofweek,
                                startTime: stimeOffset,
                                endTime: etimeOffset));
                        }

                        var depts = !string.IsNullOrWhiteSpace(section.SecDepts) ? section.SecDepts.Split(_SM).ToList() : new List<string>();
                        var faculty = !string.IsNullOrWhiteSpace(section.SecFaculty) ? section.SecFaculty.Split(_SM).ToList() : new List<string>();
                        var courseTypes = !string.IsNullOrWhiteSpace(section.CrsType) ? section.CrsType.Split(_SM).ToList() : new List<string>();

                        //book information
                        var bookData = section.BookData != null ? section.BookData.Split(bookDataSeparator, StringSplitOptions.None).ToList() : new List<string>();
                        var bookCost = section.BookCost != null ? section.BookCost.Split(bookCostSeparator).ToList() : new List<string>();

                        portalBooks = new List<PortalBookInformation>();
                        for (int i = 0; i < bookData.Count(); i++)
                        {
                            if (string.IsNullOrWhiteSpace(bookData[i]))
                                continue;

                            decimal cost;
                            if (!decimal.TryParse(bookCost[i], NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out cost))
                                cost = 0;

                            portalBooks.Add(new PortalBookInformation(information: bookData[i], cost: cost));
                        }

                        portalSections.Add(new PortalSection(
                            sectionId: section.SectionsId,
                            shortTitle: section.SecShortTitle,
                            description: section.CrsDesc,
                            location: section.SecLocation,
                            term: section.SecTerm,
                            startDate: section.SecStartDate,
                            endDate: section.SecEndDate,
                            meetingInformation: portalSectionMeetings,
                            capacity: section.SecCapacity,
                            subject: section.SecSubject,
                            departments: depts,
                            courseNumber: section.SecCourseNo,
                            sectionNumber: section.SecNo,
                            academicLevel: section.SecAcadLevel,
                            synonym: section.SecSynonym,
                            faculty: faculty,
                            minimumCredits: section.SecMinCred,
                            maximumCredits: section.SecMaxCred,
                            sectionName: section.SecName,
                            courseId: section.SecCourse,
                            prerequisiteText: section.CrsPrereqs,
                            courseTypes: courseTypes,
                            continuingEducationUnits: section.SecCeus,
                            printedComments: section.SecPrintedComments,
                            bookInformation: portalBooks,
                            totalBookCost: section.BookTotal ?? 0
                            ));
                    }

                    return new PortalUpdatedSectionsResult(shortDateFormat: response.HostShortDateFormat, totalSections: response.TotalSections, sections: portalSections);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.GET.SECTIONS.FOR.UPDT transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving updated sections for Portal at repository level";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// This returns the Course Section Ids from COURSE.SECTIONS file that are applicable for deletion from the Portal. 
        /// </summary>
        public async Task<PortalDeletedSectionsResult> GetSectionsForDeletionAsync()
        {
            PortalDeletedSectionsResult result = null;
            try
            {
                var request = new PortalGetSectionsForDeletionRequest();
                var response = await transactionInvoker.ExecuteAsync<PortalGetSectionsForDeletionRequest, PortalGetSectionsForDeletionResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.GET.SECTIONS.FOR.DEL transaction";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    if (response.TotalSections.HasValue)
                    {
                        int totalSections = response.TotalSections.Value;
                        if (totalSections < 0)
                        {
                            logger.Warn(string.Format("Total Sections {0} retrieved for deletion for Portal is less than 0", totalSections));
                        }
                        else if (response.SectionIds != null && totalSections < response.SectionIds.Count)
                        {
                            logger.Warn(string.Format("Total Sections {0} retrieved for deletion for Portal is less than the actual count of list of sections retrieved {1}", totalSections, response.SectionIds.Count));
                        }
                        else if (response.SectionIds != null && totalSections > response.SectionIds.Count)
                        {
                            logger.Info(string.Format("Total Sections {0} retrieved for deletion for Portal is more than the actual count of list of sections retrieved {1}, hence there could be more sections applicable for deletion for Portal", totalSections, response.SectionIds.Count));
                        }
                    }
                    else
                    {
                        logger.Info(string.Format("Total Sections retrieved for deletion for Portal is null"));
                    }
                    result = new PortalDeletedSectionsResult(response.TotalSections, response.SectionIds);
                }
                return result;
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.GET.SECTIONS.FOR.DEL transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (RepositoryException ex)
            {
                string error = "An exception occured while accessing repository to retrieve and sync deleted sections for Portal";
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving deleted sections for Portal at repository level";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// Returns event and reminder information for the user
        /// </summary>
        /// <param name="criteria">Event and reminder selection criteria</param>
        /// <returns>A <see cref="PortalEventsAndReminders"/> object</returns>
        public async Task<PortalEventsAndReminders> GetEventsAndRemindersAsync(string personId, PortalEventsAndRemindersQueryCriteria criteria)
        {
            PortalEventsAndReminders eventsAndReminders;
            try
            {
                if (string.IsNullOrEmpty(personId))
                {
                    throw new ArgumentNullException("Person ID is required when retrieving Portal events and reminders.");
                }
                PortalGetEventsAndRemindersRequest request = new PortalGetEventsAndRemindersRequest()
                {
                    APersonId = personId,
                    AStartDate = criteria.StartDate,
                    AEndDate = criteria.EndDate,
                    AlEventTypes = criteria.EventTypeCodes.ToList()
                };
                var response = await transactionInvoker.ExecuteAsync<PortalGetEventsAndRemindersRequest, PortalGetEventsAndRemindersResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.GET.EVENTS.AND.REMINDERS transaction.";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    // Set the Host Short Date Format
                    eventsAndReminders = new PortalEventsAndReminders(response.HostShortDateFormat);

                    // Add events
                    if (response.Events != null && response.Events.Any())
                    {
                        foreach(var portalEvent in response.Events)
                        {
                            try
                            {
                                var startTime = portalEvent.StartTime.ToPointInTimeDateTimeOffset(portalEvent.Date, colleagueTimeZone);
                                var endTime = portalEvent.EndTime.ToPointInTimeDateTimeOffset(portalEvent.Date, colleagueTimeZone);

                                PortalEvent pEvent = new PortalEvent(portalEvent.CalendarSchedulesId,
                                    portalEvent.CourseSectionsId,
                                    portalEvent.Date,
                                    startTime,
                                    endTime,
                                    portalEvent.Description,
                                    portalEvent.BuildingInfo,
                                    portalEvent.RoomInfo,
                                    portalEvent.EventType,
                                    portalEvent.CourseSectionSubject,
                                    portalEvent.CourseSectionCourseNumber,
                                    portalEvent.CourseSectionNumber,
                                    portalEvent.Participants);
                                eventsAndReminders.AddPortalEvent(pEvent);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("Portal Event", portalEvent.CalendarSchedulesId, portalEvent, ex, string.Format("Could not create Portal event data for user {0}, event {1}", personId, portalEvent.CalendarSchedulesId));
                            }
                        }
                    }

                    // Add reminders
                    if (response.Reminders != null && response.Reminders.Any())
                    {
                        foreach (var portalReminder in response.Reminders)
                        {
                            try
                            {
                                var startTime = portalReminder.ReminderActionTime.ToPointInTimeDateTimeOffset(portalReminder.ReminderStartDate, colleagueTimeZone);
                                var endTime = portalReminder.ReminderEndTime.ToPointInTimeDateTimeOffset(portalReminder.ReminderEndDate, colleagueTimeZone);

                                PortalReminder pReminder = new PortalReminder(portalReminder.ReminderId,
                                    portalReminder.ReminderStartDate.Value,
                                    startTime.Value,
                                    portalReminder.ReminderEndDate,
                                    endTime,
                                    portalReminder.ReminderCity,
                                    portalReminder.ReminderRegions,
                                    portalReminder.ReminderType,
                                    portalReminder.ReminderShortText,
                                    portalReminder.ReminderParticipants);
                                eventsAndReminders.AddPortalReminder(pReminder);                            
                            }
                            catch (Exception ex)
                            {
                                LogDataError("Portal Reminder", portalReminder.ReminderId, portalReminder, ex, string.Format("Could not create Portal reminder data for user {0}, reminder {1}", personId, portalReminder.ReminderId));
                            }
                        }
                    }
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.GET.EVENTS.AND.REMINDERS transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (RepositoryException ex)
            {
                string error = string.Format("An exception occured while accessing repository to retrieve and sync Portal events and reminders for user {0}.", personId);
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving Portal events and reminders.";
                logger.Error(ex, error);
                throw;
            }
            return eventsAndReminders;
        }

        /// <summary>
        /// This returns the Sections from COURSES file that are applicable for update from the Portal. 
        /// </summary>
        public async Task<PortalUpdatedCoursesResult> GetCoursesForUpdateAsync()
        {
            try
            {
                var request = new PortalGetCoursesForUpdateRequest();
                var response = await transactionInvoker.ExecuteAsync<PortalGetCoursesForUpdateRequest, PortalGetCoursesForUpdateResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.GET.COURSES.FOR.UPDT transaction";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    if (response.TotalCourses.HasValue)
                    {
                        int totalCourses = response.TotalCourses.Value;
                        if (totalCourses < 0)
                        {
                            logger.Warn(string.Format("Total Courses {0} retrieved for update for Portal is less than 0", totalCourses));
                        }
                        else if (response.PortalUpdatedCourses != null && totalCourses < response.PortalUpdatedCourses.Count)
                        {
                            logger.Warn(string.Format("Total Courses {0} retrieved for update for Portal is less than the actual count of list of courses retrieved {1}", totalCourses, response.PortalUpdatedCourses.Count));
                        }
                        else if (response.PortalUpdatedCourses != null && totalCourses > response.PortalUpdatedCourses.Count)
                        {
                            logger.Info(string.Format("Total Courses {0} retrieved for update for Portal is more than the actual count of list of courses retrieved {1}, hence there could be more courses applicable for update for Portal", totalCourses, response.PortalUpdatedCourses.Count));
                        }
                    }
                    else
                    {
                        logger.Info(string.Format("Total Courses retrieved for update for Portal is null"));
                    }

                    var portalCourses = new List<PortalCourse>();


                    if (response.PortalUpdatedCourses == null)
                        return new PortalUpdatedCoursesResult(totalCourses: response.TotalCourses, courses: portalCourses);

                    foreach (var course in response.PortalUpdatedCourses)
                    {
                        if (course == null)
                            continue;

                        var depts = !string.IsNullOrWhiteSpace(course.CrsDepts) ? course.CrsDepts.Split(_SM).ToList() : new List<string>();
                        var courseTypes = !string.IsNullOrWhiteSpace(course.CrsTypes) ? course.CrsTypes.Split(_SM).ToList() : new List<string>();
                        var locations = !string.IsNullOrWhiteSpace(course.Locations) ? course.Locations.Split(_SM).ToList() : new List<string>();

                        portalCourses.Add(new PortalCourse(
                            courseId: course.CoursesId,
                            shortTitle: course.CrsShortTitle,
                            title: course.CrsTitle,
                            description: course.CrsDesc,
                            subject: course.CrsSubject,
                            departments: depts,
                            courseNumber: course.CrsNo,
                            academicLevel: course.CrsAcadLevel,
                            courseName: course.CrsName,
                            courseTypes: courseTypes,
                            prerequisiteText: course.CrsPrereqs,
                            locations: locations
                            ));
                    }

                    return new PortalUpdatedCoursesResult(totalCourses: response.TotalCourses, courses: portalCourses);
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.GET.COURSES.FOR.UPDT transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while retrieving updated courses for Portal at repository level";
                logger.Error(ex, error);
                throw;
            }
        }

        /// <summary>
        /// Updates a student's list of preferred course sections
        /// </summary>
        /// <param name="studentId">ID of the student whose list of preferred course sections is being updated</param>
        /// <param name="courseSectionIds">IDs of the course sections to be added to the student's list of preferred course sections</param>
        /// <returns>Collection of <see cref="PortalStudentPreferredCourseSectionUpdateResult"/></returns>
        public async Task<IEnumerable<PortalStudentPreferredCourseSectionUpdateResult>> UpdateStudentPreferredCourseSectionsAsync(string studentId, IEnumerable<string> courseSectionIds)
        {
            List<PortalStudentPreferredCourseSectionUpdateResult> results = new List<PortalStudentPreferredCourseSectionUpdateResult>(); 
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException("Student ID is required when updating a student's list of preferred course sections.");
                }
                if (courseSectionIds == null || !courseSectionIds.Any(id => !string.IsNullOrWhiteSpace(id)))
                {
                    throw new ArgumentNullException("At least one course section ID is required when updating a student's list of preferred course sections.");
                }
                List<string> sectionsToAdd = courseSectionIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                PortalUpdatePreferredSectionsRequest request = new PortalUpdatePreferredSectionsRequest()
                {
                    StudentId = studentId,
                    CourseSectionsToAdd = sectionsToAdd
                };
                var response = await transactionInvoker.ExecuteAsync<PortalUpdatePreferredSectionsRequest, PortalUpdatePreferredSectionsResponse>(request);
                if (response == null)
                {
                    string error = "No response received while invoking PORTAL.UPDATE.PREFERRED.SECTIONS transaction.";
                    logger.Error(error);
                    throw new RepositoryException(error);
                }
                else
                {
                    if (response.Results == null || !response.Results.Any())
                    {
                        throw new RepositoryException(string.Format("Calling PORTAL.UPDATE.PREFERRED.SECTIONS for student {0} did not return any results.", studentId));
                    }
                    var resultSectionIds = response.Results.Select(res => res.CourseSectionId).ToList();
                    foreach(var sectionToAdd in sectionsToAdd)
                    {
                        if (!resultSectionIds.Contains(sectionToAdd))
                        {
                            throw new RepositoryException(string.Format("Request to PORTAL.UPDATE.PREFERRED.SECTIONS for student {0} contained course section {1} but response did not contain a result for this course section.", studentId, sectionToAdd));
                        }
                    }
                    foreach(var result in response.Results)
                    {
                        try
                        {
                            var status = ConvertStringToPortalStudentPreferredCourseSectionUpdateStatus(result.AddStatus);
                            var cleanMsg = result.Message != null ? result.Message.Trim() : string.Empty;
                            results.Add(new PortalStudentPreferredCourseSectionUpdateResult(result.CourseSectionId, status, cleanMsg));
                        }
                        catch (Exception ex)
                        {
                            LogDataError("PortalStudentPreferredCourseSectionUpdateResult", result.CourseSectionId, result, ex);
                        }
                    }
                    if (results.Count != sectionsToAdd.Count)
                    {
                        throw new RepositoryException("The number of sections requested to be added to student {0}'s preferred course sections does not match the number of valid results returned by Colleague.");
                    }
                }
            }
            catch (ColleagueTransactionException cte)
            {
                string error = "Exception occurred while invoking PORTAL.UPDATE.PREFERRED.SECTIONS transaction";
                logger.Error(cte, error);
                throw new RepositoryException(error);
            }
            catch (RepositoryException ex)
            {
                string error = string.Format("An exception occured while accessing repository to update preferred course sections for student {0}.", studentId);
                logger.Error(ex, error);
                throw;
            }
            catch (Exception ex)
            {
                string error = "An exception occurred while updating student preferred course sections.";
                logger.Error(ex, error);
                throw;
            }
            return results;
        }

        private PortalStudentPreferredCourseSectionUpdateStatus ConvertStringToPortalStudentPreferredCourseSectionUpdateStatus(string statusString)
        {
            if (string.IsNullOrWhiteSpace(statusString))
            {
                throw new ArgumentNullException("statusString", "The status for adding a course section to a student's list of preferred course sections cannot be null or blank.");
            }
            switch(statusString)
            {
                case "OK":
                    return PortalStudentPreferredCourseSectionUpdateStatus.Ok;
                case "ERROR":
                    return PortalStudentPreferredCourseSectionUpdateStatus.Error;
                default:
                    throw new ArgumentException(string.Format("Status {0} is not a valid status for adding a course section to a student's list of preferred course sections.", statusString));
            }
        }

        private List<DayOfWeek> ConvertSectionMeetingDaysStringToList(string meetingDays, char separator = ' ')
        {
            var daysOfWeek = new List<DayOfWeek>();
            var days = meetingDays.Split(separator);
            foreach (string day in days)
            {
                switch (day)
                {
                    case "SU":
                        daysOfWeek.Add(DayOfWeek.Sunday);
                        break;
                    case "M":
                        daysOfWeek.Add(DayOfWeek.Monday);
                        break;
                    case "T":
                        daysOfWeek.Add(DayOfWeek.Tuesday);
                        break;
                    case "W":
                        daysOfWeek.Add(DayOfWeek.Wednesday);
                        break;
                    case "TH":
                        daysOfWeek.Add(DayOfWeek.Thursday);
                        break;
                    case "F":
                        daysOfWeek.Add(DayOfWeek.Friday);
                        break;
                    case "S":
                        daysOfWeek.Add(DayOfWeek.Saturday);
                        break;
                }
            }
            return daysOfWeek;
        }
    }
}






