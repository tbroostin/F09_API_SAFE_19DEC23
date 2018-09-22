// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Credits earned by a student, shown in student's AcademicHistory
    /// </summary>
    public class AcademicCredit2
    {
        /// <summary>
        /// The ID of this academic credit
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the course related to this academic credit. (May be blank)
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Student Id for credit.  Needed for reference back to student.
        /// </summary>
        public string StudentId { get; set; }        

        /// <summary>
        /// Id of the course section the student was registered for. (May be blank)
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// CourseName represents the course name at the time the student took the course (HIST*101)
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// Title represents the course title at the time the student took it. Could be different than course, but can also be blank. (Intro to Math)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Id of the grade item for this academic credit.  (May be blank)
        /// </summary>
        public string VerifiedGradeId { get; set; }

        /// <summary>
        /// DateTime of last verified grade change, in Coordinated Universal Time (UTC) format
        /// </summary>
        public DateTimeOffset? VerifiedGradeTimestamp { get; set; }

        /// <summary>
        /// Id of the final, but not necessarily verified, gradefor this academic credit. See verified grade for final verified grade. (May be blank)
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
        /// Indicates if the student never attended the class
        /// </summary>
        public bool? NeverAttended { get; set; }

        /// <summary>
        /// Credit is the "enrolled" credits (STC.CRED)
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// GPA credit is the sum of the GPA credits (STC.GPA.CRED) used in GPA calculation of the term GPAs.
        /// </summary>
        public decimal GpaCredit { get; set; }

        /// <summary>
        /// GradePoints awarded based on verified grade.
        /// </summary>
        public decimal GradePoints { get; set; }

        /// <summary>
        /// Credits count as attempted.
        /// </summary>
        public decimal AttemptedCredit { get; set; }

        /// <summary>
        /// Credits count as completed.
        /// </summary>
        public decimal CompletedCredit { get; set; }

        /// <summary>
        /// Continuing Education Units
        /// </summary>
        public decimal ContinuingEducationUnits { get; set; }

        /// <summary>
        /// Status can be New, Add, Dropped, Withdrawn, Deleted, Cancelled, TransferOrNonCourse, Preliminary, Unknown
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The Date that the current status was assigned
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Term in which the credit was taken
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Location where the credit was completed
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Midterm grades for this academic credit
        /// <see cref="MidTermGrade"/>
        /// </summary>
        public List<MidTermGrade2> MidTermGrades { get; set; }

        /// <summary>
        /// Denotes whether the associated section Id was taken for a grade, pass/fail, or audit. Default is graded.
        /// <see cref="GradingType"/>
        /// </summary>
        public GradingType GradingType { get; set; }

        /// <summary>
        /// Section Number
        /// </summary>
        public string SectionNumber { get; set; }

        /// <summary>
        /// Indicates whether there is a verified grade, even if the verified grade is suppressed due to a grade viewing restriction.
        /// </summary>
        public bool HasVerifiedGrade { get; set; }

        /// <summary>
        /// The final adjusted completed credits when there is a verified grade. 
        /// </summary>
        public decimal AdjustedCredit { get; set; }

        /// <summary>
        /// Indicates whether this credit was granted based on a non-course equivalency
        /// </summary>
        public bool IsNonCourse { get; set; }

        /// <summary>
        /// Indicates whether this credit is considered a completed academic credit. Completed credits are indicated when
        /// there is a verified grade or it's a transfer/noncourse or there is no grade scheme and the end date has passed.
        /// </summary>
        public bool IsCompletedCredit { get; set; }

        /// <summary>
        /// Indicates whether this credit has been replaced or will possibly be replaced
        /// </summary>
        public ReplacedStatus ReplacedStatus { get; set; }

        /// <summary>
        /// Indicates whether this credit is a replacement for another credit, or will possibly be a replacement in the future 
        /// </summary>
        public ReplacementStatus ReplacementStatus { get; set; }

        /// <summary>
        /// Start Date for this Academic Credit
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date for this Academic Credit
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The final adjusted GPA credits when there is a verified grade. Needed for when the student has completed the course
        /// but the GPA falls below a requirement minimum.
        /// </summary>
        public decimal AdjustedGpaCredit { get; set; }

        /// <summary>
        /// Default constructor for the AcademicCredit transfer object
        /// </summary>
        public AcademicCredit2()
        {
            MidTermGrades = new List<MidTermGrade2>();
            // Explicitly set the grading type to graded by default.
            GradingType = Dtos.Student.GradingType.Graded;
        }
    }
}
