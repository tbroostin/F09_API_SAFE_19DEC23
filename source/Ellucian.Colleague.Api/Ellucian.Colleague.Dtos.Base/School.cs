using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An institutionally-defined School
    /// </summary>
    public class School
    {
        /// <summary>
        /// Unique ID for this School
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of School
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// List of Locations for School
        /// </summary>
        public IEnumerable<string> LocationCodes { get; set; }
        /// <summary>
        /// Divisions within this School
        /// </summary>
        public IEnumerable<string> DivisionCodes { get; set; }
        /// <summary>
        /// Academic Level for this School
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// Departments within this School
        /// </summary>
        public IEnumerable<string> DepartmentCodes { get; set; }
        /// <summary>
        /// Institution ID for this School if applicable
        /// </summary>
        public string InstitutionId { get; set; }
    }
}
