// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ApproverServiceTests
    {
        #region Initialize and Cleanup

        private ApproverService service = null;
        private TestApproverRepository testApproverRepository = new TestApproverRepository();
        private GeneralLedgerCurrentUser.UserFactory userFactory = new GeneralLedgerCurrentUser.UserFactory();

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;
        private Domain.Entities.Permission permissionCreateUpdateBudgetAdjustment;
        protected Domain.Entities.Role glUserRoleAllPermissions = new Domain.Entities.Role(333, "Budget.Adjustor");

        private Domain.ColleagueFinance.Entities.NextApprover nextApproverDomainEntity;

        private IEnumerable<Domain.ColleagueFinance.Entities.NextApprover> nextApproverDomainEntities;

        private Mock<IApproverRepository> approverRepositoryMock;

        private string personId = "0000143";

        [TestInitialize]
        public void Initialize()
        {
            // Use Mock to create mock implementations that are based on the same interfaces
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);

            // Mock the repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            approverRepositoryMock = new Mock<IApproverRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleAllPermissions });
            roleRepository = roleRepositoryMock.Object;

            nextApproverDomainEntity = new Domain.ColleagueFinance.Entities.NextApprover("JHN") { NextApproverPersonId = "123" };
            nextApproverDomainEntities = new List<Domain.ColleagueFinance.Entities.NextApprover>() { nextApproverDomainEntity };
            approverRepositoryMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(nextApproverDomainEntities);

            BuildService(testApproverRepository, userFactory);

            approverRepositoryMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(nextApproverDomainEntities);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testApproverRepository = null;
            userFactory = null;
            roleRepositoryMock = null;
            roleRepository = null;
        }
        #endregion

        #region Validate next approver

        [TestMethod]
        public async Task ValidateApproverAsync_Success()
        {
            string nextApproverId = "BOB1";
            var approverValidationResponse = await testApproverRepository.ValidateApproverAsync(nextApproverId);
            var nextApproverValidationResponseDto = await service.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponse.Id, nextApproverValidationResponseDto.Id);
            Assert.AreEqual(approverValidationResponse.ApproverName, nextApproverValidationResponseDto.NextApproverName);
            Assert.AreEqual(approverValidationResponse.IsValid, nextApproverValidationResponseDto.IsValid);
            Assert.AreEqual(approverValidationResponse.ErrorMessage, nextApproverValidationResponseDto.ErrorMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateApproverAsync_NullId()
        {
            var nextApproverValidationResponseDto = await service.ValidateApproverAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateApproverAsync_EmptyId()
        {
            var nextApproverValidationResponseDto = await service.ValidateApproverAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ValidateApproverAsync_NextApproverValidationResponseIsNull()
        {
            ApproverValidationResponse approverValidationResponse = null;
            var approverRepositoryMock = new Mock<IApproverRepository>();
            approverRepositoryMock.Setup(x => x.ValidateApproverAsync("")).Returns(() =>
            {
                return Task.FromResult(approverValidationResponse);
            });
            BuildService(testApproverRepository, userFactory);

            var nextApproverValidationResponseDto = await service.ValidateApproverAsync("BOB3");
        }

        #endregion

        #region Private methods

        private void BuildService(IApproverRepository testApproverRepository, ICurrentUserFactory userFactory)
        {
            nextApproverDomainEntity = new Domain.ColleagueFinance.Entities.NextApprover("JHN") { NextApproverPersonId ="123"};

            nextApproverDomainEntities = new List<Domain.ColleagueFinance.Entities.NextApprover>() { nextApproverDomainEntity };

            var loggerObject = new Mock<ILogger>().Object;
            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var nextApproverValidationResponseAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApproverValidationResponse, Dtos.ColleagueFinance.NextApproverValidationResponse>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApproverValidationResponse, Dtos.ColleagueFinance.NextApproverValidationResponse>()).Returns(nextApproverValidationResponseAdapter);
            
            var nextApproverAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>()).Returns(nextApproverAdapter);


            // Set up the service
            service = new ApproverService(testApproverRepository, adapterRegistry.Object, userFactory, roleRepository, loggerObject);
        }
        #endregion

        #region Search Next Approver

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NextApproverService_QueryNextApproverByKeywordAsync_QueryKeyword_Null()
        {
            await service.QueryNextApproverByKeywordAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NextApproverService_QueryNextApproverByKeywordAsync_QueryKeyword_Empty()
        {
            await service.QueryNextApproverByKeywordAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NextApproverService_QueryNextApproverByKeywordAsync_PermissionsException()
        {

            List<Role> roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"CREATE.UPDTAE.D")
                    };

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            await service.QueryNextApproverByKeywordAsync(personId);
        }

        [TestMethod]
        public async Task NextApproverService_QueryNextApproverByKeywordAsync_NullResult()
        {
            var Id = "123";
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);

            testApproverRepository.nextApprovers = null;
            
            var nextApproverDto = await service.QueryNextApproverByKeywordAsync(Id);
            
            Assert.AreEqual(nextApproverDto.ToList().Count,0);

        }

        [TestMethod]
        public async Task NextApproverService_QueryNextApproverByKeywordAsync()
        {
            var Id = "123";
            
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);
            approverRepositoryMock.Setup(r => r.QueryNextApproverByKeywordAsync(It.IsAny<string>())).ReturnsAsync(nextApproverDomainEntities);
            var approverDtos = await service.QueryNextApproverByKeywordAsync(Id);
            
            Assert.IsNotNull(approverDtos);
            Assert.AreEqual(approverDtos.ToList().Count, nextApproverDomainEntities.ToList().Count);

            var approversDto = approverDtos.Where(x => x.NextApproverPersonId == Id).FirstOrDefault();
            var approverDomainEntity = nextApproverDomainEntities.Where(x => x.NextApproverPersonId == Id).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(approversDto.NextApproverPersonId, approverDomainEntity.NextApproverPersonId);
            Assert.AreEqual(approversDto.NextApproverId, approverDomainEntity.NextApproverId);
        }

        #endregion
    }
}