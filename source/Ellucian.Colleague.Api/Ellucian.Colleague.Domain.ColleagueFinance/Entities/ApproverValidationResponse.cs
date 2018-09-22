// Copyright 2018 Ellucian Company L.P. and its affiliates.using System;

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Response from repository related to validating an approver ID.
    /// </summary>
    [Serializable]
    public class ApproverValidationResponse
    {
        /// <summary>
        /// Approver ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The full name for the approver ID.
        /// </summary>
        public string ApproverName { get; set; }

        /// <summary>
        /// Whether the approver is valid or not.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Null if the approver is valid.
        /// If the approver is invalid, contains an
        /// error message that describes the reason why.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes the approver validation response object.
        /// </summary>
        public ApproverValidationResponse(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "The next approver ID must have a value.");
            }
            this.id = id;
        }
    }
}