// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Method of Anonymous Grading being used
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AnonymousGradingType
    {
        /// <summary>
        /// Anonymous Grading has not be configured or is not being used
        /// </summary>
        None,

        /// <summary>
        /// Anonymous Grading is done by term and a unique ids is generated for each student term 
        /// </summary>
        Term,

        /// <summary>
        /// Anonymous Grading is done by section and a unique id is generated for each student course section
        /// </summary>
        Section
    }
}
