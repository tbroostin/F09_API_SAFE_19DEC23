// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// Student Credit Equivalency
    /// </summary>
    public class Equivalency
    {
        /// <summary>
        /// List of external course work 
        /// </summary>
        public List<ExternalCourseWork> ExternalCourseWork { get; set; }

        /// <summary>
        /// List of external non course work
        /// </summary>
        public List<ExternalNonCourseWork> ExternalNonCourseWork { get; set; }

        /// <summary>
        /// List of institutional course credit equivalencies
        /// </summary>
        public List<EquivalentCourseCredit> EquivalentCourseCredits { get; set; }

        /// <summary>
        /// List of institutionl general credit equivalencies
        /// </summary>
        public List<EquivalentGeneralCredit> EquivalentGeneralCredits { get; set; }

        /// <summary>
        /// List of Academic Programs this equivalency is restricted to.
        /// </summary>
        public List<string> AcademicPrograms { get; set; }
    }
}
