// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The result of a registration request as originally requested in  a SectionRegistration object
    /// </summary>
    [Serializable]
    public class SectionRegistrationActionResult
    {
        // COURSE.SECTIONS.ID value
        public string SectionId { get; set; }

        // The registration action that was requested for the section
        public RegistrationAction Action { get; set; }

        // True if the action was successful, else false
        public bool RegistrationActionSuccess { get; set; }
    }
}
