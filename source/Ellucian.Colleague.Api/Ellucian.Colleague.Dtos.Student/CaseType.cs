// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.


using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Attributes of Case Type related to Retention Alert
    /// </summary>

    public class CaseType 
    {
        /// <summary>
        /// Case Type Id
        /// </summary>
        public string CaseTypeId { get; set; }

        /// <summary>
        /// Case Type code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Case Type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Category to which the case belongs
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The Priority of the case
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// A flag to indicate if the case is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A flag to indicate if manual contribution to this case is allowed
        /// </summary>
        public bool AllowCaseContribution { get; set; }

        /// <summary>
        /// A list of available communication codes
        /// </summary>
        public IEnumerable<string> AvailableCommunicationCodes { get; set; }

    }
}


