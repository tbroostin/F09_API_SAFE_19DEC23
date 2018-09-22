// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A workflow to which proxy access can be granted
    /// </summary>
    [Serializable]
    public class ProxyWorkflow : CodeItem
    {
        private string _workflowGroupId;
        private bool _isEnabled;

        /// <summary>
        /// ID of the workflow group to which the workflow belongs
        /// </summary>
        public string WorkflowGroupId { get { return _workflowGroupId; } }

        /// <summary>
        /// Flag indicating whether proxy access may be granted to the workflow
        /// </summary>
        public bool IsEnabled { get { return _isEnabled; } }

        /// <summary>
        /// Worklist category special processing code 
        /// </summary>
        public string WorklistCategorySpecialProcessCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyWorkflow"/> class
        /// </summary>
        /// <param name="code">ID of the proxy workflow</param>
        /// <param name="description">Description of the proxy workflow</param>
        /// <param name="workflowGroupId">ID of the workflow group to which the workflow belongs</param>
        /// <param name="isEnabled">Flag indicating whether proxy access may be granted to the workflow</param>
        public ProxyWorkflow(string code, string description, string workflowGroupId, bool isEnabled) : base(code, description)
        {
            if (string.IsNullOrEmpty(workflowGroupId))
            {
                throw new ArgumentNullException("workflowGroupId", "A proxy workflow must belong to a proxy workflow group.");
            }

            _workflowGroupId = workflowGroupId;
            _isEnabled = isEnabled;
        }
    }
}
