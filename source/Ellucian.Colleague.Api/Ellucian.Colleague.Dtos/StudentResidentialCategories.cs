using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Categories that allow grouping of students based on their campus residential status 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentResidentialCategories : CodeItem2
    {
    }
}
