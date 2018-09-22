using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Statuses pertaining to Term Registration Eligibility information.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RegistrationEligibilityTermStatus
    {
        /// <summary>
        /// A registration action or actions is available for this term at this time.
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
        /// A priority registration item could not be found for this student in this term or they are otherwise just not eligible.
        /// </summary>
        NotEligible,
        /// <summary>
        /// An override is available for this student in this term.
        /// </summary>
        HasOverride
    }
}
