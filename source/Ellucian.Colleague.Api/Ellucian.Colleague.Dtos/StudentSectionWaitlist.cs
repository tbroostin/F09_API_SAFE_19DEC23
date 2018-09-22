//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Sections' waitlists for students. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentSectionWaitlist : BaseModel2
    {
        /// <summary>
        /// The student associated with the section waitlist.
        /// </summary>

        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentSectionWaitlistsPersonDtoProperty Person { get; set; }

        /// <summary>
        /// The section associated with the waitlist.
        /// </summary>

        [JsonProperty("section", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentSectionWaitlistsSectionDtoProperty Section { get; set; }

        /// <summary>
        /// The number that represents the ranking of the waitlisted student for the given section.
        /// </summary>

        [JsonProperty("priority", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Priority { get; set; }

    }
}
