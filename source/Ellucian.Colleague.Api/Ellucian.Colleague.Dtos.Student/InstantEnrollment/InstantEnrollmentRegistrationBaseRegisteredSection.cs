using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the section and related information for an Instant Enrollment section in which an 
    /// individual has successfully enrolled.
    /// </summary>
    public class InstantEnrollmentRegistrationBaseRegisteredSection
    {
        /// <summary>
        /// The identifier of the section of interest.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The cost of the section.
        /// </summary>
        public decimal SectionCost { get; set; }
    }
}
