// Copyright 2018 Ellucian Company L.P. and its affiliates.

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

        [TestInitialize]
        public void Initialize()
        {
            // Use Mock to create mock implementations that are based on the same interfaces
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);

            // Mock the repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleAllPermissions });
            roleRepository = roleRepositoryMock.Object;

            BuildService(testApproverRepository, userFactory);
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
            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var nextApproverValidationResponseAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.ApproverValidationResponse, Dtos.ColleagueFinance.NextApproverValidationResponse>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.ApproverValidationResponse, Dtos.ColleagueFinance.NextApproverValidationResponse>()).Returns(nextApproverValidationResponseAdapter);

            // Set up the service
            service = new ApproverService(testApproverRepository, adapterRegistry.Object, userFactory, roleRepository, loggerObject);
        }
        #endregion
    }
}