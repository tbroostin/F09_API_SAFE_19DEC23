using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Requisites related to courses or sections. For example, when taking BIOL-100
    /// you are required to take BIOL-101L concurrently. 
    /// </summary>
    public class SectionRequisite
    {
        /// <summary>
        /// Optional: Used by sections with corequisite sections. No requirement code will be supplied when
        /// corequisite sections are present.
        /// </summary>
        public IEnumerable<string> CorequisiteSectionIds { get; set; }

        /// <summary>
        /// Optional: For sections with corequisite sections, specifies the number of listed sections needed
        /// to satisfy the requisite. This value will be supplied only in conjunction with CorequisiteSectionIds.
        /// </summary>
        public int NumberNeeded { get; set; }

        /// <summary>
        /// Indicates whether the requisite is required or just recommended.
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
