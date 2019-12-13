using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities
{
    [Serializable]
    public class domF09ReportRequest
    {
        public string Id { get; set; }
        public string Report { get; set; }
        public string RequestType { get; set; }
        public string JsonRequest { get; set; }
    }

    [Serializable]
    public class domF09ReportResponse
    {
        public string RespondType { get; set; }
        public string Msg { get; set; }
        public string HtmlReport { get; set; }
        public string JsonReportOptions { get; set; }
}
}
