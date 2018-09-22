using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All information related to an test score import
    /// </summary>
    public class TestScore
    {
        /// <summary>
        /// ERP prospect ID (ID in PERSON)
        /// </summary>
        public string ErpProspectId { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        public string TestType { get; set; }

        /// <summary>
        /// Test date
        /// </summary>
        public string TestDate { get; set; }

        /// <summary>
        /// Test score source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Subtest
        /// </summary>
        public string SubtestType { get; set; }

        /// <summary>
        /// Test score
        /// </summary>
        public string Score { get; set; }

        /// <summary>
        /// Custom fields
        /// </summary>
        public IEnumerable<CustomField> CustomFields { get; set; }

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
