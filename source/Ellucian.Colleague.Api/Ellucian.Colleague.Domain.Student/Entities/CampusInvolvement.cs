// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CampusInvolvement
    {
        public string CampusInvolvementId { get; private set; }
        public string PersonId { get; private set; }
        public string CampusOrganizationId { get; private set; }
        public DateTime? StartOn { get; set; }
        public DateTime? EndOn { get; set; }
        public string AcademicPeriodId { get; set; }
        public string RoleId { get; set; }

        /// <summary>
        /// CampusInvolvement entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        /// <param name="campusOrgId"></param>
        public CampusInvolvement(string id, string campusOrgId, string personId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(campusOrgId))
            {
                throw new ArgumentNullException("personId cannot be null or empty");
            }

            CampusInvolvementId = id;
            PersonId = personId;
            CampusOrganizationId = campusOrgId;          
        }
    }
}