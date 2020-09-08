//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined aptitude assessment sources. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AptitudeAssessmentSources : CodeItem2
    {
    }
}