// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// CommunicationHistoryCodes is a grouping of data to import a communication history code.
    /// </summary>
    [Serializable]
    public class CommunicationHistory
    {
        /// <summary>
        /// Gets or sets the erp prospect identifier.
        /// </summary>
        /// <value>
        /// The erp prospect identifier.
        /// </value>
        public string ErpProspectId { get; set; }

        /// <summary>
        /// Gets or sets the CRM prospect identifier.
        /// </summary>
        /// <value>
        /// The CRM prospect identifier.
        /// </value>
        public string CrmProspectId { get; set; }

        /// <summary>
        /// Gets or sets the CRM activity identifier.
        /// </summary>
        /// <value>
        /// The CRM activity identifier.
        /// </value>
        public string CrmActivityId { get; set; }

        /// <summary>
        /// Gets or sets the communication code.
        /// </summary>
        /// <value>
        /// The communication code.
        /// </value>
        public string CommunicationCode { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public Nullable<DateTime> Date { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the name of the recruiter organization.
        /// </summary>
        /// <value>
        /// The name of the recruiter organization.
        /// </value>
        public string RecruiterOrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the recruiter organization identifier.
        /// </summary>
        /// <value>
        /// The recruiter organization identifier.
        /// </value>
        public string RecruiterOrganizationId { get; set; }
    }
}
