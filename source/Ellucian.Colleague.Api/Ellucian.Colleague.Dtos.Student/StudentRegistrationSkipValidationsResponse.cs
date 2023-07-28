// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The response received from a skip validations registration request
    /// </summary>
    public class StudentRegistrationSkipValidationsResponse
    {
        /// <summary>
        /// A list of <see cref="RegistrationMessage"></see> registration messages. 
        /// These messages are designed to be displayed to a person who will interpret them, but are not designed
        /// to be consumed by software. Each message may reflect an error or may be only informational or a warning.
        /// Each message also may or may not be associated with a section ID. The section ID is not necessarily the ID of
        /// a section for which a registration action executed. For instance a drop of a section is requested but fails
        /// because it is a required corequisite of another section, the associated message will contain the section ID
        /// of the other section.
        /// </summary>
        public List<RegistrationMessage> Messages { get; set; }

        /// <summary>
        /// List of section action responses, one for each section action that was passed with the request
        /// </summary>
        public List<SectionRegistrationActionResponse> SectionActionResponses { get; set; }

        /// <summary>
        /// The ID of the related registration payment control if applicable. 
        /// </summary>
        public string PaymentControlId { get; set; }

    }
}
