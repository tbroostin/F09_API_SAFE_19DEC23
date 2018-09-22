// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An institutionally-defined course grade
    /// </summary>
    public class Grade
    {
        /// <summary>
        /// Unique ID for this grade
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Letter grade
        /// </summary>
        public string LetterGrade { get; set; }
        /// <summary>
        /// Decimal equivalent of this grade
        /// </summary>
        public decimal? GradeValue { get; set; }
        /// <summary>
        /// Id of the grade scheme in which this grade is included
        /// </summary>
        public string GradeSchemeCode { get; set; }
        /// <summary>
        /// Description of this grade
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Boolean indicates whether this is a withdraw grade
        /// </summary>
        public bool IsWithdraw { get; set; }
        /// <summary>
        /// Priority Value when two grades are to be compared as one being greater than the other.
        /// </summary>
        public decimal? GradePriority { get; set; }
        /// <summary>
        /// Indicates if this should be excluded from the list of grades presented to faculty during grading.
        /// </summary>
        public bool ExcludeFromFacultyGrading { get; set; }
        /// <summary>
        /// The grade to which an incomplete will revert if not updated by the expiration date.
        /// </summary>
        public string IncompleteGrade { get; set; }
        /// <summary>
        /// Indicates if Last date of attendance is required if the grade is final grade
        /// </summary>
        public bool RequireLastAttendanceDate { get; set; }

    }
}
