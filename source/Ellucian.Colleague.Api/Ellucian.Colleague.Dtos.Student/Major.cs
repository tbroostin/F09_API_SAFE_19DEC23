using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a major describing an area of study (e.g., Math, Art History)
    /// </summary>
    public class Major
    {
        /// <summary>
        /// Unique code for this major
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this major
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Division Code assigned to this Major
        /// </summary>
        public string DivisionCode { get; set; }
        /// <summary>
        /// Boolean Flag for active Majors
        /// </summary>
        public bool ActiveFlag { get; set; }
        /// <summary>
        /// Federal Course Classification assigned to this Major
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// List of Local Course Classifications assigned to this Major
        /// </summary>
        public List<string> LocalCourseClassifications { get; set; }
    }
}
