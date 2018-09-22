using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a Credit, such as Institutional, transfer, etc.
    /// </summary>
    public class AcademicProgram
    {
        /// <summary>
        /// Unique code for this Academic Program
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this Academic Program
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Majors associated to the program
        /// </summary>
        public List<string> MajorCodes { get; set; }
        /// <summary>
        /// Minors associated to the program
        /// </summary>
        public List<string> MinorCodes { get; set; }
        /// <summary>
        /// Certificate (CCDs) associated to the program
        /// </summary>
        public List<string> CertificateCodes { get; set; }
        /// <summary>
        /// Specializations associated to the program
        /// </summary>
        public List<string> SpecializationCodes { get; set; }
        /// <summary>
        /// FederalCourseClassification (CIP from Colleague)
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// LocalCourseClassification (LOCAL.GOVT.CODES from Colleague)
        /// </summary>
        public List<string> LocalCourseClassifications { get; set; }
    }
}

