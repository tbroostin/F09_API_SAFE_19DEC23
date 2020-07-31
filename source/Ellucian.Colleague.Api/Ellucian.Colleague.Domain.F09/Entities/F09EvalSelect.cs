using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class QEval
    {
        public string EvalKey { get; set; }
        public string EvalType { get; set; }
        public string EvalDesc1 { get; set; }
        public string EvalDesc2 { get; set; }
    }

    [Serializable]
    public class EType
    {
        public string Type { get; set; }
        public string TypeDesc { get; set; }
    }

    [Serializable]
    public class domainF09EvalSelectRequest
    {
        public string Id { get; set; }

    }

    [Serializable]
    public class domainF09EvalSelectResponse
    {
        public string Id { get; set; }
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public List<QEval> QEval { get; set; }
        public List<EType> EType { get; set; }
    }
}
