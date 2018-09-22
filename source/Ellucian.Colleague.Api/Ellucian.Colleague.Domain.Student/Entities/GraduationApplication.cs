// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how a graduation application should be rendered to a user.
    /// </summary>
    [Serializable]
    public class GraduationApplication
    {
        /// <summary>
        /// Graduation Application Id. 
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string _id;

        /// <summary>
        /// Student ID of person who has submitted the application
        /// </summary>
        public string StudentId { get { return _studentId; } }
        private string _studentId;

        /// <summary>
        /// Program for which the student is applying for 
        /// </summary>
        public string ProgramCode { get { return _programCode; } }
        private string _programCode;

        /// <summary>
        /// Graduation Term when the student is applying to graduate
        /// </summary>
        public string GraduationTerm { get; set; }

        /// <summary>
        /// Location where student will attend commencement
        /// </summary>
        public string CommencementLocation { get; set; }

        /// <summary>
        /// Date student has chosen to attend commencement. (This could possibly include a time.)
        /// </summary>
        public DateTimeOffset? CommencementDate { get; set; }

        /// <summary>
        /// Date student graduation application was added.
        /// </summary>
        public DateTimeOffset? SubmittedDate { get; set; }

        /// <summary>
        /// Name the student wants to be printed on the diploma
        /// </summary>
        public string DiplomaName { get; set; }

        /// <summary>
        /// Indicates whether the student will pickup their diploma
        /// </summary>
        public bool? WillPickupDiploma { get; set; }

        /// <summary>
        /// Address where the diploma will be mailed to.
        /// </summary>
        public List<string> MailDiplomaToAddressLines { get; set; }

        /// <summary>
        /// City where diploma will be mailed to.
        /// </summary>
        public string MailDiplomaToCity { get; set; }

        /// <summary>
        /// State where diploma will be mailed to.
        /// </summary>
        public string MailDiplomaToState { get; set; }

        /// <summary>
        /// Postal Code where diploma will be mailed to.
        /// </summary>
        public string MailDiplomaToPostalCode { get; set; }

        /// <summary>
        /// Country where diploma will be mailed to.
        /// </summary>
        public string MailDiplomaToCountry { get; set; }

        /// <summary>
        /// Phonetic Spelling of Student's Name for Commencement Ceremony
        /// </summary>
        public string PhoneticSpellingOfName { get; set; }

        /// <summary>
        /// Indicates whether the student wished their name to be in the printed program
        /// </summary>
        public bool? IncludeNameInProgram { get; set; }

        /// <summary>
        /// Indicates whether the student will be attending commencement
        /// </summary>
        public bool? AttendingCommencement { get; set; }

        /// <summary>
        /// Number of guests the student has invited to commencement
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Hometown the student wishes to be associated with 
        /// </summary>
        public string Hometown { get; set; }

        /// <summary>
        /// Cap Size
        /// </summary>
        public string CapSize { get; set; }

        /// <summary>
        /// Gown Size
        /// </summary>
        public string GownSize { get; set; }

        /// <summary>
        /// Veteran status
        /// </summary>
        public GraduateMilitaryStatus? MilitaryStatus { get; set; }

        /// <summary>
        /// The specified accommodations that the student will need at commencement
        /// </summary>
        public string SpecialAccommodations { get; set; }

        /// <summary>
        /// If an invoice was generated as a result of this application this is the invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Primary Loacation of a student for a given program.
        /// </summary>
        public string PrimaryLocation { get; set; }

        /// <summary>
        ///  Has the Academic Credintials Record been updated in Colleague (from the Graduates)?
        /// </summary>
        public bool AcadCredentialsUpdated { get; set; }

        /// <summary>
        /// Constructor to add a new Graduation Application
        /// </summary>
        /// <param name="id">Id of the graduation application (studentID*programCode)</param>
        /// <param name="studentId">Student Id for which the application belongs</param>
        /// <param name="programCode">Program code the student is applying for graduation from</param>
        /// <param name="graduationTerm">Graduation Term in which the student is applying for graduation.</param>
        public GraduationApplication(string id, string studentId, string programCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required");
            }
            if (string.IsNullOrEmpty(programCode))
            {
                throw new ArgumentNullException("programCode", "Program Code is required");
            }
            _id = id;
            _studentId = studentId;
            _programCode = programCode;
        }
    }
}
