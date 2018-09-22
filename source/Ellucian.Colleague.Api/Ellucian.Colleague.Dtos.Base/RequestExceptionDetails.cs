using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Represents the response information detailing why a request failed.
    /// </summary>
    public class RequestExceptionDetails
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// List of conflicts that prevented an update operation. Can be null.
        /// </summary>
        public IEnumerable<string> Conflicts { get; set; }
        
    }
}
