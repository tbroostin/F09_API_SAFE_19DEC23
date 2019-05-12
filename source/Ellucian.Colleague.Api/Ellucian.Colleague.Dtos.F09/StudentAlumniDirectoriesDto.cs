using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories
{
    public class ReportOptions
    {
        public string ReportCode { get; set; }
        
        public string ReportDesc { get; set; }
    }
    
    public class FormatOptions
    {
        public string FormatCode { get; set; }
        
        public string FormatDesc { get; set; }
    }
    
    public class SortOptions
    {
        public string SortCode { get; set; }
        
        public string SortDesc { get; set; }
    }
    
    public class DeptOptions
    {
        public string DeptCode { get; set; }

        public string DeptDesc { get; set; }
    }
    
    public class ProgOptions
    {
        public string ProgCode { get; set; }
        
        public string ProgDesc { get; set; }
    }
    
    public class FacOptions
    {
        public string FacCode { get; set; }
        
        public string FacDesc { get; set; }
    }
    
    public class StateOptions
    {
        public string StateCode { get; set; }

        public string StateDesc { get; set; }
    }
    
    public class CountryOptions
    {
        public string CountryCode { get; set; }
        
        public string CountryDesc { get; set; }
    }
    
    public class ConcentraOptions
    {
        public string ConcentraCode { get; set; }

        public string ConcentraDesc { get; set; }
    }

    public class DirectoriesRequestDto
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

        public DirectoriesRequestDto()
        {
        }
    }

    public class DirectoriesResponseDto
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

        public DirectoriesResponseDto(
            string respondType,
            string htmlReport,
            string errorMsg,
            List<ReportOptions> reportOptions,
            List<FormatOptions> formatOptions,
            List<SortOptions> sortOptions,
            List<DeptOptions> deptOptions,
            List<ProgOptions> progOptions,
            List<FacOptions> facOptions,
            List<StateOptions> stateOptions,
            List<CountryOptions> countryOptions,
            List<ConcentraOptions> concentraOptions
            )
        {
            this.RespondType = respondType;
            this.HtmlReport = htmlReport;
            this.ErrorMsg = errorMsg;
            this.ReportOptions = reportOptions;
            this.FormatOptions = formatOptions;
            this.SortOptions = sortOptions;
            this.DeptOptions = deptOptions;
            this.ProgOptions = progOptions;
            this.FacOptions = facOptions;
            this.StateOptions = stateOptions;
            this.CountryOptions = countryOptions;
            this.ConcentraOptions = concentraOptions;
        }
    }
}
