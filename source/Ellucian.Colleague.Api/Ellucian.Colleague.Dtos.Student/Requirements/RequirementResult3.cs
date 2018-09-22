// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a requirement evaluation
    /// </summary>
    public class RequirementResult3 : BaseResult2
    {
        /// <summary>
        /// Id of requirement evaluated
        /// </summary>
        public string RequirementId { get; set; }
        /// <summary>
        /// List of results for each subrequirement attached to this requirement
        /// <see cref="SubrequirementResult3"/>
        /// </summary>
        public List<SubrequirementResult3> SubrequirementResults { get; set; }
    }
}
