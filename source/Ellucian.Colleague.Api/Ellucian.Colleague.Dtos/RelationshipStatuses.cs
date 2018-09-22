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
    /// The valid list of user defined relationship statuses. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RelationshipStatuses : CodeItem2
    {        
     }      
}          
