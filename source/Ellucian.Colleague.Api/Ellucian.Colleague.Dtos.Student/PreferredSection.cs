// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A course selected by the student for registration. Not applicable for students using Degree Plans for course planning and registration.
    /// </summary>
    public class PreferredSection
    {
        /// <summary>
        /// The student ID.
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// The section ID.
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// The credits planed for registration.
        /// </summary>
        public decimal? Credits { get; set; }

    }
}
