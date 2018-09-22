// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// nonacademic attendance requirement for a timeframe for a person
    /// </summary>
    public class NonAcademicAttendanceRequirement
    {
        /// <summary>
        /// Unique identifier for the requirement
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique identifier for the person to whom the requirement applies
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Code for the academic term for which the requirement applies
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Default number of units the person must earn to satisfy the requirement
        /// </summary>
        public decimal? DefaultRequiredUnits { get; set; }

        /// <summary>
        /// Number of units the person must earn to satisfy the requirement if the <see cref="DefaultRequiredUnits"/> were overridden for the person
        /// </summary>
        public decimal? RequiredUnitsOverride { get; set; }

        /// <summary>
        /// Number of units the person must earn to satisfy the requirement
        /// </summary>
        /// <remarks>This is either the <see cref="DefaultRequiredUnits"/> or the <see cref="RequiredUnitsOverride"/> if it exists</remarks>
        public decimal? RequiredUnits { get; set; }

        /// <summary>
        /// Collection of nonacademic attendance IDs
        /// </summary>
        public IEnumerable<string> NonAcademicAttendanceIds { get; set; }

    }
}
