using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class ScholarshipApplicationRequestDto
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

        public List<ScholarshipApplicationAwardsDto> Awards { get; set; }

        public List<ScholarshipApplicationSoftQDto> SoftQs { get; set; }
    }
}
