using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Denotes the ways a student might want to hold a transcript request.
    /// </summary>
    public class HoldRequestType
    {
       /// <summary>
        /// Unique code for the hold request type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Hold request type description
        /// </summary>
        public string Description { get; set; }

        
    }

}
