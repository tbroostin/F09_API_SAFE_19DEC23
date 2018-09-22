// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    [Serializable]
    public class PlannedCredit
    {
        public Course Course { get; set; }

        public string TermCode { get; set; }

        public string SectionId { get; set; }

        public decimal? Credits { get; set; }

        public PlannedCredit(Course course, string termCode, string sectionId = null)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course", "Course must be specified.");
            }
            if (string.IsNullOrEmpty(termCode) && string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("termCode", "Either term or section must be specified.");
            }

            Course = course;
            SectionId = string.IsNullOrEmpty(sectionId) ? string.Empty : sectionId;
            TermCode = string.IsNullOrEmpty(termCode) ? string.Empty : termCode;
        }
    }
}
