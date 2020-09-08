/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// The benefits enrollment configuration contains settings used for benefits enrollment
    /// </summary>
    [Serializable]
    public class BenefitsEnrollmentConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BenefitsEnrollmentConfiguration()
        {
            RelationshipTypes = new List<string>();
        }

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
