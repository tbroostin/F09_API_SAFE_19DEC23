// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionPermission
    {
        /// <summary>
        /// The id of the section 
        /// </summary>
        public string SectionId { get { return _sectionId; } }
        private string _sectionId;
        /// <summary>
        /// The student petitions related to this section.
        /// </summary>
        public ReadOnlyCollection<StudentPetition> StudentPetitions { get; private set; }
        private readonly List<StudentPetition> _studentPetitions = new List<StudentPetition>();
        /// <summary>
        /// The faculty consents related to this section.
        /// </summary>
        public ReadOnlyCollection<StudentPetition> FacultyConsents { get; private set; }
        private readonly List<StudentPetition> _facultyConsents = new List<StudentPetition>();
        /// <summary>
        /// Constructor for the SectionPermission
        /// </summary>
        /// <param name="sectionId">Id of the section</param>
        public SectionPermission(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Must provide the section Id.");
            }
            _sectionId = sectionId;
            StudentPetitions = _studentPetitions.AsReadOnly();
            FacultyConsents = _facultyConsents.AsReadOnly();
        }
        /// <summary>
        /// Method to add a student petition to the section permission
        /// </summary>
        /// <param name="studentPetition">The student petition to add.</param>
        public void AddStudentPetition(StudentPetition studentPetition)
        {

            if (studentPetition != null)
            {
                if (studentPetition.SectionId != SectionId)
                {
                    throw new ArgumentException("The student petition's section Id does not match the SectionPermission Section ID. Unable to add.");
                }
                if (studentPetition.Type != StudentPetitionType.StudentPetition)
                {
                    throw new ArgumentException("The type of student petition is not StudentPetition. Cannot add it to the StudentPetitions list.");
                }
                var stPetition = StudentPetitions.Where(rw => rw.Equals(studentPetition)).FirstOrDefault();
                if (stPetition != null)
                {
                    throw new ArgumentException("Cannot add student petition for Id " + studentPetition.Id + ". Student Petition is a duplicate ");
                }
                else
                {
                    _studentPetitions.Add(studentPetition);
                }
            }
        }
        /// <summary>
        /// Method to add a faculty consent
        /// </summary>
        /// <param name="facultyConsent">The student petition which is a faculty consent for this section petition. </param>
        public void AddFacultyConsent(StudentPetition facultyConsent)
        {
            if (facultyConsent != null)
            {
                if (facultyConsent.SectionId != SectionId)
                {
                    throw new ArgumentException("The faculty consent's section Id does not match the SectionPermission Section ID. Unable to add.");
                }
                if (facultyConsent.Type != StudentPetitionType.FacultyConsent)
                {
                    throw new ArgumentException("The type of student petition is not FacultyConsent. Cannot add it to the FacultyConsents list.");
                }
                var facConsent = FacultyConsents.Where(rw => rw.Equals(facultyConsent)).FirstOrDefault();
                if (facConsent != null)
                {
                    throw new ArgumentException("Cannot add faculty consent for  Id " + facultyConsent.Id + ". Faculty Consent is a duplicate ");
                }
                else
                {
                    _facultyConsents.Add(facultyConsent);
                }
            }
        }


    }
}
