// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
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
        /// To determine when drop reason is prompted in SelfService then if it is required or not.
        /// </summary>
        public bool RequireDropReason { get; set; }

    }
}
