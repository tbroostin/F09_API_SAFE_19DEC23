using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Federal school code and housing plans 
    /// </summary>
    public class SchoolCode
    {
            /// <summary>
            /// Federal school code
            /// </summary>
            [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata(DataDescription = "Federal School Code.", DataMaxLength = 6)]
            public string Code { get; set; }

            /// <summary>
            /// <see cref="AidApplicationHousingPlanDto"/> Federal school code housing plans
            /// </summary>
            [JsonProperty("housingPlan", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata(DataDescription = "Federal School Code - Housing Plans.", DataMaxLength = 10)]
            public AidApplicationHousingPlanDto? HousingPlan { get; set; }
        
    }
}
