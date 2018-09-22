/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.DataContracts;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayStatementRepository : IPayStatementRepository
    {
        // public static string TestPersonId = "12345";
        //public static string OtherTestPersonId = "54321";
        public class PayStatementRecord
        {
            public string recordKey;
            public DateTime? wpaDate;
            public string employeeId;
            public string adviceNumber;
            public string checkNumber;
            public string primaryDepartment;
            public string SSN;
            public DateTime? periodDate;
            public string wordPrintAmount;
            public decimal? numericPrintAmount;
            public decimal? grossPay;
            public decimal? totalTaxes;
            public decimal? totalBendeds;
            public decimal? netPay;
            public decimal? nonNetCheckAmount;
            public decimal? ytdGrossPay;
            public decimal? ytdTaxes;
            public decimal? ytdBenDeds;
            public decimal? ytdNetPay;
            public string dirDepDesc;
            public decimal? earnedVacation;
            public decimal? usedVacation;
            public decimal? vacationBalance;
            public decimal? otherLeaveEarned;
            public decimal? otherLeaveUsed;
            public decimal? otherLeaveBalance;
            //public List<EarningRecord> earnings;
            public List<BankDepositRecord> deposits;
            public List<string> mailingLabel;
            public string comments;
        }

        //public class EarningRecord
        //{
        //    public string type;
        //    public string description;
        //    public decimal? hours;
        //    public decimal? payment;
        //    public decimal? rate;
        //    public string hsFlag;
        //}

        public class BankDepositRecord
        {
            public string bankName;
            public string accountType;
            public string last4;
            public decimal? amount;
        }


        #region pay statement records
        public List<PayStatementRecord> payStatementRecords = new List<PayStatementRecord>();
        public List<PayStatementRecord> CreatePayStatementRecords(string personId)
        {
            payStatementRecords.AddRange(new List<PayStatementRecord>()
            {
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2017, 8, 1),
                    adviceNumber = "333",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2017, 7, 31),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = "D",
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "S",
                            last4 = "5678",
                            amount = 2000.00M
                        }
                    },
                    mailingLabel = new List<string>() { "Matt DeDiana", "111 1st st", "Reston VA, 55555" }
                },
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2017, 7, 1),
                    adviceNumber = "334",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2017, 6, 30),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = null,
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "X",
                            last4 = "5678",
                            amount = null
                        }
                    },
                    mailingLabel = new List<string>() { }
                },
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2017, 1, 1),
                    adviceNumber = "335",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2016, 12, 31),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = "D",
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "S",
                            last4 = "5678",
                            amount = 2000.00M
                        }
                    },
                    mailingLabel = new List<string>() {"Matt DeDiana", "5 5th St", "Apartment 5", "Herndon, VA 21029" }
                },
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2016, 12, 1),
                    adviceNumber = "336",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2016, 11, 30),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = "D",
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "S",
                            last4 = "5678",
                            amount = 2000.00M
                        }
                    },
                    mailingLabel = new List<string>() {"Matt DeDiana", "5 5th St", "Apartment 5", "Herndon, VA 21029" }
                },
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2016, 10, 1),
                    adviceNumber = "444",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2016, 9, 30),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = "D",
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "S",
                            last4 = "5678",
                            amount = 2000.00M
                        }
                    },
                    mailingLabel = new List<string>() {"Matt DeDiana", "5 5th St", "Apartment 5", "Herndon, VA 21029" }
                },
                new PayStatementRecord()
                {
                    recordKey = Guid.NewGuid().ToString(),
                    employeeId = personId,
                    wpaDate = new DateTime(2016, 10, 1),                   
                    adviceNumber = "444",
                    checkNumber = "111",
                    nonNetCheckAmount = 0,
                    numericPrintAmount = 22.38M,
                    wordPrintAmount = "One billion dollars",
                    dirDepDesc = "Bob's Bank",
                    earnedVacation = 0,
                    grossPay = 22.38M,
                    netPay = 22.38M,
                    otherLeaveBalance = 0,
                    otherLeaveEarned = 0,
                    otherLeaveUsed = 0,
                    periodDate = new DateTime(2016, 9, 30),
                    primaryDepartment = "Aeronautics",
                    SSN = "123457890",
                    totalBendeds = 0,
                    totalTaxes = 0,
                    usedVacation = 0,
                    vacationBalance = 0,
                    ytdBenDeds = 0,
                    ytdGrossPay = 1000.00M,
                    ytdNetPay = 1000.00M,
                    ytdTaxes = 0,
                    deposits = new List<BankDepositRecord>()
                    {
                        new BankDepositRecord
                        {
                            bankName = "doubledown",
                            accountType = "D",
                            last4 = "1234",
                            amount = 1000.00M
                        },
                       new BankDepositRecord
                        {
                            bankName = "FOO",
                            accountType = "S",
                            last4 = "5678",
                            amount = 2000.00M
                        }

                    },
                    mailingLabel = new List<string>() { }
                },

            });
            return payStatementRecords;
        }
        #endregion

        public async Task<PayStatementSourceData> GetPayStatementSourceDataAsync(string id)
        {
            var webPayAdvice = payStatementRecords.FirstOrDefault(rec => rec.recordKey == id);
            if (webPayAdvice == null)
            {
                throw new KeyNotFoundException();
            }
            return await Task.FromResult(BuildPayStatementSourceData(webPayAdvice));
        }

        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(string personId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await Task.FromResult(GetPayStatementSourceDataByPersonId(personId, startDate, endDate));
        }

        public IEnumerable<PayStatementSourceData> GetPayStatementSourceDataByPersonId(string personId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var records = CreatePayStatementRecords(personId);
            var payStatements = records.Select(r => BuildPayStatementSourceData(r));

            return payStatements;
        }

        public PayStatementSourceData BuildPayStatementSourceData(PayStatementRecord webPayAdvice)
        {

            var employeeName = webPayAdvice.mailingLabel.FirstOrDefault();
            var mailingLabel = webPayAdvice.mailingLabel.Skip(1).Select(s => new PayStatementAddress(s));
            var statementToAdd = new PayStatementSourceData(
                    webPayAdvice.recordKey,
                    webPayAdvice.employeeId,
                    employeeName,
                    webPayAdvice.SSN,
                    mailingLabel,
                    webPayAdvice.checkNumber,
                    webPayAdvice.adviceNumber,
                    webPayAdvice.wpaDate.Value,
                    webPayAdvice.periodDate.Value,
                    webPayAdvice.grossPay.Value,
                    webPayAdvice.netPay.Value,
                    webPayAdvice.ytdGrossPay.Value,
                    webPayAdvice.ytdNetPay.Value,
                    webPayAdvice.comments
                );

            foreach (var deposit in webPayAdvice.deposits)
            {
                var depositToAdd = new PayStatementSourceBankDeposit(
                    deposit.bankName,
                    Base.Entities.BankAccountType.Checking,
                    deposit.last4,
                    deposit.amount
                    );
                statementToAdd.SourceBankDeposits.Add(depositToAdd);
            }

            return statementToAdd;
        }

        public IEnumerable<PayStatementSourceData> GetPayStatementSourceData(IEnumerable<string> ids)
        {
            return payStatementRecords
                .Where(rec => ids.Contains(rec.recordKey))
                .Select(r => BuildPayStatementSourceData(r));
        }
        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataAsync(IEnumerable<string> ids)
        {
            return await Task.FromResult(GetPayStatementSourceData(ids));
        }

        public IEnumerable<PayStatementSourceData> GetPayStatementSourceDataByPersonId(IEnumerable<string> personIds)
        {
            var sourceData = personIds.SelectMany(id =>
                {
                    payStatementRecords = CreatePayStatementRecords(id);
                    return payStatementRecords.Select(r => BuildPayStatementSourceData(r)).ToList();
                });
            return sourceData;
        }

        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(IEnumerable<string> personIds, DateTime? startDate, DateTime? endDate)
        {
            return await Task.FromResult(GetPayStatementSourceDataByPersonId(personIds));
        }
    }
}

