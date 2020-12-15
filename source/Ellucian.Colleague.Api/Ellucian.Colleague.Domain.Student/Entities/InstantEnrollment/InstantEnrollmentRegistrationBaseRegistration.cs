// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the information common to all Instant Enrollment registration endpoints.
    /// Each specific endpoint extends this class as needed.
    /// </summary>
    public abstract class InstantEnrollmentRegistrationBaseRegistration
    {
        /// <summary>
        /// The identifier of the person proposing to register.  The person
        /// may not yet be known to the system.  Not required.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// The demographic attributes necessary to find or create a new person.
        /// </summary>
        public InstantEnrollmentPersonDemographic PersonDemographic { get; private set; }

        /// <summary>
        /// The academic program of the proposed student. 
        /// </summary>
        public string AcademicProgram { get; private set; }

        /// <summary>
        /// The catalog to use for the the academic program.
        /// </summary>
        public string Catalog { get; private set; }

        /// <summary>
        /// The list of sections for which the student would like to register. 
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseSectionToRegister> ProposedSections { get; private set; }

        /// <summary>
        /// The purpose for taking these classes.
        /// </summary>
        public string EducationalGoal { get; set; }

        public InstantEnrollmentRegistrationBaseRegistration(
            string personId,
            InstantEnrollmentPersonDemographic personDemographics,
            string acadProgram,
            string catalog,
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sections)
        {
            // provide either an id or the means to find or create one.
            if (String.IsNullOrEmpty(personId))
            {
                if (personDemographics == null)
                {
                    throw new ArgumentNullException("personId personDemographics", "personId and personDemographics cannot both be null.");
                }

                // Instant Enrollment requires an email address when searching or creating a person id
                if (String.IsNullOrEmpty(personDemographics.EmailAddress))
                {
                    throw new ArgumentException("personDemographics must provide an email address.");
                }

                if (String.IsNullOrEmpty(acadProgram))
                {
                    throw new ArgumentNullException("acadProgram", "Academic Program needs to be provided");
                }

                if (String.IsNullOrEmpty(catalog))
                {
                    throw new ArgumentNullException("catalog", "Catalog needs to be provided");
                }
            }

            if (sections == null || sections.Count == 0)
            {
                throw new ArgumentNullException("sections", "Sections for registeration are required");
            }

            ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();

            PersonId = String.IsNullOrEmpty(personId) ? String.Empty : personId;
            PersonDemographic = personDemographics;
            AcademicProgram = acadProgram;
            ProposedSections.AddRange(sections);
            Catalog = catalog;
        }
    }
}
