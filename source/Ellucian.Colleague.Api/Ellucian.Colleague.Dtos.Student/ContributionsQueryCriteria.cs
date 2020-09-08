// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Query request for retention alert my contributions
    /// </summary>
    public class ContributionsQueryCriteria
    {
        /// <summary>
        /// Gets or sets a value indicating whether [include closed cases].
        /// </summary>
        public bool IncludeClosedCases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include owned cases].
        /// </summary>
        public bool IncludeOwnedCases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include cases over one year].
        /// </summary>
        public bool IncludeCasesOverOneYear { get; set; }
    }
}
