// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Course
    {
        #region Required Fields

        private string _Id;
        /// <summary>
        /// Unique ID of the course
        /// </summary>
        public string Id
        {
            get { return _Id; }
            set
            {
                if (_Id == "")
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Id cannot be changed");
                }
            }
        }

        private readonly List<OfferingDepartment> _Departments;
        /// <summary>
        /// Departments and their percentages of responsibility for the course
        /// </summary>
        public ReadOnlyCollection<OfferingDepartment> Departments { get; private set; }

        private readonly string _SubjectCode;
        /// <summary>
        /// Course subject code, representing the first half of the course name
        /// </summary>
        public string SubjectCode { get { return _SubjectCode; } }

        private readonly string _Number;
        /// <summary>
        /// Course number, representing the second half of the course name
        /// </summary>
        public string Number { get { return _Number; } }

        private readonly string _AcademicLevelCode;
        /// <summary>
        /// The academic level of this course. Typical values: UG (Undergraduate), GR (Graduate)
        /// </summary>
        public string AcademicLevelCode { get { return _AcademicLevelCode; } }

        private readonly List<string> _CourseLevelCodes;
        /// <summary>
        /// List of levels to which this course is applicable. Typical values: 100, 200, 300
        /// </summary>
        public List<string> CourseLevelCodes { get { return _CourseLevelCodes; } }

        // Either Min Credits OR CEUs required.
        private readonly decimal? _MinimumCredits;
        /// <summary>
        /// If applicable, number of credits this course is awarded. Is used in combination with MaximumCredits and VariableCreditUnits in the case
        /// of a variable credit course. Required unless Continuing Educational Units is provided.
        /// </summary>
        public decimal? MinimumCredits { get { return _MinimumCredits; } }

        private readonly decimal? _Ceus;
        /// <summary>
        /// If applicable, number of Continuing Educational Units this course satisfies. Required unless MinimumCredits is provided.
        /// </summary>
        public decimal? Ceus { get { return _Ceus; } }

        private readonly string _Title;
        /// <summary>
        /// Brief title of this course
        /// </summary>
        public string Title { get { return _Title; } }

        private readonly string _LongTitle;
        /// <summary>
        /// Longer title of this course
        /// </summary>
        public string LongTitle { get { return _LongTitle; } }

        private readonly CourseStatus _Status;
        /// <summary>
        /// Enumeration indicating the current status of this course.
        /// </summary>
        public CourseStatus Status { get { return _Status; } }

        /// <summary>
        /// The stored computed name for the course, based on a combination of course status and effective dates.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether this is a currently active course
        /// </summary>
        public bool IsCurrent
        {
            get
            {
                // A course is current if it has an active status AND it has not ended.
                if ((Status == CourseStatus.Active) &&
                   ((EndDate == null) || (DateTime.Today <= EndDate)))
                {
                    if (StartDate == null && EndDate == null)
                        return false;
                    else
                        return true;
                }       
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// GUID for the course; not required, but cannot be changed once assigned.
        /// </summary>
        private string _Guid;
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


        #endregion

        #region Not Required fields

        // Max Credits not required, invalid without Min credits
        private decimal? _MaximumCredits;
        /// <summary>
        /// When present, indicates this is a variable credit course. Maximum number of credits for which this course may be
        /// taken. Invalid without minimum credits.
        /// </summary>
        public decimal? MaximumCredits
        {
            get { return _MaximumCredits; }
            set
            {
                if (value.HasValue)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Max Credits cannot be negative");
                    }
                    if (value > 0)
                    {
                        if (!_MinimumCredits.HasValue)
                        {
                            throw new ArgumentOutOfRangeException("value", "Max Credits cannot be provided with no Min Credits");
                        }
                        if (_MinimumCredits.Value > value)
                        {
                            throw new ArgumentOutOfRangeException("value", "Max Credits cannot be less than Min Credits");
                        }
                    }
                }
                _MaximumCredits = value;
            }
        }

        private decimal? _VariableCreditIncrement;
        /// <summary>
        /// For a variable credit course (with Maximum Credits defined), indicates the allowable credit increment.
        /// </summary>
        public decimal? VariableCreditIncrement
        {
            get { return _VariableCreditIncrement; }
            set
            {
                if (value.HasValue)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("value", "Variable Credit Increment may not be negative");
                    }
                    if (!_MaximumCredits.HasValue || value.Value > _MaximumCredits.Value)
                    {
                        throw new ArgumentOutOfRangeException("value", "Variable Credit Increment may not be larger than maximum credits");
                    }
                    _VariableCreditIncrement = value;
                }
            }
        }

        /// <summary>
        /// A verbose description of the course content.
        /// </summary>
        public string Description { get; set; }

        private List<string> _LocationCodes;
        /// <summary>
        /// Possible locations this course may offered. Typically indicates a campus location.
        /// </summary>
        public List<string> LocationCodes
        {
            get { return _LocationCodes; }
            set { if (value != null) { _LocationCodes = value; } }
        }

        private string _TopicCode;
        /// <summary>
        /// Locally defined value, used for categorization of courses.
        /// </summary>
        public string TopicCode
        {
            get { return _TopicCode; }
            set { if (value != null) { _TopicCode = value; } }
        }

        /// <summary>
        /// Code indicating the term sesion cycle in which this course is only offered - if restricted.  Blank means anytime.
        /// </summary>
        public string TermSessionCycle { get; set; }

        /// <summary>
        /// Code indicating the term yearly cycle in which this course is only offered - if restricted.  Blank means anytime.
        /// </summary>
        public string TermYearlyCycle { get; set; }

        /// <summary>
        /// String describes term cycles when a course is offered (such as fall only).
        /// Note: Generally we don't carry the description of a code in the entity but the code itself.  However, since this was
        /// already in the DTO the TermSessionCycle code was added above. 
        /// </summary>
        public string TermsOffered { get; set; }

        /// <summary>
        /// String describes yearly cycles when a course is offered (such as every other year).
        /// Note: Generally we don't carry the description of a code in the entity but the code itself.  However, since this was
        /// already in the DTO the TermYearlyCycle code was added above.
        /// </summary>
        public string YearsOffered { get; set; }

        /// <summary>
        /// Effective start date of course
        /// </summary>
        public DateTime? StartDate { get; set; }

        private DateTime? _EndDate;
        /// <summary>
        /// Effective end date of course. Required when course terminated.
        /// </summary>
        public DateTime? EndDate
        {
            get { return _EndDate; }
            set
            {
                if (value.HasValue)
                {
                    if (StartDate.HasValue && value.Value < StartDate.Value)
                    {
                        throw new ArgumentOutOfRangeException("value", "End Date cannot be earlier than Start Date.");
                    }
                    _EndDate = value;
                }
            }
        }

        private List<Requisite> _Requisites = new List<Requisite>();
        /// <summary>
        /// List of requisites defined for this course.
        /// </summary>
        public List<Requisite> Requisites
        {
            get { return _Requisites; }
            set { if (value != null) { _Requisites = value; } }
        }

        private readonly List<string> _EquatedCourseIds = new List<string>();
        /// <summary>
        /// List of individual course Ids, representing each course which can be used as an acceptable substitute for this course.
        /// Used for degree audit evaluation, corequisite checking, prerequisite checking (same as DA evaluation), GPA calculation
        /// </summary>
        public ReadOnlyCollection<string> EquatedCourseIds {get; private set;}

        /// <summary>
        /// Gets or Sets a value indicating that this course is a pseudo course.
        /// </summary>
        public bool IsPseudoCourse { get; set; }
        //
        // Added fields for Student Success Project (SRM)
        //
        /// <summary>
        /// Federal Course Classification code
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// List of Local Government Course Classification codes
        /// </summary>
        public IEnumerable<string> LocalCourseClassifications { get; set; }
        /// <summary>
        /// List of Course Type codes for this Course
        /// </summary>
        public IEnumerable<string> CourseTypeCodes { get; set; }

        /// <summary>
        /// Locally defined credit type for this course; Used for rule evaluation.
        /// </summary>
        public string LocalCreditType { get; set; }

        // Private list of course types, available publicly through Types property
        private readonly List<string> _types = new List<string>();
        /// <summary>
        /// The list of strings that may be used to categorize a course.
        /// </summary>
        public ReadOnlyCollection<string> Types {get; private set;}

        private List<string> _InstructionalMethodCodes = new List<string>();
        /// <summary>
        /// Instructional method codes for the course.
        /// </summary>
        public ReadOnlyCollection<string> InstructionalMethodCodes { get; private set; }

        /// <summary>
        /// Grade Scheme Code
        /// </summary>
        public string GradeSchemeCode { get; set; }

        private List<CourseApproval> _CourseApprovals = new List<CourseApproval>();
        /// <summary>
        /// Approval information for the course
        /// </summary>
        public ReadOnlyCollection<CourseApproval> CourseApprovals { get; private set; }

        /// <summary>
        /// Indicates whether the course can be taken Pass/No Pass
        /// </summary>
        public bool AllowPassNoPass { get; set; }

        /// <summary>
        /// Indicates whether the course can be audited
        /// </summary>
        public bool AllowAudit { get; set; }

        /// <summary>
        /// Indicates whether the course can only be taken Pass/No Pass
        /// </summary>
        public bool OnlyPassNoPass { get; set; }

        /// <summary>
        /// Indicates whether the course allows waitlists
        /// </summary>
        public bool AllowWaitlist { get; set; }

        /// <summary>
        /// Indicates whether the student must get consent from the instructor to take the course
        /// </summary>
        public bool IsInstructorConsentRequired { get; set; }

        /// <summary>
        /// Identifies the waitlist rules used by default for this course
        /// </summary>
        public string WaitlistRatingCode { get; set; }

        /// <summary>
        /// List of departments to which this course can be applied.
        /// </summary>
        public List<string> DepartmentCodes { get { return Departments.Select(d => d.AcademicDepartmentCode).ToList(); } }

        private List<LocationCycleRestriction> _LocationCycleRestrictions = new List<LocationCycleRestriction>();
        /// <summary>
        /// Session and yearly cycle restrictions by location for the course.
        /// </summary>
        public ReadOnlyCollection<LocationCycleRestriction> LocationCycleRestrictions { get; private set; }

        /// <summary>
        /// Identifies the external source associated with this course.
        /// </summary>
        public string ExternalSource { get; set; }

        /// <summary>
        /// Should the grades be immediately verified.
        /// </summary>
        public bool? VerifyGrades { get; set; }


        /// <summary>
        /// Billing credits associated with course
        /// </summary>
        /// 
        public decimal? BillingCredits { get; set; }

        /// <summary>
        /// Indicates whether to count credits for retake of course. 
        /// </summary>
        public bool AllowToCountCourseRetakeCredits { get; set; }

        private List<decimal?> _InstructionalMethodContactHours = new List<decimal?>();
        /// <summary>
        /// Instructional method codes for the course.
        /// </summary>
        public ReadOnlyCollection<decimal?> InstructionalMethodContactHours { get; private set; }

        private List<string> _InstructionalMethodContactPeriods = new List<string>();
        /// <summary>
        /// Instructional method codes for the course.
        /// </summary>
        public ReadOnlyCollection<string> InstructionalMethodContactPeriods { get; private set; }

        /// <summary>
        /// Are students allowed to waitlist multiple sections of the same course
        /// </summary>
        public bool? AllowWaitlistMultipleSections { get; set; }


        #endregion

        #region Constructor

        /// <summary>
        /// Creates a Course. All parameters are required, except either MinCredits or CEUs
        /// is required (non-null), but not both.         
        /// (Other fields are required but not included because not identified as needed
        ///    for self service: Department percent, effective start date (required before sections
        ///    can be built), grade scheme (only if credits specified, not if CEUs), 
        ///    status association.
        /// </summary>
        /// <param name="id">ID of the course</param>
        /// <param name="shortTitle">Short title of the course</param>
        /// <param name="longTitle">Longer title of the course</param>
        /// <param name="departments">Departments for the course</param>
        /// <param name="subjectCode">Subject of the course</param>
        /// <param name="number">Number of the course</param>
        /// <param name="academicLevelCode">Academic Level code of the course</param>
        /// <param name="courseLevelCodes">Course Level codes for the course</param>
        /// <param name="minCredits">Minimum credits for the course</param>
        /// <param name="ceus">CEUs for the course</param>
        /// <param name="approvals">Approvals/statuses for the course</param>
        public Course(string id, string shortTitle, string longTitle, ICollection<OfferingDepartment> departments, string subjectCode, string number, string academicLevelCode,
            ICollection<string> courseLevelCodes, decimal? minCredits, decimal? ceus, ICollection<CourseApproval> approvals)
        {
            if (string.IsNullOrEmpty(shortTitle))
            {
                throw new ArgumentNullException("shortTitle");
            }
            if (departments.Count() == 0 || departments == null)
            {
                throw new ArgumentNullException("departments");
            }
            if (string.IsNullOrEmpty(subjectCode))
            {
                throw new ArgumentNullException("subjectCode");
            }
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number");
            }
            if (string.IsNullOrEmpty(academicLevelCode))
            {
                throw new ArgumentNullException("academicLevelCode");
            }
            if (courseLevelCodes.Count() == 0)
            {
                throw new ArgumentException("At least one course level required");
            }
            if ((minCredits == null) && (ceus == null))
            {
                throw new ArgumentException("Either Course Min Credits or CEUs is required ");
            }
            if (minCredits < 0)
            {
                throw new ArgumentOutOfRangeException("minCredits");
            }
            if (ceus < 0)
            {
                throw new ArgumentOutOfRangeException("ceus");
            }
            if (approvals == null)
            {
                throw new ArgumentNullException("approvals");
            }

            // Initialize empty collections
            LocationCodes = new List<string>();

            _Id = id;
            _Title = shortTitle;
            _LongTitle = !string.IsNullOrEmpty(longTitle) ? longTitle : shortTitle;
            _Departments = departments.ToList();
            _SubjectCode = subjectCode;
            _Number = number;
            _AcademicLevelCode = academicLevelCode;
            _CourseLevelCodes = courseLevelCodes.ToList();
            _MinimumCredits = minCredits;
            _Ceus = ceus;
            _CourseApprovals = approvals.ToList();

            // If there are no valid approvals, default to "Active"
            // Courses are supposed to have approvals, and this condition is technically corrupt, but it's common enough that it has to be allowed
            _Status = approvals.Count == 0 ? CourseStatus.Active : approvals.OrderByDescending(a => a.StatusDate).ToList()[0].Status;

            IsPseudoCourse = false;

            // Initialize public readonly collections
            Types = _types.AsReadOnly();
            EquatedCourseIds = _EquatedCourseIds.AsReadOnly();
            InstructionalMethodCodes = _InstructionalMethodCodes.AsReadOnly();
            Departments = _Departments.AsReadOnly();
            CourseApprovals = _CourseApprovals.AsReadOnly();
            LocationCycleRestrictions = _LocationCycleRestrictions.AsReadOnly();
            InstructionalMethodContactHours = _InstructionalMethodContactHours.AsReadOnly();
            InstructionalMethodContactPeriods = _InstructionalMethodContactPeriods.AsReadOnly();
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Course other = obj as Course;
            if (other == null)
            {
                return false;
            }
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return SubjectCode + "*" + Number;
        }

        /// <summary>
        /// Adds a type string to the list of course types
        /// </summary>
        /// <param name="type">Type string to add to the list of Types</param>
        public void AddType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Course Type cannot be null or empty");
            }
            if (!_types.Contains(type))
            {
                _types.Add(type);
            }
        }

        /// <summary>
        /// Add an equated course ID to a course
        /// </summary>
        /// <param name="courseId">Equated course ID</param>
        public void AddEquatedCourseId(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId", "Equated Course Id cannot be null or empty");
            }
            if (!_EquatedCourseIds.Contains(courseId))
            {
                _EquatedCourseIds.Add(courseId);
            }
        }

        /// <summary>
        /// Add an instructional method code to a course
        /// </summary>
        /// <param name="instructionalMethodCode">Instructional method code</param>
        public bool AddInstructionalMethodCode(string instructionalMethodCode)
        {
            if (string.IsNullOrEmpty(instructionalMethodCode))
            {
                throw new ArgumentNullException("instructionalMethodCode", "Instructional method code cannot be null or empty");
            }
            if (!_InstructionalMethodCodes.Contains(instructionalMethodCode))
            {
                _InstructionalMethodCodes.Add(instructionalMethodCode);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Add an instructional method code to a course
        /// </summary>
        /// <param name="instructionalMethodCode">Instructional method code</param>
        public void AddInstructionalMethodHours(decimal? instructionalMethodHours)
        {
            _InstructionalMethodContactHours.Add(instructionalMethodHours);
        }

        /// <summary>
        /// Add an instructional method period to a course
        /// </summary>
        /// <param name="instructionalMethodCode">Instructional method period</param>
        public void AddInstructionalMethodPeriod(string instructionalMethodPeriod)
        {
            _InstructionalMethodContactPeriods.Add(instructionalMethodPeriod);
        }

        /// <summary>
        /// Add an Location Cycle Restriction to a course
        /// </summary>
        /// <param name="locationCycleRestriction">Instructional method code</param>
        public void AddLocationCycleRestriction(LocationCycleRestriction locationCycleRestriction)
        {
            if (locationCycleRestriction == null)
            {
                throw new ArgumentNullException("locationCycleRestriction", "Location cycle restriction cannot be null or emtpy");
            }
            // Only add a restriction if it is not a duplicate.  Otherwise disregard.
            if (!_LocationCycleRestrictions.Contains(locationCycleRestriction))
            {
                _LocationCycleRestrictions.Add(locationCycleRestriction);
            }
        }

        #endregion
    }
}
