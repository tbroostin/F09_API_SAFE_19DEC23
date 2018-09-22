// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Result of a requirement evaluation
    /// </summary>
    public class RequirementResult2 : BaseResult2
    {
        /// <summary>
        /// Id of requirement evaluated
        /// </summary>
        public string RequirementId { get; set; }
        /// <summary>
        /// List of results for each subrequirement attached to this requirement
        /// <see cref="SubrequirementResult2"/>
        /// </summary>
        public List<SubrequirementResult2> SubrequirementResults { get; set; }
    }
}
