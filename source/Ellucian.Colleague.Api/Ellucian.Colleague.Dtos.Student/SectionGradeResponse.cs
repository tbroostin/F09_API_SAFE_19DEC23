// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Represents a response to importing student section grades
    /// </summary>
    public class SectionGradeResponse
    {
        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public List<SectionGradeResponseError> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionGradeResponse"/> class.
        /// </summary>
        public SectionGradeResponse()
        {
            Errors = new List<SectionGradeResponseError>();
        }
    }
}
