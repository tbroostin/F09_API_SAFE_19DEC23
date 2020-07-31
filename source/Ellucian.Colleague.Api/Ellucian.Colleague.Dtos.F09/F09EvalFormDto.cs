using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class dtoF09EvalFormRequest
    {
        public string Id { get; set; }
        public string EvalKey { get; set; }
        public string RequestType { get; set; }
        public List<Questions> Questions { get; set; }

        public dtoF09EvalFormRequest()
        {
            Questions = new List<Questions>();
        }
    }

    public class dtoF09EvalFormResponse
    {
        public string Id { get; set; }
        public string EvalKey { get; set; }
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public string HeaderHtml { get; set; }
        public List<Questions> Questions { get; set; }

        public dtoF09EvalFormResponse()
        {
            Questions = new List<Questions>();
        }
    }
}
