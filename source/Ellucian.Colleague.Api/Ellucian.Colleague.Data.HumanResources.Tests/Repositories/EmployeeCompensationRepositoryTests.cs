/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
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
    public class EmployeeCompensationRepositoryTests : BaseRepositorySetup
    {
        public TestEmployeeCompensationRepository testDataRepository;

        public EmployeeCompensationRepository repositoryUnderTest;

        private string effectivePersonId;

        [TestInitialize]
        public void EmployeeCompensationRepositoryTestsInitialize()
        {
            effectivePersonId = "0014697";
            MockInitialize();
            testDataRepository = new TestEmployeeCompensationRepository();
            BuildRepository();
        }

        private void BuildRepository()
        {
            repositoryUnderTest = new EmployeeCompensationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestMethod]
        public async Task EmployeeCompensation_EqualsExpectedTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CalcTotalCompensationRequest, CalcTotalCompensationResponse>(It.IsAny<CalcTotalCompensationRequest>()))
                .ReturnsAsync(testDataRepository.CalcTotalCompensationResponse);
            dataReaderMock.Setup(r => r.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS", true)).ReturnsAsync(testDataRepository.HrwebDefaults());

            var expected = await testDataRepository.GetEmployeeCompensationAsync("0014698", null,false);
            var actual = await repositoryUnderTest.GetEmployeeCompensationAsync(effectivePersonId, null,false);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Bended);
            Assert.IsNotNull(actual.Taxes);
            Assert.IsNotNull(actual.Stipends);
            Assert.IsNull(actual.EmployeeCompensationError);
            Assert.AreEqual(expected.TotalCompensationPageHeader, actual.TotalCompensationPageHeader);
            Assert.AreEqual(expected.DisplayEmployeeCosts, actual.DisplayEmployeeCosts);
            Assert.AreEqual(expected.OtherBenefits, actual.OtherBenefits);
            Assert.AreEqual(expected.SalaryAmount, actual.SalaryAmount);
            Assert.AreEqual(expected.Bended.Count(), actual.Bended.Count());
            Assert.AreEqual(expected.Bended.ElementAt(0).BenededCode, actual.Bended.ElementAt(0).BenededCode);
            Assert.AreEqual(expected.Taxes.Count(), actual.Taxes.Count());
            Assert.AreEqual(expected.Taxes.ElementAt(0).TaxDescription, actual.Taxes.ElementAt(0).TaxDescription);
            Assert.AreEqual(expected.Stipends.Count(), actual.Stipends.Count());
            Assert.AreEqual(expected.Stipends.ElementAt(0).StipendAmount, actual.Stipends.ElementAt(0).StipendAmount);
        }

        [TestMethod]
        public async Task EmployeeCompensation_HrWebDefaults_Returns_Null()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CalcTotalCompensationRequest, CalcTotalCompensationResponse>(It.IsAny<CalcTotalCompensationRequest>()))
               .ReturnsAsync(testDataRepository.CalcTotalCompensationResponse);
                      
            var actual = await repositoryUnderTest.GetEmployeeCompensationAsync(effectivePersonId, null,false);

            Assert.IsTrue(string.IsNullOrEmpty(actual.OtherBenefits));
            Assert.IsTrue(string.IsNullOrEmpty(actual.TotalCompensationPageHeader));
            Assert.IsTrue(string.IsNullOrEmpty(actual.DisplayEmployeeCosts));

        }
        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task EmployeeCompensation_TransactionReturnsNullTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CalcTotalCompensationRequest, CalcTotalCompensationResponse>(It.IsAny<CalcTotalCompensationRequest>()))
                .ReturnsAsync(() => null);
            var actual = await repositoryUnderTest.GetEmployeeCompensationAsync(effectivePersonId, null,false);
        }

        [TestMethod]
        
        public async Task EmployeeCompensation_TransactionReturnsErrorMessageTest()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<CalcTotalCompensationRequest, CalcTotalCompensationResponse>(It.IsAny<CalcTotalCompensationRequest>()))
               .ReturnsAsync(testDataRepository.CalcTotalCompensationResponseWithError);

            string personId = "0014888";
            var actual = await repositoryUnderTest.GetEmployeeCompensationAsync(personId, null,false);
            var expected = await testDataRepository.GetEmployeeCompensationAsync(personId, null, false);

            Assert.AreEqual(expected.PersonId, actual.PersonId);
            Assert.AreEqual(expected.EmployeeCompensationError.ErrorCode, actual.EmployeeCompensationError.ErrorCode);
            Assert.AreEqual(expected.EmployeeCompensationError.ErrorMessage, actual.EmployeeCompensationError.ErrorMessage);
        }
    }


}
