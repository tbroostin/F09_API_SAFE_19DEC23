// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

/// <summary>
/// the grad/education level of parent DTO
/// </summary>
namespace Ellucian.Colleague.Dtos.FinancialAid
{

    /// <summary>
    /// the differnet grad/education level of parent
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AidApplicationsParentEdLevel
    {
        /// <summary>
        /// Middle School
        /// </summary>
        [EnumMember(Value = "MiddleSchoolOrJrHigh")]
        MiddleSchoolOrJrHigh = 1,

        /// <summary>
        /// High School
        /// </summary>
        [EnumMember(Value = "HighSchool")]
        HighSchool = 2,

        /// <summary>
        /// College
        /// </summary>
        [EnumMember(Value = "CollegeOrBeyond")]
        CollegeOrBeyond = 3,

        /// <summary>
        /// Others/Unreported
        /// </summary>
        [EnumMember(Value = "OtherOrUnknown")]
        OtherOrUnknown = 4,

        
    }
}
