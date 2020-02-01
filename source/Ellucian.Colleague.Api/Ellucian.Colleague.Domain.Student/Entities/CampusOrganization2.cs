// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A CampusOrganization2 is a sub set of the CampusOrganization object.
    /// </summary>
    [Serializable]
    public class CampusOrganization2
    {
        /// <summary>
        /// The CampusOrganization Id of the object
        /// </summary>
        public string CampusOrganizationId { get { return campusOrganizationId; } }
        private readonly string campusOrganizationId;

        /// <summary>
        /// The CampusOrganization Description of the object
        /// </summary>
        public string CampusOrganizationDescription { get { return campusOrganizationDescription; } }
        private readonly string campusOrganizationDescription;

        /// <summary>
        /// Build a CampusOrganization2 object
        /// </summary>
        /// <param name="campusOrganizationId"></param>
        /// <param name="campusOrganizationDescription"></param>
        public CampusOrganization2(string campusOrganizationId, string campusOrganizationDescription)
        {
            if (string.IsNullOrEmpty(campusOrganizationId))
            {
                throw new ArgumentNullException("campusOrganizationId");
            }
            if (string.IsNullOrEmpty(campusOrganizationDescription))
            {
                throw new ArgumentNullException("campusOrganizationDescription");
            }

            this.campusOrganizationId = campusOrganizationId;
            this.campusOrganizationDescription = campusOrganizationDescription;
        }       
    }
}
