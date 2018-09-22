// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Relation Type
    /// </summary>
    [Serializable]
    public class RelationType : GuidCodeItem
    {
        public PersonalRelationshipType? PersonRelType { get; private set; }

        public string OrgIndicator { get; private set; }

        public PersonalRelationshipType? MaleRelType { get; private set; }

        public PersonalRelationshipType? FemaleRelType { get; private set; }

        public string InverseRelType { get; private set; }

        public string Category { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="personRelType">Personal Relationship Type of the RelationType</param>
        public RelationType(string guid, string code, string description, string orgIndicator, PersonalRelationshipType? personRelType)
            : base (guid, code, description)
        {
            PersonRelType = personRelType;
            OrgIndicator = orgIndicator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="inverseRelType">Inverse Personal Relationship Type of the RelationType</param>
        public RelationType(string guid, string code, string description, string orgIndicator, string inverseRelType)
            : base(guid, code, description)
        {
            InverseRelType = inverseRelType;
            OrgIndicator = orgIndicator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="inverseRelType">Inverse Personal Relationship Type of the RelationType</param>
        public RelationType(string guid, string code, string description, string orgIndicator, string inverseRelType, string category)
            : base(guid, code, description)
        {
            InverseRelType = inverseRelType;
            OrgIndicator = orgIndicator;
            Category = category;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="description">Description or Title of the RelationType</param>
        /// <param name="personRelType">Personal Relationship Type of the RelationType</param>
        public RelationType(string guid, string code, string description, string orgIndicator,
            PersonalRelationshipType? personRelType, PersonalRelationshipType? maleRelType,
            PersonalRelationshipType? femaleRelType, string inverseRelType)
            : base(guid, code, description)
        {
            PersonRelType = personRelType;
            OrgIndicator = orgIndicator;
            MaleRelType = maleRelType;
            FemaleRelType = femaleRelType;
            InverseRelType = inverseRelType;
        }


    }
}
