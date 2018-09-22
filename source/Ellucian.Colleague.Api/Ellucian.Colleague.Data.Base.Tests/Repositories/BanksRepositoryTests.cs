/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class BanksRepositoryTests : BaseRepositorySetup
    {
        #region SETUP
        public BankRepository repositoryUnderTest;
        public TestBankRepository testDataRepository;
        public Bank bank;
        public string bankId = "011000015";
        public string cacheKey;

        public Mock<HttpWebRequestHelper> httpWebRequestHelperMock;

        public Mock<HttpWebResponse> webResponseMock;

        public void BankRepositoryTestsInitialize()
        {
            MockInitialize();


            testDataRepository = new TestBankRepository();
            repositoryUnderTest = BuildActualRepository();
        }
        #endregion

        #region MOCK EVENTS
        private BankRepository BuildActualRepository()
        {

            // database
            // this mocks the call to get ACH permissions
            dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(() =>
                    Task.FromResult(
                         new BankInfoParms()
                         {
                             BipUseFedRoutingDir = "Y",
                         }
                        ));

            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((x, y) =>
                    Task.FromResult(testDataRepository.DatabaseBanks == null ? new string[0] :
                         new List<string>(testDataRepository.DatabaseBanks
                             .Where(db => db.DdcIsArchived != null && !db.DdcIsArchived.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                             .Select(b => (b.Recordkey).ToString()))
                             .ToArray()
                        ));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<PrDepositCodes>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((y, z) =>
                    Task.FromResult(testDataRepository.DatabaseBanks == null ? null :
                        new Collection<PrDepositCodes>(testDataRepository.DatabaseBanks.Select(b =>
                            new PrDepositCodes
                            {
                                DdcDescription = b.DdcDescription,
                                DdcFinInstNumber = b.DdcFinInstNumber,
                                DdcBrTransitNumber = b.DdcBrTransitNumber,
                                DdcTransitNo = b.DdcTransitNo,
                                DdcIsArchived = b.DdcIsArchived,
                                Recordkey = b.Recordkey,
                            }).ToList())));

            // web
            var mockUri = new Uri(@"http://www.google.com");
            var dataUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/FedACHdir.txt?AgreementSessionObject=Agree");
            var cookieUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/agreement.html");
            var postUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/submitAgreement");


            WebRequest.RegisterPrefix(cookieUri.ToString(), new MockHttpWebRequestCreator());
            WebRequest.RegisterPrefix(dataUri.ToString(), new MockHttpWebRequestCreator());

            httpWebRequestHelperMock = new Mock<HttpWebRequestHelper>();
            httpWebRequestHelperMock.SetupProperty(m => m.Host);
            httpWebRequestHelperMock.Setup(m => m.SetHttpWebRequest(It.IsAny<HttpWebRequest>()))
                .Callback<HttpWebRequest>((req) =>
                {
                    httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest()).Returns(() => req);
                    webResponseMock = new Mock<HttpWebResponse>();                  

                    if (req.RequestUri == postUri)
                    {
                        webResponseMock.Setup(w => w.StatusCode).Returns(() => HttpStatusCode.OK);
                        httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest().GetRequestStreamAsync()).Returns(async () => await Task.FromResult(new MemoryStream()));
                    }
                    else if (req.RequestUri == dataUri)
                    {
                       // webResponseMock.SetupProperty(resp => resp.ResponseUri, dataUri);
                        webResponseMock.Setup(w => w.ResponseUri).Returns(() => dataUri);
                        var memoryStreamResponse = new MemoryStream(Encoding.UTF8.GetBytes(testDataRepository.AchBanks ?? ""));
                        webResponseMock.Setup(w => w.GetResponseStream()).Returns(() => memoryStreamResponse);
                    }
                    httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest().GetResponseAsync()).Returns(async () => await Task.FromResult(webResponseMock.Object));
                });

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            return new BankRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings, httpWebRequestHelperMock.Object);
        }

        #endregion

        #region CONSTRUCTOR TESTS
        [TestClass]
        public class BankRepositoryConstructorTests : BanksRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BankRepositoryTestsInitialize();
            }
            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                repositoryUnderTest = null;
                testDataRepository = null;
            }

            [TestMethod]
            public void ReadSizeIsSetTest()
            {
                FieldInfo readSize = repositoryUnderTest
                                            .GetType()
                                            .GetField("readSize", BindingFlags.NonPublic | BindingFlags.Instance);
                var value = readSize.GetValue(repositoryUnderTest);
                Assert.IsTrue(value != null && Convert.ToInt32(value) == apiSettings.BulkReadSize);
            }
        }

        #endregion

        #region GET BANK TESTS
        [TestClass]
        public class GetBankAsyncTests : BanksRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BankRepositoryTestsInitialize();
            }
            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                repositoryUnderTest = null;
                testDataRepository = null;
            }

            #region TESTS FOR FUNCTIONALITY
            [TestMethod]
            public async Task aDataBaseBankIsReturnedTest()
            {
                bankId = testDataRepository.DatabaseBanks[0].DdcTransitNo;
                bank = await repositoryUnderTest.GetBankAsync(bankId);
                Assert.AreEqual(bankId, bank.Id);
            }

            [TestMethod]
            public async Task webBankIsReturnedTest()
            {
                bankId = "011000206";
                bank = await repositoryUnderTest.GetBankAsync(bankId);
                Assert.AreEqual(bankId, bank.Id);
            }

            [TestMethod]
            public async Task RedundantDataDoesNotCauseErrorTest()
            {
                testDataRepository.AchBanks = "655060042O0510000331070605000000000SOCIAL SECURITY ADMINISTRATION      6401 SECURITY BOULEVARD             BALTIMORE           MD212350000000000000011\r\n655060042O0510000331070605000000000SOCIAL SECURITY ADMINISTRATION      6401 SECURITY BOULEVARD             BALTIMORE           MD212350000000000000011\r\n655060042O0510000331070605000000000SOCIAL SECURITY ADMINISTRATION      6401 SECURITY BOULEVARD             BALTIMORE           MD212350000000000000011\r\n";
                //BuildActualRepository();
                var bank = await repositoryUnderTest.GetBankAsync("655060042");
                Assert.IsNotNull(bank);
            }
            [TestMethod]
            public async Task NoAchDirectoryCanStillGetBankFromOtherSourceTest() // look at the setup for this ( setup cannot handle null )
            {
                testDataRepository.AchBanks = null;
                //BuildActualRepository();
                var bank = await repositoryUnderTest.GetBankAsync("123-12345");
                Assert.IsNotNull(bank);
            }
            [TestMethod]
            public async Task NoDatabaseCanStillGetBankFromOtherSourceTest()
            {
                testDataRepository.DatabaseBanks = null;
                // BuildActualRepository();
                var bank = await repositoryUnderTest.GetBankAsync("011000206");
                Assert.IsNotNull(bank);
            }
            #endregion

            #region TESTS FOR ERRORS (LOGGED & OTHERWISE)
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ArgumentNullExceptionIsThrownForNullBankId()
            {
                bankId = null;
                bank = await repositoryUnderTest.GetBankAsync(bankId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ArgumentNullExceptionIsThrownForWhitespaceBankId()
            {
                bankId = "   ";
                bank = await repositoryUnderTest.GetBankAsync(bankId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task KeyNotFoundExceptionIsThrownForUnmatchedBankId()
            {
                bankId = "AAA";
                bank = await repositoryUnderTest.GetBankAsync(bankId);
            }

            [TestMethod]
            public async Task ErrorCreatingPayrollBanksTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception("foobar"));

                try
                {
                    await repositoryUnderTest.GetBankAsync(bankId);
                }
                catch(Exception) 
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s == "Error getting payrollBanks")));
                }

            }

            [TestMethod]
            public async Task ErrorCreatingFederalDirectoryBanksTest()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception("foobar"));
                try
                {
                    await repositoryUnderTest.GetBankAsync(bankId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s == "Error getting AchBanks")));
                }
            }

            [TestMethod]
            public async Task ErrorReadingTheAchDirectoryIsLoggedTest()
            {
                testDataRepository.AchBanks = "012345678 A Bank \r\n123412341 Probably a Bank too \r\n";
                //BuildActualRepository();
                var bank = await repositoryUnderTest.GetBankAsync("123-12345");
                // loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentOutOfRangeException>(), It.Is<string>(s => s.StartsWith("Error reading ACH Directory line"))));
            }
            [TestMethod]
            public async Task NoDatabaseIsLoggedTest()
            {

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<PrDepositCodes>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var bank = await repositoryUnderTest.GetBankAsync("011000015");
                loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Equals("Null PR.DEPOSIT.CODES read from database"))));
            }

            [TestMethod]
            public async Task ErrorGettingSessionCookieIsLoggedTest()
            {
                var httpWebRequestHelperMock = new Mock<HttpWebRequestHelper>();
                httpWebRequestHelperMock.SetupProperty(m => m.Host);
                httpWebRequestHelperMock.Setup(m => m.SetHttpWebRequest(It.IsAny<HttpWebRequest>()))
                    .Callback<HttpWebRequest>((req) =>
                    {
                        httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest()).Returns(() => req);
                        var webResponseMock = new Mock<HttpWebResponse>();

                        webResponseMock.Setup(w => w.StatusCode).Returns(() => HttpStatusCode.OK);
                        httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest().GetResponseAsync()).Returns(() => null);
                    });
                var newRepository = new BankRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings, httpWebRequestHelperMock.Object);
                await newRepository.GetBankAsync("123-12345");
                //loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                loggerMock.Verify(l => l.Error(It.IsAny<NullReferenceException>(), It.Is<string>(s => s.Equals("Error getting session cookies for ACH data web request"))));
                httpWebRequestHelperMock = null;
            }

            [TestMethod]
            public async Task NotOKResponseFromAgreementPostReturnsFalseTest()
            {
                var dataUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/FedACHdir.txt?AgreementSessionObject=Agree");
                var cookieUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/agreement.html");
                var postUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/submitAgreement");

                WebRequest.RegisterPrefix(cookieUri.ToString(), new MockHttpWebRequestCreator());
                WebRequest.RegisterPrefix(dataUri.ToString(), new MockHttpWebRequestCreator());
                var httpWebRequestHelperMock = new Mock<HttpWebRequestHelper>();
                httpWebRequestHelperMock.SetupProperty(m => m.Host);
                httpWebRequestHelperMock.Setup(m => m.SetHttpWebRequest(It.IsAny<HttpWebRequest>()))
                    .Callback<HttpWebRequest>((req) =>
                    {
                        httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest()).Returns(() => req);
                        var webResponseMock = new Mock<HttpWebResponse>();

                        if (req.RequestUri == postUri)
                        {
                            webResponseMock.Setup(w => w.StatusCode).Returns(() => HttpStatusCode.BadRequest);
                            httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest().GetRequestStreamAsync()).Returns(async () => await Task.FromResult(new MemoryStream()));
                        }
                        else if (req.RequestUri == dataUri)
                        {
                            var memoryStreamResponse = new MemoryStream(Encoding.UTF8.GetBytes(testDataRepository.AchBanks ?? ""));
                            webResponseMock.Setup(w => w.GetResponseStream()).Returns(() => memoryStreamResponse);
                        }
                        httpWebRequestHelperMock.Setup(m => m.GetHttpWebRequest().GetResponseAsync()).Returns(async () => await Task.FromResult(webResponseMock.Object));
                    });
                var newRepository = new BankRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings, httpWebRequestHelperMock.Object);
                await newRepository.GetBankAsync("123-12345");
                loggerMock.Verify(l => l.Error("Session Agreement was denied. Unable to retrieve requested ACH data"));
                httpWebRequestHelperMock = null;
            }


            //[TestMethod]
            //public async Task BadResponseStreamCreatesErrorWhichIsLoggedTest()
            //{
            //    Mock<StreamReader> streamReaderMock = new Mock<StreamReader>();
            //    streamReaderMock.Setup(x => x.ReadToEndAsync()).Throws(new Exception("bakit mo isalin ito?"));
            //    await repositoryUnderTest.GetBankAsync("123-12345");
            //}


            //[TestMethod]
            //public async Task BadCaReferenceIsLoggedTest()
            //{
            //    var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("BankRepositoryFake"), AssemblyBuilderAccess.RunAndSave);
            //    var moduleBuilder = assemblyBuilder.DefineDynamicModule("BankRepositoryFake.dll");
            //    var derivedBuilder = moduleBuilder.DefineType("BankRepositoryFake", TypeAttributes.Public, repositoryUnderTest.GetType());

            //    const MethodAttributes methodAttributes = MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.NewSlot;

            //    var constructor = derivedBuilder.DefineConstructor(
            //        MethodAttributes.Public,
            //        CallingConventions.Any,
            //        new Type[]{typeof(ICacheProvider), typeof(IColleagueTransactionFactory), typeof(ILogger), typeof(ApiSettings)}
            //        );
            //    var cGen = constructor.GetILGenerator();
            //    cGen.Emit(OpCodes.Ret);

            //    var methodOverride = derivedBuilder.DefineMethod(
            //        "BankRepository.CanadianInstitutionReference",
            //        methodAttributes,
            //        CallingConventions.HasThis,
            //        typeof(Task<Dictionary<string, string>>),
            //        Type.EmptyTypes);
            //    var ilGen = methodOverride.GetILGenerator();
            //    ilGen.Emit(OpCodes.Ldnull);
            //    ilGen.Emit(OpCodes.Ret);
            //    var methodToOverride = repositoryUnderTest.GetType().GetMethod("CanadianInstitutionReference", BindingFlags.NonPublic | BindingFlags.Instance);
            //    derivedBuilder.DefineMethodOverride(methodOverride, methodToOverride);
            //    var derivedType = derivedBuilder.CreateType();
            //    var props = derivedType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //    assemblyBuilder.Save("BankRepositoryFake.dll");

            //    var bank = await repositoryUnderTest.GetBankAsync("001");
            //    loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            //    loggerMock.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<object>()));
            //}
            #endregion
        }

        #endregion

        #region GET ALL BANKS TESTS
        [TestClass]
        public class GetAllBankAsyncTests : BanksRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BankRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = await testDataRepository.GetAllBanksAsync();
                var actual = await repositoryUnderTest.GetAllBanksAsync();

                CollectionAssert.AreEqual(expected.Values, actual.Values);
            }

            [TestMethod]
            public async Task RedirectUponRequestForDirctoryIsHandled()
            {
                var expected = await testDataRepository.GetAllBanksAsync();
                var retries = 0;
                var dataUri = new Uri(@"https://www.frbservices.org/EPaymentsDirectory/FedACHdir.txt?AgreementSessionObject=Agree");
                var redirectUri = new Uri(@"https://www.someredirectURI.com/and/I/hope/it/is/not/real");

                if(webResponseMock == null)
                {
                    webResponseMock = new Mock<HttpWebResponse>();
                }

                webResponseMock.Setup(w => w.ResponseUri).Returns(() => { if (retries++ <= 1) return redirectUri; else return dataUri; });

                var actual = await repositoryUnderTest.GetAllBanksAsync();

                CollectionAssert.AreEqual(expected.Values, actual.Values);
            }


            [TestMethod]
            public async Task ErrorBuildingBankFromPrDepositCodesTest()
            {
                testDataRepository.DatabaseBanks.Add(new PrDepositCodes() {
                    Recordkey = "666",
                    DdcDescription = "foobar bank",
                    DdcIsArchived = null,
                    DdcTransitNo = "foobar"
                });
                var results = await repositoryUnderTest.GetAllBanksAsync();
                loggerMock.Verify(l => l.Info(It.IsAny<ArgumentOutOfRangeException>(), It.IsAny<string>(), It.IsAny<object[]>()));
                Assert.IsFalse(results.Any(r => r.Value.RoutingId == "foobar" || r.Value.Name == "foobar bank"));
            }

            [TestMethod]
            public async Task RecacheTest()
            {
                var recache = false;
                var expected = await testDataRepository.GetAllBanksAsync(recache);

                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((s1, s2) => recache).Verifiable();
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()))
                    .Returns(true).Verifiable();

                await repositoryUnderTest.GetAllBanksAsync(recache);
                cacheProviderMock.Verify(c =>
                    c.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()),
                    Times.Once);

                recache = true;
                await repositoryUnderTest.GetAllBanksAsync(recache);
                cacheProviderMock.Verify(c =>
                    c.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()),
                    Times.Exactly(2));
            }
        }
        #endregion

        #region CREATE BANK TESTS
        [TestClass]
        public class CreateBankTest
        {
            [TestInitialize]
            public void Initialize()
            {
            }

            #region TESTS FOR FUNCTIONALITY

            #endregion

            #region TESTS FOR ERRORS (LOGGED & OTHERWISE)

            #endregion
        }
        #endregion

        #region CACHING TESTS
        [TestClass]
        public class CachingTests : BanksRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BankRepositoryTestsInitialize();
            }
            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                repositoryUnderTest = null;
                testDataRepository = null;
            }

            [TestMethod]
            public async Task GetBankAsync_WritesToCache()
            {
                var bankId = "123-12345";
                var banks = await testDataRepository.BankTransferInformation();
                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()))
                    .Returns(true);

                var bank = await repositoryUnderTest.GetBankAsync(bankId);

                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()));
            }
        }
        #endregion

        #region FEDERAL DIRECTORY ACCEPTANCE PARAMETER TESTS
        [TestClass]
        public class FederalDirectoryAcceptanceTests : BanksRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BankRepositoryTestsInitialize();
            }
            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                repositoryUnderTest = null;
                testDataRepository = null;
            }

            [TestMethod]
            public async Task TrueValueReturnsAllFedBanksTest()
            {
                var dbCount = testDataRepository.DatabaseBanks.Count();
                dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(() =>
                        Task.FromResult(
                             new BankInfoParms()
                             {
                                 BipUseFedRoutingDir = "Y",
                             }
                            ));
                var gottenBanks = await repositoryUnderTest.GetAllBanksAsync();
                Assert.IsTrue(dbCount < gottenBanks.Values.Count());
            }

            [TestMethod]
            public async Task FalseValueWitholdsAllFedBanksTest()
            {
                var expectedBanks = testDataRepository.DatabaseBanks.Where(db => db.DdcIsArchived != "Y");
                dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(() =>
                        Task.FromResult(
                             new BankInfoParms()
                             {
                                 BipUseFedRoutingDir = "N",
                             }
                            ));
                var actualBanks = await repositoryUnderTest.GetAllBanksAsync();
                Assert.AreEqual(expectedBanks.Count(), actualBanks.Values.Count());
            }

            [TestMethod]
            public async Task NullParametersAreLoggedAndExceptionIsCaughtAndLoggedTest()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(null);

                await repositoryUnderTest.GetAllBanksAsync();

                loggerMock.Verify(l => l.Info("Unable to find BankInfoParams record to retrieve BipUseFedRoutingDir"));


            }

        }
        #endregion
    }
}