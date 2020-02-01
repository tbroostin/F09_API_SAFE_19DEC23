// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria for campus organization queries
    /// </summary>
    public class CampusOrganizationQueryCriteria
    {
        /// <summary>
        /// Campus Organization Ids for which the  CampusOrganization2 data is requested. 
        /// Must provide at least one Campus Organization Id to query CampusOrganization2 records.
        /// </summary>
        public List<string> CampusOrganizationIds { get; set; }
    }
}
