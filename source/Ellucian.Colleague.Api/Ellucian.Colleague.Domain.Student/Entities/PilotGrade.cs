// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class PilotGrade
    {
        public PilotGrade(string studentId, string sectionId, string grade)
        {
            this.StudentId = studentId;
            this.SectionId = sectionId;
            this.Grade = grade;
        }

        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Section this grade relates to.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Grade position
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Letter grade
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Final or Midterm
        /// </summary>
        public string GradeType { get; set; }

        /// <summary>
        /// Grade priority
        /// </summary>
        public decimal? GradeValue { get; set; }
    }
}
