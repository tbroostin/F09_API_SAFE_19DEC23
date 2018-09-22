using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of characteristics that may be requested for roommates. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RoommateCharacteristics : CodeItem2
    {
    }
}          

