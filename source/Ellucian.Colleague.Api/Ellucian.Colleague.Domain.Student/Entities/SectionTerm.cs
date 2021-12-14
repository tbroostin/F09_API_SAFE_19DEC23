// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section's Term
    /// </summary>
    [Serializable]
    public class SectionTerm
    {    
        /// <summary>
        /// Instructional method code
        /// </summary>
        public string TermId{ get; private set; }
        /// <summary>
        /// Instructional method description
        /// </summary>
        public int ReportingYear { get; private set; }
        /// <summary>
        /// Sequence
        /// </summary>
        public int Sequence { get; private set; }

        public SectionTerm(string termId, int reportingYear, int sequence)
        {
            TermId = termId;
            ReportingYear = reportingYear;
            Sequence = sequence;
        }
        public SectionTerm()
        {

        }
    
    }
}

