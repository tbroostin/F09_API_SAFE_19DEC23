/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Institution Employers domain entity
    /// </summary>
    [Serializable]
    public class InstitutionEmployers
    {
        /// <summary>
        /// The guid for institution-employers.
        /// </summary>
        public string Guid
        {
            get { return guid; }
        }
        private readonly string guid;

        /// <summary>
        /// The database ID of the employer.
        /// </summary>
        public string EmployerId
        {
            get { return employerId; }
        }
        private readonly string employerId;

        /// <summary>
        /// The preferred name of the employer.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// The address lines of the employer.
        /// </summary>
        public List<string> AddressLines { get; set; }

        /// <summary>
        /// The city of the employer.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The state of the employer.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The country of the employer.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The postal code of the employer.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// The phone number of the employer.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// A code that may be used to identify the institution employer.
        /// (not currently used by Colleague)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Institution Employer constructor 
        /// </summary>
        /// <param name="employerId"></param>
        public InstitutionEmployers(string guid, string employerId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("institution employer guid");
            }
                                
            if (string.IsNullOrEmpty(employerId))
            {
                throw new ArgumentNullException("employerId");
            }

            this.guid = guid;
            this.employerId = employerId;  
        }
    }
}