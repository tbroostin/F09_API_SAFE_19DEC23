// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This Entity will hold the details that needs to be displayed for Departmental Oversight person
    /// </summary>
    public class DepartmentalOversight
    {
        /// <summary>
        /// Id of the Departmental Oversight
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Departmental Oversight last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Departmental Oversight first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Departmental Oversight middle name or initial
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Information that should be used when displaying a Departmental Oversight's name.  
        /// The hierarchy that is used in calculating this name is defined in the Faculty Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }

        /// <summary>
        /// Name of Departmental Oversight as "formatted" to print on external sources such as section details.
        /// </summary>
        public string ProfessionalName { get; set; }
        /// <summary>
        /// Gender of the Departmental Oversight Member
        /// </summary>
        public string Gender { get; set; }


    }
}
