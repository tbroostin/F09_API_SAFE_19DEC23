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

        public string SoftQHtml { get; set; }

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

        public List<ScholarshipApplicationSoftQDto> SoftQs { get; set; }

        // Constructors
        public ScholarshipApplicationResponseDto()
        { }

        public ScholarshipApplicationResponseDto
        (
            string id,
            string respondType,
            string msgHtml,
            string softQHtml,
            string studentName,
            string studentEmail,
            string studentAddress,
            string applDeadline,
            string applTerm,
            string xfstId,
            string xfstPrevSubmit,
            string xfstRefName,
            List<string> xfstSelfRateDesc,
            List<string> xfstResearchInt,
            List<string> xfstDissTopic,
            List<string> xfstFinSit,
            string xfstSelfRate,
            string step1Html,
            string step2Html,
            string step3Html,
            string step4Html,
            string errorMsg,
            List<ScholarshipApplicationAwardsDto> awards,
            List<ScholarshipApplicationSoftQDto> softQs
        )
        {
            this.Id = id;
            this.RespondType = respondType;
            this.MsgHtml = msgHtml;
            this.SoftQHtml = softQHtml;
            this.StudentName = studentName;
            this.StudentEmail = studentEmail;
            this.StudentAddress = studentAddress;
            this.ApplDeadline = applDeadline;
            this.ApplTerm = applTerm;
            this.XfstId = xfstId;
            this.XfstPrevSubmit = xfstPrevSubmit;
            this.XfstRefName = xfstRefName;
            this.XfstSelfRateDesc = xfstSelfRateDesc;
            this.XfstResearchInt = xfstResearchInt;
            this.XfstDissTopic = xfstDissTopic;
            this.XfstFinSit = xfstFinSit;
            this.XfstSelfRate = xfstSelfRate;
            this.Step1Html = step1Html;
            this.Step2Html = step2Html;
            this.Step3Html = step3Html;
            this.Step4Html = step4Html;
            this.ErrorMsg = errorMsg;
            this.Awards = awards;
            this.SoftQs = softQs;
        }
    }
}
