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
    /// A user defined list of valid shipping methods. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ShippingMethods : CodeItem2
    {        
     }      
}          
