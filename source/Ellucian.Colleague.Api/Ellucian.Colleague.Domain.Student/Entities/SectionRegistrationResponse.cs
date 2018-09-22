// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionRegistrationResponse
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public string SectionId { get; set; }
        public string StatusCode { get; set; }
        public string GradeScheme { get; set; }
        public string PassAudit { get; set; }
        public List<RegistrationMessage> Messages { get; set; }

        public List<MidTermGrade> MidTermGrades { get; set; }
        public TermGrade FinalTermGrade { get; set; }
        public VerifiedTermGrade VerifiedTermGrade { get; set; }
        public DateTimeOffset? InvolvementStartOn { get; set; }
        public DateTimeOffset? InvolvementEndOn { get; set; }
        public DateTimeOffset? GradeExtentionExpDate { get; set; }
        public string ReportingStatus { get; set; }
        public DateTimeOffset? ReportingLastDayOdAttendance { get; set; }
        public DateTimeOffset? TranscriptVerifiedGradeDate { get; set; }
        public string TranscriptVerifiedBy { get; set; }
        public bool ErrorOccured { get; set; }
        public string CreditType { get; set; }
        //V7 changes
        public string AcademicLevel { get; set; }
        public decimal? Ceus { get; set; }
        public decimal? Credit { get; set; }
        public decimal? GradePoint { get; set; }
        public string ReplCode { get; set; }
        public List<string> RepeatedAcadCreds { get; set; }
        public decimal? AltcumContribCmplCred { get; set; }
        public decimal? AltcumContribGpaCred { get; set; }
        public decimal? EarnedCredit { get; set; }
        public decimal? EarnedCeus { get; set; }

        public SectionRegistrationResponse(string guid, string studentId, string sectionId, string statusCode, string gradeScheme, string passAudit, IEnumerable<RegistrationMessage> messages)
        {
            Id = guid;
            StudentId = studentId;
            SectionId = sectionId;
            StatusCode = statusCode;
            GradeScheme = gradeScheme;
            PassAudit = passAudit;
            Messages = new List<RegistrationMessage>(messages);
        }

        public SectionRegistrationResponse(string guid, string studentId, string sectionId, string statusCode, IEnumerable<RegistrationMessage> messages)
        {
            Id = guid;
            StudentId = studentId;
            SectionId = sectionId;
            StatusCode = statusCode;
            Messages = new List<RegistrationMessage>(messages);
        }

        public SectionRegistrationResponse(IEnumerable<RegistrationMessage> messages)
        {
            Messages = new List<RegistrationMessage>(messages);
        }
    }
}
