//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentLoanSummaryRepository : IStudentLoanSummaryRepository
    {
        //Arrays of acceptable loan types for direct and plus loans (when looking through NSLDS records for interview dates)
        private readonly string[] directLoanTypes = new string[14] { "CS", "CU", "D0", "D1", "D2", "D5", "D6", "D8", "D9", "RF", "SF", "SL", "SN", "SU" };
        private readonly string[] plusLoanTypes = new string[2] { "GB", "D3" };

        public class FaSysParameters
        {
            public string FspUseFaInterviewsOnly;
        }

        public FaSysParameters FaSysParams = new FaSysParameters()
        {
            FspUseFaInterviewsOnly = "Y"
        };

        #region FaStudentData
        public class FaStudentRecord
        {
            public string CodPersonId;
            public List<string> FaInterviewIds;
            public List<string> FaIsirNsldsIds;
            public List<string> FaSaYears;
        }

        public FaStudentRecord FaStudent = new FaStudentRecord()
        {
            CodPersonId = "1",
            FaInterviewIds = new List<string>() { "1", "2", "3", "4" },
            FaIsirNsldsIds = new List<string>() { "1", "2" },
            FaSaYears = new List<string>() { "2020"}
        };

        #endregion

        #region IsirNsldsData
        public class IsirNsldsRecord
        {
            public string id;
            public List<string> IsirNsldsLoanIds;
            public int? combinedTotal;
            public int? combinedPrinBal;
        }

        public List<IsirNsldsRecord> isirNsldsData = new List<IsirNsldsRecord>()
        {
            new IsirNsldsRecord() {id = "1", IsirNsldsLoanIds = new List<string>() {"1", "2"}, combinedTotal = 23000, combinedPrinBal = 21000},
            new IsirNsldsRecord() {id = "2", IsirNsldsLoanIds = new List<string>() {"3", "4"}, combinedTotal = 35000, combinedPrinBal = 30000}
        };

        public class IsirNsldsLoanRecord
        {
            public string id;
            public string schoolCode;
            public int? aggregatePrincipalBalance;
            public string programCode;
            public DateTime? currentStatusDate;
        }

        public List<IsirNsldsLoanRecord> isirNsldsLoanData = new List<IsirNsldsLoanRecord>()
        {
            new IsirNsldsLoanRecord() {id = "1", schoolCode = "11112222", aggregatePrincipalBalance = 500, programCode = "D1", currentStatusDate = new DateTime(1977, 08, 15)},
            new IsirNsldsLoanRecord() {id = "2", schoolCode = "22229999", aggregatePrincipalBalance = 600, programCode = "D3", currentStatusDate = new DateTime(1982, 08, 15)},
            new IsirNsldsLoanRecord() {id = "3", schoolCode = "11112222", aggregatePrincipalBalance = 400, programCode = "DU", currentStatusDate = new DateTime(1983, 03, 12)},
            new IsirNsldsLoanRecord() {id = "4", schoolCode = "99998888", aggregatePrincipalBalance = 1000, programCode = "GB", currentStatusDate = new DateTime(1984, 05, 21)}
        };
        #endregion

        #region InterviewData

        public class InterviewRecord
        {
            public string Id;
            public string InterviewCode;
            public DateTime? CompleteDate;
        }

        public List<InterviewRecord> StudentInterviews = new List<InterviewRecord>()
        {
            new InterviewRecord()
            {
                Id = "1",
                InterviewCode = "SUB",
                CompleteDate = new DateTime(2010, 01, 01)
            },
            new InterviewRecord()
            {
                Id = "2",
                InterviewCode = "UNSUB",
                CompleteDate = new DateTime(2012, 01, 01)
            },
            new InterviewRecord()
            {
                Id = "3",
                InterviewCode = "PLUS",
                CompleteDate = new DateTime(2012, 04, 19)
            },
            new InterviewRecord()
            {
                Id = "4",
                InterviewCode = "GPLUS",
                CompleteDate = new DateTime(2012, 04, 18)
            }
        };

        public class InterviewCodeRecord
        {
            public string id;
            public string categoryCode;
        }

        public List<InterviewCodeRecord> interviewCodeData = new List<InterviewCodeRecord>()
        {
            new InterviewCodeRecord() {id = "SUB", categoryCode = "S"},
            new InterviewCodeRecord() {id = "UNSUB", categoryCode = "S"},
            new InterviewCodeRecord() {id = "PLUS", categoryCode = "P"},
            new InterviewCodeRecord() {id = "GPLUS", categoryCode = "P"},
            new InterviewCodeRecord() {id = "TEACH", categoryCode = "T"}
        };

        #endregion

        #region MpnData
        public class MpnRecord
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string CreateDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public string CodmNonstuCodPersonId { get; set; }
            public string CodmStuCodPersonId { get; set; }
        }

        public List<MpnRecord> StudentMpns = new List<MpnRecord>()
        {
            //Sub/Unsub MPN (letter M in the 10th char of Id)
            new MpnRecord()
            {
                Id = "123456789M12G99999001",
                Status = "A",
                CreateDate = "2014010112000000",
                ExpirationDate = new DateTime(2024, 01, 01)
            },
            new MpnRecord()
            {
                Id = "123456789M12G99999001",
                Status = "A",
                CreateDate = "2013010112000000",
                ExpirationDate = new DateTime(2023, 01, 01)
            },
            //Plus MPN (letter N in the 10th char of Id
            new MpnRecord()
            {
                Id = "123456789N13G99999001",
                Status = "A",
                CreateDate = "2012010112010101",
                ExpirationDate = new DateTime(2022, 01, 01)
            },
            new MpnRecord()
            {
                Id = "123456789N13G99999001",
                Status = "A",
                CreateDate = "2011010112010101",
                ExpirationDate = new DateTime(2023, 01, 01)
            },
                       new MpnRecord()
            {
                Id = "123456789N14G99999001",
                Status = "E",
                CreateDate = "2011010112010101",
                ExpirationDate = new DateTime(2023, 01, 01)
            }
        };

        #endregion

        #region CodePerson data

        public class CodPersonRecord{
            public string recordKey;
            public List<string> codMpnIds;
        }

        public List<CodPersonRecord> codRecords = new List<CodPersonRecord>()
        {
            new CodPersonRecord(){
                recordKey = "1",
                codMpnIds = new List<string>() { "123456789M12G99999001", "123456789N13G99999001" }
            },
            new CodPersonRecord(){
                recordKey = "45",
                codMpnIds = new List<string>() { "123456789M12G99999999", "123456789N13G99994567" }
            }
            
        };
        #endregion


        public Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId)
        {
            var studentLoanSummary = new StudentLoanSummary(studentId);

            var selectedStudentInterviews = StudentInterviews.Where(i => FaStudent.FaInterviewIds.Contains(i.Id));
            var subInterviewCodes = interviewCodeData.Where(code => code.categoryCode == "S").Select(code => code.id);
            var subInterviews = selectedStudentInterviews.Where(i => subInterviewCodes.Contains(i.InterviewCode) && i.CompleteDate.HasValue);
            if (subInterviews.Count() > 0)
            {
                subInterviews = subInterviews.OrderByDescending(i => i.CompleteDate.Value);
                var subCode = subInterviewCodes.First();
                studentLoanSummary.DirectLoanEntranceInterviewDate = subInterviews.First().CompleteDate;
            }

            var isirNsldsRecords = GetNsldsRecords(FaStudent);

            var UseFaInterviewsOnly = FaSysParams.FspUseFaInterviewsOnly;

            // If we don't have Interview records look to NSLDS records for the loan type
            if (UseFaInterviewsOnly != "Y")
            {
                if (studentLoanSummary.DirectLoanEntranceInterviewDate == null)
                {
                    if (isirNsldsRecords != null && isirNsldsRecords.Count() > 0)
                    {
                        studentLoanSummary.DirectLoanEntranceInterviewDate = GetNsldsDate(isirNsldsRecords, subInterviewCodes.First());
                    }
                }
            }

           
            var plusInterviewCodes = interviewCodeData.Where(code => code.categoryCode == "P").Select(code => code.id);
            var plusInterviews = selectedStudentInterviews.Where(i => plusInterviewCodes.Contains(i.InterviewCode) && i.CompleteDate.HasValue);
            if (plusInterviews.Count() > 0)
            {
                plusInterviews = plusInterviews.OrderByDescending(i => i.CompleteDate.Value);
                studentLoanSummary.GraduatePlusLoanEntranceInterviewDate = plusInterviews.First().CompleteDate;
            }


            // If we don't have Interview records look to NSLDS records for the loan type
            if (UseFaInterviewsOnly != "Y")
            {
                if (studentLoanSummary.GraduatePlusLoanEntranceInterviewDate == null)
                {
                    if (isirNsldsRecords != null && isirNsldsRecords.Count() > 0)
                    {
                        studentLoanSummary.GraduatePlusLoanEntranceInterviewDate = GetNsldsDate(isirNsldsRecords, plusInterviewCodes.First());
                    }
                }
            }

            //Get Mpn expiration dates
            if (!string.IsNullOrEmpty(FaStudent.CodPersonId))
            {
                var codPersonRecord = codRecords.FirstOrDefault(r => r.recordKey == FaStudent.CodPersonId);
                if (codPersonRecord != null && codPersonRecord.codMpnIds != null && codPersonRecord.codMpnIds.Any())
                {
                    var studentMpnRecords = StudentMpns.Where(m => codPersonRecord.codMpnIds.Contains(m.Id));
                    if (studentMpnRecords != null && studentMpnRecords.Count() > 0)
                    {
                        studentLoanSummary.DirectLoanMpnExpirationDate = GetActiveMpnExpirationDate(studentId, studentMpnRecords, "S");
                        studentLoanSummary.PlusLoanMpnExpirationDate = GetActiveMpnExpirationDate(studentId, studentMpnRecords, "P");
                    }
                }
            }

            var informedBorrowerItems = new List<InformedBorrowerItem>();
            var informedBorrowerItem = new InformedBorrowerItem();
            informedBorrowerItem.FaYear = "2020";
            informedBorrowerItem.IsInformedBorrowerComplete = true;
            informedBorrowerItems.Add(informedBorrowerItem);
            studentLoanSummary.InformedBorrowerItem = informedBorrowerItems;

            if (FaStudent.FaIsirNsldsIds != null && FaStudent.FaIsirNsldsIds.Count() > 0)
            {
                var isirNsldsRecord = isirNsldsData.FirstOrDefault(i => i.id == FaStudent.FaIsirNsldsIds.First());
                var isirNsldsLoanRecords = isirNsldsLoanData.Where(ld => isirNsldsRecord.IsirNsldsLoanIds.Contains(ld.id));

                foreach (var loanRecord in isirNsldsLoanRecords)
                {
                    if (!string.IsNullOrEmpty(loanRecord.schoolCode) && loanRecord.aggregatePrincipalBalance.HasValue)
                    {
                        try
                        {
                            studentLoanSummary.AddOrUpdateLoanHistory(loanRecord.schoolCode, loanRecord.aggregatePrincipalBalance.Value);
                        }
                        catch (Exception) { }
                    }
                }
            }

            return Task.FromResult(studentLoanSummary);
        }

        private DateTime? GetActiveMpnExpirationDate(string studentId, IEnumerable<MpnRecord> studentMpnRecords, string mpnTypeCode)
        {
            //Select Mpns that match the given mpnTypeCode (the 10th character in the RecordKey indiciates the type - M=SUB/UNSUB, N=PLUS)
            //and that are active
            //and that have non-null/empty date/time stamps

            //This next logic mimics the logic of the Colleague computed column S.FA.CC.POTENTIAL.MPN.STATUS
            // Check for an Active Status first, then Pending, then use the Old Active Status

            //Find the MPNs of the correct Loan Type and are Active
            var typeSpecificActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Id.Length >= 10 &&
                    m.Id.Substring(9, 1) == mpnTypeCode &&
                    m.Status == "A" &&
                    m.ExpirationDate.HasValue
                );
            if (typeSpecificActiveMpns != null && typeSpecificActiveMpns.Count() > 0)
            {
                typeSpecificActiveMpns = typeSpecificActiveMpns.OrderByDescending(m => m.ExpirationDate);
                var mpnToUse = typeSpecificActiveMpns.FirstOrDefault();
                if (mpnToUse != null && mpnToUse.ExpirationDate >= DateTime.Today)
                {
                    return mpnToUse.ExpirationDate;
                }
            }

            //If this is a PLUS or GPLUS and there is an Endorser status use this

            //Find the MPNs of the correct Loan Type and are Endorser
            var typeSpecificEndorserMpns = studentMpnRecords.Where(
                m =>
                    m.Id.Length >= 10 &&
                    m.Id.Substring(9, 1) == mpnTypeCode &&
                    m.Status == "E" &&
                    m.ExpirationDate.HasValue
                );

            if (mpnTypeCode == "N" && typeSpecificEndorserMpns != null && typeSpecificEndorserMpns.Count() > 0)
            {
                typeSpecificEndorserMpns = typeSpecificEndorserMpns.OrderByDescending(m => m.ExpirationDate);
                var mpnToUse = typeSpecificEndorserMpns.FirstOrDefault();
                if (mpnToUse != null && mpnToUse.ExpirationDate >= DateTime.Today)
                {
                    return mpnToUse.ExpirationDate;
                }
            }


            //Find the MPNs of the correct Loan Type and are Pending
            var typeSpecificPendingMpns = studentMpnRecords.Where(
                m =>
                    m.Id.Length >= 10 &&
                    m.Id.Substring(9, 1) == mpnTypeCode &&
                    m.Status == "P" &&
                    m.ExpirationDate.HasValue

                );
            if (typeSpecificPendingMpns != null && typeSpecificPendingMpns.Count() > 0)
            {
                typeSpecificPendingMpns = typeSpecificPendingMpns.OrderByDescending(m => m.ExpirationDate);
                var mpnToUse = typeSpecificPendingMpns.FirstOrDefault();
                if (mpnToUse != null && mpnToUse.ExpirationDate >= DateTime.Today)
                {
                    return mpnToUse.ExpirationDate;
                }
            }

            //Find the MPNs of the correct Loan Type and are Old Active Status
            var typeSpecificOldActiveMpns = studentMpnRecords.Where(
                m =>
                    m.Id.Length >= 10 &&
                    m.Id.Substring(9, 1) == mpnTypeCode &&
                    m.Status == "X" &&
                    m.ExpirationDate.HasValue

                );
            if (typeSpecificOldActiveMpns != null && typeSpecificOldActiveMpns.Count() > 0)
            {
                typeSpecificOldActiveMpns = typeSpecificOldActiveMpns.OrderByDescending(m => m.ExpirationDate);
                var mpnToUse = typeSpecificOldActiveMpns.FirstOrDefault();
                if (mpnToUse != null && mpnToUse.ExpirationDate >= DateTime.Today)
                {
                    return mpnToUse.ExpirationDate;
                }
            }

            //If none return null
            return null;
        }        

        /// <summary>
        /// Gets the Isir Nslds records from the list in Fin.Aid.
        /// </summary>
        /// <param name="FaStudent"></param>
        /// <returns>List of ISIR Nslds records.</returns>
        private List<IsirNsldsRecord> GetNsldsRecords(FaStudentRecord FaStudent)
        {
            if (FaStudent.FaIsirNsldsIds != null && FaStudent.FaIsirNsldsIds.Count() > 0)
            {
                
                var selectedIsirNsldsRecords = isirNsldsData.Where(i => FaStudent.FaIsirNsldsIds.Contains(i.id));

                return selectedIsirNsldsRecords.ToList();
            }
            else
            {
                return new List<IsirNsldsRecord>();
            }
        }


        private DateTime? GetNsldsDate(List<IsirNsldsRecord> allIsirNsldsRecords, string loanType )
        {
            //var selectedIsirNsldsRecord = allIsirNsldsRecords.FirstOrDefault();
            foreach (var singleIsirNsldsRecord in allIsirNsldsRecords)
            {
                if (singleIsirNsldsRecord.IsirNsldsLoanIds != null)
                {
                    var selectedIsirNsldsLoanRecords = isirNsldsLoanData.Where(inlr => singleIsirNsldsRecord.IsirNsldsLoanIds.Contains(inlr.id));

                    foreach (var singleLoanRecord in selectedIsirNsldsLoanRecords)
                    {
                        var typeOfLoan = singleLoanRecord.programCode;
                        if (!string.IsNullOrEmpty(typeOfLoan))
                        {
                            if ((loanType == "S" && directLoanTypes.Contains(typeOfLoan.ToUpper()))
                                    || (loanType == "P" && plusLoanTypes.Contains(typeOfLoan.ToUpper())))
                            {
                                return singleLoanRecord.currentStatusDate;
                            }     
                        }
                    }
                }
            }
            return null;
            
        }

    }
}
