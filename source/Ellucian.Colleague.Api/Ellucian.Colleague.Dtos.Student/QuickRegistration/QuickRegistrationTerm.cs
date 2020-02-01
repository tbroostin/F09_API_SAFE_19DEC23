// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.QuickRegistration
{
    /// <summary>
    /// A student's previously selected course sections for a given academic term that may be used in the Colleague Self-Service Quick Registration workflow
    /// </summary>
    public class QuickRegistrationTerm
    {
        /// <summary>
        /// Code of the academic term for which the student previously selected course sections to register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// List of course sections for which the student could potentially register via the Colleague Self-Service Quick Registration workflow
        /// </summary>
        public IEnumerable<QuickRegistrationSection> Sections { get; set; }
    }
}
