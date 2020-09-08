// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class VendorCommodityServiceTests : CurrentUserSetup
    {
        #region DECLARATION
        Mock<IVendorCommodityRepository> vendorCommodityRepositoryMock;
        Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IReferenceDataRepository> refDataRepositoryMock;
        Mock<IPersonRepository> personRepositoryMock;
        Mock<IAddressRepository> addressRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IInstitutionRepository> institutionRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        private IEnumerable<Domain.Entities.Role> roles;

        VendorCommodityService vendorCommodityService;

        Domain.ColleagueFinance.Entities.VendorCommodity vendorCommodityEntity;
        private string vendorId;
        private string commodityCode;


        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Domain.Entities.Permission permissionViewAnyPerson;

        #endregion

        #region SET UP 

        [TestInitialize]
        public void Initialize()
        {
            vendorCommodityRepositoryMock = new Mock<IVendorCommodityRepository>();
            referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            refDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            addressRepositoryMock = new Mock<IAddressRepository>();
            institutionRepositoryMock = new Mock<IInstitutionRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            BuildData();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVendor);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            vendorCommodityService = new VendorCommodityService(baseConfigurationRepository, adapterRegistryMock.Object,
                currentUserFactory, roleRepositoryMock.Object,
                vendorCommodityRepositoryMock.Object,
                loggerMock.Object);

            // Set up and mock the adapter, and setup the GetAdapter method.
            var vendorCommodityDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorCommodity, Dtos.ColleagueFinance.VendorCommodity>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VendorCommodity, Dtos.ColleagueFinance.VendorCommodity>()).Returns(vendorCommodityDtoAdapter);


        }


        private void BuildData()
        {
            vendorId = "0000192";
            commodityCode = "10900";
            var id = vendorId + "*" + commodityCode;
            vendorCommodityEntity = new Domain.ColleagueFinance.Entities.VendorCommodity(id)
            {
                StdPrice = 10,
                StdPriceDate = DateTime.Today
            };
            vendorCommodityRepositoryMock.Setup(i => i.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorCommodityEntity);

            roles = new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "VIEW.VENDOR") };

            roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.ViewVendor));
        }

        #endregion

        #region Clean Up
        [TestCleanup]
        public void Cleanup()
        {
            vendorCommodityEntity = null;
            vendorCommodityRepositoryMock = null;
            referenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            institutionRepositoryMock = null;
        }
        #endregion

        #region GetVendorCommodityAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVendorCommodityAsync_SearchCriteria_Null()
        {
            await vendorCommodityService.GetVendorCommodityAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVendorCommodityAsync_SearchCriteria_Empty()
        {
            await vendorCommodityService.GetVendorCommodityAsync("", "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVendorCommodityAsync_PermissionException()
        {
            personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            await vendorCommodityService.GetVendorCommodityAsync(vendorId, commodityCode);
        }

        [TestMethod]
        public async Task GetVendorCommodityAsync_Repository_ReturnsNull()
        {
            vendorCommodityRepositoryMock.Setup(i => i.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);
            var resultDto = await vendorCommodityService.GetVendorCommodityAsync(vendorId, commodityCode);
            Assert.IsNotNull(resultDto);
            Assert.IsNull(resultDto.Id);
            Assert.IsNull(resultDto.StdPrice);
            Assert.IsNull(resultDto.StdPriceDate);
        }

        [TestMethod]
        public async Task GetVendorCommodityAsync_Repository_ReturnsResult()
        {
            vendorCommodityRepositoryMock.Setup(i => i.GetVendorCommodityAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(this.vendorCommodityEntity);
            var resultDto = await vendorCommodityService.GetVendorCommodityAsync(vendorId, commodityCode);

            Assert.AreEqual(resultDto.Id, vendorCommodityEntity.Id);
            Assert.AreEqual(resultDto.StdPrice, vendorCommodityEntity.StdPrice);
            Assert.AreEqual(resultDto.StdPriceDate, vendorCommodityEntity.StdPriceDate);
        }
        #endregion
    }
}