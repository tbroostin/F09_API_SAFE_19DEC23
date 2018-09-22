// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Dto for faculty section permission
    /// </summary>
    public class SectionPermission
    {
        /// <summary>
        /// Section Id for which this set of permission applies.
        /// </summary>
        public string SectionId { get; set; }
        
        /// <summary>
        /// A collection of student petitions
        /// </summary>
        public List<StudentPetition> StudentPetitions { get; set; }

        /// <summary>
        ///  A collection of faculty consents
        /// </summary>
        public List<StudentPetition> FacultyConsents { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SectionPermission()
        {
            StudentPetitions = new List<StudentPetition>();
            FacultyConsents = new List<StudentPetition>();
        }
    }
}
