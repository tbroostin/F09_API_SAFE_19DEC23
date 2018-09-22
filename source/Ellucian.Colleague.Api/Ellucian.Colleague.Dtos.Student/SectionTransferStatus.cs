using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// SectionTransferStatuses describe how or if a section will transfer to other institutions
    /// </summary>
    public class SectionTransferStatus
    {
        /// <summary>
        /// Unique code for this transfer status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Transfer status description
        /// </summary>
        public string Description { get; set; }

    }

}
