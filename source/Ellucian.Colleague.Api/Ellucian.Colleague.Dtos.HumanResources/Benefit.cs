/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Generic Benefit class to be inherited by different 
    /// types of benefit classes
    /// </summary>
    public abstract class Benefit
    {
        /// <summary>
        /// Benefit Id - BENDED ID
        /// </summary>
        public string BenefitId { get; set; }
        /// <summary>
        /// Benefit Description
        /// </summary>
        public string BenefitDescription { get; set; }
    }
}
