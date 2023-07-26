using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Degree or Certificates
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsDegreeOrCert
    {
        /// <summary>
        /// 1st Bachelor's Degree 
        /// </summary>
        [EnumMember(Value = "FirstBachelorsDegree")]
        FirstBachelorsDegree = 1,

        /// <summary>
        /// 2nd Bachelor's Degree
        /// </summary>
        [EnumMember(Value = "SecondBachelorsDegree")]
        SecondBachelorsDegree = 2,

        /// <summary>
        /// Associate Degree (occupational or technical program)
        /// </summary>
        [EnumMember(Value = "AssociateDegreeOccupationalOrTechnicalProgram")]
        AssociateDegreeOccupationalOrTechnicalProgram = 3,

        /// <summary>
        /// Associate degree (general education or transfer program)
        /// </summary>
        [EnumMember(Value = "AssociateDegreeGeneralEducationOrTransferProgram")]
        AssociateDegreeGeneralEducationOrTransferProgram = 4,

        /// <summary>
        /// Certificate or diploma for completing an occupational, technical, or educational program of less than two years
        /// </summary>
        [EnumMember(Value = "CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfLessThanTwoYears")]
        CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfLessThanTwoYears = 5,

        /// <summary>
        /// Certificate or diploma for completing an occupational, technical, or educational program of at least two years
        /// </summary>
        [EnumMember(Value = "CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfAtLeastTwoYears")]
        CertificateOrDiplomaForCompletingAnOccupationalOrTechnicalOrEducationalProgramOfAtLeastTwoYears = 6,

        /// <summary>
        ///  Teaching Credential Program (non- degree program)
        /// </summary>
        [EnumMember(Value = "TeachingCredentialProgram")]
        TeachingCredentialProgram = 7,

        /// <summary>
        /// Graduate or professional degree
        /// </summary>
        [EnumMember(Value = "GraduateOrProfessionalDegree")]
        GraduateOrProfessionalDegree = 8,

        /// <summary>
        /// Other/undecided 
        /// </summary>
        [EnumMember(Value = "OtherOrUndecided")]
        OtherOrUndecided = 9
    } 
}
