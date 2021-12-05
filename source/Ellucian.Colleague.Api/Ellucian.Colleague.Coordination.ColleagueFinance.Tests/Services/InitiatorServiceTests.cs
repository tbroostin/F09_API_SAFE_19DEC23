//Copyright 2020 Ellucian Company L.P.and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
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
    public class InitiatorServiceTests : GeneralLedgerCurrentUser
    {
        #region DECLARATION

        protected Domain.Entities.Role createUpdateDoc = new Domain.Entities.Role(1, "CREATE.UPDATE.DOC");

        private Mock<IInitiatorRepository> initiatorRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> financeReferenceDataRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private Mock<ILogger> loggerMock;

        private InitiatorProcurementUser currentUserFactory;

        private InitiatorService initiatorService;

        private Initiator initiator;

        private IEnumerable<Initiator> initiatorCollection;

        private Domain.ColleagueFinance.Entities.Initiator initiatorDomainEntity;

        private IEnumerable<Domain.ColleagueFinance.Entities.Initiator> initiatorDomainEntities;

        private Staff staffEntity;

        private IEnumerable<Domain.Entities.Role> roles;

        private string personId = "0000143";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {

            initiatorRepositoryMock = new Mock<IInitiatorRepository>();
            financeReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            staffRepositoryMock = new Mock<IStaffRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new InitiatorProcurementUser();

            InitializeTestData();

            InitializeTestMock();

            initiatorService = new InitiatorService(initiatorRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, staffRepositoryMock.Object, loggerMock.Object);
        }

        private void InitializeTestData()
        {
            initiatorDomainEntity = new Domain.ColleagueFinance.Entities.Initiator("123", "John Doe", "JHN");

            initiatorDomainEntities = new List<Domain.ColleagueFinance.Entities.Initiator>() { initiatorDomainEntity };

            staffEntity = new Staff("0000143", "Jef") { };

            roles = new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "CREATE.UPDATE.DOC") };

            roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.CreateUpdateRequisition));
        }

        private void InitializeTestMock()
        {
            // Set up and mock the adapter, and setup the GetAdapter method.
            var initiatorDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Initiator, Dtos.ColleagueFinance.Initiator>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Initiator, Dtos.ColleagueFinance.Initiator>()).Returns(initiatorDtoAdapter);
            createUpdateDoc.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateRequisition));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateDoc });
            roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
            staffRepositoryMock.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(staffEntity);
            initiatorRepositoryMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ReturnsAsync(initiatorDomainEntities);

        }

        #endregion

        #region TEST METHODS

            #region GET

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InitiatorService_QueryInitiatorByKeywordAsync_QueryKeyword_Null()
        {
            await initiatorService.QueryInitiatorByKeywordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InitiatorService_QueryInitiatorByKeywordAsync_QueryKeyword_Empty()
        {
            await initiatorService.QueryInitiatorByKeywordAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task InitiatorService_QueryInitiatorByKeywordAsync_PermissionsException()
        {
            roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"CREATE.UPDTAE.D")
                    };

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            await initiatorService.QueryInitiatorByKeywordAsync(personId);
        }

        [TestMethod]
        public async Task InitiatorService_QueryInitiatorByKeywordAsync_NullResult()
        {
            initiatorRepositoryMock.Setup(r => r.QueryInitiatorByKeywordAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await initiatorService.QueryInitiatorByKeywordAsync(personId);
        }

        [TestMethod]
        public async Task InitiatorService_QueryInitiatorByKeywordAsync()
        {
            var Id = "123";
            var initiatorDtos = await initiatorService.QueryInitiatorByKeywordAsync(personId);

            Assert.IsNotNull(initiatorDtos);
            Assert.AreEqual(initiatorDtos.ToList().Count, initiatorDomainEntities.ToList().Count);

            var initiatorsDto = initiatorDtos.Where(x => x.Id == Id).FirstOrDefault();
            var initiatorDomainEntity = initiatorDomainEntities.Where(x => x.Id == Id).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(initiatorsDto.Id, initiatorDomainEntity.Id);
            Assert.AreEqual(initiatorsDto.Name, initiatorDomainEntity.Name);
            Assert.AreEqual(initiatorsDto.Code, initiatorDomainEntity.Code);
        }


        #endregion

        #endregion

    }
}
