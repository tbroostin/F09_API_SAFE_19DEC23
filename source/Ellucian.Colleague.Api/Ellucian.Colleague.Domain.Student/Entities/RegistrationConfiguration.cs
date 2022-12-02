// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains information that controls how registration processing should be rendered in self-service.
    /// </summary>
    [Serializable]
    public class RegistrationConfiguration
    {
        /// <summary>
        /// Indicates whether or not Faculty Add Authorization is required during the registration add period.
        /// </summary>
        public bool RequireFacultyAddAuthorization { get; set; }

        /// <summary>
        /// The number of days from the start of the section add period when faculty add authorization is required.
        /// Only applicable if RequireFacultyAddAuthorization is true.
        /// </summary>
        public int AddAuthorizationStartOffsetDays { get; set; }

        /// <summary>
        /// Prompt for drop reason in SelfService.
        /// </summary>
        public bool PromptForDropReason { get; set; }

        /// <summary>
        /// To determine when drop reason is prompted in Colleague Self-Service then if it is required or not.
        /// </summary>
        public bool RequireDropReason { get; set; }

        /// <summary>
        /// Show course section book information on printed schedules in Colleague Self-Service
        /// </summary>
        public bool ShowBooksOnPrintedSchedules { get; set; }

        /// <summary>
        /// Show course section additional information on printed schedules in Colleague Self-Service
        /// </summary>
        public bool ShowCommentsOnPrintedSchedules { get; set; }

        /// <summary>
        /// Add academic terms with Default on New Course Plans = Yes when creating a new Degree Plan
        /// </summary>
        public bool AddDefaultTermsToDegreePlan { get; set; }

        /// <summary>
        /// Flag indicating whether or not the Colleague Self-Service Quick Registration workflow is enabled
        /// </summary>
        public bool QuickRegistrationIsEnabled { get; private set; }

        /// <summary>
        /// Indicates whether or not Faculty can add authorization from the waitlist
        /// </summary>
        public bool AllowFacultyAddAuthFromWaitlist { get; set; }

        /// <summary>
        /// List of terms for which the Colleague Self-Service Quick Registration workflow may be used
        /// </summary>
        public ReadOnlyCollection<string> QuickRegistrationTermCodes { get; private set; }

        private readonly List<string> _quickRegistrationTermCodes = new List<string>();

        /// <summary>
        /// Flag indicating whether or not to *always* present a prompt to Self-Service users when dropping course sections, inquiring if the student intends to withdraw from the institution
        /// </summary>
        public bool AlwaysPromptUsersForIntentToWithdrawWhenDropping 
        {
            get { return _alwaysPromptUsersForIntentToWithdrawWhenDropping; }
            set
            {
                if (value && CensusDateNumberForPromptingIntentToWithdraw.HasValue)
                {
                    throw new ArgumentException("Intent to withdraw prompt can only be configured to show (a) always, or (b) after a census date number, but not both.");
                }
                _alwaysPromptUsersForIntentToWithdrawWhenDropping = value;
            }
        }
        private bool _alwaysPromptUsersForIntentToWithdrawWhenDropping;

        /// <summary>
        /// Numeric position of the census date to check when deciding whether or not to present a prompt to Self-Service users when dropping course sections, inquiring if the student intends to withdraw from the institution; Today's date must be on or after the census date at the specified position for the course section being dropped
        /// </summary>
        public int? CensusDateNumberForPromptingIntentToWithdraw
        {
            get { return _censusDateNumberForPromptingIntentToWithdraw; }
            set
            {
                if (value.HasValue && AlwaysPromptUsersForIntentToWithdrawWhenDropping)
                {
                    throw new ArgumentException("Intent to withdraw prompt can only be configured to show (a) always, or (b) after a census date number, but not both.");
                }
                if (value.HasValue && value < 1)
                {
                    throw new ArgumentOutOfRangeException("Census date numbers must be greater than or equal to 1. Cannot present intent to withdraw prompt only after a census date number when that number is not valid.");
                }
                _censusDateNumberForPromptingIntentToWithdraw = value;
            }
        }
        private int? _censusDateNumberForPromptingIntentToWithdraw;

        /// <summary>
        /// Adds a quick registration term to the registration configuration object.
        /// </summary>
        /// <param name="termCode">Term Code</param>
        public void AddQuickRegistrationTerm(string termCode)
        {
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termId", "A term code is required when adding a quick registration term.");
            }
            if (!this.QuickRegistrationIsEnabled)
            {
                throw new ApplicationException("Cannot add quick registration terms when quick registration is disabled.");
            }
            if (!_quickRegistrationTermCodes.Contains(termCode))
            {
                _quickRegistrationTermCodes.Add(termCode);
            }
        }

        /// <summary>
        /// Constructor for RegistrationConfiguration
        /// </summary>
        public RegistrationConfiguration(bool requireFacultyAddAuthorization, int addAuthorizationStartOffsetDays, bool quickRegistrationIsEnabled = false)
        {
            if (addAuthorizationStartOffsetDays < 0)
            {
                throw new ArgumentException("Add authorization offset days cannot be negative.");
            }

            this.RequireFacultyAddAuthorization = requireFacultyAddAuthorization;
            this.AddAuthorizationStartOffsetDays = addAuthorizationStartOffsetDays;
            this.RequireDropReason = false;
            this.PromptForDropReason = false;
            this.ShowBooksOnPrintedSchedules = false;
            this.ShowCommentsOnPrintedSchedules = false;
            this.AddDefaultTermsToDegreePlan = true;
            this.QuickRegistrationIsEnabled = quickRegistrationIsEnabled;
            this.QuickRegistrationTermCodes = _quickRegistrationTermCodes.AsReadOnly();
            this.CensusDateNumberForPromptingIntentToWithdraw = null;
            this.AlwaysPromptUsersForIntentToWithdrawWhenDropping = false;
        }
    }
}
