using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates whether an academic credit is a replacement for another academic credit, or will possibly replace it in the future.
    /// </summary>
    [Serializable]
    public enum ReplacementStatus
    {
        /// <summary>
        /// This academic credit is not a replacement for any other academic credit
        /// </summary>
        NotReplacement,
        /// <summary>
        /// This academic credit is a replacement for another academic credit
        /// </summary>
        Replacement,
        /// <summary>
        /// This academic credit may replace another academic credit when this credit is completed
        /// </summary>
        PossibleReplacement
    }
}
