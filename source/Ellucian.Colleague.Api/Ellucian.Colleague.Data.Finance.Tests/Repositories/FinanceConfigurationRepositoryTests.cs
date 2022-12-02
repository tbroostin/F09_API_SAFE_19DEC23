// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class FinanceConfigurationRepositoryTests : BaseRepositorySetup
    {
        FinanceConfigurationRepository repository;
        SfDefaults sfDefaults = null;
        StwebDefaults stwebDefaults = null;
        PpwebDefaults ppwebDefaults = null;
        CrDefaults crDefaults = null;
        SfPayPlanParameters sfPayPlanParameters = null;
        Collection<SfppRequirements> sfppRequirements = null;
        Collection<PaymentMethods> paymentMethods = new Collection<PaymentMethods>();
        PaymentMethods payMethodCc = new PaymentMethods() { Recordkey = "CC", PmthCategory = "CC", PmthDescription = "Credit Card" };
        PaymentMethods payMethodEc = new PaymentMethods() { Recordkey = "EC", PmthCategory = "CK", PmthDescription = "E-Check" };
        Collection<SfssLinks> sfssLinks = new Collection<SfssLinks>() { new SfssLinks() { Recordkey = "1", SfssLinkTitle = "Ellucian University", SfssLinkUrl = "http://www.ellucian.edu" } };
        DateTime startDate = DateTime.Today.AddDays(-10);
        DateTime endDate = DateTime.Today.AddDays(10);
        ApiSettings settings = new ApiSettings
        {
            ColleagueTimeZone = TimeZoneInfo.Local.Id
        };

        [TestInitialize()]
        public void Initialize() 
        {
            MockInitialize();

            sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfArTypesEntityAssociation = new List<SfDefaultsSfArTypes>()
                };
            dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
            stwebDefaults = new StwebDefaults();
            dataReaderMock.Setup<StwebDefaults>(reader => reader.ReadRecord<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).Returns(stwebDefaults);
            ppwebDefaults = new PpwebDefaults();
            dataReaderMock.Setup<PpwebDefaults>(reader => reader.ReadRecord<PpwebDefaults>("ST.PARMS", "PPWEB.DEFAULTS", true)).Returns(ppwebDefaults);
            crDefaults = new CrDefaults();
            dataReaderMock.Setup<CrDefaults>(reader => reader.ReadRecord<CrDefaults>("ST.PARMS", "CR.DEFAULTS", true)).Returns(crDefaults);
            dataReaderMock.Setup(r => r.BulkReadRecord<SfssLinks>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfssLinks);
            paymentMethods.Add(payMethodCc);
            paymentMethods.Add(payMethodEc);
            dataReaderMock.Setup<Collection<PaymentMethods>>(reader => reader.BulkReadRecord<PaymentMethods>("PAYMENT.METHOD", It.IsAny<string>(), true)).Returns(paymentMethods);
            var txResponse = new TxGetCurrentPeriodDatesResponse() { OutCurrentPeriodStartDate = startDate, OutCurrentPeriodEndDate = endDate };
            transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(trans =>
                trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>())).Returns(txResponse);
                            TxGetCurrentPeriodDatesResponse response = new TxGetCurrentPeriodDatesResponse() { OutCurrentPeriodStartDate = startDate, OutCurrentPeriodEndDate = endDate };
                transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(trans =>
                    trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>())).Returns(response);

                sfPayPlanParameters = new SfPayPlanParameters() { SfplnEnabled = "Y", SfplnEligibilityRules = new List<string>() { "RULE1", "RULE2" }, SfplnIneligibleText = "PLANELIG" };
            sfppRequirements = new Collection<SfppRequirements>()
            {
                new SfppRequirements()
                {
                    Recordkey = "1",
                    SfpprEligibleRule = string.Empty,
                    SfpprTermId = "TERM1",
                    SfpprRuleEvalOrder = 1,
                    SfpprPlanRequirementsEntityAssociation = new List<SfppRequirementsSfpprPlanRequirements>()
                    {
                        new SfppRequirementsSfpprPlanRequirements()
                        {
                            SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(-7),
                            SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(7),
                            SfpprPlanTemplateAssocMember = "TEMPLATE",
                            SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(14)
                        },
                        new SfppRequirementsSfpprPlanRequirements()
                        {
                            SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(8),
                            SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(22),
                            SfpprPlanTemplateAssocMember = "TEMPLATE2",
                            SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(29)
                        },                    
                    }
                }
            };
            dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
            dataReaderMock.Setup(r => r.BulkReadRecord<SfppRequirements>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfppRequirements);

            this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
        }

        #region GetFinanceConfiguration tests

        [TestClass]
        public class GetFinanceConfiguration : FinanceConfigurationRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NoSfDefaults()
            {
                SfDefaults defaults = null;
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(defaults);
                var result = this.repository.GetFinanceConfiguration();
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithEmptyStmtInstAddrLines()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>()
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfDefaults.SfStmtInstAddrLines.Count, result.RemittanceAddress.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithOfficeCode()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfOfficeCode = "FA",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>()
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                dataReaderMock.Setup<Collection<PaymentMethods>>(reader => reader.BulkReadRecord<PaymentMethods>("PAYMENT.METHOD", It.IsAny<string>(), true)).Returns(paymentMethods);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(paymentMethods.Count, result.PaymentMethods.Count);
                for (int i = 0; i < paymentMethods.Count; i++)
                {
                    Assert.AreEqual(paymentMethods[i].Recordkey, result.PaymentMethods[i].InternalCode);
                    Assert.AreEqual(paymentMethods[i].PmthDescription, result.PaymentMethods[i].Description);
                    Assert.AreEqual(paymentMethods[i].PmthCategory, result.PaymentMethods[i].Type);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithNullStmtInstAddrLines()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = null
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(0, result.RemittanceAddress.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithStmtInstAddrLines()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>() { "123 Main Street", "Fairfax, Virginia 22033" }
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfDefaults.SfStmtInstAddrLines.Count, result.RemittanceAddress.Count);
                for (int i = 0; i < sfDefaults.SfStmtInstAddrLines.Count; i++)
                {
                    Assert.AreEqual(sfDefaults.SfStmtInstAddrLines[i], result.RemittanceAddress[i]);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithNullStatementMessage()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtMessage = null,
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsNotNull(result.StatementMessage);
                Assert.AreEqual(0, result.StatementMessage.Count);
            }
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithEmptyStatementMessage()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtMessage = new List<string>(),
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfDefaults.SfStmtMessage.Count, result.StatementMessage.Count);
            }
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithStatementMessage()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtMessage = new List<string>() { "this is Line 1", "This is Line 2" },
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfDefaults.SfStmtMessage.Count, result.StatementMessage.Count);
                for (int i = 0; i < sfDefaults.SfStmtMessage.Count; i++)
                {
                    Assert.AreEqual(sfDefaults.SfStmtMessage[i], result.StatementMessage[i]);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SelfServicePaymentsAllowedNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.SelfServicePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SelfServicePaymentsAllowedEmpty()
            {
                sfDefaults.SfPmtsEnabled = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.SelfServicePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SelfServicePaymentsAllowedTrue()
            {
                sfDefaults.SfPmtsEnabled = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.SelfServicePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SelfServicePaymentsAllowedFalse()
            {
                sfDefaults.SfPmtsEnabled = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.SelfServicePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ECommercePaymentsAllowedNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ECommercePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ECommercePaymentsAllowedEmpty()
            {
                stwebDefaults.StwebEpmtImplFlag = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ECommercePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ECommercePaymentsAllowedTrue()
            {
                stwebDefaults.StwebEpmtImplFlag = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.ECommercePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ECommercePaymentsAllowedFalse()
            {
                stwebDefaults.StwebEpmtImplFlag = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ECommercePaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialAccountPaymentsAllowedNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialAccountPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialAccountPaymentsAllowedEmpty()
            {
                stwebDefaults.StwebPartialPmtFlag = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialAccountPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialAccountPaymentsAllowedTrue()
            {
                stwebDefaults.StwebPartialPmtFlag = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialAccountPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialAccountPaymentsAllowedFalse()
            {
                stwebDefaults.StwebPartialPmtFlag = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.PartialAccountPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialPlanPaymentsNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PartialPlanPayments.Allowed, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialPlanPaymentsEmpty()
            {
                ppwebDefaults.PpwebPartialPmtInd = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PartialPlanPayments.Allowed, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialPlanPaymentsAllowed()
            {
                ppwebDefaults.PpwebPartialPmtInd = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PartialPlanPayments.Allowed, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialPlanPaymentsDenied()
            {
                ppwebDefaults.PpwebPartialPmtInd = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PartialPlanPayments.Denied, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialPlanPaymentsAllowedWhenNotOverdue()
            {
                ppwebDefaults.PpwebPartialPmtInd = "P";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PartialPlanPayments.AllowedWhenNotOverdue, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialDepositPaymentsAllowedNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialDepositPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialDepositPaymentsAllowedEmpty()
            {
                sfDefaults.SfAllowDepositPartialPmt = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialDepositPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialDepositPaymentsAllowedTrue()
            {
                sfDefaults.SfAllowDepositPartialPmt = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.PartialDepositPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PartialDepositPaymentsAllowedFalse()
            {
                sfDefaults.SfAllowDepositPartialPmt = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.PartialDepositPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UseGuaranteedChecksNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.UseGuaranteedChecks);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UseGuaranteedChecksEmpty()
            {
                crDefaults.CrdGuaranteedChecks = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.UseGuaranteedChecks);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UseGuaranteedChecksTrue()
            {
                crDefaults.CrdGuaranteedChecks = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.UseGuaranteedChecks);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UseGuaranteedChecksFalse()
            {
                crDefaults.CrdGuaranteedChecks = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.UseGuaranteedChecks);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ActivityDisplayTerm()
            {
                sfDefaults.SfFinActIntvl = "TERM";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(ActivityDisplay.DisplayByTerm, result.ActivityDisplay);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ActivityDisplayPcf()
            {
                sfDefaults.SfFinActIntvl = "PCF";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(ActivityDisplay.DisplayByPeriod, result.ActivityDisplay);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentDisplayTerm()
            {
                sfDefaults.SfPmtIntvl = "TERM";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PaymentDisplay.DisplayByTerm, result.PaymentDisplay);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentDisplayPcf()
            {
                sfDefaults.SfPmtIntvl = "PCF";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(PaymentDisplay.DisplayByPeriod, result.PaymentDisplay);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ShowCreditAmountsNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ShowCreditAmounts);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ShowCreditAmountsEmpty()
            {
                sfDefaults.SfShowCreditAmts = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ShowCreditAmounts);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ShowCreditAmountsTrue()
            {
                sfDefaults.SfShowCreditAmts = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.ShowCreditAmounts);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_ShowCreditAmountsFalse()
            {
                sfDefaults.SfShowCreditAmts = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.ShowCreditAmounts);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NotificationTextNull()
            {
                sfDefaults.SfNotificationText = null;
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(String.Empty, result.NotificationText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NotificationTextEmpty()
            {
                sfDefaults.SfNotificationText = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(String.Empty, result.NotificationText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NotificationTextInvalid()
            {
                MiscText miscText = null;
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>(It.IsAny<string>(), true)).Returns(miscText);
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>("MISC.TEXT", It.IsAny<string>(), true)).Returns(miscText);
                sfDefaults.SfNotificationText = "INVALID";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(String.Empty, result.NotificationText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NotificationTextValid()
            {
                string text = "Some text";
                MiscText miscText = new MiscText() { Recordkey = "VALID", MtxtText = text };
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>("MISC.TEXT", It.IsAny<string>(), true)).Returns(miscText);
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>(It.IsAny<string>(), true)).Returns(miscText);
                sfDefaults.SfNotificationText = "VALID";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(text, result.NotificationText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityTextInvalid()
            {
                MiscText miscText = null;
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>(It.IsAny<string>(), true)).Returns(miscText);
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>("MISC.TEXT", It.IsAny<string>(), true)).Returns(miscText);
                sfPayPlanParameters.SfplnIneligibleText = "INVALID";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(String.Empty, result.PaymentPlanEligibilityText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityTextValid()
            {
                string text = "Some text";
                MiscText miscText = new MiscText() { Recordkey = "VALID", MtxtText = text };
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>("MISC.TEXT", It.IsAny<string>(), true)).Returns(miscText);
                dataReaderMock.Setup<MiscText>(reader => reader.ReadRecord<MiscText>(It.IsAny<string>(), true)).Returns(miscText);
                sfPayPlanParameters.SfplnIneligibleText = "VALID";
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(text, result.PaymentPlanEligibilityText);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeScheduleNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeSchedule);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeScheduleEmpty()
            {
                sfDefaults.SfStmtInclSchedule = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeSchedule);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeScheduleTrue()
            {
                sfDefaults.SfStmtInclSchedule = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeSchedule);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeScheduleFalse()
            {
                sfDefaults.SfStmtInclSchedule = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeSchedule);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeDetailNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeDetail);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeDetailEmpty()
            {
                sfDefaults.SfStmtInclDetail = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeDetail);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeDetailTrue()
            {
                sfDefaults.SfStmtInclDetail = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsTrue(result.IncludeDetail);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeDetailFalse()
            {
                sfDefaults.SfStmtInclDetail = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeDetail);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeHistoryNull()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeHistory);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeHistoryEmpty()
            {
                sfDefaults.SfStmtInclHistory = String.Empty;
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeHistory);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeHistoryTrue()
            {
                sfDefaults.SfStmtInclHistory = "Y";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeHistory);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_IncludeHistoryFalse()
            {
                sfDefaults.SfStmtInclHistory = "N";
                var result = this.repository.GetFinanceConfiguration();

                Assert.IsFalse(result.IncludeHistory);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentMethodsNone()
            {
                var paymentMethods = new Collection<PaymentMethods>();
                dataReaderMock.Setup<Collection<PaymentMethods>>(reader => reader.BulkReadRecord<PaymentMethods>("PAYMENT.METHOD", It.IsAny<string>(), true)).Returns(paymentMethods);
                dataReaderMock.Setup<Collection<PaymentMethods>>(reader => reader.BulkReadRecord<PaymentMethods>(It.IsAny<string>(), true)).Returns(paymentMethods);
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(0, result.PaymentMethods.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentMethodsCount()
            {
                var result = this.repository.GetFinanceConfiguration();

                Assert.AreEqual(paymentMethods.Count, result.PaymentMethods.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentMethodsCodes()
            {
                var result = this.repository.GetFinanceConfiguration();
                for (int i = 0; i < paymentMethods.Count; i++)
                {
                    var sourcePayMethod = paymentMethods[i];
                    var resultPayMethod = result.PaymentMethods[i];
                    Assert.AreEqual(sourcePayMethod.Recordkey, resultPayMethod.InternalCode);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentMethodsDescriptions()
            {
                var result = this.repository.GetFinanceConfiguration();
                for (int i = 0; i < paymentMethods.Count; i++)
                {
                    var sourcePayMethod = paymentMethods[i];
                    var resultPayMethod = result.PaymentMethods[i];
                    Assert.AreEqual(sourcePayMethod.PmthDescription, resultPayMethod.Description);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentMethodsTypes()
            {
                var result = this.repository.GetFinanceConfiguration();
                for (int i = 0; i < paymentMethods.Count; i++)
                {
                    var sourcePayMethod = paymentMethods[i];
                    var resultPayMethod = result.PaymentMethods[i];
                    Assert.AreEqual(sourcePayMethod.PmthCategory, resultPayMethod.Type);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_VerifyCache()
            {
                // Verify that after we get the configuration, it's stored in the cache
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                string cacheKey = this.repository.BuildFullCacheKey("FinanceConfiguration");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<FinanceConfiguration>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the configuration
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(ActivityDisplay.DisplayByTerm, result.ActivityDisplay);

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<FinanceConfiguration>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_NullDefaults()
            {
                CrDefaults nullCrDefaults = null;
                dataReaderMock.Setup<CrDefaults>(reader => reader.ReadRecord<CrDefaults>("ST.PARMS", "CR.DEFAULTS", true)).Returns(nullCrDefaults);
                StwebDefaults nullStwebDefaults = null;
                dataReaderMock.Setup<StwebDefaults>(reader => reader.ReadRecord<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).Returns(nullStwebDefaults);
                PpwebDefaults nullPpwebDefaults = null;
                dataReaderMock.Setup<PpwebDefaults>(reader => reader.ReadRecord<PpwebDefaults>("ST.PARMS", "PPWEB.DEFAULTS", true)).Returns(nullPpwebDefaults);
             
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.UseGuaranteedChecks);
                Assert.IsFalse(result.ECommercePaymentsAllowed);
                Assert.AreEqual(PartialPlanPayments.Allowed, result.PartialPlanPaymentsAllowed);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_Links()
            {
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfssLinks.Count, result.Links.Count);
                for (int i = 0; i < result.Links.Count; i++)
                {
                    Assert.AreEqual(sfssLinks[i].SfssLinkTitle, result.Links[i].Title);
                    Assert.AreEqual(sfssLinks[i].SfssLinkUrl, result.Links[i].Url);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_DisplayedReceivableTypes_Null_SfArTypesEntityAssociation()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfArTypesEntityAssociation = null
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.DisplayedReceivableTypes.Any());
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_DisplayedReceivableTypes_Empty_SfArTypesEntityAssociation()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfArTypesEntityAssociation = new List<SfDefaultsSfArTypes>()
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.DisplayedReceivableTypes.Any());
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_DisplayedReceivableTypes_SfArTypesEntityAssociation_HasValues()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfArTypesEntityAssociation = new List<SfDefaultsSfArTypes>()
                    {
                        new SfDefaultsSfArTypes() { SfArTypesIdsAssocMember = "01", SfArTypesPayableFlagsAssocMember = "Y" },
                        new SfDefaultsSfArTypes() { SfArTypesIdsAssocMember = "02", SfArTypesPayableFlagsAssocMember = "N" },
                    }
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfDefaults.SfArTypesEntityAssociation.Count, result.DisplayedReceivableTypes.Count);
                for(int i = 0; i < sfDefaults.SfArTypesEntityAssociation.Count; i++)
                {
                    Assert.AreEqual(sfDefaults.SfArTypesEntityAssociation[i].SfArTypesIdsAssocMember, result.DisplayedReceivableTypes[i].Code);
                    Assert.AreEqual(sfDefaults.SfArTypesEntityAssociation[i].SfArTypesPayableFlagsAssocMember.ToUpperInvariant() == "Y", result.DisplayedReceivableTypes[i].IsPayable);

                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UserPaymentPlanCreationEnabled_SfPayPlanParameters_Null()
            {
                sfPayPlanParameters = null;
                dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.UserPaymentPlanCreationEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityRuleIds_SfPayPlanParameters_Null()
            {
                sfPayPlanParameters = null;
                dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(0, result.PaymentPlanEligibilityRuleIds.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_UserPaymentPlanCreationEnabled_SfplnEnabled_Y()
            {
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsTrue(result.UserPaymentPlanCreationEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityRuleIds_SfplnEnabled_N()
            {
                sfPayPlanParameters.SfplnEnabled = "N";
                dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.UserPaymentPlanCreationEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityRuleIds_SfplnEnabled_Empty()
            {
                sfPayPlanParameters.SfplnEnabled = string.Empty;
                dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.UserPaymentPlanCreationEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityRuleIds_SfplnEligibilityRules_Null()
            {
                sfPayPlanParameters.SfplnEligibilityRules = null;
                dataReaderMock.Setup<SfPayPlanParameters>(reader => reader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS", true)).Returns(sfPayPlanParameters);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(0, result.PaymentPlanEligibilityRuleIds.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_PaymentPlanEligibilityRuleIds_SfplnEligibilityRules_NotNull()
            {
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfPayPlanParameters.SfplnEligibilityRules.Count, result.PaymentPlanEligibilityRuleIds.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_TermPaymentPlanRequirements_SfppRequirements_Null()
            {
                sfppRequirements = null;
                dataReaderMock.Setup(r => r.BulkReadRecord<SfppRequirements>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfppRequirements);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(0, result.TermPaymentPlanRequirements.Count);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_TermPaymentPlanRequirements_SfppRequirements_NotNull()
            {
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfppRequirements.Count, result.TermPaymentPlanRequirements.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_TermPaymentPlanRequirements_SfppRequirements_Contains_Null()
            {
                sfppRequirements = new Collection<SfppRequirements>()
                {
                    null,
                    new SfppRequirements()
                    {
                        Recordkey = "1",
                        SfpprEligibleRule = string.Empty,
                        SfpprTermId = "TERM1",
                        SfpprRuleEvalOrder = 1,
                        SfpprPlanRequirementsEntityAssociation = new List<SfppRequirementsSfpprPlanRequirements>()
                        {
                            new SfppRequirementsSfpprPlanRequirements()
                            {
                                SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(-7),
                                SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(7),
                                SfpprPlanTemplateAssocMember = "TEMPLATE",
                                SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(14)
                            },
                            new SfppRequirementsSfpprPlanRequirements()
                            {
                                SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(8),
                                SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(22),
                                SfpprPlanTemplateAssocMember = "TEMPLATE2",
                                SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(29)
                            },                    
                        }
                    }
                }; 
                dataReaderMock.Setup(r => r.BulkReadRecord<SfppRequirements>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfppRequirements);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_TermPaymentPlanRequirements_SfppRequirements_Association_Null()
            {
                sfppRequirements = new Collection<SfppRequirements>()
                {
                    new SfppRequirements()
                    {
                        Recordkey = "1",
                        SfpprEligibleRule = string.Empty,
                        SfpprTermId = "TERM1",
                        SfpprRuleEvalOrder = 1,
                        SfpprPlanRequirementsEntityAssociation = null
                    }
                };
                dataReaderMock.Setup(r => r.BulkReadRecord<SfppRequirements>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfppRequirements);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfppRequirements.Count, result.TermPaymentPlanRequirements.Count);
                Assert.AreEqual(0, result.TermPaymentPlanRequirements[0].PaymentPlanOptions.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_TermPaymentPlanRequirements_SfppRequirements_Association_Contains_Null()
            {
                sfppRequirements = new Collection<SfppRequirements>()
                {
                    new SfppRequirements()
                    {
                        Recordkey = "1",
                        SfpprEligibleRule = string.Empty,
                        SfpprTermId = "TERM1",
                        SfpprRuleEvalOrder = 1,
                        SfpprPlanRequirementsEntityAssociation = new List<SfppRequirementsSfpprPlanRequirements>()
                        {
                            null,
                            new SfppRequirementsSfpprPlanRequirements()
                            {
                                SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(-7),
                                SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(7),
                                SfpprPlanTemplateAssocMember = "TEMPLATE",
                                SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(14)
                            },
                            new SfppRequirementsSfpprPlanRequirements()
                            {
                                SfpprPlanEffectiveStartAssocMember = DateTime.Today.AddDays(8),
                                SfpprPlanEffectiveEndAssocMember = DateTime.Today.AddDays(22),
                                SfpprPlanTemplateAssocMember = "TEMPLATE2",
                                SfpprPlanStartDateAssocMember = DateTime.Today.AddDays(29)
                            },                    
                        }
                    }
                };
                dataReaderMock.Setup(r => r.BulkReadRecord<SfppRequirements>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(sfppRequirements);
                this.repository = new FinanceConfigurationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings);
                var result = this.repository.GetFinanceConfiguration();
                Assert.AreEqual(sfppRequirements.Count, result.TermPaymentPlanRequirements.Count);
                Assert.AreEqual(sfppRequirements[0].SfpprPlanRequirementsEntityAssociation.Count - 1, result.TermPaymentPlanRequirements[0].PaymentPlanOptions.Count);
            }
            /// <summary>
            /// Validate DisplayPotentialD7Amounts when parameter string is null
            /// </summary>
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithD7FlagNull()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>(),
                    SfEnableD7CalcFlag = null,
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.DisplayPotentialD7Amounts);
            }

            /// <summary>
            /// Validate DisplayPotentialD7Amounts when parameter string is empty
            /// </summary>
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithD7FlagEmpty()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>(),
                    SfEnableD7CalcFlag = string.Empty,
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.DisplayPotentialD7Amounts);
            }

            /// <summary>
            /// Validate DisplayPotentialD7Amounts when parameter string is not "Y"
            /// </summary>
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithD7FlagN()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>(),
                    SfEnableD7CalcFlag = "N",
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsFalse(result.DisplayPotentialD7Amounts);
            }

            /// <summary>
            /// Validate DisplayPotentialD7Amounts when parameter string is "Y"
            /// </summary>
            [TestMethod]
            public void FinanceConfigurationRepository_GetFinanceConfiguration_SfDefaultsWithD7FlagY()
            {
                sfDefaults = new SfDefaults()
                {
                    SfFinActIntvl = "TERM",
                    SfPmtIntvl = "TERM",
                    SfTermDueDateOvrsEntityAssociation = new List<SfDefaultsSfTermDueDateOvrs>(),
                    SfStmtInstAddrLines = new List<string>(),
                    SfEnableD7CalcFlag = "Y",
                };
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(sfDefaults);
                var result = this.repository.GetFinanceConfiguration();
                Assert.IsTrue(result.DisplayPotentialD7Amounts);
            }
        }

        #endregion

        #region GetDueDateOverrides tests

        [TestClass]
        public class GetDueDateOverrides : FinanceConfigurationRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public void FinanceConfigurationRepository_GetDueDateOverrides_NoSfDefaults()
            {
                SfDefaults defaults = null;
                dataReaderMock.Setup<SfDefaults>(reader => reader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS", true)).Returns(defaults);
                var result = this.repository.GetDueDateOverrides();
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_NonTermOverride()
            {
                DateTime overrideDate = DateTime.Today.AddDays(60);
                sfDefaults.SfNonTermDueDateOvr = overrideDate;
                var result = this.repository.GetDueDateOverrides();

                Assert.AreEqual(overrideDate.Date, result.NonTermOverride.GetValueOrDefault().Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_PastPeriodOverride()
            {
                DateTime overrideDate = DateTime.Today.AddDays(60);
                sfDefaults.SfPastPeriodDueDateOvr = overrideDate;
                var result = this.repository.GetDueDateOverrides();

                Assert.AreEqual(overrideDate.Date, result.PastPeriodOverride.GetValueOrDefault().Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_CurrentPeriodOverride()
            {
                DateTime overrideDate = DateTime.Today.AddDays(60);
                sfDefaults.SfCurPeriodDueDateOvr = overrideDate;
                var result = this.repository.GetDueDateOverrides();

                Assert.AreEqual(overrideDate.Date, result.CurrentPeriodOverride.GetValueOrDefault().Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_FuturePeriodOverride()
            {
                DateTime overrideDate = DateTime.Today.AddDays(60);
                sfDefaults.SfFtrPeriodDueDateOvr = overrideDate;
                var result = this.repository.GetDueDateOverrides();

                Assert.AreEqual(overrideDate.Date, result.FuturePeriodOverride.GetValueOrDefault().Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_TermOverrides()
            {
                var termOverrides = new List<SfDefaultsSfTermDueDateOvrs>();
                termOverrides.Add(new SfDefaultsSfTermDueDateOvrs() { SfDueDateOvrTermsAssocMember = "2013/FA", SfDueDateOvrDatesAssocMember = new DateTime(2013, 11, 15) });
                termOverrides.Add(new SfDefaultsSfTermDueDateOvrs() { SfDueDateOvrTermsAssocMember = "2014/SP", SfDueDateOvrDatesAssocMember = new DateTime(2014, 3, 15) });
                termOverrides.Add(new SfDefaultsSfTermDueDateOvrs() { SfDueDateOvrTermsAssocMember = "2014/SU", SfDueDateOvrDatesAssocMember = new DateTime(2014, 7, 15) });
                termOverrides.Add(new SfDefaultsSfTermDueDateOvrs() { SfDueDateOvrTermsAssocMember = "2014/FA", SfDueDateOvrDatesAssocMember = new DateTime(2014, 11, 15) });
                sfDefaults.SfTermDueDateOvrsEntityAssociation = termOverrides;
                var results = this.repository.GetDueDateOverrides().TermOverrides;

                for (int i = 0; i < termOverrides.Count; i++)
                {
                    var source = termOverrides[i];
                    var result = results.ElementAt(i);

                    Assert.AreEqual(source.SfDueDateOvrTermsAssocMember, result.Key);
                    Assert.AreEqual(source.SfDueDateOvrDatesAssocMember.GetValueOrDefault().Date, result.Value.Date);
                }
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetDueDateOverrides_VerifyCache()
            {
                // Verify that after we get the configuration, it's stored in the cache
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                string cacheKey = this.repository.BuildFullCacheKey("DueDateOverrides");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<DueDateOverrides>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the configuration
                DateTime overrideDate = DateTime.Today.AddDays(60);
                sfDefaults.SfNonTermDueDateOvr = overrideDate;
                var result = this.repository.GetDueDateOverrides();
                Assert.AreEqual(overrideDate.Date, result.NonTermOverride.GetValueOrDefault().Date);

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<DueDateOverrides>(), It.IsAny<CacheItemPolicy>(), null));
            }
        }

        #endregion

        #region GetImmediatePaymentControl tests

        [TestClass]
        public class GetImmediatePaymentControl : FinanceConfigurationRepositoryTests
        {
            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_NoImmediatePaymentControl()
            {
                DataContracts.ImmediatePaymentControl defaults = null;
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsFalse(result.IsEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_IpcEnabledNull()
            {
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl();
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsFalse(result.IsEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_IpcEnabledEmpty()
            {
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = String.Empty };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsFalse(result.IsEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_IpcEnabledNo()
            {
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "N" };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsFalse(result.IsEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_IpcEnabledYes()
            {
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y" };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsTrue(result.IsEnabled);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_RegistrationAcknowledgementDocument()
            {
                string docId = "REGACK";
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y", IpcRegAcknowledgementDoc = docId };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.AreEqual(docId, result.RegistrationAcknowledgementDocumentId);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_TermsAndConditionsDocument()
            {
                string docId = "TANDC";
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y", IpcTermsAndConditionsDoc = docId };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.AreEqual(docId, result.TermsAndConditionsDocumentId);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_DeferralAcknowledgementDocument()
            {
                string docId = "TANDC";
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y", IpcDeferralDoc = docId };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.AreEqual(docId, result.DeferralAcknowledgementDocumentId);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetImmediatePaymentControl_VerifyCache()
            {
                // Verify that after we get the configuration, it's stored in the cache
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                string cacheKey = this.repository.BuildFullCacheKey("ImmediatePaymentControl");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<Domain.Finance.Entities.ImmediatePaymentControl>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the configuration
                DataContracts.ImmediatePaymentControl defaults = new DataContracts.ImmediatePaymentControl() { IpcEnabled = "Y" };
                dataReaderMock.Setup<DataContracts.ImmediatePaymentControl>(reader =>
                    reader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL", true)).Returns(defaults);
                var result = this.repository.GetImmediatePaymentControl();

                Assert.IsTrue(result.IsEnabled);

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<Domain.Finance.Entities.ImmediatePaymentControl>(), It.IsAny<CacheItemPolicy>(), null));
            }
        }

        #endregion

        #region GetFinancialPeriods tests

        [TestClass]
        public class GetFinancialPeriods : FinanceConfigurationRepositoryTests
        {
            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_NullDates()
            {
                TxGetCurrentPeriodDatesResponse response = new TxGetCurrentPeriodDatesResponse() { OutCurrentPeriodStartDate = null, OutCurrentPeriodEndDate = null };
                transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(trans =>
                    trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>())).Returns(response);

                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_NullStartDate()
            {
                TxGetCurrentPeriodDatesResponse response = new TxGetCurrentPeriodDatesResponse() { OutCurrentPeriodStartDate = null, OutCurrentPeriodEndDate = endDate };
                transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(trans =>
                    trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>())).Returns(response);

                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_NullEndDate()
            {
                TxGetCurrentPeriodDatesResponse response = new TxGetCurrentPeriodDatesResponse() { OutCurrentPeriodStartDate = startDate, OutCurrentPeriodEndDate = null };
                transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(trans =>
                    trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>())).Returns(response);

                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_PastEndDate()
            {
                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(startDate.AddDays(-1).Date, result.Where(x => x.Type == Domain.Base.Entities.PeriodType.Past).FirstOrDefault().End.Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_CurrentStartDate()
            {
                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(startDate.Date, result.Where(x => x.Type == Domain.Base.Entities.PeriodType.Current).FirstOrDefault().Start.Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_CurrentEndDate()
            {
                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(endDate.Date, result.Where(x => x.Type == Domain.Base.Entities.PeriodType.Current).FirstOrDefault().End.Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepositoryTests_GetFinancialPeriods_FutureStartDate()
            {
                var result = this.repository.GetFinancialPeriods();

                Assert.AreEqual(endDate.AddDays(1).Date, result.Where(x => x.Type == Domain.Base.Entities.PeriodType.Future).FirstOrDefault().Start.Date);
            }

            [TestMethod]
            public void FinanceConfigurationRepository_GetFinancialPeriods_VerifyCache()
            {
                // Verify that after we get the configuration, it's stored in the cache
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                string cacheKey = this.repository.BuildFullCacheKey("FinancialPeriods");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<IEnumerable<FinancialPeriod>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the configuration
                var result = this.repository.GetFinancialPeriods();
                Assert.AreEqual(startDate.Date, result.Where(x => x.Type == Domain.Base.Entities.PeriodType.Current).FirstOrDefault().Start.Date);

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<IEnumerable<FinancialPeriod>>(), It.IsAny<CacheItemPolicy>(), null));
            }
        }

        #endregion

    }
}
