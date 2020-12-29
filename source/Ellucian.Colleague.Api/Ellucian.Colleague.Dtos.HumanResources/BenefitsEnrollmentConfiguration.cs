/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// The benefits enrollment configuration contains settings used for benefits enrollment
    /// </summary>
    public class BenefitsEnrollmentConfiguration
    {
        /// <summary>
        /// List of relationship types to display
        /// </summary>
        public List<string> RelationshipTypes { get; set; }

        /// <summary>
        /// If true - benefits enrollment is enabled in SS and not in Web Advisor
        /// </summary>
        public bool IsBenefitsEnrollmentEnabled { get; set; }
    }
}
