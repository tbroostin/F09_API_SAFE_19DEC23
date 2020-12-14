// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.EnumProperties
{
    /// <summary>
    /// EducationalInstitutionType enumeration 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EducationalInstitutionType
    {
        /// <summary>
        /// private
        /// </summary>
        [EnumMember(Value = "secondarySchool")]
        SecondarySchool,

        /// <summary>
        /// public
        /// </summary>
        [EnumMember(Value = "postSecondarySchool")]
        PostSecondarySchool
    }
}