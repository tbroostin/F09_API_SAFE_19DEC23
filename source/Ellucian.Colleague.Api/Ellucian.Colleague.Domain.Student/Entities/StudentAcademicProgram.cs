// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentAcademicProgram
    {
        #region Required Fields

        /// <summary>
        /// Academic Program Code
        /// </summary>
        public string ProgramCode { get ;  private set; } 

        /// <summary>
        /// Code representing the Catalog Year
        /// </summary>
        public string CatalogCode { get ; private set ; } 

         /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get ; private set; } 

        /// <summary>
        /// Academic Program Start Date
        /// </summary>
        public DateTime? StartDate { get; private set; }

        /// <summary>
        /// Academic Program status
        /// </summary>
        public string Status { get; private set; }

        #endregion

        #region Optional Fields

        /// <summary>
        /// Guid
        /// </summary>        
        public string Guid { get; private set; }
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
        /// The department code associated to this program
        /// </summary>
        public string DepartmentCode { get; set; }
        /// <summary>
        /// The location associated to this program
        /// </summary>
        public string Location { get; set; }
         /// <summary>
        /// The Degree Code associated to this Academic Program
        /// </summary>
        public string DegreeCode { get; set; }
        /// A List of Student Majors coming from the program or from additional requirements.
        /// </summary>
        public List<string> StudentProgramMajors { get; private set; }

        /// A Tuple collection of Student Majors coming from the program or from additional requirements.
        /// </summary>
        public List<Tuple<string, DateTime?, DateTime?>> StudentProgramMajorsTuple { get; private set; }

        /// <summary>
        /// A List of Student Minors coming from the program or from additional requirements.
        /// </summary>
        public List<string> StudentProgramMinors { get; private set; }

        /// A Tuple collection of Student Minors coming from the program or from additional requirements.
        /// </summary>
        public List<Tuple<string, DateTime?, DateTime?>> StudentProgramMinorsTuple { get; private set; }


        /// A List of Student specializations coming from the program or from additional requirements.
        /// </summary>
        public List<string> StudentProgramSpecializations { get; private set; }

        /// A Tuple collection of Student specializations coming from the program or from additional requirements.
        /// </summary>
        public List<Tuple<string, DateTime?, DateTime?>> StudentProgramSpecializationsTuple { get; private set; }

        /// <summary>
        /// A List of Student CCDs coming from the program or from additional requirements.
        /// </summary>
        public List<string> StudentProgramCcds { get; private set; }

        /// A Tuple collection of Student CCDs coming from the program or from additional requirements.
        /// </summary>
        public List<Tuple<string, DateTime?, DateTime?>> StudentProgramCCDsTuple { get; private set; }


        /// <summary>
        /// The start term associated to this program
        /// </summary>
        public string StartTerm { get; set; }
        /// <summary>
        /// The academic period in which the student is expected to graduate
        /// </summary>
        public string AnticipatedCompletionTerm { get; set; }
        /// <summary>
        /// The academic period in which the student actually graduated.
        /// </summary>
        public string GradTerm { get; set; }
        /// <summary>
        /// A measurement of the student's overall performance in the program (e.g. GPA).
        /// </summary>
        public decimal? GradGPA { get; set; }
        /// A List of Student honors coming from Academic Credentials.
        /// </summary>
        public List<string> StudentProgramHonors { get; private set; }
        /// <summary>
        /// graduation date from Academic Credentials
        /// </summary>
        public DateTime? GraduationDate { get; set; }
         /// <summary>
        /// Academic Credentials Date
        /// </summary>
        public DateTime? CredentialsDate { get; set; }
        /// <summary>
        /// The thesis title from Academic Credentials
        /// </summary>
        public string ThesisTitle { get; set; }
        /// <summary>
        /// The number of credits earned
        /// </summary>
        public decimal? CreditsEarned { get; set; }

        /// <summary>
        /// Indicates the primary academic program of the student.  Only one academic program should be set to preferred for a student.
        /// </summary>
        public bool IsPrimary { get; set; }
        public string AdmitStatus { get; set; }
        public CurriculumObjectiveCategory CurriculumObjective { get; set; }


        #endregion

        #region Constructor

        /// <summary>
        /// Initialize the StudentProgram Method
        /// </summary>
        /// <param name="personId">Student Id for this program</param>
        /// <param name="programCode">The Academic Program Code</param>
        /// <param name="catalogCode">The Catalog Code which is typically the catalog Year</param>
        public StudentAcademicProgram(string personId, string programCode, string catalogCode, string guid, DateTime startDate, string status)
        {
            if (string.IsNullOrEmpty(personId)) 
            {
                throw new ArgumentException(string.Concat("PersonId is required. Entity: 'STUDENT.PROGRAMS', Record ID: '", guid , "'"));
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException(string.Concat("Academic Program is required. Entity: 'STUDENT.PROGRAMS', Record ID: '", guid, "'"));
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException(string.Concat("Enrollment status is required. Entity: 'STUDENT.PROGRAMS', Record ID: '", personId,  "*" , programCode, "'"));
            }

            StudentId = personId;
            ProgramCode = programCode;
            CatalogCode = catalogCode;
            Guid = guid;
            StartDate = startDate;
            Status = status;
            StudentProgramMajors = new List<string>();
            StudentProgramMinors = new List<string>();
            StudentProgramSpecializations = new List<string>();
            StudentProgramCcds = new List<string>();
            StudentProgramHonors = new List<string>();
        }

        /// <summary>
        /// Initialize the StudentProgram Method
        /// </summary>
        /// <param name="personId">Student Id for this program</param>
        /// <param name="programCode">The Academic Program Code</param>
        /// <param name="catalogCode">The Catalog Code which is typically the catalog Year</param>
        public StudentAcademicProgram(string personId, string programCode, string catalogCode, DateTime startDate)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException(string.Concat("PersonId is required. Entity: 'STUDENT.PROGRAMS', person ID: '", personId, "'"));
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentException(string.Concat("Academic Program is required. Entity: 'STUDENT.PROGRAMS', person ID: '", personId, "'"));
            }

            StudentId = personId;
            ProgramCode = programCode;
            CatalogCode = catalogCode;
            StartDate = startDate;
            StudentProgramMajors = new List<string>();
            StudentProgramMinors = new List<string>();
            StudentProgramSpecializations = new List<string>();
            StudentProgramCcds = new List<string>();
            StudentProgramHonors = new List<string>();
        }

        #endregion
        #region Methods


        /// <summary>
        /// Add Student Majors into the Student Program
        /// </summary>
        /// <param name="majors">Other Majors Object</param>
        public void AddMajors(string majors)
        {
            if (majors == null)
            {
                throw new ArgumentNullException("majors");
            }
            else if (!StudentProgramMajors.Contains(majors))
            {
                   StudentProgramMajors.Add(majors);
            }

        }

        /// <summary>
        /// Add Student Majors into the Student Program
        /// </summary>
        /// <param name="majors">Other Majors Object</param>
        /// <param name="startOn">Start on date</param>
        /// <param name="endOn">end on date</param>
        public void AddMajors(string majors, DateTime? startOn, DateTime? endOn)
        {
            if (majors == null)
            {
                throw new ArgumentNullException("majors");
            }
            var majorTuple = new Tuple<string, DateTime?, DateTime?>(majors, startOn, endOn);
            if (StudentProgramMajorsTuple == null)
            {
                StudentProgramMajorsTuple = new List<Tuple<string, DateTime?, DateTime?>>();
                StudentProgramMajorsTuple.Add(majorTuple);
            }
            else if (!StudentProgramMajorsTuple.Contains(majorTuple))
            {
                StudentProgramMajorsTuple.Add(majorTuple);
            }

        }
        /// <summary>
        /// Add Student Minors into the Student Program
        /// </summary>
        /// <param name="minors">Other Minors Object</param>
        public void AddMinors(string minors)
        {
            if (minors == null)
            {
                throw new ArgumentNullException("minors");
            }
            else if (!StudentProgramMinors.Contains(minors))
            {
                StudentProgramMinors.Add(minors);
            }
        }

        /// <summary>
        /// Add Student Minors into the Student Program
        /// </summary>
        /// <param name="minors">Other Minors Object</param>
        /// <param name="startOn">Start on date</param>
        /// <param name="endOn">end on date</param>
        public void AddMinors(string minors, DateTime? startOn, DateTime? endOn)
        {
            if (minors == null)
            {
                throw new ArgumentNullException("minors");
            }
            var minorTuple = new Tuple<string, DateTime?, DateTime?>(minors, startOn, endOn);
            if (StudentProgramMinorsTuple == null)
            {
                StudentProgramMinorsTuple = new List<Tuple<string, DateTime?, DateTime?>>();
                StudentProgramMinorsTuple.Add(minorTuple);
            }
            else if (!StudentProgramMinorsTuple.Contains(minorTuple))
            {
                StudentProgramMinorsTuple.Add(minorTuple);
            }

        }
        /// <summary>
        /// Add Student Ccds into the Student Program
        /// </summary>
        /// <param name="ccds">Other CCDS Object</param>
        public void AddCcds(string ccds)
        {
            if (ccds == null)
            {
                throw new ArgumentNullException("ccds");
            }
            else if (!StudentProgramCcds.Contains(ccds))
            {
                StudentProgramCcds.Add(ccds);
            }

        }

        /// <summary>
        /// Add Student CCDs into the Student Program
        /// </summary>
        /// <param name="ccds">CCDs</param>
        /// <param name="startOn">Start on date</param>
        /// <param name="endOn">end on date</param>
        public void AddCcds(string ccds, DateTime? startOn, DateTime? endOn)
        {
            if (ccds == null)
            {
                throw new ArgumentNullException("ccds");
            }
            var ccdTuple = new Tuple<string, DateTime?, DateTime?>(ccds, startOn, endOn);
            if (StudentProgramCCDsTuple == null)
            {
                StudentProgramCCDsTuple = new List<Tuple<string, DateTime?, DateTime?>>();
                StudentProgramCCDsTuple.Add(ccdTuple);
            }
            else if (!StudentProgramCCDsTuple.Contains(ccdTuple))
                StudentProgramCCDsTuple.Add(ccdTuple);

        }

        /// <summary>
        /// Add Student specializations into the Student Program
        /// </summary>
        /// <param name="specialization">OtherSpecialization Object</param>
        public void AddSpecializations(string specialization)
        {
            if (specialization == null)
            {
                throw new ArgumentNullException("specializations");
            }
            else if (!StudentProgramSpecializations.Contains(specialization))
            {
                StudentProgramSpecializations.Add(specialization);
            }
        }

        /// <summary>
        /// Add Student Specializations into the Student Program
        /// </summary>
        /// <param name="specializations">specializations</param>
        /// <param name="startOn">Start on date</param>
        /// <param name="endOn">end on date</param>
        public void AddSpecializations(string specializations, DateTime? startOn, DateTime? endOn)
        {
            if (specializations == null)
            {
                throw new ArgumentNullException("specializations");
            }
            var specializationTuple = new Tuple<string, DateTime?, DateTime?>(specializations, startOn, endOn);
            if (StudentProgramSpecializationsTuple == null)
            {
                StudentProgramSpecializationsTuple = new List<Tuple<string, DateTime?, DateTime?>>();
                StudentProgramSpecializationsTuple.Add(specializationTuple);
            }
            else if (!StudentProgramSpecializationsTuple.Contains(specializationTuple))
            {
                StudentProgramSpecializationsTuple.Add(specializationTuple);
            }
        }
        /// <summary>
        /// Add Other Honors
        /// </summary>
        /// <param name="honors">OtherHonor Object</param>
        public void AddHonors(string honors)
        {
            if (honors == null)
            {
                throw new ArgumentNullException("majors");
            }
            else if (!StudentProgramHonors.Contains(honors))
            {
                StudentProgramHonors.Add(honors);
            }

        }
        
        #endregion

       
    }
}
