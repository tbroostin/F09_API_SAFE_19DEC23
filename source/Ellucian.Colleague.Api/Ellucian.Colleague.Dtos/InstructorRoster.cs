using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    [DataContract]
    public class InstructorRoster
    {
        [DataMember(Name = "guid")]
        public Guid Guid { get; set; }

        [DataMember(Name = "loadPercentage")]
        public decimal LoadPercentage { get; set; }
    }
}
