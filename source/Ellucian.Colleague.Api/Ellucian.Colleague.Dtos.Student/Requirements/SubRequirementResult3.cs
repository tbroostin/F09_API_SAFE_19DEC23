// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// The results of the evaluation of a subrequirement
    /// </summary>
    public class SubrequirementResult3 : BaseResult2
    {
        /// <summary>
        /// Unique Id of the subrequirement
        /// </summary>
        public string SubrequirementId { get; set; }
        /// <summary>
        /// Results of the evaluation
        /// <see cref="GroupResult"/>
        /// </summary>
        public List<GroupResult3> GroupResults { get; set; }
    }
}
