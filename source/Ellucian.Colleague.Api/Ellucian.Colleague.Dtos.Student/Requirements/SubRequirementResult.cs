// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// The results of the evaluation of a subrequirement
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class SubrequirementResult : BaseResult
    {
        /// <summary>
        /// Unique Id of the subrequirement
        /// </summary>
        public string SubrequirementId { get; set; }
        /// <summary>
        /// Results of the evaluation
        /// <see cref="GroupResult"/>
        /// </summary>
        public List<GroupResult> GroupResults { get; set; }
    }
}
