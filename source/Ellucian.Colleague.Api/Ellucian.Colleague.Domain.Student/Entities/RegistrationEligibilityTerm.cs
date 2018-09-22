using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Information specific to a student's registration eligibility in a given term.
    /// </summary>
    [Serializable]
    public class RegistrationEligibilityTerm
    {

        /// <summary>
        /// Term for which the registration priority information pertains.  Currently this is required. Non-term priorities are not being pulled into the domain.
        /// </summary>
        private readonly string _termCode;
        public string TermCode { get { return _termCode; } }

        /// <summary>
        /// Only used in the registration priority calculation. This flag essentially comes from the underlying Registration Controls and Registration Users parameters
        /// </summary>
        private readonly bool _checkPriority;
        public bool CheckPriority { get { return _checkPriority; } }

        /// <summary>
        /// Only used in the registration priority calculation. This flag essentially comes from the underlying Registration Controls and Registration Users parameters
        /// </summary>
        private readonly bool _priorityOverridable;
        public bool PriorityOverridable { get { return _priorityOverridable; } }

        /// <summary>
        /// Any message as a result of term rules or registration priority validation
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Date and time when the student is authorized to perform "add" type of registration actions. 
        /// This could be based on registration priority or it could be based on term registration rules 
        /// or it could just be when the next registration period opens for the term.
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

        public RegistrationEligibilityTerm(string termCode, bool checkPriority, bool priorityOverridable)
        {
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term is required for an registration eligibility term.");
            }
            _termCode = termCode;
            _checkPriority = checkPriority;
            _priorityOverridable = priorityOverridable;
        }



        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            RegistrationEligibilityTerm other = obj as RegistrationEligibilityTerm;
            if (other == null)
            {
                return false;
            }
            return other.TermCode.Equals(_termCode);
        }

        public override int GetHashCode()
        {
            return _termCode.GetHashCode();
        }

    }
}
