// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Academic Credit represents some credit that a student has earned.  Typically, academic credit
    /// is a course that was taken, but can also be transfer work or non-course work (life experience, portfolio, etc)
    /// </summary>
    [Serializable]
    public class AcademicCredit
    {
        // Required Ids
        private string _Id;
        /// <summary>
        /// Unique ID.
        /// </summary>
        public string Id { get { return _Id; } }

        // Course- and section-related stuff
        private Course _Course;
        /// <summary>
        /// Course taken for this credit.
        /// </summary>
        public Course Course { get { return _Course; } }

        public string StudentId { get; set; }
        public string SectionId { get; set; }

        /// <summary>
        /// Title of the course at the time student took it. Could be different than current course title.
        /// This may be different than the current title of the course object
        /// </summary>
        public string Title { get; set; }  
        
        /// <summary>
        /// Name of the course when the student took it
        /// This may be different than the current name of the course object
        /// </summary>     
        public string CourseName { get; set; }                  

        /// <summary>
        /// Subject of this academic credit 
        /// </summary>
        public string SubjectCode { get; set; }

        /// <summary>
        /// Section number, typically only for institutional credits
        /// </summary>
        public string SectionNumber { get; set; }

        private List<string> _DepartmentCodes;
        /// <summary>
        /// List of related departments
        /// </summary>
        public List<string> DepartmentCodes { get { return _DepartmentCodes; } }
       
        /// <summary>
        /// List of related divisions
        /// </summary>
        public List<string> Divisions { get; private set; }
        /// <summary>
        /// list of related schools
        /// </summary>
        public List<string> Schools { get; private set; }
        /// <summary>
        /// Level of this credit, such as 100, 200, 300
        /// </summary>
        public string CourseLevelCode { get; set; }
        /// <summary>
        /// Academic level of this credit, such as Undergraduate, Graduate
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// Grade scheme used to assign the grade
        /// </summary>
        public string GradeSchemeCode { get; set; }
        /// <summary>
        /// The final grade earned for the credit
        /// </summary>
        public Grade VerifiedGrade { get; set; }

        /// <summary>
        /// HasVerifiedGrade is used to indicate if a verified grade exists
        /// The grade itself could be suppressed when there is a grade restriction.
        /// </summary>
        public bool HasVerifiedGrade
        {
            get
            {
                return VerifiedGrade != null;
            }
        }

        /// <summary>
        /// The verified letter grade, translated from the Grade object. Added for the purpose of rule evaluation.
        /// </summary>
        public string VerifiedLetterGrade
        {
            get
            {
                if (VerifiedGrade != null)
                return VerifiedGrade.LetterGrade;
                else return string.Empty;
            }
        }

        /// <summary>
        /// The final, but not necessarily verified, grade code 
        /// </summary>
        public string FinalGradeId { get; set; }

        /// <summary>
        /// Final grade expiration date
        /// </summary>
        public DateTime? FinalGradeExpirationDate { get; set; }

        /// <summary>
        /// Date of last attendance
        /// </summary>
        public DateTime? LastAttendanceDate { get; set; }

        /// <summary>
        /// Never attended 
        /// </summary>
        public bool? NeverAttended { get; set; }

        /// <summary>
        /// Term in which the credit was completed
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Location where the credit was completed
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Attempted credits, typically the same as the intended credits for the course unless the verified grade is not to be included in attempted.
        /// </summary>
        public decimal AttemptedCredit { get; set; }

        /// <summary>
        /// Completed credits, typically the same as the original credits for the course.
        /// </summary>
        public decimal? CompletedCredit { get; set; }

        /// <summary>
        /// Intended credits
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Continuing education units earned
        /// </summary>
        public decimal ContinuingEducationUnits { get; set; }
        
        /// <summary>
        /// Total grade points earned, used as the numerator in the GPA calculation
        /// </summary>
        public decimal GradePoints { get; set; }
        
        /// <summary>
        /// GPA credits earned, used as the denominator in the GPA calculation
        /// </summary>
        public decimal? GpaCredit { get; set; }
 
        /// <summary>
        /// Adjusted Credit value, accounting for repeats
        /// </summary>
        public decimal? AdjustedCredit { get; set; }
        
        /// <summary>
        /// Adjusted GPA credits, accounting for repeats
        /// </summary>
        public decimal AdjustedGpaCredit { get; set; }

        /// <summary>
        /// Adjusted grade points, accounting for repeats
        /// </summary>
        public decimal AdjustedGradePoints { get; set; }

        /// <summary>
        /// Can this academic credit be replaced (when retaken)?
        /// </summary>
        public bool CanBeReplaced { get; set; }

        /// <summary>
        /// Standardized credit type, distinguishes Institutional, Transfer, ContinuingEducation, Other
        /// </summary>
        public CreditType Type { get; set; }

        /// <summary>
        /// Locally defined credit type that maps to the standardized Type
        /// </summary>
        public string LocalType { get; set; }

        /// <summary>
        /// Indicates whether credit is New, Added, Dropped, etc
        /// </summary>
        public CreditStatus Status { get; set; }
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// List of credit status/date/time/reason
        /// </summary>
        public List<AcademicCreditStatus> AcademicCreditStatuses { get; set; }

        /// <summary>
        /// Mode in which the course was taken: Graded, Pass/Fail, Audit
        /// </summary>
        public GradingType GradingType { get; set; }

        /// <summary>
        /// Is this academic credit based upon a non-course equivalent?
        /// Non-course equivalent would be credit granted for life experience, portfolio, etc.
        /// </summary>
        public bool IsNonCourse { get; set; }
 
        /// <summary>
        /// Mark assists with grouping of credits, specifically when the credits have already been used in a completed program
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// Date the academic credit was started
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// (Rule-comparable) academic credit start date
        /// </summary>
        public DateTime StartDateForRules 
        { 
            get 
            {
                if (StartDate.HasValue)
                {
                    return new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day, 0, 0, 0);
                }
                else
                {
                    return DateTime.MinValue;
                }
            } 
        }

        /// <summary>
        /// Date by which this academic credit was completed (needed at least for requisite checking when no section exists)
        /// </summary>
        public DateTime? EndDate { get; set; }                     

        /// <summary>
        /// Timestamp given when final grade was verified
        /// </summary>
        public DateTimeOffset? VerifiedGradeTimestamp { get; set; }

        private List<MidTermGrade> _MidTermGrades = new List<MidTermGrade>();
        /// <summary>
        /// List of Mid-Term grades
        /// </summary>
        public List<MidTermGrade> MidTermGrades { get { return _MidTermGrades; } }

        /// <summary>
        /// Indicates if this item is transfer credit. If this is a TransferOrNonCourse credit
        /// yet not a noncourse, then it must be Transfer credit.
        /// </summary>
        public bool IsTransfer
        {
            get { return (Status == CreditStatus.TransferOrNonCourse && !IsNonCourse); }
        }

        /// <summary>
        /// Indicates whether this credit is considered a completed academic credit. Completed credits are indicated when
        ///   there is a verified grade that is not an incomplete grade, or if it's a transfer/noncourse or if there is no grade scheme and the end date has passed.
        /// </summary>
        public bool IsCompletedCredit
        {
            get
            {
                return (HasVerifiedGrade && string.IsNullOrEmpty(VerifiedGrade.IncompleteGrade)) || 
                    ((Status == CreditStatus.TransferOrNonCourse || string.IsNullOrEmpty(GradeSchemeCode)) && (EndDate != null && EndDate <= DateTime.Today));
            }
        }

        /// <summary>
        /// Status indicates whether credit is replaced or possibly replaced
        /// </summary>
        public ReplacedStatus ReplacedStatus { get; set; }

        /// <summary>
        /// Status indicates whether credit is a replacement or a possible replacement of another credit
        /// </summary>
        public ReplacementStatus ReplacementStatus { get; set; }

        /// <summary>
        /// The list of academic credit Ids that are involved in the replacement
        /// </summary>
        public List<string> RepeatAcademicCreditIds { get; set; }

        /// <summary>
        /// ID for a student course section associated with the academic credit
        /// </summary>     
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// Base constructor for academic credit. 
        /// </summary>
        /// <param name="id">ID of this academic credit</param>
        public AcademicCredit(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _Id = id;
            _DepartmentCodes = new List<string>();
            Status = CreditStatus.Unknown;
            GradingType = Entities.GradingType.Graded;
            ReplacedStatus = ReplacedStatus.NotReplaced;
            ReplacementStatus = ReplacementStatus.NotReplacement;
            AcademicCreditStatuses = new List<AcademicCreditStatus>();
            Divisions = new List<string>();
            Schools = new List<string>();
        }

        /// <summary>
        /// Academic Credit constructor. Requires Course object and Id of section.
        /// </summary>
        /// <param name="id">Unique Id of this academic credit</param>
        /// <param name="course">Course object on which credit is based</param>
        /// <param name="sectionid"></param>
        public AcademicCredit(string id, Course course, string sectionid)
            : this(id)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course", "Course is required for an Academic Credit.");
            }
          
            _Course = course;
            SectionId = sectionid;
            _DepartmentCodes = new List<string>();
            SubjectCode = course.SubjectCode;
            GradingType = Entities.GradingType.Graded;
            
        }

        /// <summary>
        /// Add a department to the list of departments
        /// </summary>
        /// <param name="department"></param>
        public void AddDepartment(string department)
        {
            if (string.IsNullOrEmpty(department)) throw new ArgumentNullException("department");
            if (!_DepartmentCodes.Contains(department))
            {
                _DepartmentCodes.Add(department);
            }
        }

        /// <summary>
        /// Add a midterm grade to the list of midterm grades.
        /// </summary>
        /// <param name="midTermGrade"></param>
        public void AddMidTermGrade(MidTermGrade midTermGrade)
        {
            if (midTermGrade == null) throw new ArgumentNullException("midTermGrade");
            
            bool invalidPos = false;
            foreach (MidTermGrade midGrade in _MidTermGrades) {
                if (midGrade.Position == midTermGrade.Position)
                {
                    invalidPos = true;
                }
            }
            if (invalidPos) throw new ArgumentException("midterm grade position occupied");
            _MidTermGrades.Add(midTermGrade);
        }

        /// <summary>
        /// Add to the academic credit statuses.
        /// </summary>
        /// <param name="midTermGrade"></param>
        public void AddStatus(string status, DateTime? date, DateTime? time, string reason)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("status", "Status must be specified");
            }
            //if (StudentHomeLocations.Where(h => h.Location.Equals(location)).Count() == 0)
            if (AcademicCreditStatuses.Where(a => a.Status.Equals(status)).Count() == 0)
            {
                //StudentHomeLocation Location = new StudentHomeLocation(location, startDate, endDate, isPrimary);
                //StudentHomeLocations.Add(Location);
                AcademicCreditStatus Status = new AcademicCreditStatus(status, date, time, reason);
                AcademicCreditStatuses.Add(Status);
            }
        }

        /// <summary>
        /// Indicates if this credit is institutional (not transfer)
        /// </summary>
        /// <returns></returns>
        public bool IsInstitutional()
        {
            return Type == CreditType.Institutional;

        }

        /// <summary>
        /// Equals method used for comparisons
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            AcademicCredit other = obj as AcademicCredit;
            if (other == null)
            {
                return false;
            }
            return Id.Equals(other.Id);

        }

        /// <summary>
        /// Needed for Equals comparisons
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Provided for ease of display in debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string identifier;
            if (Course == null)
            {
                identifier = "(noncourse, id=" + Id + ")";
            }
            else
            {
                identifier = Course.ToString();
            }
            return "ACred " + identifier;
        }
    }
}
