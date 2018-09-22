//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of standard position classifications that may be specified for an organization. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PositionClassification : CodeItem2
    {
    }
}
