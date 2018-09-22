using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Statuses pertaining to Term Registration Eligibility information.
    /// </summary>
    [Serializable]
    public enum RegistrationEligibilityTermStatus
    {
        /// <summary>
        /// Registration action or actions is available for this term at this time.
        /// </summary>
        Open,
        /// <summary>
        /// Registration actions will be available at some future point in time.
        /// </summary>
        Future,
        /// <summary>
        /// All registration periods, action times, are in the past.  Too late.
        /// </summary>
        Past,
        /// <summary>
        /// A priority registration item could not be found for this student in this term or they are otherwise just not eligible and don't know when.
        /// </summary>
        NotEligible,
        /// <summary>
        /// An override is available for this student in this term.
        /// </summary>
        HasOverride
    }
}
