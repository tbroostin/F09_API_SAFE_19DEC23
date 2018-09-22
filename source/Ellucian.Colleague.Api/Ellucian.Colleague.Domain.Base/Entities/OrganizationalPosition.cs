/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Organizational Position
    /// </summary>
    [Serializable]
    public class OrganizationalPosition
    {
        /// <summary>
        /// Organizational Position Id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Organizational Position Title
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Organizational Position Relationships
        /// </summary>
        public IEnumerable<OrganizationalPositionRelationship> Relationships { get { return relationships; } }
        private List<OrganizationalPositionRelationship> relationships;

        /// <summary>
        /// Add an Organizational Position Relationship to this Organizational Position
        /// </summary>
        /// <param name="orgPosRel"></param>
        public void AddPositionRelationship(OrganizationalPositionRelationship orgPosRel)
        {
            if (orgPosRel != null)
            {
                relationships.Add(orgPosRel);
            }
        }

        /// <summary>
        /// Organizational position constructor
        /// </summary>
        /// <param name="id">Organizational position id</param>
        /// <param name="title">Organizational position title</param>
        public OrganizationalPosition(string id, string title)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "id cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "title cannot be null or empty.");
            }
            Id = id;
            Title = title;
            relationships = new List<OrganizationalPositionRelationship>();
        }
    }

}
