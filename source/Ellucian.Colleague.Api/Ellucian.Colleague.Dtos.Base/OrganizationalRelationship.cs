// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Representation of a relationship between organizational person positions.
    /// </summary>
    public class OrganizationalRelationship
    {
        /// <summary>
        /// The unique Id of the organizational relationship. May be empty for a new relationship.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Id of the primary organizational person position
        /// </summary>
        public string OrganizationalPersonPositionId { get; set; }

        /// <summary>
        /// Id of the person assigned to the organizational person position
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Name of the person assigned to the organizational person position
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Id of the position 
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// Title of the position
        /// </summary>
        public string PositionTitle { get; set; }

        /// <summary>
        /// Status of the Organizational Person Position
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public OrganizationalPersonPositionStatus OrganizationalPersonPositionStatus { get; set; }

        /// <summary>
        /// Id of the related organizational person position
        /// </summary>
        public string RelatedOrganizationalPersonPositionId { get; set; }

        /// <summary>
        /// Id of the person assigned to the related organizational person position
        /// </summary>
        public string RelatedPersonId { get; set; }

        /// <summary>
        /// Name of the person assigned to the related organizational person position
        /// </summary>
        public string RelatedPersonName { get; set; }

        /// <summary>
        /// Id of the related position 
        /// </summary>
        public string RelatedPositionId { get; set; }

        /// <summary>
        /// Title of the related position
        /// </summary>
        public string RelatedPositionTitle { get; set; }

        /// <summary>
        /// Status of the Related Organizational Person Position
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public OrganizationalPersonPositionStatus RelatedOrganizationalPersonPositionStatus { get; set; }

        /// <summary>
        /// The nature of the relationship, as defined by the client. May be a manager, or time approver, for example.
        /// </summary>
        public string Category { get; set; }
    }
}
