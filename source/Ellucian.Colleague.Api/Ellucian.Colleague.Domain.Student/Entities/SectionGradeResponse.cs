// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Response from repository related to updating a grade for a student.
    /// </summary>
    [Serializable]
    public class SectionGradeResponse
    {
        /// <summary>
        /// Status of updating the student's grade
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Errors related to updating the grades
        /// </summary>
        public List<SectionGradeResponseError> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the SectionGradeResponse class.
        /// </summary>
        public SectionGradeResponse()
        {
            Errors = new List<SectionGradeResponseError>();
        }
    }
}
