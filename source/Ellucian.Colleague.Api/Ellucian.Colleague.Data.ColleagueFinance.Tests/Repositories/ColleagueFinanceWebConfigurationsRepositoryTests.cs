// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ColleagueFinanceWebConfigurationsRepositoryTests : BaseRepositorySetup
    {
        protected ColleagueFinanceWebConfigurationsRepository colleagueFinanceWebConfigurationsRepository;
        protected TestColleagueFinanceWebConfigurationsRepository expectedRepository;
        private CfwebDefaults cfWebDefaultsDataContract;
        private PurDefaults purDefaultsDataContract;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            expectedRepository = new TestColleagueFinanceWebConfigurationsRepository();
            colleagueFinanceWebConfigurationsRepository = BuildMockColleagueFinanceWebConfigurationsRepository();
            this.cfWebDefaultsDataContract = new CfwebDefaults();
            this.cfWebDefaultsDataContract.CfwebEmailType = "PRI";
            this.cfWebDefaultsDataContract.CfwebReqGlRequired = "Y";
            this.cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "Y";
            this.cfWebDefaultsDataContract.CfwebReqDesiredDays = 7;
            this.purDefaultsDataContract = new PurDefaults();
            this.purDefaultsDataContract.PurShipToCode = "MC";
        }


        [TestCleanup]
        public void TestCleanup()
        {
            colleagueFinanceWebConfigurationsRepository = null;
            expectedRepository = null;
        }
        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidDefaultEmailType()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.DefaultEmailType);
            Assert.AreEqual(cfWebDefaultsExpected.Result.DefaultEmailType, cfWebDefaultsActual.Result.DefaultEmailType);
        }
        public void GetColleagueFinanceWebConfigurations_InvalidDefaultEmailType()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.DefaultEmailType = "";
            cfWebDefaultsDataContract.CfwebEmailType = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.DefaultEmailType, string.Empty);
            Assert.AreEqual(cfWebDefaultsExpected.Result.DefaultEmailType, cfWebDefaultsActual.Result.DefaultEmailType);
        }
        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidDefaultShipToCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode, cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidDefaultShipToCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode = "";
            purDefaultsDataContract.PurShipToCode = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode, cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqGlRequired_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqGlRequired = false;
            cfWebDefaultsDataContract.CfwebReqGlRequired = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebReqGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqGlRequired = false;
            cfWebDefaultsDataContract.CfwebReqGlRequired = "";            
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqAllowMiscVendor_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebReqAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfWebReqDesiredDays()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqDesiredDays);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqDesiredDays, cfWebDefaultsActual.Result.CfWebReqDesiredDays);
        }
        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfWebReqDesiredDays()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqDesiredDays = null;
            cfWebDefaultsDataContract.CfwebReqDesiredDays = null;
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNull(cfWebDefaultsActual.Result.CfWebReqDesiredDays);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqDesiredDays, cfWebDefaultsActual.Result.CfWebReqDesiredDays);
        }

        private ColleagueFinanceWebConfigurationsRepository BuildMockColleagueFinanceWebConfigurationsRepository()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(cfWebDefaultsDataContract);
            });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(purDefaultsDataContract);
            });

            ColleagueFinanceWebConfigurationsRepository repository = new ColleagueFinanceWebConfigurationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return repository;
        }

    }
}
