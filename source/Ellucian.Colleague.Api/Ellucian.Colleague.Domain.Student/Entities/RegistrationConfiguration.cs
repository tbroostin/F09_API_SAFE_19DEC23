// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

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
        /// To determine when drop reason is prompted in SelfService then if it is required or not.
        /// </summary>
        public bool RequireDropReason { get; set; }

        /// <summary>
        /// Constructor for RegistrationConfiguration
        /// </summary>
        public RegistrationConfiguration(bool requireFacultyAddAuthorization, int addAuthorizationStartOffsetDays)
        {
            if (addAuthorizationStartOffsetDays < 0)
            {
                throw new ArgumentException("Add authorization offset days cannot be negative.");
            }

            this.RequireFacultyAddAuthorization = requireFacultyAddAuthorization;
            this.AddAuthorizationStartOffsetDays = addAuthorizationStartOffsetDays;
            this.RequireDropReason = false;
            this.PromptForDropReason = false;
        }

    }
}
