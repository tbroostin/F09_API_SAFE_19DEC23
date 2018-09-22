// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents 1095-C tax form PDF data for an employee's covered individuals.
    /// If the employee is self insured, there will be an instance for the 
    /// employee and additional instances for each of its covered Individuals.
    /// </summary>
    [Serializable]
    public class Form1095cCoveredIndividualsPdfData
    {
        /// <summary>
        /// Is this object the employee itself or one of his/her covered individuals
        /// </summary>
        public bool IsEmployeeItself { get; set; }

        /// <summary>
        /// Covered individual social security number
        /// </summary>
        public string CoveredIndividualSsn { get; set; }

        /// <summary>
        /// Covered individual date of birth
        /// </summary>
        public DateTime? CoveredIndividualDateOfBirth { get; set; }

        /// <summary>
        /// Covered individual first name
        /// </summary>
        public string CoveredIndividualFirstName { get; set; }

        /// <summary>
        /// Covered individual second name
        /// </summary>
        public string CoveredIndividualMiddleName { get; set; }

        /// <summary>
        /// Covered individual last name
        /// </summary>
        public string CoveredIndividualLastName { get; set; }

        /// <summary>
        /// Covered individual full name
        /// </summary>
        /// <returns></returns>
        public string CoveredIndividualName()
        {
            string name = CoveredIndividualFirstName;
            if (!string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(CoveredIndividualMiddleName))
                {
                    name = name + " " + CoveredIndividualMiddleName.Substring(0, 1);
                }
                if (!string.IsNullOrEmpty(CoveredIndividualLastName))
                {
                    name = name + " " + CoveredIndividualLastName;
                }
            }
            return name;
        }

        /// <summary>
        /// Covered individual had coverage for the 12 months
        /// </summary>
        public bool Covered12Month { get; set; }

        /// <summary>
        /// Covered individual had coverage for January
        /// </summary>
        public bool CoveredJanuary { get; set; }

        /// <summary>
        /// Covered individual had coverage for February
        /// </summary>
        public bool CoveredFebruary { get; set; }

        /// <summary>
        /// Covered individual had coverage for March
        /// </summary>
        public bool CoveredMarch { get; set; }

        /// <summary>
        /// Covered individual had coverage for April
        /// </summary>
        public bool CoveredApril { get; set; }

        /// <summary>
        /// Covered individual had coverage for May
        /// </summary>
        public bool CoveredMay { get; set; }

        /// <summary>
        /// Covered individual had coverage for June
        /// </summary>
        public bool CoveredJune { get; set; }

        /// <summary>
        /// Covered individual had coverage for July
        /// </summary>
        public bool CoveredJuly { get; set; }

        /// <summary>
        /// Covered individual had coverage for August
        /// </summary>
        public bool CoveredAugust { get; set; }

        /// <summary>
        /// Covered individual had coverage for September
        /// </summary>
        public bool CoveredSeptember { get; set; }

        /// <summary>
        /// Covered individual had coverage for October
        /// </summary>
        public bool CoveredOctober { get; set; }

        /// <summary>
        /// Covered individual had coverage for November
        /// </summary>
        public bool CoveredNovember { get; set; }

        /// <summary>
        /// Covered individual had coverage for December
        /// </summary>
        public bool CoveredDecember { get; set; }

        /// <summary>
        /// Initialize the domain entity.
        /// </summary>
        public Form1095cCoveredIndividualsPdfData()
        {
            CoveredIndividualSsn = "";
        }
    }
}
