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
    /// JobApplication
    /// </summary>
    [Serializable]
    public class JobApplication
    {
        /// <summary>
        /// Variable for guid.
        /// </summary>
        private readonly string guid;

        /// <summary>
        /// Guid for job application.
        /// </summary>
        public string Guid { get { return this.guid; } }

        /// <summary>
        /// Backing variable for PersonId.
        /// </summary>
        private readonly string personId;

        /// <summary>
        /// Person to whom this job application is assigned.
        /// </summary>
        public string PersonId { get { return this.personId; } }

        /// <summary>
        /// Backing variable for PositionId.
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// Backing variable for DesiredCompensationRateValue.
        /// </summary>
        public decimal? DesiredCompensationRateValue { get; set; }

        /// <summary>
        /// Backing variable for DesiredCompensationRateCurrency.
        /// </summary>
        public string DesiredCompensationRateCurrency { get; set; }

        /// <summary>
        /// Backing variable for DesiredCompensationRatePeriod.
        /// </summary>
        public string DesiredCompensationRatePeriod { get; set; }

        /// <summary>
        /// Backing variable for AppliedOn.
        /// </summary>
        public DateTime? AppliedOn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobApplication"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="personId">The Unique Identifier for person</param>
        public JobApplication(string guid, string personId)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "guid is required.");
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is required.");
            
            this.guid = guid;
            this.personId = personId;
        }

    }
}