// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Degree plan review request
    /// </summary>
    public class DegreePlanReviewRequest
    {
        /// <summary>
        /// Degree plan id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Review Requested Student Id(Advisee) 
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Review requested date
        /// </summary>
        public DateTime? ReviewRequestedDate { get; set; }

        /// <summary>
        /// Assigned reviewer
        /// </summary>
        public string AssignedReviewer { get; set; }
    }
}
