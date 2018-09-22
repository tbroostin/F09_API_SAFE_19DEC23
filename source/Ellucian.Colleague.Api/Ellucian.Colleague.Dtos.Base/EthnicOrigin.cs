using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Translate to Unknown, HispanicOrLatino, or NonHispanicOrLatino for Person Entity
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EthnicOrigin
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Either Hispanic or Latino
        /// </summary>
        HispanicOrLatino,
        /// <summary>
        /// Either American or Alaskan Native
        /// </summary>
        AmericanIndianOrAlaskanNative,
        /// <summary>
        /// Asian
        /// </summary>
        Asian,
        /// <summary>
        /// African American
        /// </summary>
        BlackOrAfricanAmerican,
        /// <summary>
        /// Native Hawaiian or Pacific Islander
        /// </summary>
        NativeHawaiianOrOtherPacificIslander,
        /// <summary>
        /// Caucasian or white Ethnicity
        /// </summary>
        White
    }
}
