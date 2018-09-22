using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The list of valid instructor tenure types. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InstructorTenureTypes : CodeItem2
    {
    }
}
