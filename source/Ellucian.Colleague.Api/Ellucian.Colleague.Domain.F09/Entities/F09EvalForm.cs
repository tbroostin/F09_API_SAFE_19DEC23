using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class domainF09EvalFormRequest
    {
        public string Id { get; set; }
        public string EvalKey { get; set; }
        public string RequestType { get; set; }
        public List<Questions> Questions { get; set; }

    }

    [Serializable]
    public class domainF09EvalFormResponse
    {
        public string Id { get; set; }
        public string EvalKey { get; set; }
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public string HeaderHtml { get; set; }
        public List<Questions> Questions { get; set; }

    }
}
