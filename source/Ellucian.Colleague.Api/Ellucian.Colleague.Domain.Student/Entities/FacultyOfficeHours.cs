// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;


namespace Ellucian.Colleague.Domain.Student.Entities
{   
    /// <summary>
    /// This entity holds all the office hours information for a faculty
    /// This consists of the data from a all rows in FCTY
    /// </summary>
    [Serializable]
    public class FacultyOfficeHours
    {
        /// <summary>
        /// The Faculty Id for which the data is relavant
        /// </summary>
        public string FacultyId { get; set; }

        /// <summary>
        /// A list to hold various office hours entered for faculty
        /// </summary>
        public List<OfficeHours> OfficeHours { get; set; }       
       
    }
}
