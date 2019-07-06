using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09
{
    public class GradeOptions
    {
        public string GradeCode { get; set; }
        public string GradeDesc { get; set; }
    }

    public class Questions
    {
        public string QQuestion { get; set; }
        public string QAnswers { get; set; }
        public string QHeader { get; set; }
        public string QAnswerType { get; set; }
        public string QComment { get; set; }
        public string QRequired { get; set; }
        public string QTag { get; set; }
        public string QAnswer { get; set; }

    }

    public class dtoF09KaGradingRequest
    {
        public string FacId { get; set; }
        public string StcId { get; set; }
        public string RequestType { get; set; }
        public string GradeSelected { get; set; }
        public string GradeExpireDate { get; set; }
        public string KaComments { get; set; }
        public List<Questions> Questions { get; set; }

        public dtoF09KaGradingRequest()
        {
        }
    }

    public class dtoF09KaGradingResponse
    {
        public string FacId { get; set; }
        public string StcId { get; set; }
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public string KaHeaderHtml { get; set; }

        public List<GradeOptions> GradeOptions { get; set; }
        public List<Questions> Questions { get; set; }

        public dtoF09KaGradingResponse(
            string facId,
            string stcId,
            string respondType,
            string Msg,
            string kaHeaderHtml,
            List<GradeOptions> gradeOptions,
            List<Questions> questions
            )
        {
            this.FacId = facId;
            this.StcId = stcId;
            this.RespondType = respondType;
            this.Msg = Msg;
            this.KaHeaderHtml = kaHeaderHtml;
            this.GradeOptions = gradeOptions;
            this.Questions = questions;
        }

        public dtoF09KaGradingResponse()
        {
        }
    }
}
