using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PayableDepositDirectiveRepositoryTests : BaseRepositorySetup
    {

        public PayableDepositDirectiveRepository repositoryUnderTest;
        public TestPayableDepositDirectiveRepository testData;


        public CreatePayableDepDirectiveRequest createRequest;
        public CreatePayableDepDirectiveRequest createResponse;
        public UpdatePayableDepDirectiveRequest updateRequest;
        public UpdatePayableDepDirectiveRequest updateResponse;
        public DeletePayableDepDirectiveRequest deleteRequest;
        public DeletePayableDepDirectiveRequest deleteResponse;

        public void PayableDepositDirectiveRepositoryTestsInitialize()
        {
            testData = new TestPayableDepositDirectiveRepository();
            MockInitialize();
            repositoryUnderTest = new PayableDepositDirectiveRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            //mock the db calls for the Get, since that's used in multiple places
            dataReaderMock.Setup(d => d.ReadRecordAsync<PersonAddrBnkInfo>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, x) =>
                {
                    var record = testData.payableDepositDirectiveRecords.FirstOrDefault(r => r.id == id);
                    return Task.FromResult(record == null ? null :
                        convertTestObjectToDataContract(record));
                });


            dataReaderMock.Setup(d => d.BulkReadRecordAsync<PersonAddrBnkInfo>("PERSON.ADDR.BNK.INFO", It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((f, criteria, b) =>
                {
                    var payeeId = criteria.Split(char.Parse("'"))[1];
                    return Task.FromResult(new Collection<PersonAddrBnkInfo>(
                        testData.payableDepositDirectiveRecords.Where(r => r.personId == payeeId)
                            .Select(r => convertTestObjectToDataContract(r))
                            .ToList()
                        ));
                });

            //mock select since its used in multiple places
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>()))
                .Returns<string, string[], string>((f, ids, c) =>
                    Task.FromResult(testData.payableDepositDirectiveRecords
                        .Where(rec => ids.Contains(rec.id))
                        .Select(rec => rec.id)
                        .ToArray())
     );
        }

        public PersonAddrBnkInfo convertTestObjectToDataContract(TestPayableDepositDirectiveRepository.PayableDepositDirectiveRecord record)
        {
            return new PersonAddrBnkInfo()
            {
                Recordkey = record.id,
                PabiAcctType = record.accountType,
                PabiAddressId = record.addressId,
                PabiBankAcctNoLast4 = record.accountIdLastFour,
                PabiBrTransitNo = record.branchNumber,
                PabiEcheckFlag = record.eCheckFlag,
                PabiEffDate = record.startDate,
                PabiFinInstNo = record.institutionId,
                PabiNickname = record.nickname,
                PabiPersonId = record.personId,
                PabiPrenote = record.isVerified,
                PabiRoutingNo = record.routingId,
                PersonAddrBnkInfoAdddate = record.addDate,
                PersonAddrBnkInfoAddopr = record.addOperator,
                PersonAddrBnkInfoAddtime = record.addTime,
                PersonAddrBnkInfoChgdate = record.changeDate,
                PersonAddrBnkInfoChgopr = record.changeOperator,
                PersonAddrBnkInfoChgtime = record.changeTime
            };
        }

        [TestClass]
        public class GetPayableDepositDirectivesTests : PayableDepositDirectiveRepositoryTests
        {
            public string inputPayeeId;
            public string directiveId;

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveRepositoryTestsInitialize();
                inputPayeeId = testData.payableDepositDirectiveRecords[0].personId;
                directiveId = testData.payableDepositDirectiveRecords[0].id;
            }

            [TestMethod]
            public async Task GetAllTest()
            {
                var expected = await testData.GetPayableDepositDirectivesAsync(inputPayeeId);
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId);

                Assert.AreEqual(inputPayeeId, actual.PayeeId);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod]
            public async Task GetSingleTest()
            {
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, directiveId);


                Assert.AreEqual(1, actual.Count);
                Assert.AreEqual(directiveId, actual[0].Id);
            }

            //test input args
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullPayeeIdThrowsExceptionTest()
            {
                await repositoryUnderTest.GetPayableDepositDirectivesAsync(null);
            }

            [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            public async Task DirectiveIdNotFoundOnGetThrowsExceptionTest()
            {
                try
                {
                    var unknownDirectiveId = "99999";
                    await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, unknownDirectiveId);
                }
                catch (KeyNotFoundException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            public async Task DifferentPayeeIdThrowsExceptionTest()
            {
                try
                {
                    var differentPayeeId = "99999";
                    await repositoryUnderTest.GetPayableDepositDirectivesAsync(differentPayeeId, directiveId);
                }
                catch (KeyNotFoundException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod]
            public async Task NoPayableDirectivesForPayeeIdThrowsWarningTest()
            {
                var payeeId = "99999";
                await repositoryUnderTest.GetPayableDepositDirectivesAsync(payeeId);
                loggerMock.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<object[]>()));
            }

            [TestMethod]
            public async Task RoutingIdPopulatedOnGetUSDirectiveTest()
            {
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
                Assert.IsTrue(!string.IsNullOrEmpty(actual.First().RoutingId));
                Assert.IsTrue(string.IsNullOrEmpty(actual.First().InstitutionId));
                Assert.IsTrue(string.IsNullOrEmpty(actual.First().BranchNumber));
            }

            [TestMethod]
            public async Task InstIdBranchNumberPopulatedOnGetCanadianDirectiveTest()
            {
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "502");
                Assert.IsTrue(string.IsNullOrEmpty(actual.First().RoutingId));
                Assert.IsTrue(!string.IsNullOrEmpty(actual.First().InstitutionId));
                Assert.IsTrue(!string.IsNullOrEmpty(actual.First().BranchNumber));
            }

            [TestMethod]
            public async Task InvalidRoutingIDOnGetLogsErrorTest()
            {
                try
                {
                    testData.payableDepositDirectiveRecords.First().routingId = "12345678901234567890";
                    var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                }
            }

            [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            public async Task SingleDirectiveNotReturnedOnGetTest()
            {
                testData.payableDepositDirectiveRecords.First().routingId = "12345678901234567890";
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
            }

            [TestMethod]
            public async Task PreNoteIsFalseOnGetTest()
            {
                testData.payableDepositDirectiveRecords.First().isVerified = "Q";
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
                Assert.IsFalse(actual.First().IsVerified);
            }

            [TestMethod]
            public async Task ECheckFlagIsFalseOnGetTest()
            {
                testData.payableDepositDirectiveRecords.First().eCheckFlag = "Q";
                var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
                Assert.IsFalse(actual.First().IsElectronicPaymentRequested);
            }

            [TestMethod]
            public async Task InvalidAccountTypeOnGetLogsErrorTest()
            {
                try
                {
                    testData.payableDepositDirectiveRecords.First().accountType = "Q";
                    var actual = await repositoryUnderTest.GetPayableDepositDirectivesAsync(inputPayeeId, "500");
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                }
            }

        }

        [TestClass]
        public class CreatePayableDepositDirectiveTest : PayableDepositDirectiveRepositoryTests
        {
            public PayableDepositDirective inputPayableDepositDirective;


            public CreatePayableDepDirectiveResponse responseCtx;

            //use doCreate in tests where you expect a valid result.
            //doCreate updates the testData with a new record, so the calls to the Get methods return an object
            public async Task<PayableDepositDirective> doCreate()
            {
                var fakeTheDb = await testData.CreatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                responseCtx.OutPersonAddrBnkInfoId = fakeTheDb.Id;

                var actual = await repositoryUnderTest.CreatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                return actual;
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveRepositoryTestsInitialize();

                inputPayableDepositDirective = new PayableDepositDirective(
                    null,
                    testData.payableDepositDirectiveRecords[0].personId,
                    testData.payableDepositDirectiveRecords[0].routingId,
                    "bankName",
                    BankAccountType.Checking,
                    "4444",
                    "nickname",
                    false,
                    null,
                    new DateTime(2017, 1, 1),
                    null,
                    true,
                    new Timestamp("mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)), "mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)))
                    );

                inputPayableDepositDirective.SetNewAccountId("44444444");


                responseCtx = new CreatePayableDepDirectiveResponse()
                {
                    OutPersonAddrBnkInfoId = "foo",
                    ErrorMessage = null
                };
                transManagerMock.Setup(t => t.ExecuteAsync<CreatePayableDepDirectiveRequest, CreatePayableDepDirectiveResponse>(It.IsAny<CreatePayableDepDirectiveRequest>()))
                    .Returns<CreatePayableDepDirectiveRequest>(req => Task.FromResult(responseCtx));
            }

            [TestMethod]
            public async Task CreateTest()
            {
                var actual = await doCreate();

                Assert.AreEqual(responseCtx.OutPersonAddrBnkInfoId, actual.Id);
            }

            [TestMethod]
            public async Task ActualCtxRequestTest()
            {
                var actual = await doCreate();

                transManagerMock.Verify(t => t.ExecuteAsync<CreatePayableDepDirectiveRequest, CreatePayableDepDirectiveResponse>(
                    It.Is<CreatePayableDepDirectiveRequest>(req =>
                        req.AccountType == testData.convertBankAccountTypeToRecordColumn(inputPayableDepositDirective.BankAccountType) &&
                        req.AddressId == inputPayableDepositDirective.AddressId &&
                        req.BankAccountNumber == inputPayableDepositDirective.NewAccountId &&
                        req.BranchTransitNumber == inputPayableDepositDirective.BranchNumber &&
                        req.EcheckFlag == (inputPayableDepositDirective.IsElectronicPaymentRequested ? "Y" : "N") &&
                        req.EffectiveDate == inputPayableDepositDirective.StartDate &&
                        req.FinancialInstitutionNumber == inputPayableDepositDirective.InstitutionId &&
                        req.Nickname == inputPayableDepositDirective.Nickname &&
                        req.PersonId == inputPayableDepositDirective.PayeeId &&
                        req.Prenote == (inputPayableDepositDirective.IsVerified ? "Y" : "N") &&
                        req.RoutingNumber == inputPayableDepositDirective.RoutingId
                    )));

            }

            [TestMethod]
            public async Task NullNicknameReturnsEmptyStringTest()
            {
                inputPayableDepositDirective = new PayableDepositDirective(
                    null,
                    testData.payableDepositDirectiveRecords[0].personId,
                    testData.payableDepositDirectiveRecords[0].routingId,
                    "bankName",
                    BankAccountType.Checking,
                    "4444",
                    null, // nickname
                    false,
                    null,
                    new DateTime(2017, 1, 1),
                    null,
                    true,
                    new Timestamp("mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)), "mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)))
                    );

                inputPayableDepositDirective.SetNewAccountId("44444444");

                var actual = await doCreate();

                transManagerMock.Verify(t => t.ExecuteAsync<CreatePayableDepDirectiveRequest, CreatePayableDepDirectiveResponse>(
                    It.Is<CreatePayableDepDirectiveRequest>(req =>
                        req.Nickname == string.Empty
                    )));
            }

            [TestMethod]
            public async Task LongNicknameReturnsFirst50Test()
            {
                const string First50 = "12345678901234567892123456789312345678941234567895";
                inputPayableDepositDirective = new PayableDepositDirective(
                    null,
                    testData.payableDepositDirectiveRecords[0].personId,
                    testData.payableDepositDirectiveRecords[0].routingId,
                    "bankName",
                    BankAccountType.Checking,
                    "4444",
                    "12345678901234567892123456789312345678941234567895Morethan50", // nickname
                    false,
                    null,
                    new DateTime(2017, 1, 1),
                    null,
                    true,
                    new Timestamp("mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)), "mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)))
                    );

                inputPayableDepositDirective.SetNewAccountId("44444444");

                var actual = await doCreate();

                transManagerMock.Verify(t => t.ExecuteAsync<CreatePayableDepDirectiveRequest, CreatePayableDepDirectiveResponse>(
                    It.Is<CreatePayableDepDirectiveRequest>(req =>
                        req.Nickname == First50
                    )));
            }
            
            
            //test input args
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputDirectiveOnCreateThrowsExceptionTest()
            {
                await repositoryUnderTest.CreatePayableDepositDirectiveAsync(null);
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ErrorMessageNotNullOnCreateThrowsExceptionTest()
            {
                try
                {
                    responseCtx.ErrorMessage = "Something went wrong.";
                    await repositoryUnderTest.CreatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task OutPersonAddrBnkInfoIdNullOnCreateThrowsExceptionTest()
            {
                try
                {
                    responseCtx.OutPersonAddrBnkInfoId = null;
                    await repositoryUnderTest.CreatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

        }

        [TestClass]
        public class UpdatePayableDepositDirectiveTest : PayableDepositDirectiveRepositoryTests
        {
            public UpdatePayableDepDirectiveResponse responseCtx;

            public async Task<PayableDepositDirective> inputDirective()
            {
                var directive = (await testData.GetPayableDepositDirectivesAsync(
                    testData.payableDepositDirectiveRecords[0].personId,
                    testData.payableDepositDirectiveRecords[0].id)).First();
                directive.SetNewAccountId("1234567890");
                return directive;
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveRepositoryTestsInitialize();

                responseCtx = new UpdatePayableDepDirectiveResponse()
                {
                    ErrorMessage = null
                };

                transManagerMock.Setup(t => t.ExecuteAsync<UpdatePayableDepDirectiveRequest, UpdatePayableDepDirectiveResponse>(It.IsAny<UpdatePayableDepDirectiveRequest>()))
                    .Returns<UpdatePayableDepDirectiveRequest>(req => Task.FromResult(responseCtx));
            }

            [TestMethod]
            public async Task UpdateTest()
            {
                var input = await inputDirective();
                var actual = await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(input);

                Assert.AreEqual(input, actual);
            }

            [TestMethod]
            public async Task UpdateCtxTest()
            {
                var inputPayableDepositDirective = await inputDirective();
                await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(inputPayableDepositDirective);

                transManagerMock.Verify(t => t.ExecuteAsync<UpdatePayableDepDirectiveRequest, UpdatePayableDepDirectiveResponse>(
                    It.Is<UpdatePayableDepDirectiveRequest>(req =>
                        req.AccountType == testData.convertBankAccountTypeToRecordColumn(inputPayableDepositDirective.BankAccountType) &&
                        req.BankAccountNumber == inputPayableDepositDirective.NewAccountId &&
                        req.BranchTransitNumber == inputPayableDepositDirective.BranchNumber &&
                        req.EcheckFlag == (inputPayableDepositDirective.IsElectronicPaymentRequested ? "Y" : "N") &&
                        req.EffectiveDate == inputPayableDepositDirective.StartDate &&
                        req.FinancialInstitutionNumber == inputPayableDepositDirective.InstitutionId &&
                        req.Nickname == inputPayableDepositDirective.Nickname &&
                        req.Prenote == (inputPayableDepositDirective.IsVerified ? "Y" : "N") &&
                        req.RoutingNumber == inputPayableDepositDirective.RoutingId
                    )));
            }

            //test input args
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputDirectiveOnUpdateThrowsExceptionTest()
            {
                await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(null);
            }

            [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            public async Task DirectiveIdNotFoundOnUpdateThrowsExceptionTest()
            {
                try
                {
                    PayableDepositDirective inputPayableDepositDirective;
                    inputPayableDepositDirective = new PayableDepositDirective(
                        "99999",
                        testData.payableDepositDirectiveRecords[0].personId,
                        testData.payableDepositDirectiveRecords[0].routingId,
                        "bankName",
                        BankAccountType.Checking,
                        "4444",
                        "nickname",
                        false,
                        null,
                        new DateTime(2017, 1, 1),
                        null,
                        true,
                        new Timestamp("mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)), "mcd", new DateTimeOffset(2017, 4, 10, 11, 12, 0, TimeSpan.FromHours(-4)))
                        );
                    await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                }
                catch (KeyNotFoundException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ResponseNullOnUpdateThrowsExceptionTest()
            {
                try
                {
                    var inputPayableDepositDirective = await inputDirective();
                    responseCtx = null;
                    await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ErrorMessageNotNullOnUpdateThrowsExceptionTest()
            {
                try
                {
                    var inputPayableDepositDirective = await inputDirective();
                    responseCtx.ErrorMessage = "Something went wrong.";
                    await repositoryUnderTest.UpdatePayableDepositDirectiveAsync(inputPayableDepositDirective);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

        }

        [TestClass]
        public class DeletePayableDepositDirectiveTest : PayableDepositDirectiveRepositoryTests
        {
            public string inputDirectiveId;

            public DeletePayableDepDirectiveResponse responseCtx;

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveRepositoryTestsInitialize();
                inputDirectiveId = testData.payableDepositDirectiveRecords[0].id;

                responseCtx = new DeletePayableDepDirectiveResponse()
                {
                    ErrorMessage = null
                };
                transManagerMock.Setup(t => t.ExecuteAsync<DeletePayableDepDirectiveRequest, DeletePayableDepDirectiveResponse>(It.IsAny<DeletePayableDepDirectiveRequest>()))
                    .Returns<DeletePayableDepDirectiveRequest>(req => Task.FromResult(responseCtx));

            }

            [TestMethod]
            public async Task DeleteTest()
            {
                await repositoryUnderTest.DeletePayableDepositDirectiveAsync(inputDirectiveId);
            }

            [TestMethod]
            public async Task DeleteCtxRequestTest()
            {
                await repositoryUnderTest.DeletePayableDepositDirectiveAsync(inputDirectiveId);

                transManagerMock.Verify(t => t.ExecuteAsync<DeletePayableDepDirectiveRequest, DeletePayableDepDirectiveResponse>(
                    It.Is<DeletePayableDepDirectiveRequest>(req =>
                        req.PersonAddrBnkInfoId == inputDirectiveId
                    )));
            }

            //test input args
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputDirectiveOnDeleteThrowsExceptionTest()
            {
                await repositoryUnderTest.DeletePayableDepositDirectiveAsync(null);
            }

            [TestMethod, ExpectedException(typeof(KeyNotFoundException))]
            public async Task DirectiveIdNotFoundOnDeleteThrowsExceptionTest()
            {
                try
                {
                    await repositoryUnderTest.DeletePayableDepositDirectiveAsync("99999");
                }
                catch (KeyNotFoundException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ResponseNullOnDeleteThrowsExceptionTest()
            {
                try
                {
                    responseCtx = null;
                    await repositoryUnderTest.DeletePayableDepositDirectiveAsync(inputDirectiveId);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(RecordLockException))]
            public async Task ConflictMessageOnDeleteThrowsExceptionTest()
            {
                try
                {
                    responseCtx.ErrorMessage = "Conflict detected.";
                    await repositoryUnderTest.DeletePayableDepositDirectiveAsync(inputDirectiveId);
                }
                catch (RecordLockException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public async Task ErrorMessageNotNullOnDeleteThrowsExceptionTest()
            {
                try
                {
                    responseCtx.ErrorMessage = "Something went wrong.";
                    await repositoryUnderTest.DeletePayableDepositDirectiveAsync(inputDirectiveId);
                }
                catch (ApplicationException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

        }


        [TestClass]
        public class AuthenticatePayableDepositDirectiveAsync : PayableDepositDirectiveRepositoryTests
        {
            public string inputPayeeId;
            public string inputDirectiveId;
            public string inputAccountId;
            public string inputAddressId;

            public AuthenticatePayableDepositDirectiveResponse responseCtx;

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveRepositoryTestsInitialize();

                inputPayeeId = testData.payableDepositDirectiveRecords[0].personId;
                inputDirectiveId = testData.payableDepositDirectiveRecords[0].id;
                inputAccountId = "foobar";
                inputAddressId = "5454";


                responseCtx = new AuthenticatePayableDepositDirectiveResponse()
                {
                    ErrorMessage = null,
                    ExpirationDate = new DateTime(2017, 4, 10),
                    ExpirationTime = new DateTime(1, 1, 1, 12, 28, 0),
                    Token = Guid.NewGuid().ToString()
                };

                transManagerMock.Setup(t => t.ExecuteAsync<AuthenticatePayableDepositDirectiveRequest, AuthenticatePayableDepositDirectiveResponse>(It.IsAny<AuthenticatePayableDepositDirectiveRequest>()))
                    .Returns<AuthenticatePayableDepositDirectiveRequest>(req => Task.FromResult(responseCtx));

            }

            [TestMethod]
            public async Task AuthenticationTest()
            {
                var actual = await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId, inputAddressId);
                Assert.AreEqual(Guid.Parse(responseCtx.Token), actual.Token);

                var expectedDateTimeOffset = responseCtx.ExpirationTime.ToPointInTimeDateTimeOffset(responseCtx.ExpirationDate, apiSettings.ColleagueTimeZone);
                Assert.AreEqual(expectedDateTimeOffset, actual.ExpirationDateTimeOffset);
            }

            [TestMethod]
            public async Task CtxRequestTest()
            {
                var actual = await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId,inputAddressId);


                transManagerMock.Verify(t => t.ExecuteAsync<AuthenticatePayableDepositDirectiveRequest, AuthenticatePayableDepositDirectiveResponse>(
                    It.Is<AuthenticatePayableDepositDirectiveRequest>(req =>
                        req.PayeeId == inputPayeeId &&
                        req.AddressId == inputAddressId &&
                        req.BankAccountId == inputAccountId &&
                        req.PayableDepositDirectiveId == inputDirectiveId
                    )));
            }

            //test input args
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task NullInputDirectiveOnAuthenticateThrowsExceptionTest()
            {
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(null, inputDirectiveId, inputAccountId,inputAddressId);
            }

            [TestMethod, ExpectedException(typeof(BankingAuthenticationException))]
            public async Task ResponseNullOnAuthenticateThrowsExceptionTest()
            {
                responseCtx = null;
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId,inputAddressId);
            }

            [TestMethod, ExpectedException(typeof(BankingAuthenticationException))]
            public async Task ErrorMessageNotNullOnAuthenticateThrowsExceptionTest()
            {
                responseCtx.ErrorMessage = "Something went wrong.";
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId,inputAddressId);
            }

            [TestMethod, ExpectedException(typeof(BankingAuthenticationException))]
            public async Task ExpirationDateNullOnAuthenticateThrowsExceptionTest()
            {
                responseCtx.ExpirationDate = null;
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId,inputAddressId);
            }

            [TestMethod, ExpectedException(typeof(BankingAuthenticationException))]
            public async Task ExpirationTimeOrTimeNullOnAuthenticateThrowsExceptionTest()
            {
                responseCtx.ExpirationTime = null;
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId, inputAddressId);
            }

            [TestMethod, ExpectedException(typeof(BankingAuthenticationException))]
            public async Task InvalidGuidOnAuthenticateThrowsExceptionTest()
            {
                responseCtx.Token = "foobar";
                await repositoryUnderTest.AuthenticatePayableDepositDirectiveAsync(inputPayeeId, inputDirectiveId, inputAccountId, inputAddressId);
            }

        }
    }
}
