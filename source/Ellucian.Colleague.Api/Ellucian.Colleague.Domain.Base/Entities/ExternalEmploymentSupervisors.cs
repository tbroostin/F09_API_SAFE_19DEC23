/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// describe superviosrs associated with an external-employment.
    /// </summary>
    [Serializable]
    public class ExternalEmploymentSupervisors
    {
       

        /// <summary>
        /// The first name of supervisor of the person for the external employment.
        /// </summary>
        public string SupervisorFirstName { get; set; }

        /// <summary>
        /// The last name of supervisor of the person for the external employment.
        /// </summary>
        public string SupervisorLastName { get; set; }

        /// <summary>
        /// The phone of supervisor of the person for the external employment.
        /// </summary>
        public string SupervisorPhone { get; set; }

        /// <summary>
        /// The email supervisor of the person for the external employment.
        /// </summary>
        public string SupervisorEmail { get; set; }

        /// <summary>
        /// Create a ExternalEmployments supervisors
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">The Id of the employmt</param>
        /// <param name="personId">The person associated with the external employment.</param>
        /// <param name="title">The job title for the external employment.</param>
        /// <param name="status">The status of the external employment</param>
        public ExternalEmploymentSupervisors(string firstName, string lastName, string phone, string email)
        {
            this.SupervisorFirstName = firstName;
            this.SupervisorLastName = lastName;
            this.SupervisorPhone = phone;
            this.SupervisorEmail = email;
        }
    }

       
    
}
