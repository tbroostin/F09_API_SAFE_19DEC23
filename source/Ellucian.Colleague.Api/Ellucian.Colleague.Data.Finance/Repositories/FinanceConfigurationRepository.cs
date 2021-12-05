// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class FinanceConfigurationRepository : BaseColleagueRepository, IFinanceConfigurationRepository
    {
        public FinanceConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get the master configuration for Student Finance.
        /// </summary>
        /// <returns>Financial configuration</returns>
        public FinanceConfiguration GetFinanceConfiguration()
        {
            return GetOrAddToCache<FinanceConfiguration>("FinanceConfiguration",
                () => { return BuildConfiguration(); });
        }

        /// <summary>
        /// Get the due date override specifications
        /// </summary>
        /// <returns>Due date overrides</returns>
        public DueDateOverrides GetDueDateOverrides()
        {
            return GetOrAddToCache<DueDateOverrides>("DueDateOverrides",
                () =>
                {
                    var sfDefaults = GetSfDefaults();
                    var dueDateOverrides = new DueDateOverrides()
                    {
                        NonTermOverride = sfDefaults.SfNonTermDueDateOvr,
                        PastPeriodOverride = sfDefaults.SfPastPeriodDueDateOvr,
                        CurrentPeriodOverride = sfDefaults.SfCurPeriodDueDateOvr,
                        FuturePeriodOverride = sfDefaults.SfFtrPeriodDueDateOvr
                    };
                    foreach (var termOverride in sfDefaults.SfTermDueDateOvrsEntityAssociation)
                    {
                        dueDateOverrides.TermOverrides.Add(termOverride.SfDueDateOvrTermsAssocMember, termOverride.SfDueDateOvrDatesAssocMember.Value);
                    }

                    return dueDateOverrides;
                });
        }

        /// <summary>
        /// Get the control information for Immediate Payment
        /// </summary>
        /// <returns>Immediate Payment configuration</returns>
        public Domain.Finance.Entities.ImmediatePaymentControl GetImmediatePaymentControl()
        {
            return GetOrAddToCache<Domain.Finance.Entities.ImmediatePaymentControl>("ImmediatePaymentControl",
                () =>
                {
                    var ipc = DataReader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL");
                    if (ipc == null)
                    {
                        // IPC is not set up - just return the enabled flag as false
                        return new Domain.Finance.Entities.ImmediatePaymentControl(false);
                    }

                    bool isEnabled = String.IsNullOrEmpty(ipc.IpcEnabled) ? false : ipc.IpcEnabled == "Y";
                    var immediatePaymentControl = new Domain.Finance.Entities.ImmediatePaymentControl(isEnabled)
                    {
                        RegistrationAcknowledgementDocumentId = ipc.IpcRegAcknowledgementDoc,
                        TermsAndConditionsDocumentId = ipc.IpcTermsAndConditionsDoc,
                        DeferralAcknowledgementDocumentId = ipc.IpcDeferralDoc
                    };

                    return immediatePaymentControl;
                });
        }

        /// <summary>
        /// Get the financial periods defined for the system
        /// </summary>
        /// <returns>List of financial periods</returns>
        public IEnumerable<FinancialPeriod> GetFinancialPeriods()
        {
            return GetOrAddToCache<List<FinancialPeriod>>("FinancialPeriods",
                () =>
                {
                    // Get the period info from Colleague
                    var response = transactionInvoker.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(new TxGetCurrentPeriodDatesRequest());

                    List<FinancialPeriod> periods = new List<FinancialPeriod>();
                    // If dates were returned, build the periods
                    if (response.OutCurrentPeriodStartDate != null && response.OutCurrentPeriodEndDate != null)
                    {
                        // Build Past period
                        periods.Add(new FinancialPeriod(PeriodType.Past, null, (response.OutCurrentPeriodStartDate.Value.AddDays(-1.0))));
                        // Build Current period
                        periods.Add(new FinancialPeriod(PeriodType.Current, response.OutCurrentPeriodStartDate, response.OutCurrentPeriodEndDate));
                        // Build Future period
                        periods.Add(new FinancialPeriod(PeriodType.Future, response.OutCurrentPeriodEndDate.Value.AddDays(1.0), null));
                    }
                    return periods;
                });
        }

        #region Private methods

        private SfDefaults GetSfDefaults()
        {
            SfDefaults sfDefaults = DataReader.ReadRecord<SfDefaults>("ST.PARMS", "SF.DEFAULTS");
            if (sfDefaults == null)
            {
                // SF.DEFAULTS must exist for Student Finance to function properly
                throw new ConfigurationException("Student Finance setup not complete.");
            }
            return sfDefaults;
        }

        private Ellucian.Colleague.Data.Finance.DataContracts.StwebDefaults GetStwebDefaults()
        {
            Ellucian.Colleague.Data.Finance.DataContracts.StwebDefaults stwebDefaults = DataReader.ReadRecord<Ellucian.Colleague.Data.Finance.DataContracts.StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS");
            if (stwebDefaults == null)
            {
                stwebDefaults = new DataContracts.StwebDefaults()
                {
                    StwebEpmtImplFlag = String.Empty,
                    StwebPartialPmtFlag = String.Empty
                };
            }
            return stwebDefaults;
        }

        private PpwebDefaults GetPpwebDefaults()
        {
            PpwebDefaults ppwebDefaults = DataReader.ReadRecord<PpwebDefaults>("ST.PARMS", "PPWEB.DEFAULTS");
            if (ppwebDefaults == null)
            {
                ppwebDefaults = new PpwebDefaults()
                {
                    PpwebPartialPmtInd = String.Empty
                };
            }
            return ppwebDefaults;
        }

        private CrDefaults GetCrDefaults()
        {
            CrDefaults crDefaults = DataReader.ReadRecord<CrDefaults>("ST.PARMS", "CR.DEFAULTS");
            if (crDefaults == null)
            {
                crDefaults = new CrDefaults()
                {
                    CrdGuaranteedChecks = String.Empty
                };
            }
            return crDefaults;
        }

        private SfPayPlanParameters GetSfPayPlanParameters()
        {
            return DataReader.ReadRecord<SfPayPlanParameters>("ST.PARMS", "SF.PAY.PLAN.PARAMETERS");
        }

        private IEnumerable<SfppRequirements> GetSfppRequirements()
        {
            return DataReader.BulkReadRecord<SfppRequirements>("SFPP.REQUIREMENTS", "");
        }

        private IEnumerable<SfssLinks> GetSfssLinks()
        {
            return DataReader.BulkReadRecord<Ellucian.Colleague.Data.Finance.DataContracts.SfssLinks>("SFSS.LINKS", "").OrderBy(x => x.SfssLinkDisplayOrder);
        }

        private FinanceConfiguration BuildConfiguration()
        {
            var configuration = new FinanceConfiguration();

            // Get parameters from SF.DEFAULTS, STWEB.DEFAULTS, PPWEB.DEFAULTS, and CR.DEFAULTS
            var sfDefaults = GetSfDefaults();
            var stwebDefaults = GetStwebDefaults();
            var ppwebDefaults = GetPpwebDefaults();
            var crDefaults = GetCrDefaults();

            configuration.SelfServicePaymentsAllowed = String.IsNullOrEmpty(sfDefaults.SfPmtsEnabled) ? false : sfDefaults.SfPmtsEnabled == "Y";
            configuration.ECommercePaymentsAllowed = String.IsNullOrEmpty(stwebDefaults.StwebEpmtImplFlag) ? false : stwebDefaults.StwebEpmtImplFlag == "Y";
            configuration.PartialAccountPaymentsAllowed = String.IsNullOrEmpty(stwebDefaults.StwebPartialPmtFlag) ? true : stwebDefaults.StwebPartialPmtFlag == "Y";
            if (String.IsNullOrEmpty(ppwebDefaults.PpwebPartialPmtInd))
            {
                configuration.PartialPlanPaymentsAllowed = PartialPlanPayments.Allowed;
            }
            else
            {
                switch (ppwebDefaults.PpwebPartialPmtInd)
                {
                    default:
                    case "Y":
                        configuration.PartialPlanPaymentsAllowed = PartialPlanPayments.Allowed;
                        break;
                    case "N":
                        configuration.PartialPlanPaymentsAllowed = PartialPlanPayments.Denied;
                        break;
                    case "P":
                        configuration.PartialPlanPaymentsAllowed = PartialPlanPayments.AllowedWhenNotOverdue;
                        break;
                }
            }
            configuration.PartialDepositPaymentsAllowed = String.IsNullOrEmpty(sfDefaults.SfAllowDepositPartialPmt) ? true : sfDefaults.SfAllowDepositPartialPmt == "Y";

            configuration.UseGuaranteedChecks = String.IsNullOrEmpty(crDefaults.CrdGuaranteedChecks) ? false : crDefaults.CrdGuaranteedChecks == "Y";
            configuration.ActivityDisplay = (sfDefaults.SfFinActIntvl.Equals("TERM")) ? ActivityDisplay.DisplayByTerm : ActivityDisplay.DisplayByPeriod;
            configuration.PaymentDisplay = (sfDefaults.SfPmtIntvl.Equals("TERM")) ? PaymentDisplay.DisplayByTerm : PaymentDisplay.DisplayByPeriod;
            configuration.SupportEmailAddress = sfDefaults.SfSupportEmail;
            configuration.ShowCreditAmounts = String.IsNullOrEmpty(sfDefaults.SfShowCreditAmts) ? false : sfDefaults.SfShowCreditAmts == "Y";

            // Get the text for the alert notification and payment review message
            configuration.NotificationText = GetMiscText(sfDefaults.SfNotificationText);
            configuration.PaymentReviewMessage = GetMiscText(sfDefaults.SfConfirmText);

            if (string.IsNullOrEmpty(sfDefaults.SfDisplayDueDates) || sfDefaults.SfDisplayDueDates == "Y")
            {
                configuration.DisplayDueDates = true;
            }
            else if (sfDefaults.SfDisplayDueDates == "N")
            {
                configuration.DisplayDueDates = false;
            }

            // Student Statement Parameters
            configuration.IncludeSchedule = String.IsNullOrEmpty(sfDefaults.SfStmtInclSchedule) ? true : sfDefaults.SfStmtInclSchedule == "Y";
            configuration.IncludeDetail = String.IsNullOrEmpty(sfDefaults.SfStmtInclDetail) ? true : sfDefaults.SfStmtInclDetail == "Y";
            //configuration.IncludeHistory = String.IsNullOrEmpty(sfDefaults.SfStmtInclHistory) ? true : sfDefaults.SfStmtInclHistory == "Y";
            configuration.IncludeHistory = false;
            configuration.InstitutionName = sfDefaults.SfStmtInstName;
            configuration.StatementTitle = sfDefaults.SfStmtTitle;
            if (sfDefaults.SfStmtInstAddrLines != null && sfDefaults.SfStmtInstAddrLines.Count > 0)
            {
                configuration.RemittanceAddress.AddRange(sfDefaults.SfStmtInstAddrLines);
            }
            if (sfDefaults.SfStmtMessage != null && sfDefaults.SfStmtMessage.Count != 0)
            {
                configuration.StatementMessage.AddRange(sfDefaults.SfStmtMessage);
            }

            // Get payment methods and financial periods
            configuration.PaymentMethods.AddRange(GetPaymentMethods(sfDefaults.SfOfficeCode));
            configuration.Periods.AddRange(GetFinancialPeriods());

            // Get student finance links
            var sfssLinks = GetSfssLinks();
            if (sfssLinks != null)
            {
                sfssLinks.ToList().ForEach(l => configuration.Links.Add(new StudentFinanceLink(l.SfssLinkTitle, l.SfssLinkUrl)));
            }

            if (sfDefaults.SfArTypesEntityAssociation != null && sfDefaults.SfArTypesEntityAssociation.Any())
            {
                foreach(var arType in sfDefaults.SfArTypesEntityAssociation)
                {
                    configuration.AddDisplayedReceivableType(new PayableReceivableType(arType.SfArTypesIdsAssocMember,
                        arType.SfArTypesPayableFlagsAssocMember.ToUpperInvariant() == "Y"));
                }
            }
            //Assign ecommerce provider link
            configuration.EcommerceProviderLink = sfDefaults.SfProviderLink;

            // Get student finance payment plan parameters
            var sfPayPlanParameters = GetSfPayPlanParameters();
            if (sfPayPlanParameters != null)
            {
                configuration.UserPaymentPlanCreationEnabled = !string.IsNullOrEmpty(sfPayPlanParameters.SfplnEnabled)
                    && sfPayPlanParameters.SfplnEnabled.ToUpperInvariant() == "Y";
                configuration.PaymentPlanEligibilityText = GetMiscText(sfPayPlanParameters.SfplnIneligibleText);
                if (sfPayPlanParameters.SfplnEligibilityRules != null)
                {
                    foreach (var rule in sfPayPlanParameters.SfplnEligibilityRules)
                    {
                        configuration.AddPaymentPlanEligibilityRuleId(rule);
                    }
                }
            }

            // Get student finance payment plan requirements
            var sfppRequirements = GetSfppRequirements();
            if (sfppRequirements != null)
            {
                foreach (var sfppr in sfppRequirements)
                {
                    configuration.AddTermPaymentPlanRequirement(BuildPaymentRequirement(sfppr));
                }
            }

            configuration.DisplayPotentialD7Amounts = String.IsNullOrEmpty(sfDefaults.SfEnableD7CalcFlag) ? false : sfDefaults.SfEnableD7CalcFlag == "Y";

            return configuration;
        }

        private IEnumerable<AvailablePaymentMethod> GetPaymentMethods(string officeCode)
        {
            // Limit payment methods to those that are web-enabled and for use with cash receipts
            string criteria = "WITH PMTH.WEB.PMT.FLAG EQ 'Y' WITH PMTH.CATEGORY.ACTION1 EQ 'CR'";
            // If there's a limiting office code, further limit the payment methods to those with
            // the code, or those with no office codes
            if (!String.IsNullOrEmpty(officeCode))
            {
                criteria += (" WITH PMTH.OFFICE.CODES EQ '' '" + officeCode + "'");
            }
            Collection<PaymentMethods> payMethods = DataReader.BulkReadRecord<PaymentMethods>("PAYMENT.METHOD", criteria);

            var paymentMethods = from p in payMethods
                                 orderby p.PmthDescription
                                 select new AvailablePaymentMethod()
                                 {
                                     InternalCode = p.Recordkey,
                                     Description = p.PmthDescription,
                                     Type = p.PmthCategory
                                 };

            return paymentMethods;
        }

        private string GetMiscText(string miscTextId)
        {
            string text = String.Empty;
            if (!string.IsNullOrEmpty(miscTextId))
            {
                MiscText miscText = DataReader.ReadRecord<MiscText>("MISC.TEXT", miscTextId);
                if (miscText != null)
                {
                    text = miscText.MtxtText.Replace(DmiString._VM, ' ');
                }
            }
            return text;
        }

        private PaymentRequirement BuildPaymentRequirement(SfppRequirements sfppRequirement)
        {
            if (sfppRequirement == null)
            {
                throw new ArgumentNullException("sfppRequirement", "SFPP.REQUIREMENTS record cannot be null.");
            }

            PaymentRequirement pr = null;
            try
            {
                List<PaymentPlanOption> paymentPlanOptions = new List<PaymentPlanOption>();

                if (sfppRequirement.SfpprPlanRequirementsEntityAssociation != null)
                {
                    foreach (var planRequirement in sfppRequirement.SfpprPlanRequirementsEntityAssociation)
                    {
                        paymentPlanOptions.Add(new PaymentPlanOption(planRequirement.SfpprPlanEffectiveStartAssocMember.Value,
                            planRequirement.SfpprPlanEffectiveEndAssocMember.Value,
                            planRequirement.SfpprPlanTemplateAssocMember,
                            planRequirement.SfpprPlanStartDateAssocMember.Value));
                    }
                }

                pr = new PaymentRequirement(sfppRequirement.Recordkey, sfppRequirement.SfpprTermId, sfppRequirement.SfpprEligibleRule,
                    (int)sfppRequirement.SfpprRuleEvalOrder.Value, null, paymentPlanOptions);
            }
            catch
            {
                LogDataError("SFPP.REQUIREMENTS", sfppRequirement.Recordkey, sfppRequirement);
            }
            return pr;
        }

        #endregion
    }
}
