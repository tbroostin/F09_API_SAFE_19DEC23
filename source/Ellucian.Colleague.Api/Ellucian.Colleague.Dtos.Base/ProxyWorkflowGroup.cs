// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A group of related workflows to which proxy access can be granted
    /// </summary>
    public class ProxyWorkflowGroup
    {
        /// <summary>
        /// ID of the workflow group
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the workflow group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Boolean representing whether or not this workflow is for use by employee proxy
        /// </summary>
        public bool IsEmployeeWorkflow { get; set; }

        /// <summary>
        /// Workflows to which proxy access can be granted
        /// </summary>
        public IEnumerable<ProxyWorkflow> Workflows { get; set; }
    }
}
