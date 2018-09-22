using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayrollDepositDirectiveRepositoryTests : BaseRepositorySetup
    {

        public PayrollDepositDirectivesRepository repositoryUnderTest;
        public TestPayrollDepositDirectivesRepository testData;
        public Mock<BaseCachingRepository> cacheRepoMock;

        public UpdatePayrollDepositsRequest updateRequest;
        public UpdatePayrollDepositsResponse updateResponse;
        public CreatePayrollDepositRequest createRequest;
        public CreatePayrollDepositResponse createResponse;
        public DeletePayrollDepositsRequest deleteRequest;
        public DeletePayrollDepositsResponse deleteResponse;

        public void PayrollDepositDirectiveRepositoryTestsInitialize()
        {
            testData = new TestPayrollDepositDirectivesRepository();
            MockInitialize();
            repositoryUnderTest = new PayrollDepositDirectivesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            #region get mocks

            dataReaderMock.Setup(r => r.SelectAsync("PR.DEPOSIT.CODES", It.IsAny<string>()))
                .Returns<string,string>((file,query) => Task.FromResult(testData.BankRecords.Select(r => r.Code).ToArray()));    


            dataReaderMock.Setup(d => d.BulkReadRecordAsync<PrDepositCodes>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((x, y) =>
                    Task.FromResult(
                    new Collection<PrDepositCodes>(testData.BankRecords.Select(bankRecord =>
                        new PrDepositCodes()
                        {
                            Recordkey = bankRecord.Code,
                            DdcDescription = bankRecord.Description,
                            DdcTransitNo = bankRecord.RoutingNumber,
                            DdcFinInstNumber = bankRecord.InstitutionNumber,
                            DdcBrTransitNumber = bankRecord.BranchNumber,
                            DdcPriorDescsEntityAssociation = bankRecord.priorNames.Select(n => 
                                new PrDepositCodesDdcPriorDescs()
                                {
                                    DdcPriorDescEndDatesAssocMember = n.endDate,
                                    DdcPriorDescriptionsAssocMember = n.name
                                }).ToList()                            
            }).ToList())));

            dataReaderMock.Setup(d => d.ReadRecordAsync<Employes>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string,bool>((employeeId, b) =>  {
                    var record = testData.employeeRecords.FirstOrDefault(r => r.employeeId == employeeId);
                    if (record == null) return Task.FromResult(new Employes());
                    else return Task.FromResult(new Employes()
                    {
                        Recordkey = record.employeeId,
                        DirDepEntityAssociation = record.directives.Select(dd =>
                                        new EmployesDirDep()
                                        {
                                            EmpDepositIdAssocMember = dd.RecordKey,
                                            EmpDepAcctsLast4AssocMember = dd.Last4,
                                            EmpDepositCodesAssocMember = dd.BankCode,
                                            EmpDepositAmountsAssocMember = dd.Amount,
                                            EmpDepositEndDatesAssocMember = dd.EndDate,
                                            EmpDepositPrioritiesAssocMember = dd.Priority,
                                            EmpDepositStartDatesAssocMember = dd.StartDate,
                                            EmpDepositTypesAssocMember = dd.Type,
                                            EmpDepositAdddateAssocMember = dd.AddDate,
                                            EmpDepositAddtimeAssocMember = dd.AddTime,
                                            EmpDepositAddoprAssocMember = dd.AddOperator,
                                            EmpDepositChgdateAssocMember = dd.ChangeDate,
                                            EmpDepositChgtimeAssocMember = dd.ChangeTime,
                                            EmpDepositChgoprAssocMember = dd.ChangeOperator,
                                            EmpDepositChangeFlagsAssocMember = dd.ChangeFlag,
                                            EmpDepositNicknameAssocMember = dd.Nickname
                                        }).ToList()

                    });
                }
            );
            #endregion

            #region ctx mocks
            /*
            transManagerMock.Setup(t => t.ExecuteAsync<UpdatePayrollDepositsRequest, UpdatePayrollDepositsResponse>(It.IsAny<UpdatePayrollDepositsRequest>()))
                .Callback<UpdatePayrollDepositsRequest>(req =>
                {
                    updateRequest = req;
                    testData.UpdateEmployeeDirectDepositTestData(new TestDirectDepositsRepository.UpdateDirectDepositTransaction()
                    {
                        employeeId = req.EmployeeId,
                        directDeposits = req.EmpDirectDeposits.Select(dd =>
                            new TestDirectDepositsRepository.UpdateDirectDepositRecord()
                            {
                                amount = dd.DepositAmounts,
                                accountNumber = dd.DepositAccounts,
                                addDate = dd.DepositAddDate,
                                addOperator = dd.DepositAddOpr,
                                addTime = dd.DepositAddTime,
                                changeDate = dd.DepositChgDate,
                                changeOperator = dd.DepositChgOpr,
                                changeTime = dd.DepositChgTime,
                                code = dd.DepositCodes,
                                endDate = dd.DepositEndDates,
                                priority = dd.DepositPriorites,
                                startDate = dd.DepositStartDates,
                                type = dd.DepositTypes,
                                nickname = dd.DepositNicknames
                            }).ToList()
                    });
                })
                .Returns<UpdatePayrollDepositsRequest>(req =>
                    Task.FromResult(updateResponse)); 
             */
            #endregion

        }

        [TestClass]
        public class GetPayrollDepositDirectives : PayrollDepositDirectiveRepositoryTests
        {
            public string inputEmployeeId = "0003914";

            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveRepositoryTestsInitialize();
                
            }

            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                var expected = await testData.GetPayrollDepositDirectivesAsync("24601");
                var actual = await repositoryUnderTest.GetPayrollDepositDirectivesAsync("24601");
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullEmployeeIdExceptionTest()
            {
                try
                { 
                    await repositoryUnderTest.GetPayrollDepositDirectivesAsync(null);
                }
                catch (ArgumentNullException)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }
            //[TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            //public async Task NonExistentEmployeeExceptionTest()
            //{
            //    try
            //    {
            //        await repositoryUnderTest.GetPayrollDepositDirectivesAsync("mystery-man");
            //    }
            //    catch(KeyNotFoundException)
            //    {
            //        loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            //        throw;
            //    }                
            //}
            [TestMethod]
            public async Task NoEmployeeDirectDepositsInfLogTest()
            {
                testData.employeeRecords.First().directives = new List<TestPayrollDepositDirectivesRepository.PayrollRecord>();
                await repositoryUnderTest.GetPayrollDepositDirectivesAsync("24601");
                loggerMock.Verify(l => l.Info(It.IsAny<string>()));
            }
            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task NoBankRecordsExceptionTest()
            {
                try
                {
                    testData.BankRecords = new List<TestPayrollDepositDirectivesRepository.BankRecord>();
                    await repositoryUnderTest.GetPayrollDepositDirectivesAsync("24601");
                }
                catch(ApplicationException)
                {
                    loggerMock.Verify(e => e.Error(It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetPayrollDepositDirective : PayrollDepositDirectiveRepositoryTests
        {            
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                var id = "001";
                var expected = await testData.GetPayrollDepositDirectiveAsync(id, "24601");
                var actual = await repositoryUnderTest.GetPayrollDepositDirectiveAsync(id, "24601");
                Assert.AreEqual(expected, actual);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullDirectiveIdExceptionTest()
            {
                await repositoryUnderTest.GetPayrollDepositDirectiveAsync(null, "24601");
            }
        }

        [TestClass]
        public class UpdatePayrollDepositDirectives : PayrollDepositDirectiveRepositoryTests
        {
            PayrollDepositDirectiveCollection directivesToUpdate;
            Mock<IColleagueTransactionInvoker> invokerMock;            

            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveRepositoryTestsInitialize();

                invokerMock = new Mock<IColleagueTransactionInvoker>();

                invokerMock.Setup(i => i.ExecuteAsync<CreatePrDepositCodeRequest, CreatePrDepositCodeResponse>(
                        It.IsAny<CreatePrDepositCodeRequest>()
                    )
                ).Returns<CreatePrDepositCodeRequest, CreatePrDepositCodeResponse>(
                    (a, b) => Task.FromResult(
                        new CreatePrDepositCodeResponse()
                        {
                            ErrorMessage = "",
                            NewBank = a.NewBank.Select(bank =>
                                new NewBank()
                                {
                                    Id = bank.Id,
                                    BranchId = bank.BranchId,
                                    InstitutionId = bank.InstitutionId,
                                    Name = bank.Name,
                                    RoutingId = bank.RoutingId
                                }
                            ).ToList()
                        }
                    )
                );

                transFactoryMock.Setup(t => t.GetTransactionInvoker()).Returns(invokerMock.Object);
                    

                directivesToUpdate = new PayrollDepositDirectiveCollection("24601")
                {
                    new PayrollDepositDirective("1","24601","062201892","ofam",BankAccountType.Savings,"1234","lost a piece",true,1,34.4m,DateTime.Now,null,new Timestamp("24601",DateTime.Now,"24601",DateTime.Now))
                };
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullDirectivesToUpdateArgumentIsHandled()
            {
                await repositoryUnderTest.UpdatePayrollDepositDirectivesAsync(null);
            }
            [Ignore]
            [TestMethod]
            public async Task InputDepositsWithoutBankAssociationsCreateBankRecords()
            {
                try
                {
                    var updated = await repositoryUnderTest.UpdatePayrollDepositDirectivesAsync(directivesToUpdate);
                }
                catch(Exception)
                {
                    //transFactoryMock.Verify(t =>
                    //    t.GetTransactionInvoker().ExecuteAsync<CreatePrDepositCodeRequest, CreatePrDepositCodeResponse>(
                    //        It.IsAny<CreatePrDepositCodeRequest>()
                    //    )
                    //);
                    invokerMock.Verify(t =>
                        t.ExecuteAsync<CreatePrDepositCodeRequest, CreatePrDepositCodeResponse>(
                            It.IsAny<CreatePrDepositCodeRequest>()
                        )
                    );
                }
            }
            [Ignore]
            [TestMethod]
            public async Task UpdateIsExecuted()
            {

            }
            [Ignore]
            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task NullResponseFromCTXIsHandled()
            {

            }
            [Ignore]
            [TestMethod, ExpectedException(typeof(RecordLockException))]
            public async Task RecordLockInResponseFromCTXIsHandled()
            {

            }
            [Ignore]
            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task OtherErrorInResponseFromCTXIsHandled()
            {

            }
            [Ignore]
            [TestMethod]
            public async Task UpdatedDirectiveIsRetrieved()
            {

            }
        }

        [TestClass]
        public class AuthenticatePayrollDepositDirectiveTests : PayrollDepositDirectiveRepositoryTests
        {
            public string inputDirectiveId;
            public string inputAccountId;
            public string inputEmployeeId;

            public AuthenticatePayrollDepositDirectiveRequest actualAuthenticateRequest;

            public Guid expectedToken;
            public DateTimeOffset expectedExpiration;

            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveRepositoryTestsInitialize();
                inputDirectiveId = "foo";
                inputAccountId = "bar";
                inputEmployeeId = "0003914";

                expectedToken = Guid.NewGuid();
                expectedExpiration = new DateTimeOffset(new DateTime(2017, 3, 21, 14, 59, 0), TimeSpan.FromHours(-7));

                transManagerMock.Setup(t => t.ExecuteAsync<AuthenticatePayrollDepositDirectiveRequest, AuthenticatePayrollDepositDirectiveResponse>(It.IsAny<AuthenticatePayrollDepositDirectiveRequest>()))
                    .Callback<AuthenticatePayrollDepositDirectiveRequest>(req => actualAuthenticateRequest = req)
                    .Returns<AuthenticatePayrollDepositDirectiveRequest>((req) => Task.FromResult(new AuthenticatePayrollDepositDirectiveResponse()
                    {
                        Token = expectedToken.ToString(),
                        ExpirationDate = expectedExpiration.ToLocalDateTime(apiSettings.ColleagueTimeZone),
                        ExpirationTime = expectedExpiration.ToLocalDateTime(apiSettings.ColleagueTimeZone)
                    }));

            }

            [TestMethod]
            public async Task Test()
            {
                var result = await repositoryUnderTest.AuthenticatePayrollDepositDirective(inputEmployeeId, inputDirectiveId, inputAccountId);

                Assert.IsInstanceOfType(result, typeof(BankingAuthenticationToken));
                Assert.AreEqual(expectedToken, result.Token);
                Assert.AreEqual(expectedExpiration, result.ExpirationDateTimeOffset);
            }

            [TestMethod,ExpectedException(typeof(ArgumentNullException))]
            public async Task NullEmployeeIdIsHandled()
            {
                await repositoryUnderTest.AuthenticatePayrollDepositDirective(null, inputDirectiveId, inputAccountId);
            }
        }

    }
}
