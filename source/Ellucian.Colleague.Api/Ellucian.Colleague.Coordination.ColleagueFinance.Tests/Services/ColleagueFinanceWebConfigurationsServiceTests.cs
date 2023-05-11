// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ColleagueFinanceWebConfigurationsServiceTests
    {

        protected Mock<IColleagueFinanceWebConfigurationsRepository> colleagueFinanceWebConfigurationsRepoMock;
        protected Mock<IConfigurationRepository> configurationRepoMock;
        protected IColleagueFinanceWebConfigurationsRepository colleagueFinanceWebConfigurationsRepo;
        protected IConfigurationRepository configurationRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ColleagueFinanceWebConfiguration colleagueFinanceWebConfigurationsEntity;
        protected ColleagueFinanceWebConfigurationsService colleagueFinanceWebConfigurationsService;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            
            colleagueFinanceWebConfigurationsRepoMock = new Mock<IColleagueFinanceWebConfigurationsRepository>();
            colleagueFinanceWebConfigurationsRepo = colleagueFinanceWebConfigurationsRepoMock.Object;
            configurationRepoMock = new Mock<IConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            currentUserFactory = currentUserFactoryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            colleagueFinanceWebConfigurationsEntity = new Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration()
            {
                DefaultEmailType = "PRI",
                CfWebReqAllowMiscVendor = true,
                CfWebReqDesiredDays = 7,
                CfWebReqGlRequired = true,
                DefaultAPTypeCode = "AP",
                CfWebPoGlRequired = true,
                CfWebPoAllowMiscVendor = true,
                DefaultTaxCodes = new List<string> { "PS", "GS" },
                PurchasingDefaults = new PurchasingDefaults { DefaultShipToCode = "MC", IsApprovalReturnsEnabled = true },
                RequestPaymentDefaults = new VoucherWebConfiguration {
                    DefaultAPTypeCode = "AP",
                    AllowMiscVendor = true,
                    GlRequiredForVoucher= true,
                    IsInvoiceEntryRequired = true
                }
            };
            var cfWebConfigurationAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration, Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration>(adapterRegistryMock.Object, logger);
            var purchasingDefaultsAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.PurchasingDefaults, Dtos.ColleagueFinance.PurchasingDefaults>(adapterRegistryMock.Object, logger);
            var voucherWebConfigAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VoucherWebConfiguration, Dtos.ColleagueFinance.VoucherWebConfiguration>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.ColleagueFinance.Entities.PurchasingDefaults, Dtos.ColleagueFinance.PurchasingDefaults>()).Returns(purchasingDefaultsAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.ColleagueFinance.Entities.VoucherWebConfiguration, Dtos.ColleagueFinance.VoucherWebConfiguration>()).Returns(voucherWebConfigAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration, Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration>()).Returns(cfWebConfigurationAdapter);
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            colleagueFinanceWebConfigurationsService = new ColleagueFinanceWebConfigurationsService(colleagueFinanceWebConfigurationsRepo, configurationRepo, adapterRegistry, currentUserFactory, roleRepo, logger);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_ValidResult()
        {
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.DefaultEmailType, result.DefaultEmailType);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqDesiredDays, result.CfWebReqDesiredDays);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqAllowMiscVendor, result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqGlRequired, result.CfWebReqGlRequired);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.DefaultTaxCodes.Count(), result.DefaultTaxCodes.Count());
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.DefaultAPTypeCode, result.DefaultAPTypeCode);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.PurchasingDefaults.DefaultShipToCode, result.PurchasingDefaults.DefaultShipToCode);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.PurchasingDefaults.IsApprovalReturnsEnabled, result.PurchasingDefaults.IsApprovalReturnsEnabled);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.VoucherAttachmentCollectionId, result.VoucherAttachmentCollectionId);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.PurchaseOrderAttachmentCollectionId, result.PurchaseOrderAttachmentCollectionId);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.RequisitionAttachmentCollectionId, result.RequisitionAttachmentCollectionId);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.AreVoucherAttachmentsRequired, result.AreVoucherAttachmentsRequired);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_DefaultShipToCode_NotSet()
        {
            colleagueFinanceWebConfigurationsEntity.PurchasingDefaults.DefaultShipToCode = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.PurchasingDefaults.DefaultShipToCode);
        }
        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_DefaultAPTypeCode_NotSet()
        {
            colleagueFinanceWebConfigurationsEntity.DefaultAPTypeCode = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.DefaultAPTypeCode);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_DefaultTaxCodes_NotSet()
        {
            colleagueFinanceWebConfigurationsEntity.DefaultTaxCodes = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.DefaultTaxCodes.Count());
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebReqGlRequired_False()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebReqGlRequired = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqGlRequired, result.CfWebReqGlRequired);
        }


        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebReqDesiredDays_NotSet()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebReqDesiredDays = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.CfWebReqDesiredDays);
        }


        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebReqDesiredDays_Zero()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebReqDesiredDays = 0;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqDesiredDays, result.CfWebReqDesiredDays);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebReqAllowMiscVendor_False()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebReqAllowMiscVendor = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqAllowMiscVendor, result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebPoGlRequired_False()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebReqGlRequired = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebReqGlRequired, result.CfWebReqGlRequired);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_CfWebPoAllowMiscVendor_False()
        {
            colleagueFinanceWebConfigurationsEntity.CfWebPoAllowMiscVendor = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.CfWebPoAllowMiscVendor, result.CfWebPoAllowMiscVendor);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_RequestPaymentDefaults_GlRequiredForVoucher_False()
        {
            colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.GlRequiredForVoucher = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.GlRequiredForVoucher, result.RequestPaymentDefaults.GlRequiredForVoucher);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_RequestPaymentDefaults_AllowMiscVendor_False()
        {
            colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.AllowMiscVendor = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.AllowMiscVendor, result.RequestPaymentDefaults.AllowMiscVendor);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_RequestPaymentDefaults_DefaultVoucherAPTypeCode_NotSet()
        {
            colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.DefaultAPTypeCode = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.RequestPaymentDefaults.DefaultAPTypeCode);
        }

        [TestMethod]
        public async Task GetColleagueFinanceWebConfigurations_RequestPaymentDefaults_IsInvoiceEntryRequired_False()
        {
            colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.IsInvoiceEntryRequired = false;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            var result = await colleagueFinanceWebConfigurationsService.GetColleagueFinanceWebConfigurationsAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.RequestPaymentDefaults.IsInvoiceEntryRequired, result.RequestPaymentDefaults.IsInvoiceEntryRequired);
        }
    }
}
