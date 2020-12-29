/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class containing Employee Current Benefit list.    
    /// </summary>
    public class EmployeeBenefits
    {
        /// <summary>
        /// employee id of the user
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Additional Information
        /// </summary>
        public string AdditionalInformation { get; set; }

        /// <summary>
        /// List of Benefits 
        /// </summary>
        public List<CurrentBenefit> CurrentBenefits { get; set; }
    }
}
