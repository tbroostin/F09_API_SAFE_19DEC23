//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined contract types. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContractTypes : CodeItem2
    {
    }
}