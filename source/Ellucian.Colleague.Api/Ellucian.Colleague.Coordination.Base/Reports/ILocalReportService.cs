/* Copyright 2017-2018 Ellucian Company L.P. and its affiliates. */
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Security;

namespace Ellucian.Colleague.Coordination.Base.Reports
{
    /// <summary>
    /// Use the ReportRenderService interface to abstract the rendering of a Local or Remote
    /// Report object
    /// </summary>
    public interface ILocalReportService
    {
        void ClearDataSources();
        void AddDataSource(ReportDataSource dataSource);
        void SetPath(string path);      
        void SetBasePermissionsForSandboxAppDomain(PermissionSet permissionSet);
        void EnableExternalImages(bool isEnabled);
        void SetParameters(IEnumerable<ReportParameter> parameters);
        byte[] RenderReport();
        void ResetReport();
    }
}
