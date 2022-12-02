// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class PaymentPlanService : FinanceCoordinationService, IPaymentPlanService
    {
        private IPaymentPlanRepository _paymentPlanRepository;
        private IAccountsReceivableRepository _arRepository;
        private IPaymentRepository _payRepository;
        private IRegistrationBillingRepository _rbRepository;
        private IFinanceConfigurationRepository _financeConfigurationRepository;
        private IRuleRepository _ruleRepository;

        private const string _PaymentPlanDistribution = "PP";

        public PaymentPlanService(IAdapterRegistry adapterRegistry, IPaymentPlanRepository paymentPlanRepository, IAccountsReceivableRepository arRepository,
            IPaymentRepository payRepository, IRegistrationBillingRepository rbRepository, IFinanceConfigurationRepository finConfigRepo, IRuleRepository ruleRepo,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _paymentPlanRepository = paymentPlanRepository;
            _arRepository = arRepository;
            _payRepository = payRepository;
            _rbRepository = rbRepository;
            _financeConfigurationRepository = finConfigRepo;
            _ruleRepository = ruleRepo;
        }

        /// <summary>
        /// Gets all payment plan templates
        /// </summary>
        /// <returns>A list of PaymentPlanTemplate DTOs</returns>
        public IEnumerable<PaymentPlanTemplate> GetPaymentPlanTemplates()
        {
            var templateEntities = _paymentPlanRepository.PaymentPlanTemplates;
            var templateAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTemplate, PaymentPlanTemplate>();
            var templateDtos = new List<PaymentPlanTemplate>();
            foreach (var templateEntity in templateEntities)
            {
                templateDtos.Add(templateAdapter.MapToType(templateEntity));
            }

            return templateDtos;
        }

        /// <summary>
        /// Gets the specified payment plan template
        /// </summary>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate DTO</returns>
        public PaymentPlanTemplate GetPaymentPlanTemplate(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template Id must be specified.");
            }
            var templateEntity = _paymentPlanRepository.GetTemplate(templateId);

            var templateAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTemplate, PaymentPlanTemplate>();
            PaymentPlanTemplate templateDto = templateAdapter.MapToType(templateEntity);

            return templateDto;
        }

        /// <summary>
        /// Gets the specified payment plan
        /// </summary>
        /// <param name="planId">ID of the payment plan</param>
        /// <returns>A PaymentPlan DTO</returns>
        public PaymentPlan GetPaymentPlan(string planId)
        {
            if (string.IsNullOrEmpty(planId))
            {
                throw new ArgumentNullException("planId", "Plan ID must be specified.");
            }
            var paymentPlanEntity = _paymentPlanRepository.GetPaymentPlan(planId);
            CheckAccountPermission(paymentPlanEntity.PersonId);

            var paymentPlanAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan, PaymentPlan>();
            PaymentPlan paymentPlanDto = paymentPlanAdapter.MapToType(paymentPlanEntity);

            return paymentPlanDto;
        }

        /// <summary>
        /// Accept the terms and conditions associated with a payment plan
        /// </summary>
        /// <param name="acceptance">The information for the acceptance</param>
        /// <returns>The updated approval information</returns>
        public PaymentPlanApproval ApprovePaymentPlanTerms(PaymentPlanTermsAcceptance acceptance)
        {
            // Only student can do approvals
            CheckPaymentPermission(acceptance.StudentId, CurrentUser + " cannot make approvals for student " + acceptance.StudentId);

            // Add the user's login ID to the DTO so we can create the entity
            acceptance.ApprovalUserId = CurrentUser.UserId;

            try
            {
                // Convert the DTO to an entity, process the acceptance, convert the resulting entity
                // back to a DTO, and return it
                var acceptanceAdapter = _adapterRegistry.GetAdapter<PaymentPlanTermsAcceptance, Domain.Finance.Entities.PaymentPlanTermsAcceptance>();
                var acceptanceEntity = acceptanceAdapter.MapToType(acceptance);
                var approvalEntity = _paymentPlanRepository.ApprovePaymentPlanTerms(acceptanceEntity);
                var approvalAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.PaymentPlanApproval, PaymentPlanApproval>();
                var approvalDto = approvalAdapter.MapToType(approvalEntity);

                return approvalDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }

        }

        /// <summary>
        /// Get a payment plan approval
        /// </summary>
        /// <param name="approvalId">ID of plan approval</param>
        /// <returns>The recorded approval information</returns>
        public PaymentPlanApproval GetPaymentPlanApproval(string approvalId)
        {
            if (String.IsNullOrEmpty(approvalId))
            {
                throw new ArgumentNullException("approvalId", "Approval response ID is required.");
            }

            try
            {
                var approvalEntity = _paymentPlanRepository.GetPaymentPlanApproval(approvalId);


                // Verify user permissions to data
                CheckAccountPermission(approvalEntity.StudentId);

                var approvalAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.PaymentPlanApproval, PaymentPlanApproval>();
                var approvalDto = approvalAdapter.MapToType(approvalEntity);

                return approvalDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets the down payment information for a payment plan, pay method, amount, and (
        /// </summary>
        /// <param name="planId">Payment plan ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <param name="payControlId">Optional registration payment control ID</param>
        /// <returns>Payment to be made</returns>
        public Payment GetPlanPaymentSummary(string planId, string payMethod, decimal amount, string payControlId)
        {
            if (string.IsNullOrEmpty(planId))
            {
                throw new ArgumentNullException("planId", "A payment plan ID must be provided.");
            }
            if (string.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod", "A payment method must be provided.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number.", "amount");
            }

            // Get the payment plan using the passed-in ID
            var paymentPlan = _paymentPlanRepository.GetPaymentPlan(planId);

            // Check user permissions
            CheckPaymentPermission(paymentPlan.PersonId);

            // Get the needed data
            var distribution = _arRepository.GetDistribution(paymentPlan.PersonId, paymentPlan.ReceivableTypeCode, _PaymentPlanDistribution);
            var paymentConfirmation = _payRepository.GetConfirmation(distribution, payMethod, amount.ToString());
            var receivableTypes = _arRepository.ReceivableTypes;

            // Build the plan's payment item
            var paymentItem = new Domain.Finance.Entities.Payments.PaymentItem()
            {
                AccountType = paymentPlan.ReceivableTypeCode,
                Description = receivableTypes.Where(x => x.Code == paymentPlan.ReceivableTypeCode).FirstOrDefault().Description,
                Term = paymentPlan.TermId,
                PaymentAmount = amount,
                PaymentControlId = payControlId,
                PaymentPlanId = paymentPlan.Id,
                PaymentComplete = true
            };

            // Build the plan payment
            var paymentEntity = new Domain.Finance.Entities.Payments.Payment()
            {
                AmountToPay = paymentItem.PaymentAmount + paymentConfirmation.ConvenienceFeeAmount.GetValueOrDefault(),
                Distribution = distribution,
                PayMethod = payMethod,
                PersonId = paymentPlan.PersonId,
                ConvenienceFee = paymentConfirmation.ConvenienceFeeCode,
                ConvenienceFeeAmount = paymentConfirmation.ConvenienceFeeAmount.GetValueOrDefault(),
                ConvenienceFeeGeneralLedgerNumber = paymentConfirmation.ConvenienceFeeGeneralLedgerNumber,
                ProviderAccount = paymentConfirmation.ProviderAccount
            };
            paymentEntity.PaymentItems.Add(paymentItem);

            // Perform the entity-DTO conversion and return the result
            var paymentAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Payments.Payment, Payment>();
            var paymentDto = paymentAdapter.MapToType(paymentEntity);

            return paymentDto;
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="billingTerms">Collection of payment items</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        [Obsolete("Obsolete as of API version 1.16. Use GetBillingTermPaymentPlanInformation2Async.")]
        public async Task<PaymentPlanEligibility> GetBillingTermPaymentPlanInformationAsync(IEnumerable<BillingTermPaymentPlanInformation> billingTerms)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                throw new ArgumentNullException("billing terms list cannot be empty");
            }

            var terms = billingTerms;
            // Validate inbound billing term payment plan information
            ValidateBillingTermPaymentPlanInformation(terms);

            var personIds = terms.Select(bt => bt.PersonId).Distinct().ToList();
            personIds.ForEach(pid => CheckIfUserIsSelf(pid));

            // Initialize list of DTOs to be returned;
            Domain.Finance.Entities.PaymentPlanEligibility eligibilityEntity;
            Domain.Finance.Entities.PaymentPlanIneligibilityReason? reasonEntity = (Domain.Finance.Entities.PaymentPlanIneligibilityReason?)null;
            List<Domain.Finance.Entities.BillingTermPaymentPlanInformation> eligibleBillingTerms = new List<Domain.Finance.Entities.BillingTermPaymentPlanInformation>();

            // Retrieve finance configuration information
            FinanceConfiguration config = _financeConfigurationRepository.GetFinanceConfiguration();
            if (config != null)
            {
                // Determine if user payment plan creation is enabled
                if (config.UserPaymentPlanCreationEnabled)
                {
                    // Convert DTOs to domain entities
                    List<Ellucian.Colleague.Domain.Finance.Entities.BillingTermPaymentPlanInformation> billingTermEntities = new List<Domain.Finance.Entities.BillingTermPaymentPlanInformation>();
                    var dtoAdapter = new BillingTermPaymentPlanInformationDtoAdapter(_adapterRegistry, logger);
                    var entityAdapter = new BillingTermPaymentPlanInformationEntityAdapter(_adapterRegistry, logger);
                    foreach (var billingTerm in terms)
                    {
                        billingTermEntities.Add(dtoAdapter.MapToType(billingTerm));
                    }

                    // Eliminate any billing terms with receivable types that are not payable
                    billingTermEntities = ReceivableService.RemoveBillingTermPaymentPlanInformationWithNonPayableReceivableType(billingTermEntities, config).ToList();

                    // Retrieve account holder information 
                    if (billingTermEntities.Any())
                    {
                        var accountHolder = await _arRepository.GetAccountHolderAsync(billingTermEntities[0].PersonId);

                        // Evaluate account holder against any global payment plan eligibility rules
                        bool isPlanEligible = AccountHolderIsEligibleForPaymentPlanCreation(config, accountHolder);
                        if (isPlanEligible)
                        {
                            // For each billing term, determine the applicable payment plan requirement definition,
                            // either the default or a rule-specific requirement, and the applicable payment plan
                            // option within that requirement.
                            foreach (var billingTerm in billingTermEntities)
                            {
                                var paymentPlanOption = GetApplicablePaymentPlanOption(billingTerm.TermId, billingTerm.ReceivableTypeCode, config, accountHolder);
                                if (paymentPlanOption != null)
                                {
                                    // Get the associated payment plan template 
                                    Domain.Finance.Entities.PaymentPlanTemplate template;
                                    try
                                    {
                                        template = _paymentPlanRepository.GetTemplate(paymentPlanOption.TemplateId);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Info(ex, "Error retrieving template " + paymentPlanOption.TemplateId);
                                        template = null;
                                    }
                                    // Determine if the template is valid for user payment plan creation
                                    if (template != null)
                                    {
                                        List<string> messages;
                                        Domain.Finance.Entities.PaymentPlanIneligibilityReason? reason;
                                        if (template.IsValidForUserPaymentPlanCreation(billingTerm.ReceivableTypeCode, billingTerm.PaymentPlanAmount, out messages, out reason))
                                        {
                                            billingTerm.PaymentPlanTemplateId = template.Id;
                                            eligibleBillingTerms.Add(billingTerm);
                                        }
                                        else
                                        {
                                            if (messages != null) messages.ForEach(m => logger.Info(m));
                                            billingTerm.IneligibilityReason = reason;
                                        }
                                    }
                                    else
                                    {
                                        string errorMessage = "Payment plan template " + paymentPlanOption.TemplateId + " is null.";
                                        logger.Info(errorMessage);
                                        reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                                    }
                                }
                                else
                                {
                                    logger.Info("Payment plans cannot be created on " + DateTime.Today.ToShortDateString() + " for " + billingTerm.TermId + "; user cannot create a payment plan for " + billingTerm.TermId + " for receivable type " + billingTerm.ReceivableTypeCode);
                                    reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                                }
                            }
                            var ineligibleReasons = billingTermEntities.Select(bt => bt.IneligibilityReason).Where(bt => bt.HasValue).Distinct().ToList();
                            reasonEntity = ineligibleReasons.Any() ? 
                                (ineligibleReasons.Any(ir => ir.Value == Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible) ? Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible : Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration) :
                                reasonEntity;
                        }
                        else
                        {
                            logger.Info("Account holder " + accountHolder.Id + " did not pass all user eligibility rules.");
                            reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                        }
                    }
                    else
                    {
                        logger.Info("None of the receivable types for the user's payment item(s) is payable; user cannot create a payment plan at this time.");
                        reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible;
                    }
                }
                else
                {
                    logger.Info("User payment plan creation is disabled; user cannot create a payment plan for any term/receivable type.");
                    reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                }
                eligibilityEntity = new Domain.Finance.Entities.PaymentPlanEligibility(eligibleBillingTerms, reasonEntity);
                PaymentPlanEligibilityAdapter eligibilityDtoAdapter = new PaymentPlanEligibilityAdapter(_adapterRegistry, logger);
                return eligibilityDtoAdapter.MapToType(eligibilityEntity);
            }

            // Finance configuration data is null; log an error.
            else
            {
                string errorMessage = "Finance configuration data is null.";
                logger.Info(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        /// <summary>
        /// Gets valid billing term payment plan information from a proposed billing term payment plan information collection
        /// </summary>
        /// <param name="criteria">payment plan query criteria</param>
        /// <returns>Valid billing term payment plan information from a proposed billing term payment plan information collection</returns>
        public async Task<PaymentPlanEligibility> GetBillingTermPaymentPlanInformation2Async(PaymentPlanQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            if (criteria.BillingTerms == null || !criteria.BillingTerms.Any())
            {
                throw new ArgumentException("billing terms list cannot be empty");
            }

            try
            {
                var billingTerms = criteria.BillingTerms;
                // Validate inbound billing term payment plan information
                ValidateBillingTermPaymentPlanInformation(billingTerms);

                var personIds = billingTerms.Select(bt => bt.PersonId).Distinct().ToList();
                personIds.ForEach(pid => CheckIfUserIsSelf(pid));

                // Initialize list of DTOs to be returned;
                Domain.Finance.Entities.PaymentPlanEligibility eligibilityEntity;
                Domain.Finance.Entities.PaymentPlanIneligibilityReason? reasonEntity = (Domain.Finance.Entities.PaymentPlanIneligibilityReason?)null;
                List<Domain.Finance.Entities.BillingTermPaymentPlanInformation> eligibleBillingTerms = new List<Domain.Finance.Entities.BillingTermPaymentPlanInformation>();

                // Retrieve finance configuration information
                FinanceConfiguration config = _financeConfigurationRepository.GetFinanceConfiguration();
                if (config != null)
                {
                    // Determine if user payment plan creation is enabled
                    if (config.UserPaymentPlanCreationEnabled)
                    {
                        // Convert DTOs to domain entities
                        List<Ellucian.Colleague.Domain.Finance.Entities.BillingTermPaymentPlanInformation> billingTermEntities = new List<Domain.Finance.Entities.BillingTermPaymentPlanInformation>();
                        var dtoAdapter = new BillingTermPaymentPlanInformationDtoAdapter(_adapterRegistry, logger);
                        var entityAdapter = new BillingTermPaymentPlanInformationEntityAdapter(_adapterRegistry, logger);
                        foreach (var billingTerm in billingTerms)
                        {
                            billingTermEntities.Add(dtoAdapter.MapToType(billingTerm));
                        }

                        // Eliminate any billing terms with receivable types that are not payable
                        billingTermEntities = ReceivableService.RemoveBillingTermPaymentPlanInformationWithNonPayableReceivableType(billingTermEntities, config).ToList();

                        // Retrieve account holder information 
                        if (billingTermEntities.Any())
                        {
                            var accountHolder = await _arRepository.GetAccountHolderAsync(billingTermEntities[0].PersonId);

                            // Evaluate account holder against any global payment plan eligibility rules
                            bool isPlanEligible = AccountHolderIsEligibleForPaymentPlanCreation(config, accountHolder);
                            if (isPlanEligible)
                            {
                                // For each billing term, determine the applicable payment plan requirement definition,
                                // either the default or a rule-specific requirement, and the applicable payment plan
                                // option within that requirement.
                                foreach (var billingTerm in billingTermEntities)
                                {
                                    var paymentPlanOption = GetApplicablePaymentPlanOption(billingTerm.TermId, billingTerm.ReceivableTypeCode, config, accountHolder);
                                    if (paymentPlanOption != null)
                                    {
                                        // Get the associated payment plan template 
                                        Domain.Finance.Entities.PaymentPlanTemplate template;
                                        try
                                        {
                                            template = _paymentPlanRepository.GetTemplate(paymentPlanOption.TemplateId);
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Info(ex, "Error retrieving template " + paymentPlanOption.TemplateId);
                                            template = null;
                                        }
                                        // Determine if the template is valid for user payment plan creation
                                        if (template != null)
                                        {
                                            List<string> messages;
                                            Domain.Finance.Entities.PaymentPlanIneligibilityReason? reason;
                                            if (template.IsValidForUserPaymentPlanCreation(billingTerm.ReceivableTypeCode, billingTerm.PaymentPlanAmount, out messages, out reason))
                                            {
                                                billingTerm.PaymentPlanTemplateId = template.Id;
                                                eligibleBillingTerms.Add(billingTerm);
                                            }
                                            else
                                            {
                                                if (messages != null) messages.ForEach(m => logger.Info(m));
                                                billingTerm.IneligibilityReason = reason;
                                            }
                                        }
                                        else
                                        {
                                            string errorMessage = "Payment plan template " + paymentPlanOption.TemplateId + " is null.";
                                            logger.Info(errorMessage);
                                            reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                                        }
                                    }
                                    else
                                    {
                                        logger.Info("Payment plans cannot be created on " + DateTime.Today.ToShortDateString() + " for " + billingTerm.TermId + "; user cannot create a payment plan for " + billingTerm.TermId + " for receivable type " + billingTerm.ReceivableTypeCode);
                                        reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                                    }
                                }
                                var ineligibleReasons = billingTermEntities.Select(bt => bt.IneligibilityReason).Where(bt => bt.HasValue).Distinct().ToList();
                                reasonEntity = ineligibleReasons.Any() ?
                                    (ineligibleReasons.Any(ir => ir.Value == Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible) ? Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible : Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration) :
                                    reasonEntity;
                            }
                            else
                            {
                                logger.Info("Account holder " + accountHolder.Id + " did not pass all user eligibility rules.");
                                reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                            }
                        }
                        else
                        {
                            logger.Info("None of the receivable types for the user's payment item(s) is payable; user cannot create a payment plan at this time.");
                            reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.ChargesAreNotEligible;
                        }
                    }
                    else
                    {
                        logger.Info("User payment plan creation is disabled; user cannot create a payment plan for any term/receivable type.");
                        reasonEntity = Domain.Finance.Entities.PaymentPlanIneligibilityReason.PreventedBySystemConfiguration;
                    }
                    eligibilityEntity = new Domain.Finance.Entities.PaymentPlanEligibility(eligibleBillingTerms, reasonEntity);
                    PaymentPlanEligibilityAdapter eligibilityDtoAdapter = new PaymentPlanEligibilityAdapter(_adapterRegistry, logger);
                    return eligibilityDtoAdapter.MapToType(eligibilityEntity);
                }

                // Finance configuration data is null; log an error.
                else
                {
                    string errorMessage = "Finance configuration data is null.";
                    logger.Info(errorMessage);
                    throw new ApplicationException(errorMessage);
                }
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a proposed payment plan for a given person for a given term and receivable type with total charges
        /// no greater than the stated amount
        /// </summary>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>
        /// <returns>Proposed payment plan</returns>
        public async Task<PaymentPlan> GetProposedPaymentPlanAsync(string personId, string termId,
            string receivableTypeCode, decimal planAmount)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person ID must be supplied.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "A term ID must be supplied.");
            }
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "A receivable type code must be supplied.");
            }
            if (planAmount <= 0)
            {
                throw new ArgumentException("planAmount", "A positive plan amount must be supplied.");
            }

            CheckIfUserIsSelf(personId);

            PaymentPlan plan = null;

            FinanceConfiguration config = _financeConfigurationRepository.GetFinanceConfiguration();
            if (config != null)
            {
                // Determine if the supplied receivable type is payable
                if (!config.IsReceivableTypePayable(receivableTypeCode))
                {
                    logger.Info("Receivable type " + receivableTypeCode + " is not payable; user cannot create a payment plan at this time.");
                    return plan;
                }

                // Pull account holder information
                var accountHolder = await _arRepository.GetAccountHolderAsync(personId);

                // Determine account holder's payment plan eligibility
                bool isPlanEligible = AccountHolderIsEligibleForPaymentPlanCreation(config, accountHolder);
                if (isPlanEligible)
                {
                    // Get the applicable payment plan option for the accounT holder, term, and receivableType
                    var paymentPlanOption = GetApplicablePaymentPlanOption(termId, receivableTypeCode, config, accountHolder);
                    if (paymentPlanOption != null)
                    {
                        // Get the associated payment plan template 
                        var template = _paymentPlanRepository.GetTemplate(paymentPlanOption.TemplateId);

                        // Determine if the template is valid for user payment plan creation
                        if (template != null)
                        {
                            List<string> messages;
                            Domain.Finance.Entities.PaymentPlanIneligibilityReason? reason;
                            if (template.IsValidForUserPaymentPlanCreation(receivableTypeCode, planAmount, out messages, out reason))
                            {
                                // Pull first payment date from the applicable payment plan option, set it to today if in the past or null
                                DateTime firstPaymentDate = (paymentPlanOption.FirstPaymentDate != null) ? (paymentPlanOption.FirstPaymentDate < DateTime.Today) ? DateTime.Today 
                                    : paymentPlanOption.FirstPaymentDate : DateTime.Today;

                                try
                                {
                                    // Get the proposed payment plan
                                    var proposedPlanEntity = await _paymentPlanRepository.GetProposedPaymentPlanAsync(personId,
                                        termId,
                                        receivableTypeCode,
                                        template.Id,
                                        firstPaymentDate,
                                        planAmount);

                                    // Convert plan to DTO and return
                                    var paymentPlanAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan, PaymentPlan>();
                                    return paymentPlanAdapter.MapToType(proposedPlanEntity);
                                }
                                catch (ColleagueSessionExpiredException csee)
                                {
                                    logger.Error(csee, csee.Message);
                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    // Log and re-throw any exceptions from the plan creation process.
                                    string errorMessage = "Could not create proposed payment plan for person " + personId + 
                                        " for term " + termId + 
                                        " for receivable type " + receivableTypeCode + 
                                        " with plan amount " + planAmount.ToString("C2") +
                                        ": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                                    logger.Info(errorMessage);
                                    throw new ApplicationException(errorMessage);
                                }
                            }
                            else
                            {
                                // Log any messages related to the payment plan template's validity towards payment plan creation 
                                if (messages != null) messages.ForEach(m => logger.Info(m));
                            }
                        }
                        else
                        {
                            string errorMessage = "Payment plan template " + paymentPlanOption.TemplateId + " is null.";
                            logger.Info(errorMessage);
                            throw new ApplicationException(errorMessage);
                        }
                    }
                    else
                    {
                        logger.Info("Payment plans cannot be created on " + DateTime.Today.ToShortDateString() + " for " + termId + "; user cannot create a payment plan for " + termId + " for receivable type " + receivableTypeCode);
                    }
                }
                return plan;
            }
            // Finance configuration data is null; log an error.
            else
            {
                string errorMessage = "Finance configuration data is null.";
                logger.Info(errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        /// <summary>
        /// Validate a collection of billing term payment plan information for processing
        /// </summary>
        /// <param name="billingTerms">Collection of billing term payment plan information objects</param>
        private void ValidateBillingTermPaymentPlanInformation(IEnumerable<BillingTermPaymentPlanInformation> billingTerms)
        {
            if (billingTerms == null || !billingTerms.Any())
            {
                throw new ArgumentNullException("billingTerms", "Billing term payment plan information for at least one term must be specified.");
            }
            if (billingTerms.Select(bt => bt.PersonId).Distinct().Count() != 1)
            {
                throw new ApplicationException("Cannot process billing term payment plan information for more than one person at a time.");
            }
        }

        /// <summary>
        /// Gets the applicable payment plan option for a given person in a given term for a given receivable type
        /// </summary>
        /// <param name="termId">Term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <returns>Applicable payment plan template</returns>
        private Domain.Finance.Entities.PaymentPlanOption GetApplicablePaymentPlanOption(string termId, string receivableTypeCode, 
            FinanceConfiguration config, Domain.Finance.Entities.AccountHolder accountHolder)
        {
            Domain.Finance.Entities.PaymentPlanOption paymentPlanOption = null;

            // Retrieve finance configuration information
            if (config != null)
            {
                // Determine if user payment plan creation is enabled
                if (config.UserPaymentPlanCreationEnabled)
                {
                    // Identify the subset of term payment plan options for the billing term
                    var termPaymentPlanRequirements = config.TermPaymentPlanRequirements.Where(tpp => tpp.TermId == termId).ToList();
                    if (termPaymentPlanRequirements.Any())
                    {
                        // Initialize the applicable option variable to the default payment plan requirement definition for that term
                        var termPaymentPlanRequirement = termPaymentPlanRequirements.Where(tp => tp.ProcessingOrder == 0).FirstOrDefault();

                        // Evaluate each rule-specific term payment plan requirement
                        foreach (var option in termPaymentPlanRequirements)
                        {
                            // Ignore the default payment plan requirement; it has no associated rule
                            if (!string.IsNullOrEmpty(option.EligibilityRuleId))
                            {
                                logger.Info("Evaluating account holder " + accountHolder.Id + " against rule " + option.EligibilityRuleId +
                                    " to determine eligibility for rule-based payment requirement in term " + termId + "...");
                                var accountHolderRule = new Domain.Base.Entities.Rule<Domain.Finance.Entities.AccountHolder>(option.EligibilityRuleId);
                                var accountHolderRequests = new List<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>>() 
                                { 
                                    new Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>(accountHolderRule, accountHolder) 
                                };
                                var ruleResults = _ruleRepository.Execute(accountHolderRequests).ToList();

                                // If the account holder passes the rule, use the associated payment plan requirement
                                if (ruleResults[0].Passed)
                                {
                                    logger.Info("Account holder " + accountHolder.Id + " passed rule " + option.EligibilityRuleId +
                                        " and is eligible for rule-based payment requirement in term " + termId);
                                    termPaymentPlanRequirement = option;
                                    break;
                                }
                            }
                        }

                        // With the applicable payment plan requirement identified, find the applicable payment plan option
                        if (termPaymentPlanRequirement != null)
                        {
                            paymentPlanOption = ReceivableService.GetApplicablePaymentPlanOption(termPaymentPlanRequirement);
                        }
                        else
                        {
                            logger.Info("No default or rule-specific payment plan requirements for billing term " + termId + " apply to account holder " + accountHolder.Id + "; user cannot create a payment plan for " + termId + " for receivable type " + receivableTypeCode);
                        }
                    }
                    else
                    {
                        logger.Info("No payment plan requirements are defined for billing term " + termId + "; user cannot create a payment plan for " + termId + " for receivable type " + receivableTypeCode);
                    }
                }
                else
                {
                    logger.Info("User payment plan creation is disabled; user cannot create a payment plan for any term/receivable type.");
                }
            }
            else
            {
                string errorMessage = "Finance configuration data is null.";
                logger.Info(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            return paymentPlanOption;
        }

        /// <summary>
        /// Evaluates an account holder against payment plan eligibility rules
        /// </summary>
        /// <param name="config">Finance configuration data</param>
        /// <param name="accountHolder">Account Holder</param>
        private bool AccountHolderIsEligibleForPaymentPlanCreation(FinanceConfiguration config, Domain.Finance.Entities.AccountHolder accountHolder)
        {
            // Evaluate account holder against any global payment plan eligibility rules
            var ruleResults = new List<Domain.Base.Entities.RuleResult>();
            if (config.PaymentPlanEligibilityRuleIds != null && config.PaymentPlanEligibilityRuleIds.Any())
            {
                logger.Info("Evaluating account holder " + accountHolder.Id +
                    " to determine eligibility for payment plan creation...");
                // Build rule requests for each payment plan eligibility rule declared in finance configuration
                var accountHolderRequests = new List<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>>();
                foreach (var ruleId in config.PaymentPlanEligibilityRuleIds)
                {
                    var accountHolderRule = new Domain.Base.Entities.Rule<Domain.Finance.Entities.AccountHolder>(ruleId);
                    accountHolderRequests.Add(new Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>(accountHolderRule, accountHolder));
                }

                // Execute the rule requests
                ruleResults.AddRange(_ruleRepository.Execute(accountHolderRequests));

                // The account holder must pass all specified rules; if the account holder does not 
                // pass one or more rules, then the account holder cannot sign up for a payment plan
                var failedRules = ruleResults.Where(rr => !rr.Passed).Select(rr => rr.RuleId).ToList();
                if (failedRules.Any())
                {
                    logger.Info("Account Holder " + accountHolder.Id + "did not pass all User Eligibility Rules for creating payment plans; user cannot create a payment plan for any term/receivable type." +
                        " Failed rules: " + string.Join(",", failedRules));
                    return false;
                } else {
                    logger.Info("Account Holder " + accountHolder.Id + "passed all User Eligibility Rules for creating payment plans.");

                }
                return true;
            }
            else
            {
                logger.Info("No User Eligibility Rules specified for user-created payment plans. Continuing...");
                return true;
            }
        }
    }
}
