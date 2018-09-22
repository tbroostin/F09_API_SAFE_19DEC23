//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A user defined list of attendance categories. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AttendanceCategories : CodeItem2
    {
    }
}
