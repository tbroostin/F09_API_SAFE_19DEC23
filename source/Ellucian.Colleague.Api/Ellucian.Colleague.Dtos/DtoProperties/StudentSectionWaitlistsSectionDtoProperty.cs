using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The section associated with the waitlist. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class StudentSectionWaitlistsSectionDtoProperty
    {
        /// <summary>
        /// The related section GUID
        /// </summary>
        [DataMember(Name = "id")]
        public string sectionId { get; set; }

    }
}
  