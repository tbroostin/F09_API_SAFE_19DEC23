
//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The student associated with the section waitlist. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class StudentSectionWaitlistsPersonDtoProperty
    {
        /// <summary>
        /// The related person GUID
        /// </summary>
        [DataMember(Name = "id")]
        public string personId { get; set; }
    }

}

