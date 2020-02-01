/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class having Benefit-Deduction information associated with employee compensation
    /// </summary>
    [Serializable]
     public class EmployeeBended
    {
        /// <summary>
        /// Code(ID) of the benefit/deduction in which the employee is enrolled
        /// </summary>
        public string BenededCode { get; set; }
        /// <summary>
        /// Description of the benefit or deduction code
        /// </summary>
        public string BenededDescription { get; set; }
        /// <summary>
        ///  Amount paid by employer
        /// </summary>
        public decimal? BenededEmployerAmount { get; set; }
        /// <summary>
        /// Amount paid by employee
        /// </summary>
        public decimal? BenededEmployeeAmount { get; set; }

        public EmployeeBended(string benededCode, string benededCodeDescription, decimal? benededEmployerAmount, decimal? benededEmployeeAmount)
        {
            if (string.IsNullOrEmpty(benededCode))
                throw new ArgumentNullException("Benefit-Deduction Code cannot be empty");

            BenededCode = benededCode;
            BenededDescription = benededCodeDescription;
            BenededEmployerAmount = benededEmployerAmount;
            BenededEmployeeAmount = benededEmployeeAmount;
        }
    }
}
