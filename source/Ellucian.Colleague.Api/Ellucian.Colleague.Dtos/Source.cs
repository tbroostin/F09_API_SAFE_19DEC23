// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Source of information
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Source : CodeItem2
    {
        /// <summary>
        /// The context the source is used in.
        /// </summary>
       [JsonProperty("contexts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<GuidObject2> Contexts { get; set; }

        /// <summary>
        /// Life Cycle Status enumeration.  Valid values active; inactive
        /// </summary>
        [JsonProperty("status")]
        public LifeCycleStatus Status { get; set; }

    }
}