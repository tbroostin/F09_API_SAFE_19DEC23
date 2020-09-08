// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Reporting.WebForms;

namespace Ellucian.Colleague.Coordination.Base.Reports
{
    public interface IReportUtility
    {
        List<ReportParameter> BuildReportParametersFromResourceFiles(IEnumerable<string> resourceFilePaths);
        ReportParameter BuildReportParameter(string parameterName, object parameterValue);
        bool FileExists(string path);
        DataSet ConvertToDataSet(object[] values);
    }
}
