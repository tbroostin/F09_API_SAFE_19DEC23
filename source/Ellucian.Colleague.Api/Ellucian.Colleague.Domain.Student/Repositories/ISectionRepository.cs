// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Coordination service interface for sections
    /// </summary>
    public interface ISectionRepository : IEthosExtended
    {
        /// <summary>
        /// Get a section using its record ID
        /// </summary>
        /// <param name="id">Record ID</param>
        /// <returns>The section</returns>
        Task<Section> GetSectionAsync(string id, bool ignoreFaculty = false, bool useSeatServiceWhenEnabled = false);

        /// <summary>
        /// Get section using filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="learningProvider"></param>
        /// <param name="termId"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="department"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "",
            string academicLevel = "", string course = "", string location = "", string status = "", string department = "",
            string subject = "", string instructor = "");

        /// <summary>
        /// Get section using filters for performance.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>getsectiona
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="learningProvider"></param>
        /// <param name="termId"></param>
        /// <param name="reportingTermId"></param>
        /// <param name="academicLevels"></param>
        /// <param name="course"></param>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="departments"></param>
        /// <param name="subject"></param>
        /// <param name="instructors"></param>
        /// <param name="scheduleTermId"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Section>, int>> GetSections2Async(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", List<string> instructors = null, string scheduleTermId = "", bool addToErrorCollection = false);

        /// <summary>
        /// Get section using filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="learningProvider"></param>
        /// <param name="termId"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="department"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "",
            List<string> academicLevels = null, string course = "", string location = "", string status = "", List<string> departments = null,
            string subject = "", string instructor = "");

        /// <summary>
        /// Get section using filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="learningProvider"></param>
        /// <param name="termId"></param>
        /// <param name="academicLevel"></param>
        /// <param name="reportingTermId"></param>
        /// <param name="course"></param>
        /// <param name="location"></param>
        /// <param name="status"></param>
        /// <param name="department"></param>
        /// <param name="subject"></param>
        /// <param name="instructors"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsAsync(int offset, int limit, string title = "", string startDate = "", string endDate = "",
            string code = "", string number = "", string learningProvider = "", string termId = "", string reportingTermId = "",
            List<string> academicLevel = null, string course = "", string location = "", string status = "", List<string> department = null,
            string subject = "", List<string> instructors = null, string scheduledTermId = "");


        /// <summary>        
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="searchable">Check if a section is searchable or hidden.  Required.</param>
        /// <returns>IEnumerable Sections domain entity</returns>   
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchableAsync(int offset, int limit, string searchable = "");

        /// <summary>        
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="searchable">Check if a section is searchable or hidden.  Required.</param>
        /// <returns>IEnumerable Sections domain entity</returns>   
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsSearchable1Async(int offset, int limit, string searchable = "", bool addToCollection = false);

        /// <summary>        
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="keyword">The string to search for.  Required.</param>
        /// <param name="bypassCache">use cache</param>
        /// <param name="caseSensitive">case sensative search</param>
        /// <returns>IEnumerable Sections domain entity</returns>

        Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeywordAsync(int offset, int limit, string keyword = "", 
            bool bypassCache = false, bool caseSensitive = false);

        /// <summary>
        /// Get an IEnumerable Sections domain entity using keyword search criteria
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="keyword"></param>
        /// <param name="bypassCache"></param>
        /// <param name="caseSensitive"></param>
        /// <param name="addToCollection"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Section>, int>> GetSectionsKeyword1Async(int offset, int limit, string keyword, bool bypassCache = false, bool caseSensitive = false,
            bool addToCollection = false);
        /// <summary>
        /// Return a Section Status code from a string of either
        /// "Cancelled", "Open", "Closed" or "Pending"
        /// </summary>
        /// <param name="status">Status String</param>
        /// <returns>Status Code from Colleague Valcode table SECTION.STATUSES</returns>
        Task<string> ConvertStatusToStatusCodeAsync(string status);
        
        /// <summary>
        /// Return a Section Status code from a string of either
        /// "Cancelled", "Open", "Closed" or "Pending"
        /// </summary>
        /// <param name="status">Status String</param>
        /// <returns>Status Code from Colleague Valcode table SECTION.STATUSES</returns>
        Task<string> ConvertStatusToStatusCodeNoDefaultAsync(string status);
        
        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        Task<string> GetCourseIdFromGuidAsync(string guid);

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        Task<string> GetUnidataFormattedDate(string date);

        /// <summary>
        /// Create a SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The section</param>
        /// <returns>The created sectionCrosslist</returns>
        Task<SectionCrosslist> CreateSectionCrosslistAsync(SectionCrosslist sectionCrosslist);

        /// <summary>
        /// Update a SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">The section</param>
        /// <returns>The updated sectionCrosslist</returns>
        Task<SectionCrosslist> UpdateSectionCrosslistAsync(SectionCrosslist sectionCrosslist);

        /// <summary>
        /// Delete a sectioncrosslist
        /// </summary>
        /// <param name="id">id of the sectioncrosslist to delete</param>
        Task DeleteSectionCrosslistAsync(string id);
        
        /// <summary>
        /// Get a single sectioncrosslist using an ID
        /// </summary>
        /// <param name="id">The sectioncrosslist ID</param>
        /// <returns>The sectioncrosslist</returns>
        Task<SectionCrosslist> GetSectionCrosslistAsync(string id);

        /// <summary>
        /// Get the GUID for a sectioncrosslist using its ID
        /// </summary>
        /// <param name="id">sectioncrosslist ID</param>
        /// <returns>Section GUID</returns>
        Task<string> GetSectionCrosslistGuidFromIdAsync(string id);

        /// <summary>
        /// Get a sectioncrosslist using its GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section</returns>
        Task<SectionCrosslist> GetSectionCrosslistByGuidAsync(string guid);

        /// <summary>
        /// Get the sectioncrosslist ID for a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary Key</returns>
        Task<string> GetSectionCrosslistIdFromGuidAsync(string guid);

        /// <summary>
        /// Gets a page of SectionCrosslist's which can be filtered by section guid
        /// </summary>
        /// <param name="offset">The position to start the paged return at</param>
        /// <param name="limit">the number of items to return for this page of results</param>
        /// <param name="section">The section id to filter SectionCrosslist list on</param>
        /// <returns>list of SectionCrosslist</returns>
        Task<Tuple<IEnumerable<SectionCrosslist>, int>> GetSectionCrosslistsPageAsync(int offset, int limit, string section = "");

        /// <summary>
        /// Get the GUID for a section using its ID
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <returns>Section GUID</returns>
        Task<string> GetSectionGuidFromIdAsync(string id);

        /// <summary>
        /// Get the GUID for a section meeting using its ID
        /// </summary>
        /// <param name="id">Section meeting ID</param>
        /// <returns>Section Meeting GUID</returns>
        Task<string> GetSectionMeetingGuidFromIdAsync(string id);

        /// <summary>
        /// Get a section using its GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section</returns>
        Task<Section> GetSectionByGuidAsync(string guid, bool ignoreFaculty = false);

        /// <summary>
        /// Get a section using its GUID V16
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="addToErrorCollection"></param>
        /// <returns></returns>
        Task<Section> GetSectionByGuid2Async(string guid, bool addToErrorCollection = false);

        /// <summary>
        /// Get the section ID for a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary Key</returns>
        Task<string> GetSectionIdFromGuidAsync(string guid);

        /// <summary>
        /// Post (create) a section
        /// </summary>
        /// <param name="section">Section to create</param>
        /// <returns>The new section</returns>
        Task<Section> PostSectionAsync(Section section);

        /// <summary>
        /// Put (create/update) a section
        /// </summary>
        /// <param name="section">The section to create/update</param>
        /// <returns>The created/updated section</returns>
        Task<Section> PutSectionAsync(Section section);

        /// <summary>
        /// Post (create) a section
        /// </summary>
        /// <param name="section">Section to create</param>
        /// <returns>The new section</returns>
        Task<Section> PostSection2Async(Section section);

        /// <summary>
        /// Put (create/update) a section
        /// </summary>
        /// <param name="section">The section to create/update</param>
        /// <returns>The created/updated section</returns>
        Task<Section> PutSection2Async(Section section);

        Task<IEnumerable<Section>> GetCourseSectionsCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> terms);
        Task<IEnumerable<Section>> GetNonCachedFacultySectionsAsync(IEnumerable<Term> terms, string facultyId, bool bestFit = false);
        Task<IEnumerable<Section>> GetCourseSectionsNonCachedAsync(IEnumerable<string> courseIds, IEnumerable<Term> terms);
        Task<IEnumerable<Section>> GetRegistrationSectionsAsync(IEnumerable<Term> terms);
        Task<IEnumerable<Section>> GetNonCachedSectionsAsync(IEnumerable<string> sectionIds, bool bestFit = false);
        Task<Dictionary<string,SectionSeats>> GetSectionsSeatsAsync(IEnumerable<string> sectionIds);
        Task<IEnumerable<Section>> GetCachedSectionsAsync(IEnumerable<string> sectionIds, bool bestFit = false);
        DateTime GetChangedRegistrationSectionsCacheBuildTime();
        DateTime GetChangedInstantEnrollmentSectionsCacheBuildTime();
        Task<SectionGradeSectionResponse> ImportGradesAsync(SectionGrades sectionGrades, bool forceNoVerifyFlag, bool checkForLocksFlag, GradesPutCallerTypes callerTypes, bool sendGradingCompleteEmail);
                
        /// <summary>
        /// Get a section meeting using its record ID
        /// </summary>
        /// <param name="id">Record ID</param>
        /// <returns>Section meeting</returns>
        Task<SectionMeeting> GetSectionMeetingAsync(string id);

        /// <summary>
        /// Get a section meeting using filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="section">Section Colleague Id</param>
        /// <param name="startDate">Meeting Start Date</param>
        /// <param name="startTime">Meeting Start Time</param>
        /// <param name="endDate">Meeting End Date</param>
        /// <param name="endTime">Meeting End Time</param>
        /// <param name="building">Meeting Building Code</param>
        /// <param name="room">Meeting Room Code</param>
        /// <param name="instructor">Instructor Person Id</param>
        /// <returns>Section meeting</returns>
        Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeetingAsync(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string term);


        /// <summary>
        /// Get a section meeting using filters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="section">Section Colleague Id</param>
        /// <param name="startDate">Meeting Start Date</param>
        /// <param name="startTime">Meeting Start Time</param>
        /// <param name="endDate">Meeting End Date</param>
        /// <param name="endTime">Meeting End Time</param>
        /// <param name="building">Meeting Building Code</param>
        /// <param name="room">Meeting Room Code</param>
        /// <param name="instructor">Instructor Person Id</param>
        /// <returns>Section meeting</returns>
        Task<Tuple<IEnumerable<SectionMeeting>, int>> GetSectionMeeting2Async(int offset, int limit, string section, string startDate, string endDate, string startTime, string endTime, List<string> buildings, List<string> rooms, List<string> instructors, string term);


        /// <summary>
        /// Get a section facult using filters
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="section">Section Id</param>
        /// <param name="instructor">Instructor Id</param>
        /// <returns>Section faculty</returns>
        Task<Tuple<IEnumerable<SectionFaculty>, int>> GetSectionFacultyAsync(int offset, int limit, string section, string instructor, List<string> instructionalEvents);

        /// <summary>
        /// Get a section meeting record ID using its GUID
        /// </summary>
        /// <param name="guid">the GUID</param>
        /// <returns>Section meeting ID</returns>
        Task<string> GetSectionMeetingIdFromGuidAsync(string guid);

        /// <summary>
        /// Get a section faculty record ID using its GUID
        /// </summary>
        /// <param name="guid">the GUID</param>
        /// <returns>Section faculty ID</returns>
        Task<string> GetSectionFacultyIdFromGuidAsync(string guid);

        /// <summary>
        /// Get a section meeting using its GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section meeting to get</returns>
        Task<SectionMeeting> GetSectionMeetingByGuidAsync(string guid);

        /// <summary>
        /// Get a section faculty using its GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>The section faculty to get</returns>
        Task<SectionFaculty> GetSectionFacultyByGuidAsync(string guid);

        /// <summary>
        /// Post (create) a section meeting
        /// </summary>
        /// <param name="section">The section the new meeting is in</param>
        /// <param name="meetingGuid">The GUID of the new section meeting</param>
        /// <returns>The created section meeting</returns>
        Task<SectionMeeting> PostSectionMeetingAsync(Section section, string meetingGuid);

        /// <summary>
        /// Post (create) a section meeting V11
        /// </summary>
        /// <param name="section">The section the new meeting is in</param>
        /// <param name="meetingGuid">The GUID of the new section meeting</param>
        /// <returns>The created section meeting</returns>
        Task<SectionMeeting> PostSectionMeeting2Async(Section section, string meetingGuid);

        /// <summary>
        /// Put (create/update) a section meeting
        /// </summary>
        /// <param name="section">The section the meeting is part of</param>
        /// <param name="meetingGuid">The GUID of the section meeting</param>
        /// <returns>The new/updated section meeting</returns>
        Task<SectionMeeting> PutSectionMeetingAsync(Section section, string meetingGuid);

        /// <summary>
        /// Put (create/update) a section meeting V11
        /// </summary>
        /// <param name="section">The section the meeting is part of</param>
        /// <param name="meetingGuid">The GUID of the section meeting</param>
        /// <returns>The new/updated section meeting</returns>
        Task<SectionMeeting> PutSectionMeeting2Async(Section section, string meetingGuid);

        /// <summary>
        /// Delete a section meeting
        /// </summary>
        /// <param name="id">ID of the section to delete</param>
        /// <param name="faculty"></param>
        Task DeleteSectionMeetingAsync(string id, List<SectionFaculty> faculty);

        /// <summary>
        /// Post (create) a section faculty
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to be created</param>
        /// <param name="guid">The GUID of the new section faculty</param>
        /// <returns>The created/updated section faculty</returns>
        Task<SectionFaculty> PostSectionFacultyAsync(SectionFaculty sectionFaculty, string guid);

        /// <summary>
        /// Put (create/update) a section faculty
        /// </summary>
        /// <param name="section">The section faculty to be updated</param>
        /// <param name="guid">The GUID of the section faculty</param>
        /// <returns>The created/updated section faculty</returns>
        Task<SectionFaculty> PutSectionFacultyAsync(SectionFaculty sectionFaculty, string guid);

        /// <summary>
        /// Delete a section faculty
        /// </summary>
        /// <param name="id">ID of the section faculty to delete</param>
        Task DeleteSectionFacultyAsync(SectionFaculty sectionFaculty, string guid);

        /// <summary>
        /// Get a waitlist record using its GUID
        /// </summary>
        /// <param name="guid">the GUID</param>
        /// <returns>The waitlist record</returns>
        Task<StudentSectionWaitlist> GetWaitlistFromGuidAsync(string guid);

        /// <summary>
        /// Get a section meeting record ID using its GUID
        /// </summary>
        /// <param name="guid">the GUID</param>
        /// <returns>Section meeting ID</returns>
        Task<Tuple<IEnumerable<StudentSectionWaitlist>, int>> GetWaitlistsAsync(int offset, int limit);

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook"><see cref="SectionTextbook"/></param>
        /// <returns>An updated <see cref="Section"/>  object.</returns>
        Task<Section> UpdateSectionBookAsync(SectionTextbook textbook);
        
        /// <summary>
        /// Get all status guids
        /// </summary>
        Task<IEnumerable<SectionStatusCodeGuid>> GetStatusCodesWithGuidsAsync();

        /// <summary>
        /// Gets the section meetings (events) for a specific section id
        /// </summary>
        /// <param name="sectionId">Id of section</param>
        /// <returns></returns>
        Task<IEnumerable<SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId);

        /// <summary>
        /// Get a <see cref="SectionRoster"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionRoster"/></returns>
        Task<SectionRoster> GetSectionRosterAsync(string sectionId);

        /// <summary>
        /// Get a <see cref="SectionWaitlist"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionWaitlist"/></returns>
        Task<SectionWaitlist> GetSectionWaitlistAsync(string sectionId);


        /// <summary>
        /// Get a <see cref="SectionWaitlistConfig"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="SectionWaitlistConfig"/></returns>
        Task<SectionWaitlistConfig> GetSectionWaitlistConfigAsync(string sectionId);

        /// <summary>
        /// Get a list of<see cref="SectionWaitlistStudent"/> for a given list of course section IDs
        /// </summary>
        /// <param name="sectionId">a list Course section IDs</param>
        /// <returns>A list of<see cref="SectionWaitlistStudent"/></returns>
        Task<IEnumerable<SectionWaitlistStudent>> GetSectionWaitlist2Async(List<string> sectionId);


        /// <summary>
        /// Get the list of student waitlist statuses
        /// </summary>
        /// <returns>A list of StudentWaitlistStatus</returns>
        Task<IEnumerable<StudentWaitlistStatus>> GetStudentWaitlistStatusesAsync();

        /// <summary>
        /// To get the waitlist details based on the section and student id
        /// Sends back the details on rank and rating of the waitlisted student for the section along with the config details of show rank and show rating
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="studentId"></param>
        /// <returns> StudentSectionWaitlistInfo </returns>
        Task<StudentSectionWaitlistInfo> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId);
        
        /// Using a collection of section ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="sectionIds">collection of person ids</param>
        /// <returns>Dictionary consisting of a sectionId (key) and guid (value)</returns>
        Task<Dictionary<string, string>> GetSectionGuidsCollectionAsync(IEnumerable<string> sectionIds);

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
        Task<IEnumerable<Event>> GetSectionEventsICalAsync(string calendarScheduleType, IEnumerable<string> calendarSchedulePointers, DateTime? startDate, DateTime? endDate);

        Task<SectionMidtermGradingComplete> GetSectionMidtermGradingCompleteAsync(string sectionId);
        Task<SectionMidtermGradingComplete> PostSectionMidtermGradingCompleteAsync(string sectionId, int? midtermGradeNumber, string completeOperator, DateTimeOffset? dateAndTime);

        /// <summary>
        /// Retrieve Instant Enrollment sections
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Section>> GetInstantEnrollmentSectionsAsync();
     /// <summary>
     /// Create a Census certification record
     /// </summary>
     /// <param name="sectionId"></param>
     /// <param name="censusCertificationDate"></param>
     /// <param name="censusCertificationPosition"></param>
     /// <param name="censusCertificationLabel"></param>
     /// <param name="censusCertificationRecordedDate"></param>
     /// <param name="censusCertificationRecordedTime"></param>
     /// <param name="personId"></param>
     /// <returns></returns>
        Task<SectionCensusCertification> CreateSectionCensusCertificationAsync(string sectionId, DateTime? censusCertificationDate, string censusCertificationPosition, string censusCertificationLabel, DateTime? censusCertificationRecordedDate, DateTimeOffset? censusCertificationRecordedTime, string personId);

        /// <summary>
        /// Retrieves the grading status for a course section
        /// </summary>
        /// <param name="sectionId">Unique identifier for the course section</param>
        /// <returns>Grading status for the specified course section</returns>
        Task<SectionGradingStatus> GetSectionGradingStatusAsync(string sectionId);

        /// <summary>
        /// <summary>
        /// Retrieves the data for departmental oversight person based  on the section name search performed
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="terms"></param>
        /// <param name="depts"></param>
        /// <returns>IEnumerable<DeptOversightSearchResult></returns>
        Task<IEnumerable<DeptOversightSearchResult>> GetDeptOversightSectionDetails(string keyword, IEnumerable<Term> terms, IEnumerable<string> depts);

        
        /// <summary>
        /// GetFacultySectionsAsync is used to retrieve sections taught by a faculties
        /// </summary>
        /// <param name="facultyId"></param>
        /// <param name="bestFit"></param>
        /// <returns></returns>
        Task<IEnumerable<Section>> GetFacultySectionsAsync(IEnumerable<string> facultyIds, bool bestFit = false);

        /// <summary>
        ///  GetSectionDates is used to retrieve sections date information for a given set of sections from their GUIDs
        /// </summary>
        /// <param name="guids"></param>
        /// <returns>Section entities</returns>
        Task<List<Section>> GetSectionsDatesByIds(List<string> guids);
    }
}
