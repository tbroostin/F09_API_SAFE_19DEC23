// Copyright 2013-2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The response received from a registration request
    /// </summary>
    public class RegistrationResponse
    {
        /// <summary>
        /// A list of <see cref="RegistrationMessage">registration messages</see>
        /// </summary>
        public List<RegistrationMessage> Messages { get; set; }

        /// <summary>
        /// The ID of the related registration payment control
        /// </summary>
        public string PaymentControlId { get; set; }

        /// <summary>
        /// List of identifiers for course sections for which the student was successfully registered
        /// </summary>
        public List<string> RegisteredSectionIds { get; set; }

    }
}
