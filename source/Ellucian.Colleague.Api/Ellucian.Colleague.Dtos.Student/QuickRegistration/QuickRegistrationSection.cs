// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.QuickRegistration
{    /// <summary>
     /// A student's previously selected course section that may be used in the Colleague Self-Service Quick Registration workflow
     /// </summary>
    public class QuickRegistrationSection
    {
        /// <summary>
        /// Course section identifier
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Number of credits selected for the course section
        /// </summary>
        public decimal? Credits { get; set; }

        /// <summary>
        /// Grading type selected for the course section
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public GradingType GradingType { get; set; }

        /// <summary>
        /// Student's waitlist status for the course section
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DegreePlans.WaitlistStatus WaitlistStatus { get; set; }

    }
}
