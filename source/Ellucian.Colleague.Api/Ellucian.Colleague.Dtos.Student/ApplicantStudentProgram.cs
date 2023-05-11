// Copyright 2022 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student.Requirements;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A program in which an applicant is enrolled
    /// </summary>
    public class ApplicantStudentProgram
    {
        /// <summary>
        /// Id of the applicant
        /// </summary>
        public string Applicant { get; set; }
        /// <summary>
        /// Code of the program
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// Code of the catalog for this program
        /// </summary>
        public string CatalogCode { get; set; }
        /// <summary>
        /// Date when the program is anticipated to be completed by the student
        /// </summary>
        public string AnticipatedCompletionDate { get; set; }
        /// <summary>
        /// End Date of Student Program (when inactive or graduated).
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Academic End Date of the program.
        /// </summary>
        public DateTime? AcadEndDate { get; set; }
        /// <summary>
        /// Start Date of Student Program
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Academic Start Date
        /// </summary>
        public DateTime? AcadStartDate { get; set; }
        /// <summary>
        /// Academic Level Code from the Program
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// Degree Code from the Program
        /// </summary>
        public string DegreeCode { get; set; }
        /// <summary>
        /// Admit Status from Student Program
        /// </summary>
        public string AdmitStatusCode { get; set; }
        /// <summary>
        /// Determined from Student Program status
        /// </summary>
        public bool HasGraduated { get; set; }
        /// <summary>
        /// Name/Title from Program
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// Department Code from Program
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// Location from Program
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Student program status processing code
        /// </summary>
        public StudentProgramStatusProcessingType ProgramStatusProcessingCode { get; set; }
        /// <summary>
        /// Major object from the Program and Additional Requirements.
        /// </summary>
        public IEnumerable<StudentMajors> StudentProgramMajors { get; set; }
        /// <summary>
        /// Minor object from the Program and Additional Requirements
        /// </summary> 
        public IEnumerable<StudentMinors> StudentProgramMinors { get; set; }
        /// <summary>
        /// List of  <see cref="AdditionalRequirement">additional requirements</see> specific to this student to complete this program
        /// </summary>
        public IEnumerable<AdditionalRequirement2> AdditionalRequirements { get; set; }
    }
}
