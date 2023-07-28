// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Registration priority information specific to a student
    /// </summary>
    public class RegistrationPriority
    {
        /// <summary>
        /// Unique Id of the student registration priority
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Id of the student for which the registration priority applies
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Term id for which the priority applies.
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Starting date and time for the priority.
        /// </summary>
        public DateTimeOffset? Start { get; set; }

        /// <summary>
        /// Ending Date and time for the priority. Can be blank.
        /// </summary>
        public DateTimeOffset? End { get; set; }
    }
}
