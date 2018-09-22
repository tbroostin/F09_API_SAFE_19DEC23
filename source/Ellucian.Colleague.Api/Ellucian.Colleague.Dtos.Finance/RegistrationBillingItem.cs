// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Record of a student's attempted registration for a course section
    /// </summary>
    public class RegistrationBillingItem
    {
        /// <summary>
        /// ID of the course section registration record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The course section for which the student attempted to register
        /// </summary>
        public Section AcademicCredit { get; set; }
    }
}
