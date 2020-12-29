/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Organizations involved in extra curricular activities.
    /// </summary>
    public class CampusOrganization2
    {
        /// <summary>
        /// The CampusOrganization Id of the object
        /// </summary>
        public string CampusOrganizationId { get; set; }

        /// <summary>
        /// The CampusOrganization Description of the object
        /// </summary>
        public string CampusOrganizationDescription { get; set; }
    }
}
