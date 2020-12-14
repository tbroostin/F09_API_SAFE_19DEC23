// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Contains the section and demographic information necessary to create a student,
    /// and register for classes when the total cost is zero.
    /// </summary>
    [Serializable]
    public class InstantEnrollmentZeroCostRegistration : InstantEnrollmentRegistrationBaseRegistration
    {
        public InstantEnrollmentZeroCostRegistration(
            string personId,
            InstantEnrollmentPersonDemographic personDemographics,
            string acadProgram,
            string catalog,
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sections) :
               base(personId, personDemographics, acadProgram, catalog, sections)
        {
        }
    }
}
