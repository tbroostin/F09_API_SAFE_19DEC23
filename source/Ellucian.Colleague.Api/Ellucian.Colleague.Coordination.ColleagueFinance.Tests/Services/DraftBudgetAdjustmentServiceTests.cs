// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
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
    public class DraftBudgetAdjustmentServiceTests
    {
        #region Initialize and Cleanup
        private DraftBudgetAdjustmentService service;
        private DraftBudgetAdjustmentService getDraftService;
        private DraftBudgetAdjustmentService deleteDraftService;
        private DraftBudgetAdjustmentService serviceForNoPermission;
        private DraftBudgetAdjustmentService nullDomainDraftService;

        private Mock<IDraftBudgetAdjustmentsRepository> repositoryMock = new Mock<IDraftBudgetAdjustmentsRepository>();
        private TestDraftBudgetAdjustmentRepository testDraftBudgetAdjustmentRepository;
        private Mock<IDraftBudgetAdjustmentsRepository> testDraftBaRepositoryDeleteMock;
        private Mock<IDraftBudgetAdjustmentsRepository> testDraftBaRepositoryNullDomainMock;
        private DraftBudgetAdjustment draftBudgetAdjustmentEntity;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewBudgetAdjustment;
        private Domain.Entities.Permission permissionCreateUpdateBudgetAdjustment;
        private Domain.Entities.Permission permissionDeleteBudgetAdjustment;
        protected Domain.Entities.Role glUserRoleAllPermissions = new Domain.Entities.Role(333, "Budget.Adjustor");
        protected Domain.Entities.Role glUserRoleDeletePermissions = new Domain.Entities.Role(334, "Budget.Adjustor.Delete");
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(335, "Budget.Adjustor.View");

        private GeneralLedgerCurrentUser.UserFactory userFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        private Domain.ColleagueFinance.Entities.DraftBudgetAdjustment adjustmentSuccessEntity;
        private Domain.ColleagueFinance.Entities.DraftBudgetAdjustment adjustmentErrorEntity;

        [TestInitialize]
        public void Initialize()
        {
            // Create permission domain entities for create/update, view and delete.
            permissionViewBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBudgetAdjustments);
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);
            permissionDeleteBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.DeleteBudgetAdjustments);
            // Assign all three permissions to the role that has all permissions.
            glUserRoleAllPermissions.AddPermission(permissionViewBudgetAdjustment);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);
            glUserRoleAllPermissions.AddPermission(permissionDeleteBudgetAdjustment);

            // Mock the role repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleAllPermissions });
            roleRepository = roleRepositoryMock.Object;

            // Build the service object.
            service = new DraftBudgetAdjustmentService(repositoryMock.Object, new Mock<IAdapterRegistry>().Object,
                userFactory, roleRepository, new Mock<ILogger>().Object);

            // Mock the return of the adjustment entity.
            repositoryMock.Setup(x => x.SaveAsync(It.IsAny<Domain.ColleagueFinance.Entities.DraftBudgetAdjustment>())).Returns((Domain.ColleagueFinance.Entities.DraftBudgetAdjustment adjustmentEntity) =>
            {
                if (adjustmentEntity.Initiator == null)
                {
                    adjustmentEntity = null;
                    return Task.FromResult(adjustmentEntity);
                }

                if (adjustmentEntity.Initiator == "noid")
                {
                    return Task.FromResult(adjustmentEntity);
                }

                if (adjustmentEntity.Initiator == "Pass")
                {
                    return Task.FromResult(adjustmentSuccessEntity);
                }
                return Task.FromResult(adjustmentErrorEntity);
            });

            // Mock the repository for the delete a draft.
            testDraftBudgetAdjustmentRepository = new TestDraftBudgetAdjustmentRepository();
            testDraftBaRepositoryDeleteMock = new Mock<IDraftBudgetAdjustmentsRepository>();
            testDraftBaRepositoryNullDomainMock = new Mock<IDraftBudgetAdjustmentsRepository>();

            draftBudgetAdjustmentEntity = testDraftBudgetAdjustmentRepository.GetAsync("1").Result;

            testDraftBaRepositoryDeleteMock.Setup(y => y.GetAsync(It.IsAny<string>())).ReturnsAsync(draftBudgetAdjustmentEntity);
            testDraftBaRepositoryDeleteMock.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(Task.FromResult(""));
            testDraftBaRepositoryNullDomainMock.Setup(y => y.GetAsync(It.IsAny<string>())).ReturnsAsync(null as DraftBudgetAdjustment);

            var loggerMock = new Mock<ILogger>().Object;
            var draftAdapterRegistryMock = new Mock<IAdapterRegistry>();
            var draftEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.DraftBudgetAdjustment, Dtos.ColleagueFinance.DraftBudgetAdjustment>(draftAdapterRegistryMock.Object, loggerMock);
            draftAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.DraftBudgetAdjustment, Dtos.ColleagueFinance.DraftBudgetAdjustment>()).Returns(draftEntityToDtoAdapter);
            var adjustmentLineToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.DraftAdjustmentLine, Dtos.ColleagueFinance.DraftAdjustmentLine>(draftAdapterRegistryMock.Object, loggerMock);
            draftAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.DraftAdjustmentLine, Dtos.ColleagueFinance.DraftAdjustmentLine>()).Returns(adjustmentLineToDtoAdapter);
            var nextApproverToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>(draftAdapterRegistryMock.Object, loggerMock);
            draftAdapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>()).Returns(nextApproverToDtoAdapter);

            // Build a service to delete draft budget adjustments.
            deleteDraftService = new DraftBudgetAdjustmentService(testDraftBaRepositoryDeleteMock.Object, draftAdapterRegistryMock.Object, userFactory, roleRepository, loggerMock);

            // Build a service to get a draft budgt adjustment.
            getDraftService = new DraftBudgetAdjustmentService(testDraftBudgetAdjustmentRepository, draftAdapterRegistryMock.Object, userFactory, roleRepository, loggerMock);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new DraftBudgetAdjustmentService(testDraftBaRepositoryDeleteMock.Object, draftAdapterRegistryMock.Object, noPermissionsUser, roleRepository, loggerMock);

            // Build a service for an empty budget adjustment domain entity.
            nullDomainDraftService = new DraftBudgetAdjustmentService(testDraftBaRepositoryNullDomainMock.Object, draftAdapterRegistryMock.Object, userFactory, roleRepository, loggerMock);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            deleteDraftService = null;
            getDraftService = null;
            serviceForNoPermission = null;

            adjustmentSuccessEntity = null;
            adjustmentErrorEntity = null;
            draftBudgetAdjustmentEntity = null;

            testDraftBaRepositoryDeleteMock = null;
            testDraftBudgetAdjustmentRepository = null;
            testDraftBaRepositoryNullDomainMock = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleAllPermissions = null;
            glUserRoleDeletePermissions = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region CreateDraftBudgetAdjustmentAsync
        [TestMethod]
        public async Task CreateDraftBudgetAdjustmentAsync_Success()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.DraftAdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.DraftBudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.DraftAdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.DraftAdjustmentLine() { GlNumber = adjustmentLineDto.GlNumber, FromAmount = adjustmentLineDto.FromAmount, ToAmount = adjustmentLineDto.ToAmount });
            }

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.DraftBudgetAdjustment(inputDto.Reason);
            adjustmentSuccessEntity.AdjustmentLines = adjustmentLines;
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;
            adjustmentSuccessEntity.TransactionDate = inputDto.TransactionDate;
            adjustmentSuccessEntity.Id = "1";

            var adjustmentDto = await service.SaveDraftBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);

            foreach (var adjustmentLineDto in inputDto.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateDraftBudgetAdjustmentAsync_NullInputDto()
        {
            var expectedParam = "draftBudgetAdjustmentDto";
            var actualParam = "";
            try
            {
                await service.SaveDraftBudgetAdjustmentAsync(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_RepositoryReturnsNull()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.DraftAdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.DraftBudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = null,
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.DraftAdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.DraftAdjustmentLine() { GlNumber = adjustmentLineDto.GlNumber, FromAmount = adjustmentLineDto.FromAmount, ToAmount = adjustmentLineDto.ToAmount });
            }

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.DraftBudgetAdjustment(inputDto.Reason);
            adjustmentSuccessEntity.AdjustmentLines = adjustmentLines;
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var expectedMessage = "Draft budget adjustment must not be null.";
            var actualMessage = "";
            try
            {
                await service.SaveDraftBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_RepositoryReturnsEntityWithNoId()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.DraftAdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.DraftBudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "noid",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.DraftAdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.DraftAdjustmentLine() { GlNumber = adjustmentLineDto.GlNumber, FromAmount = adjustmentLineDto.FromAmount, ToAmount = adjustmentLineDto.ToAmount });
            }

            var expectedMessage = "Adjustment appears to have succeeded, but no ID was returned.";
            var actualMessage = "";
            try
            {
                await service.SaveDraftBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateDraftBudgetAdjustmentAsync_FailErrorMessages()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.DraftAdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.DraftAdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.DraftBudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "Fail",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.DraftAdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.DraftAdjustmentLine() { GlNumber = adjustmentLineDto.GlNumber, FromAmount = adjustmentLineDto.FromAmount, ToAmount = adjustmentLineDto.ToAmount });
            }

            adjustmentErrorEntity = new Domain.ColleagueFinance.Entities.DraftBudgetAdjustment(inputDto.Reason);
            adjustmentErrorEntity.AdjustmentLines = adjustmentLines;
            adjustmentErrorEntity.Comments = inputDto.Comments;
            adjustmentErrorEntity.Initiator = inputDto.Initiator;
            adjustmentErrorEntity.ErrorMessages = new List<string>() { "Test error message 1.", "Test error message 2." };

            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.SaveDraftBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(String.Join("<>", adjustmentErrorEntity.ErrorMessages), actualMessage);
        }
        #endregion

        #region Get a draft budget adjustment

        [TestMethod]
        public async Task GetAsync_Success()
        {

            var draftBudgetAdjustmentDto = await getDraftService.GetAsync("2");
            var draftBudgetAdjustmentDomain = await testDraftBudgetAdjustmentRepository.GetAsync("2");

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(draftBudgetAdjustmentDto.Comments, draftBudgetAdjustmentDomain.Comments);
            Assert.AreEqual(draftBudgetAdjustmentDto.Id, draftBudgetAdjustmentDomain.Id);
            Assert.AreEqual(draftBudgetAdjustmentDto.Reason, draftBudgetAdjustmentDomain.Reason);
            Assert.AreEqual(draftBudgetAdjustmentDto.Initiator, draftBudgetAdjustmentDomain.Initiator);
            Assert.AreEqual(draftBudgetAdjustmentDto.TransactionDate, draftBudgetAdjustmentDomain.TransactionDate);

            foreach (var adjustmentLineDto in draftBudgetAdjustmentDto.AdjustmentLines)
            {
                var matchingLine = draftBudgetAdjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_PermissionException()
        {
            await serviceForNoPermission.GetAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullRecordId()
        {
            await getDraftService.GetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyRecordId()
        {
            await getDraftService.GetAsync("");
        }

        #endregion

        #region Delete Draft Budget Adjustment

        [TestMethod]
        public async Task DeleteAsync_Success()
        {
            bool exceptionThrown = false;
            try
            {
                await deleteDraftService.DeleteAsync("1");
            }
            catch
            {

                exceptionThrown = true;
            }
            Assert.IsFalse(exceptionThrown);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task DeleteAsync_PermissionException()
        {
            await serviceForNoPermission.DeleteAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task DeleteAsync_DifferentUserException()
        {
            draftBudgetAdjustmentEntity.PersonId = "9999999";
            await deleteDraftService.DeleteAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task DeleteAsync_NullDomainEntiyException()
        {
            await nullDomainDraftService.DeleteAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_NullRecordId()
        {
            await deleteDraftService.DeleteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_EmptyRecordId()
        {
            await deleteDraftService.DeleteAsync("");

        }
        #endregion
    }
}
