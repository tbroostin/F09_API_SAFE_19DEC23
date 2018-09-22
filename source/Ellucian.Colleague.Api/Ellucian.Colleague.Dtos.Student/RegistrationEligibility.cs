using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Indicates eligibility of a student to register and whether the current user has override ability.
    /// </summary>
    public class RegistrationEligibility
    {
        /// <summary>
        /// List of <see cref="RegistrationMessage">RegistrationMessages</see> returned regarding eligibility.
        /// </summary>
        public IEnumerable<RegistrationMessage> Messages { get; set; }
        /// <summary>
        /// Boolean indicates if the student is eligible for registration
        /// </summary>
        public bool IsEligible { get; set; }
        /// <summary>
        /// Boolean indicates if the user has permissions to override and perform registration actions for the student, even if not eligible.
        /// </summary>
        public bool HasOverride { get; set; }
        /// <summary>
        /// Term specific registration eligibility information specific to a student
        /// </summary>
        public IEnumerable<RegistrationEligibilityTerm> Terms { get; set; }
    }
}
