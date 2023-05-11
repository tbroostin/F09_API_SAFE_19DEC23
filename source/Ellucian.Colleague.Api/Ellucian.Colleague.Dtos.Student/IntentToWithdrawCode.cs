// Copyright 2021 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Code used to indicate a student's desire to withdraw from the institution
    /// </summary>
    public class IntentToWithdrawCode
    {
        /// <summary>
        /// Unique identifier for the intent to withdraw
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Code associated with the intent to withdraw
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Human-readable description of the intent to withdraw
        /// </summary>
        public string Description { get; set; }
    }
}
