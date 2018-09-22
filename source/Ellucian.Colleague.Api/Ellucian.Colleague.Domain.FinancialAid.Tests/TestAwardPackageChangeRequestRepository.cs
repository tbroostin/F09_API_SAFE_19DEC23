/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestAwardPackageChangeRequestRepository : IAwardPackageChangeRequestRepository
    {
        public class LoanAmountChangeRequestRecord
        {
            public string id;
            public string studentId;
            public string awardYear;
            public string awardId;
            public string counselorId;
            public string statusCode;
            public DateTime? AddDate;
            public DateTime? AddTime;
            public List<LoanAmountChangePeriodRecord> periodChangeRequests;
        }

        public class LoanAmountChangePeriodRecord
        {
            public string awardPeriodId;
            public decimal? newAmount;
            public string newStatusId;
        }

        public class DeclinedStatusChangeRequestRecord
        {
            public string id;
            public string studentId;
            public string awardYear;
            public string awardId;
            public string counselorId;
            public string statusCode;
            public string declinedStatusCode;
            public DateTime? AddDate;
            public DateTime? AddTime;
            public List<DeclinedStatusChangePeriodRecord> periodChangeRequests;
        }

        public class DeclinedStatusChangePeriodRecord
        {
            public string awardPeriodId;
        }

        public class FACounselorEntity
        {
            public string FinancialAidCounselorId;
            public DateTime? FaCounselorStartDate;
            public DateTime? FaCounselorEndDate;
        }

        public List<LoanAmountChangeRequestRecord> LoanAmountChangeRequestData = new List<LoanAmountChangeRequestRecord>()
        {
            new LoanAmountChangeRequestRecord()
            {
                id = "1",
                studentId = "0003914",
                awardYear = "2015",
                awardId = "HAPPY",
                counselorId = "0010479",
                statusCode = "P",
                AddDate = new DateTime(2015, 01, 01),
                AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                periodChangeRequests = new List<LoanAmountChangePeriodRecord>()
                {
                    new LoanAmountChangePeriodRecord()
                    {
                        awardPeriodId = "15/FA",
                        newAmount = 500,
                        newStatusId = "A"
                    },
                    new LoanAmountChangePeriodRecord()
                    {
                        awardPeriodId = "15/WI",
                        newAmount = 600,
                        newStatusId = "A"
                    },
                }
            },
            new LoanAmountChangeRequestRecord()
            {
                id = "2",
                studentId = "0003914",
                awardYear = "2015",
                awardId = "SNEEZY",
                counselorId = "0010479",
                statusCode = "P",
                AddDate = new DateTime(2015, 01, 01),
                AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                periodChangeRequests = new List<LoanAmountChangePeriodRecord>()
                {
                    new LoanAmountChangePeriodRecord()
                    {
                        awardPeriodId = "15/FA",
                        newAmount = 500,
                        newStatusId = "A"
                    },
                    new LoanAmountChangePeriodRecord()
                    {
                        awardPeriodId = "15/WI",
                        newAmount = 600,
                        newStatusId = "A"
                    },
                }
            },
        };

        public List<DeclinedStatusChangeRequestRecord> DeclinedStatusChangeRequestData = new List<DeclinedStatusChangeRequestRecord>()
        {
            new DeclinedStatusChangeRequestRecord()
            {
                id = "5",
                studentId = "0003914",
                awardYear = "2015",
                awardId = "LAZY",
                counselorId = "0010479",
                statusCode = "P",
                declinedStatusCode = "R",
                AddDate = new DateTime(2015, 01, 01),
                AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                periodChangeRequests = new List<DeclinedStatusChangePeriodRecord>()
                {
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "15/FA",
                    },
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "15/WI",
                    }
                }
            },
            new DeclinedStatusChangeRequestRecord()
            {
                id = "6",
                studentId = "0003914",
                awardYear = "2015",
                awardId = "GRUMPY",
                counselorId = "0010479",
                statusCode = "P",
                declinedStatusCode = "D",
                AddDate = new DateTime(2015, 01, 01),
                AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                periodChangeRequests = new List<DeclinedStatusChangePeriodRecord>()
                {
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "15/FA",
                    },
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "15/WI",
                    }
                }
            },
            new DeclinedStatusChangeRequestRecord()
            {
                id = "76",
                studentId = "0003914",
                awardYear = "2017",
                awardId = "UNSUB1",
                counselorId = "0010479",
                statusCode = "P",
                declinedStatusCode = "D",
                AddDate = new DateTime(2017, 01, 01),
                AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                periodChangeRequests = new List<DeclinedStatusChangePeriodRecord>()
                {
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "17/FA",
                    },
                    new DeclinedStatusChangePeriodRecord()
                    {
                        awardPeriodId = "18/SP",
                    }
                }
            }
        };
        
        public Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId)
        {
            return Task.FromResult(LoanAmountChangeRequestData.Where(r => r.studentId == studentId).Select(record =>
                {
                    try
                    {
                        return new AwardPackageChangeRequest(record.id, record.studentId, record.awardYear, record.awardId)
                        {
                            AssignedToCounselorId = record.counselorId,
                            CreateDateTime = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, TimeZoneInfo.Local.Id),
                            AwardPeriodChangeRequests = record.periodChangeRequests.Select(period =>
                                new AwardPeriodChangeRequest(period.awardPeriodId)
                                {
                                    NewAmount = period.newAmount,
                                    NewAwardStatusId = period.newStatusId,
                                    Status = TranslateRequestStatusCode(record.statusCode)
                                }).ToList()
                        };
                    }
                    catch (Exception) { return null; }
                })
                .Where(cr => cr != null)
                .Concat(
                DeclinedStatusChangeRequestData.Where(r => r.studentId == studentId).Select(record =>
                    {
                        try
                        {
                            return new AwardPackageChangeRequest(record.id, record.studentId, record.awardYear, record.awardId)
                            {
                                AssignedToCounselorId = record.counselorId,
                                CreateDateTime = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, TimeZoneInfo.Local.Id),
                                AwardPeriodChangeRequests = record.periodChangeRequests.Select(period =>
                                    new AwardPeriodChangeRequest(period.awardPeriodId)
                                    {
                                        NewAwardStatusId = record.declinedStatusCode,
                                        Status = TranslateRequestStatusCode(record.statusCode)
                                    }).ToList()
                            };
                        }
                        catch (Exception) { return null; }
                    })
                    .Where(cr => cr != null)));
        }

        public List<FACounselorEntity> counselorEntities = new List<FACounselorEntity>()
        {
            new FACounselorEntity()
            {
                FinancialAidCounselorId = "4567382",
                FaCounselorStartDate = new DateTime(2016, 01, 01),
                FaCounselorEndDate = new DateTime(2016, 05, 01)
            },
            new FACounselorEntity()
            {
                FinancialAidCounselorId = "99999999",
                FaCounselorStartDate = null,
                FaCounselorEndDate = null
            }
        };

        public Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId, StudentAward studentAward)
        {
            IEnumerable<AwardPackageChangeRequest> changeRequests = new List<AwardPackageChangeRequest>();
               
            if (!studentAward.Award.AwardCategoryType.HasValue || studentAward.Award.AwardCategoryType.Value == AwardCategoryType.Loan)
            {
                changeRequests = changeRequests.Concat(LoanAmountChangeRequestData.Where(r => r.studentId == studentId).Select(record =>
                {
                    try
                    {
                        return new AwardPackageChangeRequest(record.id, record.studentId, record.awardYear, record.awardId)
                        {
                            AssignedToCounselorId = record.counselorId,
                            CreateDateTime = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, TimeZoneInfo.Local.Id),
                            AwardPeriodChangeRequests = record.periodChangeRequests.Select(period =>
                                new AwardPeriodChangeRequest(period.awardPeriodId)
                                {
                                    NewAmount = period.newAmount,
                                    NewAwardStatusId = period.newStatusId,
                                    Status = TranslateRequestStatusCode(record.statusCode)
                                }).ToList()
                        };
                    }
                    catch (Exception) { return null; }
                })
                .Where(cr => cr != null));
            }
            changeRequests = changeRequests.Concat(DeclinedStatusChangeRequestData.Where(r => r.studentId == studentId).Select(record =>
                    {
                        try
                        {
                            return new AwardPackageChangeRequest(record.id, record.studentId, record.awardYear, record.awardId)
                            {
                                AssignedToCounselorId = record.counselorId,
                                CreateDateTime = record.AddTime.ToPointInTimeDateTimeOffset(record.AddDate, TimeZoneInfo.Local.Id),
                                AwardPeriodChangeRequests = record.periodChangeRequests.Select(period =>
                                    new AwardPeriodChangeRequest(period.awardPeriodId)
                                    {
                                        NewAwardStatusId = record.declinedStatusCode,
                                        Status = TranslateRequestStatusCode(record.statusCode)
                                    }).ToList()
                            };
                        }
                        catch (Exception) { return null; }
                    })
                    .Where(cr => cr != null)
            );
            return Task.FromResult(changeRequests);
        }

        public Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(AwardPackageChangeRequest awardPackageChangeRequest, StudentAward originalStudentAward)
        {
            awardPackageChangeRequest.Id = awardPackageChangeRequest.GetHashCode().ToString();
            awardPackageChangeRequest.CreateDateTime = ColleagueTimeZoneUtility.ToPointInTimeDateTimeOffset(DateTime.Now, DateTime.Today, TimeZoneInfo.Local.Id);

            return Task.FromResult(awardPackageChangeRequest);
        }

        private AwardPackageChangeRequestStatus TranslateRequestStatusCode(string requestStatus)
        {
            if (string.IsNullOrEmpty(requestStatus))
            {
                throw new ArgumentNullException("requestStatus");
            }
            switch (requestStatus.ToUpper())
            {
                case "A":
                    return AwardPackageChangeRequestStatus.Accepted;
                case "P":
                    return AwardPackageChangeRequestStatus.Pending;
                case "R":
                    return AwardPackageChangeRequestStatus.RejectedByCounselor;
                default:
                    var message = string.Format("Unable to translate request status {0}", requestStatus);
                    throw new ApplicationException(message);
            }

        }

        /// <summary>
        /// Test Helper to add change request records for the given student awards
        /// </summary>
        /// <param name="studentAwards"></param>
        public void AddPendingChangeRequestRecordsForStudentAwards(IEnumerable<StudentAward> studentAwards)
        {
            for (int i = 0; i < studentAwards.Count(); i++)
            {
                var studentAward = studentAwards.ElementAt(i);
                if (i % 2 == 0)
                {
                    LoanAmountChangeRequestData.Add(
                        new LoanAmountChangeRequestRecord()
                        {
                            id = studentAward.GetHashCode().ToString(),
                            studentId = studentAward.StudentId,
                            awardYear = studentAward.StudentAwardYear.Code,
                            awardId = studentAward.Award.Code,
                            counselorId = "0010479",
                            statusCode = "P",
                            AddDate = new DateTime(2015, 01, 01),
                            AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                            periodChangeRequests = studentAward.StudentAwardPeriods.Select(period =>
                                new LoanAmountChangePeriodRecord()
                                {
                                    awardPeriodId = period.AwardPeriodId,
                                    newAmount = period.AwardAmount + 400,
                                    newStatusId = "A"
                                }).ToList()
                        });
                }
                else
                {
                    DeclinedStatusChangeRequestData.Add(
                        new DeclinedStatusChangeRequestRecord()
                        {
                            id = studentAward.GetHashCode().ToString(),
                            studentId = studentAward.StudentId,
                            awardYear = studentAward.StudentAwardYear.Code,
                            awardId = studentAward.Award.Code,
                            counselorId = "0010479",
                            statusCode = "P",
                            declinedStatusCode = "D",
                            AddDate = new DateTime(2015, 01, 01),
                            AddTime = new DateTime(1, 1, 1, 15, 23, 55),
                            periodChangeRequests = studentAward.StudentAwardPeriods.Select(period =>
                                new DeclinedStatusChangePeriodRecord()
                                {
                                    awardPeriodId = period.AwardPeriodId
                                }).ToList()
                        });
                }
            }
        }

    }
}
