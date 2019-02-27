// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
   [Serializable]
    public class StudentTranscriptGradesHistory : IEquatable<StudentTranscriptGradesHistory>
    {
        public string GradeChangeReason { get; set; }
        public string PreviousVerifiedGradeValue { get; set; }
        public DateTime? GradeChangeDate { get; set; }


        public StudentTranscriptGradesHistory(string gradeChangeReason, string previousVerifiedGradeValue, DateTime? gradeChangeDate)
        {
            GradeChangeReason = gradeChangeReason;
            PreviousVerifiedGradeValue = previousVerifiedGradeValue;
            GradeChangeDate = gradeChangeDate;
        }

        public bool Equals(StudentTranscriptGradesHistory other)
        {
            return this.GradeChangeReason == other.GradeChangeReason &&
                   this.PreviousVerifiedGradeValue == other.PreviousVerifiedGradeValue &&
                   this.GradeChangeDate == other.GradeChangeDate;
        }
    }
}
