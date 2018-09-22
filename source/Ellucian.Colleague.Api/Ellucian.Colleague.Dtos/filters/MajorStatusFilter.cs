// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Named query for academic-disciplines
    /// </summary>
    public class MajorStatusFilter
    {

        /// <summary>
        /// The status of the major in the MAJORS file, if any (active or inactive.)
        /// If not a major at the host institution, (not in MAJORS file,) then "inactive" as well
        /// </summary>
        [JsonProperty("majorStatus")]
        [FilterProperty("majorStatus")]
        public MajorStatus MajorStatus { get; set; }

    }
}

