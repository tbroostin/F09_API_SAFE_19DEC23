﻿//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    //FinancialAidOffice for EEDM. Non-EEDM FinancialAidOffice already exists.
    [Serializable]
    public class FinancialAidOfficeItem : GuidCodeItem
    {
        /// <summary>
        /// The address lines of the financial aid office location
        /// </summary>
        public List<string> AddressLines { get; set; }
        /// <summary>
        /// The city of the financial aid office location
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// The state of the financial aid office location
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// The postal code of the financial aid office location
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// The name of the financial aid administrator for the office.
        /// </summary>
        public string AidAdministrator { get; set; }
        /// <summary>
        /// The phone number of the financial aid office.
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// The fax number of the financial aid office.
        /// </summary>
        public string FaxNumber { get; set; }
        /// <summary>
        /// The e-mail address of the financial aid office.
        /// </summary>
        public string EmailAddress { get; set; }
        /// <summary>
        /// The name of the financial aid office
        /// </summary>
        public string Name;

        /// <summary>
        /// Constructor for FinancialAidOffice
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        /// <param name="name">name</param>
        public FinancialAidOfficeItem(string guid, string code, string description, string name)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("Name cannot be null or empty");
            }
            this.Name = name;
        }
    }
}