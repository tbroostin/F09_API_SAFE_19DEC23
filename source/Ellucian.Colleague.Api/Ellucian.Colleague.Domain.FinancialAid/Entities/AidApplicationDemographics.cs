/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities

{
    [Serializable]
    public class AidApplicationDemographics
    {
        // Required fields

        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>
        public string Id { get { return _id; } }
        private readonly string _id;

        /// <summary>
        /// The Key to PERSON.
        /// </summary>
        public string PersonId { get { return _personId; } }
        private readonly string _personId;


        /// <summary>
        /// The type of application record.
        /// </summary>
        public string ApplicationType { get { return _applicantType; } }
        private readonly string _applicantType;

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>
        public string AidYear { get { return _aidYear; } }
        private readonly string _aidYear;

        // Non-required fields

        /// <summary>
        /// The student's assigned ID.
        /// </summary>
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// The first two characters of the student's last name.
        /// </summary>
        public string OrigName { get; set; }

        /// <summary>
        /// The student's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The student's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The student's middle initial.
        /// </summary>
        public string MiddleInitial { get; set; }

        /// <summary>
        /// The student's address.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// The student's birthdate.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// The student's phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The student's email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The student's citizenship status.
        /// </summary>
        public AidApplicationCitizenshipStatus? CitizenshipStatusType { get; set; }

        /// <summary>
        /// The student's alternate or cell phone number.
        /// </summary>
        public string AlternatePhoneNumber { get; set; }

        /// <summary>
        /// The student's Individual Taxpayer Identification Number (ITIN).
        /// </summary>        
        public string StudentTaxIdNumber { get; set; }

        /// <summary>
        /// constructor to initialize properties
        /// </summary>
        /// <param name="id">Id of the record</param>
        /// <param name="personId">Person Id</param>
        /// <param name="aidYear">Fin aid year</param>
        /// <param name="applicationType">application type</param>
        /// <param name="applicantAssignedId">applicant assignee Id</param>
        public AidApplicationDemographics(string id, string personId, string aidYear, string applicationType)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(aidYear))
            {
                throw new ArgumentNullException("aidYear");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            if (string.IsNullOrEmpty(applicationType))
            {
                throw new ArgumentNullException("applicationType");
            }

            _id = id;
            _aidYear = aidYear;
            _personId = personId;
            _applicantType = applicationType;
        }
    }
}

