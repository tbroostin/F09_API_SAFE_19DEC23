// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Profile configuration
    /// </summary>
    public class StudentProfileConfiguration
    {
        /// <summary>
        /// Faculty persona configurations
        /// </summary>
        public StudentProfilePersonConfiguration FacultyPersonConfiguration { get; set; }

        /// <summary>
        /// Advisor persona configurations
        /// </summary>
        public StudentProfilePersonConfiguration AdvisorPersonConfiguration { get; set; }

        /// <summary>
        /// Phone types hierarchy configuration for student
        /// </summary>
        public List<string> PhoneTypesHierarchy { get; set; }

        /// <summary>
        /// Email types hierarchy configuration for student
        /// </summary>
        public List<string> EmailTypesHierarchy { get; set; }

        /// <summary>
        /// Address types hierarchy configuration for student
        /// </summary>
        public List<string> AddressTypesHierarchy { get; set; }

        /// <summary>
        /// Faculty email type 
        /// </summary>
        public string ProfileFacultyEmailType { get; set; }

        /// <summary>
        /// Faculty phone type 
        /// </summary>
        public string ProfileFacultyPhoneType { get; set; }

        /// <summary>
        /// Faculty Advsior type
        /// </summary>
        public string ProfileAdvsiorType { get; set; }
    }
}
