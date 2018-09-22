using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student registration eligibility information specific to a term 
    /// </summary>
    public class RegistrationEligibilityTerm
    {
        /// <summary>
        /// Term for which the registration priority information pertains
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Any messages as a result of term rules
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Date and time when the student will be authorized to perform "add" type of registration actions  
        /// </summary>
        public DateTimeOffset? AnticipatedTimeForAdds { get; set; }

        /// <summary>
        /// Status of the term's overall eligibility. Indicates whether registration actions are currently available, up coming, in the past or information is missing.
        /// </summary>
        public RegistrationEligibilityTermStatus Status { get; set; }

        /// <summary>
        /// Indicates whether or not this user is allowed to skip the waitlist
        /// </summary>
        public bool SkipWaitlistAllowed { get; set; }

        /// <summary>
        /// If true it means that the student either has a registration priority for the term and its dates are not currently active, or 
        /// the term is set up to check for registration priorities but the student does not have one.  Otherwise this is false.
        /// In either case, the student will be prevented from registering for any section in this term.
        /// </summary>
        public bool FailedRegistrationPriorities { get; set; }

        /// <summary>
        /// If true it means that this term has Term Registration Rules defined and the student either doesn't pass any (as of today)
        /// or the student passes a rule but the rule dates are not open as of today.  In either case the student will be prevented from registering
        /// for any section in this term.
        /// </summary>
        public bool FailedRegistrationTermRules { get; set; }
    }

    
}
