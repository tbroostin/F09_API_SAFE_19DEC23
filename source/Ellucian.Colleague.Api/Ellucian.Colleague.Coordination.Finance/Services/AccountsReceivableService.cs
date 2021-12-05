// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Domain.Base.Repositories;


namespace Ellucian.Colleague.Coordination.Finance.Services
{
    /// <summary>
    /// Provider for accounts receivable coordination services
    /// </summary>
    [RegisterType]
    public class AccountsReceivableService : FinanceCoordinationService, IAccountsReceivableService
    {
        private IAccountsReceivableRepository _arRepository;
        private readonly IFinanceConfigurationRepository _configRepository;
        private Domain.Finance.Entities.DueDateOverrides _dueDateOverrides;
        private ITermRepository _termRepository;

        /// <summary>
        /// Constructor for the accounts receivable coordination service
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="arRepository">Interface to AccountsReceivableRepository</param>
        /// <param name="configRepository">Interface to FinanceConfigurationRepository</param>
        /// <param name="termRepository">Interface to TermRepository</param>
        /// <param name="currentUserFactory">Interface to CurrentUserFactory</param>
        /// <param name="roleRepository">Interface to RoleRepository</param>
        /// <param name="logger">Interface to Logger</param>
        /// <param name="staffRepository">Interface to Staff Repository</param>
        public AccountsReceivableService(IAdapterRegistry adapterRegistry, IAccountsReceivableRepository arRepository, IFinanceConfigurationRepository configRepository,
            ITermRepository termRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository)
        {
            _arRepository = arRepository;
            _configRepository = configRepository;
            _dueDateOverrides = _configRepository.GetDueDateOverrides();
            _termRepository = termRepository;
        }

        /// <summary>
        /// Get all the defined receivable types
        /// </summary>
        /// <returns>List of ReceivableType DTOs</returns>
        public IEnumerable<Dtos.Finance.ReceivableType> GetReceivableTypes()
        {
            var entityCollection = _arRepository.ReceivableTypes;
            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ReceivableType, Dtos.Finance.ReceivableType>();

            var dtoCollection = new List<Dtos.Finance.ReceivableType>();
            foreach (var entity in entityCollection)
            {
                dtoCollection.Add(adapter.MapToType(entity));
            }

            return dtoCollection;
        }

        /// <summary>
        /// Get all the defined deposit types
        /// </summary>
        /// <returns>List of DepositType DTOs</returns>
        public IEnumerable<Dtos.Finance.DepositType> GetDepositTypes()
        {
            var entityCollection = _arRepository.DepositTypes;
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.DepositType, Dtos.Finance.DepositType>();

            var dtoCollection = new List<Dtos.Finance.DepositType>();
            foreach (var entity in entityCollection)
            {
                dtoCollection.Add(entityToDtoAdapter.MapToType(entity));
            }

            return dtoCollection;
        }

        /// <summary>
        /// Get a specified accountholder
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>The AccountHolder DTO</returns>
        public Dtos.Finance.AccountHolder GetAccountHolder(string id)
        {
            CheckAccountPermission(id);

            var accountHolderEntity = Task.Run(async() => await _arRepository.GetAccountHolderAsync(id)).GetAwaiter().GetResult();
            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.AccountHolder, Dtos.Finance.AccountHolder>();
            var accountHolderDto = adapter.MapToType(accountHolderEntity);

            return accountHolderDto;
        }

        /// <summary>
        /// Get a specified, privacy-restricted accountholder
        /// </summary>
        /// <param name="id">Accountholder ID</param>
        /// <returns>The AccountHolder DTO</returns>
        public async Task<PrivacyWrapper<Dtos.Finance.AccountHolder>> GetAccountHolder2Async(string id, bool bypassCache)
        {
            CheckAccountPermission(id);

            var accountHolderEntity = await _arRepository.GetAccountHolderAsync(id, bypassCache);
            var privacyWrapperWithList = BuildAccountHolderDtos(new List<AccountHolder>() { accountHolderEntity });
            var accountHolderDto = privacyWrapperWithList.Dto.FirstOrDefault();
            var privacyWrapper = new PrivacyWrapper<Dtos.Finance.AccountHolder>(accountHolderDto, privacyWrapperWithList.HasPrivacyRestrictions);
            return privacyWrapper;
        }

        /// <summary>
        /// Retrieves the information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public async Task<IEnumerable<Dtos.Finance.AccountHolder>> SearchAccountHoldersAsync(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("criteria");
            }

            if (!HasPermission(FinancePermissionCodes.ViewStudentAccountActivity))
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }

            List<AccountHolder> entities;
            try
            {
                entities = (await _arRepository.SearchAccountHoldersByKeywordAsync(criteria)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw new ArgumentException("Error searching for accountholders");
            }

            List<Dtos.Finance.AccountHolder> dtos = new List<Dtos.Finance.AccountHolder>();
            if (entities.Any())
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.AccountHolder, Dtos.Finance.AccountHolder>();
                dtos.AddRange(entities.Select(e => adapter.MapToType(e)));
            }
            return dtos;
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">a query keyword such as a Person ID or a first and last name.  
        /// A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.16. Use SearchAccountHolders3Async.")]
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>>> SearchAccountHoldersAsync2(string criteria)
        {
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("criteria");
            }
            if (!HasPermission(FinancePermissionCodes.ViewStudentAccountActivity))
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }

            List<AccountHolder> entities;
            try
            {
                entities = (await _arRepository.SearchAccountHoldersByKeywordAsync(criteria)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
                throw new ArgumentException("Error searching accountholders by keyword ");
            }
          
            return BuildAccountHolderDtos(entities);
        }

        /// <summary>
        /// Retrieves the privacy-restricted information for a single accountholder if an id is provided,
        /// or the matching accountholders if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">either a list of person ids or a query keyword such as a Person ID 
        /// or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="AccountHolder">AccountHolder</see> information</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public async Task<PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>>> SearchAccountHolders3Async(Dtos.Finance.AccountHolderQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if ((criteria.Ids == null || !criteria.Ids.Any())
                && string.IsNullOrEmpty(criteria.QueryKeyword))
            {
                throw new ArgumentException("either a list of person ids or a query keyword must be present");
            }

            if (!HasPermission(FinancePermissionCodes.ViewStudentAccountActivity))
            {
                logger.Error(CurrentUser.PersonId + " does not have permission code " + FinancePermissionCodes.ViewStudentAccountActivity);
                throw new PermissionsException();
            }

            List<AccountHolder> entities;
            if (criteria.Ids != null && criteria.Ids.Any())
            {
                try
                {
                    entities = (await _arRepository.SearchAccountHoldersByIdsAsync(criteria.Ids)).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    logger.Error(ex.StackTrace);
                    throw new ArgumentException("Error searching accountholders with specified ids");
                }
            }
            else
            {
                try
                {
                    entities = (await _arRepository.SearchAccountHoldersByKeywordAsync(criteria.QueryKeyword)).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    logger.Error(ex.StackTrace);
                    throw new ArgumentException("Error searching accountholders by keyword");
                }
            }
            return BuildAccountHolderDtos(entities);
        }

        /// <summary>
        /// Get a single invoice by its ID
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>Invoice DTO</returns>
        public Dtos.Finance.Invoice GetInvoice(string invoiceId)
        {
            var invoiceEntity = _arRepository.GetInvoice(invoiceId);
            CheckAccountPermission(invoiceEntity.PersonId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Invoice, Dtos.Finance.Invoice>();
            Dtos.Finance.Invoice invoiceDto = adapter.MapToType(invoiceEntity);

            OverrideDueDate(invoiceDto);

            return invoiceDto;
        }

        /// <summary>
        /// Get a group of invoices from a list of IDs
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of Invoice DTOs</returns>
        [Obsolete("Obsolete as of API version 1.12, use QueryInvoicesAsync instead.")]
        public IEnumerable<Dtos.Finance.Invoice> GetInvoices(IEnumerable<string> invoiceIds)
        {
            var invoiceDtos = new List<Dtos.Finance.Invoice>();
            if (invoiceIds == null || invoiceIds.Count() == 0)
            {
                return invoiceDtos;
            }

            var invoiceEntities = _arRepository.GetInvoices(invoiceIds);
            if (invoiceEntities.Any())
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Invoice, Dtos.Finance.Invoice>();
                foreach (var invoice in invoiceEntities)
                {
                    CheckAccountPermission(invoice.PersonId);
                    Dtos.Finance.Invoice invoiceDto = adapter.MapToType(invoice);
                    OverrideDueDate(invoiceDto);
                    invoiceDtos.Add(invoiceDto);
                }
            }

            return invoiceDtos;
        }

        /// <summary>
        /// Get a group of invoices based on a list of invoice IDs
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs - if not provided method will return an empty list.</param>
        /// <returns>List of Invoice DTOs</returns>
        public async Task<IEnumerable<Dtos.Finance.Invoice>> QueryInvoicesAsync(IEnumerable<string> invoiceIds)
        {
            var invoiceDtos = new List<Dtos.Finance.Invoice>();
            // Using same business logic as originally written in obsolete method GetInvoices
            if (invoiceIds == null || !invoiceIds.Any())
            {
                return invoiceDtos;
            }

            // Get InvoicePayments from repository. This is the parent class of Invoice.  By saying includePaymentAmount = false it will 
            // not do that intensive calculation and the repository will return zeros for the amounts
            var invoiceEntities = await _arRepository.QueryInvoicePaymentsAsync(invoiceIds, InvoiceDataSubset.InvoiceOnly);
            if (invoiceEntities.Any())
            {
                // Convert the InvoicePayment entities to Invoice Dto (which will drop the AmountPaid property)
                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Invoice, Dtos.Finance.Invoice>();
                foreach (var invoice in invoiceEntities)
                {
                    CheckAccountPermission(invoice.PersonId);
                    Dtos.Finance.Invoice invoiceDto = adapter.MapToType(invoice);

                    OverrideDueDate(invoiceDto);

                    invoiceDtos.Add(invoiceDto);
                }
            }

            return invoiceDtos;
        }

        /// <summary>
        /// Get a group of invoice payments from a list of invoice IDs
        /// </summary>
        /// <param name="invoiceIds">List of invoice IDs</param>
        /// <returns>List of InvoicePayment DTOs</returns>
        public async Task<IEnumerable<Dtos.Finance.InvoicePayment>> QueryInvoicePaymentsAsync(IEnumerable<string> invoiceIds)
        {
            var invoicePaymentDtos = new List<Dtos.Finance.InvoicePayment>();
            // Using same business logic as originally written in obsolete method GetInvoices
            if (invoiceIds == null || !invoiceIds.Any())
            {
                return invoicePaymentDtos;
            }

            var invoicePaymentEntities = await _arRepository.QueryInvoicePaymentsAsync(invoiceIds, InvoiceDataSubset.InvoicePayment);
            if (invoicePaymentEntities.Any())
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.InvoicePayment, Dtos.Finance.InvoicePayment>();
                foreach (var invoicePayment in invoicePaymentEntities)
                {
                    CheckAccountPermission(invoicePayment.PersonId);
                    Dtos.Finance.InvoicePayment invoicePaymentDto = adapter.MapToType(invoicePayment);

                    OverrideDueDate(invoicePaymentDto);

                    invoicePaymentDtos.Add(invoicePaymentDto);
                }
            }

            return invoicePaymentDtos;
        }

        /// <summary>
        /// Get a single payment by its ID
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>Receivable Payment DTO</returns>
        public Dtos.Finance.ReceivablePayment GetPayment(string paymentId)
        {
            var paymentEntity = _arRepository.GetPayment(paymentId);
            CheckAccountPermission(paymentEntity.PersonId);

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ReceivablePayment, Dtos.Finance.ReceivablePayment>();
            Dtos.Finance.ReceivablePayment paymentDto = adapter.MapToType(paymentEntity);

            return paymentDto;
        }

        /// <summary>
        /// Get a group of payments from a list of IDs
        /// </summary>
        /// <param name="paymentIds">List of Payment IDs</param>
        /// <returns>List of Receivable Payment DTOs</returns>
        public IEnumerable<Dtos.Finance.ReceivablePayment> GetPayments(IEnumerable<string> paymentIds)
        {
            var paymentDtos = new List<Dtos.Finance.ReceivablePayment>();
            if (paymentIds == null || paymentIds.Count() == 0)
            {
                return paymentDtos;
            }

            var paymentEntities = _arRepository.GetPayments(paymentIds);
            if (paymentEntities.Count() > 0)
            {
                string id = paymentEntities.First().PersonId;
                CheckAccountPermission(id);

                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ReceivablePayment, Dtos.Finance.ReceivablePayment>();
                foreach (var payment in paymentEntities)
                {
                    Dtos.Finance.ReceivablePayment paymentDto = adapter.MapToType(payment);

                    paymentDtos.Add(paymentDto);
                }
            }

            return paymentDtos;
        }

        /// <summary>
        /// Get the deposits due for a person
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>List of DepositDue DTOs</returns>
        public IEnumerable<Dtos.Finance.DepositDue> GetDepositsDue(string id)
        {
            CheckAccountPermission(id);

            var depositsDueEntity = _arRepository.GetDepositsDue(id);
            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.DepositDue, Dtos.Finance.DepositDue>();

            List<Dtos.Finance.DepositDue> depositsDueDto = new List<Dtos.Finance.DepositDue>();
            foreach (var item in depositsDueEntity)
            {
                depositsDueDto.Add(adapter.MapToType(item));
            }

            return depositsDueDto;
        }

        /// <summary>
        /// Get the payment distributions for a student, account types, and payment process
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="accountTypes">List of account type codes</param>
        /// <param name="paymentProcess">Payment process code</param>
        /// <returns>List of payment distributions</returns>
        public IEnumerable<string> GetDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess)
        {
            CheckAccountPermission(studentId);

            return _arRepository.GetDistributions(studentId, accountTypes, paymentProcess);
        }


        /// <summary>
        /// Create a receivable invoice
        /// </summary>
        /// <param name="source">Invoice to create</param>
        /// <returns>DTO of the created invoice</returns>
        public Dtos.Finance.ReceivableInvoice PostReceivableInvoice(Dtos.Finance.ReceivableInvoice source)
        {
            // User must have proper permissions
            CheckCreateArInvoicePermission();

            // create and validate entity
            var adapter = _adapterRegistry.GetAdapter<Dtos.Finance.ReceivableInvoice,
                Domain.Finance.Entities.ReceivableInvoice>();
            var entity = adapter.MapToType(source);
            ValidateReceivableInvoice(entity);

            var invoice = _arRepository.PostReceivableInvoice(entity);
            var invoiceAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ReceivableInvoice,
                Dtos.Finance.ReceivableInvoice>();
            var outputDto = invoiceAdapter.MapToType(invoice);

            return outputDto;
        }

        /// <summary>
        /// Retrieve a deposit
        /// </summary>
        /// <param name="depositId">Identifier of the deposit to retrieve</param>
        /// <returns>The desired deposit</returns>
        public Dtos.Finance.Deposit GetDeposit(string depositId)
        {
            var newDepositEntity = _arRepository.GetDeposit(depositId);

            // return the DTO version of the newly created deposit
            var adapter = new AutoMapperAdapter<Domain.Finance.Entities.Deposit, Dtos.Finance.Deposit>(_adapterRegistry, logger);
            return adapter.MapToType(newDepositEntity);

        }

        /// <summary>
        /// Get a list of deposits
        /// </summary>
        /// <param name="ids">The list of ids of the deposits to retrieve</param>
        /// <returns>The list of retrieved deposits</returns>
        public IEnumerable<Dtos.Finance.Deposit> GetDeposits(IEnumerable<string> depositIds)
        {
            var newDepositEntities = _arRepository.GetDeposits(depositIds);
            var adapter = new AutoMapperAdapter<Domain.Finance.Entities.Deposit, Dtos.Finance.Deposit>(_adapterRegistry, logger);
            return newDepositEntities.Select(x => adapter.MapToType(x));
        }

        /// <summary>
        /// Get all charge codes
        /// </summary>
        /// <returns>List of charge codes</returns>
        public async Task<IEnumerable<Dtos.Finance.ChargeCode>> GetChargeCodesAsync()
        {
            var entityCollection = await _arRepository.GetChargeCodesAsync();
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ChargeCode, Dtos.Finance.ChargeCode>();

            var dtoCollection = new List<Dtos.Finance.ChargeCode>();
            foreach (var entity in entityCollection)
            {
                dtoCollection.Add(entityToDtoAdapter.MapToType(entity));
            }

            return dtoCollection;
        }



        #region Private helper methods

        private void OverrideDueDate(Dtos.Finance.Invoice invoice)
        {
            Domain.Finance.Entities.Configuration.FinanceConfiguration finConfig = _configRepository.GetFinanceConfiguration();
            // Term Mode
            if (finConfig.PaymentDisplay == Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByTerm)
            {
                if (string.IsNullOrEmpty(invoice.TermId))
                {
                    if (_dueDateOverrides.NonTermOverride != null)
                    {
                        invoice.DueDate = _dueDateOverrides.NonTermOverride.Value;
                    }
                }
                else
                {
                    DateTime termOverrideDate;
                    if (_dueDateOverrides.TermOverrides.TryGetValue(invoice.TermId, out termOverrideDate))
                    {
                        invoice.DueDate = termOverrideDate;
                    }
                }
            }
            // PCF Mode
            else
            {
                if (!string.IsNullOrEmpty(invoice.TermId))
                {
                    PeriodType? period = GetTermPeriod(invoice.TermId);
                    switch (period)
                    {
                        case PeriodType.Past:
                            if (_dueDateOverrides.PastPeriodOverride != null)
                            {
                                invoice.DueDate = _dueDateOverrides.PastPeriodOverride.Value;
                            }
                            break;
                        case PeriodType.Current:
                            if (_dueDateOverrides.CurrentPeriodOverride != null)
                            {
                                invoice.DueDate = _dueDateOverrides.CurrentPeriodOverride.Value;
                            }
                            break;
                        case PeriodType.Future:
                            if (_dueDateOverrides.FuturePeriodOverride != null)
                            {
                                invoice.DueDate = _dueDateOverrides.FuturePeriodOverride.Value;
                            }
                            break;
                        default:
                                break;
                    }
                }
            }
        }

        private PeriodType? GetTermPeriod(string termId)
        {
            PeriodType? period = null;
            var term = _termRepository.Get(termId);
            if (term != null)
            {
                period = term.FinancialPeriod;
            }
            return period;
        }

        private void ValidateReceivableInvoice(Domain.Finance.Entities.ReceivableInvoice entity)
        {
            ReceivableService.ValidateInvoice(entity,
                _arRepository.ReceivableTypes.ToList<Domain.Finance.Entities.ReceivableType>(),
                _arRepository.ChargeCodes.ToList<Domain.Finance.Entities.ChargeCode>(),
                _arRepository.InvoiceTypes.ToList<Domain.Finance.Entities.InvoiceType>(),
                _termRepository.Get().ToList<Term>());
        }

        // Create Advisor Dto to return
        private PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>> BuildAccountHolderDtos(IEnumerable<Domain.Finance.Entities.AccountHolder> accountHolderEntities)
        {
            var hasPrivacyRestriction = false; // Default to false, check later for each account holder

            List<Dtos.Finance.AccountHolder> accountHolderDtos = new List<Dtos.Finance.AccountHolder>();
            var adviseeAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountHolder, Dtos.Finance.AccountHolder>();
            // loop through each account holder
            foreach (var accountHolder in accountHolderEntities)
            {
                // Before doing anything, check the current user's privacy code settings (on their staff record)
                // against any privacy code on the account holder's record
                var accountHolderHasPrivacyRestriction = (string.IsNullOrEmpty(accountHolder.PrivacyStatusCode) || CurrentUser.IsPerson(accountHolder.Id) || HasProxyAccessForPerson(accountHolder.Id)) ? 
                    false : 
                    !HasPrivacyCodeAccess(accountHolder.PrivacyStatusCode);

                Dtos.Finance.AccountHolder accountHolderDto;

                // If a privacy restriction exists (staff record doesn't contain student's privacy code)
                // then blank out the record, except for name, id, and privacy code
                if (accountHolderHasPrivacyRestriction)
                {
                    hasPrivacyRestriction = true;
                    accountHolderDto = new Dtos.Finance.AccountHolder()
                    {
                        LastName = accountHolder.LastName,
                        FirstName = accountHolder.FirstName,
                        MiddleName = accountHolder.MiddleName,
                        PreferredName = accountHolder.PreferredName,
                        Id = accountHolder.Id,
                        PrivacyStatusCode = accountHolder.PrivacyStatusCode
                    };
                }
                else
                {
                    accountHolderDto = adviseeAdapter.MapToType(accountHolder);
                }
                accountHolderDtos.Add(accountHolderDto);
            }
            return new PrivacyWrapper<IEnumerable<Dtos.Finance.AccountHolder>>(accountHolderDtos, hasPrivacyRestriction);
        }


        #endregion
    }
}
