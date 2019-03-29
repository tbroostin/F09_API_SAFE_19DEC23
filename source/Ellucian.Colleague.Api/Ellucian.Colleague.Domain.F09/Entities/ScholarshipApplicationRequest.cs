using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class ScholarshipApplicationRequest
    {
        public string Id { get; set; }

        public string RequestType { get; set; }

        public string XfstId { get; set; }

        public string XfstRefName { get; set; }

        public string XfstSelfRateDesc { get; set; }

        public string XfstResearchInt { get; set; }

        public string XfstDissTopic { get; set; }

        public string XfstFinSit { get; set; }

        public string XfstSelfRate { get; set; }

        public List<ScholarshipApplicationAwards> Awards { get; set; }

        public List<ScholarshipApplicationSoftQ> SoftQs { get; set; }
    }
}
