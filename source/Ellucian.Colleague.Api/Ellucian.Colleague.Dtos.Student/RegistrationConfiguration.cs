// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains information that controls how registration processing should be tailored for a user in self-service.
    /// </summary>
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
        /// Flag indicating whether or not the Colleague Self-Service Quick Registration workflow is enabled
        /// </summary>
        public bool QuickRegistrationIsEnabled { get; set; }

        /// <summary>
        /// List of terms for which the Colleague Self-Service Quick Registration workflow may be used
        /// </summary>
        public IEnumerable<string> QuickRegistrationTermCodes { get; set; }

        /// <summary>
        /// Indicates whether or not Faculty can add authorization from the waitlist
        /// </summary>
        public bool AllowFacultyAddAuthFromWaitlist { get; set; }

        /// <summary>
        /// Flag indicating whether or not to *always* present a prompt to Self-Service users when dropping course sections, inquiring if the student intends to withdraw from the institution
        /// </summary>
        public bool AlwaysPromptUsersForIntentToWithdrawWhenDropping { get; set; }

        /// <summary>
        /// Numeric position of the census date to check when deciding whether or not to present a prompt to Self-Service users when dropping course sections, inquiring if the student intends to withdraw from the institution; today's date must be on or after the census date at the specified position for the course section being dropped
        /// </summary>
        public int? CensusDateNumberForPromptingIntentToWithdraw { get; set; }
    }
}
