using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CampusOrganization : GuidCodeItem
    {
        /// <summary>
        /// Parent organization to which the campus organization comes under.  This is the global identifier for the Parent Organization.
        /// </summary>
        public string ParentOrganizationId { get; set; }

        /// <summary>
        /// Type of the organization (eg: athletic, culture).  This is the global identifier for the Type.
        /// </summary>
        public string CampusOrganizationTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="parentOrganizationId"></param>
        /// <param name="typeId"></param>
        public CampusOrganization(string campusOrgId, string guid, string campusOrganizationName, string parentOrganizationId = "", string campusOrganizationTypeId = "")
            : base(guid, campusOrgId, campusOrganizationName)
        {
            ParentOrganizationId = parentOrganizationId;
            CampusOrganizationTypeId = campusOrganizationTypeId;
        }
    }
}
