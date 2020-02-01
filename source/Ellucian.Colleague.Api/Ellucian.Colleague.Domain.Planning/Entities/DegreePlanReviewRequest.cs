// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Degree plan review request
    /// </summary>
    [Serializable]
    public class DegreePlanReviewRequest
    {
        /// <summary>
        /// Degree plan id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Person id
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
