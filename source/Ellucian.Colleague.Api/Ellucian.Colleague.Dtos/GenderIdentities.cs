//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined gender identities. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GenderIdentities : CodeItem2
    {
    }
}