// Copyright 2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Academic Levels DTO property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class LocationSiteDtoProperty : BaseCodeTitleDetailDtoProperty, InstructionalLocationDtoProperty
    {

        /// <summary>
        /// Location type
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public InstructionalLocationType? LocationType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public LocationSiteDtoProperty() : base()
        {
            LocationType = InstructionalLocationType.InstructionalSite;
        }
    }
}