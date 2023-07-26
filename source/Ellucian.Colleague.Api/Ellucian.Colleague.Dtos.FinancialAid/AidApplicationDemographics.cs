/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Information about an aid application demographics
    /// </summary>
    [DataContract]
    public class AidApplicationDemographics
    {
        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>        
        [JsonProperty("id")]
        [Metadata("FAAPP.DEMO.ID", true, DataDescription = "The derived identifier for the resource.")]
        public string Id { get; set; }

        /// <summary>
        /// The Key to PERSON.
        /// </summary>        
        [JsonProperty("personId")]
        [FilterProperty("criteria")]
        [Metadata("FAAD.STUDENT.ID", true, DataDescription = "The Key to PERSON.")]
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record.
        /// </summary>        
        [JsonProperty("applicationType")]
        [FilterProperty("criteria")]
        [Metadata("FAAD.TYPE", true, DataDescription = "The type of application record.")]
        public string ApplicationType { get; set; }

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>        
        [JsonProperty("aidYear")]
        [FilterProperty("criteria")]
        [Metadata("FAAD.YEAR", true, DataDescription = "The year associated to the application.")]
        public string AidYear { get; set; }

        /// <summary>
        /// The student's assigned ID.
        /// </summary>        
        [JsonProperty("applicantAssignedId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FAAD.ASSIGNED.ID", false, DataDescription = "The student's assigned ID.")]
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// The first two characters of the student's last name.
        /// </summary>        
        [JsonProperty("origName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.ORIG.NAME", false, DataDescription = "The first two characters of the student's last name.")]
        public string OrigName { get; set; }

        /// <summary>
        /// The student's last name.
        /// </summary>        
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.NAME.LAST", false, DataDescription = "The student's last name.")]
        public string LastName { get; set; }

        /// <summary>
        /// The student's first name.
        /// </summary>        
        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.NAME.FIRST", false, DataDescription = "The student's first name.")]
        public string FirstName { get; set; }

        /// <summary>
        /// The student's middle initial.
        /// </summary>        
        [JsonProperty("middleInitial", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.NAME.MI", false, DataDescription = "The student's middle initial.")]
        public string MiddleInitial { get; set; }

        /// <summary>
        /// The student's address.
        /// </summary>        
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata()]
        public Address Address { get; set; }

        /// <summary>
        /// The student's birthdate.
        /// </summary>        
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("birthDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.BIRTHDATE", false, DataMaxLength = 50, DataDescription = "The student's birthdate.")]
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// The student's phone number.
        /// </summary>        
        [JsonProperty("phoneNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.PHONE", false, DataMaxLength = 15, DataDescription = "The student's phone number.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The student's email address.
        /// </summary>        
        [JsonProperty("emailAddress", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.STUDENT.EMAIL.ADDR", false, DataDescription = "The student's email address.")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// <see cref="AidApplicationCitizenshipStatus">Citizenship status</see> of the student
        /// </summary>       
        [JsonProperty("citizenshipStatus")]
        [Metadata("FAAD.CITIZENSHIP", false, DataMaxLength = 20, DataDescription = "The student's citizenship status.")]
        public AidApplicationCitizenshipStatus? CitizenshipStatusType { get; set; }

        /// <summary>
        /// The student's alternate or cell phone number.
        /// </summary>        
        [JsonProperty("alternatePhoneNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.ALTERNATE.NUMBER", false, DataMaxLength = 15, DataDescription = "The student's alternate or cell phone number.")]
        public string AlternatePhoneNumber { get; set; }

        /// <summary>
        /// The student's Individual Taxpayer Identification Number (ITIN).
        /// </summary>        
        [JsonProperty("studentTaxIdNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FAAD.ITIN", false, DataDescription = "The student's Individual Taxpayer Identification Number (ITIN).")]
        public string StudentTaxIdNumber { get; set; }
    }
}
