// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class FinanceConfigurationEntityAdapterTests
    {
        FinanceConfiguration financeConfigurationDto;
        Ellucian.Colleague.Domain.Finance.Entities.Configuration.FinanceConfiguration financeConfigurationEntity;
        FinanceConfigurationEntityAdapter financeConfigurationEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            financeConfigurationEntityAdapter = new FinanceConfigurationEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var partialPlanPaymentsEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.PartialPlanPayments, PartialPlanPayments>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PartialPlanPayments, PartialPlanPayments>()).Returns(partialPlanPaymentsEntityAdapter);

            var activityDisplayEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.ActivityDisplay, ActivityDisplay>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, ActivityDisplay>()).Returns(activityDisplayEntityAdapter);

            var paymentDisplayEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.PaymentDisplay, PaymentDisplay>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PaymentDisplay, PaymentDisplay>()).Returns(paymentDisplayEntityAdapter);

            var availablePaymentMethodEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Configuration.AvailablePaymentMethod, AvailablePaymentMethod>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.AvailablePaymentMethod, AvailablePaymentMethod>()).Returns(availablePaymentMethodEntityAdapter);

            var financialPeriodEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.FinancialPeriod, FinancialPeriod>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.FinancialPeriod, FinancialPeriod>()).Returns(financialPeriodEntityAdapter);

            var studentFinanceLinkEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.StudentFinanceLink, StudentFinanceLink>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentFinanceLink, StudentFinanceLink>()).Returns(studentFinanceLinkEntityAdapter);

            var payableReceivableTypeEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PayableReceivableType, PayableReceivableType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PayableReceivableType, PayableReceivableType>()).Returns(payableReceivableTypeEntityAdapter);

            var paymentRequirementEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentRequirement, PaymentRequirement>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentRequirement, PaymentRequirement>()).Returns(paymentRequirementEntityAdapter);

            var paymentPlanOptionEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PaymentPlanOption, PaymentPlanOption>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanOption, PaymentPlanOption>()).Returns(paymentPlanOptionEntityAdapter);


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
                StatementMessage = new List<string>() { "Statement message line 1", "Statement message line 2"},
                UserPaymentPlanCreationEnabled = true,
                DisplayPotentialD7Amounts = true,
            };
            financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("01", true));
            financeConfigurationEntity.AddDisplayedReceivableType(new Domain.Finance.Entities.PayableReceivableType("03", false));
            financeConfigurationEntity.AddPaymentPlanEligibilityRuleId("ALLREG");
            financeConfigurationEntity.AddPaymentPlanEligibilityRuleId("STUDENTS");
            financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("1", "TERM1", "RULE1", 1, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), "TEMPLATE", DateTime.Today.AddDays(14))
                }));
            financeConfigurationEntity.AddTermPaymentPlanRequirement(new Domain.Finance.Entities.PaymentRequirement("2", "TERM2", "", 0, null, new List<Domain.Finance.Entities.PaymentPlanOption>()
                {
                    new Domain.Finance.Entities.PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-7), "TEMPLATE2", DateTime.Today)
                }));

            financeConfigurationDto = financeConfigurationEntityAdapter.MapToType(financeConfigurationEntity);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_ActivityDisplay()
        {
            Assert.AreEqual(ActivityDisplay.DisplayByPeriod, financeConfigurationDto.ActivityDisplay);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_ECommercePaymentsAllowed()
        {
            Assert.AreEqual(financeConfigurationEntity.ECommercePaymentsAllowed, financeConfigurationDto.ECommercePaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_IncludeDetail()
        {
            Assert.AreEqual(financeConfigurationEntity.IncludeDetail, financeConfigurationDto.IncludeDetail);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_IncludeHistory()
        {
            Assert.AreEqual(financeConfigurationEntity.IncludeHistory, financeConfigurationDto.IncludeHistory);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_IncludeSchedule()
        {
            Assert.AreEqual(financeConfigurationEntity.IncludeSchedule, financeConfigurationDto.IncludeSchedule);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_InstitutionName()
        {
            Assert.AreEqual(financeConfigurationEntity.InstitutionName, financeConfigurationDto.InstitutionName);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_Links()
        {
            Assert.AreEqual(financeConfigurationEntity.Links.Count, financeConfigurationDto.Links.Count);
            for(int i = 0; i < financeConfigurationDto.Links.Count; i++)
            {
                Assert.AreEqual(financeConfigurationEntity.Links[0].Title, financeConfigurationDto.Links[i].Title);
                Assert.AreEqual(financeConfigurationEntity.Links[0].Url, financeConfigurationDto.Links[i].Url);
            }
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_DisplayedReceivableTypes()
        {
            var dtoDisplayedReceivableTypes = financeConfigurationDto.DisplayedReceivableTypes.ToList();
            Assert.AreEqual(financeConfigurationEntity.DisplayedReceivableTypes.Count, dtoDisplayedReceivableTypes.Count);
            for (int i = 0; i < dtoDisplayedReceivableTypes.Count; i++)
            {
                Assert.AreEqual(financeConfigurationEntity.DisplayedReceivableTypes[i].Code, dtoDisplayedReceivableTypes[i].Code);
                Assert.AreEqual(financeConfigurationEntity.DisplayedReceivableTypes[i].IsPayable, dtoDisplayedReceivableTypes[i].IsPayable);
            }
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_NotificationText()
        {
            Assert.AreEqual(financeConfigurationEntity.NotificationText, financeConfigurationDto.NotificationText);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PartialAccountPaymentsAllowed()
        {
            Assert.AreEqual(financeConfigurationEntity.PartialAccountPaymentsAllowed, financeConfigurationDto.PartialAccountPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PartialDepositPaymentsAllowed()
        {
            Assert.AreEqual(financeConfigurationEntity.PartialDepositPaymentsAllowed, financeConfigurationDto.PartialDepositPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PartialPlanPaymentsAllowed()
        {
            Assert.AreEqual(PartialPlanPayments.Allowed, financeConfigurationDto.PartialPlanPaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PaymentDisplay()
        {
            Assert.AreEqual(PaymentDisplay.DisplayByPeriod, financeConfigurationDto.PaymentDisplay);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PaymentMethods()
        {
            Assert.AreEqual(financeConfigurationEntity.PaymentMethods.Count, financeConfigurationDto.PaymentMethods.Count);
            for (int i = 0; i < financeConfigurationDto.PaymentMethods.Count; i++)
            {
                Assert.AreEqual(financeConfigurationEntity.PaymentMethods[i].Description, financeConfigurationDto.PaymentMethods[i].Description);
                Assert.AreEqual(financeConfigurationEntity.PaymentMethods[i].InternalCode, financeConfigurationDto.PaymentMethods[i].InternalCode);
                Assert.AreEqual(financeConfigurationEntity.PaymentMethods[i].Type, financeConfigurationDto.PaymentMethods[i].Type);            
            }
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PaymentReviewMessage()
        {
            Assert.AreEqual(financeConfigurationEntity.PaymentReviewMessage, financeConfigurationDto.PaymentReviewMessage);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_Periods()
        {
            Assert.AreEqual(financeConfigurationEntity.Periods.Count, financeConfigurationDto.Periods.Count);
            for (int i = 0; i < financeConfigurationDto.Periods.Count; i++)
            {
                Assert.AreEqual(financeConfigurationEntity.Periods[i].End, financeConfigurationDto.Periods[i].End);
                Assert.AreEqual(financeConfigurationEntity.Periods[i].Start, financeConfigurationDto.Periods[i].Start);
                Dtos.Base.PeriodType expected;
                switch (financeConfigurationEntity.Periods[i].Type)
                {
                    case Domain.Base.Entities.PeriodType.Past:
                        expected = Dtos.Base.PeriodType.Past;
                        break;
                    case Domain.Base.Entities.PeriodType.Current:
                        expected = Dtos.Base.PeriodType.Current;
                        break;
                    default:
                        expected = Dtos.Base.PeriodType.Future;
                        break;
                }
                Assert.AreEqual(expected, financeConfigurationDto.Periods[i].Type);
            }
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_RemittanceAddress()
        {
            CollectionAssert.AreEqual(financeConfigurationEntity.RemittanceAddress, financeConfigurationDto.RemittanceAddress);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_StatementMessage()
        {
            CollectionAssert.AreEqual(financeConfigurationEntity.StatementMessage, financeConfigurationDto.StatementMessage);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_SelfServicePaymentsAllowed()
        {
            Assert.AreEqual(financeConfigurationEntity.SelfServicePaymentsAllowed, financeConfigurationDto.SelfServicePaymentsAllowed);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_ShowCreditAmounts()
        {
            Assert.AreEqual(financeConfigurationEntity.ShowCreditAmounts, financeConfigurationDto.ShowCreditAmounts);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_StatementTitle()
        {
            Assert.AreEqual(financeConfigurationEntity.StatementTitle, financeConfigurationDto.StatementTitle);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_SupportEmailAddress()
        {
            Assert.AreEqual(financeConfigurationEntity.SupportEmailAddress, financeConfigurationDto.SupportEmailAddress);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_UseGuaranteedChecks()
        {
            Assert.AreEqual(financeConfigurationEntity.UseGuaranteedChecks, financeConfigurationDto.UseGuaranteedChecks);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_UserPaymentPlanCreationEnabled()
        {
            Assert.AreEqual(financeConfigurationEntity.UserPaymentPlanCreationEnabled, financeConfigurationDto.UserPaymentPlanCreationEnabled);
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_PaymentPlanEligibilityRuleIds()
        {
            Assert.AreEqual(financeConfigurationEntity.PaymentPlanEligibilityRuleIds.Count, financeConfigurationDto.PaymentPlanEligibilityRuleIds.Count());
        }

        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_TermPaymentPlanRequirements()
        {
            Assert.AreEqual(financeConfigurationEntity.TermPaymentPlanRequirements.Count, financeConfigurationDto.TermPaymentPlanRequirements.Count());
        }

        /// <summary>
        /// Verify conversion of DisplayPotentialD7Amounts
        /// </summary>
        [TestMethod]
        public void FinanceConfigurationEntityAdapterTests_DisplayPotentialD7Amounts()
        {
            Assert.AreEqual(financeConfigurationEntity.DisplayPotentialD7Amounts, financeConfigurationDto.DisplayPotentialD7Amounts);
        }
    }
}