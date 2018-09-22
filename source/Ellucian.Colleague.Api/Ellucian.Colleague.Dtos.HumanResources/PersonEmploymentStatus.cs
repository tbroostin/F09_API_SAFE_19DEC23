/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Data transfer object for Person Employment Status.
    /// This object contains information regarding an employees status between a start and end date,
    /// and primary position information
    /// </summary>
    public class PersonEmploymentStatus
    {
        /// <summary>
        /// The identifier of this record
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The identifier of the associated person
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// The identifier of the associated position, which is primary
        /// </summary>
        public string PrimaryPositionId { get; set; }
        /// <summary>
        /// The start date of this person status record
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The identifier of the associated position
        /// </summary>
        public string PersonPositionId { get; set; }
        /// <summary>
        /// The end date of this person status record
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
