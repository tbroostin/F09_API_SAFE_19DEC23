// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public class TestAccountActivityRepository : IAccountActivityRepository
    {
        public class slAcyrRecord
        {
            public string id;
            public List<slDisb> disbursements;
        }

        public class slDisb
        {
            public string SlAntDisbTerm;
            public DateTime? SlAntDisbDate;
            public decimal? SlActDisbAmt;
            public DateTime? SlInitDisbDt;
        }

        public class pellAcyrRecord
        {
            public string id;
            public List<pellDisb> disbursements;
        }

        public class pellDisb
        {
            public string pellAntDisbTerm;
            public DateTime? pellDisbDate;
            public decimal? pellActDisbAmt;
            public DateTime? pellInitDisbDt;
        }

        public class tcAcyrRecord
        {
            public string id;
            public string tcDateId;
        }

        public class dateAwardRecord
        {
            public string id;
            public List<string> tcDateIds;
        }

        public class dateAwardDisbRecord
        {
            public string id;
            public string dawdAwardPeriod;
            public DateTime? dawdDate;
            public decimal? dawdXmitAmount;
            public DateTime? dawdInitialXmitDate;
        }

        public List<slAcyrRecord> slAcyrs = new List<slAcyrRecord>()
        {
            new slAcyrRecord() { id = "PPLUS1",
            disbursements = new List<slDisb>()
            {
                new slDisb()
                {
                    SlActDisbAmt = 345,
                    SlAntDisbDate = new DateTime(2018, 01, 18),
                    SlAntDisbTerm = "16/SP",
                    SlInitDisbDt = new DateTime(2018, 01, 17)
                },
                new slDisb()
                {
                    SlActDisbAmt = 345,
                    SlAntDisbDate = new DateTime(2017, 09, 30),
                    SlAntDisbTerm = "15/FA",
                    SlInitDisbDt = new DateTime(2018, 09, 28)
                }
            }
            },
            new slAcyrRecord() { id = "SUB3",
            disbursements = new List<slDisb>()
            {
                new slDisb()
                {
                    SlActDisbAmt = 678,
                    SlAntDisbDate = new DateTime(2018, 01, 18),
                    SlAntDisbTerm = "16/SP",
                    SlInitDisbDt = new DateTime(2018, 01, 17)
                },
                new slDisb()
                {
                    SlActDisbAmt = 876,
                    SlAntDisbDate = new DateTime(2017, 09, 30),
                    SlAntDisbTerm = "15/FA",
                    SlInitDisbDt = new DateTime(2018, 09, 28)
                }
            }
            }
        };

        public List<tcAcyrRecord> tcAcyrs = new List<tcAcyrRecord>()
        {
            new tcAcyrRecord() {
                id ="UGTCH",
                tcDateId = "tc1"
            },
            new tcAcyrRecord() {
                id ="GRTCH",
                tcDateId = "tc2"
            }
        };

        public List<dateAwardRecord> dateAwards = new List<dateAwardRecord>()
        {
            new dateAwardRecord() {
                id = "tc1",
                tcDateIds = new List<string>() { "1a", "2b", "3c" }
            },
            new dateAwardRecord() {
                id = "tc2",
                tcDateIds = new List<string>() { "2a", "3b", "4c" }
            }
        };

        public List<dateAwardDisbRecord> dateAwardDisbursements = new List<dateAwardDisbRecord>()
        {
            new dateAwardDisbRecord()
            {
                id = "1a",
                dawdAwardPeriod = "15/FA",
                dawdDate = new DateTime(2016, 08, 26),
                dawdInitialXmitDate = new DateTime(2016, 09, 13),
                dawdXmitAmount = 567
            },
            new dateAwardDisbRecord()
            {
                id = "2b",
                dawdAwardPeriod = "16/SP",
                dawdDate = new DateTime(2017, 02, 26),
                dawdInitialXmitDate = new DateTime(2017, 02, 13),
                dawdXmitAmount = 567
            },
            new dateAwardDisbRecord()
            {
                id = "3c",
                dawdAwardPeriod = "16/SU",
                dawdDate = new DateTime(2017, 05, 03),
                dawdInitialXmitDate = new DateTime(2017, 05, 13),
                dawdXmitAmount = 345
            },
            new dateAwardDisbRecord()
            {
                id = "2a",
                dawdAwardPeriod = "17/FA",
                dawdDate = new DateTime(2016, 08, 26),
                dawdInitialXmitDate = new DateTime(2016, 09, 13),
                dawdXmitAmount = 567
            },
            new dateAwardDisbRecord()
            {
                id = "3b",
                dawdAwardPeriod = "18/SP",
                dawdDate = new DateTime(2017, 02, 26),
                dawdInitialXmitDate = new DateTime(2017, 02, 13),
                dawdXmitAmount = 567
            },
            new dateAwardDisbRecord()
            {
                id = "4c",
                dawdAwardPeriod = "18/SU",
                dawdDate = new DateTime(2017, 05, 03),
                dawdInitialXmitDate = new DateTime(2017, 05, 13),
                dawdXmitAmount = 345
            }
        };

        public List<pellAcyrRecord> pellAcyrs = new List<pellAcyrRecord>()
        {
            new pellAcyrRecord() {
                id = "PELL",
                disbursements = new List<pellDisb>()
                {
                    new pellDisb()
                    {
                        pellInitDisbDt = new DateTime(2017, 01, 01),
                        pellActDisbAmt = 876,
                        pellDisbDate = DateTime.Today,
                        pellAntDisbTerm = "17/SU"
                    },
                    new pellDisb()
                    {
                        pellInitDisbDt = new DateTime(2017, 06, 01),
                        pellActDisbAmt = 1234,
                        pellDisbDate = new DateTime(2017, 06, 09),
                        pellAntDisbTerm = "17/SP"
                    }
                }
            }
        };

        public IEnumerable<AccountPeriod> GetAccountPeriods(string studentId)
        {
            throw new NotImplementedException();
        }

        public AccountPeriod GetNonTermAccountPeriod(string studentId)
        {
            throw new NotImplementedException();
        }

        public DetailedAccountPeriod GetTermActivityForStudent(string termId, string studentId)
        {
            throw new NotImplementedException();
        }

        public DetailedAccountPeriod GetTermActivityForStudent2(string termId, string studentId)
        {
            throw new NotImplementedException();
        }

        public DetailedAccountPeriod GetPeriodActivityForStudent(IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate, string studentId)
        {
            throw new NotImplementedException();
        }

        public DetailedAccountPeriod GetPeriodActivityForStudent2(IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate, string studentId)
        {
            throw new NotImplementedException();
        }

        public Task<StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYearCode, string awardId, TIVAwardCategory awardCategory)
        {
            StudentAwardDisbursementInfo disbInfo = new StudentAwardDisbursementInfo(studentId, awardId, awardYearCode);

            //Get separate disbursements information from SL.ACYR, TC.ACYR/DATE.AWARD, or PELL.ACYR based on the award category
            if (awardCategory == TIVAwardCategory.Loan)
            {
                var loanData = slAcyrs.FirstOrDefault(a => a.id == awardId);
                foreach (var disb in loanData.disbursements)
                {
                    disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.SlAntDisbTerm, disb.SlAntDisbDate, disb.SlActDisbAmt, disb.SlInitDisbDt));
                }
            }
            else if (awardCategory == TIVAwardCategory.Teach)
            {
                var teachData = tcAcyrs.FirstOrDefault(a => a.id == awardId);
                var teachDisb = dateAwards.FirstOrDefault(a => a.id == teachData.tcDateId);

                foreach (var id in teachDisb.tcDateIds)
                {
                    var disb = dateAwardDisbursements.FirstOrDefault(d => d.id == id);
                    disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.dawdAwardPeriod, disb.dawdDate, disb.dawdXmitAmount, disb.dawdInitialXmitDate));
                }

            }
            else if (awardCategory == TIVAwardCategory.Pell)
            {
                var pellData = pellAcyrs.FirstOrDefault(a => a.id == awardId);
                foreach (var disb in pellData.disbursements)
                {
                    disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.pellAntDisbTerm, disb.pellDisbDate, disb.pellActDisbAmt, disb.pellInitDisbDt));
                }
            }
            return Task.FromResult(disbInfo);
        }
    }
}
