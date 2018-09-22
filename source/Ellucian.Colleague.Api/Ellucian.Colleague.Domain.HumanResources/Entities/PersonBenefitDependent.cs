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
    /// PersonBenefitDependent
    /// </summary>
    [Serializable]
    public class PersonBenefitDependent
    {
        /// <summary>
        /// Variable for guid.
        /// </summary>
        private readonly string guid;

        /// <summary>
        /// Guid for person benefit dependent.
        /// </summary>
        public string Guid { get { return this.guid; } }

        /// <summary>
        /// Variable for deduction arrangement.
        /// </summary>
        private readonly string deductionArrangement;

        /// <summary>
        /// The global identifier for the Deduction Arrangement.
        /// </summary>
        public string DeductionArrangement { get { return this.deductionArrangement; } }

        /// <summary>
        /// Backing variable for DependentPersonId.
        /// </summary>
        private readonly string dependentPersonId;

        /// <summary>
        /// Dependent person to whom this beneift is assigned.
        /// </summary>
        public string DependentPersonId { get { return this.dependentPersonId; } }

        /// <summary>
        /// The name of the benefit provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// The identification of the benefit provider.
        /// </summary>
        public string ProviderIdentification { get; set; }

        /// <summary>
        /// The date when the coverage begins
        /// </summary>
        public DateTime? CoverageStartOn{ get; set; }

        /// <summary>
        /// The date when the coverage ends
        /// </summary>
        public DateTime? CoverageEndOn { get; set; }

        /// <summary>
        /// An indication whether the dependent is attending an educational institution.
        /// </summary>
        public string StudentStatus { get; set; }

		/// <summary>
        /// Initializes a new instance of the <see cref="PersonBenefitDependent"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="deductionArrangementId">The Unique Identifier for deduction arrangement</param>
        /// <param name="dependentPersonId">The Unique Identifier for dependent person</param>
        public PersonBenefitDependent(string guid, string deductionArrangementId, string dependentPersonId)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "guid is required.");
            if (string.IsNullOrEmpty(deductionArrangementId))
                throw new ArgumentNullException("deductionArrangementId", "deductionArrangementId is required.");
            if (string.IsNullOrEmpty(dependentPersonId))
                throw new ArgumentNullException("dependentPersonId", "dependentPersonId is required.");
            
            this.guid = guid;
            this.deductionArrangement = deductionArrangementId;
            this.dependentPersonId = dependentPersonId;
        }
    }
}