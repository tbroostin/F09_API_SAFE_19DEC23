// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Returns the registration options for the given person, which may be a student or advisor
    /// </summary>
    public class RegistrationOptions
    {
        /// <summary>
        /// The Id of the person to whom these registration options belong.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The list of grading types this person is allowed to used in registration.
        /// </summary>
        public IEnumerable<GradingType> GradingTypes { get; set; }
    }
}
