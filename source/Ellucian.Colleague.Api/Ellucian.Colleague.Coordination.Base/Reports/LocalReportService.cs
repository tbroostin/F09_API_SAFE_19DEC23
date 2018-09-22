/* Copyright 2017-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Web.Dependency;
using Microsoft.Reporting.WebForms;
using slf4net;
using System.Collections.Generic;
using System.Security;

namespace Ellucian.Colleague.Coordination.Base.Reports
{
    /// <summary>
    /// Use the ReportRenderService to abstract the rendering of Local and Remote reports
    /// </summary>
    [RegisterType]
    public class LocalReportService : ILocalReportService
    {
        private readonly ILogger logger;

        private LocalReport LocalReport;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public LocalReportService(ILogger logger)
        {
            this.logger = logger;
            LocalReport = new LocalReport();
        }


        public void EnableExternalImages(bool isEnabled)
        {
            LocalReport.EnableExternalImages = isEnabled;
        }

        public byte[] RenderReport()
        {
            string mimeType = string.Empty;
            string encoding;
            string fileNameExtension;
            Warning[] warnings;
            string[] streams;

            // Render the report as a byte array
            var renderedBytes = LocalReport.Render(
                PdfReportConstants.ReportType,
                PdfReportConstants.DeviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            return renderedBytes;
        }

        public void SetBasePermissionsForSandboxAppDomain(PermissionSet permissionSet)
        {
            LocalReport.SetBasePermissionsForSandboxAppDomain(permissionSet);
        }

        public void SetParameters(IEnumerable<ReportParameter> parameters)
        {
            LocalReport.SetParameters(parameters);
        }

        public void SetPath(string path)
        {
            LocalReport.ReportPath = path;
        }

        public void ClearDataSources()
        {
            LocalReport.DataSources.Clear();
        }

        public void AddDataSource(ReportDataSource dataSource)
        {
            LocalReport.DataSources.Add(dataSource);
        }

        public void ResetReport()
        {
            LocalReport.DataSources.Clear();
            LocalReport.ReleaseSandboxAppDomain();
            LocalReport.Dispose();

            LocalReport = new LocalReport();
        }
    }
}