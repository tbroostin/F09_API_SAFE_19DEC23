//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentAwardRepository : IStudentAwardRepository
    {
        #region TestAwardPeriodData
        /// <summary>
        /// This class mimics a TaAcyr data contract
        /// </summary>
        public class TestAwardPeriodRecord
        {
            public string year;
            public string award;
            public string awardPeriod;
            public decimal? awardAmount;
            public string awardStatus;
            public AwardStatusCategory awardStatusCategory;
            public decimal? xmitAmount;
        }

        /// <summary>
        /// awardPeriodData populuates test data into TestAwardPeriodData objects 
        /// </summary>
        public List<TestAwardPeriodRecord> awardPeriodData = new List<TestAwardPeriodRecord>()
        {      
            new TestAwardPeriodRecord()
            {
                //not modifiable - not a loan
                year = "2012",
                award = "PELL",
                awardPeriod = "12/FA",
                awardAmount = (decimal)132.32,
                awardStatus = "A",
                awardStatusCategory = AwardStatusCategory.Accepted,
                xmitAmount = (decimal)123.32
            },

            new TestAwardPeriodRecord()
            {   //modifiable
                year = "2012",
                award = "SUB1",
                awardPeriod = "13/SP",
                awardAmount = (decimal)132.32,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - xmit amount has value
                year = "2012",
                award = "SUB1",
                awardPeriod = "12/WI",
                awardAmount = (decimal)2932.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = (decimal)2232.24
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - frozen award period
                year = "2012",
                award = "SUB1",
                awardPeriod = "13/SU",
                awardAmount = (decimal)2932.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //modifiable
                year = "2012",
                award = "UNSUB1",
                awardPeriod = "13/SP",
                awardAmount = (decimal)4924.24,
                awardStatus = "E",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - award status category is not Pending
                year = "2012",
                award = "UNSUB1",
                awardPeriod = "12/WI",
                awardAmount = (decimal)4924.24,
                awardStatus = "A",
                awardStatusCategory = AwardStatusCategory.Accepted,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //modifiable
                year = "2012",
                award = "GPLUS1",
                awardPeriod = "12/WI",
                awardAmount = (decimal)3872.32,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - associated TestAwardData is not modifiable
                year = "2013",
                award = "UNSUB2",
                awardPeriod = "13/WI",
                awardAmount = (decimal)4924.24,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - associated TestAwardData is not modifiable
                year = "2013",
                award = "SUB2",
                awardPeriod = "13/WI",
                awardAmount = (decimal)4924.24,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },
            new TestAwardPeriodRecord()
            {   //not modifiable - associated TestAwardData is not modifiable
                year = "2013",
                award = "GPLUS2",
                awardPeriod = "13/WI",
                awardAmount = (decimal)1234.54,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - associated TestAwardData is not modifiable
                year = "2014",
                award = "SUB3",
                awardPeriod = "14/FA",
                awardAmount = (decimal)4924.24,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },

            new TestAwardPeriodRecord()
            {   //not modifiable - associated TestAwardData is not modifiable
                year = "2014",
                award = "SUB3DL",
                awardPeriod = "14/FA",
                awardAmount = (decimal)4924.24,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },
            new TestAwardPeriodRecord()
            {   //not modifiable - not supported loan type
                year = "2014",
                award = "PPLUS1",
                awardPeriod = "14/FA",
                awardAmount = (decimal)9837.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },
            new TestAwardPeriodRecord()
            {   //modifiable sub
                year = "2017",
                award = "SUBDL",
                awardPeriod = "17/FA",
                awardAmount = (decimal)9837.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },
            new TestAwardPeriodRecord()
            {   //modifiable unsub
                year = "2017",
                award = "UNSUB1",
                awardPeriod = "17/FA",
                awardAmount = (decimal)345.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            },
            new TestAwardPeriodRecord()
            {   //modifiable unsub
                year = "2017",
                award = "UNSUB2",
                awardPeriod = "17/FA",
                awardAmount = (decimal)9837.23,
                awardStatus = "P",
                awardStatusCategory = AwardStatusCategory.Pending,
                xmitAmount = null
            }
        };
        #endregion

        #region TestAwardData

        /// <summary>
        /// This class mimics an SA Acyr data contract
        /// </summary>
        public class TestAwardRecord
        {
            public string awardYear;
            public List<string> awardCodes;
            public List<TestAwardPeriodAssociationData> periodAssociation;

            public class TestAwardPeriodAssociationData
            {
                public string awardPeriodId;
                public bool isFrozenOnAttendancePattern;
            }
        }

        /// <summary>
        /// awardData contains a list of TestAwardData objects. The award codes and award periods
        /// come from the TestAwardPeriodData list. Award codes must also exist in TestFinancialAidReferenceDataRepository
        /// </summary>
        public List<TestAwardRecord> awardData = new List<TestAwardRecord>()
        {
            new TestAwardRecord()
            {
                awardYear = "2012",
                awardCodes = new List<string>() {"PELL", "SUB1", "UNSUB1", "GPLUS1"},
                periodAssociation = new List<TestAwardRecord.TestAwardPeriodAssociationData>()
                {
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "12/FA", isFrozenOnAttendancePattern = false},
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "12/WI", isFrozenOnAttendancePattern = false},
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "13/SP", isFrozenOnAttendancePattern = false},
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "13/SU", isFrozenOnAttendancePattern = false}
                }
            },
            new TestAwardRecord()
            {
                awardYear = "2013",
                awardCodes = new List<string>() {"UNSUB2", "SUB2","GPLUS2"},
                periodAssociation = new List<TestAwardRecord.TestAwardPeriodAssociationData>()
                {
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "13/WI", isFrozenOnAttendancePattern = false}
                }                
            },
            new TestAwardRecord()
            {
                awardYear = "2014",
                awardCodes = new List<string>() {"SUB3DL", "SUB3", "PPLUS1"},
                periodAssociation = new List<TestAwardRecord.TestAwardPeriodAssociationData>()
                {
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "14/FA", isFrozenOnAttendancePattern = false}
                }
            },
            new TestAwardRecord()
            {
                awardYear = "2015",
                awardCodes = new List<string>() {"SUB3DL", "SUB3", "PPLUS1"},
                periodAssociation = new List<TestAwardRecord.TestAwardPeriodAssociationData>()
                {
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "14/FA", isFrozenOnAttendancePattern = false}
                }
            },
            new TestAwardRecord()
            {
                awardYear = "2017",
                awardCodes = new List<string>() {"SUBDL", "UNSUB1", "UNSUB2"},
                periodAssociation = new List<TestAwardRecord.TestAwardPeriodAssociationData>()
                {
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "17/FA", isFrozenOnAttendancePattern = false},
                    new TestAwardRecord.TestAwardPeriodAssociationData() {awardPeriodId = "18/SP", isFrozenOnAttendancePattern = false}
                }
            }
        };

        #endregion

        #region TestLoanData

        /// <summary>
        /// This class mimics an SL.ACYR record
        /// </summary>
        public class TestLoanRecord
        {
            public string awardYear;
            public string awardId;
            public List<string> anticipatedDisbursementAwardPeriodIds;
        }

        public List<TestLoanRecord> loanData = new List<TestLoanRecord>()
        {
            new TestLoanRecord()
            {
                awardYear = "2012",
                awardId = "SUB1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"12/FA", "13/SP", "13/SU"}
            },
            new TestLoanRecord()
            {
                awardYear = "2012",
                awardId = "UNSUB1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"12/FA","12/WI", "13/SP", "13/SU"}
            },
            new TestLoanRecord()
            {
                awardYear = "2012",
                awardId = "GPLUS1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"12/FA", "13/SP", "13/SU"}
            },
            new TestLoanRecord()
            {
                awardYear = "2013",
                awardId = "UNSUB2",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"13/WI"}
            },
            new TestLoanRecord()
            {
                awardYear = "2013",
                awardId = "SUB2",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"13/WI"}
            },
            new TestLoanRecord()
            {
                awardYear = "2013",
                awardId = "GPLUS2",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"13/WI"}
            },
            new TestLoanRecord()
            {
                awardYear = "2014",
                awardId = "SUB3DL",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2014",
                awardId = "SUB3",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2014",
                awardId = "PPLUS1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2015",
                awardId = "SUB3DL",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2015",
                awardId = "SUB3",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2015",
                awardId = "PPLUS1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"14/FA"}
            },
            new TestLoanRecord()
            {
                awardYear = "2017",
                awardId = "SUBDL",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"17/FA", "18/SP"}
            },
            new TestLoanRecord()
            {
                awardYear = "2017",
                awardId = "UNSUB1",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"17/FA", "18/SP"}
            },
            new TestLoanRecord()
            {
                awardYear = "2017",
                awardId = "UNSUB2",
                anticipatedDisbursementAwardPeriodIds = new List<string>() {"17/FA", "18/SP"}
            }
        };

        #endregion

        public void ClearAllAwardData()
        {
            awardPeriodData = new List<TestAwardPeriodRecord>();
            awardData = new List<TestAwardRecord>();
            loanData = new List<TestLoanRecord>();

        }

        public StudentAward DeepCopy(StudentAward studentAwardToCopy)
        {
            var newStudentAward = new StudentAward(studentAwardToCopy.StudentAwardYear, studentAwardToCopy.StudentId, studentAwardToCopy.Award, studentAwardToCopy.IsEligible)
            {
                PendingChangeRequestId = studentAwardToCopy.PendingChangeRequestId
            };
            foreach (var periodToCopy in studentAwardToCopy.StudentAwardPeriods)
            {
                new StudentAwardPeriod(newStudentAward, periodToCopy.AwardPeriodId, periodToCopy.AwardStatus, periodToCopy.IsFrozen, periodToCopy.IsTransmitted)
                {
                    AwardAmount = periodToCopy.AwardAmount,
                    HasLoanDisbursement = periodToCopy.HasLoanDisbursement
                };
            }

            return newStudentAward;
        }

        //Build the StudentAward Entities
        public Task<IEnumerable<StudentAward>> GetAllStudentAwardsAsync(string studentId, IEnumerable<StudentAwardYear> studentAwardYears, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            var studentAwardPeriodList = new List<StudentAwardPeriod>();
            var studentAwards = new List<StudentAward>();

            foreach (var studentAwardYear in studentAwardYears)
            {
                var awardPeriodDataForYear = awardPeriodData.Where(ap => ap.year == studentAwardYear.Code);

                foreach (var periodData in awardPeriodDataForYear)
                {
                    var studentYearData = awardData.FirstOrDefault(a => a.awardYear == periodData.year);
                    var studentAwardData = studentYearData.awardCodes.FirstOrDefault(a => a == periodData.award);
                    var awardObj = allAwards.First(a => a.Code == periodData.award);

                    var studentAwardObj = studentAwards.FirstOrDefault(sa => sa.StudentAwardYear.Equals(studentAwardYear) && sa.Award.Code == periodData.award);
                    if (studentAwardObj == null)
                    {
                        studentAwardObj = new StudentAward(studentAwardYear, studentId, awardObj, true);
                        studentAwards.Add(studentAwardObj);
                    }

                    bool isPeriodFrozen = studentYearData.periodAssociation
                        .Where(p => p.isFrozenOnAttendancePattern)
                        .Select(p => p.awardPeriodId)
                        .Contains(periodData.awardPeriod);

                    var awardPeriodsWithLoanDisbursements = loanData
                        .Where(l => l.awardId.ToUpper() == studentAwardObj.Award.Code.ToUpper())
                        .SelectMany(l => l.anticipatedDisbursementAwardPeriodIds);

                    var awardStatusObj = allAwardStatuses.First(a => a.Code == periodData.awardStatus);

                    var studentAwardPeriod = new StudentAwardPeriod(studentAwardObj, periodData.awardPeriod, awardStatusObj, isPeriodFrozen, periodData.xmitAmount.HasValue);
                    studentAwardPeriod.AwardAmount = periodData.awardAmount;
                    studentAwardPeriod.HasLoanDisbursement = awardPeriodsWithLoanDisbursements.Contains(studentAwardPeriod.AwardPeriodId);
                }
            }

            return Task.FromResult(studentAwards.AsEnumerable());
        }

        public Task<IEnumerable<StudentAward>> GetStudentAwardsForYearAsync(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            return GetAllStudentAwardsAsync(studentId, new List<StudentAwardYear>() { studentAwardYear }, allAwards, allAwardStatuses);
        }

        public Task<StudentAward> GetStudentAwardAsync(string studentId, StudentAwardYear studentAwardYear, string awardCode, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            return Task.FromResult(GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses).Result.First(a => a.Award.Code == awardCode));
        }

        public IEnumerable<StudentAward> GetStudentAwardSummaryForYear(string studentId, StudentAwardYear studentAwardYear, IEnumerable<Award> allAwards, IEnumerable<AwardStatus> allAwardStatuses)
        {
            return GetStudentAwardsForYearAsync(studentId, studentAwardYear, allAwards, allAwardStatuses).Result;
        }


        public Task<IEnumerable<StudentAward>> UpdateStudentAwardsAsync(StudentAwardYear studentAwardYear, IEnumerable<StudentAward> studentAwards, IEnumerable<Award> awards, IEnumerable<AwardStatus> awardStatuses)
        {
            return Task.FromResult(studentAwards);
        }
    }
}

