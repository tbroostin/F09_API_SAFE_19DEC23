// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    /// <summary>
    /// Provider for registration billing coordination services.
    /// </summary>
    [RegisterType]
    public class RegistrationBillingService : FinanceCoordinationService, IRegistrationBillingService
    {
        private IRegistrationBillingRepository _rbRepository;
        private IAccountsReceivableRepository _arRepository;
        private IAccountDueRepository _adRepository;
        private IFinanceConfigurationRepository _configRepository;
        private IPaymentRepository _payRepository;
        private IPaymentPlanRepository _payPlanRepository;
        private IRuleRepository _ruleRepository;
        private IPaymentPlanProcessor _payPlanProcessor;
        private IImmediatePaymentService _ipcService;
        private IDocumentRepository _documentRepository;
        private IApprovalService _approvalService;

        private Domain.Finance.Entities.ImmediatePaymentControl _ipcConfig;
        private const string _ImmediatePaymentDistribution = "IP";

        /// <summary>
        /// Constructor for the registration billing service
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="rbRepository">Interface to RegistrationBillingRepository</param>
        /// <param name="arRepository">Interface to AccountsReceivableRepository</param>
        /// <param name="adRepository">Interface to AccountDueRepository</param>
        /// <param name="configRepository">Interface to FinanceConfigurationRepository</param>
        /// <param name="paymentRepository">Interface to PaymentRepository</param>
        /// <param name="payPlanRepository">Interface to PaymentPlanRepository</param>
        /// <param name="approvalService">Interface to ApprovalService</param>
        /// <param name="ruleRepository">Interface to RuleRepository</param>
        /// <param name="documentRepository">Interface to DocumentRepository</param>
        /// <param name="currentUserFactory">Interface to CurrentUserFactory</param>
        /// <param name="roleRepository">Interface to RoleRepository</param>
        /// <param name="logger">Interface to Logger</param>
        public RegistrationBillingService(IAdapterRegistry adapterRegistry, IRegistrationBillingRepository rbRepository, IAccountsReceivableRepository arRepository,
            IAccountDueRepository adRepository, IFinanceConfigurationRepository configRepository, IPaymentRepository paymentRepository,
            IPaymentPlanRepository payPlanRepository, IApprovalService approvalService, IRuleRepository ruleRepository, IDocumentRepository documentRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _rbRepository = rbRepository;
            _arRepository = arRepository;
            _adRepository = adRepository;
            _configRepository = configRepository;
            _payRepository = paymentRepository;
            _payPlanRepository = payPlanRepository;
            _ruleRepository = ruleRepository;
            _documentRepository = documentRepository;
            _approvalService = approvalService;

            _ipcConfig = _configRepository.GetImmediatePaymentControl();

            _payPlanProcessor = new PaymentPlanProcessor(ProcessInvoiceExclusionRules, logger);
            _ipcService = new ImmediatePaymentService(logger);
        }

        /// <summary>
        /// Get a registration payment control
        /// </summary>
        /// <param name="id">Payment Control ID</param>
        /// <returns>RegistrationPaymentControl DTO</returns>
        public RegistrationPaymentControl GetPaymentControl(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Payment control ID is required.");
            }
            var rpcEntity = _rbRepository.GetPaymentControl(id);
            if (rpcEntity == null)
            {
                throw new KeyNotFoundException(id + " is not a valid ID.");
            }
            CheckAccountPermission(rpcEntity.StudentId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>();
            var rpcDto = adapter.MapToType(rpcEntity);

            return rpcDto;
        }

        /// <summary>
        /// Get all incomplete registration payment controls for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of RegistrationPaymentControl DTOs</returns>
        public IEnumerable<RegistrationPaymentControl> GetStudentPaymentControls(string studentId)
        {
            if (string.IsNullOrEmpty(studentId)) 
            {
                throw new ArgumentNullException("studentId", "Student ID is required.");
            }
            CheckAccountPermission(studentId);

            var entities = _rbRepository.GetStudentPaymentControls(studentId);
            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>();
            var dtos = new List<RegistrationPaymentControl>();

            if (entities != null)
            {
                foreach (var item in entities)
                {
                    dtos.Add(adapter.MapToType(item));
                }
            }

            return dtos;
        }

        /// <summary>
        /// Get a document for a registration payment control
        /// </summary>
        /// <param name="id">Payment Control ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>TextDocument DTO</returns>
        public TextDocument GetPaymentControlDocument(string id, string documentId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A payment control ID must be provided");
            }

            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "A document ID must be provided");
            }


            var rpcEntity = _rbRepository.GetPaymentControl(id);
            if (rpcEntity == null)
            {
                throw new KeyNotFoundException(id + " is not a valid payment control ID.");
            }
            CheckAccountPermission(rpcEntity.StudentId);

            var docEntity = _documentRepository.Build(documentId, "IPC.REGISTRATION", id, rpcEntity.StudentId, null);
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.TextDocument, TextDocument>();
            var docDto = adapter.MapToType(docEntity);

            return docDto;
        }

        /// <summary>
        /// Accept the terms and conditions associated with a registration
        /// </summary>
        /// <param name="acceptance">The information for the acceptance</param>
        /// <returns>The updated approval information</returns>
        [Obsolete("Obsolete as of API 1.5; call ApproveRegistrationTerms2 instead.")]
        public RegistrationTermsApproval ApproveRegistrationTerms(PaymentTermsAcceptance acceptance)
        {
            // Only student can do approvals
            if (!UserIsSelf(acceptance.StudentId))
            {
                logger.Info(CurrentUser + " cannot make approvals for student " + acceptance.StudentId);
                throw new PermissionsException();
            }

            // Add the user's login ID to the DTO so we can create the entity
            acceptance.ApprovalUserId = CurrentUser.UserId;

            // Convert the DTO to an entity, process the acceptance, convert the resulting entity
            // back to a DTO, and return it
            var acceptanceAdapter = _adapterRegistry.GetAdapter<PaymentTermsAcceptance, Domain.Finance.Entities.PaymentTermsAcceptance>();
            var acceptanceEntity = acceptanceAdapter.MapToType(acceptance);
            var approvalEntity = _rbRepository.ApproveRegistrationTerms(acceptanceEntity);
            string regApprovalId = approvalEntity.Id;

            #pragma warning disable 618
            return GetTermsApproval(regApprovalId);
            #pragma warning restore 618
        }

        /// <summary>
        /// Accept the terms and conditions associated with a registration
        /// </summary>
        /// <param name="acceptance">The information for the acceptance</param>
        /// <returns>The updated approval information</returns>
        public RegistrationTermsApproval2 ApproveRegistrationTerms2(PaymentTermsAcceptance2 acceptance)
        {
            // Only student can do approvals
            CheckPaymentPermission(acceptance.StudentId, CurrentUser + " cannot make approvals for student " + acceptance.StudentId);

            // Add the user's login ID to the DTO so we can create the entity
            acceptance.ApprovalUserId = CurrentUser.UserId;

            // Convert the DTO to an entity, process the acceptance, convert the resulting entity
            // back to a DTO, and return it
            var acceptanceAdapter = _adapterRegistry.GetAdapter<PaymentTermsAcceptance2, Domain.Finance.Entities.PaymentTermsAcceptance>();
            var acceptanceEntity = acceptanceAdapter.MapToType(acceptance);
            var approvalEntity = _rbRepository.ApproveRegistrationTerms(acceptanceEntity);
            string regApprovalId = approvalEntity.Id;

            return GetTermsApproval2(regApprovalId);
        }

        /// <summary>
        /// Update an existing registration payment control record
        /// </summary>
        /// <param name="rpcInputDto">Registration payment control to update</param>
        /// <returns>Updated registration payment control</returns>
        public RegistrationPaymentControl UpdatePaymentControl(RegistrationPaymentControl rpcInputDto)
        {
            if (rpcInputDto == null)
            {
                throw new ArgumentNullException("rpcInputDto", "Cannot update a null Registration Payment Control.");
            }

            CheckPaymentPermission(rpcInputDto.StudentId);

            var dtoAdapter = _adapterRegistry.GetAdapter<RegistrationPaymentControl, Domain.Finance.Entities.RegistrationPaymentControl>();
            var rpcInputEntity = dtoAdapter.MapToType(rpcInputDto);

            var rpcOutputEntity = _rbRepository.UpdatePaymentControl(rpcInputEntity);
            var entityAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.RegistrationPaymentControl, RegistrationPaymentControl>();
            var rpcOutputDto = entityAdapter.MapToType(rpcOutputEntity);

            return rpcOutputDto;
        }

        /// <summary>
        /// Gets the payment options for a student
        /// </summary>
        /// <param name="paymentControlId">ID of the payment control record for the student/term</param>
        /// <returns>ImmediatePaymentOptions DTO outlining the payment options available to the student</returns>
        public ImmediatePaymentOptions GetPaymentOptions(string paymentControlId)
        {
            if (string.IsNullOrEmpty(paymentControlId))
            {
                throw new ArgumentNullException("paymentControlId", "A registration payment control record ID must be provided.");
            }

            // Get the Registration Payment Control using the passed-in ID
            var paymentControl = _rbRepository.GetPaymentControl(paymentControlId);
            if (paymentControl == null)
            {
                throw new KeyNotFoundException(paymentControlId + " is not a valid payment control ID.");
            }
            // Check user permissions
            CheckAccountPermission(paymentControl.StudentId);

            // Initialize variables
            List<Domain.Finance.Entities.Invoice> invoices = null;
            List<Domain.Finance.Entities.ReceivablePayment> payments = null;
            List<Domain.Finance.Entities.AccountDue.AccountTerm> accountTerms = null;
            Dictionary<string, string> distributionMap = null;
            Domain.Finance.Entities.PaymentRequirement payReq = null;
            IEnumerable<Domain.Finance.Entities.ChargeCode> chargeCodes = null;
            Domain.Finance.Entities.PaymentPlanTemplate paymentPlanTemplate = null;

            // Get the necessary data
            GetPaymentData(paymentControl, out invoices, out payments, out accountTerms, out distributionMap, out payReq);
            var payPlanOption = _ipcService.GetPaymentPlanOption(payReq);
            string payPlanTemplateId = payPlanOption == null ? null : payPlanOption.TemplateId;
            DateTime? firstPaymentDate = payPlanOption == null ? (DateTime?)null : payPlanOption.FirstPaymentDate;
            
            paymentPlanTemplate = !string.IsNullOrEmpty(payPlanTemplateId) ? _payPlanRepository.GetTemplate(payPlanTemplateId) : null;
            chargeCodes = _arRepository.ChargeCodes;

            // Call the domain service to perform the business logic
            var optionsEntity = _ipcService.GetPaymentOptions(paymentControl, invoices, payments, accountTerms, distributionMap, payReq, chargeCodes);
            var optionsAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ImmediatePaymentOptions, ImmediatePaymentOptions>();

            // Determine if the student can sign up for a payment plan - is the template valid?
            if (paymentPlanTemplate == null || !_ipcService.IsValidTemplate(paymentPlanTemplate))
            {
                return optionsAdapter.MapToType(optionsEntity);
            }

            // Determine if the Payment Plan template has a terms and conditions document specified, but the document does not contain any text
            TextDocument termsText = new TextDocument();
            try
            {
                termsText = GetPaymentControlDocument(paymentControl.Id, paymentPlanTemplate.TermsAndConditionsDocumentId);
            }
            catch (InvalidOperationException)
            {
                string outFormat = "Terms and Conditions Document {0} on Payment Plan Template {1} does not have any text specified.";
                logger.Info(string.Format(outFormat, paymentPlanTemplate.TermsAndConditionsDocumentId, paymentPlanTemplate.Id));
                return optionsAdapter.MapToType(optionsEntity);
            }

            // Calculate the amount of the payment plan and get the receivable type code for the plan
            string payPlanReceivableTypeCode = null;
            decimal payPlanAmount = _payPlanProcessor.GetPlanAmount(optionsEntity.RegistrationBalance, optionsEntity.RegistrationBalance, paymentPlanTemplate, invoices, payments, chargeCodes,
                true, out payPlanReceivableTypeCode);

            // Payment Plan cannot be created because the calculated plan amount is zero or less - return default options
            if (payPlanAmount <= 0)
            {
                return optionsAdapter.MapToType(optionsEntity);
            }

            // Add payment plan to payment options
            // var planCharges = _payPlanProcessor.GetPlanCharges(payPlanReceivableTypeCode, payPlanAmount, paymentPlanTemplate, invoices);
            optionsEntity.AddPaymentPlanInformation(paymentPlanTemplate.Id, firstPaymentDate, payPlanAmount, payPlanReceivableTypeCode,
                paymentPlanTemplate.CalculateDownPaymentAmount(payPlanAmount), paymentPlanTemplate.DownPaymentDate);

            var optionsDto = optionsAdapter.MapToType(optionsEntity);

            return optionsDto;
        }

        /// <summary>
        /// Get the payment summary for a payment control, pay method, and payment amount
        /// </summary>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <returns>List of payments to be made</returns>
        public IEnumerable<Payment> GetPaymentSummary(string id, string payMethod, decimal amount)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A registration payment control record ID must be provided.");
            }
            if (string.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod", "A payment method must be provided.");
            }
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number.", "amount");
            }

            // Get the Registration Payment Control using the passed-in ID
            var paymentControl = _rbRepository.GetPaymentControl(id);

            // Check user permissions
            CheckPaymentPermission(paymentControl.StudentId);

            // Get the needed data
            List<Domain.Finance.Entities.Invoice> invoices = null;
            List<Domain.Finance.Entities.ReceivablePayment> payments = null;
            List<Domain.Finance.Entities.AccountDue.AccountTerm> accountTerms = null;
            Dictionary<string, string> distributionMap = null;
            Domain.Finance.Entities.PaymentRequirement payReq = null;
            GetPaymentData(paymentControl, out invoices, out payments, out accountTerms, out distributionMap, out payReq);
            var confMap = new Dictionary<string, Domain.Finance.Entities.Payments.PaymentConfirmation>();
            foreach (string distribution in distributionMap.Values.Distinct())
            {
                var ecInfo = _payRepository.GetConfirmation(distribution, payMethod, amount.ToString());
                confMap.Add(distribution, ecInfo);
            }
            var receivableTypes = _arRepository.ReceivableTypes;

            // Call the domain service to perform the business logic, and convert the output to a DTO
            var summaryEntity = _ipcService.GetPaymentSummary(paymentControl, payMethod, amount, invoices, payments, accountTerms, distributionMap, payReq, confMap, receivableTypes);
            var summaryAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Payments.Payment, Payment>();
            var summaryDto = new List<Payment>();
            foreach (var item in summaryEntity)
            {
                summaryDto.Add(summaryAdapter.MapToType(item));
            }

            return summaryDto.OrderBy(x => x.Distribution);
        }

        /// <summary>
        /// Post a registration payment
        /// </summary>
        /// <param name="paymentDto">Registration Payment DTO</param>
        /// <returns>Payment Provider DTO</returns>
        public PaymentProvider StartRegistrationPayment(Payment paymentDto)
        {
            if (paymentDto == null)
            {
                throw new ArgumentNullException("paymentDto", "Cannot post a null registration payment.");
            }

            // Determine whether current user is the student for which the payment is being made
            CheckPaymentPermission(paymentDto.PersonId);

            // Convert the DTO to an entity and post the payment
            var adapter = _adapterRegistry.GetAdapter<Payment, Domain.Finance.Entities.Payments.Payment>();
            var paymentEntity = adapter.MapToType(paymentDto);

            // Convert the entity to a DTO
            var providerEntity = _rbRepository.StartRegistrationPayment(paymentEntity);
            var providerAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Payments.PaymentProvider, PaymentProvider>();

            // Return the DTO
            return providerAdapter.MapToType(providerEntity);
        }

        /// <summary>
        /// Get a registration approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>RegistrationApproval DTO</returns>
        [Obsolete("Obsolete as of API 1.5; use GetTermsApproval2() method instead.", false)]
        public RegistrationTermsApproval GetTermsApproval(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Approval ID is required for retrieval.");
            }

            var apprEntity = _rbRepository.GetTermsApproval(id);

            // Check user permissions
            CheckAccountPermission(apprEntity.StudentId);

            // Just build the old-style DTO manually, reading other data as needed
            var apprDto = new RegistrationTermsApproval()
                {
                    Id = apprEntity.Id,
                    StudentId = apprEntity.StudentId,
                    PaymentControlId = apprEntity.PaymentControlId
                };
            apprDto.SectionIds.AddRange(apprEntity.SectionIds);
            apprDto.InvoiceIds.AddRange(apprEntity.InvoiceIds);

            var ackDocument = _approvalService.GetApprovalDocument(apprEntity.AcknowledgementDocumentId);
            var termsResponse = _approvalService.GetApprovalResponse(apprEntity.TermsResponseId);
            var termsDocument = _approvalService.GetApprovalDocument(termsResponse.DocumentId);

            apprDto.TermsResponse = termsResponse;
            apprDto.TermsDocument = termsDocument;
            apprDto.AcknowledgementDocument = ackDocument;
            apprDto.Timestamp = termsResponse.Received.UtcDateTime;

            return apprDto;
        }

        /// <summary>
        /// Get a registration approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>RegistrationApproval DTO</returns>
        public RegistrationTermsApproval2 GetTermsApproval2(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Approval ID is required for retrieval.");
            }

            var apprEntity = _rbRepository.GetTermsApproval(id);

            // Check user permissions
            CheckAccountPermission(apprEntity.StudentId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.RegistrationTermsApproval, RegistrationTermsApproval2>();
            var apprDto = adapter.MapToType(apprEntity);

            return apprDto;
        }

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <param name="payControlId">ID of a payment control record</param>
        /// <param name="receivableType">Receivable Type for proposed payment plan</param>
        /// <returns>PaymentPlan DTO</returns>
        public PaymentPlan GetProposedPaymentPlan(string payControlId, string receivableType)
        {
            if (String.IsNullOrEmpty(payControlId))
            {
                throw new ArgumentNullException("payControlId", "Payment Control ID is required for proposed payment plan retrieval.");
            }
            if (String.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable Type is required for proposed payment plan retrieval.");
            }
            var paymentControl = _rbRepository.GetPaymentControl(payControlId);

            // Check user permissions
            CheckPaymentPermission(paymentControl.StudentId);

            // Get the required data
            List<Domain.Finance.Entities.Invoice> invoices = null;
            List<Domain.Finance.Entities.ReceivablePayment>payments = null;
            List<Domain.Finance.Entities.AccountDue.AccountTerm> accountTerms = null;
            Dictionary<string, string> distributionMap = null;
            Domain.Finance.Entities.PaymentRequirement payReq = null;
            GetPaymentData(paymentControl, out invoices, out payments, out accountTerms, out distributionMap, out payReq);

            // Get the required plan information
            var payPlanOption = _ipcService.GetPaymentPlanOption(payReq);
            string templateId = payPlanOption.TemplateId;
            DateTime? firstDueDate = payPlanOption.FirstPaymentDate;
            var template = _payPlanRepository.GetTemplate(templateId);
            
            // Get the plan amount and receivable type
            var registrationBalance = invoices.Sum(invoice => invoice.Amount) - payments.Sum(payment => payment.Amount);
            var termBalance = accountTerms.First(x => x.TermId == paymentControl.TermId).Amount;
            string planReceivableTypeCode = null;
            var planAmount = _payPlanProcessor.GetPlanAmount(registrationBalance, termBalance, template, invoices, payments, _arRepository.ChargeCodes, 
                false, out planReceivableTypeCode);
            DateTime? downPaymentDate = template.DownPaymentDate;
            DateTime firstPaymentDate = (firstDueDate.HasValue) ? (firstDueDate.Value.Date < DateTime.Today.Date) ? DateTime.Today : firstDueDate.Value : DateTime.Today;

            // Get the proposed plan
            var planCharges = _payPlanProcessor.GetPlanCharges(planReceivableTypeCode, planAmount, template, invoices);
            var proposedPlanEntity = _payPlanProcessor.GetProposedPlan(template, paymentControl.StudentId, planReceivableTypeCode, paymentControl.TermId,
                planAmount, firstDueDate.Value, planCharges);
            var scheduleDates = GetPlanScheduleDates(template, proposedPlanEntity, firstPaymentDate);
            _payPlanProcessor.AddPlanSchedules(template, proposedPlanEntity, scheduleDates);

            var paymentPlanAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan, PaymentPlan>();
            PaymentPlan proposedPlanDto = paymentPlanAdapter.MapToType(proposedPlanEntity);

            return proposedPlanDto;
        }

        /// <summary>
        /// Processes a collection of invoices against a list of invoice exclusion rule IDs
        /// </summary>
        /// <param name="invoices">Collection of invoice entities</param>
        /// <param name="invoiceExclusionRuleIds">Collection of invoice exclusion rule IDs</param>
        /// <returns>List of invoices that do not pass invoice exclusion rules</returns>
        public List<Domain.Finance.Entities.Invoice> ProcessInvoiceExclusionRules(List<Domain.Finance.Entities.Invoice> invoices, IEnumerable<string> invoiceExclusionRuleIds)
        {
            if (invoices == null || invoices.Count == 0)
            {
                throw new ArgumentNullException("invoices", "At least one invoice must be provided.");
            }

            var includedInvoices = new List<Domain.Finance.Entities.Invoice>();

            if (invoiceExclusionRuleIds != null && invoiceExclusionRuleIds.Count() > 0)
            {
                var ruleResults = new List<Domain.Base.Entities.RuleResult>();

                foreach (var invoice in invoices)
                {
                    var invoiceRequests = new List<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.Invoice>>();
                    foreach (var exclusionRuleId in invoiceExclusionRuleIds)
                    {
                        var invoiceRule = new Domain.Base.Entities.Rule<Domain.Finance.Entities.Invoice>(exclusionRuleId);
                        invoiceRequests.Add(new Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.Invoice>(invoiceRule, invoice));
                    }

                    ruleResults = _ruleRepository.Execute(invoiceRequests).ToList();
                    // This condition seems backwards because these are exclusion rules - 
                    // any invoice that passes any of the rules gets excluded
                    if (!ruleResults.Any(result => result.Passed))
                    {
                        includedInvoices.Add(invoice);
                    }
                }
            }
            else
            {
                includedInvoices = invoices;
            }

            return includedInvoices;
        }

        /// <summary>
        /// Evaluate a student's payment requirements
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="paymentRequirements">List of payment requirements</param>
        /// <returns>The payment requirement applicable to this student</returns>
        public Domain.Finance.Entities.PaymentRequirement EvaluatePaymentRequirements(string studentId, IEnumerable<Domain.Finance.Entities.PaymentRequirement> paymentRequirements)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A Student ID must be provided");
            }

            if (paymentRequirements == null || paymentRequirements.Count() == 0)
            {
                throw new ArgumentNullException("paymentRequirements", "At least one payment requirement must be provided.");
            }

            var ruleIds = new List<string>();
            var ruleResults = new List<Domain.Base.Entities.RuleResult>();
            var acctHolder = _arRepository.GetAccountHolder(studentId);

            // Set the payment requirement to the default payment requirement for the term (no associated rule)
            var paymentReq = paymentRequirements.First(x => x.ProcessingOrder == 0);

            // Evaluate each payment requirement with a rule to see if the Account Holder passes the rule
            foreach (var pmtReq in paymentRequirements)
            {
                // Ignore the default payment requirement; it has no associated rule
                if (!string.IsNullOrEmpty(pmtReq.EligibilityRuleId))
                {
                    var acctHldrRule = new Domain.Base.Entities.Rule<Domain.Finance.Entities.AccountHolder>(pmtReq.EligibilityRuleId);
                    var acctHldrRequests = new List<Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>>() 
                        { new Domain.Base.Entities.RuleRequest<Domain.Finance.Entities.AccountHolder>(acctHldrRule, acctHolder) };
                    ruleResults = _ruleRepository.Execute(acctHldrRequests).ToList();

                    // If the Account Holder passes the rule, use the associated Payment Requirement
                    if (ruleResults[0].Passed)
                    {
                        return pmtReq;
                    }
                }
            }
            return paymentReq;
        }

        /// <summary>
        /// Get the list of planned schedule dates for a payment plan with a given template and first due date
        /// </summary>
        /// <param name="template">Payment Plan Template entity</param>
        /// <param name="proposedPlan">Proposed payment plan entity</param>
        /// <param name="firstPaymentDate">Due date of first scheduled payment</param>
        /// <returns>A list of payment dates; the first date in the list is the down payment date, if any.</returns>
        private List<DateTime?> GetPlanScheduleDates(Domain.Finance.Entities.PaymentPlanTemplate template, Domain.Finance.Entities.PaymentPlan proposedPlan, DateTime firstPaymentDate)
        {
            if (template == null)
            {
                throw new ArgumentNullException("template", "template cannot be null when building a plan schedule.");
            }
            if (proposedPlan == null)
            {
                throw new ArgumentNullException("proposedPlan", "proposed plan cannot be null when building a plan schedule.");
            }

            // Get the schedule dates
            List<DateTime?> scheduleDates = new List<DateTime?>();
            if (proposedPlan.Frequency == Domain.Finance.Entities.PlanFrequency.Custom)
            {
                scheduleDates = _payPlanRepository.GetPlanCustomScheduleDates(proposedPlan.PersonId, proposedPlan.ReceivableTypeCode, proposedPlan.TermId,
                    proposedPlan.TemplateId, template.DownPaymentDate, firstPaymentDate, proposedPlan.Id).ToList();
                proposedPlan.NumberOfPayments = scheduleDates.Count - 1;
            }
            else
            {
                scheduleDates.Add(template.DownPaymentDate);
                scheduleDates.AddRange(template.GetPaymentScheduleDates(firstPaymentDate).Select(x => x as DateTime?));
            }

            return scheduleDates;
        }

        /// <summary>
        /// Get the various data elements needed for payment processing
        /// </summary>
        /// <param name="paymentControl">Payment control</param>
        /// <param name="invoices">List of registration invoices</param>
        /// <param name="payments">List of registration payments</param>
        /// <param name="accountTerms">List of account terms</param>
        /// <param name="distributionMap">Mapping of receivable types to distributions</param>
        /// <param name="payReq">Payment requirement definition</param>
        private void GetPaymentData(Domain.Finance.Entities.RegistrationPaymentControl paymentControl, out List<Domain.Finance.Entities.Invoice> invoices,
            out List<Domain.Finance.Entities.ReceivablePayment> payments, out List<Domain.Finance.Entities.AccountDue.AccountTerm> accountTerms,
            out Dictionary<string, string> distributionMap, out Domain.Finance.Entities.PaymentRequirement payReq)
        {
            if (paymentControl == null)
            {
                throw new ArgumentNullException("paymentControl");
            }

            invoices = new List<Domain.Finance.Entities.Invoice>();
            if (paymentControl.InvoiceIds != null && paymentControl.InvoiceIds.Count > 0)
            {
                invoices = _arRepository.GetInvoices(paymentControl.InvoiceIds).ToList();
            }
            payments = new List<Domain.Finance.Entities.ReceivablePayment>();
            if (paymentControl.Payments != null && paymentControl.Payments.Count > 0)
            {
                payments = _arRepository.GetPayments(paymentControl.Payments).ToList();
            }

            distributionMap = GetDistributionMap(invoices);

            payReq = GetPaymentRequirement(paymentControl.StudentId, paymentControl.TermId);

            // Get the account terms for calculating the term balance
            accountTerms = GetAccountTerms(paymentControl.StudentId);
        }

        /// <summary>
        /// Get the specific payment requirement that applies to this student today
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="termId">Term ID</param>
        /// <returns>The payment requirement applicable to this student today</returns>
        private Domain.Finance.Entities.PaymentRequirement GetPaymentRequirement(string studentId, string termId)
        {
            Domain.Finance.Entities.PaymentRequirement payReq = null;
            // Get the payment requirements for the term
            var paymentReqs = _rbRepository.GetPaymentRequirements(termId);

            if (paymentReqs != null && paymentReqs.Count() > 0)
            {
                // If there's only one requirement, then there's no need for evaluation
                if (paymentReqs.Count() == 1)
                {
                    return paymentReqs.First();
                }
                // Get the applicable payment requirement for the student
                payReq = EvaluatePaymentRequirements(studentId, paymentReqs);
            }
            return payReq;
        }

        /// <summary>
        /// Get the distribution map for a group of invoices
        /// </summary>
        /// <param name="invoices">List of registration invoices for a student</param>
        /// <returns>Dictionary of receivable types to distribution codes</returns>
        private Dictionary<string, string> GetDistributionMap(IEnumerable<Domain.Finance.Entities.Invoice> invoices)
        {
            var distributionMap = new Dictionary<string, string>();

            string studentId = invoices.Select(x => x.PersonId).Distinct().SingleOrDefault();
            var receivableTypeCodes = invoices.Select(x => x.ReceivableTypeCode).Distinct().ToList();
            if (string.IsNullOrEmpty(studentId) || receivableTypeCodes == null || receivableTypeCodes.Count == 0)
            {
                return distributionMap;
            }

            var distributions = _arRepository.GetDistributions(studentId, receivableTypeCodes, _ImmediatePaymentDistribution).ToList();
            if (distributions == null || distributions.Count == 0)
            {
                return distributionMap;
            }

            for (int i = 0; i < receivableTypeCodes.Count; i++)
            {
                distributionMap.Add(receivableTypeCodes[i], distributions[i]);
            }

            return distributionMap;
        }

        /// <summary>
        /// Calculates the term balance for a student
        /// </summary>
        /// <param name="paymentControl">The payment control for a student, which contains the student ID and term ID needed to obtain the term balance</param>
        /// <returns>A student's term balance</returns>
        private List<AccountTerm> GetAccountTerms(string studentId)
        {
            // Determine whether the institution is using Term or PCF Mode
            var financeConfig = _configRepository.GetFinanceConfiguration();
            List<AccountTerm> accountTerms = new List<AccountTerm>();

            if (financeConfig.PaymentDisplay == PaymentDisplay.DisplayByPeriod)
            {
                // PCF Mode
                var accountDuePeriod = _adRepository.GetPeriods(studentId);
                if (accountDuePeriod.Current != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Current.AccountTerms);
                }
                if (accountDuePeriod.Future != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Future.AccountTerms);
                }
                if (accountDuePeriod.Past != null)
                {
                    accountTerms.AddRange(accountDuePeriod.Past.AccountTerms);
                }
            }
            else
            {
                // Term Mode
                accountTerms.AddRange(_adRepository.Get(studentId).AccountTerms);
            }

            return accountTerms;
        }
    }
}
