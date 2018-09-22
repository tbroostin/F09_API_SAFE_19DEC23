using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of valid reasons that may be used when overriding a standard cost/fee. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BillingOverrideReasons : CodeItem2
    {
    }
}
