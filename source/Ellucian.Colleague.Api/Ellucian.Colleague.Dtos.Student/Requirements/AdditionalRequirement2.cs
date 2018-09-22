using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Additional Requirement
    /// </summary>
    public class AdditionalRequirement2
    {
        /// <summary>
        /// Award when requirement is satisfied
        /// </summary>
        public string AwardName { get; set; }
        /// <summary>
        /// Type of award <see cref="AwardType"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AwardType Type { get; set; }
        /// <summary>
        /// Unique Code (id) of this requirement
        /// </summary>
        public string RequirementCode { get; set; }
        /// <summary>
        /// Detailed requirement information <see cref="Requirement"/>
        /// </summary>
        public Requirement Requirement { get; set; }
    }
}
