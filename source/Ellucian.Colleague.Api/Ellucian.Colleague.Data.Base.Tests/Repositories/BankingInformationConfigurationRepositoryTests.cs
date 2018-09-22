// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Threading;
using System.Runtime.Caching;
using Ellucian.Data.Colleague.Repositories;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class BankingInformationConfigurationRepositoryTests : BaseRepositorySetup
    {
        #region TEST DATA
        public string paramCode;
        #endregion

        #region FIELDS AND INITILIAZATION
        public BankingInformationConfigurationRepository repositoryUnderTest;
        public BankingInformationConfiguration configuration;
        public TestBankingInformationConfigurationRepository testRepository;

        public void ConfigurationTestsInitialize()
        {
            testRepository = new TestBankingInformationConfigurationRepository();
            paramCode = testRepository.dbBankInfoParameters.BipTermsPara;
            this.MockInitialize();
            this.repositoryUnderTest = BuildRepository();
        }
        #endregion

        #region MOCK EVENTS
        private BankingInformationConfigurationRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.ReadRecordAsync<BankInfoParms>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((x, y, z) =>
                    Task.FromResult(testRepository.dbBankInfoParameters));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DocPara>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testRepository.dbDocsAndParagraphs == null ? null :
                    new Collection<DocPara>(testRepository.dbDocsAndParagraphs.Where(db => ids.Contains(db.Recordkey)).ToList())));

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new BankingInformationConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        #endregion

        #region CONSTRUCTOR TESTS

        [TestClass]
        public class ConstructorTests : BankingInformationConfigurationRepositoryTests
        {
            [TestInitialize]
            public void Init()
            {
                base.ConfigurationTestsInitialize();
            }

            [TestMethod]
            public void ClassIsInstantiatedTest()
            {
                Assert.IsInstanceOfType(repositoryUnderTest, typeof(BankingInformationConfigurationRepository));
            }

        }

        #endregion

        #region GET TESTS
        [TestClass]
        public class GetTests : BankingInformationConfigurationRepositoryTests
        {
            [TestInitialize]
            public void Init()
            {
                base.ConfigurationTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullParametersThrowsKeyNotFoundExceptionTest()
            {
                testRepository.dbBankInfoParameters = null;
                try
                {
                    await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                }
                catch (Exception x)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw x;
                }
            }

            [TestMethod]
            public async Task NullParagraphCollectionLogsDataErrorTest()
            {
                testRepository.dbDocsAndParagraphs = null;
                await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }


            [TestMethod]
            public async Task TermsAndConditionsAreGottenTests()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.AddEditAccountTermsAndConditions, result.AddEditAccountTermsAndConditions);
            }

            [TestMethod]
            public async Task NoTermsAndConditionsRecordTest()
            {
                testRepository.dbDocsAndParagraphs = new List<DocPara>();
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.IsNull(result.AddEditAccountTermsAndConditions);

                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task NoTermsAndConditionsIdTest()
            {
                testRepository.dbBankInfoParameters.BipTermsPara = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.IsNull(result.AddEditAccountTermsAndConditions);
            }

            [TestMethod]
            public async Task PayrollMessageTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollMessage, result.PayrollMessage);
            }

            [TestMethod]
            public async Task NoPayrollMessageIdTest()
            {
                testRepository.dbBankInfoParameters.BipPrMessagePara = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollMessage, result.PayrollMessage);
            }

            [TestMethod]
            public async Task NoPayrollMessageRecordTest()
            {
                testRepository.dbDocsAndParagraphs = new List<DocPara>();
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollMessage, result.PayrollMessage);

                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task EffectiveDateMessageTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollEffectiveDateMessage, result.PayrollEffectiveDateMessage);
            }

            [TestMethod]
            public async Task NoEffectiveDateMessageIdTest()
            {
                testRepository.dbBankInfoParameters.BipPrEffectDatePara = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollEffectiveDateMessage, result.PayrollEffectiveDateMessage);
            }

            [TestMethod]
            public async Task NoEffectiveDateMessageRecordTest()
            {
                testRepository.dbDocsAndParagraphs = new List<DocPara>();
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.PayrollEffectiveDateMessage, result.PayrollEffectiveDateMessage);

                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task IsRemainderAccountRequired_TrueTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsRemainderAccountRequired, result.IsRemainderAccountRequired);
            }

            [TestMethod]
            public async Task IsRemainderAccountRequired_NullTest()
            {
                testRepository.dbBankInfoParameters.BipDirDepRequired = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsRemainderAccountRequired, result.IsRemainderAccountRequired);
            }

            [TestMethod]
            public async Task IsRemainderAccountRequired_FalseTest()
            {
                testRepository.dbBankInfoParameters.BipDirDepRequired = "foobar";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsRemainderAccountRequired, result.IsRemainderAccountRequired);
            }

            [TestMethod]
            public async Task UseFederalRoutingDirectory_TrueTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.UseFederalRoutingDirectory, result.UseFederalRoutingDirectory);
            }

            [TestMethod]
            public async Task UseFederalRoutingDirectory_NullTest()
            {
                testRepository.dbBankInfoParameters.BipUseFedRoutingDir = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.UseFederalRoutingDirectory, result.UseFederalRoutingDirectory);
            }

            [TestMethod]
            public async Task UseFederalRoutingDirectory_FalseTest()
            {
                testRepository.dbBankInfoParameters.BipUseFedRoutingDir = "foobar";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.UseFederalRoutingDirectory, result.UseFederalRoutingDirectory);
            }

            [TestMethod]
            public async Task IsDirectDepositEnabled_NullTest()
            {
                testRepository.dbBankInfoParameters.BipPrDepositEnabled = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsDirectDepositEnabled, result.IsDirectDepositEnabled);
            }

            [TestMethod]
            public async Task IsDirectDepositEnabled_FalseTest()
            {
                testRepository.dbBankInfoParameters.BipPrDepositEnabled = "foobar";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsDirectDepositEnabled, result.IsDirectDepositEnabled);
            }

            [TestMethod]
            public async Task IsDirectDepositEnabled_TrueTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsDirectDepositEnabled, result.IsDirectDepositEnabled);
            }

            [TestMethod]
            public async Task IsPayableDepositEnabled_TrueTest()
            {
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }

            [TestMethod]
            public async Task IsPayableDepositEnabled_NullTest()
            {
                testRepository.dbBankInfoParameters.BipPayableDepositEnabled = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }

            [TestMethod]
            public async Task IsPayableDepositEnabled_FalseTest()
            {
                testRepository.dbBankInfoParameters.BipPayableDepositEnabled = "foobar";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }

            [TestMethod]
            public async Task IsAccountAuthDisabled_TrueTest()
            {
                testRepository.dbBankInfoParameters.BipAcctAuthDisabled = "Y";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }

            [TestMethod]
            public async Task IsAccountAuthDisabled_NullTest()
            {
                testRepository.dbBankInfoParameters.BipPayableDepositEnabled = null;
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }

            [TestMethod]
            public async Task IsAccountAuthDisabled_FalseTest()
            {
                testRepository.dbBankInfoParameters.BipAcctAuthDisabled = "N";
                var result = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                var expected = await testRepository.GetBankingInformationConfigurationAsync();
                Assert.AreEqual(expected.IsPayableDepositEnabled, result.IsPayableDepositEnabled);
            }



            [TestMethod]
            public async Task QueryIsCachedTest()
            {
                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()))
                    .Returns(true);
                var config = await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()));
            }


            [TestMethod]
            public async Task NullTermsAndConditionsDataErrorIsLoggedTest()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                testRepository.dbDocsAndParagraphs = new List<DocPara>();
                base.BuildRepository();
                await repositoryUnderTest.GetBankingInformationConfigurationAsync();
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }
        }
        #endregion

    }
}
