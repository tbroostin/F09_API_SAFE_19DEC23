using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the Education History DTO for High Schools and Colleges
    /// attended at other institutions.
    /// </summary>
    public class EducationHistory
    {
        /// <summary>
        /// Person or Student Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// High School information
        /// </summary>
        public List<HighSchool> HighSchools { get; set; }
        /// <summary>
        /// College information
        /// </summary>
        public List<College> Colleges { get; set; }
    }
}
