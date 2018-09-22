// Copyright 2015 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A workflow to which proxy access can be granted
    /// </summary>
    public class ProxyWorkflow
    {
        /// <summary>
        /// ID of the workflow
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the workflow
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID of the workflow group to which the workflow belongs
        /// </summary>
        public string WorkflowGroupId { get; set; }

        /// <summary>
        /// Flag indicating whether proxy access may be granted to the workflow
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Worklist category special processing co
        /// </summary>
        public string WorklistCategorySpecialProcessCode { get; set; }
    }
}
