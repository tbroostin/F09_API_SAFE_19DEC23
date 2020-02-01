// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.QuickRegistration
{
    /// <summary>
    /// A student's previously identified academic terms and associated course sections that may be used in the Colleague Self-Service Quick Registration workflow
    /// </summary>
    public class StudentQuickRegistration
    {
        /// <summary>
        /// Unique identifier for a student
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// List of academic terms for which the student could potentially register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public IEnumerable<QuickRegistrationTerm> Terms { get; set; }
    }
}
