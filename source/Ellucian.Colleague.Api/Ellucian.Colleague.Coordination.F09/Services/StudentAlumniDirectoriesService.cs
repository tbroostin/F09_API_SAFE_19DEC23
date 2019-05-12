using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class StudentAlumniDirectoriesService : BaseCoordinationService, IStudentAlumniDirectoriesService
    {
        private readonly IStudentAlumniDirectoriesRepository _StudentAlumniDirectoriesRespository;

        public StudentAlumniDirectoriesService(IAdapterRegistry adapterRegistry, IStudentAlumniDirectoriesRepository studentAlumniDirectoriesRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._StudentAlumniDirectoriesRespository = studentAlumniDirectoriesRepository;
        }

        public async Task<DirectoriesResponseDto> GetStudentAlumniDirectoriesAsync(string personId)
        {
            var profile = await _StudentAlumniDirectoriesRespository.GetStudentAlumniDirectoriesAsync(personId);
            var dto = this.ConvertToDTO(profile);

            return dto;
        }

        public async Task<DirectoriesResponseDto> UpdateStudentAlumniDirectoriesAsync(DirectoriesRequestDto request)
        {
            Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DirectoriesRequest studentRequest = new Domain.F09.Entities.StudentAlumniDirectories.DirectoriesRequest();
            studentRequest.Id = request.Id;
            studentRequest.RequestType = request.RequestType;
            studentRequest.ReportSelected = request.ReportSelected;
            studentRequest.FormatSelected = request.FormatSelected;
            studentRequest.SortSelected = request.SortSelected;
            studentRequest.DeptSelected = request.DeptSelected;
            studentRequest.ProgSelected = request.ProgSelected;
            studentRequest.FacSelected = request.FacSelected;
            studentRequest.StateSelected = request.StateSelected;
            studentRequest.CountrySelected = request.CountrySelected;
            studentRequest.ConcentraSelected = request.ConcentraSelected;
            studentRequest.DissSearchString = request.DissSearchString;

            var student = await _StudentAlumniDirectoriesRespository.UpdateStudentAlumniDirectoriesAsync(studentRequest);
            var dto = this.ConvertToDTO(student);

            return dto;
        }

        private DirectoriesResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DirectoriesResponse student)
        {

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ReportOptions> reportOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ReportOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ReportOptions respReportOptions in student.ReportOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ReportOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ReportOptions();
                option.ReportCode = respReportOptions.ReportCode;
                option.ReportDesc = respReportOptions.ReportDesc;
                reportOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FormatOptions> formatOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FormatOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FormatOptions respFormatOptions in student.FormatOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FormatOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FormatOptions();
                option.FormatCode = respFormatOptions.FormatCode;
                option.FormatDesc = respFormatOptions.FormatDesc;
                formatOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.SortOptions> sortOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.SortOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.SortOptions respSortOptions in student.SortOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.SortOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.SortOptions();
                option.SortCode = respSortOptions.SortCode;
                option.SortDesc = respSortOptions.SortDesc;
                sortOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.DeptOptions> deptOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.DeptOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.DeptOptions respDeptOptions in student.DeptOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.DeptOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.DeptOptions();
                option.DeptCode = respDeptOptions.DeptCode;
                option.DeptDesc = respDeptOptions.DeptDesc;
                deptOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ProgOptions> progOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ProgOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ProgOptions respProgOptions in student.ProgOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ProgOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ProgOptions();
                option.ProgCode = respProgOptions.ProgCode;
                option.ProgDesc = respProgOptions.ProgDesc;
                progOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FacOptions> facOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FacOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.FacOptions respFacOptions in student.FacOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FacOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.FacOptions();
                option.FacCode = respFacOptions.FacCode;
                option.FacDesc = respFacOptions.FacDesc;
                facOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.StateOptions> stateOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.StateOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.StateOptions respStateOptions in student.StateOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.StateOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.StateOptions();
                option.StateCode = respStateOptions.StateCode;
                option.StateDesc = respStateOptions.StateDesc;
                stateOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.CountryOptions> countryOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.CountryOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.CountryOptions respCountryOptions in student.CountryOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.CountryOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.CountryOptions();
                option.CountryCode = respCountryOptions.CountryCode;
                option.CountryDesc = respCountryOptions.CountryDesc;
                countryOptions.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ConcentraOptions> concentraOptions = new List<Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ConcentraOptions>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.StudentAlumniDirectories.ConcentraOptions respConcentraOptions in student.ConcentraOptions)
            {
                Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ConcentraOptions option = new Ellucian.Colleague.Dtos.F09.StudentAlumniDirectories.ConcentraOptions();
                option.ConcentraCode = respConcentraOptions.ConcentraCode;
                option.ConcentraDesc = respConcentraOptions.ConcentraDesc;
                concentraOptions.Add(option);
            }

            var dto = new DirectoriesResponseDto
            (
                student.RespondType,
                student.HtmlReport,
                student.ErrorMsg,
                reportOptions,
                formatOptions,
                sortOptions,
                deptOptions,
                progOptions,
                facOptions,
                stateOptions,
                countryOptions,
                concentraOptions
            );

            return dto;
        }
    }
}
