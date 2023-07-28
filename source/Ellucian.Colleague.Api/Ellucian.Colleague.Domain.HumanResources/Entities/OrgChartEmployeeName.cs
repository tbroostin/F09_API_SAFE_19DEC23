/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents a name for the org chart employee
    /// </summary>
    [Serializable]
    public class OrgChartEmployeeName
    {
        /// <summary>
        /// The employee's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The employee's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The employee's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The employee's formatted full name
        /// </summary>
        public string FullName { get; set; }

        public OrgChartEmployeeName(string firstName,
            string middleName,
            string lastName,
            string fullName)
        {
            
            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentNullException("lastName");
            }

            FirstName= firstName;
            MiddleName= middleName;
            LastName= lastName;
            FullName= fullName;
        }

        public OrgChartEmployeeName() { }

    }
}
