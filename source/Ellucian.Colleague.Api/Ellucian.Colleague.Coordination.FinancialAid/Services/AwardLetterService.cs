/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using Ellucian.Colleague.Coordination.FinancialAid.Reports;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination Service for student AwardLetters
    /// </summary>
    [RegisterType]
    public class AwardLetterService : AwardYearCoordinationService, IAwardLetterService
    {
        private readonly IAwardLetterRepository awardLetterRepository;
        private readonly IAwardLetterHistoryRepository awardLetterHistoryRepository;
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly IStudentAwardRepository studentAwardRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IApplicantRepository applicantRepository;
        private readonly IFafsaRepository fafsaRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="awardLetterRepository">AwardLetterRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public AwardLetterService(IAdapterRegistry adapterRegistry,
            IAwardLetterRepository awardLetterRepository,
            IAwardLetterHistoryRepository awardLetterHistoryRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IStudentAwardRepository studentAwardRepository,
            IStudentRepository studentRepository,
            IApplicantRepository applicantRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IFinancialAidOfficeRepository officeRepository,
            IFafsaRepository fafsaRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, officeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.awardLetterRepository = awardLetterRepository;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.studentAwardRepository = studentAwardRepository;
            this.awardLetterHistoryRepository = awardLetterHistoryRepository;
            this.studentRepository = studentRepository;
            this.applicantRepository = applicantRepository;
            this.fafsaRepository = fafsaRepository;
            this.configurationRepository = configurationRepository;
        }

        #region Obsolete methods

        /// <summary>
        /// Get a student's award letters for all the student's active award years that have award data.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get award letters</param>
        /// <returns>A list of award letters. </returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        public IEnumerable<Dtos.FinancialAid.AwardLetter> GetAwardLetters(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = Task.Run(async () => await GetActiveStudentAwardYearEntitiesAsync(studentId)).GetAwaiter().GetResult();
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var fafsaRecords = Task.Run(async() => await GetStudentFafsaRecordsAsync(studentId, studentAwardYears)).GetAwaiter().GetResult();

            var awardLetterEntities = awardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
            if (awardLetterEntities == null || awardLetterEntities.Count() == 0)
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var filteredAwardLetters = ApplyConfigurationService.FilterAwardLetters(awardLetterEntities);
            if (filteredAwardLetters == null || filteredAwardLetters.Count() == 0)
            {
                var message = string.Format("No award letters are active in configuration for student {0}", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var filteredAwardLetterAwards = Task.Run(async() => await GetFilteredAwardLetterAwardsAsync(studentId, studentAwardYears)).GetAwaiter().GetResult();
            if (filteredAwardLetterAwards == null || !filteredAwardLetterAwards.Any())
            {
                var message = string.Format("Student {0} has no awards or configuration filtered out all StudentAwards.", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var studentOrApplicant = Task.Run(async() => await GetStudentOrApplicantAsync(studentId)).GetAwaiter().GetResult();

            //if the studentOrApplicant is still null, throw an exception
            if (studentOrApplicant == null)
            {
                var message = string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(_adapterRegistry, logger);
            var awardLetterDtoList = new List<Dtos.FinancialAid.AwardLetter>();
            foreach (var awardLetterEntity in filteredAwardLetters)
            {
                var studentAwardsForYear = filteredAwardLetterAwards.Where(a => a.StudentAwardYear.Code == awardLetterEntity.AwardYear.Code).ToList();
                if (studentAwardsForYear.Any())
                {
                    awardLetterDtoList.Add(awardLetterEntityAdapter.MapToType(awardLetterEntity, studentAwardsForYear, studentOrApplicant));
                }
            }
            return awardLetterDtoList;
        }

        /// <summary>
        /// Get a student's award letters for all the student's active award years. A letter object is returned even if 
        /// it does not have any awards associated with it
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get award letters</param>
        /// <returns>A list of award letters. </returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetters3Async")]
        public IEnumerable<Dtos.FinancialAid.AwardLetter> GetAwardLetters2(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYears = Task.Run(async() => await GetActiveStudentAwardYearEntitiesAsync(studentId)).GetAwaiter().GetResult();
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var fafsaRecords = Task.Run(async() => await GetStudentFafsaRecordsAsync(studentId, studentAwardYears)).GetAwaiter().GetResult();

            var awardLetterEntities = awardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);
            if (awardLetterEntities == null || awardLetterEntities.Count() == 0)
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var filteredAwardLetters = ApplyConfigurationService.FilterAwardLetters(awardLetterEntities);
            if (filteredAwardLetters == null || filteredAwardLetters.Count() == 0)
            {
                var message = string.Format("No award letters are active in configuration for student {0}", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var filteredAwardLetterAwards = Task.Run(async() => await GetFilteredAwardLetterAwardsAsync(studentId, studentAwardYears)).GetAwaiter().GetResult();
            if (filteredAwardLetterAwards == null)
            {
                var message = string.Format("Student {0} has no awards", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter>();
            }

            var studentOrApplicant = Task.Run(async() => await GetStudentOrApplicantAsync(studentId)).GetAwaiter().GetResult();

            //if the studentOrApplicant is still null, throw an exception
            if (studentOrApplicant == null)
            {
                var message = string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(_adapterRegistry, logger);
            var awardLetterDtoList = new List<Dtos.FinancialAid.AwardLetter>();
            foreach (var awardLetterEntity in filteredAwardLetters)
            {
                var studentAwardsForYear = filteredAwardLetterAwards.Where(a => a.StudentAwardYear.Code == awardLetterEntity.AwardYear.Code);
                awardLetterDtoList.Add(awardLetterEntityAdapter.MapToType(awardLetterEntity, studentAwardsForYear, studentOrApplicant));
            }

            return awardLetterDtoList;
        }

        /// <summary>
        /// Get a single student's award letter for the given year.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An award letter DTO</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        public AwardLetter GetAwardLetters(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            var awardLetterReportDto = GetAwardLetters(studentId).FirstOrDefault(a => a.AwardYearCode == awardYear);
            if (awardLetterReportDto == null)
            {
                var message = string.Format("No award letter exists or is not active in configuration for {0} for student {1}", awardYear, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return awardLetterReportDto;
        }

        /// <summary>
        /// Get a single student's award letter for the given year. Award letter object is returned
        /// even if no awards are associated with the letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An award letter DTO</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetters3Async")]
        public AwardLetter GetAwardLetters2(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            var awardLetterReportDto = GetAwardLetters2(studentId).FirstOrDefault(a => a.AwardYearCode == awardYear);
            if (awardLetterReportDto == null)
            {
                var message = string.Format("No award letter exists or is not active in configuration for {0} for student {1}", awardYear, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return awardLetterReportDto;
        }

        /// <summary>
        /// Get a single student's award letter for the given year as a byte array that represents a PDF report.
        /// The data displayed on the report can be retrieved from the GetAwardLetters(studentId, awardYear) method.
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="pathToReport">The path on the server to the report file that defines the award letter report</param>
        /// <param name="pathToLogo">The path on the server to an image file that represents an institution's logo for display on the report.</param>
        /// <returns>A byte array representation of an award letter report.</returns>
        [Obsolete("Obsolete as of API 1.9. Use GetAwardLetters2")]
        public byte[] GetAwardLetters(string studentId, string awardYear, string pathToReport, string pathToLogo)
        {
            var awardLetterDto = GetAwardLetters(studentId, awardYear);
            return GetAwardLetters(awardLetterDto, pathToReport, pathToLogo);
        }

        /// <summary>
        /// Get a single student's award letter for the given year as a byte array that represents a PDF report.
        /// The data displayed on the report can be retrieved from the GetAwardLetters2(studentId, awardYear) method.
        /// Award letter report is being returned even if no awards are associated with the award letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="pathToReport">The path on the server to the .rdlc file that defines the award letter report</param>
        /// <param name="pathToLogo">The path on the server to an image file that represents an institution's logo for display on the report.</param>
        /// <returns>A byte array representation of an award letter report.</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetterReport3")]
        public byte[] GetAwardLetters2(string studentId, string awardYear, string pathToReport, string pathToLogo)
        {
            var awardLetterDto = GetAwardLetters2(studentId, awardYear);
            return GetAwardLetters(awardLetterDto, pathToReport, pathToLogo);
        }

        /// <summary>
        /// Get the byte array representation as a PDF of the given AwardLetter DTO
        /// </summary>
        /// <param name="awardLetter">AwardLetter DTO to use as the data source for producing the award letter report.</param>
        /// <param name="pathToLogo">The path on the server to the report template</param>
        /// <param name="pathToReport">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        [Obsolete("Obsolete as of API 1.10. Use GetAwardLetterReport2")]
        public byte[] GetAwardLetters(AwardLetter awardLetterDto, string pathToReport, string pathToLogo)
        {
            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }

            if (pathToLogo == null) pathToLogo = string.Empty;

            var report = new LocalReport();

            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                var parameters = new List<ReportParameter>();
                parameters.Add(new ReportParameter("IsOfficeActive", awardLetterDto.IsContactBlockActive.ToString()));
                parameters.Add(new ReportParameter("IsNeedActive", awardLetterDto.IsNeedBlockActive.ToString()));
                parameters.Add(new ReportParameter("NumAwardPeriodColumns", awardLetterDto.NumberAwardPeriodColumns.ToString()));
                parameters.Add(new ReportParameter("AwardColumnHeader", awardLetterDto.AwardColumnHeader));
                parameters.Add(new ReportParameter("TotalColumnHeader", awardLetterDto.TotalColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod1ColumnHeader", awardLetterDto.AwardPeriod1ColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod2ColumnHeader", awardLetterDto.AwardPeriod2ColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod3ColumnHeader", awardLetterDto.AwardPeriod3ColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod4ColumnHeader", awardLetterDto.AwardPeriod4ColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod5ColumnHeader", awardLetterDto.AwardPeriod5ColumnHeader));
                parameters.Add(new ReportParameter("AwardPeriod6ColumnHeader", awardLetterDto.AwardPeriod6ColumnHeader));
                parameters.Add(new ReportParameter("LogoPath", pathToLogo));
                parameters.Add(new ReportParameter("LabelDate", "Date:"));
                parameters.Add(new ReportParameter("LabelStudentId", "Student ID:"));
                parameters.Add(new ReportParameter("LabelAwardYear", "Award Year:"));
                parameters.Add(new ReportParameter("LabelBudget", "Budget:"));
                parameters.Add(new ReportParameter("LabelEfc", "EFC:"));
                parameters.Add(new ReportParameter("LabelNeed", "Need:"));
                parameters.Add(new ReportParameter("CreatedDate", awardLetterDto.Date.ToShortDateString()));
                parameters.Add(new ReportParameter("IsHousingCodeActive", awardLetterDto.IsHousingCodeActive.ToString()));
                parameters.Add(new ReportParameter("LabelHousing", "Housing:"));
                report.SetParameters(parameters);

                report.DataSources.Add(new ReportDataSource("AwardLetter", new List<Dtos.FinancialAid.AwardLetter>() { awardLetterDto }));
                report.DataSources.Add(new ReportDataSource("AwardLetterAwards", awardLetterDto.AwardTableRows));
                report.DataSources.Add(new ReportDataSource("StudentAddress", awardLetterDto.StudentAddress));
                report.DataSources.Add(new ReportDataSource("OfficeAddress", awardLetterDto.ContactAddress));

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
            catch(Exception ex)
            {
                logger.Error(ex, "Unable to generate award letter report.");
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
        /// This method updates an Award Letter. The awardYear, StudentId attributes of the input awardLetter DTO are required.
        /// Only the value of AcceptedDate is processed.
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        /// <exception cref="ArgumentNullException">Thrown if the awardLetter argument is null</exception>
        /// <exception cref="PermissionsException">Thrown if the Current User tries to update an award letter for someone other than self</exception>
        public Dtos.FinancialAid.AwardLetter UpdateAwardLetter(Dtos.FinancialAid.AwardLetter awardLetter)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.StudentId))
            {
                var message = "Input argument awardLetter is invalid. StudentId cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.AwardYearCode))
            {
                var message = "Input argument awardLetter is invalid. AwardYear cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (!UserIsSelf(awardLetter.StudentId))
            {
                var message = string.Format("{0} does not have permission to update award letter for {1}", CurrentUser.PersonId, awardLetter.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYear = Task.Run(async() => await GetActiveStudentAwardYearEntitiesAsync(awardLetter.StudentId)).GetAwaiter().GetResult().FirstOrDefault(y => y.Code == awardLetter.AwardYearCode);
            if (studentAwardYear == null)
            {
                var message = string.Format("Student has no financial aid data for {0}", awardLetter.AwardYearCode);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            //get the Dto-to-Entity adapter and map the awardLetter object to a domain entity
            var awardLetterDtoAdapter = new AwardLetterDtoToEntityAdapter(_adapterRegistry, logger);
            var inputAwardLetterEntity = awardLetterDtoAdapter.MapToType(awardLetter, studentAwardYear);

            var studentAwards = Task.Run(async() => await GetFilteredAwardLetterAwardsAsync(inputAwardLetterEntity.StudentId, new List<Domain.FinancialAid.Entities.StudentAwardYear>() { studentAwardYear }))
                .GetAwaiter().GetResult();
                
            if (studentAwards == null || studentAwards.Count() == 0)
            {
                var message = string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var studentOrApplicantPerson = Task.Run(async() => await GetStudentOrApplicantAsync(inputAwardLetterEntity.StudentId)).GetAwaiter().GetResult();
            if (studentOrApplicantPerson == null)
            {
                var message = string.Format("Cannot create award letter for non-student/non-applicant person {0}.", inputAwardLetterEntity.StudentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var fafsaRecord = Task.Run(async() => await GetStudentFafsaRecordsAsync(awardLetter.StudentId, new List<Domain.FinancialAid.Entities.StudentAwardYear>() { studentAwardYear })).GetAwaiter().GetResult()
                .FirstOrDefault(fr => fr.AwardYear == awardLetter.AwardYearCode);

            //update the award letter
            var updatedAwardLetterEntity = awardLetterRepository.UpdateAwardLetter(inputAwardLetterEntity, studentAwardYear, fafsaRecord);
            if (updatedAwardLetterEntity == null)
            {
                var message = string.Format("Null award letter object returned by repository update method for student {0} award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new Exception(message);
            }

            //get the Entity-to-Dto adapter and return the updatedAwardLetter as a Dto
            var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(_adapterRegistry, logger);
            return awardLetterEntityAdapter.MapToType(updatedAwardLetterEntity, studentAwards, studentOrApplicantPerson);
        }

        /// <summary>
        /// This method updates an Award Letter. The awardYear, StudentId attributes of the input awardLetter DTO are required.
        /// Only the value of AcceptedDate is processed.
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        /// <exception cref="ArgumentNullException">Thrown if the awardLetter argument is null</exception>
        /// <exception cref="PermissionsException">Thrown if the Current User tries to update an award letter for someone other than self</exception>
        [Obsolete("Obsolete as of API 1.22. Use UpdateAwardLetter3Async")]
        public async Task<Dtos.FinancialAid.AwardLetter2> UpdateAwardLetter2Async(Dtos.FinancialAid.AwardLetter2 awardLetter)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.StudentId))
            {
                var message = "Input argument awardLetter is invalid. StudentId cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.AwardLetterYear))
            {
                var message = "Input argument awardLetter is invalid. AwardYear cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (!UserIsSelf(awardLetter.StudentId))
            {
                var message = string.Format("{0} does not have permission to update award letter for {1}", CurrentUser.PersonId, awardLetter.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYear = (await GetActiveStudentAwardYearEntitiesAsync(awardLetter.StudentId)).FirstOrDefault(y => y.Code == awardLetter.AwardLetterYear);
            if (studentAwardYear == null)
            {
                var message = string.Format("Student has no financial aid data for {0}", awardLetter.AwardLetterYear);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            //get the Dto-to-Entity adapter and map the awardLetter object to a domain entity
            var awardLetter2DtoAdapter = new AwardLetter2DtoToEntityAdapter(_adapterRegistry, logger);
            var inputAwardLetterEntity = awardLetter2DtoAdapter.MapToType(awardLetter, studentAwardYear);

            var studentAwards = await GetFilteredAwardLetterAwardsAsync(inputAwardLetterEntity.StudentId, new List<Domain.FinancialAid.Entities.StudentAwardYear>() { studentAwardYear });
            if (studentAwards == null || studentAwards.Count() == 0)
            {
                var message = string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;
            var studentId = inputAwardLetterEntity.StudentId;

            //update the award letter
            var updatedAwardLetterEntity = await awardLetterHistoryRepository.UpdateAwardLetterAsync(studentId, inputAwardLetterEntity, studentAwardYear, allAwards);
            if (updatedAwardLetterEntity == null)
            {
                var message = string.Format("Null award letter object returned by repository update method for student {0} award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new Exception(message);
            }
            
            //get the Entity-to-Dto adapter and return the updatedAwardLetter as a Dto
            var awardLetter2EntityAdapter = new AwardLetter2EntityToDtoAdapter(_adapterRegistry, logger);
            
            return awardLetter2EntityAdapter.MapToType(updatedAwardLetterEntity);
        }


        /// <summary>
        /// Get a student's most recent award letters for all the student's active award years. A letter object is returned even if 
        /// it does not have any awards associated with it
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get award letters</param>
        /// <returns>A list of award letters. </returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetters4Async")]
        public async Task<IEnumerable<Dtos.FinancialAid.AwardLetter2>> GetAwardLetters3Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;

            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
           
            var awardLetterEntities = await awardLetterHistoryRepository.GetAwardLettersAsync(studentId, studentAwardYears, allAwards);
            if (awardLetterEntities == null || awardLetterEntities.Count() == 0)
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter2>();
            }

            var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(_adapterRegistry, logger);
            var awardLetterDtoList = new List<Dtos.FinancialAid.AwardLetter2>();
            foreach (var awardLetterEntity in awardLetterEntities)
            {
                awardLetterDtoList.Add(awardLetterEntityAdapter.MapToType(awardLetterEntity));
            }

            return awardLetterDtoList;
        }

        /// <summary>
        /// Get a single student's award letter for the given year. Award letter object is returned
        /// even if no awards are associated with the letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An award letter2 DTO</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetter4Async")]
        public async Task<AwardLetter2> GetAwardLetter3Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Set variable for creating new AwardLetterHistory records
            // This will be false for a Counselor and a Proxy, but true if it is the student logged in
            bool createAwardLetterHistoryRecord = UserIsSelf(studentId);

            // Get the full list of award codes
            var allAwards = financialAidReferenceDataRepository.Awards;
            
            // Get the list of active StudentAwardYears
            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            
            // Get studentAwardYear for the requested awardYear
            var studentAwardYear = studentAwardYears.FirstOrDefault(s => s.Code == awardYear);
            if (studentAwardYear == null)
            {
                var message = string.Format("Student {0} has no active award year for the award year {1}", studentId, awardYear);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntity = await awardLetterHistoryRepository.GetAwardLetterAsync(studentId, studentAwardYear, allAwards, createAwardLetterHistoryRecord);
            if (awardLetterEntity == null)
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new Dtos.FinancialAid.AwardLetter2();
            }

            var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(_adapterRegistry, logger);
            
            return awardLetterEntityAdapter.MapToType(awardLetterEntity);
        }

        /// <summary>
        /// Gets AwardLetter2 DTO by the specified id
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve award letter history record</param>        
        /// <param name="recordId">award letter history record id</param>
        /// <returns>AwardLetter2 DTO</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLettersById2Async")]
        public async Task<AwardLetter2> GetAwardLetterByIdAsync(string studentId, string recordId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            var allAwards = financialAidReferenceDataRepository.Awards;

            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntity = await awardLetterHistoryRepository.GetAwardLetterByIdAsync(studentId, recordId,
                                    studentAwardYears, allAwards);


            var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(_adapterRegistry, logger);

            return awardLetterEntityAdapter.MapToType(awardLetterEntity);
        }

        /// <summary>
        /// Get a single student's award letter for the given year as a byte array that represents a PDF report.
        /// The data displayed on the report can be retrieved from the GetAwardLetter3Async(studentId, awardYear, recordId) method.
        /// Award letter report is being returned even if no awards are associated with the award letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="pathToReport">The path on the server to the .rdlc file that defines the award letter report</param>
        /// <param name="pathToLogo">The path on the server to an image file that represents an institution's logo for display on the report.</param>
        /// <returns>A byte array representation of an award letter report.</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterReport4Async")]
        public async Task<byte[]> GetAwardLetterReport3Async(AwardLetter2 awardLetterDto, string pathToReport, string pathToLogo)
        {
            var awardLetterConfigurationDto = await GetAwardLetterConfigurationAsync(awardLetterDto);
            return GetAwardLetterReport2(awardLetterDto, awardLetterConfigurationDto, pathToReport, pathToLogo);
        }

        /// <summary>
        /// Get the byte array representation as a PDF of the given AwardLetter DTO
        /// </summary>
        /// <param name="awardLetter">AwardLetter DTO to use as the data source for producing the award letter report.</param>
        /// <param name="pathToLogo">The path on the server to the report template</param>
        /// <param name="pathToReport">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterReport3")]
        public byte[] GetAwardLetterReport2(AwardLetter2 awardLetterDto, AwardLetterConfiguration awardLetterConfigurationDto, string pathToReport, string pathToLogo)
        {

            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            if (awardLetterConfigurationDto == null)
            {
                throw new ArgumentNullException("awardLetterConfigurationDto");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }

            if (pathToLogo == null) pathToLogo = string.Empty;

            var report = new LocalReport();
            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                //Extract all the award periods for the award letter
                var awardLetterAwardPeriods = awardLetterDto.AwardLetterAnnualAwards.Any() ? awardLetterDto.AwardLetterAnnualAwards
                    .SelectMany(ala => ala.AwardLetterAwardPeriods) : new List<AwardLetterAwardPeriod>();

                var distinctAwardPeriodsCount = awardLetterAwardPeriods.Select(p => p.ColumnNumber).Distinct().Count();

                //Convert Environment.NewLine into line breaks for opening and closing paragraphs
                awardLetterDto.OpeningParagraph = awardLetterDto.OpeningParagraph.Replace(Environment.NewLine, "<br />");
                awardLetterDto.ClosingParagraph = awardLetterDto.ClosingParagraph.Replace(Environment.NewLine, "<br />");

                var parameters = new List<ReportParameter>();
                parameters.Add(new ReportParameter("IsContactBlockActive", awardLetterConfigurationDto.IsContactBlockActive.ToString()));
                parameters.Add(new ReportParameter("IsNeedBlockActive", awardLetterConfigurationDto.IsNeedBlockActive.ToString()));
                parameters.Add(new ReportParameter("IsHousingBlockActive", awardLetterConfigurationDto.IsHousingBlockActive.ToString()));
                parameters.Add(new ReportParameter("AwardColumnHeader", awardLetterConfigurationDto.AwardTableTitle));
                parameters.Add(new ReportParameter("TotalColumnHeader", awardLetterConfigurationDto.AwardTotalTitle));
                parameters.Add(new ReportParameter("LogoPath", pathToLogo));
                parameters.Add(new ReportParameter("LabelDate", "Date:"));
                parameters.Add(new ReportParameter("LabelStudentId", "Student ID:"));
                parameters.Add(new ReportParameter("LabelAwardYear", "Award Year:"));
                parameters.Add(new ReportParameter("LabelBudget", "Budget:"));
                parameters.Add(new ReportParameter("LabelEfc", "EFC:"));
                parameters.Add(new ReportParameter("LabelNeed", "Need:"));
                parameters.Add(new ReportParameter("CreatedDate", awardLetterDto.CreatedDate.Value.ToShortDateString()));
                parameters.Add(new ReportParameter("LabelHousing", "Housing:"));
                parameters.Add(new ReportParameter("AreAwardPeriodsPresent", (awardLetterAwardPeriods.Any()).ToString()));
                parameters.Add(new ReportParameter("AreAnnualAwardsAbsent", (!awardLetterDto.AwardLetterAnnualAwards.Any()).ToString()));
                parameters.Add(new ReportParameter("NumAwardPeriodColumns", distinctAwardPeriodsCount.ToString()));
                report.SetParameters(parameters);

                report.DataSources.Add(new ReportDataSource("AwardLetter2", new List<Dtos.FinancialAid.AwardLetter2>() { awardLetterDto }));
                report.DataSources.Add(new ReportDataSource("AwardLetterAnnualAwards", awardLetterDto.AwardLetterAnnualAwards));
                report.DataSources.Add(new ReportDataSource("AwardLetterAwardPeriods", awardLetterAwardPeriods));
                report.DataSources.Add(new ReportDataSource("StudentAddress", awardLetterDto.StudentAddress));
                report.DataSources.Add(new ReportDataSource("OfficeAddress", awardLetterDto.ContactAddress));

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
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate award letter report.");
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// This method updates an Award Letter. The awardYear, StudentId attributes of the input awardLetter DTO are required.
        /// Only the value of AcceptedDate is processed.
        /// </summary>
        /// <param name="awardLetter">The award letter object that contains the updated data</param>
        /// <returns>An award letter object with updated data</returns>
        /// <exception cref="ArgumentNullException">Thrown if the awardLetter argument is null</exception>
        /// <exception cref="PermissionsException">Thrown if the Current User tries to update an award letter for someone other than self</exception>
        public async Task<Dtos.FinancialAid.AwardLetter3> UpdateAwardLetter3Async(Dtos.FinancialAid.AwardLetter3 awardLetter)
        {
            if (awardLetter == null)
            {
                throw new ArgumentNullException("awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.StudentId))
            {
                var message = "Input argument awardLetter is invalid. StudentId cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (string.IsNullOrEmpty(awardLetter.AwardLetterYear))
            {
                var message = "Input argument awardLetter is invalid. AwardYear cannot be null or empty";
                logger.Error(message);
                throw new ArgumentException(message, "awardLetter");
            }

            if (!UserIsSelf(awardLetter.StudentId))
            {
                var message = string.Format("{0} does not have permission to update award letter for {1}", CurrentUser.PersonId, awardLetter.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var studentAwardYear = (await GetActiveStudentAwardYearEntitiesAsync(awardLetter.StudentId)).FirstOrDefault(y => y.Code == awardLetter.AwardLetterYear);
            if (studentAwardYear == null)
            {
                var message = string.Format("Student has no financial aid data for {0}", awardLetter.AwardLetterYear);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            //get the Dto-to-Entity adapter and map the awardLetter object to a domain entity
            var awardLetter3DtoAdapter = new AwardLetter3DtoToEntityAdapter(_adapterRegistry, logger);
            var inputAwardLetterEntity = awardLetter3DtoAdapter.MapToType(awardLetter, studentAwardYear);

            var studentAwards = await GetFilteredAwardLetterAwardsAsync(inputAwardLetterEntity.StudentId, new List<Domain.FinancialAid.Entities.StudentAwardYear>() { studentAwardYear });
            if (studentAwards == null || studentAwards.Count() == 0)
            {
                var message = string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;
            var studentId = inputAwardLetterEntity.StudentId;

            //update the award letter
            var updatedAwardLetterEntity = await awardLetterHistoryRepository.UpdateAwardLetter2Async(studentId, inputAwardLetterEntity, studentAwardYear, allAwards);
            if (updatedAwardLetterEntity == null)
            {
                var message = string.Format("Null award letter object returned by repository update method for student {0} award year {1}", inputAwardLetterEntity.StudentId, studentAwardYear.Code);
                logger.Error(message);
                throw new Exception(message);
            }

            //get the Entity-to-Dto adapter and return the updatedAwardLetter as a Dto
            var awardLetter3EntityAdapter = new AwardLetter3EntityToDtoAdapter(_adapterRegistry, logger);

            return awardLetter3EntityAdapter.MapToType(updatedAwardLetterEntity);
        }


        /// <summary>
        /// Get a student's most recent award letters for all the student's active award years. A letter object is returned even if 
        /// it does not have any awards associated with it
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get award letters</param>
        /// <returns>A list of award letters. </returns>
        public async Task<IEnumerable<Dtos.FinancialAid.AwardLetter3>> GetAwardLetters4Async(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;

            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntities = await awardLetterHistoryRepository.GetAwardLetters2Async(studentId, studentAwardYears, allAwards);
            if (awardLetterEntities == null || !awardLetterEntities.Any())
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new List<Dtos.FinancialAid.AwardLetter3>();
            }

            var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(_adapterRegistry, logger);
            var awardLetterDtoList = new List<Dtos.FinancialAid.AwardLetter3>();
            foreach (var awardLetterEntity in awardLetterEntities)
            {
                awardLetterDtoList.Add(awardLetterEntityAdapter.MapToType(awardLetterEntity));
            }

            return awardLetterDtoList;
        }

        /// <summary>
        /// Get a single student's award letter for the given year. Award letter object is returned
        /// even if no awards are associated with the letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <returns>An award letter3 DTO</returns>
        public async Task<AwardLetter3> GetAwardLetter4Async(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Set variable for creating new AwardLetterHistory records
            // This will be false for a Counselor and a Proxy, but true if it is the student logged in
            bool createAwardLetterHistoryRecord = UserIsSelf(studentId);

            // Get the full list of award codes
            var allAwards = financialAidReferenceDataRepository.Awards;

            // Get the list of active StudentAwardYears
            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            // Get studentAwardYear for the requested awardYear
            var studentAwardYear = studentAwardYears.FirstOrDefault(s => s.Code == awardYear);
            if (studentAwardYear == null)
            {
                var message = string.Format("Student {0} has no active award year for the award year {1}", studentId, awardYear);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntity = await awardLetterHistoryRepository.GetAwardLetter2Async(studentId, studentAwardYear, allAwards, createAwardLetterHistoryRecord);
            if (awardLetterEntity == null)
            {
                var message = string.Format("Student {0} has no award letters", studentId);
                logger.Info(message);
                return new Dtos.FinancialAid.AwardLetter3();
            }

            var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(_adapterRegistry, logger);

            return awardLetterEntityAdapter.MapToType(awardLetterEntity);
        }

        /// <summary>
        /// Gets AwardLetter3 DTO by the specified id
        /// </summary>
        /// <param name="studentId">student id for whom to retrieve award letter history record</param>        
        /// <param name="recordId">award letter history record id</param>
        /// <returns>AwardLetter3 DTO</returns>
        public async Task<AwardLetter3> GetAwardLetterById2Async(string studentId, string recordId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access award letters for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            if (string.IsNullOrEmpty(recordId))
            {
                throw new ArgumentNullException("recordId");
            }

            var allAwards = financialAidReferenceDataRepository.Awards;

            var studentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            if (studentAwardYears == null || !studentAwardYears.Any())
            {
                var message = string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardLetterEntity = await awardLetterHistoryRepository.GetAwardLetterById2Async(studentId, recordId,
                                    studentAwardYears, allAwards);


            var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(_adapterRegistry, logger);

            return awardLetterEntityAdapter.MapToType(awardLetterEntity);
        }

        /// <summary>
        /// Get a single student's award letter for the given year as a byte array that represents a PDF report.
        /// The data displayed on the report can be retrieved from the GetAwardLetter3Async(studentId, awardYear, recordId) method.
        /// Award letter report is being returned even if no awards are associated with the award letter
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student</param>
        /// <param name="awardYear">The award year for which to get an award letter</param>
        /// <param name="pathToReport">The path on the server to the .rdlc file that defines the award letter report</param>
        /// <param name="pathToLogo">The path on the server to an image file that represents an institution's logo for display on the report.</param>
        /// <returns>A byte array representation of an award letter report.</returns>
        public async Task<byte[]> GetAwardLetterReport4Async(AwardLetter3 awardLetterDto, string pathToReport, string pathToLogo)
        {
            var awardLetterConfigurationDto = await GetAwardLetterConfiguration2Async(awardLetterDto);
            return GetAwardLetterReport3(awardLetterDto, awardLetterConfigurationDto, pathToReport, pathToLogo);
        }

        /// <summary>
        /// Get the byte array representation as a PDF of the given AwardLetter DTO
        /// </summary>
        /// <param name="awardLetter">AwardLetter DTO to use as the data source for producing the award letter report.</param>
        /// <param name="pathToLogo">The path on the server to the report template</param>
        /// <param name="pathToReport">The path on the server to the institutions logo image to be used on the report</param>
        /// <returns>A byte array representation of a PDF award letter report.</returns>
        public byte[] GetAwardLetterReport3(AwardLetter3 awardLetterDto, AwardLetterConfiguration awardLetterConfigurationDto, string pathToReport, string pathToLogo)
        {

            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            if (awardLetterConfigurationDto == null)
            {
                throw new ArgumentNullException("awardLetterConfigurationDto");
            }
            if (string.IsNullOrEmpty(pathToReport))
            {
                throw new ArgumentNullException("pathToReport");
            }

            if (pathToLogo == null) pathToLogo = string.Empty;

            var report = new LocalReport();
            try
            {
                report.ReportPath = pathToReport;
                report.SetBasePermissionsForSandboxAppDomain(new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted));
                report.EnableExternalImages = true;

                //Extract all the award periods for the award letter
                var awardLetterAwardPeriods = awardLetterDto.AwardLetterAnnualAwards.Any() ? awardLetterDto.AwardLetterAnnualAwards
                    .SelectMany(ala => ala.AwardLetterAwardPeriods) : new List<AwardLetterAwardPeriod>();

                var distinctAwardPeriodsCount = awardLetterAwardPeriods.Select(p => p.ColumnNumber).Distinct().Count();

                //Convert Environment.NewLine into line breaks for opening and closing paragraphs
                awardLetterDto.OpeningParagraph = awardLetterDto.OpeningParagraph.Replace(Environment.NewLine, "<br />");
                awardLetterDto.ClosingParagraph = awardLetterDto.ClosingParagraph.Replace(Environment.NewLine, "<br />");

                var parameters = new List<ReportParameter>();
                parameters.Add(new ReportParameter("IsContactBlockActive", awardLetterConfigurationDto.IsContactBlockActive.ToString()));
                parameters.Add(new ReportParameter("IsNeedBlockActive", awardLetterConfigurationDto.IsNeedBlockActive.ToString()));
                parameters.Add(new ReportParameter("IsHousingBlockActive", awardLetterConfigurationDto.IsHousingBlockActive.ToString()));
                parameters.Add(new ReportParameter("AwardColumnHeader", awardLetterConfigurationDto.AwardTableTitle));
                parameters.Add(new ReportParameter("TotalColumnHeader", awardLetterConfigurationDto.AwardTotalTitle));
                parameters.Add(new ReportParameter("LogoPath", pathToLogo));
                parameters.Add(new ReportParameter("LabelDate", "Date:"));
                parameters.Add(new ReportParameter("LabelStudentId", "Student ID:"));
                parameters.Add(new ReportParameter("LabelAwardYear", "Award Year:"));
                parameters.Add(new ReportParameter("LabelBudget", "Budget:"));
                parameters.Add(new ReportParameter("LabelEfc", "EFC:"));
                parameters.Add(new ReportParameter("LabelNeed", "Need:"));
                parameters.Add(new ReportParameter("CreatedDate", awardLetterDto.CreatedDate.Value.ToShortDateString()));
                parameters.Add(new ReportParameter("LabelHousing", "Housing:"));
                parameters.Add(new ReportParameter("AreAwardPeriodsPresent", (awardLetterAwardPeriods.Any()).ToString()));
                parameters.Add(new ReportParameter("AreAnnualAwardsAbsent", (!awardLetterDto.AwardLetterAnnualAwards.Any()).ToString()));
                parameters.Add(new ReportParameter("NumAwardPeriodColumns", distinctAwardPeriodsCount.ToString()));
                report.SetParameters(parameters);

                report.DataSources.Add(new ReportDataSource("AwardLetter3", new List<Dtos.FinancialAid.AwardLetter3>() { awardLetterDto }));
                report.DataSources.Add(new ReportDataSource("AwardLetterAnnualAwards", awardLetterDto.AwardLetterAnnualAwards));
                report.DataSources.Add(new ReportDataSource("AwardLetterAwardPeriods", awardLetterAwardPeriods));
                report.DataSources.Add(new ReportDataSource("StudentAddress", awardLetterDto.StudentAddress));
                report.DataSources.Add(new ReportDataSource("OfficeAddress", awardLetterDto.ContactAddress));

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
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to generate award letter report.");
                throw;
            }
            finally
            {
                report.DataSources.Clear();
                report.ReleaseSandboxAppDomain();
                report.Dispose();
            }
        }

        #region Helpers

        /// <summary>
        /// Helper method to get StudentAward objects for the given student award years and apply the configuration filter to that list
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id for whom to get StudentAwards</param>
        /// <param name="studentAwardYears">The list of student award years for which to get StudentAwards data</param>
        /// <returns>A list of student awards for the given student award years that has been filtered by the Configuration.</returns>
        private async Task<IEnumerable<Domain.FinancialAid.Entities.StudentAward>> GetFilteredAwardLetterAwardsAsync(string studentId, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears)
        {
            var allAwards = financialAidReferenceDataRepository.Awards;
            var allAwardStatuses = financialAidReferenceDataRepository.AwardStatuses;

            var studentAwards = await studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
            if (studentAwards == null || studentAwards.Count() == 0)
            {
                var message = string.Format("Student {0} has no awards", studentId);
                logger.Info(message);
                return new List<Domain.FinancialAid.Entities.StudentAward>();
            }

            var filteredAwardLetterAwards = ApplyConfigurationService.FilterAwardsForAwardLetterAndShoppingSheetDisplay(studentAwards);
            if (filteredAwardLetterAwards == null || filteredAwardLetterAwards.Count() == 0)
            {
                var message = string.Format("Configuration filtered out all StudentAwards for student {0}", studentId);
                logger.Info(message);
                return new List<Domain.FinancialAid.Entities.StudentAward>();
            }

            return filteredAwardLetterAwards;
        }

        /// <summary>
        /// Gets a list of federally flagged student FAFSA records for 
        /// all years on student's record
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYears">student award years</param>
        /// <returns>Set of FAFSA entities</returns>
        private async Task<IEnumerable<Domain.FinancialAid.Entities.Fafsa>> GetStudentFafsaRecordsAsync(string studentId, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears)
        {
            var studentIds = new List<string>() { studentId };

            var fafsaRecords = new List<Domain.FinancialAid.Entities.Fafsa>();
            foreach (var year in studentAwardYears)
            {
                var fafsaRecordsForYear = await fafsaRepository.GetFafsaByStudentIdsAsync(studentIds, year.Code);
                var federallyFlaggedFafsaRecord = fafsaRecordsForYear.FirstOrDefault(fr => fr.IsFederallyFlagged);
                if (federallyFlaggedFafsaRecord != null)
                {
                    fafsaRecords.Add(federallyFlaggedFafsaRecord);
                }
            }
            return fafsaRecords;
        }

        /// <summary>
        /// Helper method to retrieve the award letter configuration for the specified award letter
        /// </summary>
        /// <param name="awardLetterDto">award letter dto to retrieve configuration for</param>
        /// <returns>AwardLetterConfiguration object</returns>
        [Obsolete("Obsolete as of API 1.22. Use GetAwardLetterConfiguration2Async")]
        private async Task<AwardLetterConfiguration> GetAwardLetterConfigurationAsync(AwardLetter2 awardLetterDto)
        {
            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            var awardLetterConfigurationEntities = await financialAidReferenceDataRepository.GetAwardLetterConfigurationsAsync();

            var awardLetterConfigurationEntity = awardLetterConfigurationEntities != null && awardLetterConfigurationEntities.Any() ?
                awardLetterConfigurationEntities.FirstOrDefault(alc => alc.Id == awardLetterDto.AwardLetterParameterId) : null;

            if (awardLetterConfigurationEntity == null)
            {
                throw new InvalidOperationException(string.Format("Could not get {0} award letter parameter record.", awardLetterDto.AwardLetterParameterId));
            }
            var awardLetterConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardLetterConfiguration, AwardLetterConfiguration>();
            return awardLetterConfigurationDtoAdapter.MapToType(awardLetterConfigurationEntity);
        }

        /// <summary>
        /// Helper method to retrieve the award letter configuration for the specified award letter
        /// </summary>
        /// <param name="awardLetterDto">award letter dto to retrieve configuration for</param>
        /// <returns>AwardLetterConfiguration object</returns>
        private async Task<AwardLetterConfiguration> GetAwardLetterConfiguration2Async(AwardLetter3 awardLetterDto)
        {
            if (awardLetterDto == null)
            {
                throw new ArgumentNullException("awardLetterDto");
            }
            var awardLetterConfigurationEntities = await financialAidReferenceDataRepository.GetAwardLetterConfigurationsAsync();

            var awardLetterConfigurationEntity = awardLetterConfigurationEntities != null && awardLetterConfigurationEntities.Any() ?
                awardLetterConfigurationEntities.FirstOrDefault(alc => alc.Id == awardLetterDto.AwardLetterParameterId) : null;

            if (awardLetterConfigurationEntity == null)
            {
                throw new InvalidOperationException(string.Format("Could not get {0} award letter parameter record.", awardLetterDto.AwardLetterParameterId));
            }
            var awardLetterConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardLetterConfiguration, AwardLetterConfiguration>();
            return awardLetterConfigurationDtoAdapter.MapToType(awardLetterConfigurationEntity);
        }

        /// <summary>
        /// Helper method to get the student or applicant Person based object
        /// </summary>
        /// <param name="personId">The Colleague PERSON id for whom to retrieve the data</param>
        /// <returns>A Person-based object Student or Applicant</returns>
        private async Task<Domain.Base.Entities.Person> GetStudentOrApplicantAsync(string personId)
        {
            Person studentOrApplicant = null;
            try
            {
                studentOrApplicant = await studentRepository.GetAsync(personId);
            }
            catch { }
            if (studentOrApplicant == null)
            {
                try
                {
                    studentOrApplicant = await applicantRepository.GetApplicantAsync(personId);
                }
                catch { }
            }

            return studentOrApplicant;
        }

        #endregion
    }
}
