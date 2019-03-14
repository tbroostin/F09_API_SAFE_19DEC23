using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class ScholarshipApplicationResponseDto
    {
        public string Id { get; set; }

        public string RespondType { get; set; }

        public string MsgHtml { get; set; }

        public string StudentName { get; set; }

        public string StudentEmail { get; set; }

        public string StudentAddress { get; set; }

        public string ApplDeadline { get; set; }

        public string ApplTerm { get; set; }

        public string XfstId { get; set; }

        public string XfstPrevSubmit { get; set; }

        public string XfstRefName { get; set; }

        public List<string> XfstSelfRateDesc { get; set; }

        public List<string> XfstResearchInt { get; set; }

        public List<string> XfstDissTopic { get; set; }

        public List<string> XfstFinSit { get; set; }

        public string XfstSelfRate { get; set; }

        public string Step1Html { get; set; }

        public string Step2Html { get; set; }

        public string Step3Html { get; set; }

        public string Step4Html { get; set; }

        public string ErrorMsg { get; set; }

        public List<ScholarshipApplicationAwardsDto> Awards { get; set; }
    }
}
