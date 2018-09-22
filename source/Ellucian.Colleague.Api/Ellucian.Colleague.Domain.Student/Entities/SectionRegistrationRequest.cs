// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionRegistrationRequest
    {
        public string StudentId { get; set; }
        public SectionRegistration Section { get; set; }
        public bool CreateStudentFlag { get; set; }
        public string RegGuid { get; set; }

        public List<MidTermGrade> MidTermGrades { get; set; }
        public TermGrade FinalTermGrade { get; set; }
        public VerifiedTermGrade VerifiedTermGrade { get; set; }

        public DateTimeOffset? InvolvementStartOn { get; set; }
        public DateTimeOffset? InvolvementEndOn { get; set; }
        public DateTimeOffset? GradeExtentionExpDate { get; set; }
        public string ReportingStatus { get; set; }
        public DateTimeOffset? ReportingLastDayOfAttendance { get; set; }
        public DateTimeOffset? TranscriptVerifiedGradeDate { get; set; }
        public string TranscriptVerifiedBy { get; set; }
        public string StudentAcadCredId { get; set; }
        
        public SectionRegistrationRequest(string studentId, string guid, SectionRegistration section)
        {
            StudentId = studentId;
            RegGuid = guid;
            Section = section;
        }
    }
}
