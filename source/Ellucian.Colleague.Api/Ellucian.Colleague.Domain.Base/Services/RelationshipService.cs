// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Services
{
    /// <summary>
    /// Domain service for relationship processing
    /// </summary>
    public static class RelationshipService
    {
        /// <summary>
        /// Builds an inverse relationship from a relationship
        /// </summary>
        /// <param name="relationship">Source relationship</param>
        /// <param name="relationshipTypes">List of relationship types</param>
        /// <returns>Inverse relationship</returns>
        public static Domain.Base.Entities.Relationship CreateInverseRelationship(Domain.Base.Entities.Relationship relationship, IEnumerable<Domain.Base.Entities.RelationshipType> relationshipTypes)
        {
            var lookupCode = relationship.RelationshipType;
            string inv = string.Empty;
            var relType = relationshipTypes.Where(given => lookupCode == given.Code).First();
            if (relType == null)
            {
                var message = string.Format("No matching relationship type for source relationship type {0}.", lookupCode);
                throw new ApplicationException(message);
            }
            return new Domain.Base.Entities.Relationship(relationship.PrimaryEntity, relationship.OtherEntity, relType.InverseCode, relationship.IsPrimaryRelationship, relationship.StartDate, relationship.EndDate);
        }

        /// <summary>
        /// Builds a primary relationship from a non-primary relationship
        /// </summary>
        /// <param name="relationship">Source relationship</param>
        /// <returns>Primary relationship</returns>
        public static Domain.Base.Entities.Relationship CreateRelationship(Domain.Base.Entities.Relationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException("relationship");
            }
            return new Domain.Base.Entities.Relationship(relationship.OtherEntity, relationship.PrimaryEntity, relationship.RelationshipType, relationship.IsPrimaryRelationship, relationship.StartDate, relationship.EndDate);
        }
    }
}
