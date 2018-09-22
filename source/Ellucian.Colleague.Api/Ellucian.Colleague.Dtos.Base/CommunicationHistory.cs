using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// All information related to a communication history
    /// </summary>
    public class CommunicationHistory
    {
        /// <summary>
        /// ERP prospect ID (ID in PERSON)
        /// </summary>
        public string ErpProspectId { get; set; }

        /// <summary>
        /// CRM prospect GUID
        /// </summary>
        public string CrmProspectId { get; set; }
        
        /// <summary>
        /// CRM activity GUID
        /// </summary>        
        public string CrmActivityId { get; set; }

        /// <summary>
        /// Activity code
        /// </summary>        
        public string CommunicationCode { get; set; }

        /// <summary>
        /// Date
        /// </summary>        
        public Nullable<DateTime> Date { get; set; }

        /// <summary>
        /// Subject
        /// </summary>        
        public string Subject { get; set; }

        /// <summary>
        /// Status
        /// </summary>        
        public string Status { get; set; }

        /// <summary>
        /// Location
        /// </summary>        
        public string Location { get; set; }

        /// <summary>
        /// CRM organization name
        /// </summary>        
        public string RecruiterOrganizationName { get; set; }

        /// <summary>
        /// CRM organization GUID
        /// </summary>        
        public string RecruiterOrganizationId { get; set; }
    }
}
