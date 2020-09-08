//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The tax reporting instruments on which tax-related activities are reported to a government agency (i.e. 1099, T4A). 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TaxForms : CodeItem2
    {
    }
}     
