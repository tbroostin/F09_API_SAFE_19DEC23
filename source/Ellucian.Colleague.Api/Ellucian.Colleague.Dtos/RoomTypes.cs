// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of custom room types.
    /// </summary>
    [DataContract]
    public class RoomTypes : CodeItem2
    {
        /// <summary>
        /// The type or style of a of room's layout, for example, \"Classroom\" style, \"Banquet\" style, etc.
        /// </summary>
        [DataMember(Name = "type")]
        public RoomTypeTypes Type { get; set; }
    }
}