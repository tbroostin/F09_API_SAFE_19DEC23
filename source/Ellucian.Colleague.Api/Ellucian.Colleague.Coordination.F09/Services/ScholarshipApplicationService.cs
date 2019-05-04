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
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
// F09 added on 05-04-2019 for Demo Reporting Project
using Ellucian.Colleague.Coordination.Finance.Reports;
using Ellucian.Colleague.Coordination.Base.Utility;
using Microsoft.Reporting.WebForms;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class ScholarshipApplicationService : BaseCoordinationService, IScholarshipApplicationService
    {
        private readonly IScholarshipApplicationRepository _ScholarshipApplicationRepository;

        public ScholarshipApplicationService(IAdapterRegistry adapterRegistry, IScholarshipApplicationRepository ScholarshipApplicationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            this._ScholarshipApplicationRepository = ScholarshipApplicationRepository;
        }

        public async Task<ScholarshipApplicationResponseDto> GetScholarshipApplicationAsync(string personId)
        {
            var application = await _ScholarshipApplicationRepository.GetScholarshipApplicationAsync(personId);
            var dto = this.ConvertToDTO(application);

            return dto;
        }

        public async Task<ScholarshipApplicationResponseDto> UpdateScholarshipApplicationAsync(Ellucian.Colleague.Dtos.F09.ScholarshipApplicationRequestDto request)
        {
            Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationRequest applicationRequest = new Domain.F09.Entities.ScholarshipApplicationRequest();
            applicationRequest.Id = request.Id;
            applicationRequest.RequestType = request.RequestType;
            applicationRequest.XfstId = request.XfstId;
            applicationRequest.XfstRefName = request.XfstRefName;
            applicationRequest.XfstSelfRateDesc = request.XfstSelfRateDesc;
            applicationRequest.XfstResearchInt = request.XfstResearchInt;
            applicationRequest.XfstDissTopic = request.XfstDissTopic;
            applicationRequest.XfstFinSit = request.XfstFinSit;
            applicationRequest.XfstSelfRate = request.XfstSelfRate;

            List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards> awards = new List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards>();
            foreach (ScholarshipApplicationAwardsDto reqAward in request.Awards)
            {
                Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards award = new Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards();
                award.Id = reqAward.Id;
                award.Title = reqAward.Title;
                award.Desc = reqAward.Desc;
                award.MinMax = reqAward.MinMax;
                award.AddnlRequ = reqAward.AddnlRequ;
                award.LorEmailRequ = reqAward.LorEmailRequ;
                award.LorEmail = reqAward.LorEmail;
                award.Checked = reqAward.Checked;
                awards.Add(award);
            }

            applicationRequest.Awards = awards;

            List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ> softQs = new List<Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ>();
            foreach (ScholarshipApplicationSoftQDto reqSoftQ in request.SoftQs)
            {
                Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ softQ = new Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ();
                softQ.Code = reqSoftQ.Code;
                softQ.Desc = reqSoftQ.Desc;
                softQ.Checked = reqSoftQ.Checked;
                softQs.Add(softQ);
            }

            applicationRequest.SoftQs = softQs;

            var application = await _ScholarshipApplicationRepository.UpdateScholarshipApplicationAsync(applicationRequest);
            var dto = this.ConvertToDTO(application);

            return dto;
        }

        private ScholarshipApplicationResponseDto ConvertToDTO(Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationResponse application)
        {
            List<ScholarshipApplicationAwardsDto> awards = new List<ScholarshipApplicationAwardsDto>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationAwards respAward in application.Awards)
            {
                ScholarshipApplicationAwardsDto award = new ScholarshipApplicationAwardsDto();
                award.Id = respAward.Id;
                award.Title = respAward.Title;
                award.Desc = respAward.Desc;
                award.MinMax = respAward.MinMax;
                award.AddnlRequ = respAward.AddnlRequ;
                award.LorEmailRequ = respAward.LorEmailRequ;
                award.LorEmail = respAward.LorEmail;
                award.Checked = respAward.Checked;
                awards.Add(award);
            }

            List<ScholarshipApplicationSoftQDto> softQs = new List<ScholarshipApplicationSoftQDto>();
            foreach (Ellucian.Colleague.Domain.F09.Entities.ScholarshipApplicationSoftQ respSoftQ in application.SoftQs)
            {
                ScholarshipApplicationSoftQDto softQ = new ScholarshipApplicationSoftQDto();
                softQ.Code = respSoftQ.Code;
                softQ.Desc = respSoftQ.Desc;
                softQ.Checked = respSoftQ.Checked;
                softQs.Add(softQ);
            }

            var dto = new ScholarshipApplicationResponseDto
            (
                application.Id,
                application.RespondType,
                application.MsgHtml,
                application.SoftQHtml,
                application.StudentName,
                application.StudentEmail,
                application.StudentAddress,
                application.ApplDeadline,
                application.ApplTerm,
                application.XfstId,
                application.XfstPrevSubmit,
                application.XfstRefName,
                application.XfstSelfRateDesc,
                application.XfstResearchInt,
                application.XfstDissTopic,
                application.XfstFinSit,
                application.XfstSelfRate,
                application.Step1Html,
                application.Step2Html,
                application.Step3Html,
                application.Step4Html,
                application.ErrorMsg,
                awards,
                softQs
            );

            return dto;
        }

        // F09 added on 05-04-2019 for Demo Reporting Project
        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="statementDto">StudentStatement DTO to use as the data source for producing the student statement report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <param name="utility">Report Parameter Utility</param>
        /// <returns>A byte array representation of a PDF student statement report.</returns>
        public byte[] GetStudentStatementReport(ScholarshipApplicationStudentStatementDto statementDto, string pathToReport, string pathToResourceFile, string pathToLogo)
        {
            if (statementDto == null)
            {
                throw new ArgumentNullException("statementDto");
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

                parameters.Add(utility.BuildReportParameter("StudentId", statementDto.StudentId));
                parameters.Add(utility.BuildReportParameter("StudentName", statementDto.StudentName));
                parameters.Add(utility.BuildReportParameter("ImagePath", pathToLogo));
                parameters.Add(utility.BuildReportParameter("DateGenerated", statementDto.Date.ToShortDateString()));
                parameters.Add(utility.BuildReportParameter("DateFormat", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern));

                // Set the report parameters
                report.SetParameters(parameters);

                // Convert report data to be sent to the report
                DataSet ds_Awards = ConvertToDataSet(statementDto.Awards.ToArray());

                // Add data to the report
                report.DataSources.Add(new ReportDataSource("Awards", ds_Awards.Tables[0]));

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
                logger.Error(e, "Unable to generate student statement.");
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
