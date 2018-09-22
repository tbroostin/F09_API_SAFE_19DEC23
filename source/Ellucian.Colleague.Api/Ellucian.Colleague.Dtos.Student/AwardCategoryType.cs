/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// An AwardCategoryType generalizes groups of awards even further than 
    /// an award's category.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AwardCategoryType
    {
        /// <summary>
        /// Loan category type
        /// </summary>
        Loan,
        /// <summary>
        /// Grant category type
        /// </summary>
        Grant,
        /// <summary>
        /// Scholarship category type
        /// </summary>
        Scholarship,
        /// <summary>
        /// Work category type
        /// </summary>
        Work
    }
}
