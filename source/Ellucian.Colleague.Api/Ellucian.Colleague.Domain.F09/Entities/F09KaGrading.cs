using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class GradeOptions
    {
        public string GradeCode { get; set; }
        public string GradeDesc { get; set; }
    }

    [Serializable]
    public class Questions
    {
        public string QQuestion { get; set; }
        public string QAnswers { get; set; }
    }

    [Serializable]
    public class domF09KaGradingRequest
    {
        public string FacId { get; set; }
        public string StcId { get; set; }
        public string RequestType { get; set; }
        public string GradeSelected { get; set; }
        public string KaComments { get; set; }
    }

    [Serializable]
    public class domF09KaGradingResponse
    {
        public string FacId { get; set; }
        public string StcId { get; set; }
        public string RespondType { get; set; }
        public string ErrorMsg { get; set; }
        public string KaHeaderHtml { get; set; }

        public List<GradeOptions> GradeOptions { get; set; }
        public List<Questions> Questions { get; set; }
    }
}
