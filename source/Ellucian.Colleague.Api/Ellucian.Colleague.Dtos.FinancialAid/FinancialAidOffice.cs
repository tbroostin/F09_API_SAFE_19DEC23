/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The FinancialAidOffice DTO contains demographic data about a specific financial aid office
    /// and the office's list of configurations
    /// </summary>
    public class FinancialAidOffice
    {
        /// <summary>
        /// The unique id of this Office.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the office
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The AddressLabel is the address (street address, city, state, zip) used in an address label.
        /// Each element in this address list corresponds to a new line in the address label.
        /// </summary>
        public List<string> AddressLabel { get; set; }

        /// <summary>
        /// The phone number used to contact the office
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The email address used to contact the office
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The name of the Financial Aid Director in charge of this office.
        /// </summary>
        public string DirectorName { get; set; }

        /// <summary>
        /// US Department of Education's Office of Postsecondary Education Identifier
        /// </summary>
        public string OpeId { get; set; }

        /// <summary>
        /// A list of configuration objects by award year that describe aspects of this office and how this Financial Aid office 
        /// controls data and actions
        /// </summary>
        public List<FinancialAidConfiguration> Configurations { get; set; }

        /// <summary>
        /// Describes how to display AcademicProgressEvaluations to students
        /// </summary>
        public AcademicProgressConfiguration AcademicProgressConfiguration { get; set; }

        /// <summary>
        /// Flag indicating whether or not this is the default office. If a student's current
        /// office cannot be determined, or assignment of a student to an office fails, we use
        /// the default office.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
