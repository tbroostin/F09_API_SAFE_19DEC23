/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Information about an aid application additional info
    /// </summary>
    [DataContract]
    public class AidApplicationAdditionalInfo
    {
        /// <summary>
        /// The derived identifier for the resource.
        /// </summary>
        [JsonProperty("id")]
        [Metadata("FAAPP.ADDL.ID", false, DataDescription = "The derived identifier for the resource.")]
        public string Id { get; set; }

        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity.
        /// </summary>        
        [JsonProperty("appDemoId")]
        [FilterProperty("criteria")]
        [Metadata("FAAPP.DEMO.ID", true, DataDescription = "Contains the sequential key to the FAAPP.DEMO entity.")]
        public string AppDemoId { get; set; }

        /// <summary>
        /// The Key to PERSON.
        /// </summary>
        [JsonProperty("personId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FADL.STUDENT.ID", false, DataDescription = "The Key to PERSON.")]
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record.
        /// </summary>
        [JsonProperty("applicationType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FADL.TYPE", false, DataDescription = "The type of application record.")]
        public string ApplicationType { get; set; }

        /// <summary>
        /// The student's assigned ID.
        /// </summary>
        [JsonProperty("applicantAssignedId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FAAD.ASSIGNED.ID", false, DataDescription = "The student's assigned ID.")]
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// Stores the year associated to the application.
        /// </summary>
        [JsonProperty("aidYear", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [FilterProperty("criteria")]
        [Metadata("FADL.YEAR", false, DataDescription = "The year associated to the application.")]
        public string AidYear { get; set; }

        /// <summary>
        /// The student's State Student Identification Number (SSID).
        /// </summary>
        [JsonProperty("studentStateId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.SSID", false, DataDescription = "The student's State Student Identification Number (SSID).")]
        public string StudentStateId { get; set; }

        /// <summary>
        /// Whether the student is in foster care.
        /// </summary>
        [JsonProperty("fosterCare", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.FOSTER.CARE", false, DataDescription = "Whether the student is in foster care.")]
        public bool? FosterCare { get; set; }

        /// <summary>
        /// The county specified on the application.
        /// </summary>
        [JsonProperty("applicationCounty", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.COUNTY", false, DataDescription = "The county specified on the application.")]
        public string ApplicationCounty { get; set; }

        /// <summary>
        /// The state associated to the wardship.
        /// </summary>
        [JsonProperty("wardshipState", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.WARDSHIP.STATE", false, DataDescription = "The state associated to the wardship.")]
        public string WardshipState { get; set; }

        /// <summary>
        /// The Chafee Consideration indicator.
        /// </summary>
        [JsonProperty("chafeeConsideration", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.CHAFEE.CONSIDER", false, DataDescription = "The Chafee Consideration indicator.")]
        public bool? ChafeeConsideration { get; set; }


        /// <summary>
        /// Indicates the application transaction that was used to populate BOGG.ACYR data.
        /// </summary>
        [JsonProperty("createCcpgRecord", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.CCPG.ACTIVE", false, DataDescription = "Indicates the application transaction that was used to populate BOGG.ACYR data.")]
        public bool? CreateCcpgRecord { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user1", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER1", false, DataDescription = "This is a field created for client usage.")]
        public string User1 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user2", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER2", false, DataDescription = "This is a field created for client usage.")]
        public string User2 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user3", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER3", false, DataDescription = "This is a field created for client usage.")]
        public string User3 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user4", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER4", false, DataDescription = "This is a field created for client usage.")]
        public string User4 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user5", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER5", false, DataDescription = "This is a field created for client usage.")]
        public string User5 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user6", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER6", false, DataDescription = "This is a field created for client usage.")]
        public string User6 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user7", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER7", false, DataDescription = "This is a field created for client usage.")]
        public string User7 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user8", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER8", false, DataDescription = "This is a field created for client usage.")]
        public string User8 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user9", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER9", false, DataDescription = "This is a field created for client usage.")]
        public string User9 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user10", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER10", false, DataDescription = "This is a field created for client usage.")]
        public string User10 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user11", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER11", false, DataDescription = "This is a field created for client usage.")]
        public string User11 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user12", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER12", false, DataDescription = "This is a field created for client usage.")]
        public string User12 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user13", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER13", false, DataDescription = "This is a field created for client usage.")]
        public string User13 { get; set; }

        /// <summary>
        /// This is a field created for client usage.
        /// </summary>
        [JsonProperty("user14", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER14", false, DataDescription = "This is a field created for client usage.")]
        public string User14 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user15", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER15", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User15 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user16", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER16", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User16 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user17", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER17", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User17 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user18", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER18", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User18 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user19", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER19", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User19 { get; set; }

        /// <summary>
        /// This is a date field created for client usage.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("user21", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Metadata("FADL.USER21", false, DataMaxLength = 50, DataDescription = "This is a field created for client usage.")]
        public DateTime? User21 { get; set; }
    }
}
