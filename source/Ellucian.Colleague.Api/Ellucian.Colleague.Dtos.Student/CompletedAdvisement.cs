// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A completed advisement for a student for a given date and time by a given advisor
    /// </summary>
    public class CompletedAdvisement
    {
        /// <summary>
        /// Date on which an advisor is marking the student's advisement complete
        /// </summary>
        public DateTime CompletionDate { get; set; }

        /// <summary>
        /// Time of day at which an advisor is marking the student's advisement complete
        /// </summary>
        public DateTimeOffset CompletionTime { get; set; }

        /// <summary>
        /// ID of the advisor who is marking the student's advisement complete
        /// </summary>
        public string AdvisorId { get; set; }
    }
}
