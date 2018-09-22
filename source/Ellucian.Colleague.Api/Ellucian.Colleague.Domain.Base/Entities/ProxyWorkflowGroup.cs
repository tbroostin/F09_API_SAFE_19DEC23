// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A group of related workflows to which proxy access can be granted
    /// </summary>
    [Serializable]
    public class ProxyWorkflowGroup : CodeItem
    {
        private readonly List<ProxyWorkflow> _workflows = new List<ProxyWorkflow>();

        /// <summary>
        /// Workflows to which proxy access can be granted
        /// </summary>
        public ReadOnlyCollection<ProxyWorkflow> Workflows { get; private set; }

        /// <summary>
        /// Boolean representing whether or not this workflow is for use by employee proxy
        /// </summary>
        public bool IsEmployeeWorkflow { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyWorkflowGroup"/> class
        /// </summary>
        /// <param name="code">ID of the proxy workflow group</param>
        /// <param name="description">Description of the proxy workflow group</param>
        /// <param name="isEmployeeWorkflow">Boolean to determine if a group is an employee workflow</param>
        public ProxyWorkflowGroup(string code, string description, bool isEmployeeWorkflow = false)
            : base(code, description)
        {
            Workflows = _workflows.AsReadOnly();
            IsEmployeeWorkflow = isEmployeeWorkflow;
        }

        /// <summary>
        /// Add a proxy workflow to the proxy workflow group
        /// </summary>
        /// <param name="workflow">Proxy workflow to be added</param>
        public void AddWorkflow(ProxyWorkflow workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow", "A proxy workflow must be supplied.");
            }
            if (workflow.WorkflowGroupId != Code)
            {
                throw new ArgumentException("Proxy workflow's group ID must match group ID.", "workflow");
            }

            var currentWorkflowIds = _workflows.Select(w => w.Code).Distinct().ToList();
            if (!currentWorkflowIds.Contains(workflow.Code))
            {
                _workflows.Add(workflow);
            }
        }
    }
}
