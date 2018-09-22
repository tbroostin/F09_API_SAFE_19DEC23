using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Admitted Status
    /// </summary>>
    public class AdmittedStatus
    {
        /// <summary>
        /// Unique code for this admitted status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Admitted status description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Transfer flag to indicate if status indicates transfer status
        /// </summary>
        public string TransferFlag { get; set; }
    }
}