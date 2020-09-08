/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Possible benefit type actions Enroll, Keep and Cancel
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BenefitTypeAction
    {
        /// <summary>
        /// Benefit type enrolled
        /// </summary>
        Enroll,
        /// <summary>
        /// Benefit type keep
        /// </summary>
        Keep,
        /// <summary>
        /// Benefit type cancelled
        /// </summary>
        Cancel
    }
}
