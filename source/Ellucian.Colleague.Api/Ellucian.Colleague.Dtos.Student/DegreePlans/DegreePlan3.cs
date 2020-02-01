﻿// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// All information related to a degree plan (aka, course plan)
    /// </summary>
    public class DegreePlan3
    {
        /// <summary>
        /// Unique system ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Student this plan belongs to
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Latest version number associated to this degree plan. Used to control updates to the degree plan.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// List of terms and planned courses <see cref="DegreePlanTerm"/>
        /// </summary>
        public List<DegreePlanTerm3> Terms { get; set; }

        /// <summary>
        /// List of planned course sections that are defined by date instead of term
        /// <see cref="PlannedCourse"/>
        /// </summary>
        public List<PlannedCourse3> NonTermPlannedCourses { get; set; }

        /// <summary>
        /// Most recent status of each planned course that has been approved or denied.
        /// <see cref="DegreePlanApproval"/>
        /// </summary>
        public List<DegreePlanApproval2> Approvals { get; set; }

        /// <summary>
        /// History of Notes entered by student and advisor(s)
        /// <see cref="DegreePlanNote"/>
        /// </summary>
        public List<DegreePlanNote2> Notes { get; set; }

        /// <summary>
        /// Indicates whether the student has requested review by the advisor
        /// </summary>
        public bool ReviewRequested { get; set; }

        /// <summary>
        /// Date the plan was last reviewed.
        /// </summary>
        public DateTime? LastReviewedDate { get; set; }

        /// <summary>
        /// ID of the advisor who last reviewed the plan.
        /// </summary>
        public string LastReviewedAdvisorId { get; set; }
    }
}
