// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Exceptions;
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
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        private BudgetAdjustmentService service;
        private BudgetAdjustmentService baSummaryService;
        private BudgetAdjustmentService budgetAdjustmentService;
        private BudgetAdjustmentService baServiceForCurrentUserNotInitiator;
        private BudgetAdjustmentService serviceForViewPermission;
        private BudgetAdjustmentService serviceForNoPermission;
        private BudgetAdjustmentService approvalBudgetAdjustmentService;

        private Mock<IBudgetAdjustmentsRepository> repositoryMock = new Mock<IBudgetAdjustmentsRepository>();
        private Mock<IBudgetAdjustmentsRepository> repositoryMock2 = new Mock<IBudgetAdjustmentsRepository>();
        private Mock<IBudgetAdjustmentsRepository> repositoryApproveMock;
        private Mock<IBudgetAdjustmentsRepository> repositoryApproveNullDomainMock;
        private Mock<IGeneralLedgerConfigurationRepository> configurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
        private Mock<IApproverRepository> approverRepositoryMock = new Mock<IApproverRepository>();
        private Mock<IStaffRepository> staffRepositoryMock = new Mock<IStaffRepository>();
        private Mock<IStaffRepository> approverStaffRepositoryMock = new Mock<IStaffRepository>();

        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock1;
        private IRoleRepository roleRepository;
        private IRoleRepository roleRepository1;

        private Domain.Entities.Permission permissionViewBudgetAdjustment;
        private Domain.Entities.Permission permissionCreateUpdateBudgetAdjustment;
        private Domain.Entities.Permission permissionDeleteBudgetAdjustment;
        private Domain.Entities.Permission budgetAdjustmentApproverPermission;
        protected Domain.Entities.Role glUserRoleAllPermissions = new Domain.Entities.Role(333, "Budget.Adjustor");
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(334, "Budget.Adjustor.View");

        private Domain.ColleagueFinance.Entities.BudgetAdjustment adjustmentSuccessEntity;
        private Domain.ColleagueFinance.Entities.BudgetAdjustment adjustmentErrorEntity;
        private Domain.ColleagueFinance.Entities.BudgetAdjustment budgetAdjustmentToApproveEntity;

        private Dtos.ColleagueFinance.BudgetAdjustmentApproval budgetAdjustmentApprovalDto;

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

        // Inititalize the budget adjustment dto used for the approve methods.
        private Dtos.ColleagueFinance.BudgetAdjustment budgetAdjustmentToApproveDto = new Dtos.ColleagueFinance.BudgetAdjustment()
        {
            AdjustmentLines = new List<Dtos.ColleagueFinance.AdjustmentLine>()
                {
                    new Dtos.ColleagueFinance.AdjustmentLine
                    {
                        GlNumber = "10_11_12_13_33333_51001",
                        FromAmount = 100m,
                        ToAmount = 0m
                    },
                    new Dtos.ColleagueFinance.AdjustmentLine
                    {
                        GlNumber = "10_12_12_13_33333_51001",
                        FromAmount = 0m,
                        ToAmount = 100m
                    }
                },
            Comments = "Approving this budget adjustment.",
            Id = "B333333",
            Initiator = "Initiator needing approval",
            Reason = "Need this ba approved",
            TransactionDate = DateTime.Now,
            Status = Dtos.ColleagueFinance.BudgetEntryStatus.NotApproved,
            NextApprovers = new List<Dtos.ColleagueFinance.NextApprover>()
                {
                    new Dtos.ColleagueFinance.NextApprover
                    {
                        NextApproverId = "AAA",
                        NextApproverName = "Approver AAA name"
                    },
                    new Dtos.ColleagueFinance.NextApprover
                    {
                        NextApproverId = "BBB",
                        NextApproverName = "Approver BBB name"
                    }
                },
            Approvers = new List<Dtos.ColleagueFinance.Approver>()
                {
                    new Dtos.ColleagueFinance.Approver
                    {
                        ApproverId = "MMM",
                        ApprovalName = "Approver name MMM",
                        ApprovalDate = DateTime.Now.AddDays(-1)
                    },
                    new Dtos.ColleagueFinance.Approver
                    {
                        ApproverId = "NNN",
                        ApprovalName = "Approver name NNN",
                        ApprovalDate = DateTime.Now.AddDays(-3)
                    }
                }
        };

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            var loggerObject = loggerMock.Object;
            var adapterRegistryObject = adapterRegistryMock.Object;

            // Create permission domain entities for create/update, view and delete.
            permissionViewBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBudgetAdjustments);
            permissionCreateUpdateBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);
            permissionDeleteBudgetAdjustment = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.DeleteBudgetAdjustments);
            budgetAdjustmentApproverPermission = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBudgetAdjustmentsPendingApproval);

            // Assign all permissions to the role that has all permissions.
            glUserRoleAllPermissions.AddPermission(permissionViewBudgetAdjustment);
            glUserRoleAllPermissions.AddPermission(permissionCreateUpdateBudgetAdjustment);
            glUserRoleAllPermissions.AddPermission(permissionDeleteBudgetAdjustment);
            glUserRoleAllPermissions.AddPermission(budgetAdjustmentApproverPermission);
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

            approverRepositoryMock.Setup(apm => apm.GetApproverNameForIdAsync("XYZ")).Returns(() =>
           {
               return Task.FromResult(string.Empty);
           });

            staffRepositoryMock.Setup(srm => srm.GetStaffLoginIdForPersonAsync(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult("AJK");
            });

            // Build the service object.
            service = new BudgetAdjustmentService(repositoryMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, userFactory, roleRepository, loggerMock.Object);

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

            var budgetAdjustmentEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>()).Returns(budgetAdjustmentEntityToDtoAdapter);

            var adjustmentLineToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.AdjustmentLine, Dtos.ColleagueFinance.AdjustmentLine>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.AdjustmentLine, Dtos.ColleagueFinance.AdjustmentLine>()).Returns(adjustmentLineToDtoAdapter);

            var nextApproverToEntityAdapter = new AutoMapperAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>()).Returns(nextApproverToEntityAdapter);
            var nextApproverToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>()).Returns(nextApproverToDtoAdapter);

            var approverToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>()).Returns(approverToDtoAdapter);

            var statusEntityToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetEntryStatus, Dtos.ColleagueFinance.BudgetEntryStatus>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetEntryStatus, Dtos.ColleagueFinance.BudgetEntryStatus>()).Returns(statusEntityToDtoAdapter);

            var pendingApprovalToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentPendingApprovalSummary, Dtos.ColleagueFinance.BudgetAdjustmentPendingApprovalSummary>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentPendingApprovalSummary, Dtos.ColleagueFinance.BudgetAdjustmentPendingApprovalSummary>()).Returns(pendingApprovalToDtoAdapter);

            var summaryToDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentSummary, Dtos.ColleagueFinance.BudgetAdjustmentSummary>(adapterRegistryObject, loggerObject);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentSummary, Dtos.ColleagueFinance.BudgetAdjustmentSummary>()).Returns(summaryToDtoAdapter);

            // Build the service for budget adjustment summary information object.
            baSummaryService = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, userFactory, roleRepository, loggerObject);

            // Build the budgetAdjustmentService object.
            budgetAdjustmentService = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, userFactory, roleRepository, loggerObject);

            userFactory2 = new GeneralLedgerCurrentUser.UserFactoryAll();
            // Setup the service that is going to make currentUser.PersonId different for the security test.
            baServiceForCurrentUserNotInitiator = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, userFactory2, roleRepository, loggerObject);

            // Build a service for a user that has the budget adjustor role but only view permissions
            serviceForViewPermission = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, baViewUserFactory, roleRepository1, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new BudgetAdjustmentService(testBudgetAdjustmentRepository, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryObject, noPermissionsUser, roleRepository1, loggerObject);

            #region PostBudgetAdjustmentApprovalAsync
            /////////////////////////////////////////////////////
            // Area with setup for approve a budget adjustment //
            /////////////////////////////////////////////////////

            budgetAdjustmentApprovalDto = new Dtos.ColleagueFinance.BudgetAdjustmentApproval()
            {
                Comments = "Approving this budget adjustment."
            };

            // Initialize the budget adjustment domain entity used for the approve methods.
            budgetAdjustmentToApproveEntity = new BudgetAdjustment("B333333", "Reason for B333333", DateTime.Now, "0000001")
            {
                Status = BudgetEntryStatus.NotApproved,
                Initiator = "Initiator needing approval",
                Comments = "Comments for B333333. One approver already has approved.",
                NextApprovers = new List<NextApprover>
                {
                    new NextApprover("AAA"),
                    new NextApprover("BBB"),
                    new NextApprover("ABC")
                },
                Approvers = new List<Approver>
                {
                    new Approver("MMM")
                    {
                         ApprovalDate = DateTime.Now
                    },
                    new Approver("NNN")
                    {
                         ApprovalDate = DateTime.Now
                    }
                }
            };
            budgetAdjustmentToApproveEntity.AddAdjustmentLine(new AdjustmentLine("10_11_12_13_33333_51001", 12m, 0m));
            budgetAdjustmentToApproveEntity.AddAdjustmentLine(new AdjustmentLine("10_11_12_13_33333_51002", 0m, 12m));

            // Mock the repository to approve a budget adjustment.
            repositoryApproveMock = new Mock<IBudgetAdjustmentsRepository>();
            repositoryApproveMock.Setup(y => y.GetBudgetAdjustmentAsync(It.IsAny<string>())).ReturnsAsync(budgetAdjustmentToApproveEntity);
            repositoryApproveMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).Returns(Task.FromResult(budgetAdjustmentToApproveEntity));
            repositoryApproveMock.Setup(x => x.ValidateBudgetAdjustmentAsync(It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).Returns(Task.FromResult(new List<string>()));

            approverStaffRepositoryMock = new Mock<IStaffRepository>();
            approverStaffRepositoryMock.Setup(z => z.GetStaffLoginIdForPersonAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            repositoryApproveNullDomainMock = new Mock<IBudgetAdjustmentsRepository>();
            repositoryApproveNullDomainMock.Setup(y => y.GetBudgetAdjustmentAsync(It.IsAny<string>())).ReturnsAsync(null as BudgetAdjustment);

            // Build a service to approve a budget adjustment.
            approvalBudgetAdjustmentService = new BudgetAdjustmentService(repositoryApproveMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                approverStaffRepositoryMock.Object, adapterRegistryObject, userFactory, roleRepository, loggerObject);

            ///////////////////////////////////////////////
            // End of area for approve a budget adjustment.
            ///////////////////////////////////////////////
            #endregion
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            service = null;
            baSummaryService = null;
            budgetAdjustmentService = null;
            baServiceForCurrentUserNotInitiator = null;
            serviceForViewPermission = null;
            serviceForNoPermission = null;
            approvalBudgetAdjustmentService = null;
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
            repositoryApproveMock = null;
            glUserRoleAllPermissions = null;
            glUserRoleViewPermissions = null;
            budgetAdjustmentApprovalDto = null;
            budgetAdjustmentToApproveEntity = null;
            staffRepositoryMock = null;
            approverStaffRepositoryMock = null;
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
            foreach (var nextApproverDto in adjustmentDto.NextApprovers)
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

        #region PostBudgetAdjustmentApprovalAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostBudgetAdjustmentApprovalAsync_NullId()
        {
            string id = null;
            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync(id, budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostBudgetAdjustmentApprovalAsync_EmptyId()
        {
            string id = string.Empty;
            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync(id, budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostBudgetAdjustmentApprovalAsync_NullDto()
        {
            budgetAdjustmentApprovalDto = null;
            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PostBudgetAdjustmentApprovalAsync_DoesNotHavePermission()
        {
            await serviceForNoPermission.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        public async Task CatchNullStaffRecordExceptionLogErrorTest()
        {
            var exceptionCaught = false;
            try
            {
                // Mock for a thrown exception from GetStaffLoginIdForPersonAsync.
                repositoryApproveMock = new Mock<IBudgetAdjustmentsRepository>();
                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepositoryMock.Setup(z => z.GetStaffLoginIdForPersonAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException("Could not find Staff information for the user."));
                approvalBudgetAdjustmentService = new BudgetAdjustmentService(repositoryApproveMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                    staffRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepository, loggerMock.Object);

                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                Assert.AreEqual(ex.Message, "Could not find Staff information for the user.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_NullBudgetAdjustmentEntity()
        {
            // Mock the service to a return a null budget adjustment.
            approvalBudgetAdjustmentService = new BudgetAdjustmentService(repositoryApproveNullDomainMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepository, loggerMock.Object);

            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                Assert.AreEqual(ex.Message, "Could not retrieve the budget adjustment.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_BudgetAdjustmentInvalidStatus()
        {
            // Mock the service to a return a complete budget adjustment.
            budgetAdjustmentToApproveEntity.Status = BudgetEntryStatus.Complete;

            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(NotApprovedStatusException), ex.GetType());
                Assert.AreEqual(ex.Message, "The budget adjustment does not have a not approved status.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_BudgetAdjustmentAlreadyApproved()
        {
            // Mock the service to a return a budget adjustment already approved by the current user.
            budgetAdjustmentToApproveEntity.Approvers.Add(new Approver("ABC"));

            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(AlreadyApprovedByUserException), ex.GetType());
                Assert.AreEqual(ex.Message, "You have already approved this budget adjustment.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_NullNextApprovers()
        {
            // Mock the service to a return a budget adjustment where the current user is not a next approver.
            budgetAdjustmentToApproveEntity.NextApprovers = null;
            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(PermissionsException), ex.GetType());
                Assert.AreEqual(ex.Message, "You are no longer a next approver in this budget adjustment.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_CurrentUserIsNotANextApprover()
        {
            // Mock the service to a return a budget adjustment where the current user is not a next approver.
            budgetAdjustmentToApproveEntity.NextApprovers[2] = new NextApprover("XYZ");
            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(PermissionsException), ex.GetType());
                Assert.AreEqual(ex.Message, "You are no longer a next approver in this budget adjustment.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_ValidationErrors()
        {
            // Mock the service to a return a validation error.
            repositoryApproveMock.Setup(x => x.ValidateBudgetAdjustmentAsync(It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).Returns(() =>
           {
               return Task.FromResult(new List<string>() { "Validation error" });
           });
            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                Assert.AreEqual(ex.Message, "The budget adjustment fails validation before posting.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_UpdateAsyncReturnsNullEntity()
        {
            // Mock the service to a return a null entity after the update.
            repositoryApproveMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).ReturnsAsync(null as BudgetAdjustment);

            var exceptionCaught = false;
            try
            {
                await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                Assert.AreEqual(ex.Message, "Budget adjustment must not be null.");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_Success()
        {
            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_SuccessWithNoApprovers()
        {
            budgetAdjustmentToApproveEntity.Approvers = new List<Approver>();

            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_SuccessWithOneNextApprover()
        {
            budgetAdjustmentToApproveEntity.NextApprovers.RemoveAt(1);

            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_SuccessWithExistingCommentsAndApproverName()
        {
            approverRepositoryMock.Setup(apm => apm.GetApproverNameForIdAsync(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult("Mary Louise Approver");
            });

            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_SuccessWithNoExistingCommentsAndNoApproverName()
        {
            budgetAdjustmentToApproveEntity.Comments = string.Empty;

            await approvalBudgetAdjustmentService.PostBudgetAdjustmentApprovalAsync("B333333", budgetAdjustmentApprovalDto);

            //var comments = "AAA" + " " + DateTime.Now.ToString() + Environment.NewLine + budgetAdjustmentApprovalDto.Comments;
        }

        #endregion

        #region Get budget adjustment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_NullId()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentAsync_EmptyId()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync("");
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_GetAsyncReturnsNullEntity()
        {
            // Mock the service to a return a null budget adjustment.
            budgetAdjustmentService = new BudgetAdjustmentService(repositoryApproveNullDomainMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepository, loggerMock.Object);

            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync("B000111");

            Assert.IsNull(budgetAdjustmentDto.Id);
            Assert.IsNull(budgetAdjustmentDto.Reason);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetBudgetAdjustmentAsync_DoesNotHavePermission()
        {
            var budgetAdjustmentDto = await serviceForNoPermission.GetBudgetAdjustmentAsync("B000111");
        }

        [TestMethod]
        public async Task PostBudgetAdjustmentApprovalAsync_CurrentUserIsNotInitiator()
        {
            var exceptionCaught = false;
            try
            {
                // baServiceForCurrentUserNotInitiator is setup to use personId 0000004 which is not the initiator.
                var budgetAdjustmentSummaryDtos = await baServiceForCurrentUserNotInitiator.GetBudgetAdjustmentAsync("B000111");
            }
            catch (Exception ex)
            {
                exceptionCaught = true;
                Assert.AreEqual(typeof(PermissionsException), ex.GetType());
                Assert.AreEqual(ex.Message, "The current user 0000004 is not the person 0000001 that owns the record returned from the repository");
            }
            Assert.IsTrue(exceptionCaught);
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_ValidationErrors()
        {
            // Mock the service to a return a validation error.
            repositoryApproveMock.Setup(x => x.ValidateBudgetAdjustmentAsync(It.IsAny<Domain.ColleagueFinance.Entities.BudgetAdjustment>())).Returns(() =>
            {
                return Task.FromResult(new List<string>() { "Validation error" });
            });
            budgetAdjustmentService = new BudgetAdjustmentService(repositoryApproveMock.Object, configurationRepositoryMock.Object, approverRepositoryMock.Object,
                staffRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepository, loggerMock.Object);

            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentAsync("B000111");

            Assert.AreEqual(budgetAdjustmentDto.ValidationResults.Count(), 1);
            Assert.AreEqual(budgetAdjustmentDto.ValidationResults[0], "Validation error");
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentAsync_Success()
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

        #endregion

        #region GetBudgetAdjustmentPendingApprovalDetailAsync

        [TestMethod]
        public async Task GetBudgetAdjustmentPendingApprovalDetailAsync_Success()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync("B000333");

            var budgetAdjustmentEntity = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentAsync("B000333");

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
        public async Task GetBudgetAdjustmentPendingApprovalDetailAsync_CurrentUserIsNotANextApprover()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync("B000111");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentPendingApprovalDetailAsync_NullId()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetBudgetAdjustmentPendingApprovalDetailAsync_EmptyId()
        {
            var budgetAdjustmentDto = await budgetAdjustmentService.GetBudgetAdjustmentPendingApprovalDetailAsync("");
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
            // baServiceForCurrentUserNotInitiator is setup to use personId 0000004.
            var budgetAdjustmentSummaryDtos = await baServiceForCurrentUserNotInitiator.GetBudgetAdjustmentsSummaryAsync();
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

        #region Get budget adjustments pending approval summary

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetBudgetAdjustmentPendingApprovalSummary_DoesNotHavePermission()
        {
            var budgetAdjustmentPendingApprovalSummaryDtos = await serviceForNoPermission.GetBudgetAdjustmentsPendingApprovalSummaryAsync();
        }

        [TestMethod]
        public async Task GetBudgetAdjustmentPendingApprovalSummary_Success()
        {
            var budgetAdjustmentPendingApprovalSummaryDtos = await budgetAdjustmentService.GetBudgetAdjustmentsPendingApprovalSummaryAsync();
            var budgetAdjustmentpendingApprovalSummaryEntities = await this.testBudgetAdjustmentRepository.GetBudgetAdjustmentsPendingApprovalSummaryAsync("0000001");

            // Assert that the domain entities count is equal to the resulting dtos count.
            Assert.AreEqual(budgetAdjustmentPendingApprovalSummaryDtos.Count(), budgetAdjustmentpendingApprovalSummaryEntities.Count());

            // Assert each domain entity is represented in a DTO.
            foreach (var dto in budgetAdjustmentPendingApprovalSummaryDtos)
            {
                // Obtain the record from the domain entities that matches this draft dto.
                var matchingbaSummaryEntity = budgetAdjustmentpendingApprovalSummaryEntities.Where(x => x.BudgetAdjustmentNumber == dto.BudgetAdjustmentNumber).FirstOrDefault();

                Assert.AreEqual(dto.Status, Dtos.ColleagueFinance.BudgetEntryStatus.NotApproved);
                Assert.AreEqual(dto.InitiatorName, matchingbaSummaryEntity.InitiatorName);
                Assert.AreEqual(dto.Reason, matchingbaSummaryEntity.Reason);
                Assert.AreEqual(dto.ToAmount, matchingbaSummaryEntity.ToAmount);
                Assert.AreEqual(dto.TransactionDate, matchingbaSummaryEntity.TransactionDate);
            }
        }

        #endregion
    }
}