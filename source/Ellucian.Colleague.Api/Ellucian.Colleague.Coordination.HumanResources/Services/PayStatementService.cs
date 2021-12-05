/* Copyright 2017-2021 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Reports;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Use the PayStatementService to get PayStatement DTOs and Reports
    /// </summary>
    [RegisterType]
    public class PayStatementService : BaseCoordinationService, IPayStatementService
    {
        private readonly ILocalReportService reportService;
        private readonly IPayStatementRepository payStatementRepository;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IPayStatementDomainService payStatementDomainService;
        private readonly IPayrollRegisterRepository payrollRegisterRepository;
        private readonly IHumanResourcesReferenceDataRepository hrReferenceDataRepository;
        private readonly IEarningsTypeRepository earningsTypeRepository;
        private readonly IPersonBenefitDeductionRepository personBenefitDeductionRepository;
        private readonly IPersonEmploymentStatusRepository personEmploymentStatusRepository;
        private readonly IPositionRepository positionRepository;
        private readonly IStudentReferenceDataRepository studentReferenceDataRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportRenderService"></param>
        /// <param name="statementRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PayStatementService(
            ILocalReportService reportService,
            IPayStatementRepository statementRepository,
            IPayStatementDomainService payStatementDomainService,
            IEmployeeRepository employeeRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IPayrollRegisterRepository payrollRegisterRepository,
            IEarningsTypeRepository earningsTypeRepository,
            IPersonBenefitDeductionRepository personBenefitDeductionRepository,
            IPersonEmploymentStatusRepository personEmploymentStatusRepository,
            IPositionRepository positionRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.reportService = reportService;
            this.payStatementRepository = statementRepository;
            this.payStatementDomainService = payStatementDomainService;
            this.employeeRepository = employeeRepository;
            this.hrReferenceDataRepository = hrReferenceDataRepository;
            this.payrollRegisterRepository = payrollRegisterRepository;
            this.earningsTypeRepository = earningsTypeRepository;
            this.personBenefitDeductionRepository = personBenefitDeductionRepository;
            this.personEmploymentStatusRepository = personEmploymentStatusRepository;
            this.positionRepository = positionRepository;
            this.studentReferenceDataRepository = studentReferenceDataRepository;
        }

        /// <summary>
        /// Gets a Pay Statement formatted as a PDF
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pathToReportTemplate"></param>
        /// <returns>Byte Array of PDF Data</returns>
        public async Task<Tuple<string, byte[]>> GetPayStatementPdf(string id, string pathToReportTemplate, string pathToLogo)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            string currentUserId = CurrentUser.PersonId;

            var payStatementSource = await payStatementRepository.GetPayStatementSourceDataAsync(id);
            if (payStatementSource == null)
            {
                throw new KeyNotFoundException(string.Format("PayStatement {0} does not exist", id));
            }

            //check if the statement belongs to the user or if they have a permission to view all pay statements
            string payStatementEmployeeId = payStatementSource.EmployeeId;
            if (payStatementEmployeeId != currentUserId && !HasPermission(Domain.HumanResources.HumanResourcesPermissionCodes.ViewAllEarningsStatements))
            {
                var message = string.Format("User {0} does not have permission to access pay statements for other employees", currentUserId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var fileName = string.Format("ADVICE_{0}_{1}.pdf", payStatementSource.EmployeeId, payStatementSource.PayDate.ToString("ddMMMyyyy"));

            var reportByteArray = await GetPayStatementPdf(new string[1] { id }, pathToReportTemplate, pathToLogo);

            return await Task.FromResult(new Tuple<string, byte[]>(fileName, reportByteArray));
        }


        public async Task<byte[]> GetPayStatementPdf(IEnumerable<string> payStatementIds, string pathToReportTemplate, string pathToLogo)
        {
            if (payStatementIds == null || !payStatementIds.Any())
            {
                throw new ArgumentNullException("payStatementIds");
            }
            if (string.IsNullOrEmpty(pathToReportTemplate))
            {
                throw new ArgumentNullException("pathToReportTemplate");
            }

            if (string.IsNullOrEmpty(pathToLogo))
            {
                pathToLogo = string.Empty;
            }

            var hostCountry = await studentReferenceDataRepository.GetHostCountryAsync();
            var configuration = await hrReferenceDataRepository.GetPayStatementConfigurationAsync();
            var reportParameters = new List<ReportParameter>();
            var shouldDisplaySocialSecurityNumber = configuration.SocialSecurityNumberDisplay != Domain.HumanResources.Entities.SSNDisplay.Hidden;
            reportParameters.Add(new ReportParameter("ShouldDisplaySocialSecurityNumber", shouldDisplaySocialSecurityNumber.ToString(), false));
            reportParameters.Add(new ReportParameter("ShouldDisplayWithholdingStatus", configuration.DisplayWithholdingStatusFlag.ToString(), false));
            reportParameters.Add(new ReportParameter("LogoPath", pathToLogo));
            reportParameters.Add(new ReportParameter("HostCountry", hostCountry, false));

            var referenceDataUtility = await getReferenceDataUtility();

            var stopWatch = new Stopwatch();
            var timingList = new List<string>();

            if (logger.IsErrorEnabled) { stopWatch.Start(); }

            var payStatementEntities = await payStatementRepository.GetPayStatementSourceDataAsync(payStatementIds);

            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPayStatementSourceData for {0} payStatementIds: {1}", payStatementIds.Count(), stopWatch.ElapsedMilliseconds));
            }
            var employeeIds = payStatementEntities.Select(p => p.EmployeeId).Distinct().ToList();

            if (!HasPermission(Domain.HumanResources.HumanResourcesPermissionCodes.ViewAllEarningsStatements) &&
                employeeIds.Any(id => id != CurrentUser.PersonId))
            {
                var message = string.Format(
                        "User {0} does not have permission to access pay statements for other employees",
                        CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (logger.IsErrorEnabled)
            {
                stopWatch.Reset();
                stopWatch.Start();
            }
            var employeePayStatements = await payStatementRepository.GetPayStatementSourceDataByPersonIdAsync(employeeIds);
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPayStatementSourceDataByPersonId for {0} personIds, total objects returned {1}: {2}", employeeIds.Count(), employeePayStatements.Count(), stopWatch.ElapsedMilliseconds));

                stopWatch.Reset();
                stopWatch.Start();
            }
            var payrollRegister = await payrollRegisterRepository.GetPayrollRegisterByEmployeeIdsAsync(employeeIds);
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPayrollRegisterByEmployeeIdsAsync for {0} personIds, total objects returned {1}: {2}", employeeIds.Count(), payrollRegister.Count(), stopWatch.ElapsedMilliseconds));

                stopWatch.Reset();
                stopWatch.Start();
            }
            var personBenefitDeductions = await personBenefitDeductionRepository.GetPersonBenefitDeductionsAsync(employeeIds);
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPersonBenefitDeductionsAsync for {0} personIds, total objects returned {1}: {2}", employeeIds.Count(), personBenefitDeductions.Count(), stopWatch.ElapsedMilliseconds));

                stopWatch.Reset();
                stopWatch.Start();
            }
            var personStatuses = await personEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(employeeIds);
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPersonEmploymentStatusesAsync for {0} personIds, total objects returned {1}: {2}", employeeIds.Count(), personStatuses.Count(), stopWatch.ElapsedMilliseconds));

                stopWatch.Reset();
                stopWatch.Start();
            }
            payStatementDomainService.SetContext(employeePayStatements, payrollRegister, personBenefitDeductions, personStatuses, referenceDataUtility);
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("SetContext timing {0}", stopWatch.ElapsedMilliseconds));
            }
            var reportEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayStatementReport, PayStatementReport>();

            var outputDocument = new PdfDocument();

            var internalStopWatch = new Stopwatch();
            var internalTimingList = new List<string>();

            if (logger.IsErrorEnabled)
            {
                stopWatch.Reset();
                stopWatch.Start();
            }


            foreach (var payStatementSource in payStatementEntities)
            {
                try
                {
                    reportService.SetPath(pathToReportTemplate);
                    reportService.SetBasePermissionsForSandboxAppDomain(new PermissionSet(PermissionState.Unrestricted));
                    reportService.EnableExternalImages(true);
                    reportService.SetParameters(reportParameters);


                    if (logger.IsErrorEnabled)
                    {
                        internalStopWatch.Reset();
                        internalStopWatch.Start();
                    }
                    var reportDomain = payStatementDomainService.BuildPayStatementReport(payStatementSource);
                    if (logger.IsErrorEnabled)
                    {
                        internalStopWatch.Stop();
                        internalTimingList.Add(string.Format("BuildPayStatementReport: {0}", internalStopWatch.ElapsedMilliseconds));
                    }

                    var reportDto = reportEntityToDtoAdapter.MapToType(reportDomain);

                    reportService.AddDataSource(new ReportDataSource("PayStatementReport", new List<PayStatementReport>() { reportDto }));
                    reportService.AddDataSource(new ReportDataSource("EmployeeMailingLabel", reportDto.EmployeeMailingLabel));
                    reportService.AddDataSource(new ReportDataSource("InstitutionMailingLabel", reportDto.InstitutionMailingLabel));
                    reportService.AddDataSource(new ReportDataSource("PayStatementEarnings", reportDto.Earnings));
                    reportService.AddDataSource(new ReportDataSource("PayStatementDeductions", reportDto.Deductions));
                    reportService.AddDataSource(new ReportDataSource("PayStatementBankDeposits", reportDto.Deposits));
                    reportService.AddDataSource(new ReportDataSource("PayStatementLeave", reportDto.Leave));
                    reportService.AddDataSource(new ReportDataSource("PayStatementTaxableBenefits", reportDto.TaxableBenefits));

                    if (logger.IsErrorEnabled)
                    {
                        internalStopWatch.Reset();
                        internalStopWatch.Start();
                    }
                    var byteArray = reportService.RenderReport();

                    if (logger.IsErrorEnabled)
                    {
                        internalStopWatch.Stop();
                        internalTimingList.Add(string.Format("RenderReport: {0}", internalStopWatch.ElapsedMilliseconds));
                    }

                    using (var pdfStream = PdfReader.Open(new MemoryStream(byteArray), PdfDocumentOpenMode.Import))
                    {
                        foreach (PdfPage page in pdfStream.Pages)
                        {
                            outputDocument.AddPage(page);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, string.Format("Unable to create pdf for pay statement {0}", payStatementSource.Id));
                }
                finally
                {
                    reportService.ResetReport();
                }
            }
            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("Total timing to build and render {0} reports: {1}", payStatementEntities.Count(), stopWatch.ElapsedMilliseconds));

                if (timingList.Any())
                {
                    var logString = timingList.Aggregate((total, next) => string.Format("{0}\n{1}", total, next));
                    logger.Error(logString);
                }

                if (internalTimingList.Any())
                {
                    var internalLogString = internalTimingList.Aggregate((total, next) => string.Format("{0}\n{1}", total, next));

                    logger.Error(internalLogString);
                }
            }

            if (outputDocument.PageCount == 0)
            {
                var message = "Unable to build any of the requested reports";
                logger.Error(message);
                throw new ApplicationException(message);
            }


            using (var outputStream = new MemoryStream())
            {
                outputDocument.Save(outputStream);

                var reportByteArray = outputStream.ToArray();
                outputStream.Close();
                return reportByteArray;
            }

        }

        /// <summary>
        /// Get Pay Statement Summaries for the given criteria, returning only those for which the Current User has permission to access
        /// </summary>
        /// <param name="employeeIdsFilter">Filter the summaries down to the ones owned by this list of employees. CurrentUser must have proper permission to get summaries other than ones owned by self</param>
        /// <param name="hasOnlineConsentFilter">Optional: If specified, filter the summaries down to ones owned by employees with the matchine hasOnlineConsent value. If null, filter is not applied</param>
        /// <param name="payDateFilter">Optional: If specified, filter the summaries down to ones with the specified payDate. If null, filter is not applied.</param>
        /// <param name="payCycleIdFilter">Optional: If specified, filter the summaries down to ones created in the specified pay cycle Id. If null, filter is not applied</param>
        /// <param name="startDateFilter">Optional: If specified, filter the summaries with a pay date greater than or equal to the specified start date filter. If null, filter is not applied.</param>
        /// <param name="endDateFilter">Optional: If specified, filter the summaries with a pay date less than or equal to the specified start date filter. If null, filter is not applied.</param>
        /// <returns></returns>
        public async Task<IEnumerable<PayStatementSummary>> GetPayStatementSummariesAsync(IEnumerable<string> employeeIdsFilter = null,
            bool? hasOnlineConsentFilter = null,
            DateTime? payDateFilter = null,
            string payCycleIdFilter = null,
            DateTime? startDateFilter = null,
            DateTime? endDateFilter = null)
        {
            //check for permissions 
            if (employeeIdsFilter == null || !employeeIdsFilter.Any() || employeeIdsFilter.Any(id => id != CurrentUser.PersonId))
            {
                //requesting access to someone else's data, verify that permissions are correct
                if (!HasPermission(Domain.HumanResources.HumanResourcesPermissionCodes.ViewAllEarningsStatements))
                {
                    var message = string.Format(
                        "User {0} does not have permission to access pay statements for other employees",
                        CurrentUser.PersonId
                    );
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
            }

            var stopWatch = new Stopwatch();
            var timingList = new List<string>();

            var personIds = new List<string>();
            var referenceDataUtility = await getReferenceDataUtility();

            //start by filtering person ids to employee ids based on the given criteria
            //limiting employee ids to the given person ids
            //limiting employee ids to those with the mathing hasOnlineConsent filter.
            //note that this could get all employees, which is fine.
            var employeeIds = await employeeRepository.GetEmployeeKeysAsync(employeeIdsFilter, hasOnlineConsentFilter);
            personIds = employeeIds != null ? employeeIds.Distinct().ToList() : new List<string>();

            if (logger.IsErrorEnabled) { stopWatch.Start(); }
            //get source data entities for the filtered personIds
            var summaryEntities = await payStatementRepository.GetPayStatementSourceDataByPersonIdAsync(personIds, startDateFilter, endDateFilter);
            if (summaryEntities == null || !summaryEntities.Any())
            {
                logger.Info("no summaryEntities selected from repository");
                return new List<PayStatementSummary>();
            }

            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPayStatementSourceDataByPersonIdAsync for {0} summaryEntities: {1}", summaryEntities.Count(), stopWatch.ElapsedMilliseconds));
                stopWatch.Reset();
                stopWatch.Start();
            }

            //apply the pay Date filter if specified
            if (payDateFilter.HasValue)
            {
                summaryEntities = summaryEntities.Where(s => s.PayDate == payDateFilter.Value).ToList();
                if (summaryEntities == null || !summaryEntities.Any())
                {
                    logger.Info("no summaryEntities for the selected pay date");
                    return new List<PayStatementSummary>();
                }
            }

            //filter the list of personids to the ones who have pay statement sources, potentially filtered by pay date
            personIds = summaryEntities.Select(s => s.EmployeeId).Distinct().ToList();
            if (!personIds.Any())
            {
                logger.Info("no statement summaries for the selected employee ids");
                return new List<PayStatementSummary>();
            }

            //adjusted ytd start date filter is Jan 1 of the startDateFilter year
            //this ensures we get all the payroll register entries to computed YTD values
            var adjustedYtdStartDateFilter = startDateFilter.HasValue ? new DateTime(startDateFilter.Value.Year, 1, 1) : (DateTime?)null;
            var payrollRegister = await payrollRegisterRepository.GetPayrollRegisterByEmployeeIdsAsync(personIds, startDateFilter, endDateFilter);
            if (payrollRegister == null || !payrollRegister.Any())
            {
                logger.Info("no payroll register entries selected from repository");
                return new List<PayStatementSummary>();
            }

            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                timingList.Add(string.Format("GetPayrollRegisterByEmployeeIdsAsync for {0} payrollRegister: {1}", payrollRegister.Count(), stopWatch.ElapsedMilliseconds));
                stopWatch.Reset();
                stopWatch.Start();
            }


            //filter the payrollRegister to the given paycycle (or all if no payCycle specified)
            var payrollRegisterLookup = payrollRegister
                .Where(pr => !string.IsNullOrEmpty(payCycleIdFilter) ? pr.PayCycleId == payCycleIdFilter : true)
                .ToLookup(pr => pr.ReferenceKey);

            var adapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayStatementSourceData, PayStatementSummary>();
            var payStatementSummaryDtos = new List<PayStatementSummary>();


            foreach (var summaryEntity in summaryEntities)
            {
                //verify the payrollRegister contains the same reference key as the pay statement
                if (payrollRegisterLookup.Contains(summaryEntity.ReferenceKey))
                {
                    //add the summary to the list
                    var summaryDto = adapter.MapToType(summaryEntity);
                    payStatementSummaryDtos.Add(summaryDto);

                }
                else
                {
                    var message = string.Format("PayStatement record {0} does not have an associated payroll register for reference key {1}", summaryEntity.Id, summaryEntity.ReferenceKey);
                    logger.Info(message);
                }
            }

            if (logger.IsErrorEnabled)
            {
                stopWatch.Stop();
                if (timingList.Any())
                {
                    var logString = timingList.Aggregate((total, next) => string.Format("{0}\n{1}", total, next));
                    logger.Error(logString);
                }
            }
            return payStatementSummaryDtos;
        }


        /// <summary>
        /// Helper to build a PayStatementReferenceDataUtility object
        /// </summary>
        /// <returns></returns>
        private async Task<Domain.HumanResources.Entities.PayStatementReferenceDataUtility> getReferenceDataUtility()
        {
            var allBenefitDeductionTypes = await hrReferenceDataRepository.GetBenefitDeductionTypesAsync();
            var allEarningsDifferentials = await hrReferenceDataRepository.GetEarningsDifferentialsAsync();
            var allEarningsTypes = await earningsTypeRepository.GetEarningsTypesAsync();
            var allTaxCodes = await hrReferenceDataRepository.GetTaxCodesAsync();
            var allLeaveTypes = await hrReferenceDataRepository.GetLeaveTypesAsync(false);
            var configuration = await hrReferenceDataRepository.GetPayStatementConfigurationAsync();
            var positions = await positionRepository.GetPositionsAsync();

            return new Domain.HumanResources.Entities.PayStatementReferenceDataUtility(allEarningsTypes, allEarningsDifferentials, allTaxCodes, allBenefitDeductionTypes, allLeaveTypes, positions, configuration);

        }
    }
}