/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class containing Employee Current Benefit list.
    /// It includes employee's current benefit list and additional information.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Build a EmployeeBenefits object
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="AdditionalInformation"></param>        
        /// <param name="CurrentBenefits"></param>
        public EmployeeBenefits(
            string personId,
            string additionalInformation,
            List<CurrentBenefit> currentBenefits)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            PersonId = personId;
            AdditionalInformation = additionalInformation;
            CurrentBenefits = currentBenefits;
        }
    }
}
