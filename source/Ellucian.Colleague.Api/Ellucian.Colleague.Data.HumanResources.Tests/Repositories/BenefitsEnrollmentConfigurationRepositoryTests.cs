/* Copyright 2019-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class BenefitsEnrollmentConfigurationRepositoryTests : BaseRepositorySetup
    {
        private BenefitsEnrollmentConfigurationRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            repository = BuildBenefitsEnrollmentConfigurationRepository();
        }


        [TestCleanup]
        public void Cleanup()
        {
            cacheProviderMock = null;
            dataReaderMock = null;
            transFactoryMock = null;
            repository = null;
        }

        private BenefitsEnrollmentConfigurationRepository BuildBenefitsEnrollmentConfigurationRepository()
        {
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            List<string> list = new List<string>();
            list.Add("F");
            list.Add("M");
            HrwebDefaults value = new HrwebDefaults() { HrwebLimitRelationTypes = list };
            dataReaderMock.Setup(drm => drm.ReadRecordAsync<DataContracts.HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS", true)).ReturnsAsync(value);

            HrssDefaults hrssValue = new HrssDefaults() { HrssEnableBenefitsEnrlmnt = "Y" };
            dataReaderMock.Setup(drm => drm.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(hrssValue);

            repository = new BenefitsEnrollmentConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return repository;
        }

        [TestMethod]
        public async Task BenefitsEnrollmentConfigurationRepository_GetBenefitsEnrollmentConfigurationAsync()
        {
            var benefitsEnrollmentConfiguration = await repository.GetBenefitsEnrollmentConfigurationAsync();
            Assert.AreEqual(2, benefitsEnrollmentConfiguration.RelationshipTypes.Count);
            Assert.IsTrue(benefitsEnrollmentConfiguration.IsBenefitsEnrollmentEnabled);
        }

        [TestMethod]
        public async Task BenefitsEnrollmentConfigurationRepository_IsBenefitsEnrollmentEnableIsFalseWhenNull()
        {
            HrssDefaults hrssValue = new HrssDefaults() { HrssEnableBenefitsEnrlmnt = "" };
            dataReaderMock.Setup(drm => drm.ReadRecordAsync<DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(hrssValue);
            var benefitsEnrollmentConfiguration = await repository.GetBenefitsEnrollmentConfigurationAsync();
            Assert.IsFalse(benefitsEnrollmentConfiguration.IsBenefitsEnrollmentEnabled);
        }
    }
}
