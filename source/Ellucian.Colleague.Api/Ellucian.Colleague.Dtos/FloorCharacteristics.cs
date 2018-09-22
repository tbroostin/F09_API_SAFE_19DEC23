// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of floor characteristics that may be requested as part of a room request. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class FloorCharacteristics : CodeItem2
    {
    }
}
