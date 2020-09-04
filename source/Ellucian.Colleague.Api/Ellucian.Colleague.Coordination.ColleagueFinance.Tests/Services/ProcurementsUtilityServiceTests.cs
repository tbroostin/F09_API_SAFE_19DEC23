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
    public class ProcurementsUtilityServiceTests
    {

        protected Mock<IVendorCommodityRepository> vendorCommodityRepositoryMock;
        protected IVendorCommodityRepository vendorCommodityRepository;
        protected Mock<IVendorsRepository> vendorsRepositoryMock;
        protected IVendorsRepository vendorsRepository;
        protected Mock<ICommodityCodesService> commodityCodesServiceMock;
        protected ICommodityCodesService commodityCodesService;
        protected Mock<IColleagueFinanceWebConfigurationsRepository> colleagueFinanceWebConfigurationsRepoMock;
        protected IColleagueFinanceWebConfigurationsRepository colleagueFinanceWebConfigurationsRepo;
        protected Mock<IConfigurationRepository> configurationRepoMock;
        protected IConfigurationRepository configurationRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private VendorCommodity vendorCommodityEntity;
        private Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode procurementCommodityDto;
        private VendorDefaultTaxFormInfo vendorDefaultTaxInfoEnitity;
        protected ColleagueFinanceWebConfiguration colleagueFinanceWebConfigurationsEntity;
        protected ProcurementsUtilityService procurementsUtilityService;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ILogger logger;
        string commodityCode = "1";
        string vendorId = "1";
        string apType = "AP";

        [TestInitialize]
        public void Initialize()
        {
            vendorCommodityRepositoryMock = new Mock<IVendorCommodityRepository>();
            vendorCommodityRepository = vendorCommodityRepositoryMock.Object;
            vendorsRepositoryMock = new Mock<IVendorsRepository>();
            vendorsRepository = vendorsRepositoryMock.Object;
            commodityCodesServiceMock = new Mock<ICommodityCodesService>();
            commodityCodesService = commodityCodesServiceMock.Object;
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
            
            vendorCommodityEntity = new Domain.ColleagueFinance.Entities.VendorCommodity(vendorId)
            {
                StdPrice = 1M,
                StdPriceDate = DateTime.Now.Date
            };
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
                PurchasingDefaults = new PurchasingDefaults { DefaultShipToCode = "MC" },
                RequestPaymentDefaults = new VoucherWebConfiguration
                {
                    DefaultAPTypeCode = "AP",
                    AllowMiscVendor = true,
                    GlRequiredForVoucher = true,
                    IsInvoiceEntryRequired = true
                }
            };
            procurementCommodityDto = new Dtos.ColleagueFinance.ProcurementCommodityCode()
            {
                Code = "045",
                DefaultDescFlag = true,
                Description = "A4 Paper",
                FixedAssetsFlag = "S",
                Price = 1.50M,
                TaxCodes = new List<string> { "BD", "FL2" }
            };
            vendorDefaultTaxInfoEnitity = new VendorDefaultTaxFormInfo("1")
            {
                TaxForm = "1098T",
                TaxFormBoxCode = "AB",
                TaxFormState = "FL"
            };
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(colleagueFinanceWebConfigurationsEntity));
            commodityCodesServiceMock.Setup(r => r.GetCommodityCodeByCodeAsync(It.IsAny<string>())).Returns(Task.FromResult(procurementCommodityDto));
            vendorsRepositoryMock.Setup(r => r.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(vendorDefaultTaxInfoEnitity));
            vendorCommodityRepositoryMock.Setup(r => r.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(vendorCommodityEntity));
            procurementsUtilityService = new ProcurementsUtilityService(vendorsRepository, configurationRepo, adapterRegistry, currentUserFactory, roleRepo, commodityCodesService, vendorCommodityRepository, colleagueFinanceWebConfigurationsRepo, logger);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_ValidResult()
        {
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.AreEqual(procurementCommodityDto.FixedAssetsFlag, result.FixedAssetFlag);
            Assert.AreEqual(vendorDefaultTaxInfoEnitity.TaxFormBoxCode, result.BoxNo);
            Assert.AreEqual(vendorDefaultTaxInfoEnitity.TaxFormState, result.State);
            Assert.AreEqual(vendorCommodityEntity.StdPrice, result.StdPrice);
            Assert.AreEqual(4, result.TaxCodes.Count);
            Assert.AreEqual(vendorDefaultTaxInfoEnitity.TaxForm, result.TaxForm);
            Assert.AreEqual(procurementCommodityDto.Code, result.CommodityCode);
            Assert.AreEqual(procurementCommodityDto.Description, result.CommodityCodeDesc);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_CommodityCode_Argument_IsNullOrEmpty()
        {
            commodityCode = "";
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.AreEqual(colleagueFinanceWebConfigurationsEntity.DefaultTaxCodes.Count(), result.TaxCodes.Count);
            Assert.IsTrue(string.IsNullOrEmpty(result.FixedAssetFlag));
            Assert.IsTrue(string.IsNullOrEmpty(result.CommodityCode));
            Assert.IsTrue(string.IsNullOrEmpty(result.CommodityCodeDesc));
            Assert.IsFalse(result.StdPrice.HasValue);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_VendorId_Argument_IsNullOrEmpty()
        {
            vendorId = "";
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.BoxNo));
            Assert.IsTrue(string.IsNullOrEmpty(result.State));
            Assert.IsTrue(string.IsNullOrEmpty(result.TaxForm));
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_ApType_Argument_IsNullOrEmpty()
        {
            apType = "";
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TaxCodes.Count);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_VendorTaxFormInfo_IsNull()
        {
            VendorDefaultTaxFormInfo nullVendorDefaultTaxFormInfo = null;
            vendorsRepositoryMock.Setup(r => r.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(nullVendorDefaultTaxFormInfo));
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.BoxNo));
            Assert.IsTrue(string.IsNullOrEmpty(result.State));
            Assert.IsTrue(string.IsNullOrEmpty(result.TaxForm));
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_CommodityCode_IsNull()
        {
            Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode nullProcurementCommodityCode = null;
            commodityCodesServiceMock.Setup(r => r.GetCommodityCodeByCodeAsync(It.IsAny<string>())).Returns(Task.FromResult(nullProcurementCommodityCode));
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.FixedAssetFlag));
            Assert.IsTrue(string.IsNullOrEmpty(result.CommodityCode));
            Assert.IsTrue(string.IsNullOrEmpty(result.CommodityCodeDesc));
            Assert.IsFalse(result.StdPrice.HasValue);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_CfWebDefaults_IsNull()
        {
            ColleagueFinanceWebConfiguration nullColleagueFinanceWebConfiguration  = null;
            colleagueFinanceWebConfigurationsRepoMock.Setup(r => r.GetColleagueFinanceWebConfigurations()).Returns(Task.FromResult(nullColleagueFinanceWebConfiguration));
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.AreEqual(procurementCommodityDto.TaxCodes.Count, result.TaxCodes.Count);
        }

        [TestMethod]
        public async Task GetNewLineItemDefaultAdditionalInformation_VendorCommodity_IsNull()
        {
            VendorCommodity nullVendorCommodity = null;
            vendorCommodityRepositoryMock.Setup(r => r.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(nullVendorCommodity));
            var result = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(commodityCode, vendorId, apType);
            Assert.IsNotNull(result);
            Assert.AreEqual(procurementCommodityDto.Price, result.StdPrice);
        }
    }
}
