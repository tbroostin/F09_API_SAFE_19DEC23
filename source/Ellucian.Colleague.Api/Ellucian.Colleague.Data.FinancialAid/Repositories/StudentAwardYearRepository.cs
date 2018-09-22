//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class gets StudentAwardYear objects to the colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAwardYearRepository : BaseColleagueRepository, IStudentAwardYearRepository
    {
        /// <summary>
        /// Constructor for StudentAwardYearsRepository
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public StudentAwardYearRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// This gets all of the student's FA years from Colleague
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to retrieve award data</param>
        /// <param name="currentOfficeService">A CurrentOfficeService object used to set the StudentAwardYear object's CurrentOffice</param>
        /// <param name="getActiveYearsOnly">Flag to indicate whether to retrieve only active student award years. Default is false</param>
        /// <exception cref="ArgumentNullException">Thrown when studentId or currentOfficeService argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown when studentId does not exist in the FinancialAid system - Student has no FIN.AID record</exception>
        /// <returns>A list of StudentAwardYear objects</returns>
        public async Task<IEnumerable<StudentAwardYear>> GetStudentAwardYearsAsync(string studentId, CurrentOfficeService currentOfficeService, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (currentOfficeService == null)
            {
                throw new ArgumentNullException("currentOfficeService");
            }
            
            var studentAwardYearsData = await DataReader.ReadRecordAsync<FinAid>(studentId);

            if (studentAwardYearsData == null)
            {
                throw new KeyNotFoundException(string.Format("Student Id {0} does not have a Financial Aid record", studentId));
            }

            //if any of the year lists are null, instantiate them to empty lists
            if (studentAwardYearsData.FaCsYears == null) { studentAwardYearsData.FaCsYears = new List<string>(); }
            if (studentAwardYearsData.FaSaYears == null) { studentAwardYearsData.FaSaYears = new List<string>(); }
            if (studentAwardYearsData.FaYsYears == null) { studentAwardYearsData.FaYsYears = new List<string>(); }

            //concat all the year lists together and then get only the distinct years
            var studentYearsList = studentAwardYearsData.FaCsYears.Concat(studentAwardYearsData.FaSaYears).Concat(studentAwardYearsData.FaYsYears);
            studentYearsList = studentYearsList.Where(y => !string.IsNullOrEmpty(y)).Distinct().OrderBy(y => y);

            if (getActiveYearsOnly)
            {
                var activeOfficeYears = (currentOfficeService.GetActiveOfficeConfigurations()).Select(c => c.AwardYear);
                studentYearsList = studentYearsList.Where(y => activeOfficeYears.Contains(y));
            }

            //if the student has any pending loan requests, get the NewLoanRequest records and set up a dictionary of the
            //loan request's awardYear (key) and the loan request id (value)
            var pendingLoanRequestIds = studentAwardYearsData.FaPendingLoanRequestIds;
            var awardYearLoanRequestDictionary = new Dictionary<string, string>();
            if (pendingLoanRequestIds != null && pendingLoanRequestIds.Count() > 0)
            {
                var pendingLoanRequestData = await DataReader.BulkReadRecordAsync<NewLoanRequest>(pendingLoanRequestIds.ToArray());

                if (pendingLoanRequestData != null)
                {
                    foreach (var pendingLoanRequest in pendingLoanRequestData)
                    {
                        awardYearLoanRequestDictionary.Add(pendingLoanRequest.NlrAwardYear, pendingLoanRequest.Recordkey);
                    }
                }
            }

            //Get all award letter history records for the student
            string criteria = "WITH ALH.STUDENT.ID EQ '" + studentId + "'";

            var awardLetterHistoryRecords = await DataReader.BulkReadRecordAsync<AwardLetterHistory>(criteria);
            
            if (awardLetterHistoryRecords == null || !awardLetterHistoryRecords.Any())
            {
                logger.Info(string.Format("Student Id {0} has no award letter history records", studentId));
            }

            var studentAwardYears = new List<StudentAwardYear>();

            //If no fa years on record
            if (!studentYearsList.Any())
            {
                logger.Info(string.Format("Student Id {0} has no Financial Aid information available to review", studentId));
                return studentAwardYears;
            }

            var isPaperCopyOptionSelected = (!string.IsNullOrEmpty(studentAwardYearsData.FaPaperCopyOptInFlag) && studentAwardYearsData.FaPaperCopyOptInFlag.ToUpper() == "Y");
            foreach (var year in studentYearsList)
            {
                try
                {
                    //if the year exists in FaCsYears list, get data from CS.ACYR.
                    //Do this first to get the locationId which will get us the office object
                    var locationId = string.Empty;
                    decimal? totalEstimatedExpenses = null;
                    decimal? estimatedExpensesAdjustment = null;
                    string fedIsirId = string.Empty;

                    if (studentAwardYearsData.FaCsYears.Contains(year))
                    {
                        var acyrFile = "CS." + year;

                        var csRecord = await DataReader.ReadRecordAsync<CsAcyr>(acyrFile, studentId);

                        if (csRecord != null)
                        {
                            locationId = csRecord.CsLocation;
                            totalEstimatedExpenses = csRecord.CsStdTotalExpenses;
                            estimatedExpensesAdjustment = csRecord.CsBudgetAdj;
                            fedIsirId = csRecord.CsFedIsirId;
                        }
                    }

                    //Get the current office
                    FinancialAidOffice office;
                    if (getActiveYearsOnly)
                    {
                        office = currentOfficeService.GetCurrentOfficeWithActiveConfiguration(locationId, year);
                    }
                    else {
                        office = currentOfficeService.GetCurrentOfficeByLocationId(locationId);
                    }

                    var studentAwardYear = new StudentAwardYear(studentId, year, office);

                    studentAwardYear.TotalEstimatedExpenses = totalEstimatedExpenses;
                    studentAwardYear.EstimatedExpensesAdjustment = estimatedExpensesAdjustment;

                    studentAwardYear.IsPaperCopyOptionSelected = isPaperCopyOptionSelected;

                    studentAwardYear.FederallyFlaggedIsirId = fedIsirId;

                    //get the pending loan request id from the dictionary for this year.
                    string pendingLoanRequestId;
                    awardYearLoanRequestDictionary.TryGetValue(year, out pendingLoanRequestId);
                    studentAwardYear.PendingLoanRequestId = pendingLoanRequestId;

                    //get the award letter history records for the year - sort them by date, time in descending order
                    var awardLetterHistoryRecordsForYear = awardLetterHistoryRecords != null ?
                        awardLetterHistoryRecords.Where(r => r.AlhAwardYear == year).OrderByDescending(r => r.AlhAwardLetterDate)
                        .ThenByDescending(r => r.AwardLetterHistoryAddtime).ToList() : null;

                    if (awardLetterHistoryRecordsForYear != null && awardLetterHistoryRecordsForYear.Any())
                    {
                        foreach (var record in awardLetterHistoryRecordsForYear)
                        {
                            studentAwardYear.AwardLetterHistoryItemsForYear.Add(
                                new AwardLetterHistoryItem(record.Recordkey, record.AlhAwardLetterDate));
                        }
                    }

                    // if year exists in FaSaYears list, we can set the TotalAwardedAmount attribute
                    decimal totalAwardedAmount = 0;
                    if (studentAwardYearsData.FaSaYears.Contains(year))
                    {
                        var acyrFile = "SA." + year;

                        var saRecord = await DataReader.ReadRecordAsync<SaAcyr>(acyrFile, studentId);

                        if (saRecord != null)
                        {
                            totalAwardedAmount = saRecord.SaAwarded.HasValue ? saRecord.SaAwarded.Value : 0;
                        }
                    }
                    studentAwardYear.TotalAwardedAmount = totalAwardedAmount;

                    // if year exists in FaYsYears list, we can set the IsApplicationReviewed attribute
                    if (studentAwardYearsData.FaYsYears.Contains(year))
                    {
                        var acyrFile = "YS." + year;

                        var ysRecord = await DataReader.ReadRecordAsync<YsAcyr>(acyrFile, studentId);

                        studentAwardYear.IsApplicationReviewed = (ysRecord != null) ? (ysRecord.YsApplCompleteDate.HasValue && ysRecord.YsApplCompleteDate.Value <= DateTime.Today) : false;
                    }

                    studentAwardYears.Add(studentAwardYear);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Unable to create StudentAwardYear for {0}*{1}", studentId, year);
                }
            }

            return studentAwardYears;
        }

        /// <summary>
        /// Get single student award year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="currentOfficeService">current office service</param>
        /// <returns>StudentAwardYear entity</returns>
        public async Task<StudentAwardYear> GetStudentAwardYearAsync(string studentId, string awardYearCode, CurrentOfficeService currentOfficeService)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            if (currentOfficeService == null)
            {
                throw new ArgumentNullException("currentOfficeService");
            }

            var finAidRecord = await DataReader.ReadRecordAsync<FinAid>(studentId);            
            if (finAidRecord == null)
            {
                string message = string.Format("Student Id {0} does not have a Financial Aid record", studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            //if any of the year lists are null, instantiate them to empty lists
            if (finAidRecord.FaCsYears == null) { finAidRecord.FaCsYears = new List<string>(); }
            if (finAidRecord.FaSaYears == null) { finAidRecord.FaSaYears = new List<string>(); }
            if (finAidRecord.FaYsYears == null) { finAidRecord.FaYsYears = new List<string>(); }
            string locationId = string.Empty;
            if (finAidRecord.FaCsYears.Contains(awardYearCode))
            {
                var acyrFile = "CS." + awardYearCode;                
                var csRecord = await DataReader.ReadRecordAsync<CsAcyr>(acyrFile, studentId);                
                if (csRecord != null)
                {
                    locationId = csRecord.CsLocation;
                }
                return new StudentAwardYear(studentId, awardYearCode, currentOfficeService.GetCurrentOfficeByLocationId(locationId));
            }
            else if (finAidRecord.FaSaYears.Contains(awardYearCode) ||
                finAidRecord.FaYsYears.Contains(awardYearCode))
            {
                return new StudentAwardYear(studentId, awardYearCode, currentOfficeService.GetDefaultOffice());
            }

            else return null;
        }

        /// <summary>
        /// Update the paper copy option flag in the FIN.AID record
        /// </summary>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>the student award year entity</returns>
        public StudentAwardYear UpdateStudentAwardYear(StudentAwardYear studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            UpdateCorrOptionFlagRequest request = new UpdateCorrOptionFlagRequest();
            request.StudentId = studentAwardYear.StudentId;
            request.PaperCopyOptionFlag = studentAwardYear.IsPaperCopyOptionSelected;

            var transactionResponse = transactionInvoker.Execute<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorMessage))
            {
                if (transactionResponse.ErrorMessage.ToLower() == string.Format("conflict: fin.aid record for student {0} is locked by a process", studentAwardYear.StudentId))
                {
                    var message = string.Format("Paper copy option flag update canceled because record id {0} in FIN.AID table is locked.", studentAwardYear.StudentId);
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
                else
                {
                    var message = string.Format("Unable to update Paper Copy Option for student {1}. Error message from CTX: ", studentAwardYear.StudentId, transactionResponse.ErrorMessage);
                    logger.Error(message);
                    throw new Exception(message);
                }
            }

            return studentAwardYear;
        }

        /// <summary>
        /// Update the paper copy option flag in the FIN.AID record
        /// </summary>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>the student award year entity</returns>
        public async Task<StudentAwardYear> UpdateStudentAwardYearAsync(StudentAwardYear studentAwardYear)
        {
            if (studentAwardYear == null)
            {
                throw new ArgumentNullException("studentAwardYear");
            }

            UpdateCorrOptionFlagRequest request = new UpdateCorrOptionFlagRequest();
            request.StudentId = studentAwardYear.StudentId;
            request.PaperCopyOptionFlag = studentAwardYear.IsPaperCopyOptionSelected;

            var transactionResponse = await transactionInvoker.ExecuteAsync<UpdateCorrOptionFlagRequest, UpdateCorrOptionFlagResponse>(request);

            if (!string.IsNullOrEmpty(transactionResponse.ErrorMessage))
            {
                if (transactionResponse.ErrorMessage.ToLower() == string.Format("conflict: fin.aid record for student {0} is locked by a process", studentAwardYear.StudentId))
                {
                    var message = string.Format("Paper copy option flag update canceled because record id {0} in FIN.AID table is locked.", studentAwardYear.StudentId);
                    logger.Error(message);
                    throw new OperationCanceledException(message);
                }
                else
                {
                    var message = string.Format("Unable to update Paper Copy Option for student {1}. Error message from CTX: ", studentAwardYear.StudentId, transactionResponse.ErrorMessage);
                    logger.Error(message);
                    throw new Exception(message);
                }
            }

            return studentAwardYear;
        }
    }
}
