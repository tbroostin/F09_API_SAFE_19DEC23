using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayrollDepositDirectivesRepository
    {
        #region test data

        public class EmployeeRecord
        {
            public string employeeId;
            public List<PayrollRecord> directives;
        }
        public class BankRecord
        {
            public string Code;
            public string Description;
            public string RoutingNumber;
            public string InstitutionNumber;
            public string BranchNumber;
            public string IsArchived;
            public List<PriorBankNameDateMap> priorNames;

        }
        public class PriorBankNameDateMap
        {
            public DateTime? endDate;
            public string name;
        }
        public List<BankRecord> BankRecords = new List<BankRecord>()
        {
            new BankRecord()
            {
                Code = "100",
                Description = "whatever",
                RoutingNumber = "091000019",
                priorNames = new List<PriorBankNameDateMap>()
                {
                    new PriorBankNameDateMap()
                    {
                        name = "old whatever naame",
                        endDate = new DateTime(2016, 07, 02)
                    }
                }
            },
            new BankRecord() 
            {
                Code = "0100",
                Description = "old whatever naame",
                RoutingNumber = "091000019",
                IsArchived = "Y",
                priorNames = new List<PriorBankNameDateMap>()
            },
            new BankRecord()
            {
                Code = "200",
                Description = "meh",
                InstitutionNumber = "123",
                BranchNumber = "57975",
                priorNames = new List<PriorBankNameDateMap>()
            }
        };
        public class PayrollRecord
        {
            public string RecordKey;
            public string EmployeeId;
            public string BankCode;
            public string Type;
            public int Priority;
            public DateTime StartDate;
            public DateTime? EndDate;
            public string ChangeFlag;
            public decimal? Amount;
            public string Nickname;
            public string Last4;
            public string AddOperator;
            public DateTime AddDate;
            public DateTime AddTime;
            public string ChangeOperator;
            public DateTime ChangeDate;
            public DateTime ChangeTime;
        }
        public List<EmployeeRecord> employeeRecords = new List<EmployeeRecord>()
        {
            new EmployeeRecord()
            {
                employeeId = "24601",
                directives = new List<PayrollRecord>()
                {
                    new PayrollRecord()
                    {
                        RecordKey = "001",
                        EmployeeId = "24601",
                        BankCode = "100",
                        Type = "D",
                        Priority = 1,
                        StartDate = new DateTime(2017,07,01),
                        EndDate = null,
                        ChangeFlag = "Y",
                        Amount = 4.45M,
                        Nickname = "dataencode",
                        Last4 = "HMMM",
                        AddOperator = "24601",
                        AddDate = new DateTime(2017,05,01),
                        AddTime = new DateTime(2017,05,01,02,03,04),
                        ChangeOperator = "24601",
                        ChangeDate = new DateTime(2017,05,01),
                        ChangeTime = new DateTime(2017,05,01,02,03,04)
                    },
                    new PayrollRecord()
                    {
                        RecordKey = "011",
                        EmployeeId = "24601",
                        BankCode = "0100",
                        Type = "D",
                        Priority = 1,
                        StartDate = new DateTime(2016,07,01),
                        EndDate = new DateTime(2016, 07, 02),
                        ChangeFlag = "Y",
                        Amount = 4.45M,
                        Nickname = "old bank",
                        Last4 = "HMMM",
                        AddOperator = "24601",
                        AddDate = new DateTime(2017,05,01),
                        AddTime = new DateTime(2017,05,01,02,03,04),
                        ChangeOperator = "24601",
                        ChangeDate = new DateTime(2017,05,01),
                        ChangeTime = new DateTime(2017,05,01,02,03,04)
                    },
                    new PayrollRecord()
                    {
                        RecordKey = "002",
                        EmployeeId = "24601",
                        BankCode = "200",
                        Type = "D",
                        Priority = 2,
                        StartDate = new DateTime(2017,07,01),
                        EndDate = null,
                        ChangeFlag = "Y",
                        Amount = 9.72M,
                        Nickname = "ATFSBTLO",
                        Last4 = "ENEW",
                        AddOperator = "24601",
                        AddDate = new DateTime(2017,05,01),
                        AddTime = new DateTime(2017,05,01,02,03,04),
                        ChangeOperator = "24601",
                        ChangeDate = new DateTime(2017,05,01),
                        ChangeTime = new DateTime(2017,05,01,02,03,04)
                    },
                    new PayrollRecord()
                    {
                        RecordKey = "003",
                        EmployeeId = "24601",
                        BankCode = "200",
                        Type = "D",
                        Priority = 999,
                        StartDate = new DateTime(2017,07,01),
                        EndDate = null,
                        ChangeFlag = "Y",
                        Amount = null,
                        Nickname = "thelast",
                        Last4 = "ADOM",
                        AddOperator = "24601",
                        AddDate = new DateTime(2017,05,01),
                        AddTime = new DateTime(2017,05,01,02,03,04),
                        ChangeOperator = "24601",
                        ChangeDate = new DateTime(2017,05,01),
                        ChangeTime = new DateTime(2017,05,01,02,03,04)
                    }
                }
            }        
        };
        #endregion

        public async Task<PayrollDepositDirectiveCollection> GetPayrollDepositDirectivesAsync(string employeeId)
        {
            var collection = new PayrollDepositDirectiveCollection(employeeId);
            var relevantRecords = employeeRecords.First(r => r.employeeId == employeeId);

            if (relevantRecords == null || !relevantRecords.directives.Any()) return null;

            foreach (var record in relevantRecords.directives)
            {
                PayrollDepositDirective directive;
                var bank = BankRecords.First(b => b.Code == record.BankCode);
                if(bank.RoutingNumber != null)
                {
                    directive = new PayrollDepositDirective(
                        record.RecordKey, 
                        record.EmployeeId, 
                        bank.RoutingNumber, 
                        bank.Description,
                        record.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings, 
                        record.Last4, 
                        record.Nickname,
                        record.ChangeFlag == "Y" ? true : false,
                        record.Priority,
                        record.Amount,
                        record.StartDate, 
                        record.EndDate,
                        new Timestamp(
                            record.AddOperator, 
                            record.AddDate.AddTicks(record.AddTime.Ticks), 
                            record.ChangeOperator,
                            record.ChangeDate.AddTicks(record.ChangeTime.Ticks)
                        )
                    );
                } 
                else
                {
                    directive = new PayrollDepositDirective(
                        record.RecordKey,
                        record.EmployeeId,
                        bank.InstitutionNumber,
                        bank.BranchNumber,
                        bank.Description,
                        record.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        record.Last4,
                        record.Nickname,
                        record.ChangeFlag == "Y" ? true : false,
                        record.Priority,
                        record.Amount,
                        record.StartDate,
                        record.EndDate,
                        new Timestamp(
                            record.AddOperator,
                            record.AddDate.AddTicks(record.AddTime.Ticks),
                            record.ChangeOperator,
                            record.ChangeDate.AddTicks(record.ChangeTime.Ticks)
                        )
                    );    
                }
                collection.Add(directive);
            }

            return await Task.FromResult(collection);
        }

        public async Task<PayrollDepositDirective> GetPayrollDepositDirectiveAsync(string id, string employeeId)
        {
            return (await GetPayrollDepositDirectivesAsync(employeeId)).FirstOrDefault(dir => dir.Id == id);
        }
    }
}
