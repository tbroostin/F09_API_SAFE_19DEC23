/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class CurrentBenefitsRepositoryTests : BaseRepositorySetup
    {
        public TestCurrentBenefitsRepository testDataRepository;

        public CurrentBenefitsRepository repositoryUnderTest;

        private string effectivePersonId;

        [TestInitialize]
        public void PersonStipendRepositoryTestsInitialize()
        {
            effectivePersonId = "0014697";
            MockInitialize();
            testDataRepository = new TestCurrentBenefitsRepository();
            BuildRepository();
        }

        private void BuildRepository()
        {
            repositoryUnderTest = new CurrentBenefitsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestMethod]
        public async Task CurrentBenefits_EqualsExpectedTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(It.IsAny<CurrentBenefitsRequest>()))
                .ReturnsAsync(testDataRepository.CurrentBenefitsResponse);
            
            var expected = await testDataRepository.GetEmployeeCurrentBenefitsAsync("0014697");
            var actual = await repositoryUnderTest.GetEmployeeCurrentBenefitsAsync(effectivePersonId);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.CurrentBenefits);
            Assert.IsNotNull(actual.PersonId);
            Assert.AreEqual(expected.PersonId, actual.PersonId);
            Assert.AreEqual(expected.CurrentBenefits.Count(), actual.CurrentBenefits.Count());
            Assert.AreEqual(expected.AdditionalInformation, actual.AdditionalInformation);
            Assert.AreEqual(expected.CurrentBenefits[0].BenefitDescription, actual.CurrentBenefits[0].BenefitDescription);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CurrentBenefits_TransactionReturnsNullTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(It.IsAny<CurrentBenefitsRequest>()))
                .ReturnsAsync(null);
            var actual = await repositoryUnderTest.GetEmployeeCurrentBenefitsAsync(effectivePersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CurrentBenefits_TransactionReturnsErrorMessageTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(It.IsAny<CurrentBenefitsRequest>()))
                .ReturnsAsync(testDataRepository.CurrentBenefitsResponseWithError);
            var actual = await repositoryUnderTest.GetEmployeeCurrentBenefitsAsync(effectivePersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CurrentBenefits_NullEffectivePersonId()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(It.IsAny<CurrentBenefitsRequest>()))
                .ReturnsAsync(testDataRepository.CurrentBenefitsResponse);
            var actual = await repositoryUnderTest.GetEmployeeCurrentBenefitsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CurrentBenefits_TransactionGeneralExceptionTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CurrentBenefitsRequest, CurrentBenefitsResponse>(It.IsAny<CurrentBenefitsRequest>()))
                .Throws(new Exception());
            var actual = await repositoryUnderTest.GetEmployeeCurrentBenefitsAsync(effectivePersonId);
        }
    }
}
