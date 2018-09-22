// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to query a set of event ids 
    /// </summary>
    public class NonAcademicEventQueryCriteria
    {
        /// <summary>
        /// Ids of events
        /// </summary>
        public IEnumerable<string> EventIds { get; set; }
    }
}
