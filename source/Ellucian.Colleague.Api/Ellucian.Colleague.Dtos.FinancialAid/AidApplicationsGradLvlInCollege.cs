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
    /// Grad Level in College
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsGradLvlInCollege
    {
        /// <summary>
        /// 1st year, never attended college
        /// </summary>
        [EnumMember(Value = "FirstYearNeverAttendedCollege")]
        FirstYearNeverAttendedCollege = 0,

        /// <summary>
        /// 1st year, attended college before
        /// </summary>
        [EnumMember(Value = "FirstYearAttendedCollegeBefore")]
        FirstYearAttendedCollegeBefore = 1,

        /// <summary>
        /// 2nd year/sophomore
        /// </summary>
        [EnumMember(Value = "SecondYearOrSophomore")]
        SecondYearOrSophomore = 2,

        /// <summary>
        /// 3rd year/junior
        /// </summary>
        [EnumMember(Value = "ThirdYearOrJunior")]
        ThirdYearOrJunior = 3,

        /// <summary>
        /// 4th year/senior
        /// </summary>
        [EnumMember(Value = "FourthYearOrSenior")]
        FourthYearOrSenior = 4,

        /// <summary>
        /// 5th year/other undergraduate
        /// </summary>
        [EnumMember(Value = "FifthYearOrOtherUndergraduate")]
        FifthYearOrOtherUndergraduate = 5,

        /// <summary>
        /// 1st year graduate/professional
        /// </summary>
        [EnumMember(Value = "FirstYearGraduateOrProfessional")]
        FirstYearGraduateOrProfessional = 6,

        /// <summary>
        /// Continuing graduate/professional
        /// </summary>
        [EnumMember(Value = "ContinuingGraduateOrProfessional")]
        ContinuingGraduateOrProfesional = 7
    }
}
