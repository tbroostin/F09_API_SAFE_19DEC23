// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using slf4net;
using Ellucian.Colleague.Coordination.Finance.Reports;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Coordination.Base.Utility;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    /// <summary>
    /// Coordination Service for student statements
    /// </summary>
    [RegisterType]
    public class StudentStatementService : FinanceCoordinationService, IStudentStatementService
    {
        private readonly IAccountActivityRepository _accountActivityRepository;
        private readonly IAccountDueRepository _accountDueRepository;
        private readonly IAccountsReceivableRepository _accountsReceivableRepository;
        private readonly IFinanceConfigurationRepository _financeConfigurationRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ITermRepository _termRepository;
        private readonly IAcademicCreditRepository _academicCreditRepository;
        private Ellucian.Colleague.Domain.Finance.Entities.DueDateOverrides _dueDateOverrides;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="accountActivityRepository">AccountActivityRepository object</param>
        /// <param name="accountDueRepository">AccountDueRepository object</param>
        /// <param name="accountsReceivableRepository">AccountsReceivableRepository object</param>
        /// <param name="financeConfigurationRepository">FinanceConfigurationRepository object</param>
        /// <param name="sectionRepository">SectionRepository object</param>
        /// <param name="termRepository">TermRepository object</param>
        /// <param name="academicCreditRepository">AcademicCreditRepository object</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public StudentStatementService(IAdapterRegistry adapterRegistry,
            IAccountActivityRepository accountActivityRepository,
            IAccountDueRepository accountDueRepository,
            IAccountsReceivableRepository accountsReceivableRepository,
            IFinanceConfigurationRepository financeConfigurationRepository,
            ISectionRepository sectionRepository,
            ITermRepository termRepository,
            IAcademicCreditRepository academicCreditRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _accountActivityRepository = accountActivityRepository;
            _accountDueRepository = accountDueRepository;
            _accountsReceivableRepository = accountsReceivableRepository;
            _financeConfigurationRepository = financeConfigurationRepository;
            _sectionRepository = sectionRepository;
            _termRepository = termRepository;
            _academicCreditRepository = academicCreditRepository;

            _dueDateOverrides = _financeConfigurationRepository.GetDueDateOverrides();
        }

        /// <summary>
        /// Get an account holder's statement for a term or period.  
        /// </summary>
        /// <param name="accountHolderId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <returns>A student statement</returns>
        public async Task<StudentStatement> GetStudentStatementAsync(string accountHolderId, string timeframeId, DateTime? startDate, DateTime? endDate)
        {
            ValidateStudentStatementQueryCriteria(accountHolderId, timeframeId, startDate, endDate);
            CheckAccountPermission(accountHolderId);
            StudentStatement statementDto = null;

            try
            {
                statementDto = await GenerateStatementAsync(accountHolderId, timeframeId, startDate, endDate);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = string.Format("Student statement could not be generated for student {0} for term or period {1}.",
                    accountHolderId, timeframeId);
                logger.Error(ex, message);
            }

            return statementDto;
        }

        /// <summary>
        /// Get a student's accounts receivable statement as a byte array representation of a PDF file.  
        /// </summary>
        /// <param name="statementDto">StudentStatement DTO to use as the data source for producing the student statement report.</param>
        /// <param name="pathToReport">The path on the server to the report template</param>
        /// <param name="pathToResourceFile">The path on the server to the resource file</param>
        /// <param name="pathToLogo">The path on the server to the institutions logo image to be used on the report</param>
        /// <param name="utility">Report Parameter Utility</param>
        /// <returns>A byte array representation of a PDF student statement report.</returns>
        public byte[] GetStudentStatementReport(StudentStatement statementDto, string pathToReport, string pathToResourceFile, string pathToLogo)
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

                parameters.Add(utility.BuildReportParameter("PreviousBalance", statementDto.PreviousBalance));
                parameters.Add(utility.BuildReportParameter("PreviousBalanceDate", statementDto.PreviousBalanceDescription));
                parameters.Add(utility.BuildReportParameter("Balance", statementDto.CurrentBalance));
                parameters.Add(utility.BuildReportParameter("FutureBalance", statementDto.FutureBalance));
                parameters.Add(utility.BuildReportParameter("FutureBalanceDate", statementDto.FutureBalanceDescription));
                parameters.Add(utility.BuildReportParameter("OtherBalance", statementDto.OtherBalance));
                parameters.Add(utility.BuildReportParameter("InstitutionName", statementDto.InstitutionName));
                parameters.Add(utility.BuildReportParameter("RemittanceAddress", statementDto.RemittanceAddress));
                parameters.Add(utility.BuildReportParameter("StatementMessage", statementDto.StatementMessage));
                parameters.Add(utility.BuildReportParameter("StatementTitle", statementDto.Title));
                parameters.Add(utility.BuildReportParameter("StudentId", statementDto.StudentId));
                parameters.Add(utility.BuildReportParameter("StudentName", statementDto.StudentName));
                parameters.Add(utility.BuildReportParameter("StudentAddress", statementDto.StudentAddress));
                if (statementDto.DisplayDueDate)
                {
                    parameters.Add(utility.BuildReportParameter("DueDate", statementDto.DueDate));
                }
                else
                {
                    parameters.Add(utility.BuildReportParameter("DueDate", ""));
                }
                parameters.Add(utility.BuildReportParameter("Overdue", statementDto.Overdue));
                parameters.Add(utility.BuildReportParameter("CurrentAmountDue", statementDto.CurrentAmountDue));
                parameters.Add(utility.BuildReportParameter("OverdueAmount", statementDto.OverdueAmount));
                parameters.Add(utility.BuildReportParameter("TotalAmountDue", statementDto.TotalAmountDue));
                parameters.Add(utility.BuildReportParameter("Label_SummaryTermId", statementDto.AccountSummary.TimeframeDescription));
                parameters.Add(utility.BuildReportParameter("Label_SummaryTermDate", statementDto.AccountSummary.SummaryDateRange));
                parameters.Add(utility.BuildReportParameter("NoCharges", statementDto.AccountSummary.ChargeInformation.Count() == 0));
                parameters.Add(utility.BuildReportParameter("SummaryDepositsDue", statementDto.AccountSummary.CurrentDepositsDueAmount));
                parameters.Add(utility.BuildReportParameter("SummaryPayPlanAdjustments", statementDto.AccountSummary.PaymentPlanAdjustmentsAmount));
                parameters.Add(utility.BuildReportParameter("ImagePath", pathToLogo));
                parameters.Add(utility.BuildReportParameter("IncludeSchedule", statementDto.IncludeSchedule));
                parameters.Add(utility.BuildReportParameter("IncludeDetail", statementDto.IncludeDetail));
                parameters.Add(utility.BuildReportParameter("IncludeHistory", statementDto.IncludeHistory));
                parameters.Add(utility.BuildReportParameter("TotalDisbursedFa", statementDto.AccountDetails.FinancialAid.AnticipatedAid.SelectMany(aa => aa.AwardTerms).Sum(at => at.DisbursedAmount)));
                parameters.Add(utility.BuildReportParameter("TotalAnticipatedFa", statementDto.AccountDetails.FinancialAid.AnticipatedAid.SelectMany(aa => aa.AwardTerms).Sum(at => at.AnticipatedAmount)));
                parameters.Add(utility.BuildReportParameter("PaymentPlansPresent", !(statementDto.AccountDetails.PaymentPlans.PaymentPlans.Count == 1 && string.IsNullOrEmpty(statementDto.AccountDetails.PaymentPlans.PaymentPlans[0].Id))));
                parameters.Add(utility.BuildReportParameter("DepositsDuePresent", !(statementDto.DepositsDue.Count() == 1 && string.IsNullOrEmpty(statementDto.DepositsDue.First().Id))));
                parameters.Add(utility.BuildReportParameter("DisclosureStatement", statementDto.DisclosureStatement));
                parameters.Add(utility.BuildReportParameter("DateGenerated", statementDto.Date.ToShortDateString()));
                parameters.Add(utility.BuildReportParameter("DateFormat", System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern));
                parameters.Add(utility.BuildReportParameter("DisplayByTerm", statementDto.ActivityDisplay == Dtos.Finance.Configuration.ActivityDisplay.DisplayByTerm));
                parameters.Add(utility.BuildReportParameter("DisplayDueDates", statementDto.DisplayDueDate));

                // Set the report parameters
                report.SetParameters(parameters);

                // Convert report data to be sent to the report
                DataSet ds_Charges = ConvertToDataSet(statementDto.AccountSummary.ChargeInformation.ToArray());
                DataSet ds_Summary = ConvertToDataSet(statementDto.AccountSummary.NonChargeInformation.ToArray());
                DataSet ds_Schedule = ConvertToDataSet(statementDto.CourseSchedule.ToArray());
                DataSet ds_TuitionByTotal = ConvertToDataSet(statementDto.AccountDetails.Charges.TuitionByTotalGroups.SelectMany(tbt => tbt.TotalCharges).ToArray());
                DataSet ds_TuitionBySection = ConvertToDataSet(statementDto.AccountDetails.Charges.TuitionBySectionGroups.SelectMany(tbs => tbs.SectionCharges).ToArray());
                DataSet ds_Fees = ConvertToDataSet(statementDto.AccountDetails.Charges.FeeGroups.SelectMany(f => f.FeeCharges).ToArray());
                DataSet ds_RoomAndBoard = ConvertToDataSet(statementDto.AccountDetails.Charges.RoomAndBoardGroups.SelectMany(rb => rb.RoomAndBoardCharges).ToArray());
                DataSet ds_Misc = ConvertToDataSet(statementDto.AccountDetails.Charges.OtherGroups.SelectMany(o => o.OtherCharges).ToArray());
                DataSet ds_StudentPayments = ConvertToDataSet(statementDto.AccountDetails.StudentPayments.StudentPayments.ToArray());
                DataSet ds_SponsorPayments = ConvertToDataSet(statementDto.AccountDetails.Sponsorships.SponsorItems.ToArray());
                DataSet ds_FinancialAid = ConvertToDataSet(statementDto.AccountDetails.FinancialAid.AnticipatedAid.ToArray());
                DataSet ds_Deposits = ConvertToDataSet(statementDto.AccountDetails.Deposits.Deposits.ToArray());
                DataSet ds_Refunds = ConvertToDataSet(statementDto.AccountDetails.Refunds.Refunds.ToArray());
                DataSet ds_PlanInfo = ConvertToDataSet(statementDto.AccountDetails.PaymentPlans.PaymentPlans.ToArray());
                DataSet ds_PlanSchedules = ConvertToDataSet(statementDto.AccountDetails.PaymentPlans.PaymentPlans.SelectMany(pp => pp.PaymentPlanSchedules).ToArray());
                DataSet ds_DepositsDue = ConvertToDataSet(statementDto.DepositsDue.ToList().ToArray());

                // Add data to the report
                report.DataSources.Add(new ReportDataSource("ChargeGroups", ds_Charges.Tables[0]));
                report.DataSources.Add(new ReportDataSource("SummaryCharges", ds_Summary.Tables[0]));
                report.DataSources.Add(new ReportDataSource("CourseSchedule", ds_Schedule.Tables[0]));
                report.DataSources.Add(new ReportDataSource("TuitionByTotal", ds_TuitionByTotal.Tables[0]));
                report.DataSources.Add(new ReportDataSource("TuitionBySection", ds_TuitionBySection.Tables[0]));
                report.DataSources.Add(new ReportDataSource("Fees", ds_Fees.Tables[0]));
                report.DataSources.Add(new ReportDataSource("RoomAndBoard", ds_RoomAndBoard.Tables[0]));
                report.DataSources.Add(new ReportDataSource("Other", ds_Misc.Tables[0]));
                report.DataSources.Add(new ReportDataSource("StudentPayments", ds_StudentPayments.Tables[0]));
                report.DataSources.Add(new ReportDataSource("FinancialAid", ds_FinancialAid.Tables[0]));
                report.DataSources.Add(new ReportDataSource("Sponsorships", ds_SponsorPayments.Tables[0]));
                report.DataSources.Add(new ReportDataSource("Deposits", ds_Deposits.Tables[0]));
                report.DataSources.Add(new ReportDataSource("Refunds", ds_Refunds.Tables[0]));
                report.DataSources.Add(new ReportDataSource("PaymentPlanInfo", ds_PlanInfo.Tables[0]));
                report.DataSources.Add(new ReportDataSource("PaymentPlanSchedules", ds_PlanSchedules.Tables[0]));
                report.DataSources.Add(new ReportDataSource("DepositsDue", ds_DepositsDue.Tables[0]));

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
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
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
        /// Generate a student statement for a particular student and timeframe.
        /// </summary>
        /// <param name="accountHolderId">ID of the account holder for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="startDate">Date on which the supplied timeframe starts</param>
        /// <param name="endDate">Date on which the supplied timeframe ends</param>
        /// <returns>A StudentStatement</returns>
        private async Task<StudentStatement> GenerateStatementAsync(string accountHolderId, string timeframeId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var accountHolder = await _accountsReceivableRepository.GetAccountHolderAsync(accountHolderId);
                var depositTypes = _accountsReceivableRepository.DepositTypes;
                var terms = _termRepository.Get();
                var financeConfiguration = _financeConfigurationRepository.GetFinanceConfiguration();
                var termPeriods = _accountActivityRepository.GetAccountPeriods(accountHolderId).ToList();
                var nonTermPeriod = _accountActivityRepository.GetNonTermAccountPeriod(accountHolderId);
                var financialPeriods = _financeConfigurationRepository.GetFinancialPeriods().ToList();
                var statementProcessor = new StudentStatementProcessor(timeframeId, financeConfiguration.ActivityDisplay,
                    financeConfiguration.PaymentDisplay, depositTypes, terms, termPeriods, financialPeriods);
                var timeframeTermIds = statementProcessor.GetTermIdsForTimeframe().ToList();

                var detailedAccountPeriod = GetAccountDetails(accountHolderId, financeConfiguration.ActivityDisplay, timeframeTermIds, startDate, endDate);
                statementProcessor.SortAndConsolidateAccountDetails(detailedAccountPeriod);
                if (detailedAccountPeriod.Refunds != null)
                {
                    statementProcessor.UpdateRefundDatesAndReferenceNumbers(detailedAccountPeriod.Refunds.Refunds);
                }
                var depositsDue = statementProcessor.FilterAndUpdateDepositsDue(accountHolder.DepositsDue, timeframeTermIds, startDate, endDate);
                var accountTerms = GetAccountTerms(accountHolderId, financeConfiguration.PaymentDisplay);
                var dueDate = statementProcessor.CalculateDueDate(accountTerms, accountHolder.DepositsDue);
                var currentBalance = statementProcessor.CalculateCurrentBalance(detailedAccountPeriod);
                var previousBalance = statementProcessor.CalculatePreviousBalance();
                var futureBalance = statementProcessor.CalculateFutureBalance();
                var otherBalance = statementProcessor.CalculateOtherBalance(nonTermPeriod);
                var payPlanAdjustments = statementProcessor.CalculatePaymentPlanAdjustments(accountTerms);
                var currentDepositsDue = statementProcessor.CalculateCurrentDepositsDue(accountHolder.DepositsDue);
                var overdueAmount = statementProcessor.CalculateOverdueAmount(accountTerms, accountHolder.DepositsDue);
                var futureOverdueAmounts = statementProcessor.CalculateFutureOverdueAmounts(accountTerms);
                var totalAmountDue = statementProcessor.CalculateTotalAmountDue(previousBalance, currentBalance, payPlanAdjustments,
                    currentDepositsDue, futureOverdueAmounts);
                var statementSummary = statementProcessor.BuildStatementSummary(detailedAccountPeriod, accountTerms,
                    currentDepositsDue, startDate, endDate);
                var statementSchedule = await BuildStatementScheduleAsync(accountHolderId, timeframeId, financeConfiguration, terms, financialPeriods);
                var totalBalance = previousBalance + currentBalance + futureBalance + otherBalance;
                var previousBalanceDescription = statementProcessor.BuildPreviousBalanceDescription(startDate);
                var futureBalanceDescription = statementProcessor.BuildFutureBalanceDescription(endDate);
                var statement = new Ellucian.Colleague.Domain.Finance.Entities.StudentStatement(accountHolder, timeframeId, DateTime.Today,
                    dueDate, financeConfiguration, currentBalance, totalAmountDue, totalBalance, statementSummary, detailedAccountPeriod,
                    statementSchedule, overdueAmount)
                {
                    PreviousBalance = previousBalance,
                    PreviousBalanceDescription = previousBalanceDescription,
                    FutureBalance = futureBalance,
                    FutureBalanceDescription = futureBalanceDescription,
                    OtherBalance = otherBalance,
                    DisclosureStatement = null,
                    DisplayDueDate = financeConfiguration.DisplayDueDates
                };

                StudentStatement statementDto = new StudentStatementEntityAdapter(_adapterRegistry, logger).MapToType(statement);
                statementDto.DepositsDue = ConvertDepositsDueToStatementDepositsDue(depositsDue);
                CleanStatementForDisplay(statementDto);

                return statementDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieve accounts receivable details for a group of terms
        /// </summary>
        /// <param name="studentId">ID of the student for whom accounts receivable details will be retrieved</param>
        /// <param name="activityDisplay">Account activity display method</param>
        /// <param name="termIds">IDs of the terms for which to retrieve accounts receivable details for the student</param>
        /// <param name="startDate">Date on/after which accounts receivable details for the student will be retrieved</param>
        /// <param name="endDate">Date on/before which accounts receivable details for the student will be retrieved</param>
        /// <returns></returns>
        private DetailedAccountPeriod GetAccountDetails(string studentId, ActivityDisplay activityDisplay, IEnumerable<string> termIds,
            DateTime? startDate, DateTime? endDate)
        {
            switch (activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    var termId = termIds.First();
                    if (termId != null)
                    {
                        return _accountActivityRepository.GetTermActivityForStudent2(termId, studentId);
                    }
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    return _accountActivityRepository.GetPeriodActivityForStudent2(termIds, startDate, endDate, studentId);
            }
            return null;
        }

        /// <summary>
        /// Retrieve a collection of account terms for a student based on the payment display method
        /// </summary>
        /// <param name="studentId">ID of the student for whom account terms will be retrieved</param>
        /// <param name="paymentDisplay">Payment Display method</param>
        /// <returns>A collection of account terms for a student</returns>
        private List<AccountTerm> GetAccountTerms(string studentId, PaymentDisplay paymentDisplay)
        {
            // Determine whether the institution is using Term or PCF Mode
            List<AccountTerm> accountTerms = new List<AccountTerm>();

            if (paymentDisplay == PaymentDisplay.DisplayByPeriod)
            {
                // PCF Mode
                var accountDuePeriod = _accountDueRepository.GetPeriods(studentId);

                // Apply any due date overrides, then add period account terms to the master list of account terms
                DueDateOverrideProcessor.OverridePeriodDueDates(_dueDateOverrides, accountDuePeriod);
                if (accountDuePeriod.Current != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Current.AccountTerms);
                }
                if (accountDuePeriod.Future != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Future.AccountTerms);
                }
                if (accountDuePeriod.Past != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Past.AccountTerms);
                }
            }
            else
            {
                // Term Mode
                var accountDue = _accountDueRepository.Get(studentId);

                // Apply any due date overrides, then add account terms to the master list of account terms
                var colleagueTimeZone = _financeConfigurationRepository.GetFinanceConfiguration().ColleagueTimezone;
                DueDateOverrideProcessor.OverrideTermDueDates(_dueDateOverrides, accountDue, colleagueTimeZone);
                accountTerms.AddRange(accountDue.AccountTerms);
            }

            return accountTerms;
        }

        /// <summary>
        /// Build a student's schedule for display on a student statement
        /// </summary>
        /// <param name="studentId">ID of the student for whom the statement will be generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement will be generated</param>
        /// <param name="terms">Collection of terms</param>
        /// <param name="periods">Collection of financial periods</param>
        /// <returns>A student's schedule for display on a student statement</returns>
        private async Task<IEnumerable<Domain.Finance.Entities.StudentStatementScheduleItem>> BuildStatementScheduleAsync(string studentId,
            string timeframeId, FinanceConfiguration financeConfiguration, IEnumerable<Term> terms,
            IEnumerable<Domain.Finance.Entities.FinancialPeriod> periods)
        {
            List<Domain.Finance.Entities.StudentStatementScheduleItem> schedule = new List<Domain.Finance.Entities.StudentStatementScheduleItem>();
            if (!financeConfiguration.IncludeSchedule ||
                (financeConfiguration.ActivityDisplay == ActivityDisplay.DisplayByPeriod
                && timeframeId == FinanceTimeframeCodes.PastPeriod))
            {
                return schedule;
            }

            var studentAcademicCredits = new List<AcademicCredit>();
            var academicCreditsDict = await _academicCreditRepository.GetAcademicCreditByStudentIdsAsync(new List<string>() { studentId });
            if (academicCreditsDict.Any())
            {
                studentAcademicCredits = academicCreditsDict[studentId];
                var termIds = new List<string>();
                switch (financeConfiguration.ActivityDisplay)
                {
                    case ActivityDisplay.DisplayByPeriod:
                        PeriodType periodType = PeriodType.Current;
                        switch (timeframeId)
                        {
                            case FinanceTimeframeCodes.CurrentPeriod:
                                periodType = PeriodType.Current;
                                break;
                            case FinanceTimeframeCodes.FuturePeriod:
                                periodType = PeriodType.Future;
                                break;
                        }
                        // Identify all of the term IDs for the selected period
                        termIds = new List<string>(terms.Where(t => t.FinancialPeriod == periodType).Select(t => t.Code));

                        // Identify the selected period
                        var period = financeConfiguration.Periods.Where(fp => fp.Type == periodType).FirstOrDefault();

                        // Identify only the academic credits with terms in the selected period and non-term academic credits
                        // within the selected period
                        if (period != null)
                        {
                            studentAcademicCredits = academicCreditsDict[studentId].Where(ac => termIds.Contains(ac.TermCode) ||
                                (string.IsNullOrEmpty(ac.TermCode)
                                && FinancialPeriodProcessor.GetDateRangePeriod(ac.StartDate, ac.EndDate, financeConfiguration.Periods) == periodType)).ToList();
                        }
                        break;
                    case ActivityDisplay.DisplayByTerm:
                        switch (timeframeId)
                        {
                            case FinanceTimeframeCodes.NonTerm:
                                studentAcademicCredits = academicCreditsDict[studentId].Where(ac => string.IsNullOrEmpty(ac.TermCode)).ToList();
                                break;
                            default:
                                if (TermPeriodProcessor.IsReportingTerm(timeframeId, terms))
                                {
                                    termIds = TermPeriodProcessor.GetTermIdsForReportingTerm(timeframeId, terms).ToList();
                                }
                                else
                                {
                                    termIds = new List<string>() { timeframeId };
                                }

                                studentAcademicCredits = academicCreditsDict[studentId].Where(ac => termIds.Contains(ac.TermCode)).ToList();
                                break;
                        }
                        break;
                }
            }

            // Filter out any non-Add and non-New academic credits, as well as credits with no section id
            studentAcademicCredits.RemoveAll(ac => ac == null || (ac.Status != CreditStatus.Add && ac.Status != CreditStatus.New) || string.IsNullOrEmpty(ac.SectionId));

            var sections = new List<Section>();
            IEnumerable<Section> sectionsToAdd = await GetSectionsAsync(studentAcademicCredits);
            sections.AddRange(sectionsToAdd);
            if (studentAcademicCredits.Any() && sections.Any())
            {
                foreach (var studentAcademicCredit in studentAcademicCredits)
                {
                    //find a corresponding course section if any
                    var section = sections.FirstOrDefault(s => s != null && s.Id == studentAcademicCredit.SectionId);
                    if (section != null)
                    {
                        Term term = null;
                        if (!string.IsNullOrEmpty(section.TermId))
                        {
                            term = _termRepository.Get(section.TermId);
                        }
                        schedule.Add(new Domain.Finance.Entities.StudentStatementScheduleItem(studentAcademicCredit, section)
                        {
                            SectionTerm = term != null ? term.Description : string.Empty
                        });
                    }
                    else
                    {
                        logger.Warn(string.Format("Student academic credit {0} does not have an associated course section", studentAcademicCredit.Id));
                    }
                }
                sections.RemoveAll(sec => sec == null || (sec.BillingPeriodType == "T"));
            }
            schedule = schedule.OrderBy(sch => TermPeriodProcessor.GetTermSortOrder(TermPeriodProcessor.GetTermIdForTermDescription(sch.SectionTerm, terms), terms)).ThenBy(sc => sc.SectionId).ToList();
            return schedule;
        }

        /// <summary>
        /// Convert deposits due for display on a student statement
        /// </summary>
        /// <param name="depositsDue">Collection of deposits due to be converted</param>
        /// <returns>Converted deposits due for display on a student statement</returns>
        private IEnumerable<StudentStatementDepositDue> ConvertDepositsDueToStatementDepositsDue(IEnumerable<Domain.Finance.Entities.DepositDue> depositsDue)
        {
            var statementDepositsDue = new List<StudentStatementDepositDue>();
            if (depositsDue == null || depositsDue.Count() == 0)
            {
                return statementDepositsDue;
            }
            foreach (var Source in depositsDue)
            {
                statementDepositsDue.Add(new Ellucian.Colleague.Dtos.Finance.StudentStatementDepositDue()
                {
                    Id = Source.Id,
                    DepositTypeDescription = Source.DepositTypeDescription,
                    DueDate = Source.DueDate,
                    AmountDue = Source.Amount,
                    AmountPaid = Source.AmountPaid,
                    Balance = Source.Balance,
                    TermDescription = Source.TermDescription
                });
            }
            return statementDepositsDue;
        }

        /// <summary>
        /// Cleans student statement collections for display on a statement
        /// All collections on the student statement must include at least one item for display purposes
        /// - in the event that there are no items in a given collection, a blank item must be added for report rendering
        /// </summary>
        /// <param name="statement">Student Statement</param>
        /// <returns>Cleaned StudentStatement DTO</returns>
        private void CleanStatementForDisplay(StudentStatement statement)
        {
            if (statement != null && statement.AccountDetails != null && statement.AccountDetails.Charges != null)
            {
                if (statement.AccountDetails.Charges.TuitionByTotalGroups == null || statement.AccountDetails.Charges.TuitionByTotalGroups.SelectMany(tbt => tbt.TotalCharges).Count() == 0)
                {
                    statement.AccountDetails.Charges.TuitionByTotalGroups = new List<Dtos.Finance.AccountActivity.TuitionByTotalType>()
                    {
                        new Dtos.Finance.AccountActivity.TuitionByTotalType()
                        {
                            TotalCharges = new List<Dtos.Finance.AccountActivity.ActivityTuitionItem>()
                            {
                                new Dtos.Finance.AccountActivity.ActivityTuitionItem()
                            }
                        }
                    };
                }
                if (statement.AccountDetails.Charges.TuitionBySectionGroups == null || statement.AccountDetails.Charges.TuitionBySectionGroups.SelectMany(tbs => tbs.SectionCharges).Count() == 0)
                {
                    statement.AccountDetails.Charges.TuitionBySectionGroups = new List<Dtos.Finance.AccountActivity.TuitionBySectionType>()
                    {
                        new Dtos.Finance.AccountActivity.TuitionBySectionType()
                        {
                            SectionCharges = new List<Dtos.Finance.AccountActivity.ActivityTuitionItem>()
                            {
                                new Dtos.Finance.AccountActivity.ActivityTuitionItem()
                            }
                        }
                    };
                }
                if (statement.AccountDetails.Charges.FeeGroups == null || statement.AccountDetails.Charges.FeeGroups.SelectMany(fg => fg.FeeCharges).Count() == 0)
                {
                    statement.AccountDetails.Charges.FeeGroups = new List<Dtos.Finance.AccountActivity.FeeType>()
                    {
                        new Dtos.Finance.AccountActivity.FeeType()
                        {
                            FeeCharges = new List<Dtos.Finance.AccountActivity.ActivityDateTermItem>()
                            {
                                new Dtos.Finance.AccountActivity.ActivityDateTermItem()
                            }
                        }
                    };
                }
                if (statement.AccountDetails.Charges.RoomAndBoardGroups == null || statement.AccountDetails.Charges.RoomAndBoardGroups.SelectMany(rbg => rbg.RoomAndBoardCharges).Count() == 0)
                {
                    statement.AccountDetails.Charges.RoomAndBoardGroups = new List<Dtos.Finance.AccountActivity.RoomAndBoardType>()
                    {
                        new Dtos.Finance.AccountActivity.RoomAndBoardType()
                        {
                            RoomAndBoardCharges = new List<Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem>()
                            {
                                new Dtos.Finance.AccountActivity.ActivityRoomAndBoardItem()
                            }
                        }
                    };
                }
                if ((statement.AccountDetails.Charges.OtherGroups == null ||
                    statement.AccountDetails.Charges.OtherGroups.SelectMany(og => og.OtherCharges).Count() == 0) &&
                    (statement.AccountDetails.Charges.Miscellaneous == null ||
                    statement.AccountDetails.Charges.Miscellaneous.OtherCharges == null ||
                    statement.AccountDetails.Charges.Miscellaneous.OtherCharges.Count == 0))
                {
                    statement.AccountDetails.Charges.OtherGroups = new List<Dtos.Finance.AccountActivity.OtherType>()
                        {
                            new Dtos.Finance.AccountActivity.OtherType()
                            {
                                OtherCharges = new List<Dtos.Finance.AccountActivity.ActivityDateTermItem>()
                                {
                                    new Dtos.Finance.AccountActivity.ActivityDateTermItem()
                                }
                            }
                        };
                }
            }
            if (statement.AccountDetails.StudentPayments == null ||
                statement.AccountDetails.StudentPayments.StudentPayments == null
                || statement.AccountDetails.StudentPayments.StudentPayments.Count() == 0)
            {
                statement.AccountDetails.StudentPayments = new Dtos.Finance.AccountActivity.StudentPaymentCategory()
                {
                    StudentPayments = new List<Dtos.Finance.AccountActivity.ActivityPaymentPaidItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivityPaymentPaidItem()
                        }
                };
            }
            if (statement.AccountDetails.Sponsorships == null ||
                statement.AccountDetails.Sponsorships.SponsorItems == null
                || statement.AccountDetails.Sponsorships.SponsorItems.Count() == 0)
            {
                statement.AccountDetails.Sponsorships = new Dtos.Finance.AccountActivity.SponsorshipCategory()
                {
                    SponsorItems = new List<Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivitySponsorPaymentItem()
                        }
                };
            }
            if (statement.AccountDetails.FinancialAid == null ||
                statement.AccountDetails.FinancialAid.AnticipatedAid == null
                || statement.AccountDetails.FinancialAid.AnticipatedAid.Count() == 0)
            {
                statement.AccountDetails.FinancialAid = new Dtos.Finance.AccountActivity.FinancialAidCategory()
                {
                    AnticipatedAid = new List<Dtos.Finance.AccountActivity.ActivityFinancialAidItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivityFinancialAidItem()
                        }
                };
            }

            foreach (var aa in statement.AccountDetails.FinancialAid.AnticipatedAid)
            {
                aa.Comments = string.IsNullOrEmpty(aa.Comments) ? " " : aa.Comments;
            }

            if (statement.AccountDetails.Deposits == null ||
                statement.AccountDetails.Deposits.Deposits == null
                || statement.AccountDetails.Deposits.Deposits.Count() == 0)
            {
                statement.AccountDetails.Deposits = new Dtos.Finance.AccountActivity.DepositCategory()
                {
                    Deposits = new List<Dtos.Finance.AccountActivity.ActivityRemainingAmountItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivityRemainingAmountItem()
                        }
                };
            }
            if (statement.AccountDetails.Refunds == null ||
                statement.AccountDetails.Refunds.Refunds == null
                || statement.AccountDetails.Refunds.Refunds.Count() == 0)
            {
                statement.AccountDetails.Refunds = new Dtos.Finance.AccountActivity.RefundCategory()
                {
                    Refunds = new List<Dtos.Finance.AccountActivity.ActivityPaymentMethodItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivityPaymentMethodItem()
                        }
                };
            }
            if (statement.AccountDetails.PaymentPlans == null ||
                statement.AccountDetails.PaymentPlans.PaymentPlans == null
                || statement.AccountDetails.PaymentPlans.PaymentPlans.Count() == 0)
            {
                statement.AccountDetails.PaymentPlans = new Dtos.Finance.AccountActivity.PaymentPlanCategory()
                {
                    PaymentPlans = new List<Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem>()
                        {
                            new Dtos.Finance.AccountActivity.ActivityPaymentPlanDetailsItem()
                            {
                                PaymentPlanSchedules = new List<Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem>()
                                {
                                    new Dtos.Finance.AccountActivity.ActivityPaymentPlanScheduleItem()
                                }
                            }
                        }
                };
            }
            //As per jpm 01/24/2019, an empty object is added for the purposes of being able to render rdlc
            if (statement.CourseSchedule == null || statement.CourseSchedule.Count() == 0)
            {
                statement.CourseSchedule = new List<StudentStatementScheduleItem>() 
                { 
                    new StudentStatementScheduleItem()
                };
            }
            if (statement.DepositsDue == null || statement.DepositsDue.Count() == 0)
            {
                statement.DepositsDue = new List<StudentStatementDepositDue>()
                { 
                    new StudentStatementDepositDue()
                };
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

        private async Task<IEnumerable<Section>> GetSectionsAsync(IEnumerable<AcademicCredit> academicCredits)
        {
            var sections = new List<Section>();
            var sectionsToAdd =await _sectionRepository.GetCachedSectionsAsync(academicCredits.Select(x => x.SectionId));
            if (sectionsToAdd.Count() == 0)
            {
                sectionsToAdd = await _sectionRepository.GetNonCachedSectionsAsync(academicCredits.Select(x => x.SectionId));
            }
            sections.AddRange(sectionsToAdd);
            return sections;
        }

        /// <summary>
        /// Validates student statement query criteria
        /// </summary>
        /// <param name="criteria">Student Statement query criteria</param>
        private void ValidateStudentStatementQueryCriteria(string accountHolderId, string timeframeId, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(accountHolderId))
            {
                throw new ArgumentNullException("accountHolderId", "Account Holder ID cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(timeframeId))
            {
                throw new ArgumentNullException("timeframeId", "Term/Period ID cannot be null or empty.");
            }
            switch (timeframeId)
            {
                case FinanceTimeframeCodes.PastPeriod:
                    if (startDate != null || endDate == null)
                    {
                        throw new ArgumentException("Past Period cannot have a start date, but must have an end date.");
                    }
                    break;
                case FinanceTimeframeCodes.CurrentPeriod:
                    if (startDate == null || endDate == null)
                    {
                        throw new ArgumentException("Current Period must have start and end dates.");
                    }
                    break;
                case FinanceTimeframeCodes.FuturePeriod:
                    if (startDate == null || endDate != null)
                    {
                        throw new ArgumentException("Future Period must have a start date, but cannot have an end date.");
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
