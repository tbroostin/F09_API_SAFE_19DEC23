// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Describes the relationship between two entities (people or organizations).  
    /// </summary>
    /// <remarks>
    /// It should be interpreted as 
    /// <para><c>OtherEntity</c> is the <c>RelationshipType</c> for <c>PrimaryEntity</c></para>
    /// </remarks>
    /// 
    public class Relationship
    {
        /// <summary>
        /// The entity for whom the relationship is defined (e.g., a student)
        /// </summary>
        public string PrimaryEntity {get; set;}

        /// <summary>
        /// The other person in the relationship (e.g., the student's parent)
        /// </summary>
        public string OtherEntity { get; set; }

        /// <summary>
        /// The code defining the relationship
        /// </summary>
        public string RelationshipType { get; set; }

        /// <summary>
        /// Indicates if this is the primary relationship with the other entity 
        /// </summary>
        public bool IsPrimaryRelationship { get; set; }

        /// <summary>
        /// The date the relationship began
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date the relationship ended (can be future)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indicates if the relationship is currently active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
