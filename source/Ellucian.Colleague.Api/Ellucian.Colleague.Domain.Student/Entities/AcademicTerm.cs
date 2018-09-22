// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class AcademicTerm
    {
        public string TermId { get; set; }
        public decimal? GradePointAverage { get; set; }
        public decimal Credits { get; set; }
        public decimal ContinuingEducationUnits { get; set; }
        public List<AcademicCredit> AcademicCredits { get; set; }

        public AcademicTerm()
        {
            AcademicCredits = new List<AcademicCredit>();
        }
    }
}
