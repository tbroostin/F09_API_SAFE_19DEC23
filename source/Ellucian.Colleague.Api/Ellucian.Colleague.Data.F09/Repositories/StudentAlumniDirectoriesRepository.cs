using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.F09.Transactions;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.F09.Repositories
{
    [RegisterType]
    class StudentAlumniDirectoriesRepository : BaseColleagueRepository, IStudentAlumniDirectoriesRepository
    {
        public StudentAlumniDirectoriesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
        }

        public async Task<DirectoriesResponse> GetStudentAlumniDirectoriesAsync(string personId)
        {
            var request = new ctxF09DirectoriesRequest();
            request.Id = personId;
            request.RequestType = "Get";

            DirectoriesResponse application;

            try
            {
                ctxF09DirectoriesResponse response = await transactionInvoker.ExecuteAsync<ctxF09DirectoriesRequest, ctxF09DirectoriesResponse>(request);
                application = this.CreateStudentAlumniDirectoriesObject(response);

            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-GetStudentAlumniDirectoriesAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-GetStudentAlumniDirectoriesAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        public async Task<DirectoriesResponse> UpdateStudentAlumniDirectoriesAsync(DirectoriesRequest applicationRequest)
        {
            var request = new ctxF09DirectoriesRequest();
            request.Id = applicationRequest.Id;
            request.RequestType = applicationRequest.RequestType;
            request.ReportSelected = applicationRequest.ReportSelected;
            request.FormatSelected = applicationRequest.FormatSelected;
            request.SortSelected = applicationRequest.SortSelected;
            request.DeptSelected = applicationRequest.DeptSelected;
            request.ProgSelected = applicationRequest.ProgSelected;
            request.FacSelected = applicationRequest.FacSelected;
            request.StateSelected = applicationRequest.StateSelected;
            request.CountrySelected = applicationRequest.CountrySelected;
            request.ConcentraSelected = applicationRequest.ConcentraSelected;
            request.DissSearchString = applicationRequest.DissSearchString;

            DirectoriesResponse application;

            try
            {
                ctxF09DirectoriesResponse response = await transactionInvoker.ExecuteAsync<ctxF09DirectoriesRequest, ctxF09DirectoriesResponse>(request);
                application = this.CreateStudentAlumniDirectoriesObject(response);
            }
            catch (Exception ex)
            {
                logger.Error("Error in transaction 'F09-UpdateStudentAlumniDirectoriesAsync': " + String.Join("\n", ex.Message));
                throw new ColleagueTransactionException("Error in transaction 'F09-UpdateStudentAlumniDirectoriesAsync': " + String.Join("\n", ex.Message));
            }

            return application;
        }

        private DirectoriesResponse CreateStudentAlumniDirectoriesObject(ctxF09DirectoriesResponse response)
        {
            DirectoriesResponse application = new DirectoriesResponse();
            application.RespondType = response.RespondType;
            application.HtmlReport = response.HtmlReport;
            application.ErrorMsg = response.ErrorMsg;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ReportOptions> reportOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ReportOptions>();
            foreach (Transactions.ReportOptions respReportOptions in response.ReportOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ReportOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ReportOptions();
                option.ReportCode = respReportOptions.ReportCode;
                option.ReportDesc = respReportOptions.ReportDesc;
                reportOptions.Add(option);
            }
            application.ReportOptions = reportOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FormatOptions> formatOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FormatOptions>();
            foreach (Transactions.FormatOptions respFormatOptions in response.FormatOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FormatOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FormatOptions();
                option.FormatCode = respFormatOptions.FormatCode;
                option.FormatDesc = respFormatOptions.FormatDesc;
                formatOptions.Add(option);
            }
            application.FormatOptions = formatOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.SortOptions> sortOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.SortOptions>();
            foreach (Transactions.SortOptions respSortOptions in response.SortOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.SortOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.SortOptions();
                option.SortCode = respSortOptions.SortCode;
                option.SortDesc = respSortOptions.SortDesc;
                sortOptions.Add(option);
            }
            application.SortOptions = sortOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DeptOptions> deptOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DeptOptions>();
            foreach (Transactions.DeptOptions respDeptOptions in response.DeptOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DeptOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DeptOptions();
                option.DeptCode = respDeptOptions.DeptCode;
                option.DeptDesc = respDeptOptions.DeptDesc;
                deptOptions.Add(option);
            }
            application.DeptOptions = deptOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ProgOptions> progOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ProgOptions>();
            foreach (Transactions.ProgOptions respProgOptions in response.ProgOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ProgOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ProgOptions();
                option.ProgCode = respProgOptions.ProgCode;
                option.ProgDesc = respProgOptions.ProgDesc;
                progOptions.Add(option);
            }
            application.ProgOptions = progOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FacOptions> facOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FacOptions>();
            foreach (Transactions.FacOptions respFacOptions in response.FacOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FacOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FacOptions();
                option.FacCode = respFacOptions.FacCode;
                option.FacDesc = respFacOptions.FacDesc;
                facOptions.Add(option);
            }
            application.FacOptions = facOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.StateOptions> stateOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.StateOptions>();
            foreach (Transactions.StateOptions respStateOptions in response.StateOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.StateOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.StateOptions();
                option.StateCode = respStateOptions.StateCode;
                option.StateDesc = respStateOptions.StateDesc;
                stateOptions.Add(option);
            }
            application.StateOptions = stateOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.CountryOptions> countryOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.CountryOptions>();
            foreach (Transactions.CountryOptions respCountryOptions in response.CountryOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.CountryOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.CountryOptions();
                option.CountryCode = respCountryOptions.CountryCode;
                option.CountryDesc = respCountryOptions.CountryDesc;
                countryOptions.Add(option);
            }
            application.CountryOptions = countryOptions;

            List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ConcentraOptions> concentraOptions = new List<Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ConcentraOptions>();
            foreach (Transactions.ConcentraOptions respConcentraOptions in response.ConcentraOptions)
            {
                Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ConcentraOptions option = new Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ConcentraOptions();
                option.ConcentraCode = respConcentraOptions.ConcentraCode;
                option.ConcentraDesc = respConcentraOptions.ConcentraDesc;
                concentraOptions.Add(option);
            }
            application.ConcentraOptions = concentraOptions;

            return application;
        }
    }
}
