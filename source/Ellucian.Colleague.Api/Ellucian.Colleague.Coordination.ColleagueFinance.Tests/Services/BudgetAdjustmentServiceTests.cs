// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    public class BudgetAdjustmentServiceTests
    {
        #region Initialize and Cleanup
        private BudgetAdjustmentService service;
        private BudgetAdjustmentService baSummaryService;
        private BudgetAdjustmentService budgetAdjustmentService;
        private BudgetAdjustmentService service3;
        private BudgetAdjustmentService serviceForViewPermission;
        private BudgetAdjustmentService serviceForNoPermission;

        private Mock<IBudgetAdjustmentsRepository> repositoryMock = new Mock<IBudgetAdjustmentsRepository>();
        private Mock<IBudgetAdjustmentsRepository> repositoryMock2 = new Mock<IBudgetAdjustmentsRepository>();
        private Mock<IGeneralLedgerConfigurationRepository> configurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();

        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock1;
        private IRoleRepository roleRepository;
        private IRoleRepository roleRepository1;

        private Domain.Entities.Permission permissionViewBudgetAdjustment;
        private Domain.Entities.Permission permissionCreateUpdateBudgetAdjustment;
        private Domain.Entities.Permission permissionDeleteBudgetAdjustment;
        protected Domain.Entities.Role glUserRoleAllPermissions = new Domain.Entities.Role(333, "Budget.Adjustor");
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(334, "Budget.Adjustor.View");

        private Domain.ColleagueFinance.Entities.BudgetAdjustment adjustmentSuccessEntity;
        private Domain.ColleagueFinance.Entities.BudgetAdjustment adjustmentErrorEntity;
        private CostCenterStructure costCenterStructure = new CostCenterStructure();
        private BudgetAdjustmentAccountExclusions exclusions = new BudgetAdjustmentAccountExclusions();
        private BudgetAdjustmentParameters budgetAdjustmentParameters = new BudgetAdjustmentParameters(false, false, false);
        private GeneralLedgerClassConfiguration glClassConfiguration;
        private TestBudgetAdjustmentRepository testBudgetAdjustmentRepository;
        private string personId = "0000004";

        private GeneralLedgerCurrentUser.UserFactory userFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryAll userFactory2 = new GeneralLedgerCurrentUser.UserFactoryAll();
        private GeneralLedgerCurrentUser.BudgetAdjustmentViewUser baViewUserFactory = new GeneralLedgerCurrentUser.BudgetAdjustmentViewUser();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

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
            // Assign the view permission to the role that only has the view permission.
            glUserRoleViewPermissions.AddPermission(permissionViewBudgetAdjustment);

            // Mock the repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleAllPermissions });
            roleRepository = roleRepositoryMock.Object;
            // Mock the repository for the role that has only view permissions.
            roleRepositoryMock1 = new Mock<IRoleRepository>();
            roleRepositoryMock1.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository1 = roleRepositoryMock1.Object;

            configurationRepositoryMock.Setup(x => x.GetBudgetAdjustmentParametersAsync()).Returns(() =>
            {
                return Task.FromResult(budgetAdjustmentParameters);
            });

            // Build the service object.
            service = new BudgetAdjustmentService(repositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object,
                userFactory, roleRepository, new Mock<ILogger>().Object);

            // Mock the cost center structure.
            var testGlAccountRepository = new TestGlAccountRepository();
            costCenterStructure = new CostCenterStructure();
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.LOCATION_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.UNIT_CODE));
            costCenterStructure.AddCostCenterComponent(testGlAccountRepository.GlComponents.FirstOrDefault(x => x.ComponentName == TestGlAccountRepository.OBJECT_CODE));
            configurationRepositoryMock.Setup(x => x.GetCostCenterStructureAsync()).Returns(() =>
            {
                return Task.FromResult(costCenterStructure);
            });

            // Mock the budget adjustment exclusions.
            exclusions = new BudgetAdjustmentAccountExclusions();
            exclusions.ExcludedElements = new List<BudgetAdjustmentExcludedElement>();
            var excludedElement = new BudgetAdjustmentExcludedElement();
            excludedElement.ExclusionComponent = new GeneralLedgerComponent("Object", false, GeneralLedgerComponentType.Object, "19", "5");
            excludedElement.ExclusionRange = new GeneralLedgerComponentRange("75000", "75100");
            exclusions.ExcludedElements.Add(excludedElement);
            configurationRepositoryMock.Setup(x => x.GetBudgetAdjustmentAccountExclusionsAsync()).Returns(() =>
            {
                return Task.FromResult(exclusions);
            });
            // Mock the GL class configuration.
            glClassConfiguration = new GeneralLedgerClassConfiguration("GL.CLASS", new List<string>() { "5", "7" }, new List<string>() { "4" }, new List<string>() { "1" }, new List<string>() { "2" }, new List<string>() { "3" });
            glClassConfiguration.GlClassStartPosition = 18;
            glClassConfiguration.GlClassLength = 1;
            configurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).Returns(() =>
            {
                return Task.FromResult(glClassConfiguration);
            });

            // Mock the return of the adjustment entity.
            repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).Returns((Domain.ColleagueFinance.Entities.BudgetAdjustment adjustmentEntity) =>
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

            this.repositoryMock2 = new Mock<IBudgetAdjustmentsRepository>();
            testBudgetAdjustmentRepository = new TestBudgetAdjustmentRepository();

            var loggerObject = new Mock<ILogger>().Object;
            // Set up and mock the adapter, and the GetAdapter method for the budgetAdjustmentSummary.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var entityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentSummary, Dtos.ColleagueFinance.BudgetAdjustmentSummary>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentSummary, Dtos.ColleagueFinance.BudgetAdjustmentSummary>()).Returns(entityToDtoAdapter);

            var BudgetAdjustmentAdapterRegistry = new Mock<IAdapterRegistry>();
            var budgetAdjustmentEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>(BudgetAdjustmentAdapterRegistry.Object, loggerObject);
            BudgetAdjustmentAdapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>()).Returns(budgetAdjustmentEntityToDtoAdapter);
            var adjustmentLineToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.AdjustmentLine, Dtos.ColleagueFinance.AdjustmentLine>(BudgetAdjustmentAdapterRegistry.Object, loggerObject);
            BudgetAdjustmentAdapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.AdjustmentLine, Dtos.ColleagueFinance.AdjustmentLine>()).Returns(adjustmentLineToDtoAdapter);

            var nextApproverToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>(new Mock<IAdapterRegistry>().Object, loggerObject);
            BudgetAdjustmentAdapterRegistry.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>()).Returns(nextApproverToEntityAdapter);
            var nextApproverToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>(new Mock<IAdapterRegistry>().Object, loggerObject);
            BudgetAdjustmentAdapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>()).Returns(nextApproverToDtoAdapter);

            var approverToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(new Mock<IAdapterRegistry>().Object, loggerObject);
            BudgetAdjustmentAdapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>()).Returns(approverToDtoAdapter);

            // Build the service for budget adjustment summary information object.
            baSummaryService = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, adapterRegistry.Object,
                       userFactory, roleRepository, loggerObject);

            // Build the budgetAdjustmentService object.
            budgetAdjustmentService = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, BudgetAdjustmentAdapterRegistry.Object,
                       userFactory, roleRepository, loggerObject);

            userFactory2 = new GeneralLedgerCurrentUser.UserFactoryAll();
            // Setup the service that is going to make currentUser.PersonId different for the security test.
            service3 = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, adapterRegistry.Object,
                       userFactory2, roleRepository, loggerObject);

            // Build a service for a user that has the budget adjustor role but only view permissions
            serviceForViewPermission = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, adapterRegistry.Object,
                       baViewUserFactory, roleRepository1, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, adapterRegistry.Object,
                       noPermissionsUser, roleRepository1, loggerObject);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            baSummaryService = null;
            budgetAdjustmentService = null;
            service3 = null;
            serviceForViewPermission = null;
            serviceForNoPermission = null;
            adjustmentSuccessEntity = null;
            adjustmentErrorEntity = null;
            costCenterStructure = null;
            exclusions = null;
            budgetAdjustmentParameters = null;
            testBudgetAdjustmentRepository = null;
            repositoryMock2 = null;
            userFactory = null;
            userFactory2 = null;
            baViewUserFactory = null;
            noPermissionsUser = null;
            roleRepositoryMock = null;
            roleRepository = null;
            roleRepositoryMock1 = null;
            roleRepository1 = null;
            glUserRoleAllPermissions = null;
            glUserRoleViewPermissions = null;
        }
        #endregion

        #region CreateBudgetAdjustmentAsync
        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_Success()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                DraftDeletionSuccessfulOrUnnecessary = true,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);
            Assert.AreEqual(inputDto.DraftDeletionSuccessfulOrUnnecessary, adjustmentDto.DraftDeletionSuccessfulOrUnnecessary);

            foreach (var adjustmentLineDto in inputDto.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_NullInputDto()
        {
            var expectedParam = "budgetAdjustmentDto";
            var actualParam = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateBudgetAdjustmentAsync_DoesNotHavePermission()
        {
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            var adjustmentDto = await serviceForViewPermission.CreateBudgetAdjustmentAsync(inputDto);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_HasAssetAccount()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_11001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            var expectedMessage = "Only expense type GL accounts are allowed in a Budget Adjustment.";
            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_HasLiabilityAccount()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_21001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            var expectedMessage = "Only expense type GL accounts are allowed in a Budget Adjustment.";
            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_HasFundBalanceAccount()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_31001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            var expectedMessage = "Only expense type GL accounts are allowed in a Budget Adjustment.";
            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_HasRevenueAccount()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_41001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            var expectedMessage = "Only expense type GL accounts are allowed in a Budget Adjustment.";
            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_LineHasNoGlAccount()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            // Add the blank line to the DTO list.
            inputDto.AdjustmentLines.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "",
                FromAmount = 0m,
                ToAmount = 100m
            });

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);

            Assert.AreEqual(adjustmentSuccessEntity.AdjustmentLines.Count, adjustmentDto.AdjustmentLines.Count);
            foreach (var adjustmentLineEntity in adjustmentSuccessEntity.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineEntity.GlNumber
                    && x.FromAmount == adjustmentLineEntity.FromAmount
                    && x.ToAmount == adjustmentLineEntity.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_LineHasFromAndToAmounts()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            // Add the blank line to the DTO list.
            inputDto.AdjustmentLines.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51045",
                FromAmount = 100m,
                ToAmount = 100m
            });

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);

            Assert.AreEqual(adjustmentSuccessEntity.AdjustmentLines.Count, adjustmentDto.AdjustmentLines.Count);
            foreach (var adjustmentLineEntity in adjustmentSuccessEntity.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineEntity.GlNumber
                    && x.FromAmount == adjustmentLineEntity.FromAmount
                    && x.ToAmount == adjustmentLineEntity.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_LineHasNeitherFromNorToAmounts()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            // Add the blank line to the DTO list.
            inputDto.AdjustmentLines.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51045",
                FromAmount = 0m,
                ToAmount = 0m
            });

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);

            Assert.AreEqual(adjustmentSuccessEntity.AdjustmentLines.Count, adjustmentDto.AdjustmentLines.Count);
            foreach (var adjustmentLineEntity in adjustmentSuccessEntity.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineEntity.GlNumber
                    && x.FromAmount == adjustmentLineEntity.FromAmount
                    && x.ToAmount == adjustmentLineEntity.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_LineHasNegativeAmounts()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            // Add the blank line to the DTO list.
            inputDto.AdjustmentLines.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51045",
                FromAmount = -100m,
                ToAmount = -100m
            });

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            Assert.AreEqual(adjustmentSuccessEntity.Id, adjustmentDto.Id);
            Assert.AreEqual(inputDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(inputDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(inputDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(inputDto.TransactionDate, adjustmentDto.TransactionDate);

            Assert.AreEqual(adjustmentSuccessEntity.AdjustmentLines.Count, adjustmentDto.AdjustmentLines.Count);
            foreach (var adjustmentLineEntity in adjustmentSuccessEntity.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineEntity.GlNumber
                    && x.FromAmount == adjustmentLineEntity.FromAmount
                    && x.ToAmount == adjustmentLineEntity.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_ExcludedGlAccounts()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_75050",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_75060",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var expectedMessage = "Object:75050: may not be used in budget adjustments.<>Object:75060: may not be used in budget adjustments.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_MultipleCostCenters()
        {
            configurationRepositoryMock.Setup(x => x.GetBudgetAdjustmentParametersAsync()).Returns(() =>
            {
                return Task.FromResult(new BudgetAdjustmentParameters(true, false, false));
            });

            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51002",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            var expectedMessage = "GL accounts must be from the same cost center.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_Fail()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            adjustmentErrorEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment(inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentErrorEntity.Comments = inputDto.Comments;
            adjustmentErrorEntity.Initiator = inputDto.Initiator;
            adjustmentErrorEntity.ErrorMessages = new List<string>() { "Insufficient funds for GL account 1234.", "Security not assigned for GL account 1234." };

            var actualMessage = "";
            try
            {
                var adjustmentDto = await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(String.Join("<>", adjustmentErrorEntity.ErrorMessages), actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_ConfigurationRepositoryReturnsNullExclusions()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = null,
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            exclusions = null;
            var expectedMessage = "Error retrieving budget adjustment exclusions.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_ConfigurationRepositoryReturnsNullGlClassConfiguration()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = null,
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            glClassConfiguration = null;
            var expectedMessage = "Error retrieving GL class configuration.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_ConfigurationRepositoryReturnsNullCostCenterStructure()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = null,
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            costCenterStructure = null;
            var expectedMessage = "Cost center structure not defined.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_ConfigurationRepositoryReturnsEmptyCostCenterStructure()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = null,
                Initiator = null,
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            costCenterStructure = new CostCenterStructure();
            var expectedMessage = "Cost center structure not defined.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }

        [TestMethod]
        public async Task CreateBudgetAdjustmentAsync_RepositoryReturnsNull()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            adjustmentSuccessEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment("B000001", inputDto.TransactionDate, inputDto.Reason, userFactory.CurrentUser.PersonId, adjustmentLines);
            adjustmentSuccessEntity.Comments = inputDto.Comments;
            adjustmentSuccessEntity.Initiator = inputDto.Initiator;

            var expectedMessage = "Budget adjustment must not be null.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
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
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
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
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }

            var expectedMessage = "Adjustment appears to have succeeded, but no ID was returned.";
            var actualMessage = "";
            try
            {
                await service.CreateBudgetAdjustmentAsync(inputDto);
            }
            catch (ApplicationException aex)
            {
                actualMessage = aex.Message;
            }
            Assert.AreEqual(expectedMessage, actualMessage);
        }
        #endregion

        #region UpdateBudgetAdjustmentAsync

        [TestMethod]
        public async Task UpdateBudgetAdjustmentAsync_Success()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = "B000333",
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                DraftDeletionSuccessfulOrUnnecessary = true,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            inputDto.NextApprovers.Add(new Dtos.ColleagueFinance.NextApprover() { NextApproverId = "TEST", NextApproverName = "Test Name" });

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }
            var initialBudgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync(inputDto.Id);
            var adjustmentDto = await budgetAdjustmentService.UpdateBudgetAdjustmentAsync(inputDto.Id, inputDto);
            Assert.AreEqual(initialBudgetAdjustmentDto.Id, adjustmentDto.Id);
            Assert.AreEqual(initialBudgetAdjustmentDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(initialBudgetAdjustmentDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(initialBudgetAdjustmentDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(initialBudgetAdjustmentDto.TransactionDate, adjustmentDto.TransactionDate);
            Assert.AreEqual(initialBudgetAdjustmentDto.DraftDeletionSuccessfulOrUnnecessary, adjustmentDto.DraftDeletionSuccessfulOrUnnecessary);

            foreach (var adjustmentLineDto in initialBudgetAdjustmentDto.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }

            Assert.AreEqual(inputDto.NextApprovers.Count, adjustmentDto.NextApprovers.Count);
            foreach(var nextApproverDto in adjustmentDto.NextApprovers)
            {
                Assert.AreEqual(inputDto.NextApprovers[0].NextApproverId, nextApproverDto.NextApproverId);
                Assert.AreEqual(null, nextApproverDto.NextApproverName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task UpdateBudgetAdjustmentAsync_CompleteStatus()
        {
            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_11_12_13_33333_51001",
                FromAmount = 100m,
                ToAmount = 0m
            });

            adjustmentLineDtos.Add(new Dtos.ColleagueFinance.AdjustmentLine()
            {
                GlNumber = "10_12_12_13_33333_51001",
                FromAmount = 0m,
                ToAmount = 100m
            });

            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = "B000111",
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now,
                DraftDeletionSuccessfulOrUnnecessary = true,
                NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
            };

            inputDto.NextApprovers.Add(new Dtos.ColleagueFinance.NextApprover() { NextApproverId = "TEST", NextApproverName = "Test Name" });

            // Set up the entity to be returned response entities.
            var adjustmentLines = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in adjustmentLineDtos)
            {
                adjustmentLines.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
            }
            var initialBudgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync(inputDto.Id);
            var adjustmentDto = await budgetAdjustmentService.UpdateBudgetAdjustmentAsync(inputDto.Id, inputDto);
            Assert.AreEqual(initialBudgetAdjustmentDto.Id, adjustmentDto.Id);
            Assert.AreEqual(initialBudgetAdjustmentDto.Comments, adjustmentDto.Comments);
            Assert.AreEqual(initialBudgetAdjustmentDto.Initiator, adjustmentDto.Initiator);
            Assert.AreEqual(initialBudgetAdjustmentDto.Reason, adjustmentDto.Reason);
            Assert.AreEqual(initialBudgetAdjustmentDto.TransactionDate, adjustmentDto.TransactionDate);
            Assert.AreEqual(initialBudgetAdjustmentDto.DraftDeletionSuccessfulOrUnnecessary, adjustmentDto.DraftDeletionSuccessfulOrUnnecessary);

            foreach (var adjustmentLineDto in initialBudgetAdjustmentDto.AdjustmentLines)
            {
                var matchingLine = adjustmentDto.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }

            Assert.AreEqual(inputDto.NextApprovers.Count, adjustmentDto.NextApprovers.Count);
            foreach (var nextApproverDto in adjustmentDto.NextApprovers)
            {
                Assert.AreEqual(inputDto.NextApprovers[0].NextApproverId, nextApproverDto.NextApproverId);
                Assert.AreEqual(null, nextApproverDto.NextApproverName);
            }
        }

        [TestMethod]
        public async Task UpdateBudgetAdjustmentAsync_NullId()
        {
            var expectedParam = "id";
            var actualParam = "";
            try
            {
                await service.UpdateBudgetAdjustmentAsync(null, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task UpdateBudgetAdjustmentAsync_NullInputDto()
        {
            var expectedParam = "budgetAdjustmentDto";
            var actualParam = "";
            try
            {
                await service.UpdateBudgetAdjustmentAsync("1", null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdateBudgetAdjustmentAsync_DoesNotHavePermission()
        {
            var adjustmentLineDtos = new List<Dtos.ColleagueFinance.AdjustmentLine>();
            var inputDto = new Dtos.ColleagueFinance.BudgetAdjustment()
            {
                AdjustmentLines = adjustmentLineDtos,
                Comments = "additional justificaton",
                Id = "1",
                Initiator = "Pass",
                Reason = "need more money",
                TransactionDate = DateTime.Now
            };

            var adjustmentDto = await serviceForViewPermission.UpdateBudgetAdjustmentAsync(inputDto.Id, inputDto);
        }

        #endregion

        #region Get budget adjustment

        [TestMethod]
        public async Task GetBudgetAdjustment_Success()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync("B000111");
            var budgetAdjustmentEntity = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000111");

            // Assert each domain entity is represented in a DTO.
            Assert.AreEqual(budgetAdjustmentEntity.Id, budgetAdjustmentDto.Id);
            Assert.AreEqual(budgetAdjustmentEntity.TransactionDate, budgetAdjustmentDto.TransactionDate);
            Assert.AreEqual(budgetAdjustmentDto.Reason, budgetAdjustmentEntity.Reason);
            Assert.AreEqual(budgetAdjustmentDto.PersonId, budgetAdjustmentEntity.PersonId);
            Assert.AreEqual(budgetAdjustmentDto.Status.ToString(), budgetAdjustmentEntity.Status.ToString());
            Assert.AreEqual(budgetAdjustmentDto.Initiator, budgetAdjustmentEntity.Initiator);
            Assert.AreEqual(budgetAdjustmentDto.DraftBudgetAdjustmentId, budgetAdjustmentEntity.DraftBudgetAdjustmentId);
            Assert.AreEqual(budgetAdjustmentEntity.Comments, budgetAdjustmentDto.Comments);

            foreach (var adjustmentLineDto in budgetAdjustmentDto.AdjustmentLines)
            {
                var matchingLine = budgetAdjustmentEntity.AdjustmentLines.FirstOrDefault(x => x.GlNumber == adjustmentLineDto.GlNumber
                    && x.FromAmount == adjustmentLineDto.FromAmount
                    && x.ToAmount == adjustmentLineDto.ToAmount);
                Assert.IsNotNull(matchingLine);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetBudgetAdjustment_DoesNotHavePermission()
        {
            var budgetAdjustmentDto = await serviceForNoPermission.GetBudgetAdjustmentAsync("B000111");
        }

        #endregion

        #region Get budget adjustments summary

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetBudgetAdjustmentSummary_DoesNotHavePermission()
        {
            var budgetAdjustmentSummaryDtos = await serviceForNoPermission.GetBudgetAdjustmentsSummaryAsync();
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentSummary_OnlyDrafts_Success()
        {
            var budgetAdjustmentSummaryDtos = await baSummaryService.GetBudgetAdjustmentsSummaryAsync();
            var budgetAdjustmentSummaryEntities = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentsSummaryAsync("0000001");

            // Assert that the domain entities count is equal to the resulting dtos count.
            Assert.AreEqual(budgetAdjustmentSummaryDtos.Count(), budgetAdjustmentSummaryEntities.Count());

            // Assert each domain entity is represented in a DTO.
            // Obtain the draft budget adjustments and non-draft budget adjustments separatedly.
            var draftBaDtos = budgetAdjustmentSummaryEntities.Where(x => x.DraftBudgetAdjustmentId != null).ToList();
            var baDtos = budgetAdjustmentSummaryEntities.Where(x => x.BudgetAdjustmentNumber != null).ToList();

            // Validate the draft information.
            foreach (var draftDto in draftBaDtos)
            {
                // Obtain the record from the domain entities that matches this draft dto.
                var matchingbaSummaryEntity = budgetAdjustmentSummaryEntities.Where(x => x.DraftBudgetAdjustmentId == draftDto.DraftBudgetAdjustmentId).FirstOrDefault();

                Assert.AreEqual(draftDto.DraftBudgetAdjustmentId, matchingbaSummaryEntity.DraftBudgetAdjustmentId);
                Assert.IsNull(draftDto.BudgetAdjustmentNumber);
                Assert.AreEqual(draftDto.PersonId, matchingbaSummaryEntity.PersonId);
                Assert.AreEqual(draftDto.Reason, matchingbaSummaryEntity.Reason);
                Assert.AreEqual(draftDto.ToAmount, matchingbaSummaryEntity.ToAmount);
                Assert.AreEqual(draftDto.TransactionDate, matchingbaSummaryEntity.TransactionDate);
            }
        }
        [TestMethod]
        public async Task GetBudgetAdjustmentSummary_UserDoesNotMatch()
        {
            // service 3 is setup to use personId 0000004.
            var budgetAdjustmentSummaryDtos = await service3.GetBudgetAdjustmentsSummaryAsync();
            var budgetAdjustmentSummaryEntities = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentsSummaryAsync(personId);

            // Assert that the domain entities count is equal to the resulting dtos count.
            Assert.AreEqual(budgetAdjustmentSummaryDtos.Count(), budgetAdjustmentSummaryEntities.Count());
            Assert.AreEqual(0, budgetAdjustmentSummaryDtos.Count());
            Assert.AreEqual(0, budgetAdjustmentSummaryEntities.Count());
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentSummary_NoEntitiesReturned()
        {
            personId = "0000002";
            var budgetAdjustmentSummaryDtos = await baSummaryService.GetBudgetAdjustmentsSummaryAsync();
            var budgetAdjustmentSummaryEntities = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentsSummaryAsync(personId);

            // Assert that no domain entities are returned.
            Assert.AreNotEqual(budgetAdjustmentSummaryDtos.Count(), budgetAdjustmentSummaryEntities.Count());
            Assert.AreEqual(9, budgetAdjustmentSummaryDtos.Count());
            Assert.AreEqual(0, budgetAdjustmentSummaryEntities.Count());
        }

        #endregion
    }
}