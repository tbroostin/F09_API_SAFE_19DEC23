// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the information necessary to calculate the cost of the
    /// proposed section registration for a person having the given demographic
    /// characteristics
    /// </summary>
    public class InstantEnrollmentProposedRegistration
    {
        /// <summary>
        /// The list of sections for which the student would like to register.  
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseSectionToRegister> ProposedSections { get; set; }

        /// <summary>
        /// The identifier of the person proposing to register.  The person
        /// may not yet be known to the system.  
        /// </summary>
        public string PersonId { get;  set; }

        /// <summary>
        /// The demographic attributes necessary to find or create a new person.
        /// </summary>
        public InstantEnrollmentPersonDemographic PersonDemographic { get;  set;}

        /// <summary>
        /// The academic program of the proposed student.  
        /// </summary>
        public string AcademicProgram { get;  set; }


        /// <summary>
        /// The catalog to use for the the academic program.
        /// </summary>
        public string Catalog { get;  set; }

        /// <summary>
        /// The educational goal for the user.
        /// </summary>
        public string EducationalGoal { get; set; }
    }
}
