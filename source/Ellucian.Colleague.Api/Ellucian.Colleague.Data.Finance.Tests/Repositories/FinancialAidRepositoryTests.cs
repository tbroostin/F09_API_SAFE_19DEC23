// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class FinancialAidRepositoryTests : BaseRepositorySetup
    {
        private FinancialAidRepository repository;

        public TestAccountActivityRepository testAccountActivityRepository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            MockInitialize();

            // Build the test repository
            repository = new FinancialAidRepository(cacheProvider, transFactory, logger);
        }

        [TestClass]
        public class FinancialAidRepository_GetPotentialD7FinancialAid_Tests : FinancialAidRepositoryTests
        {
            private string _validKey = "Valid";
            private string _emptyKey = "Empty";
            private string _nullAwardKey = "NullAward";
            private string _nullAmountKey = "NullAmount";

            private GetPotentialD7FinancialAidResponse validResponse;
            private GetPotentialD7FinancialAidResponse emptyResponse;
            private GetPotentialD7FinancialAidResponse nullAwardResponse;
            private GetPotentialD7FinancialAidResponse nullAmountResponse;

            private Dictionary<string, GetPotentialD7FinancialAidResponse> respDict; 
            private string termId = "TERM";
            private string award1 = "AWARD1";
            private string awardDesc1 = "Description for AWARD1";
            private string award2 = "AWARD2";
            private string awardDesc2 = "Description for AWARD2";
            private decimal amount1 = 123.45m;
            private decimal amount2 = 678.90m;

            [TestInitialize]
            public void FinancialAidRepository_GetPotentialD7FinancialAidAsync_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupGetPotentialD7FinancialAidCTX();

                // Build the test repository
                repository = new FinancialAidRepository(cacheProvider, transFactory, logger);
            }

            /// <summary>
            /// Valid input returns a valid response
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task FinancialAidRepository_GetPotentialD7FinancialAidAsync_Valid()
            {
                var criteria = new PotentialD7FinancialAidCriteria(_validKey, termId,
                    new List<AwardPeriodAwardTransmitExcessStatus>());
                var response = await repository.GetPotentialD7FinancialAidAsync(criteria);
                var result = response.ToList();
                Assert.AreEqual(award1, result.ElementAt(0).AwardPeriodAward);
                Assert.AreEqual(awardDesc1, result.ElementAt(0).AwardDescription);
                Assert.AreEqual(amount1, result.ElementAt(0).AwardAmount);
                Assert.AreEqual(award2, result.ElementAt(1).AwardPeriodAward);
                Assert.AreEqual(awardDesc2, result.ElementAt(1).AwardDescription);
                Assert.AreEqual(amount2, result.ElementAt(1).AwardAmount);
            }

            /// <summary>
            /// Invalid return data throws an exception
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task FinancialAidRepository_GetPotentialD7FinancialAidAsync_ExecutionError()
            {
                var criteria = new PotentialD7FinancialAidCriteria(_emptyKey, termId,
                    new List<AwardPeriodAwardTransmitExcessStatus>());
                var response = await repository.GetPotentialD7FinancialAidAsync(criteria);
            }

            /// <summary>
            /// Invalid return data throws an exception
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task FinancialAidRepository_GetPotentialD7FinancialAidAsync_NullAwardReturned()
            {
                var criteria = new PotentialD7FinancialAidCriteria(_nullAwardKey, termId,
                    new List<AwardPeriodAwardTransmitExcessStatus>());
                var response = await repository.GetPotentialD7FinancialAidAsync(criteria);
            }

            /// <summary>
            /// Null award amount converted to 0m
            /// </summary>
            [TestMethod]
            public async Task FinancialAidRepository_GetPotentialD7FinancialAidAsync_NullAwardAmountReturned()
            {
                var criteria = new PotentialD7FinancialAidCriteria(_nullAmountKey, termId,
                    new List<AwardPeriodAwardTransmitExcessStatus>());
                var response = await repository.GetPotentialD7FinancialAidAsync(criteria);
                var result = response.ToList();
                Assert.AreEqual(award1, result.ElementAt(0).AwardPeriodAward);
                Assert.AreEqual(awardDesc1, result.ElementAt(0).AwardDescription);
                Assert.AreEqual(0m, result.ElementAt(0).AwardAmount);
            }

            [TestCleanup]
            public void FinancialAidRepository_GetPotentialD7FinancialAidAsync_Cleanup()
            {
                validResponse = null;
                emptyResponse = null;
                nullAmountResponse = null;
                nullAwardResponse = null;
                respDict = null;
                transManagerMock = null;
                repository = null;
            }

            #region private methods
            private void SetupGetPotentialD7FinancialAidCTX()
            {            
                 validResponse = new GetPotentialD7FinancialAidResponse()
                {
                    PotentialD7FinancialAid = new List<Transactions.PotentialD7FinancialAid>()
                    {
                        new Transactions.PotentialD7FinancialAid()
                        {
                            PotentialAwdPrdAwards = award1,
                            PotentialAwdPrdAwardDescriptions = awardDesc1,
                            PotentialAwdPrdAwardAmounts = amount1,
                        },
                        new Transactions.PotentialD7FinancialAid()
                        {
                            PotentialAwdPrdAwards = award2,
                            PotentialAwdPrdAwardDescriptions = awardDesc2,
                            PotentialAwdPrdAwardAmounts = amount2,
                        }
                    },
                    AbortMessage = "",
                };

                emptyResponse = new GetPotentialD7FinancialAidResponse()
                {
                    PotentialD7FinancialAid = new List<Transactions.PotentialD7FinancialAid>(),
                    AbortMessage = "Some error occurred",
                };

                nullAwardResponse = new GetPotentialD7FinancialAidResponse()
                {
                    PotentialD7FinancialAid = new List<Transactions.PotentialD7FinancialAid>()
                    {
                        new Transactions.PotentialD7FinancialAid()
                        {
                            PotentialAwdPrdAwards = null,
                            PotentialAwdPrdAwardDescriptions = awardDesc1,
                            PotentialAwdPrdAwardAmounts = amount1,
                        },
                    },
                    AbortMessage = "",
                };

                nullAmountResponse = new GetPotentialD7FinancialAidResponse()
                {
                    PotentialD7FinancialAid = new List<Transactions.PotentialD7FinancialAid>()
                    {
                        new Transactions.PotentialD7FinancialAid()
                        {
                            PotentialAwdPrdAwards = award1,
                            PotentialAwdPrdAwardDescriptions = awardDesc1,
                            PotentialAwdPrdAwardAmounts = null,
                        },
                    },
                    AbortMessage = "",
                };

                respDict = new Dictionary<string, GetPotentialD7FinancialAidResponse>();
                respDict.Add(_validKey, validResponse);
                respDict.Add(_emptyKey, emptyResponse);
                respDict.Add(_nullAwardKey, nullAwardResponse);
                respDict.Add(_nullAmountKey, nullAmountResponse);

                transManagerMock.Setup(trans => trans
                   .ExecuteAsync<GetPotentialD7FinancialAidRequest, GetPotentialD7FinancialAidResponse>(It.IsAny<GetPotentialD7FinancialAidRequest>()))
                   .Returns((GetPotentialD7FinancialAidRequest x) => 
                   {
                       return Task.FromResult(respDict[x.StudentId]);
                   });
            }
            #endregion
        }
    }
}
