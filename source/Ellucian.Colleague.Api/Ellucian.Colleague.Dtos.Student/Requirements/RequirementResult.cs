using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a requirement evaluation
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
    public class RequirementResult : BaseResult
    {
        /// <summary>
        /// Id of requirement evaluated
        /// </summary>
        public string RequirementId;
        /// <summary>
        /// List of results for each subrequirement attached to this requirement
        /// <see cref="SubrequirementResult"/>
        /// </summary>
        public List<SubrequirementResult> SubrequirementResults;
    }
}
