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
    public class StudentRestrictionsQueryCriteria
    {
        /// <summary>
        /// List of Restriction Keys for retrieval by keys.
        /// </summary>
        public IEnumerable<string> Ids { get; set; }
        /// <summary>
        /// List of Student Ids for retrieval of multiple records.
        /// </summary>
        public IEnumerable<string> StudentIds { get; set; }
    }
}