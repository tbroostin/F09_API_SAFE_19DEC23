// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Dto for Student Overload Petition
    /// </summary>
    public class StudentOverloadPetition
    {
        /// <summary>
        /// A unique identifier for the student petition
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Student this petition belongs to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Status Code - Accepted, Denied, Pending
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Term code associated to the petition
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Date/time this petition was last changed
        /// </summary>
        public DateTimeOffset DateTimeChanged { get; set; }
    }
}
