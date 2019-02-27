// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Details on how the student elected to have their transcript grades recorded.
    /// </summary>
    [DataContract]
    public class SectionRegistrationTranscript2
    {
        /// <summary>
        /// The grading scheme that will be used to record the student's grade for the section on a transcript.
        /// </summary>
        [DataMember(Name = "gradeScheme")]
        public GuidObject2 GradeScheme { get; set; }

        /// <summary>
        /// The manner in which a student's transcript grade will be reported.
        /// </summary>
        [DataMember(Name = "mode", EmitDefaultValue = false)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TranscriptMode2? Mode { get; set; }
    }
}