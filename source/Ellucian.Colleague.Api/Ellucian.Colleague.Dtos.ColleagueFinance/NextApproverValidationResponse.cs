// Copyright 2018 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Next approver validation.
    /// </summary>
    public class NextApproverValidationResponse
    {
        /// <summary>
        /// Next approver ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The full name for the next approver ID.
        /// </summary>
        public string NextApproverName { get; set; }

        /// <summary>
        /// Whether the next approver is valid or not.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Null if the next approver is valid.
        /// If the next approver is invalid, contains an
        /// error message that describes the reason why.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}