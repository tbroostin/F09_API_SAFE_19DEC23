// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Finance.Services;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Finance.Tests;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class PaymentPlanServiceTests : FinanceCoordinationTests
    {
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IPaymentPlanRepository> ppRepoMock;
        private IPaymentPlanRepository ppRepo;
        private Mock<IAccountsReceivableRepository> arRepoMock;
        private IAccountsReceivableRepository arRepo;
        private Mock<IPaymentRepository> payRepoMock;
        private IPaymentRepository payRepo;
        private Mock<IRegistrationBillingRepository> rbRepoMock;
        private IRegistrationBillingRepository rbRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IFinanceConfigurationRepository> financeConfigurationRepoMock;
        private IFinanceConfigurationRepository financeConfigurationRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private PaymentPlanService service;

        private List<Domain.Finance.Entities.PaymentPlanTemplate> allTemplates;
        private List<Domain.Finance.Entities.PaymentPlan> allPlans;
        private List<Domain.Finance.Entities.PaymentPlanApproval> allPlanApprovals;
        private List<Domain.Finance.Entities.ReceivableType> allReceivableTypes;
        private Domain.Finance.Entities.Configuration.FinanceConfiguration financeConfigurationEntity;
        private Domain.Finance.Entities.AccountHolder accountHolder;
        private Domain.Finance.Entities.PaymentPlanTemplate template;
        private List<Domain.Finance.Entities.Invoice> invoices;
        private List<Domain.Finance.Entities.Charge> charges;
        private List<Domain.Finance.Entities.ChargeCode> allChargeCodes;


        private List<PaymentPlan> allPlanDtos;
        private PaymentPlanTermsAcceptance acceptanceDto;
        private List<RuleResult> ruleResults;

        [TestInitialize]
        public void Initialize()
        {
            SetupAdapters();
            SetupData();
            SetupRepositories();

            userFactory = new FinanceCoordinationTests.StudentUserFactory();
            BuildService();
        }

        private void BuildService()
        {
            service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            roleRepoMock = null;
            roleRepo = null;
            ppRepoMock = null;
            ppRepo = null;
            arRepoMock = null;
            arRepo = null;
            payRepoMock = null;
            payRepo = null;
            rbRepoMock = null;
            rbRepo = null;
            userFactory = null;
            service = null;

            allTemplates = null;
            allPlans = null;
            allPlanApprovals = null;
            allReceivableTypes = null;
        }

        [TestClass]
        public class PaymentPlanService_GetPaymentPlanTemplates : PaymentPlanServiceTests
        {
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlanTemplates_Valid()
            {
                var templatesList = service.GetPaymentPlanTemplates().ToList();
                Assert.AreEqual(allTemplates.Count(), templatesList.Count);
            }
        }

        [TestClass]
        public class PaymentPlanService_GetPaymentPlanTemplate : PaymentPlanServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlanTemplate_Null()
            {
                var template = service.GetPaymentPlanTemplate(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlanTemplate_Empty()
            {
                var template = service.GetPaymentPlanTemplate(string.Empty);
            }

            [TestMethod]
            public void PaymentPlanService_GetPaymentPlanTemplate_Valid()
            {
                foreach (var pptid in allTemplates.Select(ppt => ppt.Id))
                {
                    var expected = allTemplates.Where(ppt => ppt.Id == pptid).FirstOrDefault();
                    var template = service.GetPaymentPlanTemplate(pptid);
                    Assert.IsNotNull(template);
                }
            }
        }

        [TestClass]
        public class PaymentPlanService_GetPaymentPlan : PaymentPlanServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlan_Null()
            {
                var template = service.GetPaymentPlan(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlan_Empty()
            {
                var template = service.GetPaymentPlan(string.Empty);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlan_UnauthorizedUser()
            {
                var plan = service.GetPaymentPlan("1");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlan_Valid()
            {
                var plan = service.GetPaymentPlan("1111");
                Assert.IsNotNull(plan);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlan_AdminUser()
            {
                SetupAdminUser();
                
                var plan = service.GetPaymentPlan("1111");
                Assert.IsNotNull(plan);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlan_AdminUserNoPermissions()
            {
                SetupAdminUserWithNoPermissions();

                service.GetPaymentPlan("1111");
                
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlan_ProxyUser()
            {
                SetupProxyUser();

                var plan = service.GetPaymentPlan("6666");
                Assert.IsNotNull(plan);
            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlan_ProxyUserDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetPaymentPlan("6666");                
            }
        }

        [TestClass]
        public class PaymentPlanService_ApprovePaymentPlanTerms : PaymentPlanServiceTests
        {
            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_ApprovePaymentPlanTerms_UnauthorizedUser()
            {
                var proposedPlan = allPlanDtos.Where(ppln => ppln.PersonId == "0003315").FirstOrDefault();
                if (proposedPlan != null)
                {
                    acceptanceDto = new PaymentPlanTermsAcceptance()
                    {
                        AcknowledgementDateTime = DateTimeOffset.UtcNow.AddMinutes(-3),
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text", "of your plan." },
                        ApprovalReceived = DateTimeOffset.UtcNow,
                        DownPaymentAmount = proposedPlan.DownPaymentAmount,
                        DownPaymentDate = proposedPlan.DownPaymentDate,
                        PaymentControlId = "123",
                        ProposedPlan = proposedPlan,
                        RegistrationApprovalId = "123",
                        StudentId = proposedPlan.PersonId,
                        StudentName = "John Smith",
                        TermsText = new List<string>() { "These are the terms", "of your plan." },
                    };

                    var approval = service.ApprovePaymentPlanTerms(acceptanceDto);
                }
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_ApprovePaymentPlanTerms_Valid()
            {
                var proposedPlan = allPlanDtos.Where(ppln => ppln.PersonId == userFactory.CurrentUser.PersonId).FirstOrDefault();
                if (proposedPlan != null && proposedPlan.ScheduledPayments != null)
                {
                    acceptanceDto = new PaymentPlanTermsAcceptance()
                    {
                        AcknowledgementDateTime = DateTimeOffset.UtcNow.AddMinutes(-3),
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text", "of your plan." },
                        ApprovalReceived = DateTimeOffset.UtcNow,
                        DownPaymentAmount = proposedPlan.DownPaymentAmount,
                        DownPaymentDate = proposedPlan.DownPaymentDate,
                        PaymentControlId = "124",
                        ProposedPlan = proposedPlan,
                        RegistrationApprovalId = "124",
                        StudentId = proposedPlan.PersonId,
                        StudentName = "Johnny",
                        TermsText = new List<string>() { "These are the terms", "of your plan." },
                    };
                    ppRepoMock.Setup(repo => repo.ApprovePaymentPlanTerms(It.IsAny<Domain.Finance.Entities.PaymentPlanTermsAcceptance>())).Returns(allPlanApprovals.Where(ppa => ppa.StudentId == userFactory.CurrentUser.PersonId).FirstOrDefault());
                    var approval = service.ApprovePaymentPlanTerms(acceptanceDto);
                    Assert.IsNotNull(approval);
                }
            }

            /// <summary>
            /// Admin cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_ApprovePaymentPlanTerms_AdminUser()
            {
                SetupAdminUser();
                var proposedPlan = allPlanDtos.Where(ppln => ppln.PersonId == "0003315").FirstOrDefault();
                if (proposedPlan != null)
                {
                    acceptanceDto = new PaymentPlanTermsAcceptance()
                    {
                        AcknowledgementDateTime = DateTimeOffset.UtcNow.AddMinutes(-3),
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text", "of your plan." },
                        ApprovalReceived = DateTimeOffset.UtcNow,
                        DownPaymentAmount = proposedPlan.DownPaymentAmount,
                        DownPaymentDate = proposedPlan.DownPaymentDate,
                        PaymentControlId = "123",
                        ProposedPlan = proposedPlan,
                        RegistrationApprovalId = "123",
                        StudentId = proposedPlan.PersonId,
                        StudentName = "John Smith",
                        TermsText = new List<string>() { "These are the terms", "of your plan." },
                    };

                    service.ApprovePaymentPlanTerms(acceptanceDto);
                }
            }

            /// <summary>
            /// Proxy cannot accept terms
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_ApprovePaymentPlanTerms_ProxyUser()
            {
                SetupProxyUser();
                var proposedPlan = allPlanDtos.Where(ppln => ppln.PersonId == "0003315").FirstOrDefault();
                if (proposedPlan != null)
                {
                    acceptanceDto = new PaymentPlanTermsAcceptance()
                    {
                        AcknowledgementDateTime = DateTimeOffset.UtcNow.AddMinutes(-3),
                        AcknowledgementText = new List<string>() { "This is the acknowledgement text", "of your plan." },
                        ApprovalReceived = DateTimeOffset.UtcNow,
                        DownPaymentAmount = proposedPlan.DownPaymentAmount,
                        DownPaymentDate = proposedPlan.DownPaymentDate,
                        PaymentControlId = "123",
                        ProposedPlan = proposedPlan,
                        RegistrationApprovalId = "123",
                        StudentId = proposedPlan.PersonId,
                        StudentName = "John Smith",
                        TermsText = new List<string>() { "These are the terms", "of your plan." },
                    };

                    service.ApprovePaymentPlanTerms(acceptanceDto);
                }
            }
        }

        [TestClass]
        public class PaymentPlanService_GetPaymentPlanApproval : PaymentPlanServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlanApproval_Null()
            {
                var template = service.GetPaymentPlanApproval(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPaymentPlanApproval_Empty()
            {
                var template = service.GetPaymentPlanApproval(string.Empty);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlanApproval_UnauthorizedUser()
            {
                var expected = allPlanApprovals.Where(ppa => ppa.Id == "123").FirstOrDefault();
                var plan = service.GetPaymentPlanApproval("123");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlanApproval_Valid()
            {
                var expected = allPlanApprovals.Where(ppa => ppa.Id == "124").FirstOrDefault();
                var planApproval = service.GetPaymentPlanApproval("124");
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlanApproval_UserIsAdmin()
            {
                SetupAdminUser();
                Assert.IsNotNull(service.GetPaymentPlanApproval("124"));
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlanApproval_UserIsAdminWithNoPermissions()
            {
                SetupAdminUserWithNoPermissions();
                service.GetPaymentPlanApproval("124");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPaymentPlanApproval_UserIsProxy()
            {
                SetupProxyUser();
                Assert.IsNotNull(service.GetPaymentPlanApproval("123"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPaymentPlanApproval_UserIsProxyForDifferentPerson()
            {
                SetupProxyUserForDifferentPerson();
                service.GetPaymentPlanApproval("123");
            }
        }

        [TestClass]
        public class PaymentPlanService_GetPaymentPlanSummary : PaymentPlanServiceTests
        {

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPlanPaymentSummary_NullPlanId()
            {
                var summary = service.GetPlanPaymentSummary(null, "CC", 500m, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPlanPaymentSummary_Empty()
            {
                var summary = service.GetPlanPaymentSummary(string.Empty, "CC", 500m, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPlanPaymentSummary_NullPaymentMethod()
            {
                var summary = service.GetPlanPaymentSummary("012", null, 500m, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanService_GetPlanPaymentSummary_EmptyPaymentMethod()
            {
                var summary = service.GetPlanPaymentSummary("012", string.Empty, 500m, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void PaymentPlanService_GetPlanPaymentSummary_NegativeAmount()
            {
                var summary = service.GetPlanPaymentSummary("012", "CC", -500m, "123");
            }

            /// <summary>
            /// User is not self
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPlanPaymentSummary_UnauthorizedUser()
            {
                var summary = service.GetPlanPaymentSummary("1", "CC", 500m, "124");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentPlanService_GetPlanPaymentSummary_Valid()
            {
                var summary = service.GetPlanPaymentSummary("1111", "CC", 500m, "124");
                Assert.IsNotNull(summary);
            }

            /// <summary>
            /// Admin cannot access data test
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPlanPaymentSummary_AdminUser()
            {
                SetupAdminUser();
                var summary = service.GetPlanPaymentSummary("1111", "CC", 500m, "124");
            }

            /// <summary>
            /// Proxy cannot access data test
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentPlanService_GetPlanPaymentSummary_ProxyUser()
            {
                SetupProxyUser();
                var summary = service.GetPlanPaymentSummary("6666", "CC", 500m, "124");
            }
        }

        [TestClass]
        public class PaymentPlanService_GetBillingTermPaymentPlanInformationAsync : PaymentPlanServiceTests
        {

            private List<BillingTermPaymentPlanInformation> criteria = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "TERM1", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
             };

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_QueryCriteria_Null()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_BillingTerms_Empty()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(new List<BillingTermPaymentPlanInformation>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_BillingTerms_Null()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(null);
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_PermissionsException()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(new List<BillingTermPaymentPlanInformation>() {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001235", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
                });
            }

            /// <summary>
            /// Admin cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_AdminNoAccess()
            {
                SetupAdminUser();
                await service.GetBillingTermPaymentPlanInformationAsync(criteria);
            }

            /// <summary>
            /// Proxy cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ProxyNoAccess()
            {
                SetupProxyUser();
                criteria.ForEach(c => c.PersonId = userFactory.CurrentUser.ProxySubjects.First().PersonId);
                await service.GetBillingTermPaymentPlanInformationAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_FinanceConfiguration_Null()
            {
                financeConfigurationEntity = null;
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_FinanceConfiguration_UserPaymentPlanCreation_False()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = false
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_FinanceConfiguration_TermPaymentPlanRequirements_Empty()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_FinanceConfiguration_No_PayableReceivableTypes()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", false));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", false));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_PaymentPlanEligiblityRule_Fail()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = false, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_No_PaymentPlanRequirements_For_BillingTerm()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(new List<BillingTermPaymentPlanInformation>()
                    {
                        new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "ABCDEF", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m }
                    });
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_No_PaymentPlanOptions_for_BillingTerm()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", true));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-8), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today.AddDays(15))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("3", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_ValidTemplate()
            {
                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.EligibleItems.Any());
                Assert.IsNull(result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_InactiveTemplate()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", false, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_Template_AutoCalculateFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = false,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_Template_AutoModifyFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = false
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_Template_NoTermsAndConditions()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = null,
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_Template_MinimumPlanAmount()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 999999999m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_Template_ReceivableType_NotAllowed()
            {
                var template = new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                };
                template.AddAllowedReceivableTypeCode("05");
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_ValidPaymentPlanOption_for_BillingTerm_NullTemplate()
            {
                template = null;
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformationAsync_PaymentPlanEligiblityRule_Pass()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = true, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformationAsync(criteria);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.EligibleItems.Any());
                Assert.IsNull(result.IneligibilityReason);
            }
        }

        [TestClass]
        public class PaymentPlanService_GetBillingTermPaymentPlanInformation2Async : PaymentPlanServiceTests
        {

            private PaymentPlanQueryCriteria criteria = new PaymentPlanQueryCriteria()
            {
                BillingTerms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "TERM1", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
                }
            };

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_QueryCriteria_Null()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_BillingTerms_Empty()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(new PaymentPlanQueryCriteria() { BillingTerms = new List<BillingTermPaymentPlanInformation>() });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_BillingTerms_Null()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(new PaymentPlanQueryCriteria() { BillingTerms = null });
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_PermissionsException()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(new PaymentPlanQueryCriteria()
                {
                    BillingTerms = new List<BillingTermPaymentPlanInformation>() {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001235", TermId = "TERM2", PaymentPlanTemplateId = "UNIQUE", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m }
                }
                });
            }

            /// <summary>
            /// Admin cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_AdminNoAccess()
            {
                SetupAdminUser();
                await service.GetBillingTermPaymentPlanInformation2Async(criteria);
            }

            /// <summary>
            /// Proxy cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ProxyNoAccess()
            {
                SetupProxyUser();
                criteria.BillingTerms.ToList().ForEach(c => c.PersonId = userFactory.CurrentUser.ProxySubjects.First().PersonId);
                await service.GetBillingTermPaymentPlanInformation2Async(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_FinanceConfiguration_Null()
            {
                financeConfigurationEntity = null;
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_FinanceConfiguration_UserPaymentPlanCreation_False()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = false
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_FinanceConfiguration_TermPaymentPlanRequirements_Empty()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_FinanceConfiguration_No_PayableReceivableTypes()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", false));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", false));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_PaymentPlanEligiblityRule_Fail()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = false, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_No_PaymentPlanRequirements_For_BillingTerm()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(new PaymentPlanQueryCriteria()
                {
                    BillingTerms = new List<BillingTermPaymentPlanInformation>()
                    {
                        new BillingTermPaymentPlanInformation() { PersonId = "0000895", TermId = "ABCDEF", PaymentPlanTemplateId = "DEFAULT", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m }
                    }
                });
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_No_PaymentPlanOptions_for_BillingTerm()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", true));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-8), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today.AddDays(15))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("3", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_ValidTemplate()
            {
                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.EligibleItems.Any());
                Assert.IsNull(result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_InactiveTemplate()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", false, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_Template_AutoCalculateFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = false,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_Template_AutoModifyFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = false
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_Template_NoTermsAndConditions()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = null,
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_Template_MinimumPlanAmount()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 999999999m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_Template_ReceivableType_NotAllowed()
            {
                var template = new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                };
                template.AddAllowedReceivableTypeCode("05");
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.ChargesAreNotEligible, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_ValidPaymentPlanOption_for_BillingTerm_NullTemplate()
            {
                template = null;
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsFalse(result.EligibleItems.Any());
                Assert.AreEqual(PaymentPlanIneligibilityReason.PreventedBySystemConfiguration, result.IneligibilityReason);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetBillingTermPaymentPlanInformation2Async_PaymentPlanEligiblityRule_Pass()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = true, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetBillingTermPaymentPlanInformation2Async(criteria);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.EligibleItems.Any());
                Assert.IsNull(result.IneligibilityReason);
            }
        }

        [TestClass]
        public class PaymentPlanService_GetProposedPaymentPlanAsync : PaymentPlanServiceTests
        {
            private string personId = "0000895";
            private string termId = "TERM1";
            private string receivableTypeCode = "01";
            private decimal planAmount = 1000m;

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_PersonId_Null()
            {
                var result = await service.GetProposedPaymentPlanAsync(null, termId, receivableTypeCode, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_TermId_Null()
            {
                var result = await service.GetProposedPaymentPlanAsync(personId, null, receivableTypeCode, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ReceivableTypeCode_Null()
            {
                var result = await service.GetProposedPaymentPlanAsync(personId, termId, null, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_PlanAmount_Invalid()
            {
                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, -(planAmount));
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_PermissionsException()
            {
                var result = await service.GetProposedPaymentPlanAsync("0001235", termId, receivableTypeCode, planAmount);
            }

            /// <summary>
            /// Admin cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_AdminUserNoAccess()
            {
                SetupAdminUser();
                await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }

            /// <summary>
            /// Proxy cannot access data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ProxyUserNoAccess()
            {
                SetupProxyUser();
                personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_FinanceConfiguration_Null()
            {
                financeConfigurationEntity = null;
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_FinanceConfiguration_UserPaymentPlanCreation_False()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = false
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_FinanceConfiguration_TermPaymentPlanRequirements_Empty()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_No_PayableReceivableTypes()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", false));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", false));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_PaymentPlanEligiblityRule_Fail()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = false, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_No_PaymentPlanRequirements_For_BillingTerm()
            {
                var result = await service.GetProposedPaymentPlanAsync(personId, "ABCDEF", receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_No_PaymentPlanRequirements_For_BillingTerm_2()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", true));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM1", "RULE2", 2, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(8), DateTime.Today.AddDays(15), "TEMPLATE2", DateTime.Today.AddDays(21))
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                ruleResults = new List<RuleResult>()
                {
                    new RuleResult() { Passed = false }
                };
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(ruleResults);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, "TERM1", receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_No_PaymentPlanOptions_for_BillingTerm()
            {
                financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
                {
                    ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Institution Name",
                    Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                    NotificationText = "This is a notification",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                    PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                    PaymentReviewMessage = "Review your payment.",
                    Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                    RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    SupportEmailAddress = "support@ellucian.edu",
                    StatementTitle = "Student Statement",
                    UseGuaranteedChecks = true,
                    StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                    UserPaymentPlanCreationEnabled = true
                };
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
                financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("02", true));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-8), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today.AddDays(15))
                }));
                financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("3", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

                financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
                financeConfigurationRepo = financeConfigurationRepoMock.Object;
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_ValidTemplate()
            {
                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_InactiveTemplate()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", false, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_Template_AutoCalculateFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = false,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_Template_AutoModifyFalse()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = false
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_Template_NoTermsAndConditions()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = null,
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_Template_MinimumPlanAmount()
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 999999999m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_Template_ReceivableType_NotAllowed()
            {
                var template = new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
                {
                    CalculatePlanAmountAutomatically = true,
                    TermsAndConditionsDocumentId = "ABCD",
                    ModifyPlanAutomatically = true
                };
                template.AddAllowedReceivableTypeCode("05");
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_ValidPaymentPlanOption_for_BillingTerm_NullTemplate()
            {
                template = null;
                ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }

            [TestMethod]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_PaymentPlanEligiblityRule_Pass()
            {
                ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(new List<RuleResult>() { new RuleResult() { Passed = true, RuleId = "ALLREG" } });
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanService_GetProposedPaymentPlanAsync_Repository_Throws_Exception()
            {
                ppRepoMock.Setup(repo => repo.GetProposedPaymentPlanAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>())).ThrowsAsync(new ApplicationException("Error occurred generating plan details."));
                service = new PaymentPlanService(adapterRegistry, ppRepo, arRepo, payRepo, rbRepo, financeConfigurationRepo, ruleRepo, userFactory, roleRepo, logger);

                var result = await service.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);
            }
        }


        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var templateAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTemplate, PaymentPlanTemplate>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTemplate, PaymentPlanTemplate>()).Returns(templateAdapter);

            var planAdapter = new PaymentPlanEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan, PaymentPlan>()).Returns(planAdapter);

            var planStatusAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, PlanStatus>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, PlanStatus>()).Returns(planStatusAdapter);

            var scheduledPaymentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment, ScheduledPayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment, ScheduledPayment>()).Returns(scheduledPaymentAdapter);

            var planChargeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, PlanCharge>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, PlanCharge>()).Returns(planChargeAdapter);

            var chargeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Charge, Charge>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Charge>()).Returns(chargeEntityAdapter);

            var planApprovalAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentPlanApproval, PaymentPlanApproval>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanApproval, PaymentPlanApproval>()).Returns(planApprovalAdapter);

            var planScheduleAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanSchedule, PlanSchedule>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanSchedule, PlanSchedule>()).Returns(planScheduleAdapter);

            var planTermsAcceptanceDtoAdapter = new PaymentPlanTermsAcceptanceDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<PaymentPlanTermsAcceptance, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTermsAcceptance>()).Returns(planTermsAcceptanceDtoAdapter);

            var scheduledPaymentDtoAdapter = new AutoMapperAdapter<ScheduledPayment, Domain.Finance.Entities.ScheduledPayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<ScheduledPayment, Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment>()).Returns(scheduledPaymentDtoAdapter);

            var planStatusDtoAdapter = new AutoMapperAdapter<PlanStatus, Domain.Finance.Entities.PlanStatus>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<PlanStatus, Ellucian.Colleague.Domain.Finance.Entities.PlanStatus>()).Returns(planStatusDtoAdapter);

            var planDtoAdapter = new AutoMapperAdapter<PaymentPlan, Domain.Finance.Entities.PaymentPlan>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<PaymentPlan, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan>()).Returns(planDtoAdapter);

            var paymentAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.Payment, Dtos.Finance.Payments.Payment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.Payment, Dtos.Finance.Payments.Payment>()).Returns(paymentAdapter);

            var paymentItemAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Payments.PaymentItem, Dtos.Finance.Payments.PaymentItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.Payments.PaymentItem, Dtos.Finance.Payments.PaymentItem>()).Returns(paymentItemAdapter);

            var billingTermPaymentPlanInformationDtoAdapter = new BillingTermPaymentPlanInformationDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<BillingTermPaymentPlanInformation, Domain.Finance.Entities.BillingTermPaymentPlanInformation>()).Returns(billingTermPaymentPlanInformationDtoAdapter);

            var billingTermPaymentPlanInformationEntityAdapter = new BillingTermPaymentPlanInformationEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Finance.Entities.BillingTermPaymentPlanInformation, BillingTermPaymentPlanInformation>()).Returns(billingTermPaymentPlanInformationEntityAdapter);
        }

        private void SetupData()
        {
            allTemplates = TestPaymentPlanTemplateRepository.PayPlanTemplates.ToList();
            allPlans = TestPaymentPlanRepository.PayPlans.ToList();
            allPlanApprovals = TestPaymentPlanApprovalRepository.PayPlanApprovals.ToList();
            allReceivableTypes = TestReceivableTypesRepository.ReceivableTypes.ToList();
            allChargeCodes = TestChargeCodesRepository.ChargeCodes.ToList();

            allPlanDtos = new List<PaymentPlan>();
            var plansTemp = allPlans;
            foreach (var plan in plansTemp)
            {
                allPlanDtos.Add(new PaymentPlanEntityAdapter(adapterRegistry, logger).MapToType(plan));
            }
            // Need to null out plan IDs for use as proposed plan
            foreach (var planDto in allPlanDtos)
            {
                if (planDto != null)
                {
                    planDto.Id = null;
                    if (planDto != null && planDto.PlanCharges != null && planDto.PlanCharges.Count() > 0)
                    {
                        foreach (var pc in planDto.PlanCharges)
                        {
                            pc.PlanId = null;
                        }
                    }
                    if (planDto != null && planDto.ScheduledPayments != null && planDto.ScheduledPayments.Count() > 0)
                    {
                        foreach (var sp in planDto.ScheduledPayments)
                        {
                            sp.Id = null;
                            sp.PlanId = null;
                        }
                    }
                }
            }

            financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
            {
                ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                ECommercePaymentsAllowed = true,
                IncludeDetail = true,
                IncludeHistory = true,
                IncludeSchedule = true,
                InstitutionName = "Institution Name",
                Links = new List<Domain.Finance.Entities.StudentFinanceLink>()
                {
                    new Domain.Finance.Entities.StudentFinanceLink("Ellucian University", "http://www.ellucian.edu")
                },
                NotificationText = "This is a notification",
                PartialAccountPaymentsAllowed = true,
                PartialDepositPaymentsAllowed = true,
                PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod,
                PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "MasterCard",
                        InternalCode = "MC",
                        Type = "Credit Card"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "ECheck",
                        InternalCode = "ECHK",
                        Type = "Electronic Check"
                    }
                },
                PaymentReviewMessage = "Review your payment.",
                Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-30)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-29), DateTime.Today.AddDays(29)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddDays(30), null)
                },
                RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                SelfServicePaymentsAllowed = true,
                ShowCreditAmounts = true,
                SupportEmailAddress = "support@ellucian.edu",
                StatementTitle = "Student Statement",
                UseGuaranteedChecks = true,
                StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2" },
                UserPaymentPlanCreationEnabled = true
            };
            financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
            financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
            financeConfigurationEntity.AddPaymentPlanEligibilityRuleId("ALLREG");
            financeConfigurationEntity.AddPaymentPlanEligibilityRuleId("STUDENTS");
            financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
            financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-8), DateTime.Today.AddDays(8), "TEMPLATE2", DateTime.Today.AddDays(15))
                }));
            financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("3", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

            accountHolder = new Domain.Finance.Entities.AccountHolder("0000895", "Smith");

            ruleResults = new List<RuleResult>()
            {
                new RuleResult() { Passed = true, RuleId = "ALLREG" }
            };

            template = new Domain.Finance.Entities.PaymentPlanTemplate("DEFAULT", "Default", true, Domain.Finance.Entities.PlanFrequency.Weekly, 5, 0m, null, null)
            {
                CalculatePlanAmountAutomatically = true,
                ModifyPlanAutomatically = true,
                TermsAndConditionsDocumentId = "PPT&C"
            };
            template.AddAllowedReceivableTypeCode("01");

            charges = new List<Domain.Finance.Entities.Charge>()
            {
                new Domain.Finance.Entities.Charge("1", "10", new List<string>() { "Charge #1" }, "TUIFT", 1000m)
            };

            invoices = new List<Domain.Finance.Entities.Invoice>()
            {
                new Domain.Finance.Entities.Invoice("10", "0000895", "01", "TERM1", "INV1", DateTime.Today.AddMonths(-1),
                    DateTime.Today.AddDays(7), DateTime.Today.AddDays(-60), DateTime.Today.AddDays(60), "Invoice #1", charges)
            };
        }

        private void SetupRepositories()
        {
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            ppRepoMock = new Mock<IPaymentPlanRepository>();
            ppRepo = ppRepoMock.Object;
            ppRepoMock.Setup(repo => repo.GetTemplate(It.IsAny<string>())).Returns(template);
            ppRepoMock.Setup(repo => repo.PaymentPlanTemplates).Returns(allTemplates);
            foreach (var pptid in allTemplates.Select(ppt => ppt.Id))
            {
                ppRepoMock.Setup(repo => repo.GetTemplate(pptid)).Returns(allTemplates.Where(ppt => ppt.Id == pptid).FirstOrDefault());
            }
            foreach (var ppid in allPlans.Select(ppln => ppln.Id))
            {
                ppRepoMock.Setup(repo => repo.GetPaymentPlan(ppid)).Returns(allPlans.Where(ppln => ppln.Id == ppid).FirstOrDefault());
            }
            foreach (var ppaid in allPlanApprovals.Select(ppa => ppa.Id))
            {
                ppRepoMock.Setup(repo => repo.GetPaymentPlanApproval(ppaid)).Returns(allPlanApprovals.Where(ppa => ppa.Id == ppaid).FirstOrDefault());
            }
            ppRepoMock.Setup(repo => repo.GetProposedPaymentPlanAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>())).ReturnsAsync(TestPaymentPlanRepository.PayPlans.First());
            ppRepoMock.Setup(repo => repo.GetPlanCustomScheduleDates(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime>(), It.IsAny<string>())).Returns(new List<DateTime?>()
                {
                    DateTime.Today.AddDays(7),
                    DateTime.Today.AddDays(14),
                    DateTime.Today.AddDays(21),
                    DateTime.Today.AddDays(28),
                    DateTime.Today.AddDays(35),
                });

            arRepoMock = new Mock<IAccountsReceivableRepository>();
            arRepo = arRepoMock.Object;
            arRepoMock.Setup(repo => repo.GetDistribution(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("BANK");
            arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(allReceivableTypes);
            arRepoMock.Setup(repo => repo.GetAccountHolder(It.IsAny<string>())).Returns(accountHolder);
            arRepoMock.Setup(repo => repo.ChargeCodes).Returns(allChargeCodes);

            payRepoMock = new Mock<IPaymentRepository>();
            payRepo = payRepoMock.Object;
            payRepoMock.Setup(repo => repo.GetConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(
                new Domain.Finance.Entities.Payments.PaymentConfirmation()
                {
                    ConfirmationText = new List<string>() { "You have successfully", "made a payment." },
                    ConvenienceFeeAmount = 1.50m,
                    ConvenienceFeeCode = "CF15",
                    ConvenienceFeeDescription = "1.5% Convenience Fee",
                    ConvenienceFeeGeneralLedgerNumber = "110101000000010001",
                    ProviderAccount = "ECOMMCC"
                });

            rbRepoMock = new Mock<IRegistrationBillingRepository>();
            rbRepo = rbRepoMock.Object;

            financeConfigurationRepoMock = new Mock<IFinanceConfigurationRepository>();
            financeConfigurationRepo = financeConfigurationRepoMock.Object;
            financeConfigurationRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;
            ruleRepoMock.Setup(repo => repo.Execute<Domain.Finance.Entities.AccountHolder>(It.IsAny<IEnumerable<RuleRequest<Domain.Finance.Entities.AccountHolder>>>())).Returns(ruleResults);

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
        }

        private void SetupAdminUser()
        {
            userFactory = new FinanceCoordinationTests.CurrentUserFactory();
            financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
            roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
            BuildService();
        }

        private void SetupAdminUserWithNoPermissions()
        {
            userFactory = new FinanceCoordinationTests.CurrentUserFactory();
            BuildService();
        }

        private void SetupProxyUser()
        {
            userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
            BuildService();
        }

        private void SetupProxyUserForDifferentPerson()
        {
            userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
            BuildService();
        }
    }
}
