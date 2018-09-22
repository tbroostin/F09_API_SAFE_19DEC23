// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents the different types of relationships available in organizational relationship
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganizationalRelationshipType
    {
        /// <summary>
        /// Represents a Subordinate-Manager relationship
        /// </summary>
        Manager,

        /// <summary>
        /// Represents an unknown relationship type
        /// </summary>
        Unknown
    }
}
