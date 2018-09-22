using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Domain.Base.Tests
{


    public class TestPayableDepositDirectiveRepository : IPayableDepositDirectiveRepository
    {
        public string colleagueTimeZone = TimeZoneInfo.Local.Id;

        public class PayableDepositDirectiveRecord
        {
            public string id;
            public string personId;
            public string addressId;
            public string routingId;
            public string institutionId;
            public string branchNumber;
            public string eCheckFlag;
            public DateTime? startDate;
            public string isVerified;
            public string accountType;
            public DateTime? addDate;
            public DateTime? addTime;
            public string addOperator;
            public DateTime? changeDate;
            public DateTime? changeTime;
            public string changeOperator;
            public string nickname;
            public string accountIdLastFour;
        }

        public List<PayableDepositDirectiveRecord> payableDepositDirectiveRecords = new List<PayableDepositDirectiveRecord>()
        {
            new PayableDepositDirectiveRecord()
            {
                id = "500",
                personId = "0003914",
                addressId = "",
                routingId = "051009364",
                eCheckFlag = "Y",
                startDate = new DateTime(2017, 1, 1),
                isVerified = "Y",
                accountType = "C",
                addDate = new DateTime(2016, 12, 31),
                addTime = new DateTime(2016, 12, 31, 13, 1, 0),
                addOperator = "0003914",
                changeDate = new DateTime(2016, 12, 31),
                changeTime = new DateTime(2016, 12, 31, 13, 1, 0),
                changeOperator = "0003914",
                nickname = "My Bank",
                accountIdLastFour = "1234"
            },
            new PayableDepositDirectiveRecord()
            {
                id = "501",
                personId = "0003914",
                addressId = "",
                routingId = "051009364",
                eCheckFlag = "Y",
                startDate = new DateTime(2018, 1, 1),
                isVerified = "Y",
                accountType = "S",
                addDate = new DateTime(2016, 12, 31),
                addTime = new DateTime(2016, 12, 31, 13, 1, 0),
                addOperator = "0003914",
                changeDate = new DateTime(2016, 12, 31),
                changeTime = new DateTime(2016, 12, 31, 13, 1, 0),
                changeOperator = "0003914",
                nickname = "My Bank",
                accountIdLastFour = "4321"
            },
            new PayableDepositDirectiveRecord()
            {
                id = "502",
                personId = "0003914",
                addressId = "",
                institutionId = "123",
                branchNumber = "12345",
                eCheckFlag = "Y",
                startDate = new DateTime(2018, 6, 1),
                isVerified = "Y",
                accountType = "C",
                addDate = new DateTime(2016, 12, 31),
                addTime = new DateTime(2016, 12, 31, 13, 1, 0),
                addOperator = "0003914",
                changeDate = new DateTime(2016, 12, 31),
                changeTime = new DateTime(2016, 12, 31, 13, 1, 0),
                changeOperator = "0003914",
                nickname = "My Canadian Bank",
                accountIdLastFour = "4444"
            }

        };

        public async Task<PayableDepositDirectiveCollection> GetPayableDepositDirectivesAsync(string payeeId, string payableDepositDirectiveId = "")
        {

            var collection = new PayableDepositDirectiveCollection(payeeId);
            
            payableDepositDirectiveRecords
                .Where(record => string.IsNullOrEmpty(payableDepositDirectiveId) ? true : record.id == payableDepositDirectiveId)
                .ToList()
                .ForEach(record => {
                PayableDepositDirective directive;
                if (string.IsNullOrEmpty(record.routingId)) {
                    directive = new PayableDepositDirective(
                        record.id,
                        payeeId,
                        record.institutionId,
                        record.branchNumber,
                        null,
                        convertRecordColumnToBankAccountType(record.accountType),
                        record.accountIdLastFour,
                        record.nickname,
                        record.isVerified == "Y",
                        record.addressId,
                        record.startDate.Value,
                        null,
                        record.eCheckFlag == "Y",
                        new Timestamp(
                            record.addOperator,
                            record.addTime.ToPointInTimeDateTimeOffset(record.addDate, colleagueTimeZone).Value,
                            record.changeOperator,
                            record.changeTime.ToPointInTimeDateTimeOffset(record.changeTime, colleagueTimeZone).Value));
                } 
                else 
                {
                    directive = new PayableDepositDirective(
                        record.id,
                        payeeId,
                        record.routingId,
                        null,
                        convertRecordColumnToBankAccountType(record.accountType),
                        record.accountIdLastFour,
                        record.nickname,
                        record.isVerified == "Y",
                        record.addressId,
                        record.startDate.Value,
                        null,
                        record.eCheckFlag == "Y",
                        new Timestamp(
                            record.addOperator,
                            record.addTime.ToPointInTimeDateTimeOffset(record.addDate, colleagueTimeZone).Value,
                            record.changeOperator,
                            record.changeTime.ToPointInTimeDateTimeOffset(record.changeTime, colleagueTimeZone).Value));
                }
                    
                collection.Add(directive);
            });

            return await Task.FromResult(collection);            
        }

        public async Task<PayableDepositDirective> CreatePayableDepositDirectiveAsync(PayableDepositDirective newPayableDepositDirective)
        {
            var random = new Random();
            var id = random.Next(int.MaxValue).ToString();
            payableDepositDirectiveRecords.Add(new PayableDepositDirectiveRecord()
            {
                id = id,
                accountIdLastFour = newPayableDepositDirective.AccountIdLastFour,
                accountType = convertBankAccountTypeToRecordColumn(newPayableDepositDirective.BankAccountType),
                addDate = newPayableDepositDirective.Timestamp.AddDateTime.Date,
                addTime = newPayableDepositDirective.Timestamp.AddDateTime.DateTime,
                addOperator = newPayableDepositDirective.Timestamp.AddOperator,
                addressId = newPayableDepositDirective.AddressId,
                branchNumber = newPayableDepositDirective.BranchNumber,
                changeDate = newPayableDepositDirective.Timestamp.ChangeDateTime.Date,
                changeTime = newPayableDepositDirective.Timestamp.ChangeDateTime.DateTime,
                changeOperator = newPayableDepositDirective.Timestamp.ChangeOperator,
                eCheckFlag = newPayableDepositDirective.IsElectronicPaymentRequested ? "Y" : "N",
                institutionId = newPayableDepositDirective.InstitutionId,
                isVerified = newPayableDepositDirective.IsVerified ? "Y" : "N",
                personId = newPayableDepositDirective.PayeeId,
                nickname = newPayableDepositDirective.Nickname,
                routingId = newPayableDepositDirective.RoutingId,
                startDate = newPayableDepositDirective.StartDate
            });

            return (await GetPayableDepositDirectivesAsync(newPayableDepositDirective.PayeeId, id))[0];

        }

        public async Task<PayableDepositDirective> UpdatePayableDepositDirectiveAsync(PayableDepositDirective inputPayableDepositDirective)
        {
            await DeletePayableDepositDirectiveAsync(inputPayableDepositDirective.Id);
            var temp = await CreatePayableDepositDirectiveAsync(inputPayableDepositDirective);
            var index = payableDepositDirectiveRecords.Find(r => r.id == temp.Id);
            index.id = inputPayableDepositDirective.Id;

            return (await GetPayableDepositDirectivesAsync(inputPayableDepositDirective.PayeeId, inputPayableDepositDirective.Id))[0];
        }

        public async Task DeletePayableDepositDirectiveAsync(string payableDepositDirectiveId)
        {
            var index = payableDepositDirectiveRecords.FindIndex(r => r.id == payableDepositDirectiveId);
            payableDepositDirectiveRecords.RemoveAt(index);
            await Task.FromResult(1);
            return;
        }

        public async Task<BankingAuthenticationToken> AuthenticatePayableDepositDirectiveAsync(string payeeId, string payableDepositDirectiveId, string accountId, string addressId)
        {
            return await Task.FromResult(new BankingAuthenticationToken(DateTimeOffset.Now.AddMinutes(10), Guid.NewGuid()));
        }


        /// <summary>
        /// Helper to convert the record data to BankAccountType enum
        /// </summary>
        /// <param name="accountTypeCode"></param>
        /// <returns></returns>
        public BankAccountType convertRecordColumnToBankAccountType(string accountTypeCode)
        {
            if (string.IsNullOrEmpty(accountTypeCode))
            {
                throw new ArgumentNullException("accountTypeCode", "Cannot convert null or empty accountTypeCode");
            }

            switch (accountTypeCode.ToUpperInvariant())
            {
                case "S":
                    return BankAccountType.Savings;
                case "C":
                    return BankAccountType.Checking;
                default:
                    throw new ApplicationException("Unknown accountTypeCode " + accountTypeCode);
            }

        }

        /// <summary>
        /// Helper to convert the BankAccountType enum to record data
        /// </summary>
        /// <param name="bankAccountType"></param>
        /// <returns></returns>
        public string convertBankAccountTypeToRecordColumn(BankAccountType bankAccountType)
        {
            switch (bankAccountType)
            {
                case BankAccountType.Savings:
                    return "S";
                case BankAccountType.Checking:
                    return "C";
                default:
                    throw new ApplicationException("Unknown bankAccountType " + bankAccountType);
            }

        }
    }
}
