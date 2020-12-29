// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// A DegreePlanArchive is a "snap-shot" of a student's degree plan at a point in time
    /// </summary>
    public class DegreePlanArchive2
    {
        /// <summary>
        /// The record key of the archive
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The record key of the degree plan from which this archive was generated
        /// </summary>
        public int DegreePlanId { get; set; }

        /// <summary>
        /// The record key of the student to whom this archive belongs
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The date/time the archive was created
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The ID of the user who created the archive
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Record key for the last person to complete a review of the plan
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// Timestamp for the last review of the plan
        /// </summary>
        public DateTimeOffset? ReviewedDate { get; set; }
    }
}
