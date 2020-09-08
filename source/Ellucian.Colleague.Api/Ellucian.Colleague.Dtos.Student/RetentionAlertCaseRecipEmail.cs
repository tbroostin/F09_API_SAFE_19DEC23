// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of retention alert case recip email
    /// </summary>
    public class RetentionAlertCaseRecipEmail
    {
        /// <summary>
        /// Person's Name associated with the email address for the Retention Alert Case
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Relationship to the Retention Alert Case
        /// </summary>
        public string Relationship { get; set; }

        /// <summary>
        /// Email Address
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
