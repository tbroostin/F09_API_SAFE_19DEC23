using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class QEval
    {
        public string EvalKey { get; set; }
        public string EvalType { get; set; }
        public string EvalDesc1 { get; set; }
        public string EvalDesc2 { get; set; }
    }

    public class EType
    {
        public string Type { get; set; }
        public string TypeDesc { get; set; }
    }

    public class dtoF09EvalSelectRequest
    {
        public string Id { get; set; }

        public dtoF09EvalSelectRequest()
        {
        }
    }

    public class dtoF09EvalSelectResponse
    {
        public string Id { get; set; }
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public List<QEval> QEval { get; set; }
        public List<EType> EType { get; set; }

        //constructors
        public dtoF09EvalSelectResponse()
        {
            QEval = new List<QEval>();
            EType = new List<EType>();
        }
        
        public dtoF09EvalSelectResponse
        (
            string Id,
            string RespondType,
            string Msg,
            List<QEval> QEval_in,
            List<EType> EType_in
        )
        {
            this.Id = Id;
            this.RespondType = RespondType;
            this.Msg = Msg;
            this.QEval = QEval_in;
            this.EType = EType_in;
        }

    }
}
