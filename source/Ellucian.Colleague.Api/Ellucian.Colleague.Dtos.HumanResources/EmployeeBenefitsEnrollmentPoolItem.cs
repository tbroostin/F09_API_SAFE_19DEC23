/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class for EmployeeBenefitsEnrollmentPoolItem
    /// </summary>
    public class EmployeeBenefitsEnrollmentPoolItem
    {
        /// <summary>
        /// Unique identifier for pool item
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Person id
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Inidcates if pool item is part of a trust
        /// </summary>
        public bool IsTrust { get; set; }

        /// <summary>
        /// Name prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Description of prefix
        /// </summary>
        public string PrefixDescription { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Name suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Description of suffix
        /// </summary>
        public string SuffixDescription { get; set; }

        /// <summary>
        /// Address line 1
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Address line 2
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// City for address
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State for address
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Postal code for address
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Country for address
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Relationship to benefits enrollment employee
        /// </summary>
        public string Relationship { get; set; }

        /// <summary>
        /// Birthdate
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Social Security or similar Government Id
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Marital status
        /// </summary>
        public string MaritalStatus { get; set; }

        /// <summary>
        /// Fulltime student indicator
        /// </summary>
        public bool IsFullTimeStudent { get; set; }

        /// <summary>
        /// Organization id
        /// </summary>
        public string OrganizationId { get; set; }

        /// <summary>
        /// Organization name
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Birthdate indicator
        /// </summary>
        public bool IsBirthDateOnFile { get; set; }

        /// <summary>
        ///  SSN indicator
        /// </summary>
        public bool IsGovernmentIdOnFile { get; set; }
    }

}