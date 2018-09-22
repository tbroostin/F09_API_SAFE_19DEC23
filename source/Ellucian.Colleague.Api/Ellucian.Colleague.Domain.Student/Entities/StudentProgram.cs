// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentProgram
    {
        private readonly string _ProgramCode;
        private readonly string _CatalogCode;
        private readonly string _StudentId;
        private readonly string _guid;
        private readonly List<StudentMajors> _studentProgramMajors = new List<StudentMajors>();
        private readonly List<StudentMinors> _studentProgramMinors = new List<StudentMinors>();
        private readonly List<AdditionalRequirement> _AdditionalRequirements = new List<AdditionalRequirement>();
        private readonly List<RequirementModification> _RequirementModifications = new List<RequirementModification>();
        private readonly List<Override> _Overrides = new List<Override>();

        /// <summary>
        /// Guid
        /// </summary>        
        public string Guid { get { return _guid; } }
        
        /// <summary>
        /// Academic Program Code
        /// </summary>
        public string ProgramCode { get { return _ProgramCode; } }
        /// <summary>
        /// Code representing the Catalog Year
        /// </summary>
        public string CatalogCode { get { return _CatalogCode; } }
        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        /// <summary>
        /// Program Start Date
        /// </summary>
        public DateTime? StartDate { get; set; }
        // Added for ESS project
        // srm - 04/08/2014
        /// <summary>
        /// Anticipated Completion date of the Academic Program
        /// </summary>
        public DateTime? AnticipatedCompletionDate { get; set; }
        /// <summary>
        /// End Date of the program.  Used in conjunction with Status.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The Academic Level associated to the program.
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// The Degree Code associated to this Academic Program
        /// </summary>
        public string DegreeCode { get; set; }
        /// <summary>
        /// The Admit Status of the student within this academic program
        /// </summary>
        public string AdmitStatusCode { get; set; }
        /// <summary>
        /// If the student has graduated from this program then this is true.
        /// </summary>
        public bool HasGraduated { get; set; }
        /// <summary>
        /// This is the actual title of the program
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// This is the status of the program
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// This is the HEDM status of the program
        /// </summary>
        public string HedmStatus { get; set; }
        /// <summary>
        /// The department code associated to this program
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// The location associated to this program
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// The location associated to this program
        /// </summary>
        public string StartTerm { get; set; }
        /// <summary>
        /// Student program status processing code
        /// </summary>
        public StudentProgramStatusProcessingType ProgramStatusProcessingCode {get;set;}
        /// <summary>
        /// A List of Student Majors coming from the program or from additional requirements.
        /// </summary>
        public ReadOnlyCollection<StudentMajors> StudentProgramMajors { get; private set; }
        /// <summary>
        /// A List of Student Minors coming from the program or from additional requirements.
        /// </summary>
        public ReadOnlyCollection<StudentMinors> StudentProgramMinors { get; private set; }
        /// <summary>
        /// Additional Requirements and Majors/Minors/etc.
        /// </summary>
        public ReadOnlyCollection<AdditionalRequirement> AdditionalRequirements { get; private set; }
        /// <summary>
        /// A List of Overrides to the academic program
        /// </summary>
        public ReadOnlyCollection<Override> Overrides { get; private set; }
        /// <summary>
        /// A list of Degree Requirements Modifications for this student in this program
        /// </summary>
        public ReadOnlyCollection<RequirementModification> RequirementModifications { get; private set; }


        public List<StudentProgramStatus> StudentProgramStatuses { get; set; }

        /// <summary>
        /// Initialize the StudentProgram Method
        /// </summary>
        /// <param name="personId">Student Id for this program</param>
        /// <param name="programCode">The Academic Program Code</param>
        /// <param name="catalogCode">The Catalog Code which is typically the catalog Year</param>
        public StudentProgram(string personId, string programCode, string catalogCode)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (programCode == null)
            {
                throw new ArgumentNullException("programCode");
            }
            if (catalogCode == null)
            {
                throw new ArgumentNullException("catalogCode");
            }

            _StudentId = personId;
            _ProgramCode = programCode;
            _CatalogCode = catalogCode;
            StudentProgramMajors = _studentProgramMajors.AsReadOnly();
            StudentProgramMinors = _studentProgramMinors.AsReadOnly();
            AdditionalRequirements = _AdditionalRequirements.AsReadOnly();
            Overrides = _Overrides.AsReadOnly();
            RequirementModifications = _RequirementModifications.AsReadOnly();
        }

        /// <summary>
        /// Initialize the StudentProgram Method
        /// </summary>
        /// <param name="personId">Student Id for this program</param>
        /// <param name="programCode">The Academic Program Code</param>
        /// <param name="catalogCode">The Catalog Code which is typically the catalog Year</param>
        public StudentProgram(string personId, string programCode, string catalogCode, string guid )
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (programCode == null)
            {
                throw new ArgumentNullException("programCode");
            }
            if (catalogCode == null)
            {
                throw new ArgumentNullException("catalogCode");
            }

            _StudentId = personId;
            _ProgramCode = programCode;
            _CatalogCode = catalogCode;
            _guid = guid;
            StudentProgramMajors = _studentProgramMajors.AsReadOnly();
            StudentProgramMinors = _studentProgramMinors.AsReadOnly();
            AdditionalRequirements = _AdditionalRequirements.AsReadOnly();
            Overrides = _Overrides.AsReadOnly();
            RequirementModifications = _RequirementModifications.AsReadOnly();
        }
        /// <summary>
        /// Add Additional Requirements Objects into the Student Program
        /// </summary>
        /// <param name="addlRequirement">Additional Requirements Object</param>
        public void AddAddlRequirement(AdditionalRequirement addlRequirement)
        {
            if (addlRequirement == null)
            {
                throw new ArgumentNullException("addlRequirement");
            }
            _AdditionalRequirements.Add(addlRequirement);
        }
        /// <summary>
        /// Add Override Objects into the Student Program
        /// </summary>
        /// <param name="overrride">Override Object</param>
        public void AddOverride(Override overrride)
        {
            if (overrride == null)
            {
                throw new ArgumentNullException("overrride");
            }
            _Overrides.Add(overrride);
        }
        /// <summary>
        /// Add Requirement Modifications into the Student Program
        /// </summary>
        /// <param name="requirementModification">Requirement Modifications Object</param>
        public void AddRequirementModification(RequirementModification requirementModification)
        {
            if (requirementModification == null)
            {
                throw new ArgumentNullException("requirementModification");
            }
            _RequirementModifications.Add(requirementModification);
        }
        /// <summary>
        /// Add Student Majors into the Student Program
        /// </summary>
        /// <param name="majors">Majors Object</param>
        public void AddMajors(StudentMajors majors)
        {
            if (majors == null)
            {
                throw new ArgumentNullException("majors");
            }
            bool inMajors = false;
            foreach (var programMajor in _studentProgramMajors)
            {
                if (programMajor.Code == majors.Code) 
                {
                    inMajors = true; 
                }
            }
            if (inMajors == false)
            {
                _studentProgramMajors.Add(majors);
            }
        }
        /// <summary>
        /// Add Student Minors into the Student Program
        /// </summary>
        /// <param name="minors">Minors Object</param>
        public void AddMinors(StudentMinors minors)
        {
            if (minors == null)
            {
                throw new ArgumentNullException("minors");
            }
            bool inMinors = false;
            foreach (var programMinor in _studentProgramMinors)
            {
                if (programMinor.Code == minors.Code)
                {
                    inMinors = true;
                }
            }
            if (inMinors == false)
            {
                _studentProgramMinors.Add(minors);
            }
        }
    }
}
