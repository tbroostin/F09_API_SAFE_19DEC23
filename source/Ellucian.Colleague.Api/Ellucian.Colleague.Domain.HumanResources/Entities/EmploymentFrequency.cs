//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmploymentFrequency
    /// </summary>
    [Serializable]
    public class EmploymentFrequency : GuidCodeItem
    {
        /// <summary>
        /// The type of the employment frequency.
        /// </summary>
        public EmploymentFrequenciesType EmploymentFrequenciesType { get { return employmentFrequenciesType; } }
        private EmploymentFrequenciesType employmentFrequenciesType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmploymentFrequency"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="eft">The type of the employment frequency.</param>
        public EmploymentFrequency(string guid, string code, string description, string eft)
            : base(guid, code, description)
        {
            switch (eft)
            {
                case "365":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Daily;
                    break;
                case "52":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Weekly;
                    break;
                case "24":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Semimonthly;
                    break;
                case "26":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Biweekly;
                    break;
                case "12":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Monthly;
                    break;
                case "4":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Quarterly;
                    break;
                case "2":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Semiannually;
                    break;
                case "1":
                    this.employmentFrequenciesType = EmploymentFrequenciesType.Annually;
                    break;
                default:
                    this.employmentFrequenciesType = Entities.EmploymentFrequenciesType.Contractual;
                    break;
            }

        }
    }
}