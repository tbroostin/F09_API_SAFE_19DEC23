// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Indicates the type of the student's program notice text.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EvaluationNoticeType
    {
        /// <summary>
        /// Text for the specific student and program
        /// </summary>
        StudentProgram, 
        /// <summary>
        /// Text specific to the program
        /// </summary>
        Program,
        /// <summary>
        /// Text designated for the beginning of the degree audit report
        /// </summary>
        Start,
        /// <summary>
        /// Text designated for the end of the degree audit report
        /// </summary>
        End
    }
}
