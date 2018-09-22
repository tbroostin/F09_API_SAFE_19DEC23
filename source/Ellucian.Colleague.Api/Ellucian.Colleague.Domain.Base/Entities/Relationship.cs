// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Describes the relationship between two entities (people or organizations).  
    /// </summary>
    /// <remarks>
    /// It should be interpreted as 
    /// <para><c>OtherEntity</c> is the <c>RelationshipType</c> for <c>PrimaryEntity</c></para>
    /// </remarks>
    /// 
    [Serializable]
    public class Relationship
    {
        private string _primaryEntity;
        private string _otherEntity;
        private string _relationshipType;
        private string _inverseRelationshipType;
        private bool _isPrimaryRelationship;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _id;

        /// <summary>
        /// Uniqueidentifier
        /// </summary>
        public string Guid { get; set; }


        /// <summary>
        /// Id
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// The entity for whom the relationship is defined (e.g., a student)
        /// </summary>
        public string PrimaryEntity { get { return _primaryEntity; } }

        /// <summary>
        /// The other person in the relationship (e.g., the student's parent)
        /// </summary>
        public string OtherEntity { get { return _otherEntity; } }

        /// <summary>
        /// The code defining the relationship
        /// </summary>
        public string RelationshipType { get { return _relationshipType; } }
        
        /// <summary>
        /// The code defining the relationship
        /// </summary>
        public string InverseRelationshipType { get { return _inverseRelationshipType; } }

        /// <summary>
        /// Indicates if this is the primary relationship with the other entity 
        /// </summary>
        public bool IsPrimaryRelationship
        {
            get { return _isPrimaryRelationship; }
        }

        /// <summary>
        /// The date the relationship began
        /// </summary>
        public DateTime? StartDate { get { return _startDate; } }

        /// <summary>
        /// The date the relationship ended (can be future)
        /// </summary>
        public DateTime? EndDate { get { return _endDate; } }

        /// <summary>
        ///  points to personal-relationship-statuses
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///  points to comments from RELATION
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gender of subject person
        /// </summary>
        public string SubjectPersonGender { get; set; }

        /// <summary>
        /// Guid of subject person
        /// </summary>
        public string SubjectPersonGuid { get; set; }

        /// <summary>
        /// flag for organization
        /// </summary>
        public bool SubjectPersonOrgFlag { get; set; }

        /// <summary>
        /// flag for institution
        /// </summary>
        public bool SubjectPersonInstFlag { get; set; }

         /// <summary>
        /// Gender of related peron to subject
        /// </summary>
        public string RelationPersonGender { get; set; }

        /// <summary>
        /// Guid of related person
        /// </summary>
        public string RelationPersonGuid{ get; set; }

        /// <summary>
        /// flag for organization
        /// </summary>
        public bool RelationPersonOrgFlag { get; set; }

        /// <summary>
        /// flag for institution
        /// </summary>
        public bool RelationPersonInstFlag { get; set; }

        /// <summary>
        /// Indicates if the relationship is currently active
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (_startDate.HasValue && _startDate.Value > DateTime.Today)
                {
                    return false;
                }
                if (_endDate.HasValue && _endDate.Value < DateTime.Today)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class
        /// </summary>
        /// <param name="primaryId">Unique identifier for the entity for whom the relationship is defined (e.g., a student)</param>
        /// <param name="otherId">Unique identifier for the other person in the relationship (e.g., the student's parent)</param>
        /// <param name="relationType">Code defining the relationship</param>
        /// <param name="isPrimaryRelationship">Flag indicating if this is the primary relationship with the other entity </param>
        /// <param name="startDate">Date the relationship began</param>
        /// <param name="endDate">Date the relationship ended</param>
        public Relationship(string primaryId, string otherId, string relationType, bool isPrimaryRelationship, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", "The primary entity must be specified.");
            }

            if (string.IsNullOrEmpty(otherId))
            {
                throw new ArgumentNullException("otherId", "The other entity must be specified.");
            }

            if (string.IsNullOrEmpty(relationType))
            {
                throw new ArgumentNullException("relationType", "The relation type must be specified.");
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentOutOfRangeException("endDate", "The end date cannot be earlier than the start date.");
            }

            _primaryEntity = primaryId;
            _otherEntity = otherId;
            _relationshipType = relationType;
            _isPrimaryRelationship = isPrimaryRelationship;
            _startDate = startDate;
            _endDate = endDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class
        /// </summary>
        /// <param name="primaryId">Unique identifier for the entity for whom the relationship is defined (e.g., a student)</param>
        /// <param name="otherId">Unique identifier for the other person in the relationship (e.g., the student's parent)</param>
        /// <param name="relationType">Code defining the relationship</param>
        /// <param name="isPrimaryRelationship">Flag indicating if this is the primary relationship with the other entity </param>
        /// <param name="startDate">Date the relationship began</param>
        /// <param name="endDate">Date the relationship ended</param>
        public Relationship(string id, string primaryId, string otherId, string relationType, bool isPrimaryRelationship, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", string.Concat("The primary relationship person Id must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }

            if (string.IsNullOrEmpty(otherId))
            {
                throw new ArgumentNullException("otherId", string.Concat("The secondary relationship person Id must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'")); 
            }

            if (string.IsNullOrEmpty(relationType))
            {
                throw new ArgumentNullException("relationType", string.Concat("The relation type must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'")); 
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentOutOfRangeException("endDate", string.Concat("The end date cannot be earlier than the start date. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }
            _id = id;
            _primaryEntity = primaryId;
            _otherEntity = otherId;
            _relationshipType = relationType;
            _isPrimaryRelationship = isPrimaryRelationship;
            _startDate = startDate;
            _endDate = endDate;
        }

        // <summary>
        /// Initializes a new instance of the <see cref="Relationship"/> class
        /// </summary>
        /// <param name="primaryId">Unique identifier for the entity for whom the relationship is defined (e.g., a student)</param>
        /// <param name="otherId">Unique identifier for the other person in the relationship (e.g., the student's parent)</param>
        /// <param name="relationType">Code defining the relationship</param>
        /// <param name="inverseRelType">Code defining the inverse relationship</param>
        /// <param name="isPrimaryRelationship">Flag indicating if this is the primary relationship with the other entity </param>
        /// <param name="startDate">Date the relationship began</param>
        /// <param name="endDate">Date the relationship ended</param>
        public Relationship(string id, string primaryId, string otherId, string relationType, string inverseRelType, bool isPrimaryRelationship, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(primaryId))
            {
                throw new ArgumentNullException("primaryId", string.Concat("The primary relationship person Id must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }

            if (string.IsNullOrEmpty(otherId))
            {
                throw new ArgumentNullException("otherId", string.Concat("The secondary relationship person Id must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }

            if (string.IsNullOrEmpty(relationType))
            {
                throw new ArgumentNullException("relationType", string.Concat("The relation type must be specified. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentOutOfRangeException("endDate", string.Concat("The end date cannot be earlier than the start date. Entity:'RELATIONSHIP', Record ID:'", id, "'"));
            }
            _id = id;
            _inverseRelationshipType = inverseRelType;
            _primaryEntity = primaryId;
            _otherEntity = otherId;
            _relationshipType = relationType;
            _isPrimaryRelationship = isPrimaryRelationship;
            _startDate = startDate;
            _endDate = endDate;
        }


    }
}
