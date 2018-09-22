// Copyright 2013 - 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// A single requirement for a program. May alternatively be used as a course requisite. Cannot be both.
    /// </summary>
    public class Requirement
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Human-readable code for this requirement
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Text of this requirement, for display on evaluation reports
        /// </summary>
        public string DisplayText { get; set; }
        /// <summary>
        /// Minimum GPA required of all the coursework taken to satisfy the subrequirements attached to this requirement
        /// </summary>
        public decimal? MinGpa { get; set; }
        /// <summary>
        /// Number of subrequirements that must be satisfied from the list of subrequirements attached to this requirement
        /// </summary>
        public int? MinSubRequirements { get; set; }
        /// <summary>
        /// List of subrequirements
        /// <see cref="Subrequirement"/>
        /// </summary>
        public List<Subrequirement> Subrequirements { get; set; }
        /// <summary>
        /// Minimum institutional credits required of all the coursework taken to satisfy the subrequirements attached to this requirement
        /// </summary>
        public decimal? MinInstitutionalCredits { get; set; }
    }
}
