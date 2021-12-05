// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a section, a specific offering of a course
    /// </summary>
    public class Section4
    {
        /// <summary>
        /// Unique section Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Parent course of this section
        /// </summary>
        public string CourseId { get; set; }
        /// <summary>
        /// Code of term in which section offered (may be blank for non-term sections)
        /// </summary>
        public string TermId { get; set; }
        /// <summary>
        /// Section start date
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Section end date
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Distinct number of this section (used to distinguish between multiple sections of the same course, e.g. 01 or 02)
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Subject and Course number put together separated by a dash '-'.
        /// </summary>
        public string CourseName { get; set; }
        /// <summary>
        /// Minimum credits allowed for this section
        /// </summary>
        public decimal? MinimumCredits { get; set; }
        /// <summary>
        /// Maximum credits allowed for this section, used for variable credit courses
        /// </summary>
        public decimal? MaximumCredits { get; set; }
        /// <summary>
        /// Allowable credit increments, used for variable credit courses
        /// </summary>
        public decimal? VariableCreditIncrement { get; set; }
        /// <summary>
        /// Number of CEUs for this section, only if this is a continuing education course
        /// </summary>
        public decimal? Ceus { get; set; }
        /// <summary>
        /// Brief section title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Institutionally-defined location of this section (often campus)
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// <see cref="SectionMeeting">Meeting</see> information for this section
        /// </summary>
        public IEnumerable<SectionMeeting2> Meetings { get; set; }

        /// <summary>
        /// List of meetings for primary section
        /// This is only populated when there is a flag that allows to override cross-listed section meetings with primary section meetings
        /// when cross-listed section does not have its own meetings defined.
        /// </summary>
        public IEnumerable<SectionMeeting2> PrimarySectionMeetings { get; set; }
        /// <summary>
        /// Id of all faculty for this section
        /// </summary>
        public IEnumerable<string> FacultyIds { get; set; }
        /// <summary>
        /// <see cref="SectionBook">Books</see> used in this section
        /// </summary>
        public IEnumerable<SectionBook> Books { get; set; }
        /// <summary>
        /// List of Ids of students registered in this section
        /// </summary>
        public IEnumerable<string> ActiveStudentIds { get; set; }
        /// <summary>
        /// Learning Provider
        /// </summary>
        public string LearningProvider { get; set; }
        /// <summary>
        /// Learning Provider Site
        /// </summary>
        public string LearningProviderSiteId { get; set; }
        /// <summary>
        /// In the case of cross-listed sections, the ID of the primary cross-listed section. 
        /// </summary>
        public string PrimarySectionId { get; set; }
        /// <summary>
        /// Indicates whether this section can be registered as Pass/NoPass
        /// </summary>
        public bool AllowPassNoPass { get; set; }
        /// <summary>
        /// Indicates whether this section can be registered as Audit
        /// </summary>
        public bool AllowAudit { get; set; }
        /// <summary>
        /// Indicates whether this section can ONLY be registered as Pass/NoPass
        /// </summary>
        public bool OnlyPassNoPass { get; set; }

        /// <summary>
        /// Capacity is "global" if section is cross-listed, otherwise it is the section's capacity
        /// If null the section has unlimited capacity.
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// Available is the number of students that can still register for this section.
        /// It will be which ever is smaller (global availability or section availability)
        /// If Available is 0 then this section is considered closed to new registrations.
        /// If Available is null then this section is unlimited.
        /// </summary>
        public int? Available { get; set; }

        /// <summary>
        /// If a student is permitted to request being added to the waitlist, this is true.
        /// If the section is not active, if the section doesn't allow placement on a waiting list, or if waitlist is closed, this is false.
        /// </summary>
        public bool WaitlistAvailable { get; set; }

        /// <summary>
        /// Indicates whether the section is active or not. If canceled this is false.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of students currently active on the waitlist for this section. If the section is 
        /// cross listed, and waitlists are "combined" for all sections, then it will show the number on the waitlist for all sections.
        /// </summary>
        public int Waitlisted { get; set; }

        /// <summary>
        /// Maximum number of students allowed to be waitlisted for this section.
        /// </summary>
        public int WaitlistMaximum { get; set; }

        /// <summary>
        /// Check if waitlist is allowed for a particular section
        /// </summary>
        public bool AllowWaitlist { get; set; }

        /// <summary>
        /// If True, the course requisites are not relevant to this section.
        /// </summary>
        public bool OverridesCourseRequisites { get; set; }

        /// <summary>
        /// <see cref="Requisite">Requisites</see> for this section. Replaces Prerequisites and Corequisites.
        /// </summary>
        public IEnumerable<Requisite> Requisites { get; set; }

        /// <summary>
        /// <see cref="SectionRequisite">SectionRequisites</see> for this section. 
        /// </summary>
        public IEnumerable<SectionRequisite> SectionRequisites { get; set; }

        /// <summary>
        /// The Academic Level Code of the section.
        /// </summary>
        public string AcademicLevelCode { get; set; }

        /// <summary>
        /// A section specific link to an external bookstore that will show the books required for this section. 
        /// This is only populated if the client defines an external bookstore template from which this link is to be calculated.
        /// </summary>
        public string BookstoreUrl { get; set; }

        /// <summary>
        /// Free form comments about this section
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Code indicating how/if this section will transfer to other institutions
        /// </summary>
        public string TransferStatus { get; set; }
        /// <summary>
        /// Code inicating the topic of the section
        /// </summary>
        public string TopicCode { get; set; }

        /// <summary>
        /// Grade scheme
        /// </summary>
        public string GradeSchemeCode { get; set; }

        /// <summary>
        /// Grade subscheme
        /// </summary>
        public string GradeSubschemeCode { get; set; }

        /// <summary>
        /// Miscellaneous (non-registration) financial charges
        /// </summary>
        public IEnumerable<SectionCharge> SectionCharges { get; set; }

        /// <summary>
        /// Indicates if Requisite Waiver is hidden for this section
        /// </summary>
        public bool HideRequisiteWaiver { get; set; }

        /// <summary>
        /// Indicates if Student Petition is hidden for this section
        /// </summary>
        public bool HideStudentPetition { get; set; }

        /// <summary>
        /// Indicates if Faculty Consent is hidden for this section
        /// </summary>
        public bool HideFacultyConsent { get; set; }

        /// <summary>
        /// Indicates if this section is excluded from Add Authorization requirements
        /// </summary>
        public bool ExcludeFromAddAuthorization { get; set; }

        /// <summary>
        /// Attendance tracking type for the course section
        /// </summary>
        public AttendanceTrackingType2 AttendanceTrackingType { get; set; }

        /// <summary>
        /// Synonym
        /// </summary>
        public string Synonym { get; set; }

        /// <summary>
        /// Indicates whether this section should display a client-specified icon (when provided) in course catalog and section search
        /// </summary>
        public bool ShowSpecialIcon { get; set; }

        /// <summary>
        /// This indicates status of the section 
        /// It is Open when there are seats available.
        /// It is Waitlisted when there are students in waitlist. 
        /// It is Closed when there are no seats available.
        /// </summary>
        public SectionAvailabilityStatusType AvailabilityStatus { get; set; }
        /// <summary>
        /// Indicates when the section's Census was certified
        /// </summary>
        public IEnumerable<SectionCensusCertification> SectionCertifiedCensuses { get; set; }

        /// <summary>
        /// Indicates whether the drop roster should be displayed for the section
        /// </summary>
        public bool ShowDropRoster { get; set; }
        /// <summary>
        /// Course Types for the section
        /// </summary>
        public List<SectionCourseType> SectionCourseTypes { get; set; }
        /// <summary>
        /// Instructional Methods for the section
        /// </summary>
        public List<SectionInstructionalMethod> SectionInstructionalMethods { get; set; }

        /// <summary>
        /// List of section faculty ids with their instructional methods
        /// </summary>
        public List<SectionFaculty> Faculty { get; set; }

        /// <summary>
        /// Indicates whether grading for this section will be done by name or anonymously using a random grading id
        /// </summary>
        public bool GradeByRandomId { get; set; }

    }
}
