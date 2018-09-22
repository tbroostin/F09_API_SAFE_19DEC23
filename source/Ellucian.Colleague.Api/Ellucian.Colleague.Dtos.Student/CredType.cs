using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a Credit, such as Institutional, transfer, etc.
    /// </summary>
    public class CredType
    {
        /// <summary>
        /// Unique code for this CredType
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this CredType
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Category (Institutional, Transfer, Continuing Ed, Exchange, Other)
        /// </summary>
        public string Category { get; set; }
    }
}
