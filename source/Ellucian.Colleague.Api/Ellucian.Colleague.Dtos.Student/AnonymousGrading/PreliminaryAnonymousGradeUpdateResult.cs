// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.AnonymousGrading
{
    /// <summary>
    /// Result of an attempted preliminary anonymous grade update
    /// </summary>
    public class PreliminaryAnonymousGradeUpdateResult
    {
        /// <summary>
        /// ID of the associated student course section data
        /// </summary>
        /// <remarks>This ID is synonymous with the record for preliminary student grade work data, as it is a shared ID.</remarks>
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// Status of the update
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PreliminaryAnonymousGradeUpdateStatus Status { get; set; }

        /// <summary>
        /// Informational message related to the status of the update
        /// </summary>
        public string Message { get; set; }
    }
}
