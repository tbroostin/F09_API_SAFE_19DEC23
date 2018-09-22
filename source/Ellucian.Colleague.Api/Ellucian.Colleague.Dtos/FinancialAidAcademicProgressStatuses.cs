//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of user defined financial aid satisfactory academic progress (SAP) statuses. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class FinancialAidAcademicProgressStatuses : CodeItem2
    {
    }
}