//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined course title types (e.g. short title, long title). 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CourseTitleType : CodeItem2
    {
    }
}
