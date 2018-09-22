using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Types of meals served at the campus dining facilities 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MealTypes : CodeItem2
    {        
    }      
}          
