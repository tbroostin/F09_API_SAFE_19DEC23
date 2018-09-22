// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Student : Person
    {
        #region private members
        private readonly string _StudentGuid;
        private readonly int? _DegreePlanId;
        private readonly List<string> _ProgramIds;
        private readonly List<string> _AcademicCreditIds;
        private readonly List<string> _StudentRestrictionIds = new List<string>();
        private readonly List<string> _RegistrationPriorityIds = new List<string>();
        private readonly List<Advisement> _Advisements = new List<Advisement>();
       

        #endregion

        #region properties

        /// <summary>
        /// Gets the student's GUID
        /// </summary>
        public string StudentGuid { get { return this._StudentGuid; } }

        /// <summary>
        /// Gets the student's Degree Plan Id if the student has a plan or null.
        /// </summary>
        public int? DegreePlanId { get { return this._DegreePlanId; } }

        /// <summary>
        /// Gets a list of the student's Academic Program Ids.
        /// </summary>
        public List<string> ProgramIds { get { return this._ProgramIds; } }

        /// <summary>
        /// Gets a list of the student's Academic Credit Ids.
        /// </summary>
        public List<string> AcademicCreditIds { get { return this._AcademicCreditIds; } }

        /// <summary>
        /// Gets a list of the student's StudentRestriction Ids;
        /// </summary>
        public List<string> StudentRestrictionIds { get { return this._StudentRestrictionIds; } }

        /// <summary>
        /// Id numbers of the active advisors who are currently assigned to this student (if applicable).
        /// Former advisors (those with an end date prior to today) are not included in this list.
        /// </summary>
        public ReadOnlyCollection<string> AdvisorIds
        {
            get
            {
                return _Advisements.Select(a => a.AdvisorId).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Student's stated educational goal, useful for advising. i.e. BA degree, Certification, New Career (institutionally defined)
        /// </summary>
        public string EducationalGoal { get; set; }

        /// <summary>
        /// Gets a list of student's RegistrationPriorityIds
        /// </summary>
        public ReadOnlyCollection<string> RegistrationPriorityIds { get; private set; }

        /// <summary>
        /// Flag to identify if the student has an active advisor assigned
        /// </summary>
        public bool HasAdvisor
        {
            get
            {
                if (Advisements.Count() > 0)
                {
                    return true;
                }
                return false;
            }
        }
        /// Added Fields for ESS project (SRM - 11/01/2013)
        /// <summary>
        /// If Parents attended this university, then True else False
        /// </summary>
        public bool IsLegacyStudent { get; set; }
        /// <summary>
        /// Flag to say if this student is the first generation to attend college
        /// </summary>
        public bool? IsFirstGenerationStudent { get; set; }
        /// <summary>
        /// Flag to show if this is an International Student
        /// </summary>
        public bool IsInternationalStudent { get; set; }
        /// <summary>
        /// List of start terms for each Academic Level(s) of a student
        /// </summary>
        public List<string> AdmitTerms { get; set; }
        /// <summary>
        /// List of Academic Levels for the student
        /// </summary>
        public List<string> AcademicLevelCodes { get; set; }
        /// <summary>
        /// The Residency status code
        /// </summary>
        public string ResidencyStatus { get; set; }
        /// <summary>
        /// List of student residencies and dates
        /// </summary>
        public List<StudentResidency> StudentResidencies { get; set; }
        /// <summary>
        /// Dictionary containing High School attended and GPA
        /// </summary>
        public List<HighSchoolGpa> HighSchoolGpas;
        /// <summary>
        /// List of student home locations and start/end date
        /// </summary>
        public List<StudentHomeLocation> StudentHomeLocations { get; set; }
        /// <summary>
        /// List of Active Advisements for the student. Former advisements are not included in this list.
        /// </summary>
        public ReadOnlyCollection<Advisement> Advisements { get; set; }
        /// <summary>
        /// Student Type Code
        /// </summary>
        public string StudentTypeCode { get; set; }
        /// <summary>
        /// Class Level Codes for each academic level
        /// </summary>
        public List<string> ClassLevelCodes { get; set; }
        /// <summary>
        /// Flag to indicate if the student record is confidential
        /// </summary>
        public bool IsConfidential { get; set; }
        /// <summary>
        /// Flag to indicate if the student is a transfer student
        /// </summary>
        public bool IsTransfer { get; set; }
        /// <summary>
        /// Collection of student academic levels
        /// </summary>
        public List<StudentAcademicLevel> StudentAcademicLevels { get; set; }
        /// <summary>
        /// Id of the student's Financial Aid counselor. If empty or null, the student has not been
        /// assigned a counselor yet.
        /// </summary>
        public string FinancialAidCounselorId { get; set; }

        /// <summary>
        /// List of student type info
        /// </summary>
        public List<StudentTypeInfo> StudentTypeInfo { get; set; }
        public Dictionary<string, string> PerformanceMeasures { get; set; }

        #endregion

        /// <summary>
        /// Create a Student domain object
        /// </summary>
        /// <param name="id">Student's ID</param>
        /// <param name="lastName">Student's last name</param>
        /// <param name="degreePlanId">Degree plan ID</param>
        /// <param name="programIds">List of program IDs</param>
        /// <param name="academicCreditIds">List of academic credit IDs</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public Student(string id, string lastName, int? degreePlanId, List<string> programIds, List<string> academicCreditIds, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            if (degreePlanId.HasValue)
            {
                if (degreePlanId.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException("id", degreePlanId.Value, "id may only be null or a positive number");
                }
            }

            this._DegreePlanId = degreePlanId;
            this._ProgramIds = programIds;
            this._AcademicCreditIds = academicCreditIds;
            HighSchoolGpas = new List<HighSchoolGpa>();
            StudentHomeLocations = new List<StudentHomeLocation>();
            Advisements = _Advisements.AsReadOnly();
            RegistrationPriorityIds = _RegistrationPriorityIds.AsReadOnly();
            StudentAcademicLevels = new List<StudentAcademicLevel>();
            StudentResidencies = new List<StudentResidency>();
        }

        /// <summary>
        /// Create a Student domain object
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="id">Student's ID</param>
        /// <param name="lastName">Student's last name</param>
        /// <param name="degreePlanId">Degree plan ID</param>
        /// <param name="programIds">List of program IDs</param>
        /// <param name="academicCreditIds"></param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public Student(string studentGuid, string id, string lastName, int? degreePlanId, List<string> programIds, List<string> academicCreditIds, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            if (degreePlanId <= 0)
            {
                throw new ArgumentOutOfRangeException("id", degreePlanId.Value, "id may only be null or a positive number");
            }

            this._DegreePlanId = degreePlanId;
            this._ProgramIds = programIds;
            this._AcademicCreditIds = academicCreditIds;
            this._StudentGuid = studentGuid;
            HighSchoolGpas = new List<HighSchoolGpa>();
            StudentHomeLocations = new List<StudentHomeLocation>();
            Advisements = _Advisements.AsReadOnly();
            RegistrationPriorityIds = _RegistrationPriorityIds.AsReadOnly();
            StudentAcademicLevels = new List<StudentAcademicLevel>();
            StudentResidencies = new List<StudentResidency>();
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Create a Student domain object with data needed for Data Model
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">Student's ID</param>
        /// <param name="lastName">Student's last name</param>
        /// <param name="programIds">List of program IDs</param>
        /// <param name="academicCreditIds"></param>
        /// <param name="getPrograms"></param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public Student(string guid, string id, List<string> programIds, List<string> academicCreditIds, string lastName, bool getPrograms = false, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            _StudentGuid = guid;
            _ProgramIds = programIds;
            _AcademicCreditIds = academicCreditIds;          
            StudentAcademicLevels = new List<StudentAcademicLevel>();
            StudentResidencies = new List<StudentResidency>();
            StudentResidencies = new List<StudentResidency>();
        }


        public void AddHighSchoolGpa(string highSchoolId, decimal? highSchoolGpa, string lastAttendYear)
        {
            if (string.IsNullOrEmpty(highSchoolId))
            {
                throw new ArgumentNullException("highSchoolId", "High School Id must be specified");
            }
            if (HighSchoolGpas.Where(h => h.HighSchoolId.Equals(highSchoolId)).Count() == 0)
            {
                HighSchoolGpa highSchool = new HighSchoolGpa(highSchoolId, highSchoolGpa, lastAttendYear);
                HighSchoolGpas.Add(highSchool);
            }
        }

        public void AddResidency(string residency, DateTime? date)
        {
            if (string.IsNullOrEmpty(residency))
            {
                throw new ArgumentNullException("residency", "Residency must be specified");
            }
            if (StudentResidencies.Where(r => r.Residency.Equals(residency)).Count() == 0)
            {
                StudentResidency Residency = new StudentResidency(residency, date);
                StudentResidencies.Add(Residency);
            }
        }

        public void AddLocation(string location, DateTime? startDate, DateTime? endDate, bool isPrimary)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException("location", "Home location must be specified");
            }
            if (StudentHomeLocations.Where(h => h.Location.Equals(location)).Count() == 0)
            {
                StudentHomeLocation Location = new StudentHomeLocation(location, startDate, endDate, isPrimary);
                StudentHomeLocations.Add(Location);
            }
        }

        public void AddAdvisement(string advisorId, DateTime? startDate, DateTime? endDate, string advisorType)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "Advisor Id must be specified");
            }
            // Like Colleague, disallow the same advisor for the same advisor type.
            if (Advisements.Where(a => (a.AdvisorId == advisorId && a.AdvisorType == advisorType)).Count() == 0)
            {
                Advisement advisor = new Advisement(advisorId, startDate) { AdvisorType = advisorType, EndDate = endDate };
                _Advisements.Add(advisor);
            }
        }

        public void AddAdvisor(string advisorId)
        {
            return;
        }

        public void AddStudentRestriction(string studentRestrictionId)
        {
            if (string.IsNullOrEmpty(studentRestrictionId))
            {
                throw new ArgumentNullException("studentRestrictionId", "Student Restriction ID must be specified");
            }
            if (StudentRestrictionIds.Where(r => r.Equals(studentRestrictionId)).Count() == 0)
            {
                _StudentRestrictionIds.Add(studentRestrictionId);
            }
        }

        public void AddRegistrationPriority(string registrationPriorityId)
        {
            if (string.IsNullOrEmpty(registrationPriorityId))
            {
                throw new ArgumentNullException("registrationPriorityId", "Registration Priority ID must be specified");
            }
            if (RegistrationPriorityIds.Where(r => r.Equals(registrationPriorityId)).Count() == 0)
            {
                _RegistrationPriorityIds.Add(registrationPriorityId);
            }
        }

        /// <summary>
        /// Convert a Student object to a StudentAccess object. If incoming student is null,
        /// return a null object (that's why constructor could not be used.
        /// </summary>
        /// <param name="student"></param>
        public StudentAccess ConvertToStudentAccess()
        {
            if (this != null)
            {
                var studentAccess = new StudentAccess(Id);
                if (Advisements != null)
                {
                    foreach (var advisement in Advisements)
                    {
                        studentAccess.AddAdvisement(advisement.AdvisorId, advisement.StartDate, advisement.EndDate, advisement.AdvisorType);
                    }
                }
                return studentAccess;
            }
            return null;
        }
        /// <summary>
        /// Determines a student's primary location based on their home locations date ranges and their active student programs
        /// This routine does not make use the StudentHomeLocation "IsPrimary" attribute because that is only set when a term is provided in building the student.
        /// </summary>
        /// <param name="studentPrograms">List of the student's programs to evaluate for primary location</param>
        /// <returns>The student's primary location code</returns>
        public string GetPrimaryLocation(IEnumerable<StudentProgram> studentPrograms)
        {
            var studentHomeLocation = StudentHomeLocations != null ? StudentHomeLocations.Where(x => x.StartDate.HasValue && x.StartDate != default(DateTime) && DateTime.Today >= x.StartDate && ((x.EndDate != null && x.EndDate != default(DateTime) && DateTime.Today < x.EndDate) || (x.EndDate == null))).FirstOrDefault() : null;
            if (studentHomeLocation != null) { return studentHomeLocation.Location; }
            // If no location found check the active student programs for a location.
            if (studentPrograms != null && studentPrograms.Any())
            {
                // Use the location code from the student's first active program                
                var activeStudentProgram = studentPrograms != null ? studentPrograms.Where(x => x.StartDate.HasValue && x.StartDate != default(DateTime) && DateTime.Today >= x.StartDate && ((x.EndDate != null && x.EndDate != default(DateTime) && DateTime.Today < x.EndDate) || (x.EndDate == null))).FirstOrDefault() : null;
                if (activeStudentProgram != null) { return activeStudentProgram.Location; }
            }

            return null;
        }

        #region public overrides

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Student))
            {
                return false;
            }
            return (obj as Student).Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion
    }
}
