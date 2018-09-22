// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
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

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class FinanceConfigurationServiceTests : FinanceCoordinationTests
    {
        private Mock<IFinanceConfigurationRepository> configRepoMock;
        private IFinanceConfigurationRepository configRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private FinanceConfigurationService service;

        private Domain.Finance.Entities.Configuration.FinanceConfiguration financeConfigurationEntity;
        private Domain.Finance.Entities.ImmediatePaymentControl ipcConfigurationEntity;

        [TestInitialize]
        public void Initialize()
        {
            SetupAdapters();
            SetupData();
            SetupRepositories();

            userFactory = new FinanceCoordinationTests.StudentUserFactory();
            service = new FinanceConfigurationService(adapterRegistry, configRepo);
        }

        [TestCleanup]
        public void Cleanup()
        {
            configRepoMock = null;
            configRepo = null;
            userFactory = null;
            service = null;

            financeConfigurationEntity = null;
            ipcConfigurationEntity = null;
        }

        [TestClass]
        public class FinanceConfigurationService_GetFinanceConfiguration : FinanceConfigurationServiceTests
        {
            [TestMethod]
            public void FinanceConfigurationService_GetFinanceConfiguration_Valid()
            {
                var conf = service.GetFinanceConfiguration();
                Assert.IsNotNull(conf);
            }
        }

        [TestClass]
        public class FinanceConfigurationService_GetImmediatePaymentControl : FinanceConfigurationServiceTests
        {
            [TestMethod]
            public void FinanceConfigurationService_GetImmediatePaymentControl_Valid()
            {
                var conf = service.GetImmediatePaymentControl();
                Assert.IsNotNull(conf);
            }
        }

        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var financeConfigurationAdapter = new FinanceConfigurationEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.FinanceConfiguration, Dtos.Finance.Configuration.FinanceConfiguration>()).Returns(financeConfigurationAdapter);

            var partialPlanPaymentsEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.PartialPlanPayments, Dtos.Finance.Configuration.PartialPlanPayments>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PartialPlanPayments, Dtos.Finance.Configuration.PartialPlanPayments>()).Returns(partialPlanPaymentsEntityAdapter);

            var activityDisplayEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.ActivityDisplay, Dtos.Finance.Configuration.ActivityDisplay>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, Dtos.Finance.Configuration.ActivityDisplay>()).Returns(activityDisplayEntityAdapter);

            var paymentDisplayEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.PaymentDisplay, Dtos.Finance.Configuration.PaymentDisplay>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PaymentDisplay, Dtos.Finance.Configuration.PaymentDisplay>()).Returns(paymentDisplayEntityAdapter);

            var availablePaymentMethodEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.AvailablePaymentMethod, Dtos.Finance.Configuration.AvailablePaymentMethod>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.AvailablePaymentMethod, Dtos.Finance.Configuration.AvailablePaymentMethod>()).Returns(availablePaymentMethodEntityAdapter);

            var financialPeriodEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.FinancialPeriod, FinancialPeriod>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.FinancialPeriod, FinancialPeriod>()).Returns(financialPeriodEntityAdapter);

            var ipcConfigurationAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ImmediatePaymentControl, ImmediatePaymentControl>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ImmediatePaymentControl, ImmediatePaymentControl>()).Returns(ipcConfigurationAdapter);
        }

        private void SetupData()
        {
            financeConfigurationEntity = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
            {
                ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByPeriod,
                ECommercePaymentsAllowed = true,
                IncludeDetail = true,
                IncludeHistory = true,
                IncludeSchedule = true,
                InstitutionName = "Institution Name",
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
                UseGuaranteedChecks = true
            };

            ipcConfigurationEntity = new Domain.Finance.Entities.ImmediatePaymentControl(true)
            {
                DeferralAcknowledgementDocumentId = "IPCDEFER",
                RegistrationAcknowledgementDocumentId = "IPCREGACK",
                TermsAndConditionsDocumentId = "IPCTC"
            };
        }

        private void SetupRepositories()
        {
            configRepoMock = new Mock<IFinanceConfigurationRepository>();
            configRepo = configRepoMock.Object;
            configRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfigurationEntity);
            configRepoMock.Setup(repo => repo.GetImmediatePaymentControl()).Returns(ipcConfigurationEntity);

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
        }
    }
}
