// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Active on named query
    /// </summary>
    public class RecruitmentProgramFilter
    {
        /// <summary>
        /// activeOn
        /// </summary>        
        [DataMember(Name = "recruitmentProgram", EmitDefaultValue = false)]
        [FilterProperty("recruitmentProgram")]
        public RecruitmentProgramType? RecruitmentProgram { get; set; }
    }
}
