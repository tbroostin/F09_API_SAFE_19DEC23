// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Code used to indicate a student's desire to withdraw from the institution
    /// </summary>
    [Serializable]
    public class IntentToWithdrawCode
    {
        /// <summary>
        /// Unique identifier for the intent to withdraw
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Code associated with the intent to withdraw
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Human-readable description of the intent to withdraw
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Creates a new <see cref="IntentToWithdrawCode"/>
        /// </summary>
        /// <param name="id">Unique identifier for the intent to withdraw</param>
        /// <param name="code">Code associated with the intent to withdraw</param>
        /// <param name="description">Human-readable description of the intent to withdraw</param>
        public IntentToWithdrawCode(string id, string code, string description)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "An intent to withdraw code must have an ID.");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "An intent to withdraw code must have an code.");
            }
            Id = id;
            Code = code;
            Description = description;
        }
    }
}
