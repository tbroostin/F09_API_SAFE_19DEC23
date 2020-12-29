/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class containing Current Benefits of employee.
    /// It includes details such as Coverage, Dependents, Health Care Provider, Beneficiaries Information of the benefits assigned to an employee .
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Build a CurrentBenefit object
        /// </summary>
        /// <param name="benefitDescription"></param>
        /// <param name="coverage"></param>
        /// <param name="employeeCost"></param> 
        /// <param name="dependents"></param>
        /// <param name="healthCareProviders"></param>
        /// <param name="beneficiaries"></param>       
        public CurrentBenefit(
            string benefitDescription,
            string coverage,
            string employeeCost,
            List<string> dependents,
            List<string> healthCareProviders,
            List<string> beneficiaries)
        {
            if (string.IsNullOrEmpty(benefitDescription))
            {
                throw new ArgumentNullException("benefitDescription");
            }

            BenefitDescription = benefitDescription;
            Coverage = coverage;
            EmployeeCost = employeeCost;
            Dependents = dependents;
            HealthCareProviders = healthCareProviders;
            Beneficiaries = beneficiaries;
        }
    }
}
