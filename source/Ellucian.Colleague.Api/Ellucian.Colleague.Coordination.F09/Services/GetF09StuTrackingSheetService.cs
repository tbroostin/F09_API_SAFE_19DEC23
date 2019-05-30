using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
// F09 added on 05-20-2019 for Pdf Student Tracking Sheet project
using Ellucian.Colleague.Coordination.Finance.Reports;
using Ellucian.Colleague.Coordination.Base.Utility;
using Microsoft.Reporting.WebForms;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class GetF09StuTrackingSheetService : BaseCoordinationService, IGetF09StuTrackingSheetService
    {
        private readonly IGetF09StuTrackingSheetRepository _GetF09StuTrackingSheetRepository;

        public GetF09StuTrackingSheetService(IAdapterRegistry adapterRegistry, IGetF09StuTrackingSheetRepository GetF09StuTrackingSheetRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._GetF09StuTrackingSheetRepository = GetF09StuTrackingSheetRepository;
        }

        public async Task<GetF09StuTrackingSheetResponseDto> GetF09StuTrackingSheetAsync(string Id)
        {
            var profile = await _GetF09StuTrackingSheetRepository.GetF09StuTrackingSheetAsync(Id);
            var dto = this.ConvertToDTO(profile);

            return dto;
        }

        private GetF09StuTrackingSheetResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.GetF09StuTrackingSheetResponse student)
        {
            var dto = new GetF09StuTrackingSheetResponseDto
            (
                student.Html
            );

            return dto;
        }
        public async Task<PdfTrackingSheetResponseDto> GetPdfStudentTrackingSheetAsync(string personId)
        {
            var profile = await _GetF09StuTrackingSheetRepository.GetPdfStudentTrackingSheetAsync(personId);
            var dto = this.ConvertToPdfTrackingSheetResponseDto(profile);

            return dto;
        }

        private PdfTrackingSheetResponseDto ConvertToPdfTrackingSheetResponseDto(Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PdfTrackingSheetResponse student)
        {
            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Phones> phones = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Phones>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Phones respOptions in student.Phones)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Phones option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Phones();
                option.PhoneNo = respOptions.PhoneNo;
                option.PhoneExt = respOptions.PhoneExt;
                option.PhoneType = respOptions.PhoneType;
                phones.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Emails> emails = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Emails>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Emails respOptions in student.Emails)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Emails option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Emails();
                option.EmailAddrs = respOptions.EmailAddrs;
                option.EmailTypes = respOptions.EmailTypes;
                option.EmailAuth = respOptions.EmailAuth;
                emails.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Programs> programs = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Programs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Programs respOptions in student.Programs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Programs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Programs();
                option.Prog = respOptions.Prog;
                option.ProgStartDate = respOptions.ProgStartDate;
                option.ProgStatus = respOptions.ProgStatus;
                option.ProgYearsEnrl = respOptions.ProgYearsEnrl;
                option.ProgAntCmpl = respOptions.ProgAntCmpl;
                option.ProgFac = respOptions.ProgFac;
                option.ProgAd = respOptions.ProgAd;
                programs.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.ProgExtras> progExtras = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.ProgExtras>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.ProgExtras respOptions in student.ProgExtras)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.ProgExtras option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.ProgExtras();
                option.ProgExtraDesc = respOptions.ProgExtraDesc;
                option.ProgExtraStartDate = respOptions.ProgExtraStartDate;
                progExtras.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.TEs> tes = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.TEs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.TEs respOptions in student.TEs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.TEs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.TEs();
                option.TeInst = respOptions.TeInst;
                option.TeTranCourse = respOptions.TeTranCourse;
                option.TeTranCredit = respOptions.TeTranCredit;
                option.TeTranGrade = respOptions.TeTranGrade;
                option.TeEquivCourse = respOptions.TeEquivCourse;
                option.TeEquivCredit = respOptions.TeEquivCredit;
                option.TeEquivStatus = respOptions.TeEquivStatus;
                tes.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.GRs> grs = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.GRs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.GRs respOptions in student.GRs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.GRs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.GRs();
                option.GrStcTerm = respOptions.GrStcTerm;
                option.GrStcCrsName = respOptions.GrStcCrsName;
                option.GrStcTitle = respOptions.GrStcTitle;
                option.GrStcCredAtt = respOptions.GrStcCredAtt;
                option.GrStcCredCmpl = respOptions.GrStcCredCmpl;
                option.GrStcGrade = respOptions.GrStcGrade;
                option.GrStcFaculty = respOptions.GrStcFaculty;
                grs.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.SAs> sas = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.SAs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.SAs respOptions in student.SAs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.SAs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.SAs();
                option.SaStcCrsName = respOptions.SaStcCrsName;
                option.SaStcTitle = respOptions.SaStcTitle;
                option.SaEndDate = respOptions.SaEndDate;
                sas.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.CEs> ces = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.CEs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.CEs respOptions in student.CEs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.CEs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.CEs();
                option.CeStcCrsName = respOptions.CeStcCrsName;
                option.CeStcTitle = respOptions.CeStcTitle;
                option.CeEndDate = respOptions.CeEndDate;
                option.CeStcCredCmpl = respOptions.CeStcCredCmpl;
                ces.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.KAs> kas = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.KAs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.KAs respOptions in student.KAs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.KAs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.KAs();
                option.KaCrsName = respOptions.KaCrsName;
                option.KaTitle = respOptions.KaTitle;
                option.KaFaculty = respOptions.KaFaculty;
                option.KaGrade = respOptions.KaGrade;
                option.KaTerm = respOptions.KaTerm;
                option.KaCred = respOptions.KaCred;
                kas.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.IPs> ips = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.IPs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.IPs respOptions in student.IPs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.IPs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.IPs();
                option.IpCrsName = respOptions.IpCrsName;
                option.IpTitle = respOptions.IpTitle;
                option.IpTerm = respOptions.IpTerm;
                option.IpFaculty = respOptions.IpFaculty;
                option.IpCred = respOptions.IpCred;
                ips.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.INs> ins = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.INs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.INs respOptions in student.INs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.INs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.INs();
                option.InSite = respOptions.InSite;
                option.InStartDate = respOptions.InStartDate;
                option.InEndDate = respOptions.InEndDate;
                option.InHours = respOptions.InHours;
                ins.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.RPs> rps = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.RPs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.RPs respOptions in student.RPs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.RPs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.RPs();
                option.RpSite = respOptions.RpSite;
                option.RpHours = respOptions.RpHours;
                option.RpProjectTitle = respOptions.RpProjectTitle;
                rps.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Ms> ms = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Ms>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Ms respOptions in student.Ms)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Ms option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Ms();
                option.MSite = respOptions.MSite;
                option.MHours = respOptions.MHours;
                option.MProjectTitle = respOptions.MProjectTitle;
                ms.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.DisSteps> disSteps = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.DisSteps>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.DisSteps respOptions in student.DisSteps)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.DisSteps option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.DisSteps();
                option.DisStep = respOptions.DisStep;
                option.DisStepDesc = respOptions.DisStepDesc;
                option.DisStepApprDate = respOptions.DisStepApprDate;
                disSteps.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Leaves> leaves = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Leaves>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Leaves respOptions in student.Leaves)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Leaves option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Leaves();
                option.LeaveStartDate = respOptions.LeaveStartDate;
                option.LeaveEndDate = respOptions.LeaveEndDate;
                option.LeaveDesc = respOptions.LeaveDesc;
                leaves.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Evals> evals = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Evals>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.Evals respOptions in student.Evals)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Evals option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.Evals();
                option.EvalStartDate = respOptions.EvalStartDate;
                option.EvalEndDate = respOptions.EvalEndDate;
                option.EvalProg = respOptions.EvalProg;
                option.EvalStatus = respOptions.EvalStatus;
                evals.Add(option);
            }

            List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.PRs> prs = new List<Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.PRs>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.PdfStudentTrackingSheet.PRs respOptions in student.PRs)
            {
                Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.PRs option = new Ellucian.Colleague.Dtos.F09.PdfStudentTrackingSheet.PRs();
                option.PrSite = respOptions.PrSite;
                option.PrStartDate = respOptions.PrStartDate;
                option.AlPrEndDate = respOptions.AlPrEndDate;
                option.AlPrHours = respOptions.AlPrHours;
                prs.Add(option);
            }

            var dto = new PdfTrackingSheetResponseDto
            (
                student.Id,
                student.StuName,
                student.StuAddr,
                student.BusAddr,
                student.FamiliarName,
                student.GradProgAdvisor,
                student.TranEquivText,
                student.TkResdyHours,
                student.TkReshrHours,
                student.ADisChair,
                student.DisAd,
                student.DisFacRdr,
                student.DisStuRdr,
                student.DisConFac,
                student.DisExtExam,
                student.DisPrApprDate,
                student.DisPrOralDate,
                student.DisReApprDate,
                student.DisReWaivDate,
                student.AdLabel,
                student.Degrees,
                phones,
                emails,
                programs,
                progExtras,
                tes,
                grs,
                sas,
                ces,
                kas,
                ips,
                ins,
                rps,
                ms,
                disSteps,
                leaves,
                evals,
                prs
            );

            return dto;
        }

        // F09 added on 05-04-2019 for Demo Reporting Project
        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="responseDto">Response DTO to use as the data source for producing the student tracking sheet report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <param name="utility">Report Parameter Utility</param>
        /// <returns>A byte array representation of a PDF student tracking sheet report.</returns>
        public byte[] GetStudentTrackingSheetReport(PdfTrackingSheetResponseDto responseDto, string pathToReport, string pathToResourceFile, string pathToLogo)
        {
            if (responseDto == null)
            {
                throw new ArgumentNullException("responseDto");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }
            if (!File.Exists(pathToResourceFile))
            {
                throw new FileNotFoundException("The statement resource file could not be found.", "pathToResourceFile");
            }

            if (pathToLogo == null) pathToLogo = string.Empty;

            var report = new LocalReport();
            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                // Specify the report parameters
                var utility = new ReportUtility();
                var parameters = utility.BuildReportParametersFromResourceFiles(new List<string>() { pathToResourceFile });

                // Replace FGU logo image path new new logo.
                string newPathToLogo = pathToLogo;
                newPathToLogo = newPathToLogo.Substring(0, newPathToLogo.Replace("/", "\\").LastIndexOf("\\") + 1) + "fgu_logo_3.png";
                parameters.Add(utility.BuildReportParameter("ImagePath", newPathToLogo));
                //parameters.Add(utility.BuildReportParameter("ImagePath", pathToLogo.Replace("fgu_logo_2", "fgu_logo_3")));

                parameters.Add(utility.BuildReportParameter("StudentId", responseDto.Id));
                parameters.Add(utility.BuildReportParameter("StudentName", responseDto.StuName));
                parameters.Add(utility.BuildReportParameter("DateGenerated", DateTime.Now.ToShortDateString()));
                parameters.Add(utility.BuildReportParameter("StudentAddress", responseDto.StuAddr));
                parameters.Add(utility.BuildReportParameter("BusinessAddress", responseDto.BusAddr));
                parameters.Add(utility.BuildReportParameter("FamiliarName", responseDto.FamiliarName));

                // Email Info
                DataSet dsEmails = new DataSet();
                if (responseDto.Emails != null && responseDto.Emails.Count > 0)
                {
                    dsEmails = ConvertToDataSet(responseDto.Emails.ToArray());
                }
                else
                {
                    List<Emails> emails = new List<Emails>();
                    emails.Add(new Emails());
                    dsEmails = ConvertToDataSet(emails.ToArray());
                }
                report.DataSources.Add(new ReportDataSource("Emails", dsEmails.Tables[0]));

                // Phone Info
                DataSet dsPhones = new DataSet();
                if (responseDto.Phones != null && responseDto.Phones.Count > 0)
                {
                    dsPhones = ConvertToDataSet(responseDto.Phones.ToArray());
                }
                else
                {
                    List<Phones> phones = new List<Phones>();
                    phones.Add(new Phones());
                    dsPhones = ConvertToDataSet(phones.ToArray());
                }
                report.DataSources.Add(new ReportDataSource("Phones", dsPhones.Tables[0]));

                parameters.Add(utility.BuildReportParameter("GraduateProgramAdvisor", responseDto.GradProgAdvisor));

                // Academic Programs
                DataSet dsPrograms = new DataSet();
                if (responseDto.Programs != null && responseDto.Programs.Count > 0)
                {
                    dsPrograms = ConvertToDataSet(responseDto.Programs.ToArray());
                }
                else
                {
                    List<Programs> programs = new List<Programs>();
                    programs.Add(new Programs());
                    dsPrograms = ConvertToDataSet(programs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("Programs", dsPrograms.Tables[0]));

                // Academic Program Extras
                DataSet dsProgExtras = new DataSet();
                if (responseDto.ProgExtras != null && responseDto.ProgExtras.Count > 0)
                {
                    dsProgExtras = ConvertToDataSet(responseDto.ProgExtras.ToArray());
                }
                else
                {
                    List<ProgExtras> ProgExtras = new List<ProgExtras>();
                    ProgExtras.Add(new ProgExtras());
                    dsProgExtras = ConvertToDataSet(ProgExtras.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("ProgExtras", dsProgExtras.Tables[0]));

                // Transcripted Work
                DataSet dsGRs = new DataSet();
                if (responseDto.GRs != null && responseDto.GRs.Count > 0)
                {
                    dsGRs = ConvertToDataSet(responseDto.GRs.ToArray());
                }
                else
                {
                    List<GRs> GRs = new List<GRs>();
                    GRs.Add(new GRs());
                    dsGRs = ConvertToDataSet(GRs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("GRs", dsGRs.Tables[0]));

                // Completed Knowledge Areas
                DataSet dsKAs = new DataSet();
                if (responseDto.KAs != null && responseDto.KAs.Count > 0)
                {
                    dsKAs = ConvertToDataSet(responseDto.KAs.ToArray());
                }
                else
                {
                    List<KAs> KAs = new List<KAs>();
                    KAs.Add(new KAs());
                    dsKAs = ConvertToDataSet(KAs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("KAs", dsKAs.Tables[0]));

                // Work in Progress
                DataSet dsIPs = new DataSet();
                if (responseDto.IPs != null && responseDto.IPs.Count > 0)
                {
                    dsIPs = ConvertToDataSet(responseDto.IPs.ToArray());
                }
                else
                {
                    List<IPs> IPs = new List<IPs>();
                    IPs.Add(new IPs());
                    dsIPs = ConvertToDataSet(IPs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("IPs", dsIPs.Tables[0]));

                // Site in Hours
                // Internship Site(s)
                DataSet dsINs = new DataSet();
                if (responseDto.INs != null && responseDto.INs.Count > 0)
                {
                    dsINs = ConvertToDataSet(responseDto.INs.ToArray());
                }
                else
                {
                    List<INs> INs = new List<INs>();
                    INs.Add(new INs());
                    dsINs = ConvertToDataSet(INs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("INs", dsINs.Tables[0]));

                // Clinical Practice Site(s)
                DataSet dsPRs = new DataSet();
                if (responseDto.PRs != null && responseDto.PRs.Count > 0)
                {
                    dsPRs = ConvertToDataSet(responseDto.PRs.ToArray());
                }
                else
                {
                    List<PRs> PRs = new List<PRs>();
                    PRs.Add(new PRs());
                    dsPRs = ConvertToDataSet(PRs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("PRs", dsPRs.Tables[0]));

                // Research Practice Site(s)
                DataSet dsRPs = new DataSet();
                if (responseDto.RPs != null && responseDto.RPs.Count > 0)
                {
                    dsRPs = ConvertToDataSet(responseDto.RPs.ToArray());
                }
                else
                {
                    List<RPs> RPs = new List<RPs>();
                    RPs.Add(new RPs());
                    dsRPs = ConvertToDataSet(RPs.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("RPs", dsRPs.Tables[0]));

                // Convert report data to be sent to the report
                DataSet dsMs = new DataSet();
                if (responseDto.Ms != null && responseDto.Ms.Count > 0)
                {
                    dsMs = ConvertToDataSet(responseDto.Ms.ToArray());
                }
                else
                {
                    List<Ms> Ms = new List<Ms>();
                    Ms.Add(new Ms());
                    dsMs = ConvertToDataSet(Ms.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("Ms", dsMs.Tables[0]));

                // Residency and Research Hours
                parameters.Add(utility.BuildReportParameter("ResidencyHours", responseDto.TkResdyHours));
                parameters.Add(utility.BuildReportParameter("ResearchHours", responseDto.TkReshrHours));

                // Dissertation
                parameters.Add(utility.BuildReportParameter("ADisChair", responseDto.ADisChair));
                parameters.Add(utility.BuildReportParameter("AdLabel", responseDto.AdLabel));
                parameters.Add(utility.BuildReportParameter("DisFacRdr", responseDto.DisFacRdr));
                parameters.Add(utility.BuildReportParameter("DisStuRdr", responseDto.DisStuRdr));
                parameters.Add(utility.BuildReportParameter("DisConFac", responseDto.DisConFac));
                parameters.Add(utility.BuildReportParameter("DisExtExam", responseDto.DisExtExam));

                parameters.Add(utility.BuildReportParameter("DisPrApprDate", responseDto.DisPrApprDate));
                parameters.Add(utility.BuildReportParameter("DisPrOralDate", responseDto.DisPrOralDate));
                parameters.Add(utility.BuildReportParameter("DisReApprDate", responseDto.DisReApprDate));
                parameters.Add(utility.BuildReportParameter("DisReWaivDate", responseDto.DisReWaivDate));

                // Dissertation Steps
                DataSet dsDisSteps = new DataSet();
                if (responseDto.DisSteps != null && responseDto.DisSteps.Count > 0)
                {
                    dsDisSteps = ConvertToDataSet(responseDto.DisSteps.ToArray());
                }
                else
                {
                    List<DisSteps> DisSteps = new List<DisSteps>();
                    DisSteps.Add(new DisSteps());
                    dsDisSteps = ConvertToDataSet(DisSteps.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("DisSteps", dsDisSteps.Tables[0]));

                // Degrees Awarded
                string degrees = string.Empty;
                if (responseDto.Degrees != null && responseDto.Degrees.Count > 0)
                {
                    foreach (string degree in responseDto.Degrees)
                        degrees += degree + "<br>";

                    if (!String.IsNullOrEmpty(degrees))
                        degrees.TrimEnd('>', 'r', 'b', '<');
                }
                parameters.Add(utility.BuildReportParameter("Degrees", degrees));
                parameters.Add(utility.BuildReportParameter("Degrees2", responseDto.Degrees));

                // Leaves
                DataSet dsLeaves = new DataSet();
                if (responseDto.Leaves != null && responseDto.Leaves.Count > 0)
                {
                    dsLeaves = ConvertToDataSet(responseDto.Leaves.ToArray());
                }
                else
                {
                    List<Leaves> Leaves = new List<Leaves>();
                    Leaves.Add(new Leaves());
                    dsLeaves = ConvertToDataSet(Leaves.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("Leaves", dsLeaves.Tables[0]));

                // Evals
                DataSet dsEvals = new DataSet();
                if (responseDto.Evals != null && responseDto.Evals.Count > 0)
                {
                    dsEvals = ConvertToDataSet(responseDto.Evals.ToArray());
                }
                else
                {
                    List<Evals> Evals = new List<Evals>();
                    Evals.Add(new Evals());
                    dsEvals = ConvertToDataSet(Evals.ToArray());
                }
                // Add data to the report
                report.DataSources.Add(new ReportDataSource("Evals", dsEvals.Tables[0]));

                // Transfer Credits
                parameters.Add(utility.BuildReportParameter("TranEquivText", responseDto.TranEquivText));

                // Set the report parameters
                report.SetParameters(parameters);

                // Set up some options for the report
                string mimeType = string.Empty;
                string encoding;
                string fileNameExtension;
                Warning[] warnings;
                string[] streams;

                // Render the report as a byte array
                var renderedBytes = report.Render(
                    PdfReportConstants.ReportType,
                    PdfReportConstants.DeviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);

                return renderedBytes;
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to generate student tracking sheet report.");
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
        }

        /// <summary>
        /// Transform stored data collection into XML.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private DataSet ConvertToDataSet(Object[] values)
        {
            DataSet ds = new DataSet();
            Type temp = values.GetType();
            XmlSerializer xmlSerializer = new XmlSerializer(values.GetType());
            StringWriter writer = new StringWriter();

            xmlSerializer.Serialize(writer, values);
            StringReader reader = new StringReader(writer.ToString());
            ds.ReadXml(reader);
            return ds;
        }
    }
}

