// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class TermGrade
    {
        private string _gradeId;
        private DateTimeOffset? _submittedOn;
        private string _submittedBy;

        /// <summary>
        /// ID of the grade given
        /// </summary>
        public string GradeId { get { return _gradeId; } }

        /// <summary>
        /// Letter Grade like A, B, C, F etc.
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Colleague ID of the grade given
        /// </summary>
        public string GradeKey { get; set; }

        /// <summary>
        /// Date of the grade given
        /// </summary>
        public DateTimeOffset? SubmittedOn { get { return _submittedOn; } }

        /// <summary>
        /// Grade submittedby
        /// </summary>
        public string SubmittedBy { get { return _submittedBy; } }

        /// <summary>
        /// Grade typecode such as FINAL or VERIFIED
        /// </summary>
        public string GradeTypeCode { get; set; }

        /// <summary>
        /// The reason for the  grade submission
        /// </summary>
        public string GradeChangeReason { get; set; }

        /// <summary>
        /// Constructor for final grade
        /// </summary>
        /// <param name="id"></param>
        /// <param name="submittedOn"></param>
        /// <param name="submittedBy"></param>
        public TermGrade(string id, DateTimeOffset? submittedOn, string submittedBy, string gradeTypeCode = "")
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");

            _gradeId = id;
            _submittedOn = submittedOn;
            _submittedBy = submittedBy;
            GradeTypeCode = gradeTypeCode;
        }
    }
}
