/* Copyright 2017-2018 Ellucian Company L.P. and its affiliates. */

namespace Ellucian.Colleague.Coordination.Base.Reports
{
    /// <summary>
    /// Use PdfReportConstants when rendering PDF reports
    /// </summary>
    public class PdfReportConstants
    {
        public const string ReportType = "PDF";
        public const string DeviceInfo = "<DeviceInfo>" +
                                         "<OutputFormat>PDF</OutputFormat>" +
                                         "<HumanReadablePDF>True</HumanReadablePDF>" +
                                         "</DeviceInfo>";
    }
}
