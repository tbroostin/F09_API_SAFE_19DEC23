/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{/// <summary>
 /// Information about an aid applications
 /// </summary>
    [DataContract]
    public class AidApplications
    {
        /// <summary>
        /// A global identifier for the resource
        /// </summary>
        [JsonProperty("id")]
        [Metadata("FAAPP.APPS.ID", true, DataDescription = "The derived identifier for the resource.")]
        public string Id { get; set; }

        /// <summary>
        /// Contains the sequential key to the FAAPP.DEMO entity
        /// </summary>
        [JsonProperty("appDemoID")]
        [Metadata("FAAPP.DEMO.ID", true, DataDescription = "Contains the sequential key to the FAAPP.DEMO entity.")]
        [FilterProperty("criteria")]
        public string AppDemoID { get; set; }

        /// <summary>
        /// The key to person
        /// </summary>
        [JsonProperty("personId")]
        [Metadata("FAAA.STUDENT.ID", false, DataDescription = "The Key to PERSON.")]
        [FilterProperty("criteria")]
        public string PersonId { get; set; }

        /// <summary>
        /// The type of application record
        /// </summary>
        [JsonProperty("applicationType")]
        [Metadata("FAAA.TYPE", false, DataDescription = "The type of application record.")]
        [FilterProperty("criteria")]
        public string ApplicationType { get; set; }

        /// <summary>
        /// This field stores the year.
        /// </summary>
        [JsonProperty("aidYear")]
        [Metadata("FAAA.YEAR", false, DataDescription = "Stores the year associated to the application.")]
        [FilterProperty("criteria")]
        public string AidYear { get; set; }

        /// <summary>
        /// This field stores the Assigned Id
        /// </summary>
        [JsonProperty("applicantAssignedId")]
        [Metadata("FAAD.ASSIGNED.ID", false, DataDescription = "The student's assigned ID.")]
        [FilterProperty("criteria")]
        public string ApplicantAssignedId { get; set; }

        /// <summary>
        /// <see cref="StudentMaritalInfo"> Student's marital status and marital status date </see> object
        /// </summary>
        [DataMember(Name = "studentMarital", EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public StudentMaritalInfo StudentMarital { get; set; }

        /// <summary>
        /// <see cref="LegalResidence"> Student's legal residence details </see> object
        /// </summary>
        [DataMember(Name = "legalResidence", EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public LegalResidence StudentLegalResidence { get; set; }

        /// <summary>
        /// <see cref="ParentsInfo"> Student's parents information </see> object
        /// </summary>
        [JsonProperty("parents", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ParentsInfo Parents { get; set; }

        /// <summary>
        /// <see cref="HighSchoolDetails"/> Student's high school information
        /// </summary>
        [JsonProperty("highSchool", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public HighSchoolDetails HighSchool { get; set; }

        /// <summary>
        /// First bachelor's Degree
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("degreeBy", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.DEGREE.BY", false, DataDescription = "First bachelor's degree.", DataMaxLength = 5)]
        public bool? DegreeBy { get; set; }

        /// <summary>
        /// <see cref="AidApplicationsGradLvlInCollege"/> Grade level in college
        /// </summary>
        [JsonProperty("gradLevelInCollege", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.GRADE.LEVEL", false, DataDescription = "Grade level in college.", DataMaxLength = 32)]
        public AidApplicationsGradLvlInCollege? GradeLevelInCollege { get; set; }

        /// <summary>
        /// <see cref="AidApplicationsDegreeOrCert"/> Degree or certificate
        /// </summary>
        [JsonProperty("degreeOrCertificate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.DEG.OR.CERT", false, DataDescription = "Degree or certificate.", DataMaxLength = 100)]
        public AidApplicationsDegreeOrCert? DegreeOrCertificate { get; set; }

        /// <summary>
        /// <see cref="StudentIncome"/> Student's income details
        /// </summary>
        [JsonProperty("studentIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public StudentIncome StudentsIncome { get; set; }

        /// <summary>
        /// Born Before the year mentioned on the application
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("bornBefore", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.BORN.B4.DT", false, DataDescription = "Born before the year mentioned on the application.", DataMaxLength = 5)]
        public bool? BornBefore { get; set; }

        /// <summary>
        /// Student is married
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("married", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.MARRIED", false, DataDescription = "Is student married?", DataMaxLength = 5)]
        public bool? Married { get; set; }

        /// <summary>
        /// Student working on a master's or doctorate program
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("gradOrProfProgram", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.GRAD.PROF", false, DataDescription = "Working on a Master's or Doctorate Program?", DataMaxLength = 5)]
        public bool? GradOrProfProgram { get; set; }

        /// <summary>
        /// Student on active duty in U.S. armed forces
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("activeDuty", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.ACTIVE.DUTY", false, DataDescription = "Are you on active duty in U.S. Armed Forces?", DataMaxLength = 5)]
        public bool? ActiveDuty { get; set; }

        /// <summary>
        /// Student is veteran of U.S. armed forces
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("usVeteran", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.VETERAN", false, DataDescription = "Veteran of U.S. Armed Forces?", DataMaxLength = 5)]
        public bool? USVeteran { get; set; }

        /// <summary>
        /// Have children you support
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("dependentChildren", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.DEPEND.CHILDREN", false, DataDescription = "Have children you support?", DataMaxLength = 5)]
        public bool? DependentChildren { get; set; }

        /// <summary>
        /// Have legal dependents other than children or spouse
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("otherDependents", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.OTHER.DEPEND", false, DataDescription = "Have legal dependents other than children or spouse?", DataMaxLength = 5)]
        public bool? OtherDependents { get; set; }

        /// <summary>
        /// Student is Orphan, ward of court, or receives foster care
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("orphanWardFoster", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.ORPHAN.WARD", false, DataDescription = "Orphan, ward of court, or foster care?", DataMaxLength = 5)]
        public bool? OrphanWardFoster { get; set; }

        /// <summary>
        ///  Student is an emancipated minor
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("emancipatedMinor", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.EMANCIPATED.MINOR", false, DataDescription = "Student is an emancipated minor?", DataMaxLength = 5)]
        public bool? EmancipatedMinor { get; set; }

        /// <summary>
        /// Student is in legal guardianship
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("legalGuardianship", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.LEGAL.GUARDIANSHIP", false, DataDescription = "Student is in legal guardianship?", DataMaxLength = 5)]
        public bool? LegalGuardianship { get; set; }

        /// <summary>
        /// Unaccompanied youth determined by school district liaison
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("homelessBySchool", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.HOMELESS.BY.SCHOOL", false, DataDescription = "Unaccompanied youth determined by school district liaison?", DataMaxLength = 5)]
        public bool? HomelessBySchool { get; set; }

        /// <summary>
        /// Unaccompanied youth determined by HUD
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("homelessByHud", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.HOMELESS.BY.HUD", false, DataDescription = "Unaccompanied youth determined by HUD?", DataMaxLength = 5)]
        public bool? HomelessByHud { get; set; }

        /// <summary>
        /// Student is at risk of homelessness
        /// </summary>
        [JsonConverter(typeof(NullableBooleanConverter))]
        [JsonProperty("homelessAtRisk", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.HOMELESS.AT.RISK", false, DataDescription = "At risk of homelessness?", DataMaxLength = 5)]
        public bool? HomelessAtRisk { get; set; }

        /// <summary>
        /// Student's number of family members
        /// </summary>
        [JsonProperty("studentNumberInFamily", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.S.NBR.FAMILY", false, DataDescription = "Student's number of family members.")]
        public int? StudentNumberInFamily { get; set; }

        /// <summary>
        /// Student's number in college
        /// </summary>
        [JsonProperty("studentNumberInCollege", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.S.NBR.COLLEGE", false, DataDescription = "Student's number in college.")]
        public int? StudentNumberInCollege { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 1
        /// </summary>
        [JsonProperty("schoolCode1", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode1 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 2
        /// </summary>
        [JsonProperty("schoolCode2", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode2 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 3
        /// </summary>
        [JsonProperty("schoolCode3", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode3 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 4
        /// </summary>
        [JsonProperty("schoolCode4", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode4 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 5
        /// </summary>
        [JsonProperty("schoolCode5", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode5 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 6
        /// </summary>
        [JsonProperty("schoolCode6", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode6 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 7
        /// </summary>
        [JsonProperty("schoolCode7", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode7 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 8
        /// </summary>
        [JsonProperty("schoolCode8", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode8 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 9
        /// </summary>
        [JsonProperty("schoolCode9", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode9 { get; set; }

        /// <summary>
        /// <see cref="SchoolCode"/> Federal School code 10
        /// </summary>
        [JsonProperty("schoolCode10", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public SchoolCode SchoolCode10 { get; set; }

        /// <summary>
        /// Date the application was completed
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("applicationCompleteDate", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.DATE.CMPL", false, DataDescription = "Date the application was completed.")]
        public DateTime? ApplicationCompleteDate { get; set; }

        /// <summary>
        /// Signed by, Indicates if only the applicant, or only the parent, or both applicant and parent signed the transaction
        /// </summary>
        [JsonProperty("signedFlag", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.SIGNED.FLAG", false, DataDescription = "Indicates if only the applicant, or only the parent, or both applicant and parent signed the transaction.", DataMaxLength = 20)]
        public string SignedFlag { get; set; }

        /// <summary>
        /// Preparers Social Security Number indicates that the preparers SSN is provided on the transaction
        /// </summary>
        [JsonProperty("preparerSsn", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.PREPARER.SSN", false, DataDescription = "Preparer’s Social Security Number.")]
        public int? PreparerSsn { get; set; }

        /// <summary>
        /// Preparers Employer Identification Number(EIN)
        /// </summary>
        [JsonProperty("preparerEin", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.PREPARER.EIN", false, DataDescription = "Preparer's Employer Identification Number (EIN).")]
        public int? PreparerEin { get; set; }

        /// <summary>
        /// Preparers signature indicates that a preparer signed the transaction
        /// </summary>
        [JsonProperty("preparerSigned", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Metadata("FAAA.PREPARER.SIGNED", false, DataDescription = "Preparer's signature.", DataMaxLength = 3)]
        public string PreparerSigned { get; set; }

    }

}

