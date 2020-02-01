/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class containing Current Benefits of employee.
    /// </summary>
    public class CurrentBenefit
    {
        /// <summary>
        /// Description of the current benefit
        /// </summary>
        public string BenefitDescription { get; set; }

        /// <summary>
        /// Coverage info under the benefit (if any)
        /// </summary>
        public string Coverage { get; set; }

        /// <summary>
        /// Total employee cost attached to the benefit (if any)
        /// </summary>
        public string EmployeeCost { get; set; }

        /// <summary>
        /// List of dependent added to the benefit (if any)
        /// </summary>
        public List<string> Dependents { get; set; }

        /// <summary>
        /// List of assigned health care provider for the dependent (if any)
        /// </summary>
        public List<string> HealthCareProviders { get; set; }

        /// <summary>
        /// List of beneficiaries for the benefits (if any)
        /// </summary>
        public List<string> Beneficiaries { get; set; }
    }
}
