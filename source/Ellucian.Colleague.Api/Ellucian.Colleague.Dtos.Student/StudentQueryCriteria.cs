// Copyright 2014 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// DTO for incoming JSON formatted student query
    /// </summary>
    public class StudentQueryCriteria
    {
        /// <summary>
        /// List of Student Ids for retrieval of multiple records.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
        /// <summary>
        /// If flag is set, then Students entity will inherit from the Person Entity
        /// </summary>
        public bool InheritFromPerson { get; set; }
        /// <summary>
        /// If flag is set, then a Degree Plan ID will be returned in the DTO.
        /// </summary>
        public bool GetDegreePlan { get; set; }
        /// <summary>
        /// If term is set, return primary home location, advisements, and advisor IDs
        /// based on term.
        /// </summary>
        public string Term { get; set; }
    }
}