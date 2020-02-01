// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
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
        /// List of terms for which the Colleague Self-Service Quick Registration workflow may be used
        /// </summary>
        public ReadOnlyCollection<string> QuickRegistrationTermCodes { get; private set; }

        private readonly List<string> _quickRegistrationTermCodes = new List<string>();
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
        }

    }
}
