// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Middle tier of the hierarchy of requirements. Listed within Requirement object.
    /// </summary>
    public class Subrequirement
    {
        /// <summary>
        /// Unique system Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Human-readable unique code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Minimum number of groups listed within this subrequirement that must be satisfied to complete
        /// this subrequirement.
        /// </summary>
        public int? MinGroups { get; set; }
        /// <summary>
        /// List of groups for this subrequirement
        /// <see cref="Group"/>
        /// </summary>
        public List<Group> Groups { get; set; }
        /// <summary>
        /// Text to display on the evaluation report for this subrequirement
        /// </summary>
        public string DisplayText { get; set; }
        /// <summary>
        /// Minimum GPA required for the sum of courses that satisfy the groups of this subrequirement
        /// </summary>
        public decimal? MinGpa { get; set; }
        /// <summary>
        /// Minimum institutional credits for the sum of courses that satisfy the groups of this subrequirement
        /// </summary>
        public decimal? MinInstitutionalCredits { get; set; }
        /// <summary>
        /// Flag indicating whether or not this block is used exclusively to convey print text
        /// </summary>
        public bool OnlyConveysPrintText { get; set; }
    }
}