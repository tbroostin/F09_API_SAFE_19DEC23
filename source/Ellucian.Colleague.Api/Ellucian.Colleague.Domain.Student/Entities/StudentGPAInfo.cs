// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentGPAInfo
    {
        public string Term { get; set; }
        public decimal? StcAttCredit { get; set; }
        public decimal? StcCumContribAttCredit { get; set; }
        public decimal? StcCumContribGpaCredit { get; set; }
        public string AcademicLevel { get; set; }
        public decimal? StcCumContribGradePoint { get; set; }
        public decimal? StcAltcumContribGradePoint { get; set; }
        public decimal? StcAltCumContribGpaCredit { get; set; }
        public decimal? StcAltCumContribAttCredit { get; set; }
        public List<string> MarkAcadCredentials { get; set; }
        public string CreditType { get; set; }
        public string StcStudentCourseSec { get; set; }
        public string StcReportingTerm { get; set; }
        public string SourceKey { get; set; }
    }
}