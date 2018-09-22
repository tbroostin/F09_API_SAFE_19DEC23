using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a section, a specific offering of a course
    /// </summary>
    public class Section
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
        public IEnumerable<SectionMeeting> Meetings { get; set; }
        /// <summary>
        /// Id of all faculty for this section
        /// </summary>
        public IEnumerable<string> FacultyIds { get; set; }
        /// <summary>
        /// <see cref="Corequisite">Co-requisites</see> of this section's course
        /// </summary>
        public IEnumerable<Corequisite> CourseCorequisites { get; set; }
        /// <summary>
        /// <see cref="Corequisite">Co-requisites</see> of this section
        /// </summary>
        public IEnumerable<Corequisite> SectionCorequisites { get; set; }
        /// <summary>
        /// <see cref="SectionBook">Books</see> used in this section
        /// </summary>
        public IEnumerable<SectionBook> Books { get; set;}
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

    }
}
