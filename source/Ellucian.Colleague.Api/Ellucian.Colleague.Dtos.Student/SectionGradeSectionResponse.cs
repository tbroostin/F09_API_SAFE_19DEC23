// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Represents a response to importing grades for multiple students in one section
    /// </summary>
    public class SectionGradeSectionResponse
    {

        /// <summary>
        /// List of responses, one per student
        /// </summary>
        public List<SectionGradeResponse> StudentResponses { get; set; }

        /// <summary>
        /// Informational messages returned in relation to the entire section
        /// </summary>
        public List<string> InformationalMessages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionGradeResponse"/> class.
        /// </summary>
        public SectionGradeSectionResponse()
        {
            StudentResponses = new List<SectionGradeResponse>();
            InformationalMessages = new List<string>();
        }
    }
}
