// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Base;
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information needed for a StudentPlanning student
    /// </summary>
    public class PlanningStudent 
    {
        /// <summary>
        ///  Unique system ID of this person
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Person's last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Person's first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Person's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The Id of this student's degree plan
        /// </summary>
        public int? DegreePlanId { get; set; }

        /// <summary>
        /// Gets a list of the student's Academic Program Ids.
        /// </summary>
        public List<string> ProgramIds { get; set; }

        /// <summary>
        /// Indicates whether student has an assigned advisor
        /// </summary>
        public bool HasAdvisor { get; set; }
        
        /// <summary>
        /// Preferred email address of student
        /// </summary>
        public string PreferredEmailAddress { get; set; }
 
        /// <summary>
        /// List of advisements for the student (advisors, advisor types, start dates, end dates)
        /// </summary>
        public IEnumerable<Advisement> Advisements { get; set; }

        /// <summary>
        /// List of advisor Ids who have this student assigned as an advisee
        /// </summary>
        public List<string> AdvisorIds { get; set; }

        /// <summary>
        /// Privacy status code
        /// </summary>
        public string PrivacyStatusCode { get; set; }

        /// <summary>
        /// Information that should be used when displaying a student's name.  
        /// The hierarchy that is used in calculating this name is defined in the Student Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }

        /// <summary>
        /// Personal Pronoun Code indicating person's preferred manner of address
        /// </summary>
        public string PersonalPronounCode { get; set; }

        /// <summary>
        /// Phonetypes hierarchy for student profile
        /// </summary>
        public List<string> PhoneTypesHierarchy { get; set; }

        /// <summary>
        /// All email address of student
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// List of completed advisements for the student for a given date and time by a given advisor
        /// </summary>
        public IEnumerable<CompletedAdvisement> CompletedAdvisements { get; set; }
    }
}
