// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Tests.Services
{
    [TestClass]
    public class BudgetDevelopmentServiceTests
    {
        #region Initialize and Cleanup
        public BudgetDevelopmentService service = null;
        public BudgetDevelopmentService serviceWB2 = null;
        public TestBudgetDevelopmentRepository testBuDevRepository;
        public TestWorkingBudget2Repository testWorkingBudget2Repository;
        public TestBudgetDevelopmentConfigurationRepository testBuDevConfigRepository;
        public TestGeneralLedgerConfigurationRepository testGlConfigurationRepository;
        public TestGeneralLedgerAccountRepository testGlAcccountRepository;

        public GeneralLedgerCurrentUser.UserFactory userFactory = new GeneralLedgerCurrentUser.UserFactory();
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> logger;
        public Mock<IAdapterRegistry> adapterRegistry;

        public string workingBudgetId;
        public ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables;
        public BudgetConfiguration budgetConfiguration;
        public string personId;
        public IList<string> majorComponentStartPositions;
        public GeneralLedgerAccountStructure glAccountStructure;

        public Dtos.BudgetManagement.WorkingBudgetQueryCriteria criteriaDto;
        public Dtos.BudgetManagement.ComponentQueryCriteria componentCriteriaDto;
        public Dtos.BudgetManagement.ComponentRangeQueryCriteria rangeCriteriaDto;
        public WorkingBudgetQueryCriteria criteriaDomain;
        public ComponentQueryCriteria componentCriteriaDomain;
        public ComponentRangeQueryCriteria rangeCriteriaDomain;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistry = new Mock<IAdapterRegistry>();
            logger = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();

            testBuDevRepository = new TestBudgetDevelopmentRepository();
            testWorkingBudget2Repository = new TestWorkingBudget2Repository();
            testBuDevConfigRepository = new TestBudgetDevelopmentConfigurationRepository();
            testGlConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGlAcccountRepository = new TestGeneralLedgerAccountRepository();

            workingBudgetId = testBuDevConfigRepository.BudgetDevDefaultsContract.BudDevBudget;
            personId = userFactory.CurrentUser.PersonId;
            budgetConfiguration = new BudgetConfiguration(workingBudgetId);
            glAccountStructure = new GeneralLedgerAccountStructure();

            componentCriteriaDto = new Dtos.BudgetManagement.ComponentQueryCriteria()
            {
                ComponentName = "FUND",
                IndividualComponentValues = new List<string>()
                {
                    "11"
                },
                RangeComponentValues = new List<Dtos.BudgetManagement.ComponentRangeQueryCriteria>()
            };
            criteriaDto = new Dtos.BudgetManagement.WorkingBudgetQueryCriteria()
            {
                Ids = null,
                StartLineItem = 0,
                LineItemCount = 999,
                ComponentCriteria = new List<Dtos.BudgetManagement.ComponentQueryCriteria>()
                {
                    componentCriteriaDto
                },
                SortSubtotalComponentQueryCriteria = new List<Dtos.BudgetManagement.SortSubtotalComponentQueryCriteria>()
                {
                    new Dtos.BudgetManagement.SortSubtotalComponentQueryCriteria()
                    {
                                                SubtotalType = "GL",
                        SubtotalName = "Fund",
                        Order = 1,
                        IsDisplaySubTotal = true
                    },
                                        new Dtos.BudgetManagement.SortSubtotalComponentQueryCriteria()
                    {
                        SubtotalType = "GL",
                        SubtotalName = "Unit",
                        Order = 2,
                        IsDisplaySubTotal = true
                    },
                    new Dtos.BudgetManagement.SortSubtotalComponentQueryCriteria()
                    {
                        SubtotalType = "BO",
                        SubtotalName = "Budget Officer",
                        Order = 3,
                        IsDisplaySubTotal = true
                    }
                }
            };

            componentCriteriaDomain = new ComponentQueryCriteria("FUND")
            {
                IndividualComponentValues = new List<string>()
                {
                    "11"
                },
                RangeComponentValues = new List<ComponentRangeQueryCriteria>()
            };
            criteriaDomain = new WorkingBudgetQueryCriteria(new List<ComponentQueryCriteria>())
            {
                StartLineItem = 0,
                LineItemCount = 999,
                ComponentCriteria = new List<ComponentQueryCriteria>()
                {
                    componentCriteriaDomain
                },
                SortSubtotalComponentQueryCriteria = new List<SortSubtotalComponentQueryCriteria>()
                 {
                     new SortSubtotalComponentQueryCriteria()
                     {
                            SubtotalType = "GL",
                            SubtotalName = "Fund",
                            Order = 1,
                            IsDisplaySubTotal = true
                     },
                     new SortSubtotalComponentQueryCriteria()
                     {
                            SubtotalType = "GL",
                            SubtotalName = "Unit",
                            Order = 2,
                            IsDisplaySubTotal = true
                     },
                     new SortSubtotalComponentQueryCriteria()
                     {
                            SubtotalType = "BO",
                            SubtotalName = "Budget Officer",
                            Order = 3,
                            IsDisplaySubTotal = true
                     }
                }
            };

            // Set up and mock the adapter, and setup the GetAdapter method.
            var workingBudgetDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>()).Returns(workingBudgetDtoAdapter);
            var workingBudget2tDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.WorkingBudget2, Dtos.BudgetManagement.WorkingBudget2>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget2, Dtos.BudgetManagement.WorkingBudget2>()).Returns(workingBudget2tDtoAdapter);
            var buDevLineItemDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>()).Returns(buDevLineItemDtoAdapter);
            var lineItemDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.LineItem, Dtos.BudgetManagement.LineItem>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.LineItem, Dtos.BudgetManagement.LineItem>()).Returns(lineItemDtoAdapter);
            var subtotalDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.SubtotalLineItem, Dtos.BudgetManagement.SubtotalLineItem>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.SubtotalLineItem, Dtos.BudgetManagement.SubtotalLineItem>()).Returns(subtotalDtoAdapter);
            var lineItemComparableDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>()).Returns(lineItemComparableDtoAdapter);
            var budgetOfficerDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>()).Returns(budgetOfficerDtoAdapter);
            var filterCriteriaDtoAdapter = new AutoMapperAdapter<Dtos.BudgetManagement.WorkingBudgetQueryCriteria, Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.BudgetManagement.WorkingBudgetQueryCriteria, Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria>()).Returns(filterCriteriaDtoAdapter);
            var reportingUnitDtoAdapter = new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetReportingUnit, Dtos.BudgetManagement.BudgetReportingUnit>(adapterRegistry.Object, logger.Object);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetReportingUnit, Dtos.BudgetManagement.BudgetReportingUnit>()).Returns(reportingUnitDtoAdapter);

            // Set up the service
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            serviceWB2 = new BudgetDevelopmentService(testWorkingBudget2Repository, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testBuDevRepository = null;
            testWorkingBudget2Repository = null;
            testBuDevConfigRepository = null;
            testGlConfigurationRepository = null;
            testGlAcccountRepository = null;
            userFactory = null;
            roleRepositoryMock = null;
            logger = null;
            adapterRegistry = null;
            budgetConfiguration = null;
            glAccountStructure = null;
            criteriaDto = null;
            criteriaDomain = null;
        }
        #endregion

        #region QueryWorkingBudget2Async

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryWorkingBudget2Async_NullCriteria()
        {

            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task QueryWorkingBudget2Async_NoGlAccountStructure()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var repositoryGlAccountStructureNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlAccountStructureNullMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(null as GeneralLedgerAccountStructure);
            serviceWB2 = new BudgetDevelopmentService(testWorkingBudget2Repository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlAccountStructureNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task QueryWorkingBudget2Async_NoGlClassConfiguration()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            var glClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            var repositoryGlClassConfigurationNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlClassConfigurationNullMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(null as GeneralLedgerClassConfiguration);
            serviceWB2 = new BudgetDevelopmentService(testWorkingBudget2Repository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlClassConfigurationNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(criteriaDto);
        }

        [TestMethod]
        public async Task QueryWorkingBudget2Async_NullDomainEntity()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            // Mock the service to return a null domain entity.
            var repositoryNullDomainMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryNullDomainMock.Setup(x => x.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                null, personId, glAccountStructure, 0, 1)).ReturnsAsync(null as WorkingBudget2);

            serviceWB2 = new BudgetDevelopmentService(repositoryNullDomainMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(criteriaDto);
            Assert.AreEqual(buDevDto.LineItems, null);
            Assert.AreEqual(buDevDto.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task QueryWorkingBudget2Async_Success()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var workingBudget2Entity = await testWorkingBudget2Repository.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                criteriaDomain, personId, glAccountStructure, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                It.IsAny<WorkingBudgetQueryCriteria>(), personId, glAccountStructure, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount)).ReturnsAsync(workingBudget2Entity);
            serviceWB2 = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(criteriaDto);

            Assert.AreEqual(buDevDto.LineItems.Count(), workingBudget2Entity.LineItems.Count());
            var domainBudgetLineItems = workingBudget2Entity.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            var domainSubtotalLineItems = workingBudget2Entity.LineItems.Where(li => li.SubtotalLineItem != null).Select(li => li.SubtotalLineItem);
            var dtoBudgetLineItems = buDevDto.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            var dtoSubtotalLineItems = buDevDto.LineItems.Where(li => li.SubtotalLineItem != null).Select(li => li.SubtotalLineItem);
            Assert.AreEqual(dtoBudgetLineItems.Count(), domainBudgetLineItems.Count());
            Assert.AreEqual(dtoSubtotalLineItems.Count(), domainSubtotalLineItems.Count());
            Assert.AreEqual(dtoBudgetLineItems.Count() + dtoSubtotalLineItems.Count(), domainBudgetLineItems.Count() + domainSubtotalLineItems.Count());

            foreach (var lineItemDto in dtoBudgetLineItems)
            {
                var lineItemEntity = domainBudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
            }

            var domainSubItems = workingBudget2Entity.LineItems.Where(li => li.SubtotalLineItem != null);
            var dtoSubItems = buDevDto.LineItems.Where(li => li.SubtotalLineItem != null);
            foreach (var subItemDto in dtoSubItems)
            {
                var subtotalLineItemDto = subItemDto.SubtotalLineItem;
                var subtotalLineItemDomain = domainSubItems.Where(sb => sb.SequenceNumber == subItemDto.SequenceNumber).Select(sb => sb.SubtotalLineItem).FirstOrDefault();

                Assert.AreEqual(subtotalLineItemDto.SubtotalBaseBudgetAmount, subtotalLineItemDomain.SubtotalBaseBudgetAmount);
                Assert.AreEqual(subtotalLineItemDto.SubtotalDescription, subtotalLineItemDomain.SubtotalDescription);
                Assert.AreEqual(subtotalLineItemDto.SubtotalName, subtotalLineItemDomain.SubtotalName);
                Assert.AreEqual(subtotalLineItemDto.SubtotalOrder, subtotalLineItemDomain.SubtotalOrder);
                Assert.AreEqual(subtotalLineItemDto.SubtotalType, subtotalLineItemDomain.SubtotalType);
                Assert.AreEqual(subtotalLineItemDto.SubtotalValue, subtotalLineItemDomain.SubtotalValue);
                Assert.AreEqual(subtotalLineItemDto.SubtotalWorkingAmount, subtotalLineItemDomain.SubtotalWorkingAmount);
                Assert.AreEqual(subtotalLineItemDto.SubtotalBudgetComparables.Count(), subtotalLineItemDomain.SubtotalBudgetComparables.Count());

                foreach (var comp in subtotalLineItemDto.SubtotalBudgetComparables)
                {
                    if (comp.ComparableNumber == "C1")
                    {
                        Assert.AreEqual(comp.ComparableAmount, subtotalLineItemDomain.SubtotalBudgetComparables.Find(c => c.ComparableNumber == "C1").ComparableAmount);
                    }
                    if (comp.ComparableNumber == "C2")
                    {
                        Assert.AreEqual(comp.ComparableAmount, subtotalLineItemDomain.SubtotalBudgetComparables.Find(c => c.ComparableNumber == "C2").ComparableAmount);
                    }
                    if (comp.ComparableNumber == "C3")
                    {
                        Assert.AreEqual(comp.ComparableAmount, subtotalLineItemDomain.SubtotalBudgetComparables.Find(c => c.ComparableNumber == "C3").ComparableAmount); ;
                    }
                    if (comp.ComparableNumber == "C4")
                    {
                        Assert.AreEqual(comp.ComparableAmount, subtotalLineItemDomain.SubtotalBudgetComparables.Find(c => c.ComparableNumber == "C4").ComparableAmount);
                    }
                    if (comp.ComparableNumber == "C5")
                    {
                        Assert.AreEqual(comp.ComparableAmount, subtotalLineItemDomain.SubtotalBudgetComparables.Find(c => c.ComparableNumber == "C5").ComparableAmount);
                    }
                }
            }
        }

        [TestMethod]
        public async Task QueryWorkingBudget2Async_Success_Revenue()
        {
            glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var workingBudget2Entity = await testWorkingBudget2Repository.GetBudgetDevelopmentREVENUEWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                criteriaDomain, personId, glAccountStructure, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudget2Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                It.IsAny<WorkingBudgetQueryCriteria>(), personId, glAccountStructure, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount)).ReturnsAsync(workingBudget2Entity);
            serviceWB2 = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevDto = await serviceWB2.QueryWorkingBudget2Async(criteriaDto);

            Assert.AreEqual(buDevDto.LineItems.Count(), workingBudget2Entity.LineItems.Count());
            var domainBudgetLineItems = workingBudget2Entity.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            var dtoBudgetLineItems = buDevDto.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            foreach (var lineItemDto in dtoBudgetLineItems)
            {
                var lineItemEntity = domainBudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                foreach (var comp in lineItemDto.BudgetComparables)
                {
                    var entityComp = lineItemEntity.BudgetComparables.Where(c => c.ComparableNumber == comp.ComparableNumber).FirstOrDefault();
                    Assert.AreEqual(comp.ComparableAmount, entityComp.ComparableAmount);
                }
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        #endregion

        #region UpdateBudgetDevelopmentWorkingBudgetAsync tests

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task UpdateBudgetDevelopmentWorkingBudgetAsync_NoGlAccountStructure()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var repositoryGlAccountStructureNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlAccountStructureNullMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(null as GeneralLedgerAccountStructure);
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlAccountStructureNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var budgetLineItemsIn = new List<Dtos.BudgetManagement.BudgetLineItem>()
            {
                new Dtos.BudgetManagement.BudgetLineItem()
                {
                    BudgetAccountId = "11_00_02_00_20002_51000",
                    WorkingAmount = 5000
                }
            };
            var buDevDto = await service.UpdateBudgetDevelopmentWorkingBudgetAsync(budgetLineItemsIn);
        }

        [TestMethod]
        public async Task UpdateBudgetDevelopmentWorkingBudgetAsync_Success()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            List<string> budgetAccountIds = new List<string>()
            {
                {
                    "11_00_02_00_20002_51000"
                }
            };
            List<long?> newBudgetAmounts = new List<long?>()
            {
                {
                    5000
                }
            };
            List<string> newNotes = new List<string>()
            {
                {
                    "justification note"
                }
            };

            var lineItemEntities = await testBuDevRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, newBudgetAmounts, newNotes);
            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.UpdateBudgetDevelopmentBudgetLineItemsAsync(personId, workingBudgetId, budgetAccountIds, newBudgetAmounts, newNotes)).ReturnsAsync(lineItemEntities);
            var lineItemEntityOnDisk = await testBuDevRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds)).ReturnsAsync(lineItemEntityOnDisk);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var budgetLineItemsIn = new List<Dtos.BudgetManagement.BudgetLineItem>()
            {
                new Dtos.BudgetManagement.BudgetLineItem()
                {
                    BudgetAccountId = "11_00_02_00_20002_51000",
                    WorkingAmount = 5000,
                    JustificationNotes = "justification note"
                }
            };
            var budgetLineItemsOut = await service.UpdateBudgetDevelopmentWorkingBudgetAsync(budgetLineItemsIn);

            Assert.AreEqual(budgetLineItemsOut.Count(), lineItemEntities.Count());
            foreach (var lineItemDto in budgetLineItemsOut)
            {
                var lineItemEntity = lineItemEntities.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }
        #endregion

        #region GetBudgetDevelopmentBudgetOfficersAsync tests

        [TestMethod]
        public async Task GetBudgetDevelopmentBudgetOfficersAsync_Success()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            List<string> budgetAccountIds = new List<string>()
            {
                {
                    "11_00_02_00_20002_51000"
                }
            };

            var budgetOfficerEntities = await testBuDevRepository.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId);
            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentBudgetOfficersAsync(workingBudgetId, personId)).ReturnsAsync(budgetOfficerEntities);
            var lineItemEntityOnDisk = await testBuDevRepository.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds);
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentBudgetLineItemsAsync(workingBudgetId, personId, budgetAccountIds)).ReturnsAsync(lineItemEntityOnDisk);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var budgetOfficers = await service.GetBudgetDevelopmentBudgetOfficersAsync();
            Assert.AreEqual(budgetOfficers.Count(), budgetOfficerEntities.Count());
            foreach (var ofcr in budgetOfficers)
            {
                var ofcrEntity = budgetOfficerEntities.Where(x => x.BudgetOfficerId == ofcr.BudgetOfficerId).FirstOrDefault();
                Assert.AreEqual(ofcr.BudgetOfficerName, ofcrEntity.BudgetOfficerName);
            }
        }
        #endregion

        #region GetBudgetDevelopmentReportingUnitsAsync tests

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_SuccessOnlyOneDto()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var reportingUnitEntities = await testBuDevRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);
            var reportingUnitEntity = new List<BudgetReportingUnit>();
            reportingUnitEntity.Add(reportingUnitEntities[0]);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId)).ReturnsAsync(reportingUnitEntities);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var reportingUnitsDtos = await service.GetBudgetDevelopmentReportingUnitsAsync();
            Assert.AreEqual(reportingUnitsDtos.Count(), reportingUnitEntities.Count());
            foreach (var unit in reportingUnitsDtos)
            {
                var unitEntity = reportingUnitEntities.Where(x => x.ReportingUnitId == unit.ReportingUnitId).FirstOrDefault();
                Assert.AreEqual(unit.Description, unitEntity.Description);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentReportingUnitsAsync_SuccessMultipleDto()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var reportingUnitEntities = await testBuDevRepository.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentReportingUnitsAsync(workingBudgetId, personId)).ReturnsAsync(reportingUnitEntities);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var reportingUnitsDtos = await service.GetBudgetDevelopmentReportingUnitsAsync();
            Assert.AreEqual(reportingUnitsDtos.Count(), reportingUnitEntities.Count());
            foreach (var unit in reportingUnitsDtos)
            {
                var unitEntity = reportingUnitEntities.Where(x => x.ReportingUnitId == unit.ReportingUnitId).FirstOrDefault();
                Assert.AreEqual(unit.Description, unitEntity.Description);
            }
        }
        #endregion



        // -------------------------------------------
        //           DEPRECATED / OBSOLETE
        // -------------------------------------------


        #region DEPRECATED / OBSOLETE

        #region GetBudgetDevelopmentWorkingBudgetAsync tests

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_Success()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1);
            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), buDevEntity.BudgetLineItems.Count());
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_RevenueSuccess()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentRevenueWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1);
            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), buDevEntity.BudgetLineItems.Count());
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(-lineItemDto.BaseBudgetAmount, -lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(-lineItemDto.WorkingAmount, -lineItemEntity.WorkingAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_OnlyOneLine()
        {
            // Use only one comparable

            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentWorkingBudgetOnlyOneLineAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1);

            var repositoryOneComparableMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryOneComparableMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositoryOneComparableMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), 1);
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoComparables()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentWorkingBudgetNoC1C5Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1);

            var repositoryNoComparableMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryNoComparableMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositoryNoComparableMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), 1);
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), 0);
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NullEntity()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            // Mock the service to return a null domain entity.
            var repositoryNullDomainMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryNullDomainMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(null as WorkingBudget);

            service = new BudgetDevelopmentService(repositoryNullDomainMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);
            Assert.AreEqual(buDevDto.BudgetLineItems, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoGlAccountStructure()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var repositoryGlAccountStructureNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlAccountStructureNullMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(null as GeneralLedgerAccountStructure);
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlAccountStructureNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoGlClassConfiguration()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            var repositoryGlClassConfigurationNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlClassConfigurationNullMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(null as GeneralLedgerAccountStructure);
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlClassConfigurationNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);
        }

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_NoDescription()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentWorkingBudgetNoC1C5Async(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1);
            buDevEntity.BudgetLineItems.FirstOrDefault().BudgetAccountDescription = null;

            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;

            var repositoryNoComparableMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryNoComparableMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositoryNoComparableMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.GetBudgetDevelopmentWorkingBudgetAsync(0, 1);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), 1);
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), 0);
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }
        #endregion

        #region QueryWorkingBudgetAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryWorkingBudgetAsync_NullCriteria()
        {

            var buDevDto = await service.QueryWorkingBudgetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task QueryWorkingBudgetAsync_NoGlAccountStructure()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();

            var repositoryGlAccountStructureNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlAccountStructureNullMock.Setup(x => x.GetAccountStructureAsync()).ReturnsAsync(null as GeneralLedgerAccountStructure);
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlAccountStructureNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.QueryWorkingBudgetAsync(criteriaDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task QueryWorkingBudgetAsync_NoGlClassConfiguration()
        {
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();
            var glClassConfiguration = await testGlConfigurationRepository.GetClassConfigurationAsync();

            var repositoryGlClassConfigurationNullMock = new Mock<IGeneralLedgerConfigurationRepository>();
            repositoryGlClassConfigurationNullMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(null as GeneralLedgerClassConfiguration);
            service = new BudgetDevelopmentService(testBuDevRepository, testBuDevConfigRepository, testGlAcccountRepository, repositoryGlClassConfigurationNullMock.Object, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.QueryWorkingBudgetAsync(criteriaDto);
        }

        [TestMethod]
        public async Task QueryWorkingBudgetAsync_NullDomainEntity()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            // Mock the service to return a null domain entity.
            var repositoryNullDomainMock = new Mock<IBudgetDevelopmentRepository>();
            repositoryNullDomainMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables, null, personId, majorComponentStartPositions, 0, 1)).ReturnsAsync(null as WorkingBudget);

            service = new BudgetDevelopmentService(repositoryNullDomainMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);

            var buDevDto = await service.QueryWorkingBudgetAsync(criteriaDto);
            Assert.AreEqual(buDevDto.BudgetLineItems, null);
            Assert.AreEqual(buDevDto.TotalLineItems, 0);
        }

        [TestMethod]
        public async Task QueryWorkingBudgetAsync_Success()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                criteriaDomain, personId, majorComponentStartPositions, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                It.IsAny<WorkingBudgetQueryCriteria>(), personId, majorComponentStartPositions, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevDto = await service.QueryWorkingBudgetAsync(criteriaDto);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), buDevEntity.BudgetLineItems.Count());
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        [TestMethod]
        public async Task QueryWorkingBudgetAsync_Success_Revenue()
        {
            var glAccountStructure = await testGlConfigurationRepository.GetAccountStructureAsync();
            majorComponentStartPositions = glAccountStructure.MajorComponentStartPositions;
            budgetConfiguration = await testBuDevConfigRepository.GetBudgetDevelopmentConfigurationAsync();

            var buDevEntity = await testBuDevRepository.GetBudgetDevelopmentRevenueWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                criteriaDomain, personId, majorComponentStartPositions, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount);

            var repositorySuccessMock = new Mock<IBudgetDevelopmentRepository>();
            repositorySuccessMock.Setup(x => x.GetBudgetDevelopmentWorkingBudgetAsync(workingBudgetId, budgetConfiguration.BudgetConfigurationComparables,
                It.IsAny<WorkingBudgetQueryCriteria>(), personId, majorComponentStartPositions, criteriaDomain.StartLineItem, criteriaDomain.LineItemCount)).ReturnsAsync(buDevEntity);
            service = new BudgetDevelopmentService(repositorySuccessMock.Object, testBuDevConfigRepository, testGlAcccountRepository, testGlConfigurationRepository, adapterRegistry.Object, userFactory, roleRepositoryMock.Object, logger.Object);
            var buDevDto = await service.QueryWorkingBudgetAsync(criteriaDto);

            Assert.AreEqual(buDevDto.BudgetLineItems.Count(), buDevEntity.BudgetLineItems.Count());
            foreach (var lineItemDto in buDevDto.BudgetLineItems)
            {
                var lineItemEntity = buDevEntity.BudgetLineItems.Where(x => x.BudgetAccountId == lineItemDto.BudgetAccountId).FirstOrDefault();
                Assert.AreEqual(lineItemDto.BudgetComparables.Count(), lineItemEntity.BudgetComparables.Count());
                foreach (var comp in lineItemDto.BudgetComparables)
                {
                    var entityComp = lineItemEntity.BudgetComparables.Where(c => c.ComparableNumber == comp.ComparableNumber).FirstOrDefault();
                    Assert.AreEqual(comp.ComparableAmount, entityComp.ComparableAmount);
                }
                Assert.AreEqual(lineItemDto.BudgetAccountDescription, lineItemEntity.BudgetAccountDescription);
                Assert.AreEqual(lineItemDto.BaseBudgetAmount, lineItemEntity.BaseBudgetAmount);
                Assert.AreEqual(lineItemDto.FormattedBudgetAccountId, lineItemEntity.FormattedBudgetAccountId);
                Assert.AreEqual(lineItemDto.WorkingAmount, lineItemEntity.WorkingAmount);
            }
        }

        #endregion

        #endregion
    }
}
