using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollDepositDirectiveTests : TestPayrollDepositDirectivesRepository
    {
        string employeeId;
        PayrollDepositDirective directiveToTest;
        public PayrollRecord usData;
        public PayrollRecord caData;
        public BankRecord usBank;
        public BankRecord caBank;

        public async Task ReallyInitialize()
        {
            employeeId = "24601";
            directiveToTest = (await base.GetPayrollDepositDirectivesAsync(employeeId)).FirstOrDefault();
        }

        [TestInitialize]
        public void Initialize()
        {
            if(!ReallyInitialize().IsCompleted)
                ReallyInitialize().RunSynchronously();

            usBank = base.BankRecords.FirstOrDefault(b => string.IsNullOrWhiteSpace(b.InstitutionNumber));
            usData = base.employeeRecords.First().directives.FirstOrDefault(r => usBank.Code == r.BankCode);
            
            caBank = base.BankRecords.FirstOrDefault(b => string.IsNullOrWhiteSpace(b.RoutingNumber));
            caData = base.employeeRecords.First().directives.FirstOrDefault(r => caBank.Code == r.BankCode);            
        }

        [TestMethod]
        public void USAccountIsCreatedTest()
        {
            var usEntity = new PayrollDepositDirective(
                        usData.RecordKey,
                        usData.EmployeeId,
                        usBank.RoutingNumber,
                        usBank.Description,
                        usData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        usData.Last4,
                        usData.Nickname,
                        usData.ChangeFlag == "Y" ? true : false,
                        usData.Priority,
                        usData.Amount,
                        usData.StartDate,
                        usData.EndDate,
                        new Timestamp(
                            usData.AddOperator,
                            usData.AddDate.AddTicks(usData.AddTime.Ticks),
                            usData.ChangeOperator,
                            usData.ChangeDate.AddTicks(usData.ChangeTime.Ticks)
                        )
                    );
            Assert.IsNotNull(usEntity);
        }

        [TestMethod]
        public void CAAccountIsCreatedTest()
        {
            var caEntity = new PayrollDepositDirective(
                        caData.RecordKey,
                        caData.EmployeeId,
                        caBank.InstitutionNumber,
                        caBank.BranchNumber,
                        caBank.Description,
                        caData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        caData.Last4,
                        caData.Nickname,
                        caData.ChangeFlag == "Y" ? true : false,
                        caData.Priority,
                        caData.Amount,
                        caData.StartDate,
                        caData.EndDate,
                        new Timestamp(
                            caData.AddOperator,
                            caData.AddDate.AddTicks(caData.AddTime.Ticks),
                            caData.ChangeOperator,
                            caData.ChangeDate.AddTicks(caData.ChangeTime.Ticks)
                        )
                    );
            Assert.IsNotNull(caEntity);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void PersonIdIsRequiredTest()
        {
            new PayrollDepositDirective(
                        usData.RecordKey,
                        null,
                        usBank.RoutingNumber,
                        usBank.Description,
                        usData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        usData.Last4,
                        usData.Nickname,
                        usData.ChangeFlag == "Y" ? true : false,
                        usData.Priority,
                        usData.Amount,
                        usData.StartDate,
                        usData.EndDate,
                        new Timestamp(
                            usData.AddOperator,
                            usData.AddDate.AddTicks(usData.AddTime.Ticks),
                            usData.ChangeOperator,
                            usData.ChangeDate.AddTicks(usData.ChangeTime.Ticks)
                        )
                    );
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void RoutingIdFailsValiditationTest()
        {
            new PayrollDepositDirective(
                        usData.RecordKey,
                        usData.EmployeeId,
                        "091OOOO19",
                        usBank.Description,
                        usData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        usData.Last4,
                        usData.Nickname,
                        usData.ChangeFlag == "Y" ? true : false,
                        usData.Priority,
                        usData.Amount,
                        usData.StartDate,
                        usData.EndDate,
                        new Timestamp(
                            usData.AddOperator,
                            usData.AddDate.AddTicks(usData.AddTime.Ticks),
                            usData.ChangeOperator,
                            usData.ChangeDate.AddTicks(usData.ChangeTime.Ticks)
                        )
                    );
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void RoutingIdFailsChecksumTest()
        {
            new PayrollDepositDirective(
                        usData.RecordKey,
                        usData.EmployeeId,
                        "091000020",
                        usBank.Description,
                        usData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        usData.Last4,
                        usData.Nickname,
                        usData.ChangeFlag == "Y" ? true : false,
                        usData.Priority,
                        usData.Amount,
                        usData.StartDate,
                        usData.EndDate,
                        new Timestamp(
                            usData.AddOperator,
                            usData.AddDate.AddTicks(usData.AddTime.Ticks),
                            usData.ChangeOperator,
                            usData.ChangeDate.AddTicks(usData.ChangeTime.Ticks)
                        )
                    );
        }
        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InstitutionIdFailsValidationTest()
        {
            new PayrollDepositDirective(
                        caData.RecordKey,
                        caData.EmployeeId,
                        "4444",
                        caBank.BranchNumber,
                        caBank.Description,
                        caData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        caData.Last4,
                        caData.Nickname,
                        caData.ChangeFlag == "Y" ? true : false,
                        caData.Priority,
                        caData.Amount,
                        caData.StartDate,
                        caData.EndDate,
                        new Timestamp(
                            caData.AddOperator,
                            caData.AddDate.AddTicks(caData.AddTime.Ticks),
                            caData.ChangeOperator,
                            caData.ChangeDate.AddTicks(caData.ChangeTime.Ticks)
                        )
                    );
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BranchNumberFailsValidationTest()
        {
            new PayrollDepositDirective(
                        caData.RecordKey,
                        caData.EmployeeId,
                        caBank.InstitutionNumber,
                        "4444",
                        caBank.Description,
                        caData.Type == "D" ? BankAccountType.Checking : BankAccountType.Savings,
                        caData.Last4,
                        caData.Nickname,
                        caData.ChangeFlag == "Y" ? true : false,
                        caData.Priority,
                        caData.Amount,
                        caData.StartDate,
                        caData.EndDate,
                        new Timestamp(
                            caData.AddOperator,
                            caData.AddDate.AddTicks(caData.AddTime.Ticks),
                            caData.ChangeOperator,
                            caData.ChangeDate.AddTicks(caData.ChangeTime.Ticks)
                        )
                    );
        }

        [TestMethod]
        public void AccountIdCanBeSetTest()
        {
            Initialize();
            var someAccountId = "adgkskfds78sa745r";
            directiveToTest.SetNewAccountId(someAccountId);
            Assert.AreEqual(someAccountId, directiveToTest.NewAccountId);
        }
        [TestMethod,ExpectedException(typeof(ArgumentNullException))]        
        public void NullAccountIdSetterErrorTest()
        {
            Initialize();
            var someAccountId = string.Empty;
            directiveToTest.SetNewAccountId(someAccountId);            
        }


    }
}
