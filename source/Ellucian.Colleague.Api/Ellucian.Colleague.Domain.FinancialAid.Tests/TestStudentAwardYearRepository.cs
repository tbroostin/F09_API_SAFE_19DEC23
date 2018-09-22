/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentAwardYearRepository : IStudentAwardYearRepository
    {
        //public string studentId = "0004791";

        public static TestLoanRequestRepository testLoanRequestRepository = new TestLoanRequestRepository();

        public class FaStudent
        {
            public List<string> FaCsYears;
            public List<string> FaSaYears;
            public List<string> FaYsYears;
            public string FaPaperCopyOptInFlag;
            public List<string> PendingLoanRequestIds;
            public string StudentId;
        }

        //pendingLoanRequestIds comes from TestLoanRequestRepository
        public FaStudent FaStudentData = new FaStudent()
        {
            FaCsYears = new List<string>() { "2012", "2013" },
            FaSaYears = new List<string>() { "2012", "2015", "2017" },
            FaYsYears = new List<string>() { "2012", "2013", "2014", "2014" },
            FaPaperCopyOptInFlag = "Y",
            PendingLoanRequestIds = new List<string>() { "1", "2" }
        };

        //helper method for running tests when no student award years exist
        public void ClearAwardYears()
        {
            FaStudentData.FaSaYears = new List<string>();
            FaStudentData.FaCsYears = new List<string>();
            FaStudentData.FaYsYears = new List<string>();
        }

        public class CsStudent
        {
            public string AwardYear;
            public string LocationId;
            public int? TotalEstimatedExpenses;
            public int? ExpensesAdjustment;
            public string FedIsirId;
            public string StudentId;
        }

        //See TestFinancialAidReferenceDataRepository -- LocationIds should come from the LocationRecordData in the Offices region
        public List<CsStudent> CsStudentData = new List<CsStudent>()
        {
            new CsStudent() {AwardYear = "2012", LocationId = "MC", ExpensesAdjustment = 1234, TotalEstimatedExpenses = 43211, FedIsirId = "2"},
            new CsStudent() {AwardYear = "2013", LocationId = "MC", ExpensesAdjustment = 2345, TotalEstimatedExpenses = 54322, FedIsirId = "3"},
            new CsStudent() {AwardYear = "2014", LocationId = "MC", ExpensesAdjustment = null, TotalEstimatedExpenses = 54321, FedIsirId = "4"},
            new CsStudent() {AwardYear = "2015", LocationId = "MC", ExpensesAdjustment = null, TotalEstimatedExpenses = 11111},
            new CsStudent() {AwardYear = "2017", LocationId = "MC", ExpensesAdjustment = null, TotalEstimatedExpenses = 98765, FedIsirId = "5"}
        };

        public class SaStudent
        {
            public string AwardYear;
            public decimal? TotalAwardedAmount;
            public string StudentId;
        }

        public List<SaStudent> SaStudentData = new List<SaStudent>()
        {
            new SaStudent() { AwardYear = "2012", TotalAwardedAmount = (decimal) 12345.67},
            new SaStudent() { AwardYear = "2013", TotalAwardedAmount = (decimal) 76543},
            new SaStudent() { AwardYear = "2014", TotalAwardedAmount = (decimal) 9283.21},
            new SaStudent() { AwardYear = "2015", TotalAwardedAmount = (decimal) 9283.21},
            new SaStudent() { AwardYear = "2017", TotalAwardedAmount = (decimal) 8754.21}
        };

        public class StudentYs
        {
            public string awardYear;
            public DateTime? FileCompleteDate;
            public string StudentId;
            public DateTime? ApplicationReviewedDate;
        }

        public List<StudentYs> YsStudentData = new List<StudentYs>()
        {
            new StudentYs()
            {
                awardYear = "2012",                
                ApplicationReviewedDate = DateTime.Today
            },
            new StudentYs()
            {
                awardYear = "2013",              
                ApplicationReviewedDate = new DateTime(2014, 01, 22)
            },
            new StudentYs()
            {
                awardYear = "2014",                
                ApplicationReviewedDate = null
            },
            new StudentYs()
            {
                awardYear = "2015",                
                ApplicationReviewedDate = null
            }

        };

        public class AwardLetterHistoryRecord
        {
            public string recordKey;
            public DateTime createdDate;
            public string awardYearCode;
            public string studentId;
            public DateTime addTime;
        }

        public List<AwardLetterHistoryRecord> AwardLetterHistoryRecordsData = new List<AwardLetterHistoryRecord>()
        {
            new AwardLetterHistoryRecord(){
                recordKey = "1",
                createdDate = new DateTime(2015, 01, 01),
                awardYearCode = "2015",
                addTime = new DateTime(2015, 01, 01, 3, 23, 55)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "2",
                createdDate = new DateTime(2015, 02, 01),
                awardYearCode = "2015",
                addTime = new DateTime(2015, 02, 01, 1, 0, 0)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "22",
                createdDate = new DateTime(2015, 02, 01),
                awardYearCode = "2015",
                addTime = new DateTime(2015, 02, 01, 1, 1, 0)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "23",
                createdDate = new DateTime(2015, 02, 01),
                awardYearCode = "2015",
                addTime = new DateTime(2015, 02, 01, 1, 0, 23)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "24",
                createdDate = new DateTime(2015, 02, 02),
                awardYearCode = "2015",
                addTime = new DateTime(2015, 02, 02, 1, 0, 0)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "3",
                createdDate = new DateTime(2014, 08, 09),
                awardYearCode = "2014",
                addTime = new DateTime(2014, 08, 09, 12, 44, 12)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "9",
                createdDate = new DateTime(2014, 08, 09),
                awardYearCode = "2014",
                addTime = new DateTime(2014, 08, 09, 12, 44, 12)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "4",
                createdDate = new DateTime(2014, 10, 09),
                awardYearCode = "2014",
                addTime = new DateTime(2014, 10, 09, 12, 44, 12)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "5",
                createdDate = new DateTime(2013, 02, 09),
                awardYearCode = "2013",
                addTime = new DateTime(2013, 02, 09, 0 , 0, 0)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "6",
                createdDate = new DateTime(2013, 12, 23),
                awardYearCode = "2013",
                addTime = new DateTime(2013, 12, 23, 10, 10, 10)
            },

            new AwardLetterHistoryRecord(){
                recordKey = "7",
                createdDate = new DateTime(2013, 08, 09),
                awardYearCode = "2013",
                addTime = new DateTime(2013, 08, 09, 11, 11, 12)
            }
        };

        public class UpdateCorrOptionTransactionResponse
        {
            public string ErrorMessage;
        }

        public UpdateCorrOptionTransactionResponse transactionResponseData = new UpdateCorrOptionTransactionResponse()
        {
            ErrorMessage = null
        };

        public IEnumerable<StudentAwardYear> GetStudentAwardYears(string studentId, CurrentOfficeService currentOfficeService)
        {
            var yearsList = FaStudentData.FaCsYears.Concat(FaStudentData.FaSaYears).Concat(FaStudentData.FaYsYears);
            yearsList = yearsList.Where(y => !string.IsNullOrEmpty(y)).Distinct().OrderBy(y => y);

            var pendingLoanRequests = testLoanRequestRepository.NewLoanRequestList.Where(r => FaStudentData.PendingLoanRequestIds.Contains(r.id));
            var awardYearLoanRequestDictionary = new Dictionary<string, string>();
            foreach (var request in pendingLoanRequests)
            {
                awardYearLoanRequestDictionary.Add(request.awardYear, request.id);
            }

            var awardLetterHistoryRecords = AwardLetterHistoryRecordsData;
            var studentAwardYears = new List<StudentAwardYear>();
            var isPaperCopyOptionSelected = (!string.IsNullOrEmpty(FaStudentData.FaPaperCopyOptInFlag) && FaStudentData.FaPaperCopyOptInFlag.ToUpper() == "Y");
            foreach (var year in yearsList)
            {

                var locationId = string.Empty;
                decimal? totalEstimatedExpenses = null;
                decimal? estimatedExpensesAdjustment = null;
                string fedIsirId = string.Empty;

                if (FaStudentData.FaCsYears.Contains(year))
                {
                    var csRecord = CsStudentData.FirstOrDefault(cs => cs.AwardYear == year);
                    if (csRecord != null)
                    {
                        locationId = csRecord.LocationId;
                        totalEstimatedExpenses = csRecord.TotalEstimatedExpenses;
                        estimatedExpensesAdjustment = csRecord.ExpensesAdjustment;
                        fedIsirId = csRecord.FedIsirId;
                    }
                }

                var studentAwardYear = new StudentAwardYear(studentId, year, currentOfficeService.GetCurrentOfficeByLocationId(locationId));
                studentAwardYear.TotalEstimatedExpenses = totalEstimatedExpenses;
                studentAwardYear.EstimatedExpensesAdjustment = estimatedExpensesAdjustment;
                studentAwardYear.IsPaperCopyOptionSelected = isPaperCopyOptionSelected;
                studentAwardYear.FederallyFlaggedIsirId = fedIsirId;

                string requestId;
                awardYearLoanRequestDictionary.TryGetValue(year, out requestId);
                studentAwardYear.PendingLoanRequestId = requestId;

                var awardLetterHistoryRecordsForYear = awardLetterHistoryRecords != null ?
                        awardLetterHistoryRecords.Where(r => r.awardYearCode == year).ToList() : null;

                if (awardLetterHistoryRecordsForYear != null && awardLetterHistoryRecordsForYear.Any())
                {
                    foreach (var record in awardLetterHistoryRecordsForYear)
                    {
                        studentAwardYear.AwardLetterHistoryItemsForYear.Add(
                            new AwardLetterHistoryItem(record.recordKey, record.createdDate));
                    }
                }

                if (FaStudentData.FaSaYears.Contains(year))
                {
                    var saRecord = SaStudentData.FirstOrDefault(sa => sa.AwardYear == year);
                    if (saRecord != null)
                    {
                        studentAwardYear.TotalAwardedAmount = saRecord.TotalAwardedAmount ?? 0;
                    }
                }

                if (FaStudentData.FaYsYears.Contains(year))
                {
                    var ysRecord = YsStudentData.FirstOrDefault(ys => ys.awardYear == year);
                    if (ysRecord != null)
                    {
                        studentAwardYear.IsApplicationReviewed = (ysRecord.ApplicationReviewedDate.HasValue && ysRecord.ApplicationReviewedDate.Value >= DateTime.Today);
                    }
                }

                studentAwardYears.Add(studentAwardYear);

            }

            return studentAwardYears;
        }

        public Task<IEnumerable<StudentAwardYear>> GetStudentAwardYearsAsync(string studentId, CurrentOfficeService currentOfficeService, bool getActiveYearsOnly = false)
        {
            var yearsList = FaStudentData.FaCsYears.Concat(FaStudentData.FaSaYears).Concat(FaStudentData.FaYsYears);
            yearsList = yearsList.Where(y => !string.IsNullOrEmpty(y)).Distinct().OrderBy(y => y);

            var pendingLoanRequests = testLoanRequestRepository.NewLoanRequestList.Where(r => FaStudentData.PendingLoanRequestIds.Contains(r.id));
            var awardYearLoanRequestDictionary = new Dictionary<string, string>();
            foreach (var request in pendingLoanRequests)
            {
                awardYearLoanRequestDictionary.Add(request.awardYear, request.id);
            }

            var awardLetterHistoryRecords = AwardLetterHistoryRecordsData;
            var studentAwardYears = new List<StudentAwardYear>();
            var isPaperCopyOptionSelected = (!string.IsNullOrEmpty(FaStudentData.FaPaperCopyOptInFlag) && FaStudentData.FaPaperCopyOptInFlag.ToUpper() == "Y");
            foreach (var year in yearsList)
            {

                var locationId = string.Empty;
                decimal? totalEstimatedExpenses = null;
                decimal? estimatedExpensesAdjustment = null;
                string fedIsirId = string.Empty;

                if (FaStudentData.FaCsYears.Contains(year))
                {
                    var csRecord = CsStudentData.FirstOrDefault(cs => cs.AwardYear == year);
                    if (csRecord != null)
                    {
                        locationId = csRecord.LocationId;
                        totalEstimatedExpenses = csRecord.TotalEstimatedExpenses;
                        estimatedExpensesAdjustment = csRecord.ExpensesAdjustment;
                        fedIsirId = csRecord.FedIsirId;
                    }
                }

                var studentAwardYear = new StudentAwardYear(studentId, year, currentOfficeService.GetCurrentOfficeByLocationId(locationId));
                studentAwardYear.TotalEstimatedExpenses = totalEstimatedExpenses;
                studentAwardYear.EstimatedExpensesAdjustment = estimatedExpensesAdjustment;
                studentAwardYear.IsPaperCopyOptionSelected = isPaperCopyOptionSelected;
                studentAwardYear.FederallyFlaggedIsirId = fedIsirId;

                string requestId;
                awardYearLoanRequestDictionary.TryGetValue(year, out requestId);
                studentAwardYear.PendingLoanRequestId = requestId;

                var awardLetterHistoryRecordsForYear = awardLetterHistoryRecords != null ?
                        awardLetterHistoryRecords.Where(r => r.awardYearCode == year).ToList() : null;

                if (awardLetterHistoryRecordsForYear != null && awardLetterHistoryRecordsForYear.Any())
                {
                    foreach (var record in awardLetterHistoryRecordsForYear)
                    {
                        studentAwardYear.AwardLetterHistoryItemsForYear.Add(
                            new AwardLetterHistoryItem(record.recordKey, record.createdDate));
                    }
                }

                if (FaStudentData.FaSaYears.Contains(year))
                {
                    var saRecord = SaStudentData.FirstOrDefault(sa => sa.AwardYear == year);
                    if (saRecord != null)
                    {
                        studentAwardYear.TotalAwardedAmount = saRecord.TotalAwardedAmount ?? 0;
                    }
                }

                if (FaStudentData.FaYsYears.Contains(year))
                {
                    var ysRecord = YsStudentData.FirstOrDefault(ys => ys.awardYear == year);
                    if (ysRecord != null)
                    {
                        studentAwardYear.IsApplicationReviewed = (ysRecord.ApplicationReviewedDate.HasValue && ysRecord.ApplicationReviewedDate.Value >= DateTime.Today);
                    }
                }

                studentAwardYears.Add(studentAwardYear);

            }

            return Task.FromResult(studentAwardYears.AsEnumerable());
        }

        public Task<StudentAwardYear> GetStudentAwardYearAsync(string studentId, string awardYearCode, CurrentOfficeService currentOfficeService)
        {
            return Task.FromResult(GetStudentAwardYears(studentId, currentOfficeService).First(y => y.Code == awardYearCode));
        }


        public StudentAwardYear UpdateStudentAwardYear(StudentAwardYear studentAwardYear)
        {
            return studentAwardYear;
        }

        public Task<StudentAwardYear> UpdateStudentAwardYearAsync(StudentAwardYear studentAwardYear)
        {
            return Task.FromResult(studentAwardYear);
        }
    }
}
