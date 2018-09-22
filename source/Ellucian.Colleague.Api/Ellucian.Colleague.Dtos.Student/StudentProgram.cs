using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student.Requirements;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A program in which a student is enrolled
    /// </summary>
    public class StudentProgram
    {
        /// <summary>
        /// Id of the student
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Code of the program
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// Code of the catalog for this program
        /// </summary>
        public string CatalogCode { get; set; }
        /// <summary>
        /// List of  <see cref="AdditionalRequirement">additional requirements</see> specific to this student to complete this program
        /// </summary>
        public IEnumerable<AdditionalRequirement> AdditionalRequirements { get; set; }
    }
}
