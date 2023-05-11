// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This Entity will hold the details that needs to be displayed for Departmental Oversight person
    /// </summary>
    [Serializable]
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
        /// This is the name the departmental oversight member has indicated should be used in student-facing software.
        /// This is not guaranteed to have a value - it is up to the consumer of this entity to determine which
        /// name is appropriate to use based on context.
        /// </summary>
        public string ProfessionalName { get; set; }

        /// <summary>
        /// Name that should be used when displaying a person's name on reports and forms.
        /// This property is based on a Name Address Hierarcy and will be null if none is provided.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }
        /// <summary>
        /// Gender of the Departmental Oversight Member
        /// </summary>
        public string Gender { get; set; }
    }   
}