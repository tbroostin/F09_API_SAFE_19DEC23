// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A list of standard employment vocation. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class EmploymentVocation : CodeItem2
    {
    }
}
