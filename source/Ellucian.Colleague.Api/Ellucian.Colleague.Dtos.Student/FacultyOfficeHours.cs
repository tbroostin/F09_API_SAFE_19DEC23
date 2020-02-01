// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
     
    /// <summary>
    /// This entity holds all the office hours information for a faculty
    /// This consists of the data from a all rows in FCTY
    /// </summary>
    public class FacultyOfficeHours
    {
        /// <summary>
        /// The Faculty Id for which the data is relavant
        /// </summary>
        public string facultyId { get; set; }

        /// <summary>
        /// A list to hold various office hours entered for faculty
        /// </summary>
        public List<OfficeHours> OfficeHours { get; set; }

    }
}
