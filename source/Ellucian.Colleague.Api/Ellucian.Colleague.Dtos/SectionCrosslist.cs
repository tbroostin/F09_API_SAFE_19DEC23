// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Instnce of Crosslist information for Sections
    /// </summary>
    [DataContract]
    public class SectionCrosslist : BaseModel2
    {
        /// <summary>
        /// The code that identifies the list of cross-listed sections.
        /// </summary>
        //[DataMember(Name = "code", EmitDefaultValue = false)]
        //public string Code { get; set; }

        /// <summary>
        /// The list of cross-listed sections.
        /// </summary>
        [DataMember(Name = "sections")]
        public List<DtoProperties.SectionsForCrosslistDtoProperty> Sections { get; set; }

        /// <summary>
        /// An indicator specifying if all students are placed on the wait-list when any of the cross-listed sections has 
        /// reached its maximum enrollment or only when the combined registration has reached the specified maximum enrollment of the cross-list.
        /// </summary>
        [DataMember(Name = "waitlist", EmitDefaultValue = false)]
        public EnumProperties.WaitlistForCrosslist? Waitlist { get; set; }

        /// <summary>
        /// The maximum enrollment of the cross-listed section over all included sections.
        /// </summary>
        [DataMember(Name = "maxEnrollment", EmitDefaultValue = false)]
        public int? MaxEnrollment { get; set; }

        /// <summary>
        /// The maximum number of students allowed in the combined wait-list for the cross-listed section.
        /// </summary>
        [DataMember(Name = "maxWaitlist", EmitDefaultValue = false)]
        public int? MaxWaitlist { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SectionCrosslist() : base()
        {
            Sections = new List<SectionsForCrosslistDtoProperty>();
        }

    }
}
