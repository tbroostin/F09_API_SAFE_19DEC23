using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Coordination.Planning.Reports
{
    /// <summary>
    /// Contains some settings used in generating PDF reports 
    /// </summary>
    public class PdfReportConstants
    {
        public const string ReportType = "PDF";
        public const string DeviceInfo = "<DeviceInfo>" +
                " <OutputFormat>PDF</OutputFormat>" +
                " <PageWidth>11in</PageWidth>" +
                " <PageHeight>8.5in</PageHeight>" +
                " <MarginTop>0.5in</MarginTop>" +
                " <MarginLeft>0.5in</MarginLeft>" +
                " <MarginRight>0.5in</MarginRight>" +
                " <MarginBottom>0.5in</MarginBottom>" +
                "</DeviceInfo>";
    }
}
