       //Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined fixed asset designations. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class FixedAssetDesignations : CodeItem2
    {
    }
}
