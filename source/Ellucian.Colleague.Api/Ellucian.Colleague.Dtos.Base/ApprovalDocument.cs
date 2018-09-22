// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An approval document
    /// </summary>
    public class ApprovalDocument
    {
        /// <summary>
        /// Internal ID of the approval document
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The text that is presented to the user for approval
        /// </summary>
        public List<string> Text { get; set; }

        /// <summary>
        /// ID of the person to whom person-specific information (if any) in the approval document applies
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// ApprovalDocument constructor
        /// </summary>
        public ApprovalDocument()
        {
            Text = new List<string>();
        }
    }
}
