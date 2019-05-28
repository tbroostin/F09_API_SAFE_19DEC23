using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class F09SsnRequest
    {
        public string Id { get; set; }

        public string RequestType { get; set; }

        public string Ssn { get; set; }
    }
}
