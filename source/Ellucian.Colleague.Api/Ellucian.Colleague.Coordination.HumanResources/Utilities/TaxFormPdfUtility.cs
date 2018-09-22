// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Utility;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.HumanResources.Utilities
{
    public class TaxFormPdfUtility
    {
        public static void Populate1095CDependentRow(ref List<ReportParameter> parameters, Form1095cCoveredIndividualsPdfData coveredIndividual, int rowNumber)
        {
            var utility = new ReportUtility();
            string key = "Box" + rowNumber.ToString() + "Name";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredIndividualName()));

            key = "Box" + rowNumber.ToString() + "SSN";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredIndividualSsn));

            key = "Box" + rowNumber.ToString() + "DOB";
            var dob = coveredIndividual.CoveredIndividualDateOfBirth.HasValue ? coveredIndividual.CoveredIndividualDateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty;
            parameters.Add(utility.BuildReportParameter(key, dob));

            key = "Box" + rowNumber.ToString() + "All";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.Covered12Month ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Jan";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredJanuary ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Feb";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredFebruary ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Mar";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredMarch ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Apr";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredApril ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "May";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredMay ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "June";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredJune ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "July";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredJuly ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Aug";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredAugust ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Sept";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredSeptember ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Oct";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredOctober ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Nov";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredNovember ? "X" : ""));

            key = "Box" + rowNumber.ToString() + "Dec";
            parameters.Add(utility.BuildReportParameter(key, coveredIndividual.CoveredDecember ? "X" : ""));
        }

        public static void Populate1095CDependentRow(ref Dictionary<string, string> pdfData, Form1095cCoveredIndividualsPdfData coveredIndividual, int rowNumber)
        {
            string key = "Box" + rowNumber.ToString() + "Name";
            pdfData[key] = coveredIndividual.CoveredIndividualName();

            key = "Box" + rowNumber.ToString() + "SSN";
            pdfData[key] = coveredIndividual.CoveredIndividualSsn;

            key = "Box" + rowNumber.ToString() + "DOB";
            pdfData[key] = coveredIndividual.CoveredIndividualDateOfBirth.HasValue ? coveredIndividual.CoveredIndividualDateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty;

            key = "Box" + rowNumber.ToString() + "All";
            pdfData[key] = coveredIndividual.Covered12Month ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Jan";
            pdfData[key] = coveredIndividual.CoveredJanuary ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Feb";
            pdfData[key] = coveredIndividual.CoveredFebruary ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Mar";
            pdfData[key] = coveredIndividual.CoveredMarch ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Apr";
            pdfData[key] = coveredIndividual.CoveredApril ? "X" : "";

            key = "Box" + rowNumber.ToString() + "May";
            pdfData[key] = coveredIndividual.CoveredMay ? "X" : "";

            key = "Box" + rowNumber.ToString() + "June";
            pdfData[key] = coveredIndividual.CoveredJune ? "X" : "";

            key = "Box" + rowNumber.ToString() + "July";
            pdfData[key] = coveredIndividual.CoveredJuly ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Aug";
            pdfData[key] = coveredIndividual.CoveredAugust ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Sept";
            pdfData[key] = coveredIndividual.CoveredSeptember ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Oct";
            pdfData[key] = coveredIndividual.CoveredOctober ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Nov";
            pdfData[key] = coveredIndividual.CoveredNovember ? "X" : "";

            key = "Box" + rowNumber.ToString() + "Dec";
            pdfData[key] = coveredIndividual.CoveredDecember ? "X" : "";
        }
    }
}