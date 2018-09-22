using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class BankingAuthenticationClaimRepositoryTests : BaseRepositorySetup
    {
        public BankingAuthenticationClaimRepository repositoryUnderTest;
        public string[] expiredBankingAuthClaimsRecordIds;

        public DeleteBankingAuthClaimsRequest actualDeleteRequest_ctx;
        public DeleteBankingAuthClaimsResponse returnedDeleteResponse_ctx;

        public DateTimeOffset expectedBankAuthClaimExpriation;

        public void BankingAuthenticationClaimRepositoryTestsInitialize()
        {
            MockInitialize();

            expectedBankAuthClaimExpriation = new DateTimeOffset(new DateTime(2017, 1, 1, 16, 20, 0));

            dataReaderMock.Setup(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.IsAny<string>(), true))
                .Returns<string, bool>((guid, b) => Task.FromResult(new BankingAuthClaims()
                {
                    BacExpirationDate = expectedBankAuthClaimExpriation.Date,
                    BacExpirationTime = expectedBankAuthClaimExpriation.DateTime,
                    Recordkey = guid,                   
                }));

            expiredBankingAuthClaimsRecordIds = new string[5] { "1", "2", "3", "4", "5" };

            dataReaderMock.Setup(dr => dr.SelectAsync("BANKING.AUTH.CLAIMS", It.IsAny<string>()))
                .Returns<string, string>((a, b) => Task.FromResult(expiredBankingAuthClaimsRecordIds));

            returnedDeleteResponse_ctx = new DeleteBankingAuthClaimsResponse();
            transManagerMock.Setup(t => t.ExecuteAsync<DeleteBankingAuthClaimsRequest, DeleteBankingAuthClaimsResponse>(It.IsAny<DeleteBankingAuthClaimsRequest>()))
                .Callback<DeleteBankingAuthClaimsRequest>((req) => actualDeleteRequest_ctx = req)
                .Returns<DeleteBankingAuthClaimsRequest>((req) => Task.FromResult(returnedDeleteResponse_ctx));
                


            repositoryUnderTest = new BankingAuthenticationClaimRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }


        [TestClass]
        public class GetBankingAuthenticationClaimTests : BankingAuthenticationClaimRepositoryTests
        {
            public Guid inputToken;

            [TestInitialize]
            public void Initialize()
            {
                BankingAuthenticationClaimRepositoryTestsInitialize();

                inputToken = Guid.NewGuid();
            }


            [TestMethod]
            public async Task ReadBankingAuthClaimsRecordTest()
            {
                await repositoryUnderTest.Get(inputToken);

                dataReaderMock.Verify(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.Is<string>(s => s == inputToken.ToString()), true));
            }

            [TestMethod]
            public async Task BuildBankingAuthenticationTokenTest()
            {
                var actual = await repositoryUnderTest.Get(inputToken);

                Assert.AreEqual(expectedBankAuthClaimExpriation, actual.ExpirationDateTimeOffset);
                Assert.AreEqual(inputToken, actual.Token);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ClaimsRecordNotFoundTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.IsAny<string>(), true))
                    .ReturnsAsync(null);

                await repositoryUnderTest.Get(inputToken);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RecordExpirationDateMustHaveValueTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.IsAny<string>(), true))
                .Returns<string, bool>((guid, b) => Task.FromResult(new BankingAuthClaims()
                {
                    BacExpirationDate = null,
                    BacExpirationTime = expectedBankAuthClaimExpriation.DateTime,
                    Recordkey = guid,
                }));

                await repositoryUnderTest.Get(inputToken);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RecordExpirationTimeMustHaveValueTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.IsAny<string>(), true))
                .Returns<string, bool>((guid, b) => Task.FromResult(new BankingAuthClaims()
                {
                    BacExpirationDate = expectedBankAuthClaimExpriation.Date,
                    BacExpirationTime = null,
                    Recordkey = guid,
                }));

                await repositoryUnderTest.Get(inputToken);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ErrorParsingGuidFromRecordTest()
            {
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<BankingAuthClaims>(It.IsAny<string>(), true))
                .Returns<string, bool>((guid, b) => Task.FromResult(new BankingAuthClaims()
                {
                    BacExpirationDate = expectedBankAuthClaimExpriation.Date,
                    BacExpirationTime = expectedBankAuthClaimExpriation.DateTime,
                    Recordkey = "foobar",
                }));

                await repositoryUnderTest.Get(inputToken);
            }

            [TestMethod]
            public async Task ExpiredTokensAreDeletedByCtxTest()
            {
                await repositoryUnderTest.Get(inputToken);

                CollectionAssert.AreEqual(expiredBankingAuthClaimsRecordIds, actualDeleteRequest_ctx.RecordIds);

                transManagerMock.Verify(t =>
                    t.ExecuteAsync<DeleteBankingAuthClaimsRequest, DeleteBankingAuthClaimsResponse>(
                        It.IsAny<DeleteBankingAuthClaimsRequest>()));                                
            }

            [TestMethod]
            public async Task NoExpiredTokensToDeleteTest()
            {
                expiredBankingAuthClaimsRecordIds = null;
                await repositoryUnderTest.Get(inputToken);

                transManagerMock.Verify(t =>
                    t.ExecuteAsync<DeleteBankingAuthClaimsRequest, DeleteBankingAuthClaimsResponse>(
                        It.IsAny<DeleteBankingAuthClaimsRequest>()), Times.Never);
            }
        }
    }
}
