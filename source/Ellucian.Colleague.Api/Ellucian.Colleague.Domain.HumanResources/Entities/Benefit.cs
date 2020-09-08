/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Generic Benefit class to be inherited by different 
    /// types of benefit classes
    /// </summary>
    [Serializable]
    public abstract class Benefit
    {
        /// <summary>
        /// Benefit Id - BENDED ID
        /// </summary>
        public string BenefitId { get { return benefitId; } }
        protected readonly string benefitId;

        /// <summary>
        /// Benefit Description
        /// </summary>
        public string BenefitDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public Benefit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            benefitId = id;
        }
    }
}
