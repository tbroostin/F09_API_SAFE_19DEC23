// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service interface for all things related to sections
    /// </summary>
    public interface ISectionCoordinationService : IBaseService
    {
        /// <summary>
        /// Retrieves roster information for a course section.
        /// </summary>
        /// <param name="sectionId">ID of the course section for which roster students will be retrieved</param>
        /// <returns>All <see cref="RosterStudent">students</see> in the course section</returns>
        Task<IEnumerable<Dtos.Student.RosterStudent>> GetSectionRosterAsync(string sectionId);

        /// <summary>
        /// Get a <see cref="Dtos.Student.SectionRoster"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="Dtos.Student.SectionRoster"/></returns>
        Task<Dtos.Student.SectionRoster> GetSectionRoster2Async(string sectionId);

        /// <summary>
        /// Get a<see cref="Dtos.Student.SectionWaitlist"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="Dtos.Student.SectionWaitlist"/></returns>
        Task<Dtos.Student.SectionWaitlist>GetSectionWaitlistAsync(string sectionId);

        /// <summary>
        /// Get a list of <see cref="Dtos.Student.SectionWaitlistStudent"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A list of<see cref="Dtos.Student.SectionWaitlistStudent"/></returns>
        Task<IEnumerable<Dtos.Student.SectionWaitlistStudent>> GetSectionWaitlist2Async(string sectionId);

        /// <summary>
        /// Get a list of <see cref="Dtos.Student.StudentWaitlistStatus"/>
        /// </summary>
        /// <returns>A list of<see cref="Dtos.Student.StudentWaitlistStatus"/> </returns>
        Task<IEnumerable<Dtos.Student.StudentWaitlistStatus>> GetStudentWaitlistStatusesAsync();


        /// <summary>
        /// Get a<see cref="Dtos.Student.SectionWaitlistConfig"/> for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A <see cref="Dtos.Student.SectionWaitlistConfig"/></returns>
        Task<Dtos.Student.SectionWaitlistConfig> GetSectionWaitlistConfigAsync(string sectionId);

        /// <summary>
        /// Gets the <see cref="Dtos.Student.StudentSectionWaitlistInfo"/> based on the student and section ID
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="studentId"></param>
        /// <returns>StudentSectionWaitlistInfo</returns>
        Task<Dtos.Student.StudentSectionWaitlistInfo> GetStudentSectionWaitlistsByStudentAndSectionIdAsync(string sectionId, string studentId);
        
        /// <summary>
        /// Create a section
        /// </summary>
        /// <param name="section">The DTO of the Section to create</param>
        /// <returns>A CDM-format Section DTO</returns>
        Task<Section> PostSectionAsync(Section section);

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">The DTO of the Section to update</param>
        /// <returns>A CDM-format Section DTO</returns>
        Task<Section> PutSectionAsync(Section section);

        /// <summary>
        /// Update a book assignment for a section.
        /// </summary>
        /// <param name="textbook">The textbook whose assignment to a specific section is being updated.</param>
        /// <returns>An updated <see cref="Section3"/> object.</returns>
        Task<Dtos.Student.Section3> UpdateSectionBookAsync(Dtos.Student.SectionTextbook textbook);

        /// <summary>
        /// Get a SectionMaximum2
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.SectionMaximum2>, int>> GetSectionsMaximum2Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "");

        /// <summary>
        /// Get a SectionMaximum3
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.SectionMaximum3>, int>> GetSectionsMaximum3Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null);



        /// <summary>
        /// Get a SectionMaximum4
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.SectionMaximum4>, int>> GetSectionsMaximum4Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null);

        /// <summary>
        /// Get selection criteria, convert it to codes as required and select 
        /// multiple sections from Colleague.
        /// </summary>
        /// <param name="titles">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevel">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningOrganization">Section Department equal to (guid)</param>
        /// <returns>List of SectionMaximum4 <see cref="Dtos.SectionMaximum5"/> objects representing matching sections</returns>
        Task<Tuple<IEnumerable<Dtos.SectionMaximum5>, int>> GetSectionsMaximum5Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "", string reportingAcademicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null, List<string> instructors = null, string scheduleAcademicPeriod = "", bool bypassCache = false);

        /// <summary>
        /// Get a SectionMaximum using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format SectionMaximum DTO</returns>
        Task<SectionMaximum2> GetSectionMaximumByGuid2Async(string guid);

        /// <summary>
        /// Get a SectionMaximum using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format SectionMaximum DTO</returns>
        Task<SectionMaximum3> GetSectionMaximumByGuid3Async(string guid);

        /// <summary>
        /// Get a SectionMaximum using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format SectionMaximum DTO</returns>
        Task<SectionMaximum4> GetSectionMaximumByGuid4Async(string guid);

        /// <summary>
        /// Get a SectionMaximum using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format SectionMaximum DTO</returns>
        Task<SectionMaximum5> GetSectionMaximumByGuid5Async(string guid, bool bypassCache);

        /// <summary>
        /// Get all with filter parameters
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <returns>page results of sections by filter criteria</returns>
        Task<Tuple<IEnumerable<Dtos.Section2>, int>> GetSections2Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "");

        /// <summary>
        /// Get all with filter parameters (V6)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <returns>page results of sections by filter criteria</returns>
        Task<Tuple<IEnumerable<Dtos.Section3>, int>> GetSections3Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            string academicLevel = "", string course = "", string site = "", string status = "", string owningOrganization = "");

        /// <summary>
        /// Get all with filter parameters (V8)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// <param name="search"></param>
        /// <param name="keyword"></param>
        /// <returns>page results of sections by filter criteria</returns>
        Task<Tuple<IEnumerable<Dtos.Section4>, int>> GetSections4Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "");

        /// <summary>
        /// Get a section using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section3> GetSection3ByGuidAsync(string guid);

        /// <summary>
        /// Get a section using its GUID
        /// </summary>
        /// <param name="guid">The section's GUID</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section4> GetSection4ByGuidAsync(string guid);

        /// <summary>
        /// Create a section
        /// </summary>
        /// <param name="section">The DTO of the Section to create</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section4> PostSection4Async(Section4 section);

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">The DTO of the Section to update</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section4> PutSection4Async(Section4 section);

        /// <summary>
        /// Create a section
        /// </summary>
        /// <param name="section">The DTO of the Section to create</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section3> PostSection3Async(Section3 section);

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">The DTO of the Section to update</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section3> PutSection3Async(Section3 section);

        /// <summary>
        /// Get an instructional event
        /// </summary>
        /// <param name="guid">GUID of the desired instructional event</param>
        /// <returns>A HEDM-format InstructionalEvent DTO</returns>
        Task<InstructionalEvent2> GetInstructionalEvent2Async(string id);
        Task<InstructionalEvent3> GetInstructionalEvent3Async(string id);
        Task<InstructionalEvent4> GetInstructionalEvent4Async(string id);
        Task<Tuple<IEnumerable<Dtos.InstructionalEvent2>, int>> GetInstructionalEvent2Async(int offset, int limit, string section = "", string startOn = "", string endOn = "", string room = "", string instructor = "");
        Task<Tuple<IEnumerable<Dtos.InstructionalEvent3>, int>> GetInstructionalEvent3Async(int offset, int limit, string section = "", string startOn = "", string endOn = "", List<string> rooms = null, List<string> instructor = null, string academicPeriod = "");
        Task<Tuple<IEnumerable<Dtos.InstructionalEvent4>, int>> GetInstructionalEvent4Async(int offset, int limit, string section = "", string startOn = "", string endOn = "", string academicPeriod = "");

        /// <summary>
        /// Create an instructional event
        /// </summary>
        /// <param name="meeting">The DTO of the InstructionalEvent to create</param>
        /// <returns>A HEDM-format InstructionalEvent DTO</returns>
        Task<InstructionalEvent2> CreateInstructionalEvent2Async(InstructionalEvent2 meeting);
        Task<InstructionalEvent3> CreateInstructionalEvent3Async(InstructionalEvent3 meeting);

        /// <summary>
        /// Update an instructional event
        /// </summary>
        /// <param name="meeting">The DTO of the InstructionalEvent to update</param>
        /// <returns>A HEDM-format InstructionalEvent DTO</returns>
        Task<InstructionalEvent2> UpdateInstructionalEvent2Async(InstructionalEvent2 meeting);
        Task<InstructionalEvent3> UpdateInstructionalEvent3Async(InstructionalEvent3 meeting);

        /// <summary>
        /// Delete an instructional event
        /// </summary>
        /// <param name="guid">GUID of the event to delete</param>
        Task DeleteInstructionalEventAsync(string guid);

        Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGradesAsync(Dtos.Student.SectionGrades sectionGrades);

        Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades2Async(Dtos.Student.SectionGrades2 sectionGrades);

        Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades3Async(Dtos.Student.SectionGrades3 sectionGrades);

        Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportGrades4Async(Dtos.Student.SectionGrades3 sectionGrades);

        Task<IEnumerable<Dtos.Student.SectionGradeResponse>> ImportIlpGrades1Async(Dtos.Student.SectionGrades3 sectionGrades);

        /// <summary>
        /// Create an instructional event V11
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        Task<InstructionalEvent4> CreateInstructionalEvent4Async(InstructionalEvent4 meeting);

        /// <summary>
        /// Update an instructional event V11
        /// </summary>
        /// <param name="instructionalEvent4"></param>
        /// <returns></returns>
        Task<InstructionalEvent4> UpdateInstructionalEvent4Async(InstructionalEvent4 instructionalEvent4);
        /// Gets the section meeting instances for a specific section
        /// </summary>
        /// <param name="sectionId">The section id.</param>
        /// <returns>SectionMeetingInstance DTOs</returns>
        Task<IEnumerable<Dtos.Student.SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId);

        /// <summary>
        /// Get all with filter parameters (V11)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="title"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="code"></param>
        /// <param name="number"></param>
        /// <param name="instructionalPlatform"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="academicLevel"></param>
        /// <param name="course"></param>
        /// <param name="site"></param>
        /// <param name="status"></param>
        /// <param name="owningOrganization"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// <param name="search"></param>
        /// <param name="keyword"></param>
        /// <returns>page results of sections by filter criteria</returns>
        Task<Tuple<IEnumerable<Dtos.Section5>, int>> GetSections5Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "");

        /// <summary>
        /// Get a section
        /// </summary>
        /// <param name="guid">The guid of the Section to get</param>
        /// <returns>A HEDM-format Section DTO</returns
        Task<Section5> GetSection5ByGuidAsync(string guid);

        /// <summary>
        /// Create a section
        /// </summary>
        /// <param name="section">The DTO of the Section to create</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section5> PostSection5Async(Section5 section);

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">The DTO of the Section to update</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section5> PutSection5Async(Section5 section);


        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="searchable">named query</param>
        /// <param name="keywordSearch">named query</param>
        /// <param name="subject">named query</param>
        /// <param name="instructor">named query</param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section6 <see cref="Dtos.Section6"/> objects representing matching sections</returns>
        Task<Tuple<IEnumerable<Dtos.Section6>, int>> GetSections6Async(int offset, int limit, string title = "", string startOn = "", string endOn = "",
            string code = "", string number = "", string instructionalPlatform = "", string academicPeriod = "", string reportingAcademicPeriod = "",
            List<string> academicLevel = null, string course = "", string site = "", string status = "", List<string> owningOrganization = null,
            string subject = "", string instructor = "", SectionsSearchable search = SectionsSearchable.NotSet, string keyword = "", bool bypassCache = false);

        /// <summary>
        /// Get a section
        /// </summary>
        /// <param name="guid">The guid of the Section to get</param>
        ///  <param name="bypassCache"></param>
        /// <returns>A HEDM-format Section DTO</returns
        Task<Section6> GetSection6ByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Create a section
        /// </summary>
        /// <param name="section">The DTO of the Section to create</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section6> PostSection6Async(Section6 section);

        /// <summary>
        /// Update a section
        /// </summary>
        /// <param name="section">The DTO of the Section to update</param>
        /// <returns>A HEDM-format Section DTO</returns>
        Task<Section6> PutSection6Async(Section6 section);

        /// <summary>
        /// Retrieve Section wrapped in privacy wrapper
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<Dtos.Student.Section>> GetSectionAsync(string sectionId, bool useCache);

        /// <summary>
        /// Retrieve Section wrapped in privacy wrapper
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<Dtos.Student.Section2>> GetSection2Async(string sectionId, bool useCache);

        /// <summary>
        /// Retrieve Section wrapped in privacy wrapper
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<Dtos.Student.Section3>> GetSection3Async(string sectionId, bool useCache);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<List<Dtos.Student.Section>>> GetSectionsAsync(IEnumerable<string> sectionIds, bool useCache);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<List<Dtos.Student.Section2>>> GetSections2Async(IEnumerable<string> sectionIds, bool useCache);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        Task<PrivacyWrapper<List<Dtos.Student.Section3>>> GetSections3Async(IEnumerable<string> sectionIds, bool useCache = true, bool bestFit = false);
        /// <summary>
        /// Retrieve sections events iCal from calendar schedules
        /// </summary>
        /// <param name="sectionIds"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Base.EventsICal> GetSectionEventsICalAsync(IEnumerable<string> sectionIds, DateTime? startDate, DateTime? endDate);

        Task<Dtos.Student.SectionMidtermGradingComplete> GetSectionMidtermGradingCompleteAsync(string sectionId);

        Task<Dtos.Student.SectionMidtermGradingComplete> PostSectionMidtermGradingCompleteAsync(string sectionId, Dtos.Student.SectionMidtermGradingCompleteForPost sectionGradingComplete);
    }
}