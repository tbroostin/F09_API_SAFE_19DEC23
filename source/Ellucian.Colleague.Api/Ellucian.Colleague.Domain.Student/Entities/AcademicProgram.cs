using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Code Item of Academic Programs
    /// </summary>
    [Serializable]
    public class AcademicProgram : GuidCodeItem
    {
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
        /// Degree associated to the academic program
        /// </summary>
        public string DegreeCode { get; set; }
        /// <summary>
        /// Honor Code associated to academic program
        /// </summary>
        public string HonorCode { get; set; }
        /// <summary>
        /// Academic Level Code for academic program
        /// </summary>
        public string AcadLevelCode { get; set; }
        /// <summary>
        /// Start date of academic program
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date of academic program
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Description of Academic Program
        /// </summary>
        public string LongDescription { get; set; }
        /// <summary>
        /// Location of Academic Program
        /// </summary>
        public List<string> Location { get; set; }
        /// <summary>
        /// Institution that authorizes Academic Program
        /// </summary>
        public List<string> AuthorizingInstitute { get; set; }
        /// <summary>
        /// List of departments associated to the Academic Program
        /// </summary>
        public List<string> DeptartmentCodes { get; set; }
        // <summary>
        /// List of approval agencies associated to the Academic Program
        /// </summary>
        public List<string> ApprovalAgencies { get; set; }
        /// <summary>
        /// FederalCourseClassification (CIP from Colleague)
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// LocalCourseClassification (LOCAL.GOVT.CODES from Colleague)
        /// </summary>
        public List<string> LocalCourseClassifications { get; set; }
        /// <summary>
        /// Academic Program constructor
        /// </summary>
        /// <param name="guid">Record GUID key</param>
        /// <param name="code">Original Key or Code to program</param>
        /// <param name="desc">Academic Program Title</param>
        public AcademicProgram(string guid, string code, string desc)
            : base(guid, code, desc)
        {
            MajorCodes = new List<string>();
            MinorCodes = new List<string>();
            CertificateCodes = new List<string>();
            SpecializationCodes = new List<string>();
            LocalCourseClassifications = new List<string>();
            DeptartmentCodes = new List<string>();
        }

        public List<string> AddnlCcds { get; set; }
        public List<string> AddnlMajors { get; set; }
        public List<string> AddnlMinors { get; set; }
        public List<string> AddnlSpecializations { get; set; }
    }
}
