﻿// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section information
    /// </summary>
    [Serializable]
    public class Section
    {
        #region Constructor

        /// <summary>
        /// Section Constructor. Section inherits course name (Course.Subject.Code + Course.Number)
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <param name="courseId">Course ID</param>
        /// <param name="number">Section number</param>
        /// <param name="startDate">Start date</param>
        /// <param name="minCredits">Either minCredits or CEUs required. May be 0.</param>
        /// <param name="ceus">Either minCredits or CEUs required. May be 0</param>
        /// <param name="title">Title</param>
        /// <param name="creditTypeCode">Credit type code</param>
        /// <param name="departments">Departments collection</param>
        /// <param name="courseLevelCodes">Collection of course levels</param>
        /// <param name="academicLevelCode">Academic level code</param>
        /// <param name="statuses">Collection of statuses</param>
        /// <param name="allowPassNoPass">Allow Pass/Fail grading</param>
        /// <param name="allowAudit">Allow students to audit this section</param>
        /// <param name="onlyPassNoPass">Students may only take this section as Pass/Fail</param>
        /// <param name="allowWaitlist">Students may put themselves on a wait list</param>
        /// <param name="waitlistClosed">The wait list for this section is closed</param>
        /// <param name="isInstructorConsentRequired">Students must get instructor consent before taking this section</param>
        public Section(string id, string courseId, string number, DateTime startDate, decimal? minCredits, decimal? ceus, string title, string creditTypeCode,
            ICollection<OfferingDepartment> departments, ICollection<string> courseLevelCodes, string academicLevelCode, IEnumerable<SectionStatusItem> statuses,
            bool allowPassNoPass = true, bool allowAudit = true, bool onlyPassNoPass = false, bool allowWaitlist = false, bool waitlistClosed = false,
            bool isInstructorConsentRequired = false, bool hideInCatalog = false)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId", "Course must have a value");
            }
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Section Number is required");
            }
            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start Date is required");
            }
            if (minCredits < 0)
            {
                throw new ArgumentOutOfRangeException("minCredits", "Min Credits may not be negative");
            }
            if (ceus < 0)
            {
                throw new ArgumentOutOfRangeException("ceus", "CEUs may not be negative");
            }
            if ((minCredits == null) && (ceus == null))
            {
                throw new ArgumentException("Either Minimum Credits or CEUs must have a value");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "Title is required");
            }
            if (string.IsNullOrEmpty(creditTypeCode))
            {
                throw new ArgumentNullException("creditTypeCode", "Credit Type is required.");
            }
            if (departments == null)
            {
                throw new ArgumentNullException("departments", "Departments may not be null");
            }
            if (departments.Count < 1)
            {
                throw new ArgumentException("At least one Department is required");
            }
            if (courseLevelCodes == null)
            {
                throw new ArgumentNullException("courseLevelCodes", "Course Levels may not be null");
            }
            if (courseLevelCodes.Count < 1)
            {
                throw new ArgumentException("At least one Course Level code is required");
            }
            if (string.IsNullOrEmpty(academicLevelCode))
            {
                throw new ArgumentNullException("academicLevelCode", "Academic Level is required");
            }
           

            // OnlyPassNoPass ALWAYS takes precedence over allowPassNoPass and allowAudit. 
            // If OnlyPassNoPass is true then allowPassNoPass MUST be true and allowAudit MUST be false.
            // For consistency will ensure this in the section. 
            if (onlyPassNoPass)
            {
                allowPassNoPass = true;
                allowAudit = false;
            }
            _Id = id;
            _CourseId = courseId;
            _Number = number;
            _StartDate = startDate;
            _MinimumCredits = minCredits;
            _Ceus = ceus;
            _Title = title;
            _CreditTypeCode = creditTypeCode;
            _Departments.AddRange(departments);
            _CourseLevelCodes = courseLevelCodes.ToList();
            _AcademicLevelCode = academicLevelCode;
            _Statuses = statuses.ToList();
            _AllowPassNoPass = allowPassNoPass;
            _AllowAudit = allowAudit;
            _OnlyPassNoPass = onlyPassNoPass;
            _AllowWaitlist = allowWaitlist;
            _WaitlistClosed = waitlistClosed;
            _IsInstructorConsentRequired = isInstructorConsentRequired;
            _HideInCatalog = hideInCatalog;

            Departments = _Departments.AsReadOnly();
            CourseLevelCodes = _CourseLevelCodes.AsReadOnly();
            Statuses = _Statuses.AsReadOnly();
            Meetings = _SectionMeetings.AsReadOnly();
            PrimarySectionMeetings = _PrimarySectionMeetings.AsReadOnly();
            Faculty = _SectionFaculty.AsReadOnly();
            FacultyIds = _FacultyIds.AsReadOnly();
            Books = _Books.AsReadOnly();
            ActiveStudentIds = _ActiveStudentIds.AsReadOnly();
            CrossListedSections = _CrossListedSections.AsReadOnly();
            CourseTypeCodes = _CourseTypeCodes.AsReadOnly();
            InstructionalContacts = _instructionalContacts.AsReadOnly();
            SectionCharges = _sectionCharges.AsReadOnly();
            ExcludeFromAddAuthorization = false;
            ShowSpecialIcon = false;
        }

        #endregion

        #region Required fields

        private string _Id;
        /// <summary>
        /// Section ID
        /// </summary>
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Section Id cannot be changed");
                }
            }
        }

        private readonly string _CourseId;
        /// <summary>
        /// ID of the course of which this section is an offering
        /// </summary>
        public string CourseId { get { return _CourseId; } }

        private readonly string _Number;
        // Number is added to course subject and number to complete the section name
        /// <summary>
        /// Section number
        /// </summary>
        public string Number { get { return _Number; } }

        /// <summary>
        /// Name of the course
        /// </summary>
        public string CourseName { get; set; }

        private readonly DateTime _StartDate;
        /// <summary>
        /// Date on which this section begins meeting
        /// </summary>        
        public DateTime StartDate { get { return _StartDate; } }

        private decimal? _MinimumCredits;
        // MinimumCredits cannot be an override because it may be null
        /// <summary>
        /// Minimum credits for which this section can be taken
        /// </summary>
        public decimal? MinimumCredits { get { return _MinimumCredits; } }

        private decimal? _Ceus;
        // CEU cannot be an override because it may be null
        /// <summary>
        /// Number of CEUs awarded upon completion of this section
        /// </summary>
        public decimal? Ceus { get { return _Ceus; } }

        private readonly string _Title;
        /// <summary>
        /// Section title
        /// </summary>
        public string Title { get { return _Title; } }

        private readonly List<OfferingDepartment> _Departments = new List<OfferingDepartment>();
        /// <summary>
        /// List of departments offering this section
        /// </summary>
        public ReadOnlyCollection<OfferingDepartment> Departments { get; private set; }

        private readonly List<string> _CourseLevelCodes = new List<string>();
        /// <summary>
        /// List of course levels applicable to this section
        /// </summary>
        public ReadOnlyCollection<string> CourseLevelCodes { get; private set; }

        private readonly string _AcademicLevelCode;
        /// <summary>
        /// Academic level of this section
        /// </summary>
        public string AcademicLevelCode { get { return _AcademicLevelCode; } }

        private readonly bool _AllowPassNoPass;
        /// <summary>
        /// Indicates whether students are allowed to take this section on a pass/no-pass basis
        /// </summary>
        public bool AllowPassNoPass { get { return _AllowPassNoPass; } }

        private readonly bool _AllowAudit;
        /// <summary>
        /// Indicates whether this section can be audited
        /// </summary>
        public bool AllowAudit { get { return _AllowAudit; } }

        private readonly bool _OnlyPassNoPass;
        /// <summary>
        /// Indicates whether students can only take this for a pass or no-pass grade
        /// </summary>
        public bool OnlyPassNoPass { get { return _OnlyPassNoPass; } }

        private readonly bool _AllowWaitlist;
        /// <summary>
        /// Indicates whether students are allowed to go on the waitlist for this section
        /// </summary>
        public bool AllowWaitlist { get { return _AllowWaitlist; } }

        private readonly bool _IsInstructorConsentRequired;
        /// <summary>
        /// Indicates whether the student must get consent from the instructor to take this section
        /// </summary>
        public bool IsInstructorConsentRequired { get { return _IsInstructorConsentRequired; } }

        private readonly bool _WaitlistClosed;
        /// <summary>
        /// Indicates whether the waitlist is closed for this section
        /// </summary>
        public bool WaitlistClosed { get { return _WaitlistClosed; } }

        private readonly string _CreditTypeCode;
        /// <summary>
        /// Credit type
        /// </summary>
        public string CreditTypeCode { get { return _CreditTypeCode; } }

        private readonly List<SectionStatusItem> _Statuses;
        /// <summary>
        /// Section status information
        /// </summary>
        public ReadOnlyCollection<SectionStatusItem> Statuses { get; private set; }

        private readonly bool _HideInCatalog;
        /// <summary>
        /// Indicates whether the section should show in the course catalog 
        /// </summary>
        public bool HideInCatalog { get { return _HideInCatalog; } }

        #endregion

        #region Optional Fields

        private string _Guid;
        /// <summary>
        /// GUID for the section; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// Grade scheme
        /// </summary>
        public string GradeSchemeCode { get; set; }

        /// <summary>
        /// Grade subscheme
        /// </summary>
        public string GradeSubschemeCode { get; set; }


        ///<summary>
        /// Course categories
        /// </summary>
        private List<CourseCategory> _CourseCategories = new List<CourseCategory>();
        public List<CourseCategory> CourseCategories { get { return _CourseCategories; } }


        ///<summary>
        /// Instructional Methods
        /// </summary>
        private List<string> _InstructionalMethods = new List<string>();
        public List<string> InstructionalMethods { get { return _InstructionalMethods; } }

        /// <summary>
        /// Term in which this section is being offered
        /// </summary>
        public string TermId { get; set; }

        private decimal? _MaximumCredits;
        private List<InstructionalContact> _instructionalContacts = new List<InstructionalContact>();
        /// <summary>
        /// The types of instructional contacts for the section
        /// </summary>
        public ReadOnlyCollection<InstructionalContact> InstructionalContacts { get; private set; }

        /// <summary>
        /// The maximum number of credits for which this section may be taken
        /// </summary>
        public decimal? MaximumCredits
        {
            get { return _MaximumCredits; }
            set
            {
                if (value != null)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Maximum Credits may not be negative");
                    }
                    if (MinimumCredits > value)
                    {
                        throw new ArgumentOutOfRangeException("value", "Max Credits cannot be less than Min Credits");
                    }
                    _MaximumCredits = value;
                }
            }
        }

        private decimal? _VariableCreditIncrement;
        /// <summary>
        /// For variable credit section, this is the credit increment that can be used
        /// </summary>
        public decimal? VariableCreditIncrement
        {
            get { return _VariableCreditIncrement; }
            set
            {
                if (value != null)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Variable Credit Increment may not be negative");
                    }
                    if (value > MaximumCredits)
                    {
                        throw new ArgumentOutOfRangeException("value", "Variable Credit Increment may not be larger than maximum credits");
                    }
                    _VariableCreditIncrement = value;
                }
            }
        }

        /// <summary>
        /// Location at which this section is offered
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Optional ending date of the section
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// First meeting date derived from generated/edited CALENDAR.SCHEDLUES
        /// </summary>
        public DateTime? FirstMeetingDate { get; set; }
        
        /// <summary>
        /// Last meeting date derived from generated/edited CALENDAR.SCHEDLUES
        /// </summary>
        public DateTime? LastMeetingDate { get; set; }

        /// <summary>
        /// Identifies the waitlist rules used for this section
        /// </summary>
        public string WaitlistRatingCode { get; set; }

        /// <summary>
        /// The section's computed name
        /// </summary>
        public string Name { get; set; }

        private readonly List<string> _FacultyIds = new List<string>();
        /// <summary>
        /// List of faculty assigned to this section
        /// </summary>
        public ReadOnlyCollection<string> FacultyIds { get; private set; }

            
        private readonly List<SectionMeeting> _SectionMeetings = new List<SectionMeeting>();

        private readonly List<SectionMeeting> _PrimarySectionMeetings = new List<SectionMeeting>();
        /// <summary>
        /// List of the meetings for this section
        /// </summary>

        public ReadOnlyCollection<SectionMeeting> Meetings { get; private set; }
        /// <summary>
        /// List of meetings for primary section.
        /// This is only populated when there is a flag that allows to override cross-listed section meetings with primary section meetings
        /// when cross-listed section does not have its own meetings defined.
        /// For all other conditions, this field will be empty list.
        /// </summary>
        public ReadOnlyCollection<SectionMeeting> PrimarySectionMeetings { get; private set; }
  

        private readonly List<SectionFaculty> _SectionFaculty = new List<SectionFaculty>();
        /// <summary>
        /// List of the faculty details for this section
        /// </summary>
        public ReadOnlyCollection<SectionFaculty> Faculty { get; private set; }

        private List<Requisite> _Requisites = new List<Requisite>();
        /// <summary>
        /// Represents Course requisites that the section is overriding
        /// </summary>
        public List<Requisite> Requisites
        {
            get { return _Requisites; }
            set { if (value != null) { _Requisites = value; } }
        }

        private List<SectionRequisite> _SectionRequisites = new List<SectionRequisite>();
        /// <summary>
        /// Section Requisites replace Course Co-requisites and Section Co-requisites
        /// </summary>
        public List<SectionRequisite> SectionRequisites
        {
            get { return _SectionRequisites; }
            set { if (value != null) { _SectionRequisites = value; } }
        }

        /// <summary>
        // If true, this section overrides course requisites and the course
        // requisites should be ignored. 
        /// </summary>
        public bool OverridesCourseRequisites { get; set; }

        private List<SectionBook> _Books = new List<SectionBook>();
        /// <summary>
        /// Books related to this section.
        /// </summary>
        public ReadOnlyCollection<SectionBook> Books { get; private set; }

        private List<string> _CourseTypeCodes = new List<string>();
        /// <summary>
        /// Course types for this section
        /// </summary>
        public ReadOnlyCollection<string> CourseTypeCodes { get; private set; }

        private string _TopicCode;
        /// <summary>
        /// Topic code for this section
        /// </summary>
        public string TopicCode { get { return _TopicCode; } set { _TopicCode = value; } }

        /// <summary>
        /// The duration of the section in weeks
        /// </summary>
        public int? NumberOfWeeks { get; set; }

        /// <summary>
        /// Indicates whether section is held fully online, partially online, or is not online at all.
        /// </summary>
        public OnlineCategory OnlineCategory 
        { 
            get 
            {
                if (Meetings.Count() == 0)
                {
                    return OnlineCategory.NotOnline;
                }
                if (Meetings.Any(m => m.IsOnline) && Meetings.Any(mt => !mt.IsOnline))
                {
                    return OnlineCategory.Hybrid;
                }
                if (Meetings.Any(m => m.IsOnline))
                {
                    return OnlineCategory.Online;
                }
                return OnlineCategory.NotOnline;
            } 
        }

        private List<string> _ActiveStudentIds = new List<string>();
        /// <summary>
        /// Active Students in section, not dropped or deleted
        /// </summary>
        public ReadOnlyCollection<string> ActiveStudentIds { get; private set; }

        public string LearningProvider { get; set; }
        public string LearningProviderSiteId { get; set; }
        public string PrimarySectionId { get; set; }
        public string BillingMethod { get; set; }

        private int? _NumberOnWaitlist;
        /// <summary>
        /// Number of students actively on the waitlist for this specific section (includes active and permitted to register).
        /// </summary>
        public int? NumberOnWaitlist
        {
            get { return _NumberOnWaitlist; }
            set
            {
                if (value != null)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Number On Waitlist may not be negative");
                    }
                    _NumberOnWaitlist = value;
                }
            }
        }

        private int? _PermittedToRegisterOnWaitlist;
        /// <summary>
        /// Number of students on the waitlist who've been given permission to register. (Used to reduce availability.)
        /// Subset of ActiveOnWaitlist above!
        /// </summary>
        public int? PermittedToRegisterOnWaitlist 
        { 
            get { return _PermittedToRegisterOnWaitlist; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Number of students permitted to register cannot be negative.");
                }
                _PermittedToRegisterOnWaitlist = value;
            }
        }

        private int? _ReservedSeats;
        /// <summary>
        /// Number of seats being held in reserve by web processes for a particular section.
        /// </summary>
        public int? ReservedSeats
        {
            get { return _ReservedSeats; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Reserved Seats cannot be negative.");
                }
                _ReservedSeats = value;
            }
        }

        private int? _SectionCapacity;
        /// <summary>
        /// Capacity set for this specific section. If null it means there is no limit.
        /// </summary>
        public int? SectionCapacity 
        {
            get { return _SectionCapacity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Section capacity may not be negative");
                }
                _SectionCapacity = value;
            }
        }

        private int? _WaitlistMaximum;
        /// <summary>
        /// Maximum number of students that can be placed on this section's waitlist.
        /// </summary>
        public int? WaitlistMaximum
        {
            get { return _WaitlistMaximum; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Waitlist Maximum cannot be negative.");
                }
                _WaitlistMaximum = value;
            }
        }

        private int? _GlobalCapacity;
        /// <summary>
        /// Capacity set for all cross listed sections. Null if section is NOT cross listed.
        /// NOTE: If the section IS cross listed both the global and the local capacities have to be null for it to be truly unlimited.
        /// </summary>
        public int? GlobalCapacity 
        { 
            get { return _GlobalCapacity; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Global capacity may not be negative");
                }
                _GlobalCapacity = value;
            }
        }

        private int? _GlobalWaitlistMaximum;
        /// <summary>
        /// Maximum to be place on the waitlist for any cross listed sections. Null if the section is NOT cross listed.
        /// </summary>
        public int? GlobalWaitlistMaximum
        {
            get { return _GlobalWaitlistMaximum; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Global Waitlist Maximum cannot be negative.");
                }
                _GlobalWaitlistMaximum = value;
            }
        }

        /// <summary>
        /// Indicates whether cross-listed sections are to operate with effectively 1 waitlist.  If true, then if a waitlist is started for any section 
        /// then the waitlist starts for ALL sections.  If this is false, sections with availability will show their availability even
        /// if other cross-listed sections are wait-listed. 
        /// section.
        /// </summary>
        public bool CombineCrosslistWaitlists { get; set; }

        private List<Section> _CrossListedSections = new List<Section>();
        /// <summary>
        /// Sections that are cross listed with this section
        /// </summary>
        public ReadOnlyCollection<Section> CrossListedSections { get; private set; }

        /// <summary>
        /// Public property to get/set the transfer status for a section
        /// Transfer status indicates whether or not this section will transfer to other colleges
        /// </summary>
        public string TransferStatus { get; set; }

        /// <summary>
        /// Public property to get/set comments related to this section.
        /// Comments are free-form text that may be used to keep any miscellaneous data about this section
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Contains a RegistrationDates object with any applicable registration date overrides for this section.
        /// </summary>
        public RegistrationDate RegistrationDateOverrides { get; set; }

        /// <summary>
        /// Public property to get/set census dates
        /// Census dates set when section enrollment counts are calculated
        /// </summary>
        public List<DateTime?> CensusDates { get; set; }

        /// <summary>
        /// Public property to get/set Billing Credit
        /// </summary>
        public Decimal? BillingCred { get; set; }

        private int? _WaitlistNumberOfDays;
        /// <summary>
        /// Maximum number of students that can be placed on this section's waitlist.
        /// </summary>
        public int? WaitListNumberOfDays
        {
            get { return _WaitlistNumberOfDays; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Waitlist number of days cannot be negative.");
                }
                _WaitlistNumberOfDays = value;
            }
        }
        /// <summary>
        /// Status of waitlist (Clsd, Cncl, Froz, Wlst, Open, Wcls, Wful)
        /// </summary>
        public string WaitlistStatus;

        private List<SectionCharge> _sectionCharges = new List<SectionCharge>();
        /// <summary>
        /// Miscellaneous (non-registration) financial charges
        /// </summary>
        public ReadOnlyCollection<SectionCharge> SectionCharges { get; private set; }

        /// <summary>
        /// Indicates if this section is excluded from Add Authorization requirements
        /// </summary>
        public bool ExcludeFromAddAuthorization { get; set; }

        /// <summary>
        /// Attendance tracking type for the course section
        /// </summary>
        public AttendanceTrackingType AttendanceTrackingType { get; set; }

        /// <summary>
        /// Synonym for the section
        /// </summary>
        public string Synonym { get; set; }

        /// <summary>
        /// Indicates whether this section should display a client-specified icon (when provided) in course catalog and section search
        /// </summary>
        public bool ShowSpecialIcon { get; set; }

        #endregion

        #region Calculated properties

        /// <summary>
        /// CALCULATED: If global capacity is not null it will return global capacity, otherwise it returns the section capacity
        /// </summary>
        public int? Capacity
        {
            get
            {
                return _GlobalCapacity != null ? _GlobalCapacity : _SectionCapacity;
            }
        }

        /// <summary>
        /// CALCULATED: Number of available slots still available for registration. If there is no capacity this is null. For cross-listed
        /// sections, this will which ever is less - global available or section available.  NOTE: If CombineCrosslistWaitlist is true, and a waitlist
        /// exists for any of the cross-listed sections, then available is zero.
        /// </summary>
        public int? Available
        {
            get
            {
                // NOTES:
                // Uses Global Available if it is not null AND it is less then the Section Available.
                // If there IS a global capacity but no section capacity (it's null - meaning unlimited), then it return null, same as Colleague.
                // If the CombineCrosslistWaitlists is true and anyone is waitlisted for the section or any of its cross-listed sections, then availability is zero. End of story.
                if (CombineCrosslistWaitlists == true )
                {
                    int TotalOnWaitlists = CrossListedSections.Sum(sc => sc.NumberOnWaitlist.GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                    if(TotalOnWaitlists > 0)
                    {
                        return 0;
                    }
                }
                var numberEnrolledLocal = ActiveStudentIds.Count() + _ReservedSeats.GetValueOrDefault(0) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                int crossListedActiveStudentCount = CrossListedSections.Sum(cl => cl.ActiveStudentIds.Count());
                int crossListedReservedCount = CrossListedSections.Sum(cls => cls._ReservedSeats.GetValueOrDefault(0));
                int crossListedEnrolled = crossListedActiveStudentCount + crossListedReservedCount;
                int crossListedPermissionToRegister = (CrossListedSections.Sum(c => c._PermittedToRegisterOnWaitlist.GetValueOrDefault(0))) + _PermittedToRegisterOnWaitlist.GetValueOrDefault(0);
                var numberEnrolledGlobal = numberEnrolledLocal + crossListedEnrolled;
                int? numberAvailableLocal = null;
                if (_SectionCapacity != null)
                {
                    if (_SectionCapacity - numberEnrolledLocal <= 0)
                    {
                        numberAvailableLocal = 0;
                    }
                    else
                    {
                        numberAvailableLocal = _SectionCapacity - numberEnrolledLocal;
                    }
                }
                int? numberAvailableGlobal = null;
                if (_GlobalCapacity != null)
                {
                    if (_GlobalCapacity - numberEnrolledGlobal <= 0)
                    {
                        numberAvailableGlobal = 0;
                    }
                    else
                    {
                        numberAvailableGlobal = _GlobalCapacity - numberEnrolledGlobal - crossListedPermissionToRegister;
                    }
                }
                if (numberAvailableGlobal != null && (numberAvailableGlobal <= numberAvailableLocal))
                {
                    return numberAvailableGlobal;
                }
                return numberAvailableLocal;
            }
        }

        /// <summary>
        /// CALCULATED: Indicates when the ability to add oneself to a waitlist for this section is currently available.
        /// This is only true if the section is active, it allows a waitlist, the waitlist isn't closed and if waitlist maximums have not been reached
        /// </summary>
        public bool WaitlistAvailable
        {
            get
            {
                if (IsActive == false || AllowWaitlist == false || WaitlistClosed == true)
                {
                    return false;
                }
                if (_WaitlistMaximum == null && _GlobalWaitlistMaximum == null)
                {
                    // A waitlist is available and no waitlist maximums have been set.
                    return true;
                }
                if (_GlobalWaitlistMaximum == null)
                {
                    // No global maximum so only the section waitlist max matters.
                    if (_WaitlistMaximum > 0 && NumberOnWaitlist.GetValueOrDefault(0) < _WaitlistMaximum)
                    {
                        return true;
                    }
                    return false;
                }
                int GlobalActiveOnWaitlist = (CrossListedSections.Sum(c => c.NumberOnWaitlist).GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                if (_WaitlistMaximum == null)
                {
                    // No maximum on the section's local waitlist - just need to see if the global max has been reached yet.
                    if (GlobalActiveOnWaitlist < _GlobalWaitlistMaximum)
                    {
                       return true; 
                    }
                    return false;
                }
                // If we get her this means there is both a section waitlist max AND a global waitlist max.  Both must be unmet.
                if (GlobalActiveOnWaitlist < _GlobalWaitlistMaximum && NumberOnWaitlist.GetValueOrDefault(0) < _WaitlistMaximum)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// CALCULATED: This is the number of active people on the waitlist for the section.
        /// If the section is cross listed, and if "CombinedCrosslistWaitlist" is true, then the number waitlisted will be
        /// the number on any of the cross listed section's waitlists.
        /// </summary>
        public int Waitlisted
        {
            get
            {
                int TotalOnWaitlists = CrossListedSections.Sum(sc => sc.NumberOnWaitlist.GetValueOrDefault(0)) + NumberOnWaitlist.GetValueOrDefault(0);
                if (CombineCrosslistWaitlists == true && TotalOnWaitlists > 0)
                {
                    return TotalOnWaitlists;
                }
                return NumberOnWaitlist.GetValueOrDefault(0);
            }
        }

        /// <summary>
        /// If the client has defined a bookstore template this is a section specific URL for this section
        /// </summary>
        public string BookstoreURL { get; set; }

        /// <summary>
        /// Current status for this section
        /// </summary>
        public SectionStatus CurrentStatus { get { return Statuses.Any() ? Statuses[0].Status : SectionStatus.NotSet; } }

        /// <summary>
        /// Indicates whether this section is active
        /// </summary>
        public bool IsActive { get { return CurrentStatus == SectionStatus.Active; } }

        public bool? IsCrossListedSection { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Add an active student to this section
        /// </summary>
        /// <param name="activeStudentId">Student ID to add</param>
        public void AddActiveStudent(string activeStudentId)
        {
            if (string.IsNullOrEmpty(activeStudentId))
            {
                throw new ArgumentNullException("activeStudentId", "Student Id must be specified");
            }
            if (ActiveStudentIds.Contains(activeStudentId))
            {
                throw new ArgumentException("ActiveStudent with this Id already exists in this list");
            }
            _ActiveStudentIds.Add(activeStudentId);
        }

        /// <summary>
        /// Add a book for this section
        /// </summary>
        /// <param name="bookId">ID of book</param>
        /// <param name="requirementStatusCode"> enumeration value</param>
        public void AddBook(string bookId, string requirementStatusCode, bool isRequired)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                throw new ArgumentNullException("bookId", "Book Id must be specified");
            }
            if (Books.Count(b => b.BookId == bookId) > 0)
            {
                throw new ArgumentException("Book with this Id already exists in this list");
            }
            _Books.Add(new SectionBook(bookId, requirementStatusCode, isRequired));
        }

        /// <summary>
        /// Add a course type for the section
        /// </summary>
        /// <param name="courseType">Course type code</param>
        public void AddCourseType(string courseType)
        {
            if (string.IsNullOrEmpty(courseType))
            {
                throw new ArgumentNullException("courseType", "Course type is required.");
            }
            if (!_CourseTypeCodes.Contains(courseType))
            {
                _CourseTypeCodes.Add(courseType);
            }
        }

        /// <summary>
        /// Add a cross-listed section to this section
        /// </summary>
        /// <param name="crossListSection">The section to cross-list with this one</param>
        public void AddCrossListedSection(Section crossListSection)
        {
            if (crossListSection == null)
            {
                throw new ArgumentNullException("crossListSection", "Cross List section must be provided");
            }
            if (crossListSection.Id == _Id || CrossListedSections.Where(x => x.Id == crossListSection.Id).Count() > 0)
            {
                throw new ArgumentException("Cross Listed Section is same as section or already exists in this list");
            }
            _CrossListedSections.Add(crossListSection);
        }

        
        /// <summary>
        /// Add a faculty member to a section
        /// </summary>
        /// <param name="facultyId"></param>
        public void AddFaculty(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty Id must be specified");
            }
            if (!FacultyIds.Contains(facultyId))
            {
                _FacultyIds.Add(facultyId);
            }
        }

        /// <summary>
        /// Add a meeting time to a section
        /// </summary>
        /// <param name="meetingTime">A meeting Time object</param>
        public void AddSectionMeeting(SectionMeeting sectionMeeting)
        {
            if (sectionMeeting == null)
            {
                throw new ArgumentNullException("sectionMeeting", "Section meeting cannot be null");
            }
            _SectionMeetings.Add(sectionMeeting);
        }


        

        /// <summary>
        /// This method will update meetings for primary section in PrimarySectionMeetings collection
        /// </summary>
        /// <param name="meetings"></param>
        public void UpdatePrimarySectionMeetings(IEnumerable<SectionMeeting> meetings)
        {
           if(meetings!=null)
            {
                
                    _PrimarySectionMeetings.Clear();
               
                foreach (var meeting in meetings)
                {
                    if (meeting != null)
                    {
                        _PrimarySectionMeetings.Add(meeting);
                    }
                }
            }
        }



        /// <summary>
        /// Remove a section meeting from this section
        /// </summary>
        /// <param name="sectionMeeting">The section meeting to remove</param>
        public void RemoveSectionMeeting(SectionMeeting sectionMeeting)
        {
            if (sectionMeeting == null)
            {
                throw new ArgumentNullException("sectionMeeting", "Section meeting cannot be null");
            }
            // Find the section meeting that is to be removed
            int idx = _SectionMeetings.FindIndex(x => x.Id == sectionMeeting.Id);
            if (idx < 0)
            {
                throw new InvalidOperationException("Section meeting " + sectionMeeting.Id + " is not part of section " + Id);
            }
            _SectionMeetings.RemoveAt(idx);
        }

        /// <summary>
        /// Add details about an instructor to a section
        /// </summary>
        /// <param name="sectionFaculty">A section Faculty object</param>
        public void AddSectionFaculty(SectionFaculty sectionFaculty)
        {
            if (sectionFaculty == null)
            {
                throw new ArgumentNullException("sectionFaculty");
            }
            _SectionFaculty.Add(sectionFaculty);
        }

        /// <summary>
        /// Remove section faculty from this section
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to remove</param>
        public void RemoveSectionFaculty(SectionFaculty sectionFaculty)
        {
            if (sectionFaculty == null)
            {
                throw new ArgumentNullException("sectionFaculty", "Section faculty cannot be null");
            }
            // Find the section faculty that is to be removed
            int idx = _SectionFaculty.FindIndex(x => x.Id == sectionFaculty.Id);
            if (idx < 0)
            {
                throw new InvalidOperationException("Section faculty " + sectionFaculty.Id + " is not part of section " + Id);
            }
            _SectionFaculty.RemoveAt(idx);
        }

        /// <summary>
        /// Remove section faculty from this section by its ID
        /// </summary>
        /// <param name="id">Section faculty ID</param>
        public void RemoveSectionFaculty(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            var faculty = _SectionFaculty.FirstOrDefault(f => f.Id == id);
            if (faculty == null)
            {
                throw new InvalidOperationException("ID " + id + " is not a valid section faculty ID for section " + Id);
            }
            RemoveSectionFaculty(faculty);
        }

        /// <summary>
        /// Add a status entry for this section
        /// </summary>
        /// <param name="status">New status to add</param>
        /// <param name="date">Date of status change; defaults to today</param>
        public void AddStatus(SectionStatus status, string code, DateTime? date = null)
        {
            // If no status date was provided, use today's date
            if (!date.HasValue)
            {
                date = DateTime.Today;
            }
            // New statuses go at the first
            _Statuses.Insert(0, new SectionStatusItem(status, code, date.Value));
        }

        

        /// <summary>
        /// Add an instructional contact for this section
        /// </summary>
        /// <param name="contact">Contact to add</param>
        public void AddInstructionalContact(InstructionalContact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");    
            }
            if (!_instructionalContacts.Contains(contact))
            {
                _instructionalContacts.Add(contact);
            }
        }
        /// <summary>
        /// Add a course category
        /// </summary>
        /// <param name="category">Category to add</param>
        public void AddCourseCategory(CourseCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            if (!_CourseCategories.Contains(category))
            {
                _CourseCategories.Add(category);
            }
        }
        /// <summary>
        /// Add an instructional method
        /// </summary>
        /// <param name="method">Category to add</param>
        public void AddInstructionalMethod(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (!_InstructionalMethods.Contains(method))
            {
                _InstructionalMethods.Add(method);
            }
        }

        /// <summary>
        /// Adds a financial charge for this section
        /// </summary>
        /// <param name="charge"><see cref="SectionCharge"> to add</param>
        public void AddSectionCharge(SectionCharge charge)
        {
            if (charge == null) throw new ArgumentNullException("charge", "Cannot add a null financial charge to a course section.");
            if (!_sectionCharges.Contains(charge))
            {
                _sectionCharges.Add(charge);
            }
        }

        #endregion

        #region Object override methods

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Section other = obj as Section;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

    }
}
