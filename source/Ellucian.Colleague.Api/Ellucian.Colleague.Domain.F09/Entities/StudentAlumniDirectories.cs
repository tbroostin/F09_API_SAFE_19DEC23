using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories
{
    [Serializable]
    public class ReportOptions
    {
        public string ReportCode { get; set; }

        public string ReportDesc { get; set; }
    }

    [Serializable]
    public class FormatOptions
    {
        public string FormatCode { get; set; }

        public string FormatDesc { get; set; }
    }

    [Serializable]
    public class SortOptions
    {
        public string SortCode { get; set; }

        public string SortDesc { get; set; }
    }

    [Serializable]
    public class DeptOptions
    {
        public string DeptCode { get; set; }

        public string DeptDesc { get; set; }
    }

    [Serializable]
    public class ProgOptions
    {
        public string ProgCode { get; set; }

        public string ProgDesc { get; set; }
    }

    [Serializable]
    public class FacOptions
    {
        public string FacCode { get; set; }

        public string FacDesc { get; set; }
    }

    [Serializable]
    public class StateOptions
    {
        public string StateCode { get; set; }

        public string StateDesc { get; set; }
    }

    [Serializable]
    public class CountryOptions
    {
        public string CountryCode { get; set; }

        public string CountryDesc { get; set; }
    }

    [Serializable]
    public class ConcentraOptions
    {
        public string ConcentraCode { get; set; }

        public string ConcentraDesc { get; set; }
    }

    [Serializable]
    public class DirectoriesRequest
    {
        public string Id { get; set; }

        public string RequestType { get; set; }

        public string ReportSelected { get; set; }

        public string FormatSelected { get; set; }

        public string SortSelected { get; set; }

        public List<string> DeptSelected { get; set; }

        public List<string> ProgSelected { get; set; }

        public string FacSelected { get; set; }

        public string StateSelected { get; set; }

        public string CountrySelected { get; set; }

        public string ConcentraSelected { get; set; }

        public string DissSearchString { get; set; }
    }

    [Serializable]
    public class DirectoriesResponse
    {
        public string RespondType { get; set; }

        public string HtmlReport { get; set; }

        public string ErrorMsg { get; set; }

        public List<ReportOptions> ReportOptions { get; set; }

        public List<FormatOptions> FormatOptions { get; set; }

        public List<SortOptions> SortOptions { get; set; }

        public List<DeptOptions> DeptOptions { get; set; }

        public List<ProgOptions> ProgOptions { get; set; }

        public List<FacOptions> FacOptions { get; set; }

        public List<StateOptions> StateOptions { get; set; }

        public List<CountryOptions> CountryOptions { get; set; }

        public List<ConcentraOptions> ConcentraOptions { get; set; }
    }
}

