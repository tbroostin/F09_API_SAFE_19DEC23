// Copyright 2012-2019 Ellucian Company L.P. and its affiliates
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SectionRepository : BaseColleagueRepository, ISectionRepository, IEthosExtended
    {
        private ApplValcodes waitlistStatuses;
        private IEnumerable<BookOption> bookOptions;
        private ITermRepository termRepository;
        private IEnumerable<Term> termList;
        private string colleagueTimeZone;
        private IEnumerable<SectionStatusCode> _sectionStatusCodes;
        private Data.Base.DataContracts.IntlParams internationalParameters;
        //private IEnumerable<CourseCategory> _courseCategories;
        private RepositoryException exception;
        private bool addToErrorCollection = false;

        const int SectionMeetingCacheTimeout = 20; // Clear from cache every 20 minutes
        const int SectionInstructorCacheTimeout = 20;
        const string AllSectionInstructorCache = "AllSectionInstructor";


        private async Task<IEnumerable<SectionStatusCode>> GetSectionStatusCodesAsync()
        {
            if (_sectionStatusCodes == null)
            {
                _sectionStatusCodes = await GetValcodeAsync<SectionStatusCode>("ST", "SECTION.STATUSES",
                ss => new SectionStatusCode(ss.ValInternalCodeAssocMember, ss.ValExternalRepresentationAssocMember,
                    ConvertSectionStatusActionToStatusType(ss.ValActionCode1AssocMember),
                    ConvertSectionStatusActionToIntegrationStatusType(ss.ValActionCode3AssocMember)), Level1CacheTimeoutValue);

            }
            return _sectionStatusCodes;
        }

        public async Task<IEnumerable<SectionStatusCodeGuid>> GetStatusCodesWithGuidsAsync()
        {

            return await GetGuidValcodeAsync("ST", "SECTION.STATUSES", (ss, g) => new SectionStatusCodeGuid(g, ss.ValInternalCodeAssocMember,
                ss.ValExternalRepresentationAssocMember), Level1CacheTimeoutValue);

        }


        // Track the time that the addtional/changed section cache was last built.
        private static DateTime ChangedRegistrationSectionsCacheBuildTime = new DateTime();

        //private ApplValcodes waitlistStatuses;
        private Ellucian.Colleague.Data.Base.DataContracts.IntlParams _internationalParameters;

        private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> InternationalParametersAsync()
        {

            if (_internationalParameters == null)
            {
                // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
                _internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                     async () =>
                     {
                         Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                         if (intlParams == null)
                         {
                             var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                             logger.Info(errorMessage);
                             // If we cannot read the international parameters default to US with a / delimiter.
                             // throw new Exception(errorMessage);
                             Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                             newIntlParams.HostShortDateFormat = "MDY";
                             newIntlParams.HostDateDelimiter = "/";
                             intlParams = newIntlParams;
                         }
                         return intlParams;
                     }, Level1CacheTimeoutValue);
            }
            return _internationalParameters;

        }

        private string _activeStatus = null;
        private async Task<string> GetActiveStatusAsync()
        {
            if (_activeStatus == null)
            {
                var status = (await GetSectionStatusCodesAsync()).FirstOrDefault(ss => ss.StatusType == SectionStatus.Active);
                _activeStatus = status == null ? null : status.Code;
            }
            return _activeStatus;
        }

        private string _cancelledStatus = null;
        private async Task<string> GetCancelledStatusAsync()
        {
            if (_cancelledStatus == null)
            {
                var status = (await GetSectionStatusCodesAsync()).FirstOrDefault(ss => ss.StatusType == SectionStatus.Cancelled);
                _cancelledStatus = status == null ? null : status.Code;
            }
            return _cancelledStatus;
        }

        private string _otherStatus = null;
        private async Task<string> GetOtherStatusAsync()
        {
            if (_otherStatus == null)
            {
                var status = (await GetSectionStatusCodesAsync()).FirstOrDefault(ss => ss.StatusType == SectionStatus.Inactive);
                _otherStatus = status == null ? null : status.Code;
            }
            return _otherStatus;
        }

        private Dflts _coreParameters;
        private async Task<Dflts> CoreParametersAsync()
        {
            if (_coreParameters == null)
            {
                _coreParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.Dflts>("CoreParameters",
                    async () =>
                    {
                        Data.Base.DataContracts.Dflts defaults = await DataReader.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS");
                        if (defaults == null)
                        {
                            var errorMessage = "Unable to access core parameters DEFAULTS";
                            logger.Info(errorMessage);
                            // If we cannot read the core parameters create an empty one.
                            Data.Base.DataContracts.Dflts newCoreParams = new Data.Base.DataContracts.Dflts();
                            defaults = newCoreParams;
                        }
                        return defaults;
                    }, Level1CacheTimeoutValue);
            }
            return _coreParameters;
        }

        private Domain.Base.Entities.CampusCalendar _campusCalendar;
        private async Task<Domain.Base.Entities.CampusCalendar> CampusCalendarAsync()
        {
            if (_campusCalendar == null)
            {
                _campusCalendar = await GetOrAddToCacheAsync<Domain.Base.Entities.CampusCalendar>("CampusCalendar",
                    async () =>
                    {
                        Domain.Base.Entities.CampusCalendar calendar = null;
                        var coreParameters = await CoreParametersAsync();
                        if (!string.IsNullOrEmpty(coreParameters.DfltsCampusCalendar))
                        {
                            Data.Base.DataContracts.CampusCalendar calendarData = await DataReader.ReadRecordAsync<Data.Base.DataContracts.CampusCalendar>(coreParameters.DfltsCampusCalendar);
                            if (calendarData == null)
                            {
                                var errorMessage = "Calendar record not found for ID " + coreParameters.DfltsCampusCalendar;
                                logger.Info(errorMessage);
                                throw new KeyNotFoundException("Calendar record not found for ID " + coreParameters.DfltsCampusCalendar);
                            }

                            //read the specialDay records
                            var specialDayIds = calendarData.CmpcSpecialDays != null ? calendarData.CmpcSpecialDays.ToArray() : new string[0];
                            var specialDayRecords = await DataReader.BulkReadRecordAsync<Base.DataContracts.CampusSpecialDay>(specialDayIds);
                            if (specialDayRecords == null || !specialDayRecords.Any())
                            {
                                logger.Info(string.Format("No special day records were found from the CampusCalendar {0} SpecialDay pointers", calendarData.Recordkey));
                                specialDayRecords = new Collection<CampusSpecialDay>();
                            }

                            //read the calendar day types valcode
                            var calendarDayTypesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CALENDAR.DAY.TYPES");
                            if (calendarDayTypesValcode == null)
                            {
                                logger.Info("Unable to find CORE.VALCODES -> CALENDAR.DAY.TYPES");
                                calendarDayTypesValcode = new ApplValcodes();
                            }

                            try
                            {
                                calendar = new Domain.Base.Entities.CampusCalendar(calendarData.Recordkey, calendarData.CmpcDesc, calendarData.CmpcDayStartTime.Value.TimeOfDay,
                                    calendarData.CmpcDayEndTime.Value.TimeOfDay);
                                int days = 0;
                                if (int.TryParse(calendarData.CmpcBookPastNoDays, out days))
                                {
                                    calendar.BookPastNumberOfDays = days;
                                };
                                if (calendarData.CmpcSchedules != null && calendarData.CmpcSchedules.Count > 0)
                                {
                                    foreach (var scheduleId in calendarData.CmpcSchedules)
                                    {
                                        calendar.AddBookedEventDate(await BuildBookedEventDateAsync(scheduleId));
                                    }
                                }
                                if (calendarData.CmpcSpecialDays != null && calendarData.CmpcSpecialDays.Any())
                                {
                                    foreach (var specialDayId in calendarData.CmpcSpecialDays)
                                    {

                                        var specialDayRecord = specialDayRecords.FirstOrDefault(r => r.Recordkey == specialDayId);
                                        if (specialDayRecord != null)
                                        {
                                            try
                                            {
                                                calendar.AddSpecialDay(BuildSpecialDay(specialDayRecord, calendarDayTypesValcode));
                                            }
                                            catch (Exception e)
                                            {
                                                LogDataError("CAMPUS.SPECIAL.DAY", specialDayId, specialDayRecord, e);
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogDataError("Campus calendar", calendarData.Recordkey, calendarData, ex);
                                throw new KeyNotFoundException("Calendar record not found for ID " + coreParameters.DfltsCampusCalendar);
                            }

                            return calendar;
                        }
                        else
                        {
                            throw new ArgumentException("Core Parameters does not have a default campus calendar id defined.");
                        }
                    }, Level1CacheTimeoutValue);
            }
            return _campusCalendar;
        }

        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public SectionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using 24 hours for the RegistrationSections cache timeout - otherwise not caching section info.
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;

            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
            termRepository = new TermRepository(cacheProvider, transactionFactory, logger);
        }

        // Sets string for the AllRegistrationSectionsCache
        const string AllRegistrationSectionsCache = "AllRegistrationSections";

        // Sets string for the AllRegistrationSectionsDateCache
        const string AllRegistrationSectionsCacheDate = "AllRegistrationSectionsCacheDate";

        // Settings for the ChangedRegistrationSectionsCache
        const string ChangedRegistrationSectionsCache = "ChangedRegistrationSections";
        const int changedRegistrationSectionsCacheTimeout = 10; // Rebuild every 10 minutes

        // setting for SectionMeetingInstance Cache
        const string SectionMeetingInstancesCache = "SectionMeetingInstances_";
        const int sectionMeetingInstanceCacheTimeout = 20; // Rebuild every 20 minutes

        // setting for AllSections Cache
        const string AllSectionsCache = "AllSections";
        const int AllSectionsCacheTimeout = 20; // Clear from cache every 20 minutes

        /// <summary>
        /// Get a <see cref="SectionWaitlist"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionWaitlist"/></returns>
        public async Task<SectionWaitlist> GetSectionWaitlistAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a course section ID.");
            }
            List<WaitList> waitlistStudents = await GetWaitListsAsync(new List<string>() { sectionId });

            SectionWaitlist sectionWailitst = new SectionWaitlist(sectionId);

            foreach (var item in waitlistStudents)
            {
                string waitListStatusValcode = await GetWaitlistStatusActionCodeAsync(item.WaitStatus);
                /* Waitlist status val codes
                 * 1. Active
                 * 4. Permission to register
                 */
                if (!string.IsNullOrEmpty(waitListStatusValcode) && (waitListStatusValcode == "1" || waitListStatusValcode == "4"))
                    sectionWailitst.AddStudentId(item.WaitStudent);
            }

            return sectionWailitst;
        }


        /// <summary>
        /// Get a list of<see cref="SectionWaitlist"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A list of<see cref="SectionWaitlist"/></returns>
        public async Task<IEnumerable<SectionWaitlistStudent>> GetSectionWaitlist2Async(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a course section ID.");
            }
            List<WaitList> waitlistStudents = await GetWaitListsAsync(new List<string>() { sectionId });
            List<SectionWaitlistStudent> WaitlistDetails = new List<SectionWaitlistStudent>();
            int rank = 0;
            var stwebSettings = await GetStwebDefaultsAsync();
            List<string> nonActiveStudentsStatuses = new List<string>();
            if (stwebSettings != null)
            {
                nonActiveStudentsStatuses = stwebSettings.StwebShowOthWaitStatuses;
            }

            if (waitlistStudents != null && waitlistStudents.Any())
            {
                var waitlist = waitlistStudents.Where(ws => ws != null).OrderByDescending(s => s.WaitRating).ThenBy(s => s.WaitStatusDate).ThenBy(s => s.WaitTime);
                foreach (var item in waitlist)
                {
                    string waitListStatusValcode = await GetWaitlistStatusActionCodeAsync(item.WaitStatus);
                    /* Waitlist status val codes
                     * 1. Active
                     * 4. Permission to register
                     */                    
                    if (!string.IsNullOrEmpty(waitListStatusValcode) && (waitListStatusValcode == "1" || waitListStatusValcode == "4"))
                    {
                        rank++;
                        SectionWaitlistStudent sectionWaitlist = new SectionWaitlistStudent(item.WaitCourseSection, item.WaitStudent, rank, item.WaitRating, waitListStatusValcode, item.WaitStatusDate, item.WaitTime);
                        WaitlistDetails.Add(sectionWaitlist);
                    }
                   else if(nonActiveStudentsStatuses.Count() > 0 && nonActiveStudentsStatuses.Contains(item.WaitStatus))
                    {                        
                        SectionWaitlistStudent sectionWaitlist = new SectionWaitlistStudent(item.WaitCourseSection, item.WaitStudent, 0, 0, waitListStatusValcode, item.WaitStatusDate, item.WaitTime);
                        WaitlistDetails.Add(sectionWaitlist);
                    }

                }
            }
            return WaitlistDetails;
        }

        /// <summary>
        /// Get a <see cref="SectionRoster"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionRoster"/></returns>
        public async Task<SectionRoster> GetSectionRosterAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section roster without a course section ID.");
            }

            // Read COURSE.SECTION
            CourseSections courseSection = await DataReader.ReadRecordAsync<CourseSections>(sectionId);
            if (courseSection == null)
            {
                throw new KeyNotFoundException(string.Format("Unable to retrieve course data for section {0}", sectionId));
            }

            // Initialize the section roster
            SectionRoster sectionRoster = new SectionRoster(sectionId);

            // Add faculty assigned to the course section
            if (courseSection.SecFaculty != null)
            {
                // Remove null/empty IDs from list of records to read
                string[] sanitizedCsfIds = courseSection.SecFaculty.Where(csf => !string.IsNullOrEmpty(csf)).Distinct().ToArray();

                // Read COURSE.SEC.FACULTY records
                Collection<CourseSecFaculty> courseSecFacultys = await DataReader.BulkReadRecordAsync<CourseSecFaculty>(sanitizedCsfIds);
                if (courseSecFacultys == null)
                {
                    string message = string.Format("Unable to retrieve course section faculty data for section {0}", sectionId);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                // Determine if any COURSE.SEC.FACULTY records are missing
                List<string> missingCourseSecFacultys = sanitizedCsfIds.Except(courseSecFacultys.Where(csf => csf != null).Select(csf => csf.Recordkey)).ToList();
                if (missingCourseSecFacultys.Any())
                {
                    string message = string.Format("Unable to retrieve COURSE.SEC.FACULTY data for IDs: {0}", String.Join(", ", missingCourseSecFacultys));
                    logger.Error(message);
                }

                // Extract faculty IDs from STUDENT.COURSE.SEC records
                List<string> facultyIds = courseSecFacultys.Where(csf => csf != null).Select(csf => csf.CsfFaculty).ToList();

                // Add faculty to section roster
                foreach (var id in facultyIds)
                {
                    try
                    {
                        sectionRoster.AddFacultyId(id);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(String.Format("Unable to add faculty ID {0} to roster for section {1}: " + ex, id, sectionId));
                    }
                }
            }

            // Read the course section's referenced STUDENT.COURSE.SEC records
            if (courseSection.SecActiveStudents != null)
            {
                // Remove null/empty IDs from list of records to read
                string[] sanitizedScsIds = courseSection.SecActiveStudents.Where(sas => !string.IsNullOrEmpty(sas)).Distinct().ToArray();

                // Read STUDENT.COURSE.SEC records
                Collection<StudentCourseSec> studentCourseSecs = await DataReader.BulkReadRecordAsync<StudentCourseSec>(sanitizedScsIds);
                if (studentCourseSecs == null)
                {
                    string message = string.Format("Unable to retrieve student course section data for section {0}", sectionId);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                // Determine if any STUDENT.COURSE.SEC records are missing
                List<string> missingStudentCourseSecs = sanitizedScsIds.Except(studentCourseSecs.Where(scs => scs != null).Select(scs => scs.Recordkey)).ToList();
                if (missingStudentCourseSecs.Any())
                {
                    string message = string.Format("Unable to retrieve STUDENT.COURSE.SECTION data for IDs: {0}", String.Join(", ", missingStudentCourseSecs));
                    logger.Error(message);
                }

                // Extract student IDs from STUDENT.COURSE.SEC records
                List<string> studentIds = studentCourseSecs.Where(scs => scs != null).Select(scs => scs.ScsStudent).ToList();

                // Add students to section roster
                foreach (var id in studentIds)
                {
                    try
                    {
                        sectionRoster.AddStudentId(id);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(String.Format("Unable to add student ID {0} to roster for section {1}: " + ex, id, sectionId));
                    }
                }
            }
            return sectionRoster;
        }

        /// <summary>
        /// Get a single section using an ID
        /// </summary>
        /// <param name="id">The section ID</param>
        /// <returns>The section</returns>
        public async Task<Section> GetSectionAsync(string id)
        {
            Section section = null;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a section.");
            }

            // Now we have an ID, so we can read the record
            var courseSection = await DataReader.ReadRecordAsync<CourseSections>(id);
            if (courseSection == null)
            {
                throw new KeyNotFoundException("Invalid ID for section: " + id);
            }

            // Build the section data
            section = (await BuildNonCachedSectionsAsync(new List<CourseSections>() { courseSection })).FirstOrDefault();

            return section;
        }

        /// <summary>
        /// Create a SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The section</param>
        /// <returns>The created sectionCrosslist</returns>
        public async Task<SectionCrosslist> CreateSectionCrosslistAsync(SectionCrosslist sectionCrosslist)
        {
            return await CreateOrUpdateSectionCrosslistAsync(sectionCrosslist);
        }

        /// <summary>
        /// Update a SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The section</param>
        /// <returns>The updated sectionCrosslist</returns>
        public async Task<SectionCrosslist> UpdateSectionCrosslistAsync(SectionCrosslist sectionCrosslist)
        {
            return await CreateOrUpdateSectionCrosslistAsync(sectionCrosslist);
        }

        /// <summary>
        /// Create/Update a SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The SectionCrosslist to create or update</param>
        /// <returns>The new/updated SectionCrosslist</returns>
        private async Task<SectionCrosslist> CreateOrUpdateSectionCrosslistAsync(SectionCrosslist sectionCrosslist)
        {
            var extendedDataTuple = GetEthosExtendedDataLists();


            var request = new UpdateSectionXlistRequest()
            {
                Capacity = sectionCrosslist.Capacity,
                CourseSections = sectionCrosslist.SectionIds,
                CourseSecXlistsId = sectionCrosslist.Id,
                PrimarySection = sectionCrosslist.PrimarySectionId,
                StrGuid = sectionCrosslist.Guid,
                WaitlistFlag = string.Equals(sectionCrosslist.WaitlistFlag, "Y", StringComparison.OrdinalIgnoreCase),
                WaitlistMax = sectionCrosslist.WaitlistMax
            };

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateSectionXlistRequest, UpdateSectionXlistResponse>(request);

            if (response.ErrorMessages != null && response.ErrorMessages.Any())
            {
                // Register repository errors and throw an exception
                var exception = new RepositoryException("Errors encountered while updating SectionCrosslist " + sectionCrosslist.Id);
                exception.AddErrors(response.ErrorMessages.ConvertAll(x => (new RepositoryError(x.ErrorCode, x.ErrorMsg))));
                throw exception;
            }

            return string.IsNullOrEmpty(response.StrGuid) ? null : await GetSectionCrosslistByGuidAsync(response.StrGuid);
        }

        /// <summary>
        /// Delete a sectioncrosslist by the id
        /// </summary>
        /// <param name="id">id of the section to delete</param>
        public async Task DeleteSectionCrosslistAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Record not found, no delete occurred.");
            }
            // Now we have an ID, so we can pass of the rest of the work to a transaction
            var request = new DeleteSectionXlistRequest()
            {
                CourseSecXlistsId = id
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteSectionXlistRequest, DeleteSectionXlistResponse>(request);

            if (response.DeleteCrossListErrors != null && response.DeleteCrossListErrors.Any())
            {
                var exception = new RepositoryException("Errors encountered while deleting sectioncrosslist " + id);
                foreach (var error in response.DeleteCrossListErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(error.ErrorCode) ? "" : error.ErrorCode, error.ErrorMsg));
                }
                throw exception;
            }
        }

        /// <summary>
        /// Get a single sectioncrosslist using an ID
        /// </summary>
        /// <param name="id">The sectioncrosslist ID</param>
        /// <returns>The sectioncrosslist</returns>
        public async Task<SectionCrosslist> GetSectionCrosslistAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a sectioncrosslist.");
            }

            var secXlist = await DataReader.ReadRecordAsync<CourseSecXlists>(id);
            if (secXlist == null)
            {
                throw new KeyNotFoundException("Invalid ID for sectioncrosslist: " + id);
            }

            return new SectionCrosslist(secXlist.Recordkey, secXlist.CsxlPrimarySection, secXlist.CsxlCourseSections,
                secXlist.RecordGuid, secXlist.CsxlCapacity, secXlist.CsxlWaitlistFlag, secXlist.CsxlWaitlistMax);

        }

        /// <summary>
        /// Get a single SectionCrosslist using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The SectionCrosslist</returns>
        public async Task<SectionCrosslist> GetSectionCrosslistByGuidAsync(string guid)
        {
            return await GetSectionCrosslistAsync(await GetSectionCrosslistIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Gets a list of SectionCrosslist's which can be filtered by section guid
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="section">The section id to filter SectionCrosslist list on</param>
        /// <returns>list of SectionCrosslist</returns>
        public async Task<Tuple<IEnumerable<SectionCrosslist>, int>> GetSectionCrosslistsPageAsync(int offset, int limit, string section = "")
        {
            string criteria = string.Empty;
            if (!string.IsNullOrEmpty(section))
            {
                criteria = string.Concat(criteria, "WITH CSXL.COURSE.SECTIONS = ", section, " ");
            }

            var crosslistIds = await DataReader.SelectAsync("COURSE.SEC.XLISTS", criteria);
            var totalCount = crosslistIds.Count();

            Array.Sort(crosslistIds);

            var subList = crosslistIds.Skip(offset).Take(limit).ToArray();

            var pageData = await DataReader.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", subList);

            var returnList = new List<SectionCrosslist>();
            pageData.ForEach(x =>
            {
                returnList.Add(new SectionCrosslist(x.Recordkey, x.CsxlPrimarySection, x.CsxlCourseSections,
                    x.RecordGuid, x.CsxlCapacity, x.CsxlWaitlistFlag, x.CsxlWaitlistMax));
            });

            return new Tuple<IEnumerable<SectionCrosslist>, int>(returnList, totalCount);
        }

        /// <summary>
        /// Get the GUID for a SectionCrosslist using its ID
        /// </summary>
        /// <param name="id">SectionCrosslist ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetSectionCrosslistGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("COURSE.SEC.XLISTS", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("SectionCrosslist.Guid.NotFound", "GUID not found for SectionCrosslist " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get the SectionCrosslist record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetSectionCrosslistIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("SectionCrosslist GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("SectionCrosslist GUID " + guid + " lookup failed.");
            }


            if (!string.Equals(foundEntry.Value.Entity, "COURSE.SEC.XLISTS", StringComparison.OrdinalIgnoreCase))
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, COURSE.SEC.XLISTS");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the GUID for a section using its ID
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        public async Task<string> GetSectionGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("COURSE.SECTIONS", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Section.Guid.NotFound", "GUID not found for section " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Using a collection of section ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="sectionIds">collection of person ids</param>
        /// <returns>Dictionary consisting of a sectionId (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetSectionGuidsCollectionAsync(IEnumerable<string> sectionIds)
        {
            if ((sectionIds == null) || (sectionIds != null && !sectionIds.Any()))
            {
                return new Dictionary<string, string>();
            }
            var sectionGuidCollection = new Dictionary<string, string>();
            var missingGuids = new List<string>();
            try
            {
                var sectionGuidLookup = sectionIds
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup("COURSE.SECTIONS", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(sectionGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!sectionGuidCollection.ContainsKey(splitKeys[1]))
                    {
                        if (recordKeyLookupResult.Value == null || string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid))
                        {
                            missingGuids.Add(splitKeys[1]);
                        }
                        else
                        {
                            sectionGuidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
                if (missingGuids != null && missingGuids.Any())
                {
                    var repositoryException = new RepositoryException();
                    foreach (var csKey in missingGuids)
                    {
                        repositoryException.AddError(new RepositoryError("section.id", string.Format("The guid is missing for COURSE.SECTIONS ID: '{0}'", csKey)));
                    }
                    throw repositoryException;
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while getting course section guids.", ex); ;
            }

            return sectionGuidCollection;
        }

        /// <summary>
        /// Get the GUID for a section meeting using its ID
        /// </summary>
        /// <param name="id">Section meeting ID</param>
        /// <returns>Section meeting GUID</returns>
        public async Task<string> GetSectionMeetingGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("COURSE.SEC.MEETING", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("SectionMeeting.Guid.NotFound", "GUID not found for section meeting " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get the GUID for a section instructors using its ID
        /// </summary>
        /// <param name="id">Section instructors ID</param>
        /// <returns>Section instructors GUID</returns>
        private async Task<string> GetSectionFacultyGuidFromIdAsync(string id)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync("COURSE.SEC.FACULTY", id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("SectionInstructors.Guid.NotFound", "GUID not found for section instructors " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetSectionIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Section GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Section GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "COURSE.SECTIONS")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, COURSE.SECTIONS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetCourseIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Course GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Course GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "COURSES")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, COURSES");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single section using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section</returns>
        public async Task<Section> GetSectionByGuidAsync(string guid)
        {
            return await GetSectionAsync(await GetSectionIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Get a single section using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section</returns>
        public async Task<Section> GetSectionByGuid2Async(string guid, bool addToErrorCollection = false)
        {
            this.addToErrorCollection = addToErrorCollection;

            var section = await GetSectionByGuidAsync(guid);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return section;
        }

        /// <summary>
        /// Get a list of sections using criteria
        /// </summary>
        /// <returns>A list of sections Entities</returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "",
            string academicLevel = "", string course = "", string location = "", string status = "", string department = "",
            string subject = "", string instructor = "")
        {
            var secAcadLevels = new List<string>();
            var secDepartments = new List<string>();
            if (!string.IsNullOrEmpty(academicLevel)) secAcadLevels.Add(academicLevel);
            if (!string.IsNullOrEmpty(department)) secDepartments.Add(department);

            return await GetSectionsAsync(offset, limit, title, startDate, endDate, code, number, learningProvider, termId,
                secAcadLevels, course, location, status, secDepartments, subject, instructor);
        }

        /// <summary>
        /// Get a list of sections using criteria
        /// </summary>
        /// <returns>A list of sections Entities</returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", string instructor = "")
        {
            var instructors = new List<string>();
            var reportingTermId = "";
            if (!string.IsNullOrEmpty(instructor)) instructors.Add(instructor);

            return await GetSectionsAsync(offset, limit, title, startDate, endDate, code, number, learningProvider, termId, reportingTermId,
                academicLevels, course, location, status, departments, subject, instructors);
        }

        /// <summary>
        /// Get a list of sections using criteria
        /// </summary>
        /// <returns>A list of sections Entities</returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", List<string> instructors = null, string scheduleTermId = "")
        {
            IEnumerable<Section> sections = new List<Section>();
            int totalCount = 0;
            string[] subList = null;
           
            string sectionsCacheKey = CacheSupport.BuildCacheKey(AllSectionsCache, title, startDate, endDate, code, number, learningProvider, termId,
                reportingTermId, academicLevels, course, location, status, departments, subject, instructors, scheduleTermId);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                   this,
                   ContainsKey,
                   GetOrAddToCacheAsync,
                   AddOrUpdateCacheAsync,
                   transactionInvoker,
                   sectionsCacheKey,
                   "COURSE.SECTIONS",
                   offset,
                   limit,
                   SectionMeetingCacheTimeout,
                   async () =>
                   {
                       var keys = new List<string>();        
                       var sectionTuple = await GetSectionsForFiltersAsync(title, startDate, endDate, code, number, learningProvider, termId, reportingTermId, 
                                                                    academicLevels, course, location, status, departments, subject, instructors, scheduleTermId);
                       if (sectionTuple != null)
                       {                          
                           return sectionTuple;
                       }
                       return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                   }
            );

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<Section>, int>(sections, 0);
            }

            subList = keyCacheObject.Sublist.ToArray();
            totalCount = keyCacheObject.TotalCount.Value;
            var sectionData = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", subList);

            sections = await BuildNonCachedSectionsAsync(sectionData.ToList());

            return new Tuple<IEnumerable<Section>, int>(sections, totalCount);
        }

        /// <summary>
        /// Get a list of sections using criteria
        /// </summary>
        /// <returns>A list of sections Entities</returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSections2Async(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", List<string> instructors = null, string scheduleTermId = "", bool addToCollection = false)
        {
            IEnumerable<Section> sections = new List<Section>();
            int totalCount = 0;
            string[] subList = null;

            this.addToErrorCollection = addToCollection;

            string sectionsCacheKey = CacheSupport.BuildCacheKey(AllSectionsCache, title, startDate, endDate, code, number, learningProvider, termId,
                reportingTermId, academicLevels, course, location, status, departments, subject, instructors, scheduleTermId);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    sectionsCacheKey,
                    "COURSE.SECTIONS",
                    offset,
                    limit,
                    SectionMeetingCacheTimeout,
                    async () =>
                    {
                        var keys = new List<string>();

                        var sectionTuple = await GetSectionsForFiltersAsync(title, startDate, endDate, code, number, learningProvider, termId, reportingTermId,
                                                                        academicLevels, course, location, status, departments, subject, instructors, scheduleTermId);
                        if (sectionTuple != null)
                        {                            
                            return sectionTuple;
                        }
                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    }
            );

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<Section>, int>(sections, 0);
            }

            subList = keyCacheObject.Sublist.ToArray();
            totalCount = keyCacheObject.TotalCount.Value;
            var sectionData = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", subList);

            sections = await BuildNonCachedSectionsAsync(sectionData.ToList());
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<Section>, int>(sections, totalCount);
        }

        private async Task<CacheSupport.KeyCacheRequirements> GetSectionsForFiltersAsync(string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", List<string> instructors = null, string scheduleTermId = "")
        { 
            string[] limitingKeys = null;
            string criteria = "";
          
            // If we have a course, then select the limited list from the COURSES record first
            if (!string.IsNullOrEmpty(course))
            {
                limitingKeys = await DataReader.SelectAsync("COURSES", new string[] { course }, "BY.EXP CRS.SECTIONS SAVING CRS.SECTIONS");
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
            }
            // Process all indexes first, starting with term
            if (!string.IsNullOrEmpty(termId))
            {
                if (!string.IsNullOrEmpty(course) && !string.IsNullOrEmpty(number))
                {
                    if (criteria != "")
                    {
                        criteria += " AND ";
                    }
                    // index contains SEC.COURSE:SEC.TERM:SEC.NO
                    criteria += "WITH SECTION.TERM.INDEX EQ '" + course + termId + number + "'";
                }
                else
                {
                    if (criteria != "")
                    {
                        criteria += " AND ";
                    }
                    // index on SEC.TERM
                    criteria += "WITH SEC.TERM EQ '" + termId + "'";
                }
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            if (!string.IsNullOrEmpty(scheduleTermId))
            {
                if (!string.IsNullOrEmpty(course) && !string.IsNullOrEmpty(number))
                {
                    if (criteria != "")
                    {
                        criteria += " AND ";
                    }
                    // index contains SEC.COURSE:SEC.TERM:SEC.NO
                    criteria += "WITH SECTION.TERM.INDEX EQ '" + course + scheduleTermId + number + "'";
                }
                else
                {
                    if (criteria != "")
                    {
                        criteria += " AND ";
                    }
                    // index on SEC.TERM
                    criteria += "WITH SEC.TERM EQ '" + scheduleTermId + "'";
                }
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = criteria, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            if (!string.IsNullOrEmpty(subject))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                // index on SEC.SUBJECT
                criteria += "WITH SEC.SUBJECT EQ '" + subject + "'";
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            // May have already selected off course, term, and number
            if (!string.IsNullOrEmpty(number) && (string.IsNullOrEmpty(termId) || string.IsNullOrEmpty(course)))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                // index on SEC.NO
                criteria += "WITH SEC.NO EQ '" + number + "'";
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            if (!string.IsNullOrEmpty(code))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                // index on SEC.NAME but using LIKE here
                criteria += string.Concat("WITH SEC.NAME LIKE ", '"', "...'", code, "'...", '"');
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            if (startDate != "")
            {
                // startDate was converted to unidata format in the service
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                // index on SEC.START.DATE
                criteria += "WITH SEC.START.DATE NE '' AND SEC.START.DATE GE '" + startDate + "'";
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                }
                criteria = "";
            }
            if (!string.IsNullOrEmpty(reportingTermId))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH SEC.REPORTING.TERM EQ '" + reportingTermId + "'";
            }
            if (!string.IsNullOrEmpty(location))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH SEC.LOCATION EQ '" + location + "'";
            }
            if (!string.IsNullOrEmpty(learningProvider))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH SEC.LEARNING.PROVIDER EQ '" + learningProvider + "'";
            }
            if (!string.IsNullOrEmpty(status))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH SEC.CURRENT.STATUS EQ " + status;
            }
            if (!string.IsNullOrEmpty(title))
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += string.Concat("WITH SEC.SHORT.TITLE LIKE ", '"', "...'", title, "'...", '"');
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                // endDate was converted to unidata format in the service
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                criteria += "WITH SEC.END.DATE NE '' AND SEC.END.DATE LE '" + endDate + "'";
            }
            if (academicLevels != null && academicLevels.Any())
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                int i = 0;
                foreach (var academicLevel in academicLevels)
                {
                    if (i == 0)
                        criteria += "WITH ";
                    else
                        criteria += " AND ";
                    criteria += "SEC.ACAD.LEVEL EQ '" + academicLevel + "'";
                    i++;
                }
            }
            if (departments != null && departments.Any())
            {
                if (criteria != "")
                {
                    criteria += " AND ";
                }
                int i = 0;
                foreach (var department in departments)
                {
                    if (i == 0)
                        criteria += "WITH ";
                    else
                        criteria += " AND ";
                    criteria += "SEC.DEPTS EQ '" + department + "'";
                    i++;
                }
            }

            //Apply instructors filter
            if (instructors != null && instructors.Any())
            {
                var instructor = "";
                foreach (var instr in instructors)
                {
                    if (!string.IsNullOrEmpty(instr))
                    {
                        instructor += "'" + instr + "'";
                    }
                }
                if (!string.IsNullOrEmpty(instructor))
                {
                    string[] instructorKeys = null;
                    if (!string.IsNullOrEmpty(criteria) || limitingKeys != null && limitingKeys.Any())
                    {
                        if (criteria != "")
                        {
                            criteria += " ";
                        }
                        criteria += "WITH SEC.FACULTY BY.EXP SEC.FACULTY SAVING SEC.FACULTY";
                        instructorKeys = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
                        if (instructorKeys == null || !instructorKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                        }
                        criteria = "";
                    }
                    var instructorCriteria = "WITH CSF.FACULTY = " + instructor;
                    if (instructorKeys != null && instructorKeys.Any()) instructorKeys = instructorKeys.Distinct().ToArray();
                    var courseSecFacultyIds = await DataReader.SelectAsync("COURSE.SEC.FACULTY", instructorKeys, instructorCriteria);
                    if (courseSecFacultyIds == null || !courseSecFacultyIds.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                    }
                    // build list of COURSE.SEC.FACULTY records with all instructors assigned
                    // so, if we have 2 or more instructors in the filter, then only sections taught
                    // by ALL of these instructors would be returned.
                    if (instructors.Count() > 1)
                    {
                        var facultyDict = new Dictionary<string, List<string>>();
                        var secFacultyData = await DataReader.BulkReadRecordAsync<CourseSecFaculty>(courseSecFacultyIds);
                        instructors.ForEach(instrId =>
                            facultyDict.Add(instrId, secFacultyData.Where(sfd =>
                                sfd.CsfFaculty == instrId).Select(sv => sv.CsfCourseSection).ToList()));

                        limitingKeys = facultyDict.Values.ElementAt(0).ToArray();
                        facultyDict.ForEach(fd =>
                            limitingKeys = limitingKeys.Intersect(fd.Value).ToArray());

                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                        }
                    }
                    else
                    {
                        limitingKeys = await DataReader.SelectAsync("COURSE.SEC.FACULTY", courseSecFacultyIds, "SAVING UNIQUE CSF.COURSE.SECTION");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { criteria = string.Empty, limitingKeys = new List<string>(), NoQualifyingRecords = true };
                        }
                    }
                }
            }

            //execute existing criteria to limit potential sections
            //sectionIds = await DataReader.SelectAsync("COURSE.SECTIONS", limitingKeys, criteria);
            return new CacheSupport.KeyCacheRequirements() { criteria = criteria, limitingKeys = limitingKeys != null && limitingKeys.Any()? limitingKeys.ToList() : null, NoQualifyingRecords = false };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="searchable"></param>
        /// <param name="addToCollection"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchable1Async(int offset, int limit,
            string searchable, bool addToCollection = false)
        {
            this.addToErrorCollection = addToCollection;
            var sections = await GetSectionsSearchableAsync(offset, limit, searchable);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return sections;
        }

        /// <summary>        
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="searchable">Check if a section is searchable or hidden.  Required.</param>
        /// <returns>IEnumerable Sections domain entity</returns>  
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchableAsync(int offset, int limit,
        string searchable)
        {

            IEnumerable<Section> sections = new List<Section>();
            string criteria = "";
            var totalCount = 0;
            List<string> sectionIds = new List<string>();

            if (string.IsNullOrWhiteSpace(searchable))
            {
                throw new ArgumentNullException("searchable is a required field.");
            }
            var stwebDefaults = await GetStwebDefaultsAsync();
            if (stwebDefaults == null)
            {
                throw new Exception("Unable to access STWEB.DEFAULTS values.");

            }
            var stwebRegTermsAllowed = string.Empty;
            if (stwebDefaults != null && stwebDefaults.StwebRegTermsAllowed != null)
            {
                foreach (var webTerm in stwebDefaults.StwebRegTermsAllowed)
                {
                    stwebRegTermsAllowed = string.Concat(stwebRegTermsAllowed, "'", webTerm, "'");
                }
            }

            //get all course types to view special processing
            var courseTypes = await GetGuidValcodeAsync<CourseType>("ST", "COURSE.TYPES",
                    (courseType, g) => new CourseType(g, courseType.ValInternalCodeAssocMember, courseType.ValExternalRepresentationAssocMember, courseType.ValActionCode2AssocMember == "N" ? false : true) { Categorization = courseType.ValActionCode1AssocMember });

            switch (searchable.ToLower())
            {
                case ("yes"):
                    {
                        // Use index instead.  Index is based off special processing.  1=Active, 2=Cancelled
                        //criteria = "WITH SEC.CURRENT.STATUS EQ 'A' AND WITH SEC.HIDE.IN.CATALOG NE 'Y'";
                        criteria = "WITH SEC.CRNT.STATUS.INDEX EQ '1' AND WITH SEC.HIDE.IN.CATALOG NE 'Y'";

                        if (stwebRegTermsAllowed != null && !string.IsNullOrEmpty(stwebRegTermsAllowed))
                        {
                            criteria += " AND WITH SEC.TERM EQ " + stwebRegTermsAllowed;
                        }

                        var hiddenCourseTypes = courseTypes.Where(y => y.Categorization.ToUpper() == "P" || y.ShowInCourseSearch == false).Select(x => x.Code);
                        var hiddenCourseTypesSelect = string.Empty;
                        foreach (var type in hiddenCourseTypes)
                        {
                            hiddenCourseTypesSelect = string.Concat(hiddenCourseTypesSelect, "'", type, "'");
                        }

                        if (hiddenCourseTypes != null && hiddenCourseTypes.Any())
                        {
                            criteria += " AND WITH EVERY SEC.COURSE.TYPES NE " + hiddenCourseTypesSelect;
                        }

                        var ldmdRegUsersId = await GetLdmdRegUsersIdAsync();
                        if (string.IsNullOrWhiteSpace(ldmdRegUsersId))
                        {
                            throw new Exception("Registration Users ID is required for Ethos Integration.");
                        }

                        var regControlsIds = await GetRegControlsIdsAsync(new List<string> { ldmdRegUsersId });

                        var regControls = await DataReader.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true);
                        if ((regControls != null) && (regControlsIds != null) && (!string.IsNullOrWhiteSpace(ldmdRegUsersId)))
                        {

                            if (regControlsIds.ContainsKey(ldmdRegUsersId))
                            {
                                var regUserRegControls = regControls.FirstOrDefault(rc => rc.Recordkey == regControlsIds[ldmdRegUsersId]);

                                if ((regUserRegControls != null) && (regUserRegControls.RgcSectionLookupCriteria != null) && (regUserRegControls.RgcSectionLookupCriteria.Any()))
                                {
                                    var sectionLookupCriteria = string.Join(" ", regUserRegControls.RgcSectionLookupCriteria);
                                    sectionLookupCriteria = sectionLookupCriteria.Replace(@"\""", "'");

                                    logger.Info("Sections searchable lookup criteria: COURSE.SECTIONS: " + sectionLookupCriteria + " AND " + criteria);
                                    sectionIds = (await DataReader.SelectAsync("COURSE.SECTIONS", sectionLookupCriteria + " AND " + criteria)).ToList();
                                }
                            }
                        }
                        if (!sectionIds.Any())
                        {
                            sectionIds = (await DataReader.SelectAsync("COURSE.SECTIONS", criteria)).ToList();
                            logger.Info("Sections searchable criteria (yes): COURSE.SECTIONS: " + criteria);
                        }

                        break;
                    }
                case ("no"):
                    {
                        // Use index instead.  Index is based off special processing.  1=Active, 2=Cancelled
                        //criteria = "WITH SEC.HIDE.IN.CATALOG EQ 'Y' AND WITH SEC.CURRENT.STATUS NE 'C'";
                        criteria = "WITH SEC.CRNT.STATUS.INDEX NE '2' AND WITH SEC.HIDE.IN.CATALOG EQ 'Y'";

                        if (stwebRegTermsAllowed != null && !string.IsNullOrEmpty(stwebRegTermsAllowed))
                        {
                            criteria += " OR WITH SEC.CRNT.STATUS.INDEX NE '2' AND WITH SEC.TERM NE " + stwebRegTermsAllowed;
                        }

                        var hiddenCourseTypes = courseTypes.Where(y => y.Categorization.ToUpper() == "P" || y.ShowInCourseSearch == false).Select(x => x.Code);
                        var hiddenCourseTypesSelect = string.Empty;
                        foreach (var type in hiddenCourseTypes)
                        {
                            hiddenCourseTypesSelect = string.Concat(hiddenCourseTypesSelect, "'", type, "'");
                        }

                        if (hiddenCourseTypes != null && hiddenCourseTypes.Any())
                        {
                            criteria += " OR WITH SEC.CRNT.STATUS.INDEX NE '2' AND WITH SEC.COURSE.TYPES EQ " + hiddenCourseTypesSelect;
                        }

                        var ldmdRegUsersId = await GetLdmdRegUsersIdAsync();
                        if (string.IsNullOrWhiteSpace(ldmdRegUsersId))
                        {
                            throw new Exception("Registration Users ID is required for Ethos Integration.");
                        }
                        var regControlsIds = await GetRegControlsIdsAsync(new List<string> { ldmdRegUsersId });

                        var regControls = await DataReader.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true);
                        if ((regControls != null) && (regControlsIds != null) && (!string.IsNullOrWhiteSpace(ldmdRegUsersId)))
                        {

                            if (regControlsIds.ContainsKey(ldmdRegUsersId))
                            {
                                var regUserRegControls = regControls.FirstOrDefault(rc => rc.Recordkey == regControlsIds[ldmdRegUsersId]);
                                if ((regUserRegControls != null) && (regUserRegControls.RgcSectionLookupCriteria != null) && (regUserRegControls.RgcSectionLookupCriteria.Any()))
                                {
                                    var sectionLookupCriteria = string.Join(" ", regUserRegControls.RgcSectionLookupCriteria);
                                    sectionLookupCriteria = sectionLookupCriteria.Replace(@"\""", "'");
                                    logger.Info("Sections searchable lookup criteria: COURSE.SECTIONS: " + sectionLookupCriteria);
                                    var visibleCourseSectionIds = await DataReader.SelectAsync("COURSE.SECTIONS", sectionLookupCriteria);

                                    var allCourseSectionsIds = await DataReader.SelectAsync("COURSE.SECTIONS", criteria);
                                    sectionIds = (allCourseSectionsIds.Except(visibleCourseSectionIds)).ToList();

                                }
                            }
                        }

                        if (!sectionIds.Any())
                        {
                            sectionIds = (await DataReader.SelectAsync("COURSE.SECTIONS", criteria)).ToList();
                            logger.Info("Sections searchable criteria (no): COURSE.SECTIONS: " + criteria);
                        }

                        break;
                    }
                case ("hidden"):
                    {
                        criteria = "WITH SEC.HIDE.IN.CATALOG EQ 'Y' ";

                        var hiddenCourseTypes = courseTypes.Where(y => y.Categorization.ToUpper() == "P" || y.ShowInCourseSearch == false).Select(x => x.Code);
                        var hiddenCourseTypesSelect = string.Empty;
                        foreach (var type in hiddenCourseTypes)
                        {
                            hiddenCourseTypesSelect = string.Concat(hiddenCourseTypesSelect, "'", type, "'");
                        }

                        if (hiddenCourseTypes != null && hiddenCourseTypes.Any())
                        {
                            criteria += " OR WITH SEC.COURSE.TYPES EQ " + hiddenCourseTypesSelect;
                        }

                        var ldmdRegUsersId = await GetLdmdRegUsersIdAsync();
                        if (string.IsNullOrWhiteSpace(ldmdRegUsersId))
                        {
                            throw new Exception("Registration Users ID is required for Ethos Integration.");
                        }
                        var regControlsIds = await GetRegControlsIdsAsync(new List<string> { ldmdRegUsersId });

                        var regControls = await DataReader.BulkReadRecordAsync<RegControls>("REG.CONTROLS", "", true);
                        if ((regControls != null) && (regControlsIds != null) && (!string.IsNullOrWhiteSpace(ldmdRegUsersId)))
                        {

                            if (regControlsIds.ContainsKey(ldmdRegUsersId))
                            {
                                var regUserRegControls = regControls.FirstOrDefault(rc => rc.Recordkey == regControlsIds[ldmdRegUsersId]);
                                if ((regUserRegControls != null) && (regUserRegControls.RgcSectionLookupCriteria != null) && (regUserRegControls.RgcSectionLookupCriteria.Any()))
                                {
                                    var sectionLookupCriteria = string.Join(" ", regUserRegControls.RgcSectionLookupCriteria);
                                    sectionLookupCriteria = sectionLookupCriteria.Replace(@"\""", "'");
                                    logger.Info("Sections searchable lookup criteria: COURSE.SECTIONS: " + sectionLookupCriteria);
                                    var visibleCourseSectionIds = await DataReader.SelectAsync("COURSE.SECTIONS", sectionLookupCriteria);

                                    var allCourseSectionsIds = await DataReader.SelectAsync("COURSE.SECTIONS", criteria);
                                    sectionIds = (allCourseSectionsIds.Except(visibleCourseSectionIds)).ToList();

                                }
                            }
                        }

                        if (!sectionIds.Any())
                        {
                            sectionIds = (await DataReader.SelectAsync("COURSE.SECTIONS", criteria)).ToList();
                            logger.Info("Sections searchable criteria (hidden): COURSE.SECTIONS: " + criteria);
                        }

                        break;
                    }
                default:
                    break;
            }

            totalCount = sectionIds.Count();

            sectionIds.Sort();

            var subList = sectionIds.Skip(offset).Take(limit).ToArray();
            var bulkData = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", subList);

            var sectionData = new List<CourseSections>();
            sectionData.AddRange(bulkData);

            sections = await BuildNonCachedSectionsAsync(sectionData);

            return new Tuple<IEnumerable<Section>, int>(sections, totalCount);
        }

        private async Task<Dictionary<string, string>> GetRegControlsIdsAsync(IEnumerable<string> ids)
        {
            var regControlsIdsDict = new Dictionary<string, string>();
            // Determine the reg.controls for each user and build registration options object
            foreach (var id in ids)
            {
                // Get cached reg control ID for the given user. If not found, call transaction to get it and store it.
                var regControlsId = await GetOrAddToCacheAsync<string>("RegControlsIdForUser_" + id,
                    async () =>
                    {
                        try
                        {
                            GetRegControlsIdForUserRequest request = new GetRegControlsIdForUserRequest() { InPersonIds = new List<string>() { id } };
                            GetRegControlsIdForUserResponse response = await transactionInvoker.ExecuteAsync<GetRegControlsIdForUserRequest, GetRegControlsIdForUserResponse>(request);
                            return response.PersonRegControls.Where(prc => prc.PersonIds == id).Select(prc => prc.RegControlsIds).First();
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Unable to retrieve reg.controls Id for user " + id + ". Exception: " + ex.Message);
                        }
                        return null;
                    }
                );
                // Add something to the dict for user user, even if null
                regControlsIdsDict[id] = regControlsId;
            }
            return regControlsIdsDict;
        }

        /// <summary>
        /// Get LdmdRegUsersId from LDM.DEFAULTS
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetLdmdRegUsersIdAsync()
        {
            var ldmdRegUsersId = string.Empty;
            var ldmDefaults = await DataReader.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");

            if (!string.IsNullOrEmpty(ldmDefaults.LdmdRegUsersId))
            {
                ldmdRegUsersId = ldmDefaults.LdmdRegUsersId;
            }


            return ldmdRegUsersId;
        }

        /// <summary>
        /// EEDM method to catch all errors for standards.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="keyword"></param>
        /// <param name="bypassCache"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="addToCollection"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeyword1Async(int offset, int limit, string keyword, bool bypassCache = false, bool caseSensitive = false, 
            bool addToCollection = false)
        {
            this.addToErrorCollection = addToCollection;
            var sections = await GetSectionsKeywordAsync(offset, limit, keyword, bypassCache);
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return sections;
        }
        /// <summary>        
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="keyword">The string to search for.  Required.</param>
        /// <param name="bypassCache">use cache</param>
        /// <param name="caseSensitive">case sensative search</param>
        /// <returns>IEnumerable Sections domain entity</returns>
        public async Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeywordAsync(int offset, int limit,
            string keyword, bool bypassCache = false, bool caseSensitive = false)
        {
            IEnumerable<Section> sections = new List<Section>();
            string criteria = "";

            int totalCount = 0;

            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentNullException("Must provide a keyword for section search.");
            }
            keyword = caseSensitive ? keyword : keyword.ToLower();

            List<CourseSections> courseSectionRecords = null;
            if (!bypassCache)
            {
                courseSectionRecords = await GetOrAddToCacheAsync<List<CourseSections>>("AllCourseSections",
                    async () =>
                    {
                        var courseSectionKeys = await DataReader.SelectAsync("COURSE.SECTIONS", criteria);
                        //bulkread the records for all the keys
                        var courseSections = new List<CourseSections>();
                        for (var i = 0; i < courseSectionKeys.Count(); i += readSize)
                        {
                            var courseSubList = courseSectionKeys.Skip(i).Take(readSize);
                            var records = await DataReader.BulkReadRecordAsync<CourseSections>(courseSubList.ToArray());
                            if (records != null)
                            {
                                courseSections.AddRange(records);
                            }
                        }
                        return courseSections;
                    }, Level1CacheTimeoutValue);
            }
            else
            {
                var courseSectionKeys = await DataReader.SelectAsync("COURSE.SECTIONS", criteria);
                courseSectionRecords = new List<CourseSections>();
                for (var i = 0; i < courseSectionKeys.Count(); i += readSize)
                {
                    var courseSubList = courseSectionKeys.Skip(i).Take(readSize);
                    var records = await DataReader.BulkReadRecordAsync<CourseSections>(courseSubList.ToArray());
                    if (records != null)
                    {
                        courseSectionRecords.AddRange(records);
                    }
                }
                await AddOrUpdateCacheAsync<List<CourseSections>>("AllCourseSections", courseSectionRecords, Level1CacheTimeoutValue);
            }

            List<Courses> courseRecords = null;
            if (!bypassCache)
            {
                courseRecords = await GetOrAddToCacheAsync<List<Courses>>("AllCourses",
                    async () =>
                    {
                        var courses = new List<Courses>();
                        var courseKeys = courseSectionRecords.Select(c => c.SecCourse).Distinct().ToArray();

                        for (var i = 0; i < courseKeys.Count(); i += readSize)
                        {
                            var courseList = courseKeys.Skip(i).Take(readSize);
                            var records = await DataReader.BulkReadRecordAsync<Courses>(courseList.ToArray());
                            if (records != null)
                            {
                                courses.AddRange(records);
                            }
                        }
                        return courses;
                    }, Level1CacheTimeoutValue);
            }
            else
            {
                courseRecords = new List<Courses>();
                var courseKeys = courseSectionRecords.Select(c => c.SecCourse).Distinct().ToArray();

                for (var i = 0; i < courseKeys.Count(); i += readSize)
                {
                    var courseList = courseKeys.Skip(i).Take(readSize);
                    var records = await DataReader.BulkReadRecordAsync<Courses>(courseList.ToArray());
                    if (records != null)
                    {
                        courseRecords.AddRange(records);
                    }
                }
                await AddOrUpdateCacheAsync<List<Courses>>("AllCourses", courseRecords, Level1CacheTimeoutValue);

            }

            var departments = await GetGuidCodeItemAsync<Depts, Department>("AllSectionDepartments", "DEPTS",
                (d, g) => new Department(g, d.Recordkey, d.DeptsDesc, d.DeptsActiveFlag == "A"),
                 CacheTimeout, this.DataReader.IsAnonymous, bypassCache);

            var locations = await GetGuidCodeItemAsync<Locations, Location>("AllSectionLocations", "LOCATIONS",
                (l, g) => new Location(g, l.Recordkey, l.LocDesc, null, null, null, null, string.Empty, null, l.LocHideInSsCourseSearch.ToUpperInvariant() == "Y"),
                CacheTimeout, this.DataReader.IsAnonymous, bypassCache);

            var subjects = await GetGuidCodeItemAsync<Subjects, Subject>("AllSectionSubjects", "SUBJECTS",
                (s, g) => new Subject(g, s.Recordkey, s.SubjDesc, (s.SubjSelfServCourseCatlg == "Y" ? true : false)),
                CacheTimeout, this.DataReader.IsAnonymous, bypassCache);


            Dictionary<string, string> keywordSectionDict = null;
            if (!bypassCache)
            {
                keywordSectionDict = GetOrAddToCache<Dictionary<string, string>>("AllSectionsKeyword" + keyword,
                    () => PopulateSectionsKeyword(courseSectionRecords, courseRecords, departments, locations, subjects, caseSensitive));
            }
            else
            {
                keywordSectionDict = PopulateSectionsKeyword(courseSectionRecords, courseRecords, departments, locations, subjects, caseSensitive);
                await AddOrUpdateCacheAsync<Dictionary<string, string>>("AllSectionsKeyword" + keyword, keywordSectionDict, Level1CacheTimeoutValue);
            }

            var sectionIds = new List<string>();
            foreach (var item in keywordSectionDict)
            {
                if (item.Value.Contains(keyword))
                {
                    sectionIds.Add(item.Key);
                }
            }

            totalCount = sectionIds.Count();
            sectionIds.Sort();
            var subList = sectionIds.Skip(offset).Take(limit).ToArray();
            var bulkData = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", subList);

            var sectionData = new List<CourseSections>();
            sectionData.AddRange(bulkData);

            sections = await BuildNonCachedSectionsAsync(sectionData);

            return new Tuple<IEnumerable<Section>, int>(sections, totalCount);
        }

        private static Dictionary<string, string> PopulateSectionsKeyword(IEnumerable<CourseSections> courseSectionRecords,
            List<Courses> courseRecords, IEnumerable<Department> departments, IEnumerable<Location> locations,
            IEnumerable<Subject> subjects, bool caseSensitive = false)
        {
            var keywordSections = new Dictionary<string, string>();
            foreach (var courseSection in courseSectionRecords)
            {
                var sb = new StringBuilder();
                if (courseRecords != null && courseRecords.Any())
                {
                    var course = courseRecords.FirstOrDefault(cs => cs.Recordkey == courseSection.SecCourse);
                    if (course != null)
                    {
                        //CRS.NAME ( subject and course number with varying delimiters, ie math101, math-101, math 101) 
                        if (!string.IsNullOrEmpty(course.CrsName))
                        {
                            sb.Append(course.CrsName);
                            sb.Append(course.CrsName.Replace(" ", ""));
                            sb.Append(course.CrsName.Replace("-", ""));
                        }

                        if (!string.IsNullOrEmpty(course.CrsTitle))
                        {
                            sb.Append(course.CrsTitle); //CRS.TITLE 
                        }
                        if (!string.IsNullOrEmpty(course.CrsShortTitle))
                        {
                            sb.Append(course.CrsShortTitle); //CRS.SHORT.TITLE
                        }
                        if (!string.IsNullOrEmpty(course.CrsSubject))
                        {
                            sb.Append(course.CrsSubject); //CRS.SUBJECT  
                            if (subjects != null && subjects.Any())
                            {
                                var subject = subjects.FirstOrDefault(sub => sub.Code == course.CrsSubject);
                                if (subject != null)
                                {
                                    sb.Append(subject.Description); //(SUBJ.DESC)  
                                }
                            }
                        }

                        if (course.CrsDepts != null && course.CrsDepts.Any())
                        {
                            sb.Append(string.Join("", course.CrsDepts)); //CRS.DEPTS  
                            if (departments != null && departments.Any())
                            {
                                foreach (var courseDept in course.CrsDepts)
                                {
                                    var itemDepartment = departments.FirstOrDefault(dept => dept.Code == courseDept);
                                    if (itemDepartment != null)
                                    {
                                        sb.Append(itemDepartment.Description); //(DEPTS.DESC) 
                                    }
                                }
                            }
                        }

                        if (course.CrsLocations != null && course.CrsLocations.Any())
                        {
                            sb.Append(string.Join("", course.CrsLocations)); //CRS.LOCATIONS  
                            if (locations != null && locations.Any())
                            {
                                foreach (var courseLoc in course.CrsLocations)
                                {
                                    var courseLocation = locations.FirstOrDefault(dept => dept.Code == courseLoc);
                                    if (courseLocation != null)
                                    {
                                        sb.Append(courseLocation.Description); //LOC.DESC  
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(course.CrsDesc))
                        {
                            sb.Append(course.CrsDesc); //CRS.DESC
                        }
                    }
                }
                //SEC.NAME (subject/course/section number with varying delimiters, ie math10101, math-101-01, math 101 01)

                if (!string.IsNullOrEmpty(courseSection.SecName))
                {
                    sb.Append(courseSection.SecName);
                    sb.Append(courseSection.SecName.Replace(" ", ""));
                    sb.Append(courseSection.SecName.Replace("-", ""));
                }

                //SEC.LONG.TITLE  
                //TODO - couldnt find this in ABOWT

                if (!string.IsNullOrEmpty(courseSection.SecShortTitle))
                {
                    sb.Append(courseSection.SecShortTitle); //SEC.SHORT.TITLE 
                }

                if (courseSection.SecDepts != null && courseSection.SecDepts.Any())
                {
                    sb.Append(string.Join("", courseSection.SecDepts)); //SEC.DEPTS 

                    if (departments != null && departments.Any())
                    {
                        foreach (var secDept in courseSection.SecDepts) //(DEPTS.DESC) 
                        {
                            var itemDepartment = departments.FirstOrDefault(dept => dept.Code == secDept);
                            if (itemDepartment != null)
                            {
                                sb.Append(itemDepartment.Description);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(courseSection.SecLocation))
                {
                    sb.Append(courseSection.SecLocation); //SEC.LOCATION
                    if (locations != null && locations.Any())
                    {
                        var itemLocation = locations.FirstOrDefault(loc => loc.Code == courseSection.SecLocation);
                        if (itemLocation != null)
                        {
                            sb.Append(itemLocation.Description); //(LOC.DESC) 
                        }
                    }
                }

                keywordSections.Add(courseSection.Recordkey, caseSensitive ? sb.ToString() : sb.ToString().ToLower());
            }
            return keywordSections;
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        public async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await InternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        /// <summary>
        /// Post a single section
        /// </summary>
        /// <param name="section">The section</param>
        /// <returns>The created/updated section</returns>
        public async Task<Section> PostSectionAsync(Section section)
        {
            return await UpdateAsync(section);
        }

        /// <summary>
        /// Put a single section
        /// </summary>
        /// <param name="section">The section</param>
        /// <returns>The created/updated section</returns>
        public async Task<Section> PutSectionAsync(Section section)
        {
            return await UpdateAsync(section);
        }


        /// <summary>
        /// Post a single section
        /// </summary>
        /// <param name="section">The section</param>
        /// <returns>The created/updated section</returns>
        public async Task<Section> PostSection2Async(Section section)
        {
            return await Update2Async(section);
        }

        /// <summary>
        /// Put a single section
        /// </summary>
        /// <param name="section">The section</param>
        /// <returns>The created/updated section</returns>
        public async Task<Section> PutSection2Async(Section section)
        {
            return await Update2Async(section);
        }

        /// <summary>
        /// Add/Update a section
        /// </summary>
        /// <param name="section">The section to add or update</param>
        /// <param name="updateEntity">Indicates whether this is an update to an existing record</param>
        /// <returns>The new/updated section</returns>
        private async Task<Section> UpdateAsync(Section section)
        {
            // Pass the section data down to a Colleague transaction to do the record add
            var request = new UpdateCourseSectionsRequest()
            {
                CourseSectionsId = section.Id,
                SecGuid = section.Guid,
                SecAcadLevel = section.AcademicLevelCode,
                SecAllowAuditFlag = section.AllowAudit,
                SecAllowPassNopassFlag = section.AllowPassNoPass,
                SecAllowWaitlistFlag = section.AllowWaitlist,
                SecCapacity = section.SectionCapacity,
                SecCeus = section.Ceus,
                SecCloseWaitlistFlag = section.WaitlistClosed,
                SecCourse = section.CourseId,
                SecCourseLevels = section.CourseLevelCodes.ToList(),
                SecCourseTypes = section.CourseTypeCodes.ToList(),
                SecCredType = section.CreditTypeCode,
                SecDepartments = section.Departments.Select(x =>
                    new SecDepartments() { SecDepts = x.AcademicDepartmentCode, SecDeptPcts = x.ResponsibilityPercentage }).ToList(),
                SecEndDate = section.EndDate,
                SecFacultyConsentFlag = section.IsInstructorConsentRequired,
                SecGradeScheme = section.GradeSchemeCode,
                SecLocation = section.Location,
                SecMaxCred = section.MaximumCredits,
                SecMinCred = section.MinimumCredits,
                SecNo = section.Number,
                SecNoWeeks = section.NumberOfWeeks,
                SecOnlyPassNopassFlag = section.OnlyPassNoPass,
                SecOvrCensusDates = section.CensusDates,
                SecShortTitle = section.Title,
                SecStartDate = section.StartDate,
                SecStatuses = await BuildSectionStatusesAsync(section),
                SecTerm = section.TermId,
                SecTopicCode = section.TopicCode,
                SecVarCredIncrement = section.VariableCreditIncrement,
                SecLearningProvider = section.LearningProvider,
                SecBillingCred = section.BillingCred,
                SecWaitlistMax = section.WaitlistMaximum,
                SecWaitlistNoDays = section.WaitListNumberOfDays,
            };

            if (section.InstructionalMethods != null && section.InstructionalMethods.Any())
            {
                List<SecContact> secContact = new List<SecContact>();
                foreach (var instructionalMethod in section.InstructionalMethods)
                {
                    secContact.Add(new SecContact()
                    {
                        SecInstrMethods = instructionalMethod

                    });
                }
                request.SecContact = secContact;
            }
            // Get Extended Data names and values
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(request);

            if (response.UpdateCourseSectionWarnings != null && response.UpdateCourseSectionWarnings.Count > 0)
            {
                // Just log the warnings
                foreach (var warning in response.UpdateCourseSectionWarnings)
                {
                    logger.Warn("WARNING - " + warning.WarningCodes + ": " + warning.WarningMessages);
                }
            }

            if (response.UpdateCourseSectionErrors != null && response.UpdateCourseSectionErrors.Count > 0)
            {
                // Register repository errors and throw an exception
                var exception = new RepositoryException("Errors encountered while updating section " + section.Id);
                exception.AddErrors(response.UpdateCourseSectionErrors.ConvertAll(x => (new RepositoryError(x.ErrorCodes, x.ErrorMessages))));
                throw exception;
            }

            return string.IsNullOrEmpty(response.CourseSectionsId) ? null : await GetSectionAsync(response.CourseSectionsId);
        }


        /// <summary>
        /// Add/Update a section
        /// </summary>
        /// <param name="section">The section to add or update</param>
        /// <param name="updateEntity">Indicates whether this is an update to an existing record</param>
        /// <returns>The new/updated section</returns>
        private async Task<Section> Update2Async(Section section)
        {
            // Pass the section data down to a Colleague transaction to do the record add
            var request = new UpdateCourseSectionsRequest()
            {
                CourseSectionsId = section.Id,
                SecGuid = section.Guid,
                SecAcadLevel = section.AcademicLevelCode,
                SecAllowAuditFlag = section.AllowAudit,
                SecAllowPassNopassFlag = section.AllowPassNoPass,
                SecAllowWaitlistFlag = section.AllowWaitlist,
                SecCapacity = section.SectionCapacity,
                SecCeus = section.Ceus,
                SecCloseWaitlistFlag = section.WaitlistClosed,
                SecCourse = section.CourseId,
                SecCourseLevels = section.CourseLevelCodes.ToList(),
                SecCourseTypes = section.CourseTypeCodes.ToList(),
                SecCredType = section.CreditTypeCode,
                SecDepartments = section.Departments.Select(x =>
                    new SecDepartments() { SecDepts = x.AcademicDepartmentCode, SecDeptPcts = x.ResponsibilityPercentage }).ToList(),
                SecEndDate = section.EndDate,
                SecFacultyConsentFlag = section.IsInstructorConsentRequired,
                SecGradeScheme = section.GradeSchemeCode,
                SecLocation = section.Location,
                SecMaxCred = section.MaximumCredits,
                SecMinCred = section.MinimumCredits,
                SecNo = section.Number,
                SecNoWeeks = section.NumberOfWeeks,
                SecOnlyPassNopassFlag = section.OnlyPassNoPass,
                SecOvrCensusDates = section.CensusDates,
                SecShortTitle = section.Title,
                SecStartDate = section.StartDate,
                SecStatuses = await BuildSectionStatusesAsync(section),
                SecTerm = section.TermId,
                SecTopicCode = section.TopicCode,
                SecVarCredIncrement = section.VariableCreditIncrement,
                SecLearningProvider = section.LearningProvider,
                SecBillingCred = section.BillingCred,
                SecWaitlistMax = section.WaitlistMaximum,
                SecWaitlistNoDays = section.WaitListNumberOfDays,
                SecPrintedComments = section.Comments
            };

            if (section.InstructionalContacts != null && section.InstructionalContacts.Any())
            {
                List<SecContact> secContact = new List<SecContact>();
                foreach (var instructionalMethod in section.InstructionalContacts)
                {
                    secContact.Add(new SecContact()
                    {
                        SecInstrMethods = instructionalMethod.InstructionalMethodCode,
                        SecContactHours = instructionalMethod.ContactHours,
                        SecContactMeasures = instructionalMethod.ContactMeasure
                    });
                }
                request.SecContact = secContact;
            }
            if (section.IsCrossListedSection.HasValue)
            {
                request.SecXlistFlag = ((bool)section.IsCrossListedSection) ? "Y" : "N";
            }
            if (!string.IsNullOrEmpty(section.BillingMethod))
            {
                request.SecBillingMethod = section.BillingMethod;
            }
            request.Version = "2";
            // Get Extended Data names and values
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateCourseSectionsRequest, UpdateCourseSectionsResponse>(request);

            if (response.UpdateCourseSectionWarnings != null && response.UpdateCourseSectionWarnings.Count > 0)
            {
                // Just log the warnings
                foreach (var warning in response.UpdateCourseSectionWarnings)
                {
                    logger.Warn("WARNING - " + warning.WarningCodes + ": " + warning.WarningMessages);
                }
            }

            if (response.UpdateCourseSectionErrors != null && response.UpdateCourseSectionErrors.Count > 0)
            {
                // Register repository errors and throw an exception
                var exception = new RepositoryException("Errors encountered while updating section " + section.Id);
                exception.AddErrors(response.UpdateCourseSectionErrors.ConvertAll(x => (new RepositoryError(x.ErrorCodes, x.ErrorMessages))));
                throw exception;
            }

            return string.IsNullOrEmpty(response.CourseSectionsId) ? null : await GetSectionAsync(response.CourseSectionsId);
        }

        private async Task<List<SecStatuses>> BuildSectionStatusesAsync(Section section)
        {
            List<SecStatuses> statuses = new List<SecStatuses>();
            foreach (var status in section.Statuses)
            {
                if (!string.IsNullOrEmpty(status.StatusCode))
                {
                    var newStatus = new SecStatuses() { SecStatus = status.StatusCode, SecStatusDate = status.Date };
                    statuses.Add(newStatus);
                }
                else
                {
                    statuses.Add(new SecStatuses() { SecStatus = await ConvertSectionIntegrationStatusToStatusCodeAsync(status.IntegrationStatus), SecStatusDate = status.Date });
                }
            }

            return statuses;
        }

        public async Task<IEnumerable<Section>> GetRegistrationSectionsAsync(IEnumerable<Term> registrationTerms)
        {
            var sections = await GetRegistrationSectionsBySectionAsync(registrationTerms);
            return sections.Values;
        }

        private async Task<IDictionary<string, List<Section>>> GetRegistrationSectionsByCourseAsync(IEnumerable<Term> registrationTerms)
        {
            var sectionDict = await GetOrAddToCacheAsync<Dictionary<string, List<Section>>>("RegistrationSectionsByCourse",
              async () =>
              {
                  Dictionary<string, List<Section>> courseSections = new Dictionary<string, List<Section>>();
                  IEnumerable<Section> sections = await GetRegistrationSectionsAsync(registrationTerms);
                  foreach (var section in sections)
                  {
                      if (!string.IsNullOrEmpty(section.CourseId))
                      {
                          if (!(courseSections.ContainsKey(section.CourseId)))
                          {
                              courseSections[section.CourseId] = new List<Section>() { section };
                          }
                          else
                          {
                              courseSections[section.CourseId].Add(section);
                          }
                      }
                  }
                  return courseSections;
              }
            );
            return sectionDict;
        }

        private async Task<IDictionary<string, Section>> GetRegistrationSectionsBySectionAsync(IEnumerable<Term> registrationTerms)
        {
            var sectionsDict = await GetOrAddToCacheAsync<Dictionary<string, Section>>(AllRegistrationSectionsCache,
          async () =>
          {
              Dictionary<string, Section> sectionResult = new Dictionary<string, Section>();
              if (registrationTerms != null && registrationTerms.Any())
              {
                  _internationalParameters = await InternationalParametersAsync();
                  Tuple<DateTime, DateTime> retrievedDates = await GetSectionsRetrievalDateRangeAsync(registrationTerms);
                  DateTime earliestDate = retrievedDates.Item1;
                  DateTime latestDate = retrievedDates.Item2;
                  string beginningStartDate = UniDataFormatter.UnidataFormatDate(earliestDate, _internationalParameters.HostShortDateFormat, _internationalParameters.HostDateDelimiter);
                  string endingStartDate = UniDataFormatter.UnidataFormatDate(latestDate, _internationalParameters.HostShortDateFormat, _internationalParameters.HostDateDelimiter);
                  var queryQuotedTermIds = QuoteDelimit(registrationTerms.Select(x => x.Code).Distinct().ToList());
                  string selectCriteria = "WITH SEC.START.DATE GE '" + beginningStartDate + "' AND SEC.START.DATE LE '" + endingStartDate + "'" + " AND SEC.TERM EQ " + queryQuotedTermIds + "''";
                  // Bulk read course sections in chunks
                  string[] sectionIds = await DataReader.SelectAsync("COURSE.SECTIONS", selectCriteria);
                  var sectionIdsArray = String.Join(",", sectionIds);
                  var sectionData = new List<CourseSections>();
                  for (int i = 0; i < sectionIds.Count(); i += readSize)
                  {
                      var subList = sectionIds.Skip(i).Take(readSize).ToArray();
                      var bulkData = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", subList);
                      sectionData.AddRange(bulkData);
                  }
                  sectionData = new List<CourseSections>(sectionData);

                  // Bulk read course section meetings in chunks
                  var meetingIds = sectionData.Where(cs => cs.SecMeeting != null && cs.SecMeeting.Count > 0).SelectMany(sm => sm.SecMeeting).Distinct().ToList();
                  var meetingData = new List<CourseSecMeeting>();
                  for (int i = 0; i < meetingIds.Count(); i += readSize)
                  {
                      var subList = meetingIds.Skip(i).Take(readSize).ToArray();
                      var bulkData = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", subList);
                      meetingData.AddRange(bulkData);
                  }

                  var facultyIds = sectionData.Where(cs => cs.SecFaculty != null && cs.SecFaculty.Count > 0).SelectMany(cs => cs.SecFaculty).Distinct().ToList();
                  Collection<CourseSecFaculty> facultyData = null;
                  if (facultyIds != null && facultyIds.Count > 0)
                  {
                      facultyData = await DataReader.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", facultyIds.ToArray());
                  }
                  if (facultyIds != null && facultyIds.Any() && facultyData == null)
                  {
                      logger.Info("Warning: Unable to get facultyData from COURSE.SEC.FACULTY");
                  }

                  List<string> studentCourseSecIds = new List<string>();
                  studentCourseSecIds = sectionData.Where(cs => cs.SecActiveStudents != null && cs.SecActiveStudents.Count > 0)
                      .SelectMany(cs => cs.SecActiveStudents).Distinct().ToList();

                  var rosterData = await GetStudentCourseSecStudents(studentCourseSecIds);
                  Collection<PortalSites> portalSiteData = await GetPortalSitesAsync(sectionData);
                  Collection<CourseSecXlists> crosslistData = await GetCrossListedSectionsAsync(sectionData);
                  Collection<CourseSecPending> pendingData = await GetPendingSectionsAsync(sectionData.Select(s => s.Recordkey).Distinct().ToList());
                  List<WaitList> waitlistData = await GetWaitListsAsync(queryQuotedTermIds);
                  var requisiteData = await GetRequisitesAsync(sectionData);
                  var regBillingRateData = await GetRegBillingRatesAsync(sectionData);
                  sectionResult = await BuildSectionsAsync(sectionData, meetingData, facultyData, rosterData, portalSiteData, crosslistData, pendingData, waitlistData, requisiteData, regBillingRateData);
              }

              // Before returning, since the section cache is now fresh, add an empty list to the ChangedRegistrationSectionsCache.
              // Every 10 minutes, this will be rebuilt based on the changes since the original cache was built.
              await GetOrAddToCacheAsync<List<Section>>(ChangedRegistrationSectionsCache, () => Task.FromResult(new List<Section>()), changedRegistrationSectionsCacheTimeout);
              // Take the current date/time and convert to Colleague local date/time and cache that Colleague date.
              // Used later to select the sections that have had a status change since this date. 
              // Cached for the same amount of time as the AllRegistrationSectionCache.
              // Cached as a list because apparently caching a lone date is confusing to the caching methods.
              GetOrAddToCache<List<DateTime>>(AllRegistrationSectionsCacheDate, () =>
             {
                 // The API's UTC date.
                 var APIDateTimeOffset = DateTimeOffset.UtcNow;
                 // Convert to Colleague time zone, which may force the date to the day before
                 var colleagueDateTime = APIDateTimeOffset.ToLocalDateTime(colleagueTimeZone);
                 logger.Info("Saved cache date: " + colleagueDateTime.ToShortDateString() + " at " + colleagueDateTime.ToShortTimeString());
                 return new List<DateTime>() { colleagueDateTime };
             }
             );
              return sectionResult;
          });

            // Get activated/cancelled sections (built every 10 minutes) and add to/update the cached sections dict.
            try
            {
                var changedSections = await GetChangedSectionsAsync(registrationTerms);
                // The sections returned by the above method may be new sections or changed sections. Either way,
                // we want the updated information in the section data that is returned.
                foreach (var changedSection in changedSections)
                {
                    sectionsDict[changedSection.Id] = changedSection;
                    //now for all the changedSections if any of the section is primary section and have associated cross-listed sections then modify the primary section meetings on those secondary sections
                    //this is to keep cache up-to-date so that if primary section meeting was changed then all the associated cross-listed sections that have property to carry primary section
                    //Meeting info should also be updated.
                    foreach (var secondarySection in changedSection.CrossListedSections)
                    {
                        sectionsDict[secondarySection.Id] = secondarySection;
                    }

                }
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error("Error occurred while getting changed sections");
                    logger.Error(ex.Message);
                    throw ex;
                }
            }

            return sectionsDict;
        }

        private async Task<Tuple<DateTime, DateTime>> GetSectionsRetrievalDateRangeAsync(IEnumerable<Term> registrationTerms)
        {
            try
            {
                if (registrationTerms == null && !registrationTerms.Any())
                {
                    throw new ArgumentNullException("registrationTerms cannot be null or empty");
                }
                DateTime earliestDate = registrationTerms.Min(t => t.StartDate);
                DateTime latestDate = registrationTerms.Max(t => t.EndDate);
                DateTime startDate = earliestDate;
                DateTime endDate = latestDate;

                logger.Info("sections will be retrieved with broader range of dates by reading STWEB.REG.START.DATE and STWEB.REG.END.DATE fields on RGWP");

                StwebDefaults webDefaults = await GetStwebDefaultsAsync();
                if (webDefaults.StwebRegStartDate.HasValue)
                {
                    if (webDefaults.StwebRegStartDate.Value < earliestDate)
                    {
                        startDate = webDefaults.StwebRegStartDate.Value;
                    }

                }
                if (webDefaults.StwebRegEndDate.HasValue)
                {
                    if (webDefaults.StwebRegEndDate.Value > latestDate)
                    {
                        endDate = webDefaults.StwebRegEndDate.Value;
                    }
                }

                if (logger.IsInfoEnabled)
                {
                    logger.Info(string.Format("Range of dates extracted for sections retrieval from terms are : EarliestDate- {0}  LatestDate- {1}", earliestDate.ToString(), latestDate.ToString()));

                    logger.Info(string.Format("Range of dates extracted for sections retrieval are : StartDate- {0}  EndDate- {1}", startDate.ToString(), endDate.ToString()));
                }
                return new Tuple<DateTime, DateTime>(startDate, endDate);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        private async Task<Collection<AcadReqmts>> GetRequisitesAsync(List<CourseSections> sectionData)
        {
            // Get all acad reqmts specified in SecReq (section requirement codes) -- if converted
            var requisiteData = new Collection<AcadReqmts>();
            if (await RequisitesConvertedAsync())
            {
                var reqIds = sectionData.SelectMany(s => s.SecReqs).Distinct().ToList();
                if (reqIds != null && reqIds.Count > 0)
                {
                    requisiteData = await DataReader.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqIds.ToArray());
                }
            }
            return requisiteData;
        }

        private async Task<List<WaitList>> GetWaitListsAsync(string queryQuotedTermIds)
        {
            // Get all waitlist items for the registration terms AND those WAIT.LIST items with no term.       
            var query = "WAIT.COURSE.SECTION NE '' AND WAIT.TERM EQ " + queryQuotedTermIds + "''";
            Collection<WaitList> waitlistBulkData = await DataReader.BulkReadRecordAsync<WaitList>("WAIT.LIST", query);
            List<WaitList> waitlistData = new List<WaitList>();
            if (waitlistBulkData != null)
            {
                waitlistData.AddRange(waitlistBulkData);
            }
            return waitlistData;
        }

        public async Task<Tuple<IEnumerable<StudentSectionWaitlist>, int>> GetWaitlistsAsync(int offset, int limit)
        {

            // Get all waitlist items 
            var query = "WAIT.COURSE.SECTION NE ''";


            var waitlistIds = await DataReader.SelectAsync("WAIT.LIST", query);
            var totalCount = waitlistIds.Count();

            Array.Sort(waitlistIds);

            var subList = waitlistIds.Skip(offset).Take(limit).ToArray();

            if (subList.Any())
            {
                Collection<WaitList> waitlistBulkData = await DataReader.BulkReadRecordAsync<WaitList>("WAIT.LIST", subList);
                List<StudentSectionWaitlist> waitlistData = new List<StudentSectionWaitlist>();
                if (waitlistBulkData != null)
                {
                    foreach (var wl in waitlistBulkData)
                    {
                        try
                        {
                            var studentGuid = await GetGuidFromRecordInfoAsync("PERSON", wl.WaitStudent);
                            var sectionGuid = await GetGuidFromRecordInfoAsync("COURSE.SECTIONS", wl.WaitCourseSection);
                            waitlistData.Add(new StudentSectionWaitlist(wl.RecordGuid, studentGuid, sectionGuid, wl.WaitRating));
                        }
                        catch (Exception e)
                        {
                            logger.Error("Error converting waitlist entity with guid :'" + wl.RecordGuid + "'.  Error message: " + e.Message);
                        }
                    }
                }
                return new Tuple<IEnumerable<StudentSectionWaitlist>, int>(waitlistData, totalCount);
            }
            else
            {
                return new Tuple<IEnumerable<StudentSectionWaitlist>, int>(new List<StudentSectionWaitlist>(), 0);
            }
        }
        /// <summary>
        /// To get the waitlist details based on the section and student id
        /// Sends back the details on rank and rating of the waitlisted student for the section along with the config details of show rank and show rating
        /// </summary>
        /// <param name="sectionId"> section Id </param>
        /// <param name="studentId"> student Id </param>
        /// <returns>StudentSectionWaitlistInfo</returns>
        public async Task<StudentSectionWaitlistInfo> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId)
        {
            try
            {
                StudentSectionWaitlistInfo studentSectionWaitlistInfo = new StudentSectionWaitlistInfo();
                if (string.IsNullOrEmpty(sectionId))
                {
                    throw new ArgumentNullException("sectionId", "Cannot build a section waitlist student without a course section ID.");
                }
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException("studentId", "Cannot build a section waitlist student without a student ID.");
                }
                List<WaitList> waitlistStudents = await GetWaitListsAsync(new List<string>() { sectionId });
                int rank = 0;
                //caluclate the rank and assign the rating here
                if (waitlistStudents != null && waitlistStudents.Any())
                {
                    var waitlist = waitlistStudents.Where(ws => ws != null).OrderByDescending(s => s.WaitRating).ThenBy(s => s.WaitStatusDate).ThenBy(s => s.WaitTime);
                    foreach (var item in waitlist)
                    {
                        string waitListStatusValcode = await GetWaitlistStatusActionCodeAsync(item.WaitStatus);
                        /* Waitlist status val codes
                         * 1. Active
                         * 4. Permission to register
                         */
                        if (!string.IsNullOrEmpty(waitListStatusValcode) && (waitListStatusValcode == "1" || waitListStatusValcode == "4"))
                        {
                            rank++;
                            if (item.WaitStudent == studentId)
                            {
                                studentSectionWaitlistInfo.SectionWaitlistStudent = new SectionWaitlistStudent(sectionId, studentId, rank, item.WaitRating, waitListStatusValcode, item.WaitStatusDate, null);
                                break;
                            }
                        }
                    }
                }
                bool displayRank = false;
                bool displayRating = false;               
                var stwebSettings = await GetStwebDefaultsAsync();
                var query = "WITH COURSE.SECTIONS.ID EQ " + sectionId;
                Collection<CourseSections> courseSection = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", query);
                int? noOfDaysToEnroll= null;
                if (stwebSettings != null)
                {
                    if (!string.IsNullOrEmpty(stwebSettings.StwebShowWaitlistRank))
                    {
                        if (stwebSettings.StwebShowWaitlistRank.ToUpper() == "Y")
                        {
                            displayRank = true;
                        }
                    }
                    if (courseSection != null && courseSection.FirstOrDefault() != null && !string.IsNullOrEmpty(stwebSettings.StwebShowWaitlistRating))
                    {
                        var courseSec = courseSection.FirstOrDefault();
                        noOfDaysToEnroll = courseSec.SecWaitlistNoDays;
                        if (!string.IsNullOrEmpty(courseSec.SecWaitlistRating) && stwebSettings.StwebShowWaitlistRating.ToUpper() == "Y")
                        {
                            displayRating = true;
                        }
                    }
                }           
                studentSectionWaitlistInfo.SectionWaitlistConfig = new SectionWaitlistConfig(sectionId, displayRank, displayRating, noOfDaysToEnroll);
                return studentSectionWaitlistInfo;
            }
            catch (Exception)
            {               
                throw;
            }
        }

        public async Task<StudentSectionWaitlist> GetWaitlistFromGuidAsync(string waitlistGuid)
        {
            WaitList wl;
            var checkGuid = "";
            var id = GetRecordKeyFromGuid(waitlistGuid);

            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("No Id found that matches guid '" + waitlistGuid + "'.");
            }
            try
            {
                checkGuid = await GetGuidFromRecordInfoAsync("WAIT.LIST", id);
            }
            catch (RepositoryException)
            {
                throw new KeyNotFoundException("No waitlist found with GUID '" + waitlistGuid + "'.");
            }
            if (string.IsNullOrEmpty(checkGuid) || checkGuid != waitlistGuid)
            {
                throw new KeyNotFoundException("No waitlist found with GUID '" + waitlistGuid + "'.");
            }

            try
            {
                wl = await DataReader.ReadRecordAsync<WaitList>(id);
                if (wl != null)
                {
                    var studentGuid = await GetGuidFromRecordInfoAsync("PERSON", wl.WaitStudent);
                    var sectionGuid = await GetGuidFromRecordInfoAsync("COURSE.SECTIONS", wl.WaitCourseSection);
                    return new StudentSectionWaitlist(wl.RecordGuid, studentGuid, sectionGuid, wl.WaitRating);
                }
                else
                {
                    throw new KeyNotFoundException("No waitlist found with GUID '" + waitlistGuid + "'.");
                }
            }
            catch (Exception e)
            {

                var msg = "Error retrieving waitlist data contract with guid :'" + waitlistGuid + "' and record key '" + id + "'.  Error message: " + e.Message;
                logger.Error(msg);
                throw new RepositoryException(msg);

            }

        }

        private async Task<Collection<CourseSecXlists>> GetCrossListedSectionsAsync(List<CourseSections> sectionData)
        {
            var secCrosslistIds = sectionData.Where(s => s.SecXlist.Length > 0).Select(s => s.SecXlist).Distinct().ToList();
            Collection<CourseSecXlists> crosslistData = new Collection<CourseSecXlists>();
            if (secCrosslistIds != null && secCrosslistIds.Count > 0)
            {
                crosslistData = await DataReader.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", secCrosslistIds.ToArray());
            }
            return crosslistData;
        }

        /// <summary>
        /// Retrieves registration billing rate information for "other" financial charges on sections
        /// </summary>
        /// <param name="sectionData">Collection of <see cref="CourseSections">COURSE.SECTIONS</see> records</param>
        /// <returns>Collection of <see cref="RegBillingRates">REG.BILLING.RATES</see> records</returns>
        private async Task<Collection<RegBillingRates>> GetRegBillingRatesAsync(List<CourseSections> sectionData)
        {
            Collection<RegBillingRates> billingRateData = new Collection<RegBillingRates>();
            if (sectionData != null)
            {
                var regBillingRateIds = sectionData.Where(s => s.SecOtherRegBillingRates != null && s.SecOtherRegBillingRates.Any()).SelectMany(s => s.SecOtherRegBillingRates).Distinct().ToList();
                if (regBillingRateIds != null && regBillingRateIds.Any())
                {
                    billingRateData = await DataReader.BulkReadRecordAsync<RegBillingRates>(regBillingRateIds.ToArray());
                }
            }
            return billingRateData;
        }

        private async Task<List<StudentCourseSectionStudents>> GetStudentCourseSecStudents(List<string> studentCourseSecIds)
        {
            var rosterData = new List<StudentCourseSectionStudents>();
            if (studentCourseSecIds != null && studentCourseSecIds.Any())
            {
                var rosterSections = await DataReader.SelectAsync("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray(), "SAVING SCS.COURSE.SECTION");
                var rosterStudents = await DataReader.SelectAsync("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray(), "SAVING SCS.STUDENT");
                if (rosterSections != null && rosterSections.Any() && rosterStudents != null && rosterStudents.Any())
                {
                    for (var i = 0; i < rosterSections.Count(); i++)
                    {
                        var sectionId = "";
                        if (rosterSections.Count() > i)
                            sectionId = rosterSections.ElementAt(i);

                        var studentId = "";
                        if (rosterStudents.Count() > i)
                            studentId = rosterStudents.ElementAt(i);

                        if (!string.IsNullOrEmpty(sectionId) && !string.IsNullOrEmpty(studentId))
                        {
                            var transResult = new StudentCourseSectionStudents()
                            {
                                CourseSectionIds = sectionId,
                                StudentIds = studentId
                            };
                            rosterData.Add(transResult);
                        }
                    }
                }
            }
            return rosterData;
        }

        private async Task<Collection<PortalSites>> GetPortalSitesAsync(List<CourseSections> sectionData)
        {
            var portalSiteData = new Collection<PortalSites>();
            var portalSitesIds = sectionData.Where(s => s.SecPortalSite.Length > 0 && s.SecPortalSite != "PENDING").Select(s => s.SecPortalSite).Distinct().ToList();
            if (portalSitesIds != null && portalSitesIds.Count > 0)
            {
                portalSiteData = await DataReader.BulkReadRecordAsync<PortalSites>("PORTAL.SITES", portalSitesIds.ToArray());
            }
            return portalSiteData;
        }

        // Select the CourseSections table to determine if there has been a status change for any section since the cache build, 
        // typically indicating that a section has been activated or cancelled. Build a cache of these sections. This cache will expire
        // and be rebuilt every 10 minutes.
        private async Task<List<Section>> GetChangedSectionsAsync(IEnumerable<Term> registrationTerms)
        {
            List<Section> changedSections = await GetOrAddToCacheAsync<List<Section>>(ChangedRegistrationSectionsCache,
            async () =>
            {
                var changedRegistrationSections = new List<Section>();
                try
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    // In case the original cache date is missing from cache, default to a date that will work.
                    DateTime originalCacheDate = DateTimeOffset.UtcNow.AddMinutes(0 - CacheTimeout).ToLocalDateTime(colleagueTimeZone);
                    // Get the date the original cache was built. Log if the get from cache fails, because it really should be there.
                    try
                    {
                        originalCacheDate = GetOrAddToCache<List<DateTime>>(AllRegistrationSectionsCacheDate, () => new List<DateTime>()).First();
                    }
                    catch
                    {
                        logger.Info("Error trying to retrieve AllRegistrationSectionsCacheDate from cache. Defaulted to " + originalCacheDate.ToShortDateString());
                    }

                    // Select sections exactly the same way as the original caching, but also select for those with a section status
                    // date change on or after the calculated date. This will cause us to pick up any sections changed to Active or Cancelled
                    // since the original cache build.
                    Tuple<DateTime, DateTime> retrievedDates = await GetSectionsRetrievalDateRangeAsync(registrationTerms);
                    DateTime earliestDate = retrievedDates.Item1;
                    DateTime latestDate = retrievedDates.Item2;
                    var internationalParameters = await InternationalParametersAsync();
                    string beginningStartDate = UniDataFormatter.UnidataFormatDate(earliestDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
                    string endingStartDate = UniDataFormatter.UnidataFormatDate(latestDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
                    var queryQuotedTermIds = QuoteDelimit(registrationTerms.Select(x => x.Code).Distinct().ToList());
                    // This needs to select items that have a status change on or after the date the cache was built.
                    // Normally the cache is built just after midnight and we will pick up any status changes during the day. But to cover all possible situations,
                    // we need to use the Colleague date/time as of the time the cache is built and use that as the date to check against the section status change date.
                    string statusChangeDate = UniDataFormatter.UnidataFormatDate(originalCacheDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
                    string selectCriteria = "WITH SEC.START.DATE GE '" + beginningStartDate + "' AND SEC.START.DATE LE '" + endingStartDate + "'" +
                        " AND SEC.TERM EQ " + queryQuotedTermIds + "''" +
                        " AND SEC.STATUS.DATE GE '" + statusChangeDate + "'";
                    logger.Info("Selecting for changed sections: " + selectCriteria);
                    // Bulk read course sections in chunks
                    string[] changedSectionIds = (await DataReader.SelectAsync("COURSE.SECTIONS", selectCriteria)).Distinct().ToArray();

                    watch.Stop();
                    logger.Info("Changed Section selection completed in: " + watch.ElapsedMilliseconds.ToString() + "   Number of Changed Sections identified: " + changedSectionIds.Count());

                    if (changedSectionIds.Any())
                    {
                        watch.Start();

                        changedRegistrationSections = (await GetNonCachedSectionsAsync(changedSectionIds)).ToList();

                        watch.Stop();
                        logger.Info("Changed section retrieval completed in: " + watch.ElapsedMilliseconds.ToString());
                    }
                }
                catch (Exception ex)
                {
                    logger.Info("Error occurred while building added section cache. Empty list cached. " + ex.Message);
                }

                ChangedRegistrationSectionsCacheBuildTime = DateTime.Now;

                return changedRegistrationSections;
            }
            , changedRegistrationSectionsCacheTimeout);

            // Return the sections built (or retrieved from cache)
            return changedSections;
        }

        /// <summary>
        /// GetCourseSectionsCached is used by CourseService to pull registration sections for a set of selected course ids using cached section data.
        /// </summary>
        /// <param name="courseIds"></param>
        /// <param name="registrationTerms"></param>
        /// <returns>Sections</returns>
        public async Task<IEnumerable<Section>> GetCourseSectionsCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> registrationTerms)
        {
            var sections = new List<Section>();
            if ((courseIds != null) && courseIds.Any())
            {
                IDictionary<string, List<Section>> registrationSections = await GetRegistrationSectionsByCourseAsync(registrationTerms);
                foreach (var id in courseIds)
                {
                    if (registrationSections.ContainsKey(id))
                    {
                        sections.AddRange(registrationSections[id]);
                    }
                }
            }
            return sections;
        }

        /// <summary>
        /// GetCourseSectionsNonCached is used to pull registration sections for a set of selected course Ids - but pull fresh data from the database
        /// instead of looking at any cached values.
        /// </summary>
        /// <param name="courseIds"></param>
        /// <param name="registrationTerms"></param>
        /// <returns>Sections</returns>
        public async Task<IEnumerable<Section>> GetCourseSectionsNonCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> registrationTerms)
        {
            IEnumerable<Section> sections = new List<Section>();
            if ((courseIds != null && courseIds.Any()) && (registrationTerms != null && registrationTerms.Any()))
            {

                _internationalParameters = await InternationalParametersAsync();
                Tuple<DateTime, DateTime> retrievedDates = await GetSectionsRetrievalDateRangeAsync(registrationTerms);
                DateTime earliestDate = retrievedDates.Item1;
                DateTime latestDate = retrievedDates.Item2;
                string beginningStartDate = UniDataFormatter.UnidataFormatDate(earliestDate, _internationalParameters.HostShortDateFormat, _internationalParameters.HostDateDelimiter);
                string endingStartDate = UniDataFormatter.UnidataFormatDate(latestDate, _internationalParameters.HostShortDateFormat, _internationalParameters.HostDateDelimiter);
                var courseQuotedIds = QuoteDelimit(courseIds);
                string selectCriteria = "WITH SEC.COURSE EQ " + courseQuotedIds + " AND SEC.START.DATE GE '" + beginningStartDate + "' AND SEC.START.DATE LE '" + endingStartDate + "'";
                string[] sectionIds = await DataReader.SelectAsync("COURSE.SECTIONS", selectCriteria);
                sections = await GetNonCachedSectionsAsync(sectionIds.AsEnumerable());


            }
            return sections;
        }

        /// <summary>
        /// GetNonCachedFacultySections is used to retrieve sections taught by a faculty for a list of terms not present in the current registration terms.
        /// </summary>
        /// <param name="terms">Terms used to scope section selection</param>
        /// <param name="facultyId">Faculty Member Id</param>
        /// <param name="bestFit">Determines whether the resulting sections should be placed in a term based on the section dates</param>
        /// <returns>Sections</returns>
        public async Task<IEnumerable<Section>> GetNonCachedFacultySectionsAsync(IEnumerable<Term> terms, string facultyId, bool bestFit = false)
        {
            IEnumerable<Section> sections = new List<Section>();
            if (terms != null && terms.Any() && !String.IsNullOrEmpty(facultyId))
            {

                var internationalParameters = await InternationalParametersAsync();
                Tuple<DateTime, DateTime> retrievedDates = await GetSectionsRetrievalDateRangeAsync(terms);
                DateTime earliestDate = retrievedDates.Item1;
                DateTime latestDate = retrievedDates.Item2;
                string beginningDate = UniDataFormatter.UnidataFormatDate(earliestDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
                string endingDate = UniDataFormatter.UnidataFormatDate(latestDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
                // time span bound by earliest term's start date and latest term's end date, CSF should fall within those bounds to be selected
                var queryQuotedTermIds = QuoteDelimit(terms.Select(x => x.Code).Distinct().ToList());
                string criteria = "WITH CSF.FACULTY EQ '" + facultyId + "' AND CSF.START.DATE GE '" + beginningDate + "' AND CSF.END.DATE LE '" + endingDate + "'" + " AND CSF.SECTION.TERM EQ " + queryQuotedTermIds + "''";

                Collection<CourseSecFaculty> courseSecFaculty = await DataReader.BulkReadRecordAsync<CourseSecFaculty>(criteria, true);
                // ensure unique sectionIds in case of split course assignments
                List<string> csfSectionIds = courseSecFaculty.Select(csf => csf.CsfCourseSection).Distinct().ToList();
                sections = await GetNonCachedSectionsAsync(csfSectionIds.AsEnumerable(), bestFit);
            }
            return sections;
        }

        /// <summary>
        /// Retrieve a list of course section records and return Section objects.
        /// </summary>
        /// <param name="sectionIds">Keys to the Course Section records</param>
        /// <param name="bestFit">If "true" then find the best term to associate to non-term based sections</param>
        /// <returns></returns>
        public async Task<IEnumerable<Section>> GetNonCachedSectionsAsync(IEnumerable<string> sectionIds, bool bestFit = false)
        {
            // Assuming the list of section Ids needed (including all associated crosslists) is less than 5000 - not breaking up the bulkreads in this method at this time. 
            var sectionsRequested = new List<Section>();
            if ((sectionIds != null) && (sectionIds.Any()))
            {
                List<CourseSections> sectionsToBuild = (await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", sectionIds.ToArray())).ToList();
                sectionsRequested = (await BuildNonCachedSectionsAsync(sectionsToBuild, bestFit)).ToList();

            }

            return sectionsRequested;
        }

        /// <summary>
        /// to retrieve sections seats to check for availability
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="bestFit"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, SectionSeats>> GetSectionsSeatsAsync(IEnumerable<string> sectionIds)
        {
            List<CourseSections> sectionsToBuild = new List<CourseSections>();
            var sectionsRequested = new Dictionary<string, SectionSeats>();
            if ((sectionIds != null) && (sectionIds.Any()))
            {
                sectionsToBuild = await RetrieveBulkDataInBatchAsync<CourseSections>(sectionIds, "COURSE.SECTIONS");
                sectionsRequested = (await BuildSectionsSeatsAsync(sectionsToBuild));
            }

            return sectionsRequested;
        }

        /// <summary>
        /// Checks addErrorToCollection boolean to determine of error message should be added to the repository exception
        /// Note - the exception is not thrown, but is only added to the collection.
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="id"></param>
        /// <param name="dataObject"></param>
        /// <param name="ex"></param>
        private void LogRepoError(string errorMessage, string guid = "", string sourceId = "")
        {
            if (addToErrorCollection)
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", errorMessage)
                {
                    SourceId = string.IsNullOrEmpty(sourceId) ? "" : sourceId,
                    Id = string.IsNullOrEmpty(guid) ? "" : guid
                });
            }
        }

        private async Task<IEnumerable<Section>> BuildNonCachedSectionsAsync(List<CourseSections> sectionsToBuild, bool bestFit = false)
        {

            // Save the list of original section IDs for later
            var sectionIds = sectionsToBuild.Select(x => x.Recordkey).ToList();

            // Assuming the list of section Ids needed (including all associated crosslists) is less than 5000 - not breaking up the bulkreads in this method at this time. 

            var sectionsRequested = new List<Section>();
            if ((sectionsToBuild != null) && (sectionsToBuild.Any()))
            {
                // If any section is cross listed need to also build out all of the cross-listed sections to get the links to the other sections.
                Collection<CourseSecXlists> crossListData = new Collection<CourseSecXlists>();

                List<string> crossListIds = sectionsToBuild.Where(cs => !string.IsNullOrEmpty(cs.SecXlist)).Select(cs => cs.SecXlist).Distinct().ToList();
                if (crossListIds != null && crossListIds.Any())
                {
                    crossListData = await DataReader.BulkReadRecordAsync<CourseSecXlists>("COURSE.SEC.XLISTS", crossListIds.ToArray());
                    var crossListSectionIds = crossListData.SelectMany(cx => cx.CsxlCourseSections).Distinct().ToList();
                    // Now determine if there are additional cross list section Ids (not already requested) that should be added to list of section to build.
                    // If so, gather those together and do a bulk read for them and add them into the sectionsToBuild list.
                    List<string> additionalSectionIds = new List<string>();
                    foreach (var xlistId in crossListSectionIds)
                    {
                        if (!sectionsToBuild.Select(x => x.Recordkey).Contains(xlistId))
                        {
                            additionalSectionIds.Add(xlistId);
                        }
                    }
                    if (additionalSectionIds.Any())
                    {
                        List<CourseSections> courseSectionsToAdd = (await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", additionalSectionIds.ToArray())).ToList();
                        sectionsToBuild.AddRange(courseSectionsToAdd);
                    }
                }

                // Add to eliminate possible duplicate section Ids
                sectionsToBuild = sectionsToBuild.Distinct().ToList();

                // We now have a complete list of sections to build - pull additional data.
                var meetingIds = sectionsToBuild.Where(cs => cs.SecMeeting != null && cs.SecMeeting.Count > 0).SelectMany(sm => sm.SecMeeting).Distinct().ToArray();
                var meetingData = await GetCourseSecMeetingAsync(meetingIds);

                var facultyIds = sectionsToBuild.Where(cs => cs.SecFaculty != null && cs.SecFaculty.Count > 0).SelectMany(sf => sf.SecFaculty).Distinct().ToList();
                var facultyData = await GetCourseSecFacultyAsync(facultyIds);

                var studentCourseSecIds = sectionsToBuild.Where(cs => cs.SecActiveStudents != null && cs.SecActiveStudents.Count > 0)
                    .SelectMany(s => s.SecActiveStudents).Distinct().ToList();

                var rosterData = await GetStudentCourseSecStudents(studentCourseSecIds);
                var portalSiteData = await GetPortalSitesAsync(sectionsToBuild);
                var sectionsToBuildIds = sectionsToBuild.Select(s => s.Recordkey).Distinct().ToList();
                var pendingData = await GetPendingSectionsAsync(sectionsToBuildIds);
                var waitlistData = await GetWaitListsAsync(sectionsToBuildIds);
                var requisiteData = await GetRequisitesAsync(sectionsToBuild);
                var regBillingRatesData = await GetRegBillingRatesAsync(sectionsToBuild);
                var sectionDict = await BuildSectionsAsync(sectionsToBuild,
                                                meetingData,
                                                facultyData,
                                                rosterData,
                                                portalSiteData,
                                                crossListData,
                                                pendingData,
                                                waitlistData,
                                                requisiteData,
                                                regBillingRatesData,
                                                bestFit);


                // Now return just the Ids of the sections requested - there could be additional cross listed sections in the list that are not needed.
                foreach (var secId in sectionIds)
                {
                    if (sectionDict.ContainsKey(secId))
                    {
                        sectionsRequested.Add(sectionDict[secId]);
                    }
                }
            }
            return sectionsRequested;
        }

        private async Task<List<T>> RetrieveBulkDataInBatchAsync<T>(IEnumerable<string> Ids, string tableToRead, int batchCount = 5000) where T : class, IColleagueEntity
        {
            // to read in batch of  less than 5000 

            List<T> sectionsToBuild = new List<T>();
            if (Ids != null)
            {
                List<string> IdsToSearch = Ids.ToList();
                int howManyTimes = (IdsToSearch.Count / batchCount);

                for (int i = 0; i <= howManyTimes; i++)
                {
                    var idSubList = IdsToSearch.Skip(i * batchCount).Take(batchCount);
                    if (idSubList != null && idSubList.Any())
                    {
                        List<T> sectionsRetrieved = (await DataReader.BulkReadRecordAsync<T>(tableToRead, idSubList.ToArray())).ToList();
                        sectionsToBuild.AddRange(sectionsRetrieved);
                    }
                }
            }
            return sectionsToBuild;
        }
        private async Task<Dictionary<string, SectionSeats>> BuildSectionsSeatsAsync(List<CourseSections> sectionsToBuild)
        {
            Dictionary<string, SectionSeats> sectionDict = new Dictionary<string, SectionSeats>();
            if ((sectionsToBuild != null) && (sectionsToBuild.Any()))
            {
                // If any section is cross listed need to also build out all of the cross-listed sections to get the links to the other sections.
                List<CourseSecXlists> crossListData = new List<CourseSecXlists>();
                List<string> crossListIds = sectionsToBuild.Where(cs => !string.IsNullOrEmpty(cs.SecXlist)).Select(cs => cs.SecXlist).Distinct().ToList();
                if (crossListIds != null && crossListIds.Count > 0)
                {

                    crossListData = await RetrieveBulkDataInBatchAsync<CourseSecXlists>(crossListIds, "COURSE.SEC.XLISTS");
                    var crossListSectionIds = crossListData.SelectMany(cx => cx.CsxlCourseSections).Distinct().ToList();
                    // Now determine if there are additional cross list section Ids (not already requested) that should be added to list of section to build.
                    // If so, gather those together and do a bulk read for them and add them into the sectionsToBuild list.
                    List<string> additionalSectionIds = new List<string>();
                    additionalSectionIds = crossListSectionIds.Except(sectionsToBuild.Select(s => s.Recordkey).ToList()).ToList();
                    if (additionalSectionIds.Any())
                    {
                        List<CourseSections> courseSectionsToAdd = await RetrieveBulkDataInBatchAsync<CourseSections>(additionalSectionIds, "COURSE.SECTIONS");
                        sectionsToBuild.AddRange(courseSectionsToAdd);
                    }
                }

                var sectionIds = sectionsToBuild.Select(x => x.Recordkey).ToList();
                var sectionsToBuildIds = sectionIds.Distinct().ToList();
                var pendingData = await GetPendingSectionsInBatchAsync(sectionsToBuildIds);
                var waitlistData = await GetWaitListsAsync(sectionsToBuildIds);
                sectionDict = await BuildSectionsSeatsAsync(sectionsToBuild,

                                              crossListData,
                                               pendingData,
                                               waitlistData);


            }
            return sectionDict;
        }

        private async Task<Dictionary<string, SectionSeats>> BuildSectionsSeatsAsync(List<CourseSections> sectionData,
                                                 List<CourseSecXlists> crosslistData,
                                                   List<CourseSecPending> pendingData,
                                                List<WaitList> waitlistData)
        {
            var sectionsSeats = new Dictionary<string, SectionSeats>();
            // If no data passed in, return a null collection
            if (sectionData == null)
            {
                return sectionsSeats;
            }

            // Put colleague data into section-based dictionaries wherever practical.
            if (waitlistData == null)
            {
                waitlistData = new List<WaitList>();
            }
            var groupedWaitlists = waitlistData.ToLookup(g => g.WaitCourseSection);
            if (pendingData == null)
            {
                pendingData = new List<CourseSecPending>();
            }
            var groupedPendinglists = pendingData.ToLookup(p => p.Recordkey, p => p);
            sectionsSeats = sectionData.ToDictionary(sec => sec.Recordkey, sec => new SectionSeats(sec.Recordkey)

            {
                Guid = sec.RecordGuid,
                SectionCapacity = sec.SecCapacity
            });
            var waitlistCodesDict = (await GeWaitlistStatusCodesAsync()).ToDictionary(w => w.Code, w => w);
            foreach (var courseSection in sectionData)
            {
                if (sectionsSeats.ContainsKey(courseSection.Recordkey) && sectionsSeats[courseSection.Recordkey] != null)
                {
                    sectionsSeats[courseSection.Recordkey].ActiveStudentIds.AddRange(courseSection.SecActiveStudents.Distinct().ToList());

                    List<string> sectionWaitlistStudents = new List<string>();
                    List<string> sectionPermittedToRegisterStudents = new List<string>();
                    if (groupedWaitlists.Contains(courseSection.Recordkey) && groupedWaitlists[courseSection.Recordkey] != null)
                    {
                        foreach (var wlItem in groupedWaitlists[courseSection.Recordkey])
                        {
                            if (!String.IsNullOrEmpty(wlItem.WaitStatus))
                            {
                                if ((await GetWaitlistStatusAsync(wlItem.WaitStatus)) == WaitlistStatus.WaitingToEnroll)
                                {
                                    sectionWaitlistStudents.Add(wlItem.WaitStudent);
                                }
                                if (waitlistCodesDict.ContainsKey(wlItem.WaitStatus) && waitlistCodesDict[wlItem.WaitStatus].Status == WaitlistStatus.OfferedEnrollment)
                                {
                                    sectionWaitlistStudents.Add(wlItem.WaitStudent);
                                    sectionPermittedToRegisterStudents.Add(wlItem.WaitStudent);
                                }
                            }
                        }
                    }
                    sectionsSeats[courseSection.Recordkey].NumberOnWaitlist = sectionWaitlistStudents.Distinct().Count();
                    sectionsSeats[courseSection.Recordkey].PermittedToRegisterOnWaitlist = sectionPermittedToRegisterStudents.Distinct().Count();
                    sectionsSeats[courseSection.Recordkey].ReservedSeats = groupedPendinglists.Contains(courseSection.Recordkey) ? groupedPendinglists[courseSection.Recordkey] != null ? groupedPendinglists[courseSection.Recordkey].First().CspReservedSeats : default(int?) : default(int?);
                }
            }
            foreach (var crossList in crosslistData)
            {
                foreach (var crossListSectionId in crossList.CsxlCourseSections)
                {
                    if (sectionsSeats.ContainsKey(crossListSectionId))
                    {
                        try
                        {
                            SectionSeats updateSection = sectionsSeats[crossListSectionId];
                            updateSection.GlobalCapacity = crossList.CsxlCapacity;
                            updateSection.CombineCrosslistWaitlists = crossList.CsxlWaitlistFlag == "Y" ? true : false;
                            foreach (var otherCrossListSection in crossList.CsxlCourseSections)
                            {
                                if (otherCrossListSection != updateSection.Id)
                                {
                                    if (sectionsSeats.ContainsKey(otherCrossListSection))
                                    {
                                        updateSection.CrossListedSections.Add(sectionsSeats[otherCrossListSection]);
                                    }
                                }
                            }
                            sectionsSeats[updateSection.Id] = updateSection;
                        }
                        catch (Exception ex)
                        {
                            var sectionError = "Unable to update Cross List info for section " + crossListSectionId;
                            LogDataError("Section Cross List", crossListSectionId, crossList, ex, sectionError);
                        }

                    }
                }
            }
            return sectionsSeats;
        }

        public async Task<SectionWaitlistConfig> GetSectionWaitlistConfigAsync(string sectionId)
        {
            try
            {
                bool displayRank = false;
                bool displayRating = false;
                var query = "COURSE.SECTIONS.ID EQ " + sectionId;
                var stwebSettings = await GetStwebDefaultsAsync();
                Collection<CourseSections> courseSection = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", query);
                if (stwebSettings != null)
                {
                    if (!string.IsNullOrEmpty(stwebSettings.StwebShowWlRankRating))
                    {
                        if (stwebSettings.StwebShowWlRankRating.ToUpper() == "Y")
                        {
                            displayRank = true;
                        }
                    }
                }

                if (courseSection != null && stwebSettings != null)
                {
                    if (!string.IsNullOrEmpty(courseSection.FirstOrDefault().SecWaitlistRating) && stwebSettings.StwebShowWlRankRating.ToUpper() == "Y")
                    {
                        displayRating = true;
                    }
                }

                SectionWaitlistConfig sectionWaitlistSetting = new SectionWaitlistConfig(sectionId, displayRank, displayRating,null);
                return sectionWaitlistSetting;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        private async Task<List<WaitList>> GetWaitListsAsync(List<string> sectionsToBuildIds)
        {
            var waitlistData = new List<WaitList>();
            for (int i = 0; i < sectionsToBuildIds.Count(); i += 250)
            {
                var idSubList = sectionsToBuildIds.Skip(i).Take(250);
                var queryQuotedIds = QuoteDelimit(idSubList);
                var query = "WAIT.COURSE.SECTION EQ " + queryQuotedIds;
                Collection<WaitList> waitlistBulkData = await DataReader.BulkReadRecordAsync<WaitList>("WAIT.LIST", query);
                if (waitlistBulkData != null)
                {
                    waitlistData.AddRange(waitlistBulkData);
                }
            }
            return waitlistData;
        }
        private async Task<Collection<CourseSecPending>> GetPendingSectionsAsync(List<string> sectionsToBuildIds)
        {
            var pendingData = new Collection<CourseSecPending>();
            if (sectionsToBuildIds != null && sectionsToBuildIds.Count > 0)
            {
                pendingData = await DataReader.BulkReadRecordAsync<CourseSecPending>("COURSE.SEC.PENDING", sectionsToBuildIds.ToArray());

            }
            return pendingData;
        }

        private async Task<List<CourseSecPending>> GetPendingSectionsInBatchAsync(List<string> sectionsToBuildIds)
        {
            var pendingData = new List<CourseSecPending>();
            if (sectionsToBuildIds != null && sectionsToBuildIds.Count > 0)
            {
                pendingData = await RetrieveBulkDataInBatchAsync<CourseSecPending>(sectionsToBuildIds, "COURSE.SEC.PENDING");


            }
            return pendingData;
        }

        /// <summary>
        /// Retrieve a list of course section records from cache and return Section objects.
        /// </summary>
        /// <param name="sectionIds">Keys to the Course Section records</param>
        /// <param name="bestFit">If "true" then find the best term to associate to non-term based sections</param>
        public async Task<IEnumerable<Section>> GetCachedSectionsAsync(IEnumerable<string> sectionIds, bool bestFit = false)
        {
            var sectionsRequested = new List<Section>();
            if ((sectionIds == null) || (sectionIds.Count() == 0))
            {
                return sectionsRequested;
            }
            List<string> sectionsNotFound = new List<string>();
            string cacheKey = BuildFullCacheKey(AllRegistrationSectionsCache);
            if (ContainsKey(cacheKey))
            {
                Dictionary<string, Section> regSectionsDict = (Dictionary<string, Section>)_cacheProvider.Get(cacheKey);
                if (regSectionsDict != null)
                {
                    foreach (var sectionId in sectionIds)
                    {
                            if (regSectionsDict.ContainsKey(sectionId))
                            {
                                sectionsRequested.Add(regSectionsDict[sectionId]);
                            }
                            else
                            {
                                sectionsNotFound.Add(sectionId);
                            }                       
                    }
                }
                else
                {
                    sectionsNotFound.AddRange(sectionIds);
                }
            }
            else
            {
                // If we don't find the Cache set sectionsNotFound to all incoming Ids.
                sectionsNotFound.AddRange(sectionIds);
            }
            // Next get any sections that were not pulled from cache using GetArchivedSections.
            if (sectionsNotFound.Any())
            {
                IEnumerable<Section> additionalSections = await GetArchivedSectionsAsync(sectionsNotFound, bestFit);
                sectionsRequested.AddRange(additionalSections);
            }
            return sectionsRequested;
        }

        /// <summary>
        /// Imports student grades for a section
        /// </summary>
        /// <param name="sectionGrades">Student grades for a section</param>
        /// <param name="forceNoVerifyFlag">
        /// true to override the default immediate verification behavior and force that final grades are not verified 
        /// immediately. false to implement the default immediate verification behavior.
        /// </param>
        /// <param name="checkForLocksFlag">
        /// true to explicitly check for record locks and return an error if locked.
        /// false to wait on a record lock until it is released.
        /// The false behavior is to maintain backward compatability with older version endpoints that did not check for record locks.
        /// </param>
        /// <param name="callerType">
        /// Indicates the caller type. Some functionality varies by caller type.
        /// </param>
        /// 
        public async Task<IEnumerable<SectionGradeResponse>> ImportGradesAsync(SectionGrades sectionGrades, bool forceNoVerifyFlag, bool checkForLocksFlag,
            GradesPutCallerTypes callerType)
        {
            if (sectionGrades == null)
                throw new ArgumentNullException("sectionGrades", "SectionGrades must be specified");

            if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                throw new ArgumentException("sectionGrades.StudentGrades", "SectionGrades.StudentGrades must be specified");

            ImportGradesFromILPRequest request = new ImportGradesFromILPRequest();
            request.TransactionId = DateTime.Now.ToString("MMddHHmmss"); // generate a unique id
            request.SectionId = sectionGrades.SectionId;
            request.ForceNoVerify = forceNoVerifyFlag;
            request.CheckForLocks = checkForLocksFlag;
            if (callerType == GradesPutCallerTypes.ILP)
            {
                request.CallerType = "ILP";
            }
            else
            {
                // Standard is the only value besides ILP.
                request.CallerType = "Standard";
            }
            request.ItemsToPostInput = new List<ItemsToPostInput>();

            foreach (var studentGrade in sectionGrades.StudentGrades)
            {
                request.ItemsToPostInput.AddRange(BuildImportPostItems(studentGrade));
            }

            ImportGradesFromILPResponse transactionResponse = await transactionInvoker.ExecuteAsync<ImportGradesFromILPRequest, ImportGradesFromILPResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorCode))
            {
                string errorMessage = string.Format("Error importing grades, Error Code: {0}, Error Code Message: {1}, TransactionId: {2}, SectionId: {3}",
                    transactionResponse.ErrorCode, LookupImportErrorMessage(transactionResponse.ErrorCode), transactionResponse.TransactionId, transactionResponse.SectionId);
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            List<SectionGradeResponse> domainResponse = ConvertImportOutputToDomainEntities(transactionResponse);

            return domainResponse;
        }

        // Check the fairly temporary archive cache for these sections before going out to colleague
        // to get them. The archive cache is intended to cache the sections from the academic history
        // of the users that are currently accessing planning. It is set to expire every 20 minutes.
        // When it does expire, only the current users will be affected the next time they try to
        // load their degree plan, the colleague COURSE.SECTIONS file will need to be read to retrieve
        // these old (non-registration term) sections, which will add about a second to the wait time.
        // Short expiration prevents this cache from building up too much, as it will get a different
        // set of sections for every user. Perhaps at some point in the future this expiration will
        // need to be tweaked.
        private async Task<IEnumerable<Section>> GetArchivedSectionsAsync(IEnumerable<string> sectionIds, bool bestFit = false)
        {
            const int _ArchiveCacheTimeout = 20;
            const string _ArchivedSectionCache = "ArchivedSection";

            var sectionsRequested = new List<Section>();
            if ((sectionIds == null) || (sectionIds.Count() == 0))
            {
                return sectionsRequested;
            }
            List<string> sectionsNotFound = new List<string>();
            foreach (var sectionId in sectionIds)
            {
                string cacheKey = _ArchivedSectionCache + sectionId;
                string fullCacheKey = BuildFullCacheKey(cacheKey);
                if (ContainsKey(fullCacheKey))
                {
                    var sec = (Section)_cacheProvider.Get(fullCacheKey);
                    sectionsRequested.Add(sec);
                    // AddOrUpdateCache takes the raw cache key, not the fully built one
                    AddOrUpdateCache<Section>(cacheKey, sec, _ArchiveCacheTimeout);
                }
                else
                {
                    sectionsNotFound.Add(sectionId);
                }
            }
            // Now as last resort, get any sections not found in the archive from the Colleague database
            if (sectionsNotFound.Any())
            {
                var additionalSections = await GetNonCachedSectionsAsync(sectionsNotFound, bestFit);
                foreach (var sec in additionalSections)
                {
                    // Add this section to the archive cache and add to the list of sections to return
                    // GetOrAddToCache takes the raw cache key, not the fully built one.
                    sectionsRequested.Add(GetOrAddToCache<Section>(_ArchivedSectionCache + sec.Id,
                    () =>
                    {
                        return sec;
                    }, _ArchiveCacheTimeout));
                }
            }
            return sectionsRequested;
        }

        private async Task<Dictionary<string, Section>> BuildSectionsAsync(List<CourseSections> sectionData,
                                                   List<CourseSecMeeting> meetingData,
                                                   Collection<CourseSecFaculty> facultyData,
                                                   List<StudentCourseSectionStudents> rosterData,
                                                   Collection<PortalSites> portalSiteData,
                                                   Collection<CourseSecXlists> crosslistData,
                                                   Collection<CourseSecPending> pendingData,
                                                   List<WaitList> waitlistData,
                                                   Collection<AcadReqmts> requisiteData,
                                                   Collection<RegBillingRates> regBillingRateData,
                                                   bool bestFit = false)
        {
            var sections = new Dictionary<string, Section>();
            // If no data passed in, return a null collection
            if (sectionData == null)
            {
                return sections;
            }

            // Put colleague data into section-based dictionaries wherever practical.
            var groupedMeetings = meetingData != null ? meetingData.GroupBy(m => m.CsmCourseSection).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<CourseSecMeeting>>();
            var groupedFaculty = facultyData != null ? facultyData.GroupBy(f => f.CsfCourseSection).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<CourseSecFaculty>>();
            var groupedRosters = rosterData != null ? rosterData.GroupBy(r => r.CourseSectionIds).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<StudentCourseSectionStudents>>();
            var groupedWaitlists = waitlistData != null ? waitlistData.GroupBy(w => w.WaitCourseSection).ToDictionary(g => g.Key, g => g.ToList()) : new Dictionary<string, List<WaitList>>();

            var bookOptions = (await GetBookOptionsAsync()).ToList();
            string sectionBookstoreUrlTemplate = await GetBookstoreUrlTemplateAsync();
            foreach (var sec in sectionData)
            {
                try
                {

                    var section = await BuildSectionAsync(sec, bestFit);

                    var secFaculty = new List<CourseSecFaculty>();
                    if (groupedFaculty.ContainsKey(sec.Recordkey) && groupedFaculty[sec.Recordkey] != null)
                    {
                        secFaculty = groupedFaculty[sec.Recordkey];
                    }

                    var courseSecMeetings = new List<CourseSecMeeting>();
                    if (groupedMeetings.ContainsKey(sec.Recordkey) && groupedMeetings[sec.Recordkey] != null)
                    {
                        courseSecMeetings = groupedMeetings[sec.Recordkey];
                        foreach (var meeting in groupedMeetings[sec.Recordkey])
                        {
                            try
                            {
                                // For each meeting pattern in Colleague, Instructional method, start date, end date and frequency are all required, thus all
                                // are required in constructor. Anything with a missing item will be caught and not included in the section meetings.
                                var sectionMeeting = await BuildSectionMeetingAsync(meeting, secFaculty);
                                section.AddSectionMeeting(sectionMeeting);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("CourseSecMeeting", meeting.Recordkey, meeting, ex);
                                LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                            }

                        }

                    }

                    if (groupedFaculty.ContainsKey(sec.Recordkey) && groupedFaculty[sec.Recordkey] != null)
                    {
                        foreach (var courseSecFaculty in groupedFaculty[sec.Recordkey])
                        {
                            try
                            {
                                section.AddFaculty(courseSecFaculty.CsfFaculty);
                                var sectionFaculty = BuildSectionFaculty(courseSecFaculty);
                                var ethosSectionFaculty = BuildEthosSectionFaculty(sectionFaculty, sec, courseSecMeetings);
                                section.AddSectionFaculty(ethosSectionFaculty);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("CourseSecFaculty", courseSecFaculty.Recordkey, courseSecFaculty, ex);
                                LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                            }
                        }
                    }

                    try
                    {
                        // Create a dictionary of the instructional methods and associated loads
                        var sectionInstrMethodLoads = sec.SecContactEntityAssociation.ToDictionary(i => i.SecInstrMethodsAssocMember, i => i.SecLoadAssocMember.GetValueOrDefault());
                        // Initialize the logger for the section processor service
                        SectionProcessor.InitializeLogger(logger);
                        // Call domain service method to update the FacultyRoster MeetingLoadFactor for each Meeting
                        SectionProcessor.CalculateMeetingLoadFactor(section.Meetings, sectionInstrMethodLoads);
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Unable to Calculate Meeting Load Factor for section " + section.Id + ": " + ex.Message);
                        LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                    }

                    if (groupedRosters.ContainsKey(sec.Recordkey) && groupedRosters[sec.Recordkey] != null)
                    {
                        foreach (var scs in groupedRosters[sec.Recordkey])
                        {
                            try
                            {
                                section.AddActiveStudent(scs.StudentIds);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("StudentCourseSec", scs.CourseSectionIds, scs, ex);
                            }
                        }
                    }

                    // Compile a list of all section requisites
                    var requisites = new List<Requisite>();
                    var sectionRequisites = new List<SectionRequisite>();

                    if (await RequisitesConvertedAsync())
                    {

                        // Get the requisite information from the post-conversion data fields in Section
                        if (sec.SecOverrideCrsReqsFlag == "Y")
                        {
                            // If this flag is true, we bring in the section requisites defined for the section in Colleague.
                            // If not set, this section inherits course requisites and we ignore any requisites that may be
                            // defined on the section. It is legitimate that the section could be set to override with 
                            // no section requisites defined.
                            section.OverridesCourseRequisites = true;
                            if (sec.SecReqs != null)
                            {
                                foreach (var secReq in sec.SecReqs)
                                {
                                    try
                                    {
                                        if (string.IsNullOrEmpty(secReq))
                                        {
                                            throw new ArgumentNullException("Cannot build a requisite with a null requisite code.");
                                        }
                                        var acadReqmt = requisiteData.Where(r => r.Recordkey == secReq).First();
                                        RequisiteCompletionOrder completionOrder;
                                        switch (acadReqmt.AcrReqsTiming)
                                        {
                                            case "C":
                                                completionOrder = RequisiteCompletionOrder.Concurrent;
                                                break;
                                            case "P":
                                                completionOrder = RequisiteCompletionOrder.Previous;
                                                break;
                                            case "E":
                                                completionOrder = RequisiteCompletionOrder.PreviousOrConcurrent;
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException("AcadReqmt requisite completion order is invalid.");
                                        }
                                        // Note: in the case of a requisite on a section, the isProtected flag will always be false because it is only
                                        // applicable on the requisites of a course.
                                        var req = new Requisite(secReq, (acadReqmt.AcrReqsEnforcement == "RQ") ? true : false, completionOrder, false);
                                        requisites.Add(req);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error("Error building requisite " + secReq + " for section " + sec.Recordkey + ": " + ex);
                                        LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // If the section does not override course requisites, ignore any secReqs and set this flag. The API
                            // needs to know this for validation. If this flag is set to False, course requisites are relevant
                            // to the section.
                            section.OverridesCourseRequisites = false;
                        }

                        // Build the required corequisite section requisites.
                        if (sec.SecCoreqSecs != null && sec.SecCoreqSecs.Any())
                        {
                            // If the number required matches the number of required sections, create a separate requisite for each
                            int minNoCoreqSecs;
                            try
                            {
                                minNoCoreqSecs = (sec.SecMinNoCoreqSecs.HasValue && sec.SecMinNoCoreqSecs > 0) ? sec.SecMinNoCoreqSecs.Value : sec.SecCoreqSecs.Count();
                            }
                            catch
                            {
                                minNoCoreqSecs = sec.SecCoreqSecs.Count();
                            }
                            if (sec.SecCoreqSecs.Any() && sec.SecCoreqSecs.Count() == minNoCoreqSecs)
                            {
                                foreach (var secCoreq in sec.SecCoreqSecs)
                                {
                                    try
                                    {
                                        var secReq = new SectionRequisite(secCoreq, true);
                                        sectionRequisites.Add(secReq);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error("Error constructing single-section requisite for required section " + secCoreq + " for requiring section " + sec.Recordkey + ": " + ex);
                                    }
                                }
                            }
                            else
                            // Otherwise, create a multi-section requisite treating all the requisite sections as a group, indicating how many in the group are required
                            {
                                try
                                {
                                    var secReq = new SectionRequisite(sec.SecCoreqSecs, minNoCoreqSecs);
                                    sectionRequisites.Add(secReq);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error constructing required multi-section requisite for requiring section " + sec.Recordkey + ": " + ex);
                                }
                            }
                        }

                        // Build the recommended section requisites
                        if (sec.SecRecommendedSecs != null && sec.SecRecommendedSecs.Any())
                        {
                            foreach (var recommendedSection in sec.SecRecommendedSecs)
                            {
                                try
                                {
                                    var secreq = new SectionRequisite(recommendedSection, false);
                                    sectionRequisites.Add(secreq);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error constructing recommended section requisite for section " + sec.Recordkey + ": " + ex);
                                }
                            }
                        }
                    }

                    else

                    // Get Requisite data from the pre-conversion data fields on Section
                    {

                        // Pre-conversion, set override to false. Requisites with a requisite code are always inherited pre-conversion. But
                        // Course corequisites are not inherited pre-conversion. (Pre-conversion requisites get special treatment in the UI
                        // so that the course coreqs are always overridden in the UI, even though the requirement-coded requisites are not.)
                        section.OverridesCourseRequisites = false;

                        // Bring in course and section Corequisites into Requisite form
                        if (sec.SecCourseCoreqsEntityAssociation != null)
                        {

                            foreach (var crsCoreq in sec.SecCourseCoreqsEntityAssociation)
                            {
                                try
                                {
                                    var coreq = new Requisite(crsCoreq.SecCoreqCoursesAssocMember, (crsCoreq.SecCoreqCoursesReqdFlagAssocMember == "Y") ? true : false);
                                    requisites.Add(coreq);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Exception occurred while creating course requisite for section Id " + sec.Recordkey + " requisite course Id " + crsCoreq.SecCoreqCoursesAssocMember + ". Error: " + ex.Message);
                                }
                            }
                        }
                        if (sec.SecCoreqsEntityAssociation != null)
                        {
                            // Create a separate section requisite for each required/recommended section
                            var requiredCoreqSections = new List<string>();
                            var recommendedCoreqSections = new List<string>();
                            foreach (var secCoreq in sec.SecCoreqsEntityAssociation)
                            {
                                try
                                {
                                    // Create the required section requisite
                                    var coreq = new SectionRequisite(secCoreq.SecCoreqSectionsAssocMember, secCoreq.SecCoreqSecReqdFlagAssocMember == "Y" ? true : false);
                                    sectionRequisites.Add(coreq);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Exception occurred while creating requisite of required sections for section Id " + sec.Recordkey + ". Error: " + ex.Message);
                                }
                            }
                        }
                    }
                    section.Requisites = requisites;
                    section.SectionRequisites = sectionRequisites;

                    // Add books to section
                    AddBooksToSection(section, sec, bookOptions);
                    // Find reserved seat information.
                    CourseSecPending pendingSection = null;
                    if (pendingData != null)
                    {
                        var matchingPendingSections = pendingData.Where(ps => ps.Recordkey == section.Id);
                        pendingSection = (matchingPendingSections != null) ? matchingPendingSections.FirstOrDefault() : null;
                    }
                    if (pendingSection != null)
                    {
                        section.ReservedSeats = pendingSection.CspReservedSeats;
                    }

                    // Add only "active" or "PermissionToRegister" waitlist information to the section.
                    // Ignore the rest of the waitlist items for now.
                    List<string> sectionWaitlistStudents = new List<string>();
                    List<string> sectionPermittedToRegisterStudents = new List<string>();
                    if (groupedWaitlists.ContainsKey(sec.Recordkey) && groupedWaitlists[sec.Recordkey] != null)
                    {
                        foreach (var wlItem in groupedWaitlists[sec.Recordkey])
                        {
                            if (!String.IsNullOrEmpty(wlItem.WaitStatus))
                            {
                                try
                                {
                                    if ((await GetWaitlistStatusAsync(wlItem.WaitStatus)) == WaitlistStatus.WaitingToEnroll)
                                    {
                                        sectionWaitlistStudents.Add(wlItem.WaitStudent);
                                    }
                                    if ((await GetWaitlistStatusAsync(wlItem.WaitStatus)) == WaitlistStatus.OfferedEnrollment)
                                    {
                                        sectionWaitlistStudents.Add(wlItem.WaitStudent);
                                        sectionPermittedToRegisterStudents.Add(wlItem.WaitStudent);
                                    }
                                }
                                catch(Exception ex)
                                {
                                    LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                                    if (!this.addToErrorCollection)
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    section.NumberOnWaitlist = sectionWaitlistStudents.Distinct().Count();
                    section.PermittedToRegisterOnWaitlist = sectionPermittedToRegisterStudents.Distinct().Count();

                    // determine portal site ID, learning provider and provider specific ID for Intelligent Learning Platform
                    if (!string.IsNullOrEmpty(sec.SecPortalSite))
                    {
                        PortalSites portalSite = null;
                        if (portalSiteData != null)
                        {
                            var matchingPortalSites = portalSiteData.Where(ps => ps.Recordkey == sec.SecPortalSite);
                            portalSite = (matchingPortalSites != null) ? matchingPortalSites.FirstOrDefault() : null;
                        }
                        else
                        {
                            logger.Info("Warning: No portal site data found.");
                            LogRepoError("Warning: No portal site data found.", string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                        }

                        if (portalSite != null)
                        {
                            section.LearningProvider = portalSite.PsLearningProvider;
                            section.LearningProviderSiteId = portalSite.PsPrtlSiteGuid;
                            if (string.IsNullOrEmpty(section.LearningProvider))
                            {
                                // if ILP is not licensed, then learning provider is not set in Colleague, and defaults to SHAREPOINT
                                section.LearningProvider = "SHAREPOINT";
                            }
                        }
                        else
                        {
                            section.LearningProviderSiteId = null;
                            section.LearningProvider = sec.SecLearningProvider;
                        }
                    }
                    else
                    {
                        section.LearningProviderSiteId = null;
                        section.LearningProvider = sec.SecLearningProvider;
                    }
                    // See if there is a section specific bookstore URL that should be constructed
                    if (!string.IsNullOrEmpty(sectionBookstoreUrlTemplate))
                    {
                        // If the template has a query parameter portion substitute encoded values within that.
                        int index = sectionBookstoreUrlTemplate.IndexOf('?');
                        if (index > 0)
                        {
                            string query = sectionBookstoreUrlTemplate.Substring(index);
                            query = query.Replace("{0}", !string.IsNullOrEmpty(sec.SecTerm) ? HttpUtility.UrlEncode(sec.SecTerm) : string.Empty);
                            query = query.Replace("{1}", !string.IsNullOrEmpty(sec.SecSubject) ? HttpUtility.UrlEncode(sec.SecSubject) : string.Empty);
                            query = query.Replace("{2}", !string.IsNullOrEmpty(sec.SecCourseNo) ? HttpUtility.UrlEncode(sec.SecCourseNo) : string.Empty);
                            query = query.Replace("{3}", !string.IsNullOrEmpty(sec.SecNo) ? HttpUtility.UrlEncode(sec.SecNo) : string.Empty);
                            query = query.Replace("{4}", !string.IsNullOrEmpty(sec.Recordkey) ? HttpUtility.UrlEncode(sec.Recordkey) : string.Empty);
                            query = query.Replace("{5}", !string.IsNullOrEmpty(sec.SecCourse) ? HttpUtility.UrlEncode(sec.SecCourse) : string.Empty);
                            query = query.Replace("{6}", !string.IsNullOrEmpty(sec.SecLocation) ? HttpUtility.UrlEncode(sec.SecLocation) : string.Empty);
                            section.BookstoreURL = sectionBookstoreUrlTemplate.Substring(0, index) + query;
                        }
                        else
                        {
                            // Pass it through without substitutions
                            section.BookstoreURL = sectionBookstoreUrlTemplate;
                        }
                    }
                    // Create a RegistrationDates object if the section has any registration date overrides.
                    if (sec.SecOvrAddEndDate != null ||
                        sec.SecOvrAddStartDate != null ||
                        sec.SecOvrDropEndDate != null ||
                        sec.SecOvrDropStartDate != null ||
                        sec.SecOvrPreregEndDate != null ||
                        sec.SecOvrPreregStartDate != null ||
                        sec.SecOvrRegEndDate != null ||
                        sec.SecOvrRegStartDate != null ||
                        sec.SecOvrDropGrReqdDate != null ||
                        sec.SecOvrCensusDates != null && sec.SecOvrCensusDates.Any())
                    {
                        section.RegistrationDateOverrides = new RegistrationDate(sec.SecLocation, sec.SecOvrRegStartDate, sec.SecOvrRegEndDate, sec.SecOvrPreregStartDate, sec.SecOvrPreregEndDate, sec.SecOvrAddStartDate, sec.SecOvrAddEndDate, sec.SecOvrDropStartDate, sec.SecOvrDropEndDate, sec.SecOvrDropGrReqdDate, sec.SecOvrCensusDates);
                    }

                    // Add financial charges for section
                    AddChargesToSection(section, sec, regBillingRateData);

                    sections[section.Id] = section;
                }
                catch (RepositoryException rex)
                {
                    var secString = "Error building section: Section Id: " + sec.Recordkey + " Section Title: " + sec.SecShortTitle + " Section Course: " + sec.SecCourse + " Section Term: " + sec.SecTerm;
                    secString += " Section GUID: " + sec.RecordGuid + " " + rex.Message;
                    LogDataError("Section", sec.Recordkey, null, rex, secString);
                    //throw new RepositoryException(secString);
                    // Removed because this may break self service.  Will revisit in 1.25 when we upgrade error messaging.

                }
                catch (Exception ex)
                {
                    // This is currently unable to serialize the section - maybe because of association in section. For now simplifying it.
                    //var formattedSection = ObjectFormatter.FormatAsXml(sec);
                    var secString = "Section Id: " + sec.Recordkey + " Section Title: " + sec.SecShortTitle + " Section Course: " + sec.SecCourse + " Section Term: " + sec.SecTerm;
                    LogDataError("Section", sec.Recordkey, null, ex, secString);
                }

            }

            // Now that the Sections have been built - use the cross list data to link up cross listed sections and add any global info
            // to the appropriate sections.
            foreach (var crossList in crosslistData)
            {
                foreach (var crossListSectionId in crossList.CsxlCourseSections)
                {
                    if (sections.ContainsKey(crossListSectionId))
                    {
                        try
                        {
                            Section updateSection = sections[crossListSectionId];
                            updateSection.GlobalCapacity = crossList.CsxlCapacity;
                            updateSection.GlobalWaitlistMaximum = crossList.CsxlWaitlistMax;
                            updateSection.PrimarySectionId = crossList.CsxlPrimarySection;
                            updateSection.CombineCrosslistWaitlists = crossList.CsxlWaitlistFlag == "Y" ? true : false;

                            foreach (var otherCrossListSection in crossList.CsxlCourseSections)
                            {
                                if (otherCrossListSection != updateSection.Id)
                                {
                                    // This is a secondary section for the section being updated with crosslist info
                                    if (sections.ContainsKey(otherCrossListSection))
                                    {
                                        updateSection.AddCrossListedSection(sections[otherCrossListSection]);
                                    }

                                }
                            }
                            // When the section being updated is not the primary, update the secondary with some key information from the primary section.
                            if (!string.IsNullOrEmpty(updateSection.PrimarySectionId) && (updateSection.PrimarySectionId != updateSection.Id))
                            {
                                // fetch the primary section for the info
                                Section primarySection = sections[updateSection.PrimarySectionId];
                                if (primarySection != null)
                                {
                                    // Attendance type of a secondary section MUST ALWAYS be the same as the primary.  Cannot enter
                                    // attendance by hour for a secondary section but a different type for the primary.
                                    updateSection.AttendanceTrackingType = primarySection.AttendanceTrackingType;
                                    //update PrimarySectionMeetings on cross-listed section with Primary section meetings only when flag allows to do so and cross-listed section does not have its own meeting section
                                    if ((updateSection.Meetings == null || updateSection.Meetings.Count() == 0) && primarySection.Meetings != null && primarySection.Meetings.Any())
                                    {
                                        bool shouldUsePrimarySectionMeetings = false;
                                        //check for primary section meeting override flag on crossList
                                        if (!string.IsNullOrEmpty(crossList.CsxlPrimSecMngOvrdeFlag))
                                        {
                                            shouldUsePrimarySectionMeetings = crossList.CsxlPrimSecMngOvrdeFlag.ToUpper() == "Y" ? true : false;

                                        }
                                        else
                                        {
                                            //get STWEB.DEFAULTS value here retrieve default value that determines if corss-listed with no meetings should use primary section meeting info
                                            var stwebDefaults = await GetStwebDefaultsAsync();
                                            shouldUsePrimarySectionMeetings = stwebDefaults != null && stwebDefaults.StwebUsePrimSecMtgFlag.ToUpper() == "Y" ? true : false;

                                        }
                                        if (shouldUsePrimarySectionMeetings)
                                        {
                                            updateSection.UpdatePrimarySectionMeetings(primarySection.Meetings);
                                        }
                                    }
                                }
                            }
                            sections[updateSection.Id] = updateSection;
                        }
                        catch (Exception ex)
                        {
                            var sectionError = "Unable to update Cross List info for section " + crossListSectionId;
                            LogDataError("Section Cross List", crossListSectionId, crossList, ex, sectionError);
                            LogRepoError(sectionError);
                        }

                    }
                }
            }
            return sections;
        }

        private async Task<Section> BuildSectionAsync(CourseSections sec, bool bestFit)
        {
            List<SectionStatusItem> statuses = null;
            if (sec.SecStatusesEntityAssociation != null)
            {
                try
                {
                    statuses = await BuildSectionStatusItemAsync(sec);
                }
                catch(Exception ex)
                {
                    LogRepoError(ex.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                    if (!this.addToErrorCollection)
                    {
                        throw;
                    }
                }
            }
            bool allowPassNoPass = sec.SecAllowPassNopassFlag == "Y";
            bool allowAudit = sec.SecAllowAuditFlag == "Y";
            bool onlyPassNoPass = sec.SecOnlyPassNopassFlag == "Y";
            bool allowWaitlist = sec.SecAllowWaitlistFlag == "Y";
            bool waitlistClosed = sec.SecCloseWaitlistFlag == "Y";
            bool consentRequired = sec.SecFacultyConsentFlag == "Y";
            bool hideInCatalog = sec.SecHideInCatalog != null && sec.SecHideInCatalog.ToUpper() == "Y";
            var departments = sec.SecDepartmentsEntityAssociation.Select(item =>
                new OfferingDepartment(item.SecDeptsAssocMember, item.SecDeptPctsAssocMember.GetValueOrDefault())).ToList();

            // We need to convert value marks to new line characters because we want to maintain any formatting
            // (line-to-line) that the user may have entered.
            string sectionComments = string.Empty;
            if (!string.IsNullOrEmpty(sec.SecPrintedComments))
            {
                sectionComments = sec.SecPrintedComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
            }

            //
            Section section;
            try
            {
                section = new Section(sec.Recordkey,
                 sec.SecCourse,
                 sec.SecNo,
                 sec.SecStartDate.GetValueOrDefault(DateTime.MinValue),
                 sec.SecMinCred,
                 sec.SecCeus,
                 sec.SecShortTitle,
                 sec.SecCredType,
                 departments,
                 sec.SecCourseLevels,
                 sec.SecAcadLevel,
                 statuses,
                 allowPassNoPass,
                 allowAudit,
                 onlyPassNoPass,
                 allowWaitlist,
                 waitlistClosed,
                 consentRequired,
                 hideInCatalog
                 )
                {
                    Guid = sec.RecordGuid,
                    TermId = sec.SecTerm,
                    Location = sec.SecLocation,
                    MaximumCredits = sec.SecMaxCred,
                    VariableCreditIncrement = sec.SecVarCredIncrement,
                    EndDate = sec.SecEndDate,
                    TopicCode = sec.SecTopicCode,
                    GradeSchemeCode = sec.SecGradeScheme,
                    NumberOfWeeks = sec.SecNoWeeks,
                    Name = sec.SecName,
                    SectionCapacity = sec.SecCapacity,
                    WaitlistMaximum = sec.SecWaitlistMax,
                    WaitlistRatingCode = sec.SecWaitlistRating,
                    CourseName = sec.SecSubject + "-" + sec.SecCourseNo,
                    TransferStatus = sec.SecTransferStatus,
                    Comments = sectionComments,
                    CensusDates = sec.SecOvrCensusDates == null ? new List<DateTime?>() : sec.SecOvrCensusDates,
                    WaitListNumberOfDays = sec.SecWaitlistNoDays,
                    GradeSubschemeCode = sec.SecGradeSubschemesId,
                    Synonym = sec.SecSynonym
                };
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message); 
                LogRepoError(e.Message, string.IsNullOrWhiteSpace(sec.RecordGuid)? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                throw new RepositoryException(e.Message);
            }

            // Make sure bad colleague data doesn't get in
            section.CensusDates.RemoveAll(cd => cd == null);

            section.ExcludeFromAddAuthorization = sec.SecAddAuthExclusionFlag != null && sec.SecAddAuthExclusionFlag.ToUpper() == "Y";

            // Other processes are already dependent on the course "type" code
            // so add the proper course category as well


            foreach (var item in sec.SecCourseTypes)
            {
                section.AddCourseType(item);
            }

            if (sec.SecInstrMethods != null && sec.SecInstrMethods.Any())
            {
                foreach (var instrMethod in sec.SecInstrMethods)
                {
                    section.AddInstructionalMethod(instrMethod);

                }
            }

            // Add instructional contact info
            if (sec.SecContactEntityAssociation != null && sec.SecContactEntityAssociation.Count > 0)
            {
                foreach (var contact in sec.SecContactEntityAssociation)
                {
                    try
                    {
                        section.AddInstructionalContact(new InstructionalContact(contact.SecInstrMethodsAssocMember)
                        {
                            Load = contact.SecLoadAssocMember,
                            ClockHours = contact.SecClockHoursAssocMember,
                            ContactHours = contact.SecContactHoursAssocMember,
                            ContactMeasure = contact.SecContactMeasuresAssocMember
                        });
                    }
                    catch(Exception e)
                    {
                        LogRepoError(e.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                        if (!this.addToErrorCollection)
                        {
                            throw;
                        }
                    }
                }
            }

            // NOTE: these have to come from a scan of the calendar schedules records for the section to get
            // the actual values. If the schedule hasn't been built, they default to the section start/end dates.
            section.FirstMeetingDate = sec.SecFirstMeetingDate ?? sec.SecStartDate;
            section.LastMeetingDate = sec.SecLastMeetingDate ?? sec.SecEndDate;
            // If the section is non-term based, then pick the first term that meets the
            // criteria of being within the start and end dates.
            if (string.IsNullOrEmpty(section.TermId) && bestFit == true)
            {
                section.TermId = FindBestFit(section.StartDate, section.EndDate);
            }

            section.BillingCred = sec.SecBillingCred;
            section.BillingMethod = sec.SecBillingMethod;
            //Causing performance issue - removing temporarily
            //section.WaitlistStatus = await GetSectionWaitlistStatusAsync(sec.Recordkey);

            // Set the attendance tracking type
            try
            {
                section.AttendanceTrackingType = ConvertStringToAttendanceTrackingType(sec);
            }
            catch(Exception e)
            {
                LogRepoError(e.Message, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                if (!this.addToErrorCollection)
                {
                    throw;
                }

            }
            return section;
        }

        /// <summary>
        /// Adds one or more <see cref="SectionBook">books</see> to a <see cref="Section">course section</see>
        /// </summary>
        /// <param name="section">A <see cref="Section">course section</see></param>
        /// <param name="sec">A COURSE.SECTIONS record</param>
        /// <param name="bookOptions">Collection of <see cref="BookOption">book options</see></param>
        private void AddBooksToSection(Section section, CourseSections sec, IEnumerable<BookOption> bookOptions)
        {
            if (section != null && sec != null)
            {
                if (sec.SecBooks != null && sec.SecBooks.Count > 0)
                {
                    for (int i = 0; i < sec.SecBooks.Count; i++)
                    {
                        string bookId = sec.SecBooks.ElementAt(i);
                        try
                        {
                            if (bookOptions == null)
                            {
                                throw new ApplicationException("Book options data could not be retrieved.");
                            }
                            if (sec.SecBookOptions != null && sec.SecBookOptions.Count > 0)
                            {
                                var matchingOptions = bookOptions.Where(op => op.Code == sec.SecBookOptions[i]);
                                var option = (matchingOptions != null) ? matchingOptions.FirstOrDefault() : null;
                                if (option != null)
                                {
                                    section.AddBook(bookId, sec.SecBookOptions[i], option.IsRequired);
                                }
                                else
                                {
                                    throw new ApplicationException(string.Format("Book option {0} for book {1} on section {2} is not a valid BOOK.OPTION value", sec.SecBookOptions[i], bookId, sec.Recordkey));
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            string errorMsg = String.Format("COURSE.SECTIONS {0} SEC.BOOKS {1} SEC.BOOK.OPTIONS {2}", sec.Recordkey, bookId, sec.SecBookOptions[i]);
                            LogDataError(errorMsg, sec.Recordkey, sec, ex);
                            LogRepoError(errorMsg, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Adds one or more <see cref="SectionCharge">financial charges</see> to a <see cref="Section">course section</see>
        /// </summary>
        /// <param name="section">A <see cref="Section">course section</see></param>
        /// <param name="sec">A COURSE.SECTIONS record</param>
        /// <param name="regBillingRateData">Collection of <see cref="RegBillingRates">registration billing rate records</see></param>
        private void AddChargesToSection(Section section, CourseSections sec, Collection<RegBillingRates> regBillingRateData)
        {
            if (section != null && sec != null)
            {
                if (sec.SecOtherRegBillingRates != null && sec.SecOtherRegBillingRates.Any())
                {
                    foreach (var otherBillingRate in sec.SecOtherRegBillingRates)
                    {
                        try
                        {
                            if (regBillingRateData == null)
                            {
                                throw new ApplicationException("Registration billing rate data could not be retrieved.");
                            }
                            var matchingRegBillingRateData = regBillingRateData.Where(rb => rb.Recordkey == otherBillingRate);
                            var rbr = (matchingRegBillingRateData != null) ? matchingRegBillingRateData.FirstOrDefault() : null;
                            if (rbr != null)
                            {
                                decimal baseAmount = (rbr.RgbrChargeAmt ?? 0 - rbr.RgbrCrAmt ?? 0);
                                bool isFlatFee = string.IsNullOrEmpty(rbr.RgbrAmtCalcType) || rbr.RgbrAmtCalcType.ToUpperInvariant() == "F";
                                section.AddSectionCharge(new SectionCharge(rbr.Recordkey, rbr.RgbrArCode, baseAmount, isFlatFee, !string.IsNullOrEmpty(rbr.RgbrRule)));
                            }
                            else
                            {
                                throw new ApplicationException(string.Format("Registration billing rate {0} on section {1} is not valid.", otherBillingRate, sec.Recordkey));
                            }

                        }
                        catch (Exception ex)
                        {
                            string errorMsg = String.Format("COURSE.SECTIONS {0} SEC.OTHER.REG.BILLING.RATES {1}", sec.Recordkey, otherBillingRate);
                            LogDataError(errorMsg, otherBillingRate, sec, ex);
                            LogRepoError(errorMsg, string.IsNullOrWhiteSpace(sec.RecordGuid) ? "" : sec.RecordGuid, string.IsNullOrWhiteSpace(sec.Recordkey) ? "" : sec.Recordkey);
                        }
                    }
                }
            }
            return;
        }

        private async Task<List<SectionStatusItem>> BuildSectionStatusItemAsync(CourseSections sec)
        {
            List<SectionStatusItem> sectionStatus = new List<SectionStatusItem>();
            foreach (var association in sec.SecStatusesEntityAssociation)
            {
                sectionStatus.Add(new SectionStatusItem((await ConvertStatusCodeToSectionStatusAsync(association.SecStatusAssocMember)), (await ConvertStatusCodeToSectionIntegrationStatusAsync(association.SecStatusAssocMember)), association.SecStatusAssocMember, association.SecStatusDateAssocMember.GetValueOrDefault()));
            }
            return sectionStatus;
        }

        /// <summary>
        /// Get a section meeting
        /// </summary>
        /// <param name="id">Section Meeting ID</param>
        /// <returns>The section meeting</returns>
        public async Task<SectionMeeting> GetSectionMeetingAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "An ID must be provided.");
            }
            CourseSecMeeting meeting = null;
            meeting = await DataReader.ReadRecordAsync<CourseSecMeeting>(id);

            if (meeting == null)
            {
                throw new KeyNotFoundException("Record not found, invalid ID provided: " + id);
            }

            // Now we have a record, so we can pass of the rest of the work to another routine
            return await BuildSectionMeetingAsync(meeting);
        }

        /// <summary>
        /// Get a list of course sec meeting using an array of IDs
        /// </summary>
        /// <param name="secMeetingIds">CourseSecMeeting IDs</param>
        /// <returns>course sec meeting</returns>
        public async Task<List<CourseSecMeeting>> GetCourseSecMeetingAsync(string[] secMeetingIds)
        {
            if ((secMeetingIds == null) || (!secMeetingIds.Any()))
            {
                return new List<CourseSecMeeting>();
            }
            return (await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", secMeetingIds)).ToList();
        }

        /// <summary>
        /// Get a collection of course sec faculty using a list of IDs
        /// </summary>
        /// <param name="secFacultyIds">CourseSecFaculty IDs</param>
        /// <returns>course sec faculty</returns>
        public async Task<Collection<CourseSecFaculty>> GetCourseSecFacultyAsync(List<string> secFacultyIds)
        {
            if ((secFacultyIds == null) || (!secFacultyIds.Any()))
            {
                return new Collection<CourseSecFaculty>();
            }
            return await DataReader.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", secFacultyIds.ToArray());
        }

        /// <summary>
        /// Get a section meeting using its record ID
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="section">Section Id</param>
        /// <param name="startDate">Meeting Start Date</param>
        /// <param name="startTime">Meeting Start Time</param>
        /// <param name="endDate">Meeting End Date</param>
        /// <param name="endTime">Meeting End Time</param>
        /// <param name="room">Meeting Room Id</param>
        /// <param name="instructor">Instructor Id</param>
        /// <returns>Section meeting</returns>
        public async Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeetingAsync(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string termId)
        {
            List<SectionMeeting> meetings = new List<SectionMeeting>();
            var selectStatement = new StringBuilder();
            string[] limitingKeys = null;

            // If we have a section, then select the limited list from the COURSE.SECTIONS record first
            if (!string.IsNullOrEmpty(section))
            {
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", new string[] { section }, "WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING");
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, 0);
                }
                selectStatement.AppendFormat("WITH CSM.COURSE.SECTION EQ '{0}'", section);
            }
            // Look at any index fields first
            if (!string.IsNullOrEmpty(termId) && string.IsNullOrEmpty(section))
            {
                limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", string.Format("WITH SEC.TERM = '{0}' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING", termId));
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, 0);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(termId))
                {
                    if (selectStatement.Length > 0)
                    {
                        selectStatement.Append(" AND ");
                    }
                    selectStatement.AppendFormat("WITH SEC.TERM EQ '{0}'", termId);
                }
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                if (selectStatement.Length > 0)
                {
                    selectStatement.Append(" AND ");
                }
                selectStatement.AppendFormat("WITH CSM.START.DATE EQ '{0}'", startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                if (selectStatement.Length > 0)
                {
                    selectStatement.Append(" AND ");
                }
                selectStatement.AppendFormat("WITH CSM.END.DATE EQ '{0}'", endDate);
            }
            if (!string.IsNullOrEmpty(startTime))
            {
                if (selectStatement.Length > 0)
                {
                    selectStatement.Append(" AND ");
                }
                selectStatement.AppendFormat("WITH CSM.START.TIME EQ '{0}'", startTime);
            }
            if (!string.IsNullOrEmpty(endTime))
            {
                if (selectStatement.Length > 0)
                {
                    selectStatement.Append(" AND ");
                }
                selectStatement.AppendFormat("WITH CSM.END.TIME EQ '{0}'", endTime);
            }
            if (buildings != null && buildings.Any())
            {
                int x = 0;
                foreach (var building in buildings)
                {
                    var room = rooms != null && rooms.Any() && rooms.ElementAt(x) != null ? rooms.ElementAt(x) : string.Empty;
                    if (!string.IsNullOrEmpty(building) && !string.IsNullOrEmpty(room))
                    {
                        if (selectStatement.Length > 0)
                        {
                            if (x == 0)
                            {
                                selectStatement.Append(" AND ");
                            }
                            else
                            {
                                selectStatement.Append(" OR ");
                            }
                        }
                        selectStatement.AppendFormat("WITH CSM.BLDG = '{0}' AND WITH CSM.ROOM = '{1}'", building, room);
                    }
                    x++;
                }
            }
            if (instructors != null && instructors.Any())
            {
                string instructorSelect = string.Empty;
                foreach (var instructor in instructors)
                {
                    if (!string.IsNullOrEmpty(instructor))
                    {
                        instructorSelect = string.Concat(instructorSelect, "'", instructor, "'");
                    }
                }
                if (!string.IsNullOrEmpty(instructorSelect))
                {
                    instructorSelect = string.Concat("WITH CSF.FACULTY = ", instructorSelect, "SAVING UNIQUE CSF.COURSE.SECTION");
                    var courseSectionKeys = await DataReader.SelectAsync("COURSE.SEC.FACULTY", instructorSelect);
                    if (courseSectionKeys != null && courseSectionKeys.Any())
                    {
                        var meetingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", courseSectionKeys, "WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING");
                        if (limitingKeys != null && limitingKeys.Any())
                        {
                            limitingKeys = limitingKeys.Distinct().Intersect(meetingKeys).ToArray();
                        }
                        else
                        {
                            limitingKeys = meetingKeys.Distinct().ToArray();
                        }
                    }
                    if (limitingKeys == null || !limitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, 0);
                    }
                }
            }
            // Instructional Method is required in the API so do not select items without
            // an instructional method
            if (selectStatement.Length > 0)
            {
                selectStatement.Append(" AND ");
            }
            if (string.IsNullOrEmpty(section))
            {
                selectStatement.Append("WITH CSM.INSTR.METHOD NE '' AND WITH CSM.COURSE.SECTION NE ''");
            }
            else
            {
                selectStatement.Append("WITH CSM.INSTR.METHOD NE ''");
            }

            int totalCount = 0;
            string criteria = selectStatement.ToString();
            Collection<CourseSecMeeting> sectionMeetings = null;

            // Now we have criteria, so we can select and read the records
            var secMeetingIds = await DataReader.SelectAsync("COURSE.SEC.MEETING", limitingKeys, criteria);
            if (secMeetingIds == null || !secMeetingIds.Any())
            {
                return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, 0);
            }

            if (limit == 0 && offset == 0)
            {
                sectionMeetings = await DataReader.BulkReadRecordAsync<CourseSecMeeting>(secMeetingIds);
                totalCount = sectionMeetings.Count();
            }
            else
            {
                totalCount = secMeetingIds.Count();

                Array.Sort(secMeetingIds);

                var subList = secMeetingIds.Skip(offset).Take(limit).ToArray();

                // Now we have criteria, so we can select and read the records
                sectionMeetings = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", subList);
            }

            if (sectionMeetings != null || sectionMeetings.Any())
            {
                foreach (var meeting in sectionMeetings)
                {
                    var sectionMeeting = await BuildSectionMeetingAsync(meeting);
                    meetings.Add(sectionMeeting);
                }
            }

            return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, totalCount);
        }


        /// <summary>
        /// Get a section meeting using its record ID
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="section">Section Id</param>
        /// <param name="startDate">Meeting Start Date</param>
        /// <param name="startTime">Meeting Start Time</param>
        /// <param name="endDate">Meeting End Date</param>
        /// <param name="endTime">Meeting End Time</param>
        /// <param name="room">Meeting Room Id</param>
        /// <param name="instructor">Instructor Id</param>
        /// <returns>Section meeting</returns>
        public async Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeeting2Async(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string termId)
        {
            List<SectionMeeting> meetings = new List<SectionMeeting>();
            var selectStatement = new StringBuilder();
            string[] limitingKeys = null;

            int totalCount = 0;
            string[] subList = null;

            string sectionMeegtingCacheKey = CacheSupport.BuildCacheKey("GetSectionMeeting2", section, startDate, endDate, startTime, endTime, buildings, rooms, instructors, termId);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                sectionMeegtingCacheKey,
                "COURSE.SEC.MEETING",
                offset,
                limit,
                SectionMeetingCacheTimeout,

                async () =>
                {
                    // If we have a section, then select the limited list from the COURSE.SECTIONS record first
                    if (!string.IsNullOrEmpty(section))
                    {
                        limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", new string[] { section }, "WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        selectStatement.AppendFormat("WITH CSM.COURSE.SECTION EQ '{0}'", section);
                    }
                    // Look at any index fields first
                    if (!string.IsNullOrEmpty(termId) && string.IsNullOrEmpty(section))
                    {
                        limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", string.Format("WITH SEC.TERM = '{0}' WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING", termId));
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(termId))
                        {
                            if (selectStatement.Length > 0)
                            {
                                selectStatement.Append(" AND ");
                            }
                            selectStatement.AppendFormat("WITH SEC.TERM EQ '{0}'", termId);
                        }
                    }
                    if (!string.IsNullOrEmpty(startDate))
                    {
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.AppendFormat("WITH CSM.START.DATE EQ '{0}'", startDate);
                    }
                    if (!string.IsNullOrEmpty(endDate))
                    {
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.AppendFormat("WITH CSM.END.DATE EQ '{0}'", endDate);
                    }
                    if (!string.IsNullOrEmpty(startTime))
                    {
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.AppendFormat("WITH CSM.START.TIME EQ '{0}'", startTime);
                    }
                    if (!string.IsNullOrEmpty(endTime))
                    {
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.AppendFormat("WITH CSM.END.TIME EQ '{0}'", endTime);
                    }
                    if (buildings != null && buildings.Any())
                    {
                        int x = 0;
                        foreach (var building in buildings)
                        {
                            var room = rooms != null && rooms.Any() && rooms.ElementAt(x) != null ? rooms.ElementAt(x) : string.Empty;
                            if (!string.IsNullOrEmpty(building) && !string.IsNullOrEmpty(room))
                            {
                                if (selectStatement.Length > 0)
                                {
                                    if (x == 0)
                                    {
                                        selectStatement.Append(" AND ");
                                    }
                                    else
                                    {
                                        selectStatement.Append(" OR ");
                                    }
                                }
                                selectStatement.AppendFormat("WITH CSM.BLDG = '{0}' AND WITH CSM.ROOM = '{1}'", building, room);
                            }
                            x++;
                        }
                    }
                    if (instructors != null && instructors.Any())
                    {
                        string instructorSelect = string.Empty;
                        foreach (var instructor in instructors)
                        {
                            if (!string.IsNullOrEmpty(instructor))
                            {
                                instructorSelect = string.Concat(instructorSelect, "'", instructor, "'");
                            }
                        }
                        if (!string.IsNullOrEmpty(instructorSelect))
                        {
                            instructorSelect = string.Concat("WITH CSF.FACULTY = ", instructorSelect, "SAVING UNIQUE CSF.COURSE.SECTION");
                            var courseSectionKeys = await DataReader.SelectAsync("COURSE.SEC.FACULTY", instructorSelect);
                            if (courseSectionKeys != null && courseSectionKeys.Any())
                            {
                                var meetingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", courseSectionKeys, "WITH SEC.MEETING BY.EXP SEC.MEETING SAVING SEC.MEETING");
                                if (limitingKeys != null && limitingKeys.Any())
                                {
                                    limitingKeys = limitingKeys.Distinct().Intersect(meetingKeys).ToArray();
                                }
                                else
                                {
                                    limitingKeys = meetingKeys.Distinct().ToArray();
                                }
                            }
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                        }
                    }
                    // Instructional Method is required in the API so do not select items without
                    // an instructional method
                    if (selectStatement.Length > 0)
                    {
                        selectStatement.Append(" AND ");
                    }
                    if (string.IsNullOrEmpty(section))
                    {
                        selectStatement.Append("WITH CSM.INSTR.METHOD NE '' AND WITH CSM.COURSE.SECTION NE ''");
                    }
                    else
                    {
                        selectStatement.Append("WITH CSM.INSTR.METHOD NE ''");
                    }

                    string criteria = selectStatement.ToString();
                   
                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null, // secMeetingIds.Distinct().ToList(),
                        criteria = criteria.ToString(),
                    };

                    return requirements;
                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, 0);
            }

           subList = keyCache.Sublist.ToArray();

           totalCount = keyCache.TotalCount.Value;
                // Now we have criteria, so we can select and read the records
            var sectionMeetings = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", subList);
            
            if (sectionMeetings != null || sectionMeetings.Any())
            {
                var meetingIds = sectionMeetings.Where(cs => cs.CsmCourseSection != null).Select(sm => sm.CsmCourseSection).Distinct().ToArray();
                var courseSecFacultyData = new List<CourseSecFaculty>();
                if (meetingIds != null && meetingIds.Any())
                {
                    var courseSecFacLimitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS",
                         meetingIds != null && meetingIds.Any() ? meetingIds : null, "WITH SEC.FACULTY BY.EXP SEC.FACULTY SAVING SEC.FACULTY");

                    if (courseSecFacLimitingKeys != null && courseSecFacLimitingKeys.Any())
                    {
                        for (int i = 0; i < courseSecFacLimitingKeys.Count(); i += readSize)
                        {
                            var subList2 = courseSecFacLimitingKeys.Skip(i).Take(readSize).ToArray();
                            courseSecFacultyData.AddRange(await DataReader.BulkReadRecordAsync<CourseSecFaculty>(subList2));
                        }
                    }
                }

                foreach (var meeting in sectionMeetings)
                {
                    var secFaculty = courseSecFacultyData.Where(x => !string.IsNullOrEmpty(x.CsfCourseSection) && x.CsfCourseSection == meeting.CsmCourseSection).ToList();
                    var sectionMeeting = await BuildSectionMeetingAsync(meeting, secFaculty);
                    meetings.Add(sectionMeeting);
                }
            }

            return new Tuple<IEnumerable<SectionMeeting>, int>(meetings, totalCount);
        }

        /// <summary>
        /// Get a section facult using filters
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="section">Section Id</param>
        /// <param name="instructor">Instructor Id</param>
        /// <returns>Section faculty</returns>
        public async Task<Tuple<IEnumerable<SectionFaculty>, int>> GetSectionFacultyAsync(int offset, int limit, string section, string instructor, List<string> instructionalEvents)
        {
            exception = new RepositoryException();

            List<SectionFaculty> faculties = new List<SectionFaculty>();
            Collection<CourseSecFaculty> sectionFaculties = null;
            string[] subList = null;
            var selectStatement = new StringBuilder();
            string[] limitingKeys = null;
            int totalCount = 0;

            string sectionMeegtingCacheKey = CacheSupport.BuildCacheKey(AllSectionInstructorCache, section, section, instructor, instructionalEvents);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                sectionMeegtingCacheKey,
                "",
                offset,
                limit,
                SectionInstructorCacheTimeout,
                async () =>
                {
                    if (!string.IsNullOrEmpty(section))
                    {
                        limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", new string[] { section }, "WITH SEC.FACULTY BY.EXP SEC.FACULTY SAVING SEC.FACULTY");
                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                        selectStatement.AppendFormat("WITH CSF.COURSE.SECTION EQ '{0}'", section);
                    }
                    if (!string.IsNullOrEmpty(instructor))
                    {
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.AppendFormat("WITH CSF.FACULTY EQ '{0}'", instructor);
                    }
                    if (instructionalEvents.Any())
                    {
                        foreach (var instructionalEvent in instructionalEvents)
                        {
                            if (!string.IsNullOrEmpty(instructionalEvent))
                            {
                                var sectionMeeting = await GetSectionMeetingAsync(instructionalEvent);
                                if (sectionMeeting != null && !string.IsNullOrEmpty(sectionMeeting.InstructionalMethodCode) && !string.IsNullOrEmpty(sectionMeeting.SectionId))
                                {
                                    if (selectStatement.Length > 0)
                                    {
                                        selectStatement.Append(" AND ");
                                    }
                                    selectStatement.AppendFormat("WITH CSF.COURSE.SECTION EQ '{0}' AND WITH CSF.INSTR.METHOD = '{1}'", sectionMeeting.SectionId, sectionMeeting.InstructionalMethodCode);
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                            }
                        }
                    }
                    else
                    {
                        // Instructional Method is required in Colleague so do not select items without
                        // an instructional method; these would be invalid records.
                        if (selectStatement.Length > 0)
                        {
                            selectStatement.Append(" AND ");
                        }
                        selectStatement.Append("WITH CSF.INSTR.METHOD NE '' AND WITH CSF.COURSE.SECTION NE ''");
                    }
                    // Start and End dates and percentage are required in Colleague so do not select items without
                    // an start and end dates; these would be invalid records.
                    if (selectStatement.Length > 0)
                    {
                        selectStatement.Append(" AND ");
                    }
                    selectStatement.Append("WITH CSF.START.DATE NE '' AND WITH CSF.END.DATE NE ''");

                    
                    string criteria = selectStatement.ToString();

                    // Now we have criteria, so we can select and read the records
                    if (limitingKeys != null && limitingKeys.Any()) limitingKeys = limitingKeys.Distinct().ToArray();
                    var secFacultyIds = await DataReader.SelectAsync("COURSE.SEC.FACULTY", limitingKeys, criteria);
                    if (secFacultyIds == null || !secFacultyIds.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    }

                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = secFacultyIds.Distinct().ToList(),
                        criteria = criteria.ToString()
                    };

                    return requirements;
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<SectionFaculty>, int>(faculties, 0);
            }

            subList = keyCacheObject.Sublist.ToArray();
            totalCount = keyCacheObject.TotalCount.Value;
            sectionFaculties = await DataReader.BulkReadRecordAsync<CourseSecFaculty>("COURSE.SEC.FACULTY", subList);

            // Read in course sections records to get Primary indicator and meeting pointers
            var courseSectionIds = sectionFaculties.Select(sf => sf.CsfCourseSection).Distinct().ToArray();
            var courseSections = await DataReader.BulkReadRecordAsync<CourseSections>("COURSE.SECTIONS", courseSectionIds);
            var courseSecMeetingIds = courseSections.SelectMany(cs => cs.SecMeeting).Distinct().ToArray();
            var courseSecMeetings = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", courseSecMeetingIds);

            if (sectionFaculties != null || sectionFaculties.Count > 0)
            {
                foreach (var faculty in sectionFaculties)
                {
                    var sectionFacultyEntity = BuildSectionFaculty(faculty);
                    // Update from Section
                    var courseSection = courseSections.FirstOrDefault(cs => cs.Recordkey == sectionFacultyEntity.SectionId);
                    if (courseSection != null)
                    {
                        var courseMeetings = courseSecMeetings.Where(csm => csm.CsmCourseSection == sectionFacultyEntity.SectionId && csm.CsmInstrMethod == sectionFacultyEntity.InstructionalMethodCode);
                        var sectionFaculty = BuildEthosSectionFaculty(sectionFacultyEntity, courseSection, courseMeetings);
                        faculties.Add(sectionFaculty);
                    }
                    else
                    {
                        exception.AddError(new RepositoryError(faculty.RecordGuid, faculty.Recordkey, "Invalid.Section", string.Concat("Course section '" + faculty.CsfCourseSection
                            + "' does not exist")));
                    }
                }
            }
            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<SectionFaculty>, int>(faculties, totalCount);
        }

        /// <summary>
        /// Get the record ID from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The record ID</returns>
        public async Task<string> GetSectionMeetingIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section meeting.");
            }

            Dictionary<string, GuidLookupResult> idLookup = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idLookup == null || idLookup.Count == 0)
            {
                // GUID not found - should be a new record
                return null;
            }

            var result = idLookup[guid];
            if (result == null)
            {
                // GUID not found
                return null;
            }
            if (result.Entity != "COURSE.SEC.MEETING")
            {
                throw new KeyNotFoundException("GUID " + guid + " not valid for COURSE.SEC.MEETING");
            }

            return result.PrimaryKey;
        }

        /// <summary>
        /// Get a section faculty record ID using its GUID
        /// </summary>
        /// <param name="guid">the GUID</param>
        /// <returns>Section faculty ID</returns>
        public async Task<string> GetSectionFacultyIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a section faculty.");
            }

            Dictionary<string, GuidLookupResult> idLookup = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idLookup == null || idLookup.Count == 0)
            {
                // GUID not found - should be a new record
                return null;
            }

            var result = idLookup[guid];
            if (result == null)
            {
                // GUID not found
                return null;
            }
            if (result.Entity != "COURSE.SEC.FACULTY")
            {
                throw new KeyNotFoundException("GUID " + guid + " not valid for COURSE.SEC.FACULTY");
            }

            return result.PrimaryKey;
        }

        /// <summary>
        /// Get a section meeting using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section meeting</returns>
        public async Task<SectionMeeting> GetSectionMeetingByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID must be provided.");
            }
            CourseSecMeeting meeting = null;
            meeting = await DataReader.ReadRecordAsync<CourseSecMeeting>(new GuidLookup(guid, null));

            if (meeting == null)
            {
                throw new KeyNotFoundException("Record not found, invalid GUID provided: " + guid);
            }

            // Now we have a record, so we can pass of the rest of the work to another routine
            return await BuildSectionMeetingAsync(meeting);
        }

        /// <summary>
        /// Get a section faculty using a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section faculty</returns>
        public async Task<SectionFaculty> GetSectionFacultyByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID must be provided.");
            }
            CourseSecFaculty faculty = null;
            faculty = await DataReader.ReadRecordAsync<CourseSecFaculty>(new GuidLookup(guid, null));

            if (faculty == null)
            {
                throw new KeyNotFoundException("Record not found, invalid GUID provided: " + guid);
            }

            // Read in course sections records to get Primary indicator and meeting pointers
            var courseSectionId = faculty.CsfCourseSection;
            var courseSection = await DataReader.ReadRecordAsync<CourseSections>("COURSE.SECTIONS", courseSectionId);
            if (courseSection != null)
            {
                var courseSecMeetingIds = courseSection.SecMeeting.ToArray();
                var courseSecMeetings = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", courseSecMeetingIds);

                var sectionFacultyEntity = BuildSectionFaculty(faculty);
                // Update from Section
                var courseMeetings = courseSecMeetings.Where(csm => csm.CsmInstrMethod == sectionFacultyEntity.InstructionalMethodCode);
                var sectionFaculty = BuildEthosSectionFaculty(sectionFacultyEntity, courseSection, courseMeetings);
                return sectionFaculty;
            }
            else
            {
                var exception = new RepositoryException(string.Concat("Course section '" + faculty.CsfCourseSection + "' does not exist"));
                throw exception;
            }
        }

        /// <summary>
        /// Post a single section meeting
        /// </summary>
        /// <param name="meeting">The section meeting</param>
        /// <returns>The created/updated section meeting</returns>
        public async Task<SectionMeeting> PostSectionMeetingAsync(Section section, string meetingGuid)
        {
            return await UpdateSectionMeetingAsync(section, meetingGuid);
        }

        /// <summary>
        /// Post a single section meeting V11
        /// </summary>
        /// <param name="meeting">The section meeting</param>
        /// <returns>The created/updated section meeting</returns>
        public async Task<SectionMeeting> PostSectionMeeting2Async(Section section, string meetingGuid)
        {
            return await UpdateSectionMeeting2Async(section, meetingGuid);
        }

        /// <summary>
        /// Put a single section meeting
        /// </summary>
        /// <param name="meeting">The section meeting</param>
        /// <returns>The created/updated section meeting</returns>
        public async Task<SectionMeeting> PutSectionMeetingAsync(Section section, string meetingGuid)
        {
            return await UpdateSectionMeetingAsync(section, meetingGuid);
        }

        /// <summary>
        /// Put a single section meeting V11
        /// </summary>
        /// <param name="meeting">The section meeting</param>
        /// <returns>The created/updated section meeting</returns>
        public async Task<SectionMeeting> PutSectionMeeting2Async(Section section, string meetingGuid)
        {
            return await UpdateSectionMeeting2Async(section, meetingGuid);
        }

        /// <summary>
        /// Delete a single section meeting
        /// </summary>
        /// <param name="id">ID of the section meeting</param>
        public async Task DeleteSectionMeetingAsync(string id, List<SectionFaculty> faculty)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new KeyNotFoundException("Record not found, no delete occurred.");
            }
            // Now we have an ID, so we can pass of the rest of the work to a transaction
            var request = new DeleteInstructionalEventRequest()
            {
                CourseSecMeetingId = id,
                DeleteCsfIds = faculty.Select(f => f.Id).ToList()
            };

            foreach (var fac in faculty)
            {
                request.Faculty.Add(new Transactions.Faculty()
                {
                    FacCsfIds = fac.Id,
                    FacFaculty = fac.FacultyId,
                    FacFacultyLoad = fac.LoadFactor,
                    FacStartDate = fac.StartDate,
                    FacEndDate = fac.EndDate,
                    FacFacultyPct = fac.ResponsibilityPercentage,
                    FacInstrMethod = fac.InstructionalMethodCode,
                    FacPacLpAsgmt = fac.ContractAssignment,
                    FacTeachingArrangement = fac.TeachingArrangementCode
                }
                );
            }

            var response = await transactionInvoker.ExecuteAsync<DeleteInstructionalEventRequest, DeleteInstructionalEventResponse>(request);

            if (response.DeleteInstructionalEventErrors != null && response.DeleteInstructionalEventErrors.Count > 0)
            {
                var exception = new RepositoryException("Errors encountered while deleting instructional event " + id);
                foreach (var error in response.DeleteInstructionalEventErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(error.ErrorCodes) ? "" : error.ErrorCodes, error.ErrorMessages));
                }
                throw exception;
            }
        }

        /// <summary>
        /// Post a single section faculty
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to be created</param>
        /// <param name="guid">The GUID for the section faculty record</param>
        /// <returns>The created/updated section faculty</returns>
        public async Task<SectionFaculty> PostSectionFacultyAsync(SectionFaculty sectionFaculty, string guid)
        {
            return await UpdateSectionFacultyAsync(sectionFaculty, guid);
        }

        /// <summary>
        /// Put a single section faculty
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to be updated</param>
        /// <returns>The created/updated section faculty</returns>
        public async Task<SectionFaculty> PutSectionFacultyAsync(SectionFaculty sectionFaculty, string guid)
        {
            return await UpdateSectionFacultyAsync(sectionFaculty, guid);
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
        public async Task<IEnumerable<Event>> GetSectionEventsICalAsync(string calendarScheduleType, IEnumerable<string> calendarSchedulePointers, DateTime? startDate, DateTime? endDate)
        {
            string startDatePart = string.Empty;
            string endDatePart = string.Empty;
            IEnumerable<Event> calendarEvents = new List<Event>();
            Dictionary<Section, Collection<CalendarSchedules>> sectionWiseCalData = new Dictionary<Section, Collection<CalendarSchedules>>();

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
            try
            {
                //get all the section details. This is check for each section if its cross-listed. Check for PrimarysectionMeetings property.
                //if not null take primary section id to retrieve calendar schedules, if empty take section id
                List<Section> sections = (await GetCachedSectionsAsync(calendarSchedulePointers)).ToList();


                IEnumerable<string> calendarPointersNotFoundInCache = calendarSchedulePointers.Except(sections.Select(s => s.Id));
                //Validate count of sections retrieved with sections in calendarschedulPointers
                if (calendarPointersNotFoundInCache.Any())
                {
                    string message = "Following calendarSchedulePointers have no corresponding Section information in a cache, their calendar schedules will not be retrieved. " + string.Join(",", calendarPointersNotFoundInCache);
                    logger.Info(message);
                }

                if (sections.Any())
                {
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        internationalParameters = await GetInternationalParametersAsync();


                        if (startDate.HasValue)
                        {
                            startDatePart = string.Format("AND WITH CALS.DATE GE '{0}'", UniDataFormatter.UnidataFormatDate(startDate.Value, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter));
                        }
                        if (endDate.HasValue)
                        {
                            endDatePart = string.Format("AND WITH CALS.DATE LE '{0}'", UniDataFormatter.UnidataFormatDate(endDate.Value, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter));
                        }
                    }
                    foreach (var section in sections)
                    {
                        try
                        {
                            string sectionId = section.Id;
                            if (section.PrimarySectionMeetings != null && section.PrimarySectionMeetings.Any())
                            {
                                sectionId = section.PrimarySectionId ?? section.Id;
                            }
                            string criteria = null;
                            if (startDatePart != null && endDatePart != null)
                            {
                                criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} {3} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, sectionId, startDatePart, endDatePart);
                            }
                            else if (startDatePart != null)
                            {
                                criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, sectionId, startDatePart);
                            }
                            else if (endDatePart != null)
                            {
                                criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' {2} BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, sectionId, endDatePart);
                            }
                            else
                            {
                                criteria = string.Format("WITH CALS.TYPE = '{0}' AND WITH CALS.POINTER = '{1}' BY CALS.DATE BY CALS.START.TIME", calendarScheduleType, sectionId);
                            }
                            Collection<CalendarSchedules> cals = await DataReader.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", criteria);
                            if (!sectionWiseCalData.ContainsKey(section))
                            {
                                sectionWiseCalData.Add(section, cals);
                            }
                            else
                            {
                                logger.Info(string.Format("Section with id {0} is already in dictionary with its calendar schedules", section.Id));
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, ex.Message);
                        }
                    }
                }
                calendarEvents = BuildEvents(sectionWiseCalData);
            }
            catch (Exception e)
            {
                logger.Error("Error occured while retrieving section's calendar schedule events for iCal");
                logger.Error(e, e.Message);
            }
            return calendarEvents;
        }

        /// <summary>
        /// Get all midterm grading complete indications for a section
        /// </summary>
        /// <param name="sectionId">The section ID</param>
        /// <returns></returns>
        public async Task<SectionMidtermGradingComplete> GetSectionMidtermGradingCompleteAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionID", "Section ID is required to get grading complete information for a section.");
            }

            // If no grading completion indications have been recorded, this will just return an object with only the section ID
            var sectionGradingComplete = new SectionMidtermGradingComplete(sectionId);

            var secGradingStatuses = await DataReader.ReadRecordAsync<SecGradingStatus>("SEC.GRADING.STATUS", sectionId);

            // A SEC.GRADING.STATUS record may not exist for the section which is a valid condition. Just return the object with no 
            // midterm grading complete data.
            if (secGradingStatuses != null)
            {
                foreach (SecGradingStatusSgsMidGrade1Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade1CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade1CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 1 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    } else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade1CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                                sgsMidGradeComplete.SgsMidGrade1CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 1 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading1Complete(sgsMidGradeComplete.SgsMidGrade1CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }

                foreach (SecGradingStatusSgsMidGrade2Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade2CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade2CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 2 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    }
                    else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade2CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                            sgsMidGradeComplete.SgsMidGrade2CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 2 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading2Complete(sgsMidGradeComplete.SgsMidGrade2CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }

                foreach (SecGradingStatusSgsMidGrade3Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade3CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade3CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 3 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    }
                    else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade3CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                            sgsMidGradeComplete.SgsMidGrade3CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 3 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading3Complete(sgsMidGradeComplete.SgsMidGrade3CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }

                foreach (SecGradingStatusSgsMidGrade4Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade4CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade4CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 4 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    }
                    else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade4CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                            sgsMidGradeComplete.SgsMidGrade4CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 4 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading4Complete(sgsMidGradeComplete.SgsMidGrade4CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }

                foreach (SecGradingStatusSgsMidGrade5Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade5CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade5CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 5 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    }
                    else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade5CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                            sgsMidGradeComplete.SgsMidGrade5CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 5 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading5Complete(sgsMidGradeComplete.SgsMidGrade5CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }

                foreach (SecGradingStatusSgsMidGrade6Complete sgsMidGradeComplete in secGradingStatuses.SgsMidGrade6CompleteEntityAssociation)
                {
                    // Log and discard record if operator is null
                    if (string.IsNullOrEmpty(sgsMidGradeComplete.SgsMidGrade6CmplOpersAssocMember))
                    {
                        var errorMessage = "The grading complete operator for a midterm grading 6 complete indication is blank. Not returning this grading complete indicator for the section {0}";
                        logger.Info(errorMessage, sectionId);
                    }
                    else
                    {
                        DateTimeOffset? completeDate = sgsMidGradeComplete.SgsMidGrade6CmplTimesAssocMember.ToPointInTimeDateTimeOffset(
                            sgsMidGradeComplete.SgsMidGrade6CmplDatesAssocMember, colleagueTimeZone);

                        // The date/time stamp should never be null. If it is, log and discard this record.
                        if (completeDate == null)
                        {
                            var errorMessage = "The date and time indicator for midterm grading 6 produced a null return value from ToPointInTimeDateTimeOffset. Not returning this grading complete indicator for the section {0}";
                            logger.Info(errorMessage, sectionId);
                        }
                        else
                        {
                            // We know the date is not null, but must coalesce to match the expected non-nullable DateTime type
                            sectionGradingComplete.AddMidtermGrading6Complete(sgsMidGradeComplete.SgsMidGrade6CmplOpersAssocMember, completeDate ?? new DateTime());
                        }
                    }
                }
            }

            return sectionGradingComplete;
        }
        public async Task<SectionMidtermGradingComplete> PostSectionMidtermGradingCompleteAsync(string sectionId, int? midtermGradeNumber, string completeOperator, DateTimeOffset? dateAndTime)
        {
            if (sectionId == null)
            {
                throw new ArgumentNullException("SectionId", "SectionId is not provided");
            }

            if (midtermGradeNumber == null)
            {
                throw new ArgumentNullException("MidtermGradeNumber", "MidtermGradeNumber is not provided");
            }

            if ((midtermGradeNumber < 1) || (midtermGradeNumber > 6))
            {
                throw new ArgumentException("MidtermGradeNumber", "MidtermGradeNumber must be between 1 and 6");
            }

            if (string.IsNullOrEmpty(completeOperator))
            {
                throw new ArgumentNullException("CompleteOperator", "CompleteOperator is not provided");
            }

            if (dateAndTime == null)
            {
                throw new ArgumentNullException("DateAndTime", "DateAndTime is not provided");
            }

            var request = new AddMidtermGradeCompleteRequest();
            request.ACourseSectionId = sectionId;
            request.AMidtermGradeNumber = midtermGradeNumber;
            request.AOperator = completeOperator;
            // Convert the date/time, already checked for NULL, to local time, then divide into date and time parts expected by the CTX.
            DateTimeOffset? localDateTime = dateAndTime.ToLocalDateTime(colleagueTimeZone);
            request.ADate = localDateTime.Value.Date;
            request.ATime = localDateTime.Value.DateTime;

            var response = new AddMidtermGradeCompleteResponse();

            try
            {
                response = await transactionInvoker.ExecuteAsync<AddMidtermGradeCompleteRequest, AddMidtermGradeCompleteResponse>(request);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The Colleague transaction failed recording midterm grading complete for section {0}" , sectionId);
                throw new ApplicationException("Unable to record midterm grading complete");
            }

            if (!string.IsNullOrEmpty(response.AErrorMsg))
            {
                logger.Error("The Colleague transaction returned the following error recording midterm grading complete for section {0}. " + response.AErrorMsg, sectionId);
                throw new ApplicationException("Unable to record midterm grading complete");
            }

            // Return the updated midterm grading complete information for the section
            return await GetSectionMidtermGradingCompleteAsync(sectionId);

        }

        private async Task<SectionFaculty> UpdateSectionFacultyAsync(SectionFaculty sectionFaculty, string guid)
        {
            var exception = new RepositoryException();

            if (sectionFaculty == null)
            {
                exception.AddError(new RepositoryError("section", "Section Faculty cannot be null."));
            }
            if (guid == null)
            {
                exception.AddError(new RepositoryError("guid", "Section Faculty GUID cannot be null."));
            }

            if (exception.Errors.Any())
            {
                throw exception;
            }

            // Pass the section faculty data down to a Colleague transaction to do the record add/update
            var request = new UpdateSectionFacultyRequest()
            {
                CsfGuid = sectionFaculty.Guid.ToString().Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) ? string.Empty : sectionFaculty.Guid,
                CsfPrimaryFlag = sectionFaculty.PrimaryIndicator,
                CourseSecFacultyId = sectionFaculty.Id,
                CsfCourseSection = sectionFaculty.SectionId,
                CsfInstrMethod = sectionFaculty.InstructionalMethodCode,
                CsfFaculty = sectionFaculty.FacultyId,
                CsfFacultyLoad = sectionFaculty.LoadFactor,
                CsfFacultyPct = sectionFaculty.ResponsibilityPercentage,
                CsfStartDate = sectionFaculty.StartDate,
                CsfEndDate = sectionFaculty.EndDate
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateSectionFacultyRequest, UpdateSectionFacultyResponse>(request);

            if (response.UpdateSectionFacultyWarnings != null && response.UpdateSectionFacultyWarnings.Count > 0)
            {
                foreach (var warning in response.UpdateSectionFacultyWarnings)
                {
                    string code = warning.WarningCodes ?? string.Empty;
                    string msg = warning.WarningMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(code + msg))
                    {
                        string message = string.Format("WARNING - {0}: {1}", code, msg);
                        logger.Warn(msg);
                    }
                }
            }

            if (response.UpdateSectionFacultyErrors != null && response.UpdateSectionFacultyErrors.Count > 0)
            {
                exception.AddError(new RepositoryError("section", "Errors encountered while updating section-instructors " + sectionFaculty.Id));
                foreach (var error in response.UpdateSectionFacultyErrors)
                {
                    // If the code is null, just log the error message, unless it's blank, too
                    string errorCode = error.ErrorCodes ?? string.Empty;
                    string errorMessage = error.ErrorMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        string message = string.Format("ERROR - {0}: {1}", errorCode, errorMessage);
                        logger.Error(message);
                    }
                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        exception.AddError(new RepositoryError(errorCode, errorMessage));
                    }
                }
                throw exception;
            }
            var sectionFacultyGuid = string.IsNullOrEmpty(response.CsfGuid) ? null : response.CsfGuid;
            if (string.IsNullOrEmpty(response.CsfGuid) && !string.IsNullOrEmpty(response.CourseSecFacultyId))
            {
                sectionFacultyGuid = await GetSectionFacultyGuidFromIdAsync(response.CourseSecFacultyId);
            }
            var sectionFacultyEntity = await GetSectionFacultyByGuidAsync(sectionFacultyGuid);
            return sectionFacultyEntity == null ? null : sectionFacultyEntity;
        }

        /// <summary>
        /// Delete a single section faculty
        /// </summary>
        /// <param name="id">ID of the section meeting</param>
        public async Task DeleteSectionFacultyAsync(SectionFaculty faculty, string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException("Record not found, no delete occurred.");
            }
            // Now we have an ID, so we can pass of the rest of the work to a transaction
            var request = new DeleteSectionInstructorsRequest()
            {
                CourseSecFacultyId = faculty.Id,
                CsfGuid = guid
            };

            var response = await transactionInvoker.ExecuteAsync<DeleteSectionInstructorsRequest, DeleteSectionInstructorsResponse>(request);

            if (response.DeleteSectionInstructorsErrors != null && response.DeleteSectionInstructorsErrors.Count > 0)
            {
                var exception = new RepositoryException("Errors encountered while deleting section instructors " + guid);
                foreach (var error in response.DeleteSectionInstructorsErrors)
                {
                    exception.AddError(new RepositoryError(string.IsNullOrEmpty(error.ErrorCodes) ? "" : error.ErrorCodes, error.ErrorMessages));
                }
                throw exception;
            }
        }

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook"><see cref="SectionTextbook"/></param>
        /// <returns>An updated <see cref="Section"/> object.</returns>
        public async Task<Section> UpdateSectionBookAsync(SectionTextbook textbook)
        {
            if (textbook == null)
            {
                throw new ArgumentNullException("textbook", "Textbook can not be null.");
            }

            if (string.IsNullOrEmpty(textbook.SectionId))
            {
                throw new ArgumentNullException("textbook.SectionId", "Section Id may not be null or empty.");
            }
            if (textbook.Textbook == null)
            {
                throw new ArgumentNullException("textbook.Textbook", "The book being updated can not be null.");
            }

            string actionCode = ConvertSectionBookActionToInternalCode(textbook.Action);

            var request = new UpdateSectionTextbooksRequest();

            request.InAction = actionCode;
            request.InBookId = textbook.Textbook.Id;
            request.InCourseSectionsId = textbook.SectionId;
            request.InSecBookOptions = textbook.RequirementStatusCode;


            var response = await transactionInvoker.ExecuteAsync<UpdateSectionTextbooksRequest, UpdateSectionTextbooksResponse>(request);

            if (response.OutWarningMsgs != null && response.OutWarningMsgs.Count > 0)
            {
                response.OutWarningMsgs.ForEach(message => logger.Warn(message));
            }

            if (response.OutErrorMsgs != null && response.OutErrorMsgs.Count > 0)
            {
                string error = "An error occurred while trying to add book " + textbook.Textbook.Id + " to section " + textbook.SectionId;
                logger.Error(error);
                response.OutErrorMsgs.ForEach(message => logger.Error(message));
                throw new ApplicationException(error);
            }

            var sectionEntity = await GetSectionAsync(textbook.SectionId);
            return sectionEntity;
        }

        /// <summary>
        /// Gets the calendar schedule for a specific section
        /// </summary>
        /// <param name="sectionId">Id of section</param>
        /// <returns>Events for the section</returns>
        /// <exception cref="System.ArgumentNullException">section Id is required</exception>
        public async Task<IEnumerable<SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "section Id may not be null or empty");
            }
            // Get cached reg control ID for the given user. If not found, call transaction to get it and store it.
            var sectionMeetingInstances = await GetOrAddToCacheAsync<IEnumerable<SectionMeetingInstance>>(SectionMeetingInstancesCache + sectionId,
                async () =>
                {
                    var section = await DataReader.ReadRecordAsync<CourseSections>(sectionId, false);
                    if (section == null)
                    {
                        throw new KeyNotFoundException("section " + sectionId + " does not exist.");
                    }
                    List<SectionMeetingInstance> results = new List<SectionMeetingInstance>();
                    if (section.SecCalendarSchedules != null && section.SecCalendarSchedules.Any())
                    {
                        var calsData = await DataReader.BulkReadRecordAsync<CalendarSchedules>("CALENDAR.SCHEDULES", section.SecCalendarSchedules.ToArray());
                        Collection<CourseSecMeeting> sectionMeetingData = null;
                        if (calsData != null && calsData.Any() && section.SecMeeting != null && section.SecMeeting.Any())
                        {
                            sectionMeetingData = await DataReader.BulkReadRecordAsync<CourseSecMeeting>("COURSE.SEC.MEETING", section.SecMeeting.ToArray());
                        }
                        results = BuildSectionMeetingInstances(calsData, sectionMeetingData);
                    }
                    return results;
                }
                , sectionMeetingInstanceCacheTimeout);
            return sectionMeetingInstances;
        }

        private List<SectionMeetingInstance> BuildSectionMeetingInstances(Collection<CalendarSchedules> calsData, Collection<CourseSecMeeting> sectionMeetingData)
        {
            var cals = new List<SectionMeetingInstance>();
            if (calsData != null && calsData.Any())
            {
                foreach (var cal in calsData)
                {
                    try
                    {
                        // Calculate the start/end datetimeoffset value based on the Colleague time zone for the given date
                        if (!cal.CalsDate.HasValue || cal.CalsDate == new DateTime(1968, 1, 1))
                        {
                            var calString = "Calendar Schedule Id: " + cal.Recordkey + "Type: " + cal.CalsType + "Pointer: " + cal.CalsPointer + " Description: " + cal.CalsDescription + "Date " + cal.CalsDate.ToString();
                            LogDataError("Error: Invalid Calendar Schedule - missing valid date.", cal.Recordkey, cal, null, calString);
                            throw new Exception("Calendar item must have at least a date.");
                        }
                        if (string.IsNullOrEmpty(cal.CalsPointer))
                        {
                            var calString = "Calendar Schedule Id: " + cal.Recordkey + "Type: " + cal.CalsType + "Pointer: " + cal.CalsPointer + " Description: " + cal.CalsDescription + "Date " + cal.CalsDate.ToString();
                            LogDataError("Error: Invalid Calendar Schedule - missing pointer.", cal.Recordkey, cal, null, calString);
                            throw new Exception("Calendar item must pointer to build a section meeting instance.");
                        }

                        DateTimeOffset? meetingStartTime = cal.CalsStartTime.HasValue ? cal.CalsStartTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                        DateTimeOffset? meetingEndTime = cal.CalsEndTime.HasValue ? cal.CalsEndTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;
                        var calEvent = new SectionMeetingInstance(cal.Recordkey,
                            cal.CalsPointer,
                            cal.CalsDate.Value,
                            meetingStartTime,
                            meetingEndTime);
                        if (sectionMeetingData != null && sectionMeetingData.Any())
                        {
                            var matchingSecMeetings = sectionMeetingData.Where(sm => sm.Recordkey == cal.CalsCourseSecMeeting);
                            var secMeeting = (matchingSecMeetings != null) ? matchingSecMeetings.FirstOrDefault() : null;
                            if (secMeeting != null)
                            {
                                calEvent.InstructionalMethod = string.IsNullOrEmpty(secMeeting.CsmInstrMethod) ? null : secMeeting.CsmInstrMethod.ToUpper();
                            }

                        }
                        cals.Add(calEvent);
                    }
                    catch (Exception ex)
                    {
                        var calString = "Calendar Schedule Id: " + cal.Recordkey + "Type: " + cal.CalsType + "Pointer: " + cal.CalsPointer + " Description: " + cal.CalsDescription + "Date " + cal.CalsDate.ToString();
                        LogDataError("Error: Invalid Calendar Schedule", cal.Recordkey, cal, ex, calString);
                    }
                }
            }
            return cals;
        }


        /// <summary>
        /// Converts a <see cref="SectionBookAction"/> object to an appropriate internal code
        /// </summary>
        /// <param name="status">The <see cref="SectionBookAction"/> to convert</param>
        /// <returns>Internal code representation of the <see cref="SectionBookAction"/></returns>
        private string ConvertSectionBookActionToInternalCode(SectionBookAction action)
        {
            switch (action)
            {
                case SectionBookAction.Remove:
                    return "X";
                case SectionBookAction.Add:
                    return "A";
                default:
                    return "U";
            }
        }

        /// <summary>
        /// Build a section meeting
        /// </summary>
        /// <param name="csm"></param>
        /// <returns></returns>
        private async Task<SectionMeeting> BuildSectionMeetingAsync(CourseSecMeeting csm, IEnumerable<CourseSecFaculty> allSectionFaculty = null)
        {
            string room = csm.CsmBldg + "*" + csm.CsmRoom;
            var meeting = new SectionMeeting(csm.Recordkey, csm.CsmCourseSection, csm.CsmInstrMethod, csm.CsmStartDate, csm.CsmEndDate, csm.CsmFrequency)
            {
                Guid = csm.RecordGuid,
                Room = room,
                Days = CalculateDays(csm),
                IsOnline = await IsInstructionOnlineAsync(csm.CsmInstrMethod),
                Load = csm.CsmLoad
            };
            if (csm.CsmFaculty != null && csm.CsmFaculty.Count > 0)
            {
                meeting.AddFacultyIds(csm.CsmFaculty);
            }

            // Meeting time is a time-only field that has no related date-only field.
            // Return meeting time combined with today's date so that 
            // Daylight Saving Time can be accounted for.
            meeting.StartTime = csm.CsmStartTime.HasValue ?
                csm.CsmStartTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;

            meeting.EndTime = csm.CsmEndTime.HasValue ?
                csm.CsmEndTime.ToTimeOfDayDateTimeOffset(colleagueTimeZone) : null;

            // Calculate the total minutes of this meeting
            try
            {
                meeting.TotalMeetingMinutes = await CalculateMeetingMinutesAsync(meeting);
            }
            catch
            {
                // meeting minutes could not be calculated, leave null
            }

            if (allSectionFaculty == null)
            {
                var limitingKeys = await DataReader.SelectAsync("COURSE.SECTIONS", new string[] { csm.CsmCourseSection }, "WITH SEC.FACULTY BY.EXP SEC.FACULTY SAVING SEC.FACULTY");
                if (limitingKeys == null || !limitingKeys.Any())
                {
                    return meeting;
                }
                // Get the faculty info for this section, limiting it by the section faculty, if there are any
                allSectionFaculty = await DataReader.BulkReadRecordAsync<CourseSecFaculty>(limitingKeys.Distinct().ToArray());
            }
            if (allSectionFaculty == null)
            {
                return meeting;
            }

            foreach (var member in allSectionFaculty)
            {
                if (csm.CsmFaculty != null && csm.CsmFaculty.Any())
                {
                    if (csm.CsmFaculty.Contains(member.CsfFaculty))
                    {
                        meeting.AddSectionFaculty(BuildSectionFaculty(member));
                    }
                }
                else
                {
                    // Add faculty to meeting only if faculty is listed with the meeting's instructional method
                    if (member.CsfInstrMethod == csm.CsmInstrMethod)
                    {
                        meeting.AddSectionFaculty(BuildSectionFaculty(member));
                    }
                }
            }

            return meeting;
        }

        /// <summary>
        /// Build a SectionFaculty object from a CourseSecFaculty object
        /// </summary>
        /// <param name="csf">CourseSecFaculty object</param>
        /// <returns>SectionFaculty object</returns>
        private SectionFaculty BuildSectionFaculty(CourseSecFaculty csf)
        {
            // If the faculty percent responsible is 0%, log it (it's not an error, shouldn't stop processing, but logging it just in case it's of use)
            if (csf.CsfFacultyPct <= 0)
            {
                logger.Info("CourseSecFaculty.RecordKey: " + csf.Recordkey + " has CsfFacultyPct of " + csf.CsfFacultyPct);
            }
            if (!string.IsNullOrEmpty(csf.RecordGuid))
            {
                var secFaculty = new SectionFaculty(csf.RecordGuid, csf.Recordkey, csf.CsfCourseSection, csf.CsfFaculty, csf.CsfInstrMethod, csf.CsfStartDate.GetValueOrDefault(),
                    csf.CsfEndDate.GetValueOrDefault(), csf.CsfFacultyPct.GetValueOrDefault())
                {
                    LoadFactor = csf.CsfFacultyLoad,
                    ContractAssignment = csf.CsfPacLpAsgmt,
                    TeachingArrangementCode = csf.CsfTeachingArrangement
                };

                return secFaculty;
            }
            else
            {
                var secFaculty = new SectionFaculty(csf.Recordkey, csf.CsfCourseSection, csf.CsfFaculty, csf.CsfInstrMethod, csf.CsfStartDate.GetValueOrDefault(),
                    csf.CsfEndDate.GetValueOrDefault(), csf.CsfFacultyPct.GetValueOrDefault())
                {
                    LoadFactor = csf.CsfFacultyLoad,
                    ContractAssignment = csf.CsfPacLpAsgmt,
                    TeachingArrangementCode = csf.CsfTeachingArrangement
                };

                return secFaculty;
            }
        }

        /// <summary>
        /// Build a SectionFaculty object from a CourseSecFaculty object
        /// </summary>
        /// <param name="csf">CourseSecFaculty object</param>
        /// <returns>SectionFaculty object</returns>
        private SectionFaculty BuildEthosSectionFaculty(SectionFaculty sectionFacultyEntity, CourseSections courseSection, IEnumerable<CourseSecMeeting> sectionMeeting)
        {
            if (sectionMeeting != null && sectionMeeting.Any())
            {
                sectionFacultyEntity.SecMeetingIds = new List<string>();
                foreach (var meeting in sectionMeeting)
                {
                    if (meeting != null && !string.IsNullOrEmpty(meeting.Recordkey) && meeting.CsmInstrMethod == sectionFacultyEntity.InstructionalMethodCode)
                    {
                        sectionFacultyEntity.SecMeetingIds.Add(meeting.Recordkey);
                    }
                }
            }
            if (courseSection.SecFaculty != null && courseSection.SecFaculty.Contains(sectionFacultyEntity.Id))
            {
                var index = courseSection.SecFaculty.IndexOf(sectionFacultyEntity.Id);
                if (index == 0)
                {
                    sectionFacultyEntity.PrimaryIndicator = true;
                }
            }

            return sectionFacultyEntity;
        }

        /// <summary>
        /// Calculate the days of the week to which the section meeting applies
        /// </summary>
        /// <param name="meeting">The section meeting</param>
        /// <returns>A List of days of the week</returns>
        private List<DayOfWeek> CalculateDays(CourseSecMeeting meeting)
        {
            List<DayOfWeek> days = new List<DayOfWeek>();
            if (meeting.CsmSunday == "Y") days.Add(DayOfWeek.Sunday);
            if (meeting.CsmMonday == "Y") days.Add(DayOfWeek.Monday);
            if (meeting.CsmTuesday == "Y") days.Add(DayOfWeek.Tuesday);
            if (meeting.CsmWednesday == "Y") days.Add(DayOfWeek.Wednesday);
            if (meeting.CsmThursday == "Y") days.Add(DayOfWeek.Thursday);
            if (meeting.CsmFriday == "Y") days.Add(DayOfWeek.Friday);
            if (meeting.CsmSaturday == "Y") days.Add(DayOfWeek.Saturday);

            return days;
        }

        private async Task<bool> IsInstructionOnlineAsync(string instrMethod)
        {
            var matchingInstructionalMethods = (await InstructionalMethodsAsync()).Where(x => x.Code == instrMethod);
            var instruction = (matchingInstructionalMethods != null) ? matchingInstructionalMethods.FirstOrDefault() : null;
            return instruction != null && instruction.IsOnline;
        }

        private async Task<SectionMeeting> UpdateSectionMeetingAsync(Section section, string meetingGuid)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }
            if (meetingGuid == null)
            {
                throw new ArgumentNullException("meetingGuid");
            }
            var meeting = (section.Meetings != null) ? section.Meetings.FirstOrDefault(x => x.Guid == meetingGuid) : null;
            if (meeting == null)
            {
                throw new KeyNotFoundException("Section meeting not found with GUID " + meetingGuid);
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            // Pass the section data down to a Colleague transaction to do the record add/update
            var request = new UpdateInstructionalEventRequest()
            {
                CourseSecMeetingId = string.IsNullOrEmpty(meeting.Id) ? string.Empty : meeting.Id,
                CsmGuid = meeting.Guid.ToString().Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) ? string.Empty : meeting.Guid,
                CsmCourseSection = meeting.SectionId,
                CsmInstrMethod = meeting.InstructionalMethodCode,
                CsmStartDate = meeting.StartDate.HasValue ? meeting.StartDate : null,
                CsmStartTime = meeting.StartTime.HasValue ? meeting.StartTime.ToLocalDateTime(colleagueTimeZone) : null,
                CsmEndDate = meeting.EndDate.HasValue ? meeting.EndDate : null,
                CsmEndTime = meeting.EndTime.HasValue ? meeting.EndTime.ToLocalDateTime(colleagueTimeZone) : null,
                CsmFrequency = meeting.Frequency,
                CsmMonday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Monday) : false,
                CsmTuesday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Tuesday) : false,
                CsmWednesday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Wednesday) : false,
                CsmThursday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Thursday) : false,
                CsmFriday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Friday) : false,
                CsmSaturday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Saturday) : false,
                CsmSunday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Sunday) : false,
                CsmLoad = meeting.Load,
                CsmFaculty = meeting.FacultyIds != null && meeting.FacultyIds.Any() ? meeting.FacultyIds.ToList() : new List<string>(),
                OverrideFacultyAvailability = meeting.OverrideFacultyAvailability,
                OverrideFacultyCapacity = meeting.OverrideFacultyCapacity,
                OverrideRoomAvailability = meeting.OverrideRoomAvailability,
                OverrideRoomCapacity = meeting.OverrideRoomCapacity
            };

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            if (meeting.Room != null && meeting.Room.Any())
            {
                var room = meeting.Room.Contains('*') ? meeting.Room.Split('*') : new string[2] { meeting.Room, string.Empty };
                if (room != null)
                {
                    request.CsmBldg = room[0];
                    request.CsmRoom = room[1];
                }
            }

            request.FacultyRoster = new List<FacultyRoster>();
            foreach (var csf in meeting.FacultyRoster)
            {
                request.FacultyRoster.Add(new FacultyRoster()
                {
                    FacCsfId = string.IsNullOrEmpty(csf.Id) ? "$NEW" : csf.Id,
                    FacInstrMethod = csf.InstructionalMethodCode,
                    FacFaculty = csf.FacultyId,
                    FacStartDate = csf.StartDate,
                    FacEndDate = csf.EndDate,
                    FacFacultyLoad = csf.LoadFactor,
                    FacFacultyPct = csf.ResponsibilityPercentage,
                    FacPacLpAsgmt = csf.ContractAssignment,
                    FacTeachingArrangement = csf.TeachingArrangementCode
                });
            }
            var facultyList = meeting.FacultyRoster.Select(x => x.FacultyId).Distinct().ToList();
            request.SecMeet = new List<SecMeet>();
            foreach (var csm in section.Meetings)
            {
                request.SecMeet.Add(new SecMeet()
                {
                    SecmeetId = csm.Id,
                    SecmeetFaculty = csm.FacultyIds.Count == 0 ? string.Empty : string.Join(DmiString.sSM, csm.FacultyIds.ToArray())
                });
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateInstructionalEventRequest, UpdateInstructionalEventResponse>(request);

            if (response.UpdateInstructionalEventWarnings != null && response.UpdateInstructionalEventWarnings.Count > 0)
            {
                foreach (var warning in response.UpdateInstructionalEventWarnings)
                {
                    string code = warning.WarningCodes ?? string.Empty;
                    string msg = warning.WarningMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(code + msg))
                    {
                        string message = string.Format("WARNING - {0}: {1}", code, msg);
                        logger.Warn(msg);
                    }
                }
            }

            if (response.UpdateInstructionalEventErrors != null && response.UpdateInstructionalEventErrors.Count > 0)
            {
                var exception = new RepositoryException("Errors encountered while updating instructional event " + meeting.Id);
                foreach (var error in response.UpdateInstructionalEventErrors)
                {
                    // If the code is null, just log the error message, unless it's blank, too
                    string errorCode = error.ErrorCodes ?? string.Empty;
                    string errorMessage = error.ErrorMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        string message = string.Format("ERROR - {0}: {1}", errorCode, errorMessage);
                        logger.Error(message);
                    }
                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        exception.AddError(new RepositoryError(errorCode, errorMessage));
                    }
                }
                throw exception;
            }

            return string.IsNullOrEmpty(response.CourseSecMeetingId) ? null : await GetSectionMeetingAsync(response.CourseSecMeetingId);
        }

        /// <summary>
        /// Creates/Updates instructional event V11
        /// </summary>
        /// <param name="section"></param>
        /// <param name="meetingGuid"></param>
        /// <returns></returns>
        private async Task<SectionMeeting> UpdateSectionMeeting2Async(Section section, string meetingGuid)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }
            if (meetingGuid == null)
            {
                throw new ArgumentNullException("meetingGuid");
            }
            var meeting = (section.Meetings != null) ? section.Meetings.FirstOrDefault(x => x.Guid == meetingGuid) : null;
            if (meeting == null)
            {
                throw new KeyNotFoundException("Section meeting not found with GUID " + meetingGuid);
            }

            var extendedDataTuple = GetEthosExtendedDataLists();

            // Pass the section data down to a Colleague transaction to do the record add/update
            var request = new UpdateInstructionalEventV2Request()
            {
                CourseSecMeetingId = string.IsNullOrEmpty(meeting.Id) ? string.Empty : meeting.Id,
                CsmGuid = meeting.Guid.ToString().Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) ? string.Empty : meeting.Guid,
                CsmCourseSection = meeting.SectionId,
                CsmInstrMethod = meeting.InstructionalMethodCode,
                CsmStartDate = meeting.StartDate.HasValue ? meeting.StartDate : null,
                CsmStartTime = meeting.StartTime.HasValue ? meeting.StartTime.ToLocalDateTime(colleagueTimeZone) : null,
                CsmEndDate = meeting.EndDate.HasValue ? meeting.EndDate : null,
                CsmEndTime = meeting.EndTime.HasValue ? meeting.EndTime.ToLocalDateTime(colleagueTimeZone) : null,
                CsmFrequency = meeting.Frequency,
                CsmMonday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Monday) : false,
                CsmTuesday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Tuesday) : false,
                CsmWednesday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Wednesday) : false,
                CsmThursday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Thursday) : false,
                CsmFriday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Friday) : false,
                CsmSaturday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Saturday) : false,
                CsmSunday = meeting.Days != null && meeting.Days.Any() ? meeting.Days.Contains(DayOfWeek.Sunday) : false,
                CsmLoad = meeting.Load,
                OverrideRoomAvailability = meeting.OverrideRoomAvailability,
                OverrideRoomCapacity = meeting.OverrideRoomCapacity
            };

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            if (meeting.Room != null && meeting.Room.Any())
            {
                var room = meeting.Room.Contains('*') ? meeting.Room.Split('*') : new string[2] { meeting.Room, string.Empty };
                if (room != null)
                {
                    request.CsmBldg = room[0];
                    request.CsmRoom = room[1];
                }
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateInstructionalEventV2Request, UpdateInstructionalEventV2Response>(request);

            if (response.UpdateInstructionalEventWarnings2 != null && response.UpdateInstructionalEventWarnings2.Any())
            {
                foreach (var warning in response.UpdateInstructionalEventWarnings2)
                {
                    string code = warning.WarningCodes ?? string.Empty;
                    string msg = warning.WarningMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(code + msg))
                    {
                        string message = string.Format("WARNING - {0}: {1}", code, msg);
                        logger.Warn(msg);
                    }
                }
            }

            if (response.UpdateInstructionalEventErrors2 != null && response.UpdateInstructionalEventErrors2.Any())
            {
                var exception = new RepositoryException("Errors encountered while updating instructional event " + meeting.Id);
                foreach (var error in response.UpdateInstructionalEventErrors2)
                {
                    // If the code is null, just log the error message, unless it's blank, too
                    string errorCode = error.ErrorCodes ?? string.Empty;
                    string errorMessage = error.ErrorMessages ?? string.Empty;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        string message = string.Format("ERROR - {0}: {1}", errorCode, errorMessage);
                        logger.Error(message);
                    }
                    if (!string.IsNullOrEmpty(errorCode))
                    {
                        exception.AddError(new RepositoryError(errorCode, errorMessage));
                    }
                }
                throw exception;
            }

            return string.IsNullOrEmpty(response.CourseSecMeetingId) ? null : await GetSectionMeetingAsync(response.CourseSecMeetingId);
        }

        private async Task<IEnumerable<InstructionalMethod>> InstructionalMethodsAsync()
        {

            var im = await GetGuidCodeItemAsync<InstrMethods, InstructionalMethod>("AllInstructionalMethods", "INSTR.METHODS",
                (i, g) => new InstructionalMethod(g, i.Recordkey, i.InmDesc, i.InmOnline == "Y"), Level1CacheTimeoutValue);
            return im;
        }


        private async Task<IEnumerable<Domain.Base.Entities.ScheduleRepeat>> GetScheduleRepeatsAsync()
        {
            return await GetValcodeAsync<Domain.Base.Entities.ScheduleRepeat>("CORE", "SCHED.REPEATS", r =>
                (new Domain.Base.Entities.ScheduleRepeat(r.ValInternalCodeAssocMember, r.ValExternalRepresentationAssocMember, r.ValActionCode1AssocMember,
                    ConvertFrequencyCodeToFrequencyType(r.ValActionCode2AssocMember))), Level1CacheTimeoutValue);
        }

        private Domain.Base.Entities.FrequencyType? ConvertFrequencyCodeToFrequencyType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code)
            {
                case "D":
                    return Domain.Base.Entities.FrequencyType.Daily;
                case "W":
                    return Domain.Base.Entities.FrequencyType.Weekly;
                case "M":
                    return Domain.Base.Entities.FrequencyType.Monthly;
                case "Y":
                    return Domain.Base.Entities.FrequencyType.Yearly;
            }
            return null;
        }

        private async Task<SectionStatus> ConvertStatusCodeToSectionStatusAsync(string status)
        {
            var statusCodes = await GetSectionStatusCodesAsync();
            var statusEntry = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.Code == status) : null;
            return statusEntry == null || !statusEntry.StatusType.HasValue ? SectionStatus.Inactive : statusEntry.StatusType.Value;
        }

        private async Task<string> ConvertSectionStatusToStatusCodeAsync(SectionStatus status)
        {
            switch (status)
            {
                case SectionStatus.Active:
                    return await GetActiveStatusAsync();
                case SectionStatus.Cancelled:
                    return await GetCancelledStatusAsync();
                default:
                    return await GetOtherStatusAsync();
            }
        }

        private async Task<SectionStatusIntegration> ConvertStatusCodeToSectionIntegrationStatusAsync(string status)
        {
            var statusCodes = await GetSectionStatusCodesAsync();
            var statusEntry = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.Code == status) : null;
            return statusEntry == null || !statusEntry.IntegrationStatusType.HasValue ? SectionStatusIntegration.Pending : statusEntry.IntegrationStatusType.Value;
        }

        public async Task<string> ConvertSectionIntegrationStatusToStatusCodeAsync(SectionStatusIntegration status)
        {
            var retval = string.Empty;
            var statusCodes = await GetSectionStatusCodesAsync();

            switch (status)
            {
                case SectionStatusIntegration.Open:
                    var sectionStatusCodeOpen = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.IntegrationStatusType == SectionStatusIntegration.Open) : null;
                    if (sectionStatusCodeOpen != null) retval = sectionStatusCodeOpen.Code;
                    break;
                case SectionStatusIntegration.Closed:
                    var sectionStatusCodeClosed = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.IntegrationStatusType == SectionStatusIntegration.Closed) : null;
                    if (sectionStatusCodeClosed != null) retval = sectionStatusCodeClosed.Code;
                    break;
                case SectionStatusIntegration.Cancelled:
                    var sectionStatusCodeCancelled = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.IntegrationStatusType == SectionStatusIntegration.Cancelled) : null;
                    if (sectionStatusCodeCancelled != null) retval = sectionStatusCodeCancelled.Code;
                    break;
                default:
                    var sectionStatusCodePending = (statusCodes != null) ? statusCodes.FirstOrDefault(ss => ss.IntegrationStatusType == SectionStatusIntegration.Pending) : null;
                    if (sectionStatusCodePending != null) retval = sectionStatusCodePending.Code;
                    break;
            }

            return retval;
        }

        /// <summary>
        /// Return a Unidata Select formatted list of Section Status codes from a string of either
        /// "Cancelled", "Open", "Closed" or "Pending"
        /// </summary>
        /// <param name="status">Status String</param>
        /// <returns>Unidata Select formatted Status Codes from Colleague Valcode table SECTION.STATUSES</returns>
        public async Task<string> ConvertStatusToStatusCodeAsync(string status)
        {
            string statuses = string.Empty;
            List<string> statusList = new List<string>();
            switch (status.ToLower())
            {
                case ("open"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Open).Select(ss => ss.Code).ToList());
                    break;
                case ("cancelled"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Cancelled).Select(ss => ss.Code).ToList());
                    break;
                case ("closed"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Closed).Select(ss => ss.Code).ToList());
                    break;
                default:
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Pending).Select(ss => ss.Code).ToList());
                    break;
            }
            foreach (var stat in statusList)
            {
                statuses += "'" + stat + "'";
            }
            return statuses;
        }

        /// <summary>
        /// Return a Unidata Select formatted list of Section Status codes from a string of either
        /// "Cancelled", "Open", "Closed" or "Pending" with no default.  Throw exception if supplied 
        /// value is not in the listed enumeration.  For EEDM use.
        /// </summary>
        /// <param name="status">Status String</param>
        /// <returns>Unidata Select formatted Status Codes from Colleague Valcode table SECTION.STATUSES</returns>
        public async Task<string> ConvertStatusToStatusCodeNoDefaultAsync(string status)
        {
            string statuses = string.Empty;
            List<string> statusList = new List<string>();
            switch (status.ToLower())
            {
                case ("open"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Open).Select(ss => ss.Code).ToList());
                    break;
                case ("cancelled"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Cancelled).Select(ss => ss.Code).ToList());
                    break;
                case ("closed"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Closed).Select(ss => ss.Code).ToList());
                    break;
                case ("pending"):
                    statusList.AddRange((await GetSectionStatusCodesAsync()).Where(ss => ss.IntegrationStatusType == SectionStatusIntegration.Pending).Select(ss => ss.Code).ToList());
                    break;
                default:
                    throw new ArgumentException("Supplied status value of " + status + " is not in the list of allowable values: \"open\", \"cancelled\", \"closed\", \"pending\".");
            }
            foreach (var stat in statusList)
            {
                statuses += "'" + stat + "'";
            }
            return statuses;
        }
        /// <summary>
        /// Convert a section status action code into a section status type
        /// </summary>
        /// <param name="action">The action code of the section status</param>
        /// <returns>The section status</returns>
        public SectionStatus ConvertSectionStatusActionToStatusType(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return SectionStatus.Inactive;
            }
            switch (action)
            {
                case "1":
                    return SectionStatus.Active;
                case "2":
                    return SectionStatus.Cancelled;
                default:
                    return SectionStatus.Inactive;
            }
        }

        /// <summary>
        /// Convert a section status action code into a section status type
        /// </summary>
        /// <param name="action">The action code of the section status</param>
        /// <returns>The section status</returns>
        public SectionStatusIntegration ConvertSectionStatusActionToIntegrationStatusType(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return SectionStatusIntegration.NotSet;
            }
            switch (action)
            {
                case "open":
                    return SectionStatusIntegration.Open;
                case "closed":
                    return SectionStatusIntegration.Closed;
                case "cancelled":
                    return SectionStatusIntegration.Cancelled;
                default:
                    return SectionStatusIntegration.Pending;
            }
        }

        public async Task<IEnumerable<WaitlistStatusCode>> GeWaitlistStatusCodesAsync()
        {
            List<WaitlistStatusCode> waitListStatusCodes = new List<WaitlistStatusCode>();
            waitListStatusCodes = (await GetValcodeAsync<WaitlistStatusCode>("ST", "WAIT.LIST.STATUSES",
                wl => new WaitlistStatusCode(wl.ValInternalCodeAssocMember, wl.ValExternalRepresentationAssocMember, ConvertWaitlistActionToStatus(wl.ValActionCode1AssocMember)), Level1CacheTimeoutValue)).ToList();
            return waitListStatusCodes;
        }

        private WaitlistStatus ConvertWaitlistActionToStatus(string actionCode)
        {
            WaitlistStatus status;
            if (Enum.TryParse<WaitlistStatus>(actionCode, out status))
            {
                return status;
            }
            return WaitlistStatus.Unknown;
        }


        private async Task<WaitlistStatus> GetWaitlistStatusAsync(string waitlistStatusCode)
        {
            if (string.IsNullOrEmpty(waitlistStatusCode))
            {
                throw new ArgumentNullException("waitlistStatusCode");
            }
            var entry = (await GeWaitlistStatusCodesAsync()).FirstOrDefault(x => x.Code == waitlistStatusCode);
            if (entry == null)
            {
                return WaitlistStatus.Unknown;
            }

            return entry.Status;
        }

        /// <summary>
        /// Calculate the total number of minutes for a SectionMeeting
        /// </summary>
        /// <param name="meeting">The SectionMeeting</param>
        /// <returns>Number of minutes it meets</returns>
        private async Task<int> CalculateMeetingMinutesAsync(SectionMeeting meeting)
        {
            if (!meeting.StartTime.HasValue || !meeting.EndTime.HasValue)
            {
                return 0;
            }

            // Calculate all the meeting dates of the section
            var frequencyType = ConvertFrequencyCodeToFrequencyType(meeting.Frequency) == null ? FrequencyType.Weekly : ConvertFrequencyCodeToFrequencyType(meeting.Frequency).Value;
            var repeatCode = (await GetScheduleRepeatsAsync()).FirstOrDefault(x => x.Code == meeting.Frequency);
            var interval = repeatCode == null ? 1 : repeatCode.Interval.Value;
            var campusCalendar = await CampusCalendarAsync();
            var meetingDates = RoomAvailabilityService.BuildDateList(meeting.StartDate.Value, meeting.EndDate.Value, frequencyType, interval,
                meeting.Days, campusCalendar.BookedEventDates, campusCalendar.BookPastNumberOfDays);
            var meetingTime = meeting.EndTime.Value.TimeOfDay - meeting.StartTime.Value.TimeOfDay;

            return (((meetingTime.Hours * 60) + meetingTime.Minutes) * meetingDates.Count());
        }

        /// <summary>
        /// Build a BookedEventDate
        /// </summary>
        /// <param name="id">Calendar Schedule record ID</param>
        /// <returns>DateTime representing the Booked Event Date</returns>
        private async Task<DateTime> BuildBookedEventDateAsync(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
            {
                throw new ArgumentNullException("scheduleId", "ID must be provided.");
            }
            var calendarScheduleData = await DataReader.ReadRecordAsync<CalendarSchedules>(scheduleId);
            if (calendarScheduleData == null)
            {
                throw new KeyNotFoundException("CalendarSchedules record not found for ID " + scheduleId);
            }
            return calendarScheduleData.CalsDate.Value;
        }

        private IEnumerable<ScheduleRepeat> _scheduleRepeat = null;
        private async Task<IEnumerable<ScheduleRepeat>> GetScheduleRepeatAsync()
        {
            if (_scheduleRepeat == null)
            {
                _scheduleRepeat = await GetValcodeAsync<ScheduleRepeat>("CORE", "SCHED.REPEATS",
                s => new ScheduleRepeat(s.ValInternalCodeAssocMember, s.ValExternalRepresentationAssocMember, s.ValActionCode1AssocMember,
                    ConvertCodeToFrequencyType(s.ValActionCode2AssocMember)));
            }
            return _scheduleRepeat;
        }

        private FrequencyType? ConvertCodeToFrequencyType(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            switch (code)
            {
                case "D":
                    return FrequencyType.Daily;
                case "W":
                    return FrequencyType.Weekly;
                case "M":
                    return FrequencyType.Monthly;
                case "Y":
                    return FrequencyType.Yearly;
                default:
                    return null;
            }
        }

        private async Task<Data.Student.DataContracts.StwebDefaults> GetStwebDefaultsAsync()
        {
            Data.Student.DataContracts.StwebDefaults studentWebDefaults = await GetOrAddToCacheAsync<Data.Student.DataContracts.StwebDefaults>("StudentWebDefaults",
            async () =>
            {
                Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", false);
                if (stwebDefaults == null)
                {
                    var errorMessage = "Unable to access student web defaults from ST.PARMS. STWEB.DEFAULTS.";
                    logger.Info(errorMessage);
                    stwebDefaults = new StwebDefaults();
                }
                return stwebDefaults;
            }, Level1CacheTimeoutValue);
            return studentWebDefaults;
        }

        private async Task<string> GetBookstoreUrlTemplateAsync()
        {
            Ellucian.Colleague.Data.Student.DataContracts.StwebDefaults stwebDefaults = await GetStwebDefaultsAsync();
            string template = stwebDefaults.StwebBookstoreUrlTemplate;
            if (!string.IsNullOrEmpty(template))
            {
                template = template.Replace(DmiString.sVM, string.Empty);
            }
            return template;
        }

        private async Task<bool> RequisitesConvertedAsync()
        {
            var crsParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Student.DataContracts.CdDefaults>("CourseParameters",
                async () =>
                {
                    Data.Student.DataContracts.CdDefaults courseParams = await DataReader.ReadRecordAsync<Data.Student.DataContracts.CdDefaults>("ST.PARMS", "CD.DEFAULTS");
                    if (courseParams == null)
                    {
                        var errorMessage = "Unable to access course parameters CD.DEFAULTS to determine CoreqPrereq conversion flag. Defaulting to unconverted.";
                        logger.Info(errorMessage);
                        // If we cannot read the course parameters - default to "unconverted".
                        // throw new Exception(errorMessage);
                        Data.Student.DataContracts.CdDefaults newCourseParams = new Data.Student.DataContracts.CdDefaults();
                        newCourseParams.CdReqsConvertedFlag = "N";
                        courseParams = newCourseParams;
                    }
                    return courseParams;
                }, Level1CacheTimeoutValue);
            return (crsParameters.CdReqsConvertedFlag == "Y") ? true : false;
        }

        private string FindBestFit(DateTime? startDate, DateTime? endDate)
        {
            string term = "";
            if (startDate.HasValue)
            {
                // fetch this once, and only once needed
                if (termList == null) termList = termRepository.Get();
                if (termList != null && termList.Any())
                {
                    var testTerms = termList.Where(t => ((t.StartDate.CompareTo(startDate.Value) <= 0 && t.EndDate.CompareTo(startDate.Value) >= 0) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && (endDate.HasValue && t.StartDate.CompareTo(endDate.Value) <= 0)) ||
                            (t.StartDate.CompareTo(startDate.Value) >= 0 && !endDate.HasValue)));
                    if (testTerms != null && testTerms.Any())
                    {
                        term = testTerms.First().Code;
                    }
                }
            }
            return term;
        }

        /// <summary>
        /// Converts grade entity to dataset that transaction can use
        /// </summary>
        private List<ItemsToPostInput> BuildImportPostItems(StudentGrade studentGrade)
        {
            List<ItemsToPostInput> postItems = new List<ItemsToPostInput>();

            if (studentGrade.MidtermGrade1 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade1",
                    ItemValue = studentGrade.MidtermGrade1,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.MidtermGrade2 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade2",
                    ItemValue = studentGrade.MidtermGrade2,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.MidtermGrade3 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade3",
                    ItemValue = studentGrade.MidtermGrade3,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.MidtermGrade4 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade4",
                    ItemValue = studentGrade.MidtermGrade4,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.MidtermGrade5 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade5",
                    ItemValue = studentGrade.MidtermGrade5,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.MidtermGrade6 != null)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "MidtermGrade6",
                    ItemValue = studentGrade.MidtermGrade6,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                });
            }

            if (studentGrade.FinalGrade != null)
            {
                ItemsToPostInput item = new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "FinalGrade",
                    ItemValue = studentGrade.FinalGrade,
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                };

                // Final grade expiration date is included with Final grade if both exist
                string delimiter = "|";

                if (studentGrade.ClearFinalGradeExpirationDateFlag)
                {
                    item.ItemValue += delimiter; // empty date value
                }
                else
                {
                    if (studentGrade.FinalGradeExpirationDate.HasValue)
                    {
                        string dateString = studentGrade.FinalGradeExpirationDate.Value.ToString("yyyy/MM/dd");
                        item.ItemValue += delimiter + dateString;
                    }
                }

                postItems.Add(item);
            }
            else
            {
                if (studentGrade.ClearFinalGradeExpirationDateFlag)
                {
                    postItems.Add(new ItemsToPostInput()
                    {
                        ItemPerson = studentGrade.StudentId,
                        ItemCode = "FinalGradeExpirationDate",
                        ItemValue = "",
                        ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                        ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                    });
                }
                else
                {
                    if (studentGrade.FinalGradeExpirationDate.HasValue)
                    {
                        postItems.Add(new ItemsToPostInput()
                        {
                            ItemPerson = studentGrade.StudentId,
                            ItemCode = "FinalGradeExpirationDate",
                            ItemValue = studentGrade.FinalGradeExpirationDate.Value.ToString("yyyy/MM/dd"),
                            ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                            ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                        });
                    }
                }
            }

            if (studentGrade.ClearLastAttendanceDateFlag)
            {
                postItems.Add(new ItemsToPostInput()
                {
                    ItemPerson = studentGrade.StudentId,
                    ItemCode = "LastAttendanceDate",
                    ItemValue = "",
                    ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                    ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null

                });
            }
            else
            {
                if (studentGrade.LastAttendanceDate.HasValue)
                {
                    postItems.Add(new ItemsToPostInput()
                    {
                        ItemPerson = studentGrade.StudentId,
                        ItemCode = "LastAttendanceDate",
                        ItemValue = studentGrade.LastAttendanceDate.Value.ToString("yyyy/MM/dd"),
                        ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                        ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null

                    });
                }
            }

            if (studentGrade.NeverAttended.HasValue)
            {
                if (studentGrade.NeverAttended.Value)
                {
                    postItems.Add(new ItemsToPostInput()
                    {
                        ItemPerson = studentGrade.StudentId,
                        ItemCode = "NeverAttended",
                        ItemValue = "1",
                        ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                        ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                    });
                }
                else
                {
                    postItems.Add(new ItemsToPostInput()
                    {
                        ItemPerson = studentGrade.StudentId,
                        ItemCode = "NeverAttended",
                        ItemValue = "0",
                        ItemStartDate = studentGrade.EffectiveStartDate.HasValue ? studentGrade.EffectiveStartDate.Value.ToString("yyyy/MM/dd") : null,
                        ItemEndDate = studentGrade.EffectiveEndDate.HasValue ? studentGrade.EffectiveEndDate.Value.ToString("yyyy/MM/dd") : null
                    });
                }
            }

            return postItems;
        }
        /// <summary>
        /// Read the international parameters records to extract date format used
        /// locally and setup in the INTL parameters.
        /// </summary>
        /// <returns>International Parameters with date properties</returns>
        new private async Task<Ellucian.Colleague.Data.Base.DataContracts.IntlParams> GetInternationalParametersAsync()
        {
            if (internationalParameters != null)
            {
                return internationalParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            internationalParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Base.DataContracts.IntlParams>("InternationalParameters",
                async () =>
                {
                    Data.Base.DataContracts.IntlParams intlParams = await DataReader.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL");
                    if (intlParams == null)
                    {
                        var errorMessage = "Unable to access international parameters INTL.PARAMS INTERNATIONAL.";
                        logger.Info(errorMessage);
                        // If we cannot read the international parameters default to US with a / delimiter.
                        // throw new Exception(errorMessage);
                        Data.Base.DataContracts.IntlParams newIntlParams = new Data.Base.DataContracts.IntlParams();
                        newIntlParams.HostShortDateFormat = "MDY";
                        newIntlParams.HostDateDelimiter = "/";
                        intlParams = newIntlParams;
                    }
                    return intlParams;
                }, Level1CacheTimeoutValue);
            return internationalParameters;
        }

        private string LookupImportErrorMessage(string code)
        {
            switch (code)
            {
                case "MD-TID":
                    return "Missing Data: No transaction ID";
                case "MD-SID":
                    return "Missing Data: No  section ID";
                case "MD-TBL":
                    return "Missing Data: No items-to-post table";
                case "ID-SID":
                    return "Invalid Data: Section ID (no such course section exists)";
                case "ID-ICD":
                    return "Invalid Data: Item Code";
                default:
                    return "Unknown import error";
            }
        }

        /// <summary>
        /// Coverts the output from the import grades transaction to domain entities.
        /// </summary>
        /// <param name="transactionResponse">The import grades transaction response</param>
        /// <returns>   list of domain entities</returns>
        private List<SectionGradeResponse> ConvertImportOutputToDomainEntities(ImportGradesFromILPResponse transactionResponse)
        {
            // The output from the transaction contains a list of each grade/person/status (for a single section) imported.
            // This needs to be consolidated to a domain entity list by person.

            List<SectionGradeResponse> domainEntities = new List<SectionGradeResponse>();

            // Get unique list of people
            List<string> persons = (from o in transactionResponse.ItemsToPostOutput
                                    group o by o.ItemOutPerson into g
                                    select g.Key).ToList<string>();

            foreach (string person in persons)
            {
                SectionGradeResponse entity = new SectionGradeResponse();
                entity.StudentId = person;

                var errorOutputs = transactionResponse.ItemsToPostOutput
                    .Where(x => x.ItemOutPerson == person && x.ItemOutStatus == "failure")
                    .Select(x => x);

                if (errorOutputs.Any())
                {
                    // If there are any failure statuses for a person then domain entity is a failure. Collect all the error messages.
                    entity.Status = "failure";

                    foreach (ItemsToPostOutput error in errorOutputs)
                    {
                        entity.Errors.Add(new SectionGradeResponseError() { Message = error.ItemErrorMsg, Property = error.ItemOutCode });
                    }
                }
                else
                {
                    // Multiple success outputs are consolidated to one domain entity
                    var successOutputs = transactionResponse.ItemsToPostOutput
                        .Where(x => x.ItemOutPerson == person && x.ItemOutStatus == "success")
                        .Select(x => x);

                    if (successOutputs.Any())
                    {
                        entity.Status = "success";
                    }
                }

                domainEntities.Add(entity);
            }

            return domainEntities;
        }

        /// <summary>
        /// Get the time that the changed registration sections cache was last built.
        /// </summary>
        /// <returns>A DateTime representing the last time the changed registration section cache was built</returns>
        public DateTime GetChangedRegistrationSectionsCacheBuildTime()
        {
            return ChangedRegistrationSectionsCacheBuildTime;
        }

        private async Task<ApplValcodes> GetWaitlistStatusesAsync()
        {
            if (waitlistStatuses != null)
            {
                return waitlistStatuses;
            }

            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            waitlistStatuses = await GetOrAddToCacheAsync<ApplValcodes>("WaitlistStatuses",
                async () =>
                {
                    ApplValcodes waitlistStatusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES");
                    if (waitlistStatusesTable == null)
                    {
                        // log this but don't throw exception because not all clients use wait lists.
                        var errorMessage = "Unable to access WAIT.LIST.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        waitlistStatusesTable = new ApplValcodes() { ValsEntityAssociation = new List<ApplValcodesVals>() };
                    }
                    return waitlistStatusesTable;
                }, Level1CacheTimeoutValue);
            return waitlistStatuses;
        }

        private async Task<string> GetWaitlistStatusActionCodeAsync(string waitlistStatusCode)
        {
            if (!String.IsNullOrEmpty(waitlistStatusCode))
            {
                var matchingWaitlistStatuses = (await GetWaitlistStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == waitlistStatusCode);
                var codeAssoc = (matchingWaitlistStatuses != null) ? matchingWaitlistStatuses.FirstOrDefault() : null;
                if (codeAssoc != null)
                {
                    return codeAssoc.ValActionCode1AssocMember;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the list of student waitlist statuses
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<StudentWaitlistStatus>> GetStudentWaitlistStatusesAsync()
        {           
            List<StudentWaitlistStatus> studentWaitlistStatuses = (await GetWaitlistStatusesAsync()).ValsEntityAssociation.Select(y => new StudentWaitlistStatus(statuscode: y.ValActionCode1AssocMember, status: y.ValInternalCodeAssocMember, statusdescription: y.ValExternalRepresentationAssocMember)).ToList();                                                                                 
            return studentWaitlistStatuses; 
        }

        /// <summary>
        /// Convert a list of strings into a single quote-delimited string
        /// </summary>
        /// <param name="stringList"></param>
        /// <returns></returns>
        private string QuoteDelimit(IEnumerable<string> stringList)
        {
            if (stringList == null || stringList.Select(i => (!string.IsNullOrEmpty(i))).Count() == 0)
            {
                return null;
            }
            else
            {
                return "'" + (string.Join(" ", stringList.ToArray())).Replace(" ", "' '") + "'";
            }
        }

        /// <summary>
        /// Get a collection of <see cref="BookOption">book options</see>
        /// </summary>
        /// <returns>A collection of <see cref="BookOption">book options</see></returns>
        private async Task<IEnumerable<BookOption>> GetBookOptionsAsync()
        {
            if (bookOptions != null)
            {
                return bookOptions;
            }
            bookOptions = await GetValcodeAsync<BookOption>("ST", "BOOK.OPTION",
                op => new BookOption(op.ValInternalCodeAssocMember, op.ValExternalRepresentationAssocMember, op.ValActionCode1AssocMember == "1"));

            return bookOptions;
        }

        /// <summary>
        /// Build a SpecialDay object for the given data contracts
        /// </summary>
        /// <param name="specialDayRecord"></param>
        /// <param name="calendarDayTypesValcode"></param>
        /// <returns></returns>
        private SpecialDay BuildSpecialDay(CampusSpecialDay specialDayRecord, ApplValcodes calendarDayTypesValcode)
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
            var dayTypeValcodeEntry = calendarDayTypesValcode.ValsEntityAssociation != null ?
                                      calendarDayTypesValcode.ValsEntityAssociation.FirstOrDefault(v => specialDayRecord.CmsdType.Equals(v.ValInternalCodeAssocMember, StringComparison.CurrentCultureIgnoreCase)) :
                                      null;

            //its a holiday if the valcode's special action code 1 equals HO
            var isHoliday = dayTypeValcodeEntry != null && dayTypeValcodeEntry.ValActionCode1AssocMember != null &&
                dayTypeValcodeEntry.ValActionCode1AssocMember.Equals("HO", StringComparison.CurrentCultureIgnoreCase);


            bool isFullDay;
            DateTimeOffset startDateTime;
            DateTimeOffset endDateTime;
            if (specialDayRecord.CmsdStartTime.HasValue && specialDayRecord.CmsdEndTime.HasValue)
            {
                //not a full day when a start and end time are specified
                isFullDay = false;

                //convert to offset
                startDateTime = specialDayRecord.CmsdStartTime.ToPointInTimeDateTimeOffset(specialDayRecord.CmsdStartDate, colleagueTimeZone).Value;
                endDateTime = specialDayRecord.CmsdEndTime.ToPointInTimeDateTimeOffset(specialDayRecord.CmsdEndDate, colleagueTimeZone).Value;
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
                endDateTime);

            return specialDay;
        }

        /// <summary>
        /// Converts SEC.ATTEND.TRACKING.TYPE for a course section to its corresponding <see cref="AttendanceTrackingType"/>
        /// </summary>
        /// <param name="section"></param>
        /// <returns><see cref="AttendanceTrackingType"/></returns>
        private AttendanceTrackingType ConvertStringToAttendanceTrackingType(CourseSections section)
        {
            try
            {
                if (section == null)
                {
                    throw new ArgumentNullException("section", "Cannot determine attendance tracking type for null section.");
                }
                if (string.IsNullOrEmpty(section.SecAttendTrackingType))
                {
                    return AttendanceTrackingType.PresentAbsent;
                }
                switch (section.SecAttendTrackingType)
                {
                    case "P":
                        return AttendanceTrackingType.PresentAbsent;
                    case "A":
                        return AttendanceTrackingType.HoursByDateWithoutSectionMeeting;
                    case "S":
                        return AttendanceTrackingType.HoursBySectionMeeting;
                    case "T":
                        return AttendanceTrackingType.CumulativeHours;
                    default:
                        string error = string.Format("COURSE.SECTIONS record '{0}' has an invalid SEC.ATTEND.TRACKING.TYPE value of '{1}'", section.Recordkey, section.SecAttendTrackingType);
                        throw new ApplicationException(error);
                }
            }
            catch (Exception ex)
            {
                LogDataError("Section Attendance Tracking Type", section.Recordkey, section, ex);
                throw ex;
            }
        }

        private IEnumerable<Event> BuildEvents(Dictionary<Section, Collection<CalendarSchedules>> sectionsWiseCalendarScehdules)
        {
            var cals = new List<Event>();
            if (sectionsWiseCalendarScehdules == null)
            {
                throw new ArgumentNullException("sectionsWiseCalendarScehdules", "Sections calendar schedules may not be null");

            }

            foreach (var sectionWiseCal in sectionsWiseCalendarScehdules)
            {
                try
                {
                    Section section = sectionWiseCal.Key;
                    string sectionDescription = section.PrimarySectionMeetings != null && section.PrimarySectionMeetings.Any() ? string.Join(" ", section.Name, section.Title) : null;
                    Collection<CalendarSchedules> calData = sectionWiseCal.Value;
                    if (calData != null)
                    {
                        foreach (var cal in calData)
                        {

                            try
                            {
                                // Calculate the start/end datetimeoffset value based on the Colleague time zone for the given date
                                if (!cal.CalsDate.HasValue || cal.CalsDate == new DateTime(1968, 1, 1))
                                {
                                    throw new Exception("Calendar item must have at least a date.");
                                }
                                DateTimeOffset startDateTime = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(cal.CalsStartTime, cal.CalsDate, colleagueTimeZone).GetValueOrDefault();
                                DateTimeOffset endDateTime = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(cal.CalsEndTime, cal.CalsDate, colleagueTimeZone).GetValueOrDefault();
                                var calEvent = new Event(cal.Recordkey,
                                    sectionDescription ?? cal.CalsDescription,
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
                                var calString = String.Format("Calendar Schedule couldn't be retrieved for Id:{0} sectionId:{1} Type:{2} Pointer:{3}", cal.Recordkey, section.Id, cal.CalsType, cal.CalsPointer);
                                logger.Error(ex, calString);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, e.Message);

                }
            }

            return cals;
        }
    }
}
