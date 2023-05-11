//Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class AccountsPayableInvoicesService : BaseCoordinationService, IAccountsPayableInvoicesService
    {
        private readonly IColleagueFinanceReferenceDataRepository _colleagueFinanceReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAccountsPayableInvoicesRepository _accountsPayableInvoicesRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IVendorsRepository _vendorsRepository;
        private readonly IGeneralLedgerConfigurationRepository _generalLedgerConfigurationRepository;
        private readonly IPersonRepository _personRepository;      
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAccountFundsAvailableRepository _accountFundAvailableRepository;
        IDictionary<string, string> _projectReferenceIds = null;   

        public AccountsPayableInvoicesService(

            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IAccountsPayableInvoicesRepository accountsPayableInvoicesRepository,
            IAddressRepository addressRepository,
            IVendorsRepository vendorsRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IAccountFundsAvailableRepository accountFundAvailableRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _vendorsRepository = vendorsRepository;
            _colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            _addressRepository = addressRepository;
            _accountsPayableInvoicesRepository = accountsPayableInvoicesRepository;
            _referenceDataRepository = referenceDataRepository;
            _generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            _personRepository = personRepository;
            _accountFundAvailableRepository = accountFundAvailableRepository;           
            _configurationRepository = configurationRepository;
        }

        private IEnumerable<Domain.Base.Entities.CommerceTaxCode> _commerceTaxCodes;
        private IEnumerable<Domain.Base.Entities.CommerceTaxCodeRate> _commerceTaxCodeRates;
        private IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> _accountsPayableSources;
        private IEnumerable<BoxCodes> _boxCodes;
        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> _commodityCodes;
        private IEnumerable<Domain.ColleagueFinance.Entities.FxaTransferFlags> _fxaTransferFlags = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> _commodityUnitTypes;
        private IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> _vendorTerms;
        private Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure _glAccountStructure;
        private string _hostCountry;

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<Domain.Base.Entities.State> _states = null;
        private async Task<IEnumerable<Domain.Base.Entities.State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }
        
        #region PUT/POST EEDM version 11

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Put (Update) an Accounts Payable Invoices domain entity
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="accountsPayableInvoicesDto"><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></param>
        /// <returns><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        public async Task<AccountsPayableInvoices2> PutAccountsPayableInvoices2Async(string guid, Dtos.AccountsPayableInvoices2 accountsPayableInvoicesDto)
        {
            if (accountsPayableInvoicesDto == null)
                throw new ArgumentNullException("accountsPayableInvoices", "Must provide a accountsPayableInvoices for update");
            if (string.IsNullOrEmpty(accountsPayableInvoicesDto.Id))
                throw new ArgumentNullException("accountsPayableInvoices", "Must provide a guid for accountsPayableInvoices update");

            ValidateAccountsPayableInvoices2(accountsPayableInvoicesDto);

            if (accountsPayableInvoicesDto.Payment != null && accountsPayableInvoicesDto.Payment.Source == null)
            {
                throw new ArgumentNullException("accountsPayableInvoices.Payment.Source", "The payment source cannot be unset.");
            }

            // get the person ID associated with the incoming guid
            var accountsPayableInvoicesId = string.Empty;
            try
            {
                accountsPayableInvoicesId = await _accountsPayableInvoicesRepository.GetAccountsPayableInvoicesIdFromGuidAsync(accountsPayableInvoicesDto.Id);
            }
            //if the guid does not exist, the method above will throw KeyNotFoundException, which we are going to ignore but if they sent GUID belongs to different entity then
            // it throws repo exception that we want to catch and throw. 
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, "Unable to get accounts payable invoice by guid");
            }


            //Used to figure out if there are funds that exceed the budget.
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(accountsPayableInvoicesId))
            {
                try
                {
                    if ((!string.IsNullOrEmpty(accountsPayableInvoicesDto.InvoiceNumber))
                    && (!accountsPayableInvoicesDto.InvoiceNumber.Equals(accountsPayableInvoicesId, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ColleagueWebApiException(string.Format("InvoiceNumber does not match voucher ID associated with GUID {0}.", guid));
                    }
                    _accountsPayableInvoicesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    

                    var glConfiguration = await GetGeneralLedgerAccountStructure();
                    if (glConfiguration == null)
                    {
                        throw new ArgumentNullException("GL Account Structure is not configured.");
                    }

                    if ((accountsPayableInvoicesDto.LineItems) != null && (accountsPayableInvoicesDto.LineItems.Any()))
                    {
                        var projectRefNos = accountsPayableInvoicesDto.LineItems
                       .Where(i => i.AccountDetails != null)
                       .SelectMany(p => p.AccountDetails)
                       .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                       .Select(pj => pj.AccountingString.Split('*')[1])
                       .ToList()
                       .Distinct();

                        if (projectRefNos != null && projectRefNos.Any())
                        {
                            _projectReferenceIds = await _accountsPayableInvoicesRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                        }
                    }
                    // map the DTO to entities
                    var accountsPayableInvoicesEntity
                    = await ConvertAccountsPayableInvoices2DtoToEntityAsync(accountsPayableInvoicesId, accountsPayableInvoicesDto, glConfiguration.MajorComponents.Count());

                    //Fund checking.
                    overrideAvailable = await checkFunds2(accountsPayableInvoicesDto);

                    // update the entity in the database
                    var updatedAccountsPayableInvoicesEntity =
                        await _accountsPayableInvoicesRepository.UpdateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);
                    // get needed guids
                    Dictionary<string, string> vendorGuidCollection = null;
                    Dictionary<string, string> poGuidCollection = null;
                    Dictionary<string, string> bpoGuidCollection = null;
                    Dictionary<string, string> addressGuidCollection = null;
                    Dictionary<string, string> personGuidCollection = null;
                    var personIds = new List<string>();
                    if (!string.IsNullOrEmpty(updatedAccountsPayableInvoicesEntity.SubmittedBy))
                        personIds.Add(updatedAccountsPayableInvoicesEntity.SubmittedBy);
                    if (!string.IsNullOrEmpty(updatedAccountsPayableInvoicesEntity.VoucherRequestor))
                        personIds.Add(updatedAccountsPayableInvoicesEntity.VoucherRequestor);
                    personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                    if (!string.IsNullOrEmpty(updatedAccountsPayableInvoicesEntity.VendorId))
                        vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(new List<string>() { updatedAccountsPayableInvoicesEntity.VendorId });                    
                    if (!string.IsNullOrEmpty(updatedAccountsPayableInvoicesEntity.VendorAddressId))
                        addressGuidCollection = await _personRepository.GetAddressGuidsCollectionAsync(new List<string>() { updatedAccountsPayableInvoicesEntity.VendorAddressId });
                    if (!string.IsNullOrEmpty(updatedAccountsPayableInvoicesEntity.BlanketPurchaseOrderId))
                        bpoGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(new List<string>() { updatedAccountsPayableInvoicesEntity.BlanketPurchaseOrderId }, "BPO");
                    if (updatedAccountsPayableInvoicesEntity.LineItems != null && updatedAccountsPayableInvoicesEntity.LineItems.Any())
                    {
                        var PoIds = updatedAccountsPayableInvoicesEntity.LineItems.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                        poGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");
                    }                       
                    
                    // convert the entity to a DTO
                    var dtoAccountsPayableInvoice = await ConvertAccountsPayableInvoicesEntityToDto2Async(updatedAccountsPayableInvoicesEntity, glConfiguration, vendorGuidCollection, poGuidCollection, bpoGuidCollection, addressGuidCollection, personGuidCollection,true);
                                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

                    //populate our response if we had a submitted by and if there was items Overridden when a Budget was exceeded.
                    if (accountsPayableInvoicesDto.SubmittedBy != null && !string.IsNullOrWhiteSpace(accountsPayableInvoicesDto.SubmittedBy.Id))
                        dtoAccountsPayableInvoice.SubmittedBy = accountsPayableInvoicesDto.SubmittedBy;

                    if (dtoAccountsPayableInvoice.LineItems != null && dtoAccountsPayableInvoice.LineItems.Any() && overrideAvailable != null && overrideAvailable.Any())
                    {
                        int lineCount = 0;
                        foreach (var lineItem in dtoAccountsPayableInvoice.LineItems)
                        {
                            int detailCount = 0;
                            lineCount++;
                            foreach (var detail in lineItem.AccountDetails)
                            {
                                detailCount++;
                                string VouID = lineCount.ToString() + "." + detailCount.ToString();
                                var findOvr = overrideAvailable.FirstOrDefault(a => a.Sequence == VouID || a.Sequence == VouID + ".DS");
                                if (findOvr != null)
                                {
                                    if (!string.IsNullOrEmpty(findOvr.SubmittedBy) && findOvr.Sequence == VouID + ".DS")
                                    {
                                        var submittedByGuid = await _personRepository.GetPersonGuidFromIdAsync(findOvr.SubmittedBy);
                                        if (string.IsNullOrEmpty(submittedByGuid))
                                        {
                                            throw new ColleagueWebApiException(string.Concat("Process finished but we couldn't return a Submitted By GUID purchase order: ", dtoAccountsPayableInvoice.Id, " Submitted By: ", findOvr.SubmittedBy));
                                        }
                                        detail.SubmittedBy = new GuidObject2(submittedByGuid);
                                    }
                                    if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                        detail.BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
                                }
                            }
                        }
                    }
                    // return the newly updated DTO
                    return dtoAccountsPayableInvoice;

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (ArgumentNullException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (IntegrationApiException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiException(ex.Message, ex.InnerException);
                }
            }
            // perform a create instead
            return await PostAccountsPayableInvoices2Async(accountsPayableInvoicesDto);
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Post (Create) an Accounts Payable Invoices doamin entity
        /// </summary>
        /// <param name="accountsPayableInvoicesDto"><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></param>
        /// <returns><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        public async Task<AccountsPayableInvoices2> PostAccountsPayableInvoices2Async(Dtos.AccountsPayableInvoices2 accountsPayableInvoicesDto)
        {
            if (accountsPayableInvoicesDto == null)
                throw new ArgumentNullException("accountsPayableInvoices", "Must provide a accountsPayableInvoices for update");
            if (string.IsNullOrEmpty(accountsPayableInvoicesDto.Id))
                throw new ArgumentNullException("accountsPayableInvoices", "Must provide a guid for accountsPayableInvoices update");

            if (accountsPayableInvoicesDto.VoidDate != null)
            {
                throw new ArgumentNullException("accountsPayableInvoices.VoidDate", "Cannot have a void date on a POST");
            }

            ValidateAccountsPayableInvoices2(accountsPayableInvoicesDto);

            Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableInvoices createdAccountsPayableInvoices = null;

            //Used to figure out if there are funds that exceed the budget.
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

            _accountsPayableInvoicesRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                var glConfiguration = await GetGeneralLedgerAccountStructure();
                if (glConfiguration == null)
                {
                    throw new ArgumentNullException("GL Account Structure is not configured.");
                }

                if ((accountsPayableInvoicesDto.LineItems) != null && (accountsPayableInvoicesDto.LineItems.Any()))
                {
                    var projectRefNos = accountsPayableInvoicesDto.LineItems
                   .Where(i => i.AccountDetails != null)
                   .SelectMany(p => p.AccountDetails)
                   .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                   .Select(pj => pj.AccountingString.Split('*')[1])
                   .ToList()
                   .Distinct();

                    if (projectRefNos != null && projectRefNos.Any())
                    {
                        _projectReferenceIds = await _accountsPayableInvoicesRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                    }
                }
                var accountsPayableInvoicesEntity = await ConvertAccountsPayableInvoices2DtoToEntityAsync(accountsPayableInvoicesDto.Id, accountsPayableInvoicesDto, glConfiguration.MajorComponents.Count);
                //Fund checking.
                overrideAvailable = await checkFunds2(accountsPayableInvoicesDto);
                // create a AccountsPayableInvoices entity in the database
                createdAccountsPayableInvoices =
                    await _accountsPayableInvoicesRepository.CreateAccountsPayableInvoicesAsync(accountsPayableInvoicesEntity);
                //get needed guid collection
                Dictionary<string, string> vendorGuidCollection = null;
                Dictionary<string, string> poGuidCollection = null;
                Dictionary<string, string> bpoGuidCollection = null;
                Dictionary<string, string> addressGuidCollection = null;
                Dictionary<string, string> personGuidCollection = null;
                var personIds = new List<string>();
                if (!string.IsNullOrEmpty(createdAccountsPayableInvoices.SubmittedBy))
                    personIds.Add(createdAccountsPayableInvoices.SubmittedBy);
                if (!string.IsNullOrEmpty(createdAccountsPayableInvoices.VoucherRequestor))
                    personIds.Add(createdAccountsPayableInvoices.VoucherRequestor);
                personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                if (!string.IsNullOrEmpty(createdAccountsPayableInvoices.VendorId))
                    vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(new List<string>() { createdAccountsPayableInvoices.VendorId });
                if (!string.IsNullOrEmpty(createdAccountsPayableInvoices.VendorAddressId))
                    addressGuidCollection = await _personRepository.GetAddressGuidsCollectionAsync(new List<string>() { createdAccountsPayableInvoices.VendorAddressId });
                if (!string.IsNullOrEmpty(createdAccountsPayableInvoices.BlanketPurchaseOrderId))
                    bpoGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(new List<string>() { createdAccountsPayableInvoices.BlanketPurchaseOrderId }, "BPO");
                if (createdAccountsPayableInvoices.LineItems != null && createdAccountsPayableInvoices.LineItems.Any())
                {
                    var PoIds = createdAccountsPayableInvoices.LineItems.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                    poGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");                    
                }
                var dtoAccountsPayableInvoice = await ConvertAccountsPayableInvoicesEntityToDto2Async(createdAccountsPayableInvoices, glConfiguration, vendorGuidCollection, poGuidCollection, bpoGuidCollection, addressGuidCollection, personGuidCollection, true);
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                //populate our response if we had a submitted by and if there was items Overridden when a Budget was exceeded.
                if (accountsPayableInvoicesDto.SubmittedBy != null && !string.IsNullOrWhiteSpace(accountsPayableInvoicesDto.SubmittedBy.Id))
                    dtoAccountsPayableInvoice.SubmittedBy = accountsPayableInvoicesDto.SubmittedBy;

                if (dtoAccountsPayableInvoice.LineItems != null && dtoAccountsPayableInvoice.LineItems.Any() && overrideAvailable != null && overrideAvailable.Any())
                {
                    int lineCount = 0;
                    foreach (var lineItem in dtoAccountsPayableInvoice.LineItems)
                    {
                        int detailCount = 0;
                        lineCount++;
                        foreach (var detail in lineItem.AccountDetails)
                        {
                            detailCount++;
                            string VouID = lineCount.ToString() + "." + detailCount.ToString();
                            var findOvr = overrideAvailable.FirstOrDefault(a => a.Sequence == VouID || a.Sequence == VouID + ".DS");
                            if (findOvr != null)
                            {
                                if (!string.IsNullOrEmpty(findOvr.SubmittedBy) && findOvr.Sequence == VouID + ".DS")
                                {
                                    var submittedByGuid = await _personRepository.GetPersonGuidFromIdAsync(findOvr.SubmittedBy);
                                    if (string.IsNullOrEmpty(submittedByGuid))
                                    {
                                        throw new ColleagueWebApiException(string.Concat("Process finished but we couldn't return a Submitted By GUID purchase order: ", dtoAccountsPayableInvoice.Id, " Submitted By: ", findOvr.SubmittedBy));
                                    }
                                    detail.SubmittedBy = new GuidObject2(submittedByGuid);
                                }
                                if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                    detail.BudgetCheck = AccountsPayableInvoicesAccountBudgetCheck.Override;
                            }
                        }
                    }
                }
                // return the newly created AccountsPayableInvoices
                return dtoAccountsPayableInvoice;
            }

            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }
        }

        #endregion

        #region GET EEDM Version 11

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get AccountsPayableInvoices data.
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>List of <see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.AccountsPayableInvoices2>, int>> GetAccountsPayableInvoices2Async(int offset, int limit, Dtos.AccountsPayableInvoices2 criteriaFilter, bool bypassCache = false)
        {
            var invoiceNumber = string.Empty;
            if (criteriaFilter != null)
            {
                invoiceNumber = criteriaFilter.InvoiceNumber;
            }

            var accountsPayableInvoicesCollection = new List<Ellucian.Colleague.Dtos.AccountsPayableInvoices2>();
            Tuple<IEnumerable<AccountsPayableInvoices>, int> accountsPayableInvoicesEntities = null;
            try
            {
                accountsPayableInvoicesEntities = await _accountsPayableInvoicesRepository.GetAccountsPayableInvoices2Async(offset, limit, invoiceNumber);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (accountsPayableInvoicesEntities == null)
            {
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountsPayableInvoices2>, int>(accountsPayableInvoicesCollection, 0);
            }
            var totalRecords = accountsPayableInvoicesEntities.Item2;
            if (accountsPayableInvoicesEntities.Item1 != null && accountsPayableInvoicesEntities.Item1.Any())
            {
                var projectIds = accountsPayableInvoicesEntities.Item1
                             .SelectMany(t => t.LineItems)
                             .Where(i => i.GlDistributions != null)
                             .SelectMany(p => p.GlDistributions)
                             .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                             .Select(pj => pj.ProjectId)
                             .ToList()
                             .Distinct();
                if (projectIds != null && projectIds.Any())
                {
                    _projectReferenceIds = await _accountsPayableInvoicesRepository.GetProjectReferenceIds(projectIds.ToArray());
                }
                if (accountsPayableInvoicesEntities.Item1 != null && accountsPayableInvoicesEntities.Item1.Any())
                {
                    GeneralLedgerAccountStructure glConfiguration = null;
                    try
                    {
                        glConfiguration = await GetGeneralLedgerAccountStructure();
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "Bad.Data", "", "");
                        throw IntegrationApiException;
                    }

                    //get needed guid collection
                    Dictionary<string, string> poGuidCollection = null;
                    var associatedLineItems = accountsPayableInvoicesEntities.Item1.SelectMany(x => x.LineItems);
                    var vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(accountsPayableInvoicesEntities.Item1.Where(x => (!string.IsNullOrEmpty(x.VendorId))).Select(x => x.VendorId).Distinct().ToArray());
                    var addressGuidCollection = await _personRepository.GetAddressGuidsCollectionAsync(accountsPayableInvoicesEntities.Item1.Where(x => (!string.IsNullOrEmpty(x.VendorAddressId))).Select(x => x.VendorAddressId).Distinct().ToArray());
                    var bpoGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(accountsPayableInvoicesEntities.Item1.Where(x => (!string.IsNullOrEmpty(x.BlanketPurchaseOrderId))).Select(x => x.BlanketPurchaseOrderId).Distinct().ToArray(),"BPO");
                    var personIds = new List<string>();
                    personIds.AddRange(accountsPayableInvoicesEntities.Item1.Where(x => (!string.IsNullOrEmpty(x.SubmittedBy))).Select(x => x.SubmittedBy).Distinct().ToList());
                    personIds.AddRange(accountsPayableInvoicesEntities.Item1.Where(x => (!string.IsNullOrEmpty(x.VoucherRequestor))).Select(x => x.VoucherRequestor).Distinct().ToList());
                    var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds); 
                    if (associatedLineItems != null && associatedLineItems.Any())
                    {
                        var PoIds = associatedLineItems.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                        poGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");
                    }

                    foreach (var accountsPayableInvoiceEntity in accountsPayableInvoicesEntities.Item1)
                    {
                        var accountsPayableInvoiceDto = await this.ConvertAccountsPayableInvoicesEntityToDto2Async(accountsPayableInvoiceEntity, glConfiguration, vendorGuidCollection, poGuidCollection, bpoGuidCollection, addressGuidCollection, personGuidCollection, bypassCache);
                        accountsPayableInvoicesCollection.Add(accountsPayableInvoiceDto);
                    }
                }
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }
            return new Tuple<IEnumerable<Dtos.AccountsPayableInvoices2>, int>(accountsPayableInvoicesCollection, totalRecords);

        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get AccountsPayableInvoices data from a Guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        public async Task<Dtos.AccountsPayableInvoices2> GetAccountsPayableInvoices2ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new IntegrationApiException("A GUID is required to obtain an Accounts Payable Invoice.");
            }
            AccountsPayableInvoices accountsPayableInvoice = null;
            AccountsPayableInvoices2 accountsPayableInvoiceDto = null;
            try
            {
                accountsPayableInvoice = await _accountsPayableInvoicesRepository.GetAccountsPayableInvoicesByGuidAsync(guid, false);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("No accounts-payable-invoices was found for GUID '" + guid + "'.");
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, "Bad.Data", guid, "");
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", guid, "");
                throw IntegrationApiException;
            }
            if (accountsPayableInvoice == null)
            {
                throw new KeyNotFoundException("No accounts-payable-invoices was found for GUID '" + guid + "'.");
            }
            else
            {
                GeneralLedgerAccountStructure glConfiguration = null;
                try
                {
                    glConfiguration = await GetGeneralLedgerAccountStructure();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data", guid, "");
                    throw IntegrationApiException;
                }
                try
                {
                    var projectIds = accountsPayableInvoice.LineItems
                               .Where(i => i.GlDistributions != null)
                               .SelectMany(p => p.GlDistributions)
                               .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                               .Select(pj => pj.ProjectId)
                               .ToList()
                               .Distinct();
                    if (projectIds != null && projectIds.Any())
                    {
                        _projectReferenceIds = await _accountsPayableInvoicesRepository.GetProjectReferenceIds(projectIds.ToArray());
                    }
                    //get needed guid collection
                    Dictionary<string, string> vendorGuidCollection = null;
                    Dictionary<string, string> poGuidCollection = null;
                    Dictionary<string, string> bpoGuidCollection = null;
                    Dictionary<string, string> addressGuidCollection = null;
                    Dictionary<string, string> personGuidCollection = null;
                    var personIds = new List<string>();
                    if (!string.IsNullOrEmpty(accountsPayableInvoice.SubmittedBy))
                        personIds.Add(accountsPayableInvoice.SubmittedBy);
                    if (!string.IsNullOrEmpty(accountsPayableInvoice.VoucherRequestor))
                        personIds.Add(accountsPayableInvoice.VoucherRequestor);
                    personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(personIds);
                    if (!string.IsNullOrEmpty(accountsPayableInvoice.VendorId))
                        vendorGuidCollection = await _vendorsRepository.GetVendorGuidsCollectionAsync(new List<string>() { accountsPayableInvoice.VendorId });
                    if (!string.IsNullOrEmpty(accountsPayableInvoice.VendorAddressId))
                        addressGuidCollection = await _personRepository.GetAddressGuidsCollectionAsync(new List<string>() { accountsPayableInvoice.VendorAddressId });
                    if (!string.IsNullOrEmpty(accountsPayableInvoice.BlanketPurchaseOrderId))
                        bpoGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(new List<string>() { accountsPayableInvoice.BlanketPurchaseOrderId }, "BPO");
                    if (accountsPayableInvoice.LineItems != null && accountsPayableInvoice.LineItems.Any())
                    {
                        var PoIds = accountsPayableInvoice.LineItems.Where(cv => !string.IsNullOrEmpty(cv.PurchaseOrderId)).Select(cd => cd.PurchaseOrderId).Distinct();
                        poGuidCollection = await _accountsPayableInvoicesRepository.GetGuidsCollectionAsync(PoIds.ToArray(), "PURCHASE.ORDERS");
                    }

                    accountsPayableInvoiceDto = await ConvertAccountsPayableInvoicesEntityToDto2Async(accountsPayableInvoice, glConfiguration, vendorGuidCollection, poGuidCollection, bpoGuidCollection, addressGuidCollection, personGuidCollection);
                }
                catch (KeyNotFoundException ex)
                {
                    throw new KeyNotFoundException("No accounts-payable-invoices was found for GUID '" + guid + "'.");
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("No accounts-payable-invoices was found for GUID '" + guid + "'.", ex);
                }
                catch (RepositoryException ex)
                {
                    throw new RepositoryException(ex.Message);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(ex.Message, ex);
                }
                catch (ApplicationException ae)
                {
                    throw new ApplicationException(ae.Message);
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiException("No accounts-payable-invoices was found for GUID '" + guid + "'.", ex);
                }
                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                return accountsPayableInvoiceDto;
            }
        }

        #endregion

        #region Convert EEDM version 11

        /// <summary>
        /// Convert AccountsPayableInvoices Dto To AccountsPayableInvoicesEntity
        /// </summary>
        /// <param name="accountsPayableInvoicesId">guid</param>
        /// <param name="accountsPayableInvoices"><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></param>
        /// <returns><see cref="Domain.ColleagueFinance.Entities.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        private async Task<Domain.ColleagueFinance.Entities.AccountsPayableInvoices> ConvertAccountsPayableInvoices2DtoToEntityAsync(string accountsPayableInvoicesId, Dtos.AccountsPayableInvoices2 accountsPayableInvoices, int GLCompCount)
        {
            string mainCurrency = string.Empty;
            var existingVendor = (accountsPayableInvoices.Vendor != null && accountsPayableInvoices.Vendor.ExistingVendor != null) ?
                accountsPayableInvoices.Vendor.ExistingVendor : null;
            var manualVendor = (accountsPayableInvoices.Vendor != null && accountsPayableInvoices.Vendor.ManualVendorDetails != null) ?
                accountsPayableInvoices.Vendor.ManualVendorDetails : null;
            var voucherStatus = VoucherStatus.InProgress;
            if (accountsPayableInvoices.ProcessState != null && accountsPayableInvoices.ProcessState != AccountsPayableInvoicesProcessState.NotSet)
            {
                switch (accountsPayableInvoices.ProcessState)
                {
                    case AccountsPayableInvoicesProcessState.Inprogress:
                        voucherStatus = VoucherStatus.InProgress; //"U";
                        break;
                    case AccountsPayableInvoicesProcessState.Notapproved:
                        voucherStatus = VoucherStatus.NotApproved; //"N";
                        break;
                    case AccountsPayableInvoicesProcessState.Outstanding:
                        voucherStatus = VoucherStatus.Outstanding; //"O";
                        break;
                    case AccountsPayableInvoicesProcessState.Paid:
                        voucherStatus = VoucherStatus.Paid; //"P";
                        break;
                    case AccountsPayableInvoicesProcessState.Reconciled:
                        voucherStatus = VoucherStatus.Reconciled; //"R";
                        break;
                    case AccountsPayableInvoicesProcessState.Voided:
                        voucherStatus = VoucherStatus.Voided; //"V";
                        break;

                    default: break;
                        // accept any status value since the vaue is ignored and the CTX will determine the status
                        //throw new ApplicationException("Invalid voucher status for voucher: " + accountsPayableInvoices.ProcessState.ToString());
                }
                //if the status is void then voidDate is required.
                    
                if (voucherStatus == VoucherStatus.Voided && !accountsPayableInvoices.VoidDate.HasValue)
                {
                    throw new ArgumentNullException("accountsPayableInvoices", "Must provide void date for the processState of voided.");
                }
            }

            var guid = accountsPayableInvoices.Id;
            var voucherId = string.Empty;            
            try
            {
                voucherId = await _accountsPayableInvoicesRepository.GetAccountsPayableInvoicesIdFromGuidAsync(guid);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to get accounts payable invoice by guid");
            }
            //voucherId cannot be null as it will throw entity exception. This value is nulled in repo before the ctx is called. 
            if (string.IsNullOrEmpty(voucherId))
            {
                voucherId = "NEW";
            }
            var date = (accountsPayableInvoices.TransactionDate == DateTime.MinValue)
                ? DateTime.Now.Date : accountsPayableInvoices.TransactionDate;
            var invoiceNumber = accountsPayableInvoices.VendorInvoiceNumber;
            var invoiceDate = accountsPayableInvoices.VendorInvoiceDate;

            var accountsPayableInvoicesEntity = new Domain.ColleagueFinance.Entities.AccountsPayableInvoices(
                guid, voucherId ?? new Guid().ToString(), date, voucherStatus, "", invoiceNumber, invoiceDate);

            string vendorId = string.Empty;
            if (existingVendor != null && existingVendor.Vendor != null && !string.IsNullOrEmpty(existingVendor.Vendor.Id))
            {
                try
                {
                    vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(existingVendor.Vendor.Id);
                }
                catch (Exception)
                {
                    throw new ArgumentException
                        (string.Concat("The vendor id must correspond with a valid vendor record : ", existingVendor.Vendor.Id));
                }
                if (!string.IsNullOrEmpty(vendorId))
                    accountsPayableInvoicesEntity.VendorId = vendorId;
                else
                {
                    throw new ArgumentNullException("accountsPayableInvoices", "Must provide valid vendor.id for accountsPayableInvoices");
                }
            }
            var addressId = string.Empty;
            if ((existingVendor != null && existingVendor.AlternativeVendorAddress != null) && (!string.IsNullOrEmpty(accountsPayableInvoices.Vendor.ExistingVendor.AlternativeVendorAddress.Id)))
            {
                addressId = await _addressRepository.GetAddressFromGuidAsync(existingVendor.AlternativeVendorAddress.Id);
                if (!string.IsNullOrEmpty(addressId))
                {
                    accountsPayableInvoicesEntity.VoucherAddressId = addressId;
                }
                else
                {
                    throw new ArgumentNullException("accountsPayableInvoices", "Must provide valid address.id for accountsPayableInvoices");
                }
            }
            if (manualVendor != null)
            {
                if (manualVendor.Type != null && manualVendor.Type != AccountsPayableInvoicesVendorType.NotSet)
                    accountsPayableInvoicesEntity.VoucherMiscType = manualVendor.Type.ToString();
                if (string.IsNullOrEmpty(addressId))
                {
                    if (!string.IsNullOrEmpty(manualVendor.Name))
                        accountsPayableInvoicesEntity.VoucherMiscName = new List<string> { manualVendor.Name };
                    if (manualVendor.AddressLines != null && manualVendor.AddressLines.Any())
                        accountsPayableInvoicesEntity.VoucherMiscAddress = manualVendor.AddressLines;
                    if (manualVendor.Place != null)
                    {
                        var place = manualVendor.Place;
                        if (place.Country != null && place.Country.Code != null)
                        {
                            //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                            Domain.Base.Entities.Country countryEntity = null;
                            var countryEntities = (await GetAllCountriesAsync(false)).Where(cc => cc.IsoAlpha3Code == place.Country.Code.ToString());
                            if (countryEntities != null && countryEntities.Any())
                            {
                                if (countryEntities.Count() > 1)
                                   countryEntity = countryEntities.FirstOrDefault(con => con.IsNotInUse == false);
                                if (countryEntity == null)
                                    countryEntity = countryEntities.FirstOrDefault();
                            }
                            if (countryEntity == null)
                            {
                                throw new ArgumentException(string.Format("Country Code of '{0}' cannot be found in the COUNTRIES table (ISO.ALPHA3.CODE) field in Colleague. ", place.Country.Code.ToString()), "manualVendor.place.country.code");
                            }
                            accountsPayableInvoicesEntity.VoucherMiscCountry = countryEntity.Code;
                        }

                        if (place.Country != null && place.Country.Region != null && place.Country.Region.Code != null)
                        {
                            var state = place.Country.Region.Code.ToString();
                            if (state.Contains("-")) state = state.Split('-').ElementAt(1);
                            var stateEntity = (await GetAllStatesAsync(false)).Where(sc => sc.Code == state).FirstOrDefault();
                            if (stateEntity == null)
                            {
                                throw new ArgumentException(string.Format("State Code of '{0}' does not exist in the STATES table in Colleague. ", state), "manualVendor.place.country.region.code");
                            }
                            accountsPayableInvoicesEntity.VoucherMiscState = stateEntity.Code;
                        }
                        if (place.Country != null && !string.IsNullOrEmpty(place.Country.Locality))
                            accountsPayableInvoicesEntity.VoucherMiscCity = place.Country.Locality;
                        if (place.Country != null && !string.IsNullOrEmpty(place.Country.PostalCode))
                            accountsPayableInvoicesEntity.VoucherMiscZip = place.Country.PostalCode;
                    }
                }
            }

            if (!string.IsNullOrEmpty(accountsPayableInvoices.ReferenceNumber))
            {
                accountsPayableInvoicesEntity.VoucherReferenceNo = new List<string>() { accountsPayableInvoices.ReferenceNumber };
            }

            accountsPayableInvoicesEntity.VoucherVoidGlTranDate = accountsPayableInvoices.VoidDate;
            if (accountsPayableInvoices.Type != null && accountsPayableInvoices.Type != AccountsPayableInvoicesType.NotSet)
            {

                switch (accountsPayableInvoices.Type)
                {
                    case AccountsPayableInvoicesType.Eprocurement:
                        accountsPayableInvoicesEntity.Type = "EPROCUREMENT";
                        break;
                    case AccountsPayableInvoicesType.Travel:
                        accountsPayableInvoicesEntity.Type = "TRAVEL";
                        break;
                    case AccountsPayableInvoicesType.Procurement:
                        accountsPayableInvoicesEntity.Type = "PROCUREMENT";
                        break;

                    default: break;

                }
            }

            if (accountsPayableInvoices.Payment != null)
            {
                var payment = accountsPayableInvoices.Payment;
                if ((payment.Source != null) && (!string.IsNullOrEmpty(payment.Source.Id)))
                {
                    var accountsPayableSource = (await this.GetAllAccountsPayableSourcesAsync(false)).FirstOrDefault(ap => ap.Guid == payment.Source.Id);
                    if (accountsPayableSource == null)
                        throw new KeyNotFoundException("AccountsPayableSources not found for guid: " + payment.Source.Id);
                    else if (accountsPayableSource.Source != "R")
                    {
                        throw new ArgumentException(string.Concat("The AP.TYPES code '", accountsPayableSource.Code, "' does not have a source code of 'R'"));
                    }
                    accountsPayableInvoicesEntity.ApType = accountsPayableSource.Code;
                }

                if ((payment.PaymentTerms != null) && (!string.IsNullOrEmpty(payment.PaymentTerms.Id)))
                {
                    var vendorTerms = (await this.GetAllVendorTermsAsync(false)).FirstOrDefault(ap => ap.Guid == payment.PaymentTerms.Id);
                    if (vendorTerms == null)
                        throw new KeyNotFoundException("PaymentTerm not found for guid: " + payment.PaymentTerms.Id);
                    accountsPayableInvoicesEntity.VoucherVendorTerms = vendorTerms.Code;
                }

                if (payment.PaymentDueOn.HasValue)
                    accountsPayableInvoicesEntity.DueDate = payment.PaymentDueOn;
                else
                    accountsPayableInvoicesEntity.DueDate = DateTime.Now.Date;

            }
            accountsPayableInvoicesEntity.VoucherPayFlag =
                accountsPayableInvoices.PaymentStatus == AccountsPayableInvoicesPaymentStatus.Nohold ? "Y" : "N";

           if (!string.IsNullOrEmpty(accountsPayableInvoices.InvoiceComment))
                accountsPayableInvoicesEntity.Comments = accountsPayableInvoices.InvoiceComment;

            if (accountsPayableInvoices.SubmittedBy != null && !string.IsNullOrEmpty(accountsPayableInvoices.SubmittedBy.Id))
            {
                var submittedById = await _personRepository.GetPersonIdFromGuidAsync(accountsPayableInvoices.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    throw new ColleagueWebApiException(string.Concat("SubmittedBy is not found. Guid: ", accountsPayableInvoices.SubmittedBy.Id));
                }
                accountsPayableInvoicesEntity.SubmittedBy = submittedById;
            }
            if (accountsPayableInvoices.InvoiceDiscountAmount != null && accountsPayableInvoices.InvoiceDiscountAmount.Value.HasValue)
            {
                accountsPayableInvoicesEntity.VoucherDiscAmt = accountsPayableInvoices.InvoiceDiscountAmount.Value;
            }
            if (accountsPayableInvoices.VendorBilledAmount != null)
            {
                if (accountsPayableInvoices.VendorBilledAmount.Value.HasValue)
                {
                    accountsPayableInvoicesEntity.VoucherInvoiceAmt = accountsPayableInvoices.VendorBilledAmount.Value;
                }
                if (accountsPayableInvoices.VendorBilledAmount.Currency != null)
                {
                    
                    switch (accountsPayableInvoices.VendorBilledAmount.Currency)
                    {
                        case CurrencyIsoCode.USD:
                            mainCurrency = "USD";
                            break;
                        case CurrencyIsoCode.CAD:
                            mainCurrency = "CAD";
                            break;
                        default:
                            mainCurrency = accountsPayableInvoices.VendorBilledAmount.Currency.ToString();
                            break;
                    }

                    accountsPayableInvoicesEntity.CurrencyCode = mainCurrency;
                }
            }

            if (accountsPayableInvoices.InvoiceDiscountAmount != null && accountsPayableInvoices.InvoiceDiscountAmount.Value.HasValue)
            {
                accountsPayableInvoicesEntity.VoucherDiscAmt = accountsPayableInvoices.InvoiceDiscountAmount.Value;
                //if the currency is still null then check this one has the value.if the value is different, then throw an exception
                if (accountsPayableInvoices.InvoiceDiscountAmount.Currency != null)
                {
                    if (string.IsNullOrEmpty(mainCurrency))
                        mainCurrency = accountsPayableInvoices.InvoiceDiscountAmount.Currency.ToString();
                    else
                        if (!mainCurrency.Equals(accountsPayableInvoices.InvoiceDiscountAmount.Currency.ToString()))
                        throw new ColleagueWebApiException(string.Concat("Only one type of currency is supported. Invalid currency", accountsPayableInvoices.InvoiceDiscountAmount.Currency.ToString()));
                }

            }


            if ((accountsPayableInvoices.Taxes != null) && (accountsPayableInvoices.Taxes.Any()))
            {
                var taxCodesEntities = await GetAllCommerceTaxCodesAsync(false);
                if (taxCodesEntities == null)
                {
                    throw new ColleagueWebApiException("Unable to retrieve commerce tax codes.");
                }
                var uniqueValues = new List<string>();
                foreach (var itemTax in accountsPayableInvoices.Taxes)
                {
                    if (itemTax != null && itemTax.TaxCode != null && !string.IsNullOrEmpty(itemTax.TaxCode.Id))
                    {
                        var taxCode = taxCodesEntities.FirstOrDefault(tax => tax.Guid == itemTax.TaxCode.Id);
                        if (taxCode == null)
                        {
                            throw new ColleagueWebApiException(string.Concat("Tax code  not found for guid:'", itemTax.TaxCode.Id, "'"));
                        }
                        if (taxCode.AppurEntryFlag == false)
                        {
                            throw new ArgumentException(string.Concat("Tax Code '", taxCode.Code, "' is not permitted for use on AP/PUR documents."));
                        }
                        if (!uniqueValues.Contains(itemTax.TaxCode.Id))
                        {
                            uniqueValues.Add(itemTax.TaxCode.Id);
                        }
                        else
                        {
                            throw new ArgumentException(string.Concat("Duplicate taxCode Ids are not permitted."));
                        }
                    }
                    //check tax code rates
                    if (itemTax.TaxCodeRates != null && itemTax.TaxCodeRates.Any())
                    {
                        var allTaxCodeRates = await GetAllCommerceTaxCodeRatesAsync(false);
                        foreach (var rate in itemTax.TaxCodeRates)
                        {
                            if (rate.Rate != null && !string.IsNullOrEmpty(rate.Rate.Id))
                            {
                                var taxRate = allTaxCodeRates.FirstOrDefault(r => r.Guid == rate.Rate.Id);
                                if (taxRate == null)
                                {
                                    throw new ColleagueWebApiException(string.Concat("Tax rate not found for guid:'", rate.Rate.Id, "'"));
                                }
                            }                            
                        }
                    }
                }
            }

            //set the bypassApproval and tax form flags
            accountsPayableInvoicesEntity.ByPassTaxForms = HasPermission(ColleagueFinancePermissionCodes.ByPassTaxForms);
            accountsPayableInvoicesEntity.ByPassVoucherApproval = HasPermission(ColleagueFinancePermissionCodes.ByPassVoucherApproval);

            if ((accountsPayableInvoices.LineItems != null) && (accountsPayableInvoices.LineItems.Any()))
            {
                foreach (var lineItem in accountsPayableInvoices.LineItems)
                {

                    var id = string.Empty;

                    if ((!string.IsNullOrEmpty(lineItem.ReferenceDocumentLineItemNumber))
                        && (!string.IsNullOrEmpty(lineItem.LineItemNumber)))
                    {
                        if (lineItem.ReferenceDocumentLineItemNumber != lineItem.LineItemNumber)
                        {
                            throw new ArgumentException("If providing both a ReferenceDocumentLineItemNumber and a LineItemNumber, they must be the same value. ");
                        }
                        id = lineItem.LineItemNumber;
                    }
                    else if (!string.IsNullOrEmpty(lineItem.ReferenceDocumentLineItemNumber))
                    {
                        id = lineItem.ReferenceDocumentLineItemNumber;
                    }
                    else if (!string.IsNullOrEmpty(lineItem.LineItemNumber))
                    {
                        id = lineItem.LineItemNumber;
                    }
                    else
                    {
                        id = new Guid().ToString();
                    }


                    var description = lineItem.Description;
                    decimal quantity = lineItem.Quantity == null ? 0 : lineItem.Quantity.Value;
                    decimal price = 0;
                    if ((lineItem.UnitPrice != null) && (lineItem.UnitPrice.Value.HasValue))
                    {
                        price = Convert.ToDecimal(lineItem.UnitPrice.Value);
                        //if the currency is still null then check this one has the value.
                        if (lineItem.UnitPrice.Currency != null)
                        {
                            if (string.IsNullOrEmpty(mainCurrency))
                                mainCurrency = lineItem.UnitPrice.Currency.ToString();
                            else
                                if (!mainCurrency.Equals(lineItem.UnitPrice.Currency.ToString()))
                                throw new ColleagueWebApiException(string.Concat("Only one type of currency is supported. Invalid currency", lineItem.UnitPrice.Currency.ToString()));
                        }
                    }
                    decimal extendedPrice = 0;
                    if ((lineItem.VendorBilledUnitPrice != null) && (lineItem.VendorBilledUnitPrice.Value.HasValue))
                    {
                        extendedPrice = Convert.ToDecimal(lineItem.VendorBilledUnitPrice.Value);
                    }
                    var apLineItem = new AccountsPayableInvoicesLineItem(id, description, quantity, price, extendedPrice);

                    //set final payment flag
                    apLineItem.FinalPaymentFlag = lineItem.EncumbranceStatus == AccountsPayableInvoicesEncumbranceStatus.FinalPayment ? true : false;

                    if (lineItem.ReferenceDocument != null)
                    {
                        if (lineItem.ReferenceDocument.PurchaseOrder != null && (!string.IsNullOrWhiteSpace(lineItem.ReferenceDocument.PurchaseOrder.Id)))
                        {

                            var poId = await _accountsPayableInvoicesRepository.GetIdFromGuidAsync(lineItem.ReferenceDocument.PurchaseOrder.Id);
                            if ((poId == null) || (poId.Entity != "PURCHASE.ORDERS"))
                            {
                                throw new ArgumentNullException("ReferenceDocument.PurchaseOrder.Id", string.Format("Guid not found for ReferenceDocument.PurchaseOrder.Id: {0}", lineItem.ReferenceDocument.PurchaseOrder.Id));
                            }
                            apLineItem.PurchaseOrderId = poId.PrimaryKey;
                            //if there is a PO, then there needs to be a line item number
                            if (string.IsNullOrEmpty(lineItem.ReferenceDocumentLineItemNumber))
                            {
                                throw new ColleagueWebApiException("referenceDocumentLineItemNumber is required if referenceDocument is in the payload.");
                            }
                        }
                        if (lineItem.ReferenceDocument.BlanketPurchaseOrder != null && (!string.IsNullOrWhiteSpace(lineItem.ReferenceDocument.BlanketPurchaseOrder.Id)))
                        {

                            var bpoId = await _accountsPayableInvoicesRepository.GetIdFromGuidAsync(lineItem.ReferenceDocument.BlanketPurchaseOrder.Id);
                            if ((bpoId == null) || (bpoId.Entity != "BPO"))
                            {
                                throw new ArgumentNullException("ReferenceDocument.BlanketPurchaseOrder.Id", string.Format("Guid not found for ReferenceDocument.BlanketPurchaseOrder.Id: {0}", lineItem.ReferenceDocument.BlanketPurchaseOrder.Id));
                            }
                            apLineItem.BlanketPurchaseOrderId = bpoId.PrimaryKey;
                            //if there is a BPO, then there cannot be a reference line number
                            if (!string.IsNullOrEmpty(lineItem.ReferenceDocumentLineItemNumber))
                            {
                                throw new ColleagueWebApiException("A reference document line item number is not permitted when paying a Blanket Purchase Order");
                            }
                        }
                    }
                    //final [ayment is only valid if there is a purchase order

                    if (string.IsNullOrEmpty(apLineItem.PurchaseOrderId) && apLineItem.FinalPaymentFlag == true)
                    {
                        throw new ColleagueWebApiException("The encumbrance status of finalPayment is only valid for the line item with the reference document type of PurchaseOrder.");
                    }

                    apLineItem.DocLineItemId = lineItem.ReferenceDocumentLineItemNumber != null ? lineItem.ReferenceDocumentLineItemNumber : new Guid().ToString();

                    if ((lineItem.CommodityCode != null) && (!string.IsNullOrEmpty(lineItem.CommodityCode.Id)))
                    {
                        var allCommodityCodes = await GetAllCommodityCodesAsync(false);
                        if ((allCommodityCodes == null) || (!allCommodityCodes.Any()))
                        {
                            throw new ColleagueWebApiException("An error occurred extracting all commodity codes");
                        }
                        var commodityCode = allCommodityCodes.FirstOrDefault(c => c.Guid == lineItem.CommodityCode.Id);
                        if (commodityCode == null)
                        {
                            throw new ColleagueWebApiException("Unable to determine commodity code represented by guid: " + lineItem.CommodityCode.Id);
                        }
                        apLineItem.CommodityCode = commodityCode.Code;
                    }

                    if ((lineItem.FixedAssetDesignation != null) && (!string.IsNullOrEmpty(lineItem.FixedAssetDesignation.Id)))
                    {
                        var allFixedAssesDesigns = (await GetFixedAssetDesignationsAsync(false));
                        if ((allFixedAssesDesigns == null) || (!allFixedAssesDesigns.Any()))
                        {
                            throw new ColleagueWebApiException("An error occurred extracting all fixed asset designations");
                        }
                        var fixedAssetDesignation = allFixedAssesDesigns.FirstOrDefault(c => c.Guid == lineItem.FixedAssetDesignation.Id);
                        if (fixedAssetDesignation == null)
                        {
                            throw new ColleagueWebApiException("Unable to determine fixed asset designations represented by guid: " + lineItem.FixedAssetDesignation.Id);
                        }
                        apLineItem.FixedAssetsFlag = fixedAssetDesignation.Code;
                    }

                    if ((lineItem.UnitofMeasure != null) && (!string.IsNullOrEmpty(lineItem.UnitofMeasure.Id)))
                    {
                        var allCommodityUnitTypes = await this.GetAllCommodityUnitTypesAsync(false);
                        if ((allCommodityUnitTypes == null) || (!allCommodityUnitTypes.Any()))
                        {
                            throw new ColleagueWebApiException("An error occurred extracting all commodity unit types");
                        }
                        var commodityUnitType = allCommodityUnitTypes.FirstOrDefault(c => c.Guid == lineItem.UnitofMeasure.Id);
                        if (commodityUnitType == null)
                        {
                            throw new ColleagueWebApiException("Unable to determine unit of measure represented by guid: " + lineItem.UnitofMeasure.Id);
                        }
                        apLineItem.UnitOfMeasure = commodityUnitType.Code;

                    }
                    apLineItem.Comments = lineItem.Comment;

                    if (lineItem.Discount != null)
                    {
                        if (lineItem.Discount.Amount != null && lineItem.Discount.Amount.Value != null && lineItem.Discount.Amount.Value.HasValue)
                        {
                            apLineItem.TradeDiscountAmount = lineItem.Discount.Amount.Value;
                            //if the currency is still null then check this one has the value.if the value is different, then throw an exception
                            if (lineItem.Discount.Amount.Currency != null)
                            {
                                if (string.IsNullOrEmpty(mainCurrency))
                                    mainCurrency = lineItem.Discount.Amount.Currency.ToString();
                                else
                                    if (!mainCurrency.Equals(lineItem.Discount.Amount.Currency.ToString()))
                                    throw new ColleagueWebApiException(string.Concat("Only one type of currency is supported. Invalid currency", lineItem.Discount.Amount.Currency.ToString()));
                            }
                        }
                        else if (lineItem.Discount.Percent != null && lineItem.Discount.Percent.HasValue)
                        {
                            apLineItem.TradeDiscountPercent = lineItem.Discount.Percent;
                        }
                    }

                    var lineItemTaxCodes = new List<string>();
                    var lineItemTaxAmts = new List<decimal?>();
                    if ((lineItem.Taxes != null) && (lineItem.Taxes.Any()))
                    {
                        var accountsPayableLineItemTaxes = new List<LineItemTax>();
                        var uniqueValues = new List<string>();
                        foreach (var lineItemTax in lineItem.Taxes)
                        {
                            if (lineItemTax != null && lineItemTax.TaxCode != null && !string.IsNullOrEmpty(lineItemTax.TaxCode.Id))
                            {
                                var taxCodesEntities = await GetAllCommerceTaxCodesAsync(false);
                                if (taxCodesEntities == null)
                                {
                                    throw new ColleagueWebApiException("An error occurred extracting all commerce tax codes");
                                }
                                var taxCode = taxCodesEntities.FirstOrDefault(tax => tax.Guid == lineItemTax.TaxCode.Id);
                                if (taxCode == null)
                                {
                                    throw new ColleagueWebApiException(string.Concat("Tax code not found for guid:'", lineItemTax.TaxCode.Id, "'"));
                                }
                                if (taxCode.AppurEntryFlag == false)
                                {
                                    throw new ArgumentException(string.Concat("Tax Code '", taxCode.Code, "' is not permitted for use on AP/PUR documents."));
                                }
                                if (!uniqueValues.Contains(lineItemTax.TaxCode.Id))
                                {
                                    uniqueValues.Add(lineItemTax.TaxCode.Id);
                                }
                                else
                                {
                                    throw new ArgumentException(string.Concat("Duplicate taxCode Ids are not permitted."));
                                }
                                if (lineItemTax.VendorAmount != null && lineItemTax.VendorAmount.Value != null && lineItemTax.VendorAmount.Value.HasValue)
                                {
                                    var lineItemTaxEntity = new LineItemTax(taxCode.Code, lineItemTax.VendorAmount.Value.Value);
                                    accountsPayableLineItemTaxes.Add(lineItemTaxEntity);
                                }
                                else
                                {
                                    var lineItemTaxEntity = new LineItemTax(taxCode.Code, null);
                                    accountsPayableLineItemTaxes.Add(lineItemTaxEntity);
                                }

                            }
                        }
                        if (accountsPayableLineItemTaxes.Any())
                            apLineItem.AccountsPayableLineItemTaxes = accountsPayableLineItemTaxes.OrderBy(t => t.TaxCode).ToList();
                    }      
                

                    if (lineItem.AccountDetails != null && lineItem.AccountDetails.Any())
                    {
                        foreach (var accountDetails in lineItem.AccountDetails)
                        {
                            decimal distributionQuantity = 0;
                            decimal distributionAmount = 0;
                            decimal distributionPercent = 0;
                            var allocated = accountDetails.Allocation != null ? accountDetails.Allocation.Allocated : null;
                            if (allocated != null)
                            {
                                if (allocated.Quantity.HasValue)
                                    distributionQuantity = Convert.ToDecimal(allocated.Quantity);
                                if (allocated.Amount != null && allocated.Amount.Value.HasValue)
                                {
                                    distributionAmount = Convert.ToDecimal(allocated.Amount.Value);
                                    //if the currency is still null then check this one has the value.
                                    if (allocated.Amount.Currency != null)
                                    {
                                        if (string.IsNullOrEmpty(mainCurrency))
                                            mainCurrency = allocated.Amount.Currency.ToString();
                                        else
                                            if (!mainCurrency.Equals(allocated.Amount.Currency.ToString()))
                                            throw new ColleagueWebApiException(string.Concat("Only one type of currency is supported. Invalid currency", allocated.Amount.Currency.ToString()));
                                    }                              
                                }
                                if (allocated.Percentage.HasValue)
                                    distributionPercent = Convert.ToDecimal(allocated.Percentage);
                            }
                           
                            string accountingString = ConvertAccountingString(GLCompCount, accountDetails.AccountingString);

                            var glDistribution = new LineItemGlDistribution(accountingString, distributionQuantity, distributionAmount, distributionPercent);

                            apLineItem.AddGlDistribution(glDistribution);

                            if ((accountDetails.SubmittedBy != null) && (!string.IsNullOrEmpty(accountDetails.SubmittedBy.Id)))
                            {
                                var personid = await _personRepository.GetPersonIdFromGuidAsync(accountDetails.SubmittedBy.Id);
                                if (!string.IsNullOrEmpty(personid))
                                {
                                    if ((!string.IsNullOrEmpty(accountsPayableInvoicesEntity.VoucherRequestor)) &&
                                        (accountsPayableInvoicesEntity.VoucherRequestor != personid))
                                    {
                                        throw new ArgumentException("Can not provided different submitted by ids in the same request.");
                                    }
                                    accountsPayableInvoicesEntity.VoucherRequestor = personid;
                                }
                            }

                            //validate source even if it is ignored. Ignored if present in the request
                            if ((accountDetails.Source != null) && (!string.IsNullOrEmpty(accountDetails.Source.Id)))
                            {
                                var apTypesEntities = await GetAllAccountsPayableSourcesAsync(false);
                                if (apTypesEntities == null)
                                {
                                    throw new ColleagueWebApiException("An error occurred extracting all accounts-payable-sources");
                                }
                                var payableSources = apTypesEntities.FirstOrDefault(c => c.Guid == accountDetails.Source.Id);
                                if (payableSources == null)
                                {
                                    throw new ColleagueWebApiException("Unable to determine accounts-payable-sources represented by guid: " + accountDetails.Source.Id);
                                }                                
                            }

                            //validate taxForm Component
                            if ((accountDetails.TaxFormComponent != null) && (!string.IsNullOrEmpty(accountDetails.TaxFormComponent.Id)))
                            {
                                var taxBoxEntities = await GetAllBoxCodesAsync(false);
                                if (taxBoxEntities == null)
                                {
                                    throw new ColleagueWebApiException("An error occurred extracting all tax-form-components");
                                }
                                var taxBox = taxBoxEntities.FirstOrDefault(c => c.Guid == accountDetails.TaxFormComponent.Id);
                                if (taxBox == null)
                                {
                                    throw new ColleagueWebApiException("Unable to determine tax form components represented by guid: " + accountDetails.TaxFormComponent.Id);
                                }
                                else
                                {
                                    apLineItem.TaxFormCode = taxBox.Code;
                                    apLineItem.TaxForm = taxBox.TaxCode;
                                    
                                }
                            }
                        }

                        
                    }
                    accountsPayableInvoicesEntity.AddAccountsPayableInvoicesLineItem(apLineItem);
                }
            }
            if (string.IsNullOrEmpty(accountsPayableInvoicesEntity.CurrencyCode) && !string.IsNullOrEmpty(mainCurrency))
                accountsPayableInvoicesEntity.CurrencyCode = mainCurrency;
            return accountsPayableInvoicesEntity;
        }

        /// <summary>
        /// Converts an AccountsPayableInvoices domain entity to its corresponding AccountsPayableInvoices DTO
        /// </summary>
        /// <param name="source">AccountsPayableInvoices domain entity</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.AccountsPayableInvoices2">AccountsPayableInvoices</see></returns>
        private async Task<AccountsPayableInvoices2> ConvertAccountsPayableInvoicesEntityToDto2Async(Domain.ColleagueFinance.Entities.AccountsPayableInvoices source,
            GeneralLedgerAccountStructure GlConfig, Dictionary<string, string> vendorGuidCollection, Dictionary<string, string> poGuidCollection, Dictionary<string, string> bpoGuidCollection, Dictionary<string, string> addressGuidCollection, Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {

            var accountsPayableInvoices = new Ellucian.Colleague.Dtos.AccountsPayableInvoices2();
            Dtos.EnumProperties.CurrencyIsoCode currency = Dtos.EnumProperties.CurrencyIsoCode.USD;
            if (source == null)
            {
                IntegrationApiExceptionAddError("Unable to retrieve accounts-payable-invoices entity.", "Bad.Data");
                return null;
            }
            if (GlConfig == null)
            {
                IntegrationApiExceptionAddError("GL Account Structure is not configured.", "Bad.Data", source.Guid, source.Id);
            }
            try
            {
                if (!(string.IsNullOrEmpty(source.CurrencyCode)))
                {
                    currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), source.CurrencyCode);
                }
                else
                {
                    var hostCountry = source.HostCountry;
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                        CurrencyIsoCode.USD;
                }
            }
            catch
            {
                var hostCountry = source.HostCountry;
                currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                    CurrencyIsoCode.USD;
            }
            if (string.IsNullOrEmpty(source.Guid))
                IntegrationApiExceptionAddError(string.Concat("Missing GUID for voucher: ", source.Id), "GUID.Not.Found", null, source.Id);
            else
                accountsPayableInvoices.Id = source.Guid;
            accountsPayableInvoices.InvoiceNumber = source.Id;

            if (!string.IsNullOrEmpty(source.Type))
            {
                string upperType = source.Type.ToUpper();
                switch (upperType)
                {
                    case ("PROCUREMENT"):
                        accountsPayableInvoices.Type = AccountsPayableInvoicesType.Procurement;
                        break;
                    case ("EPROCUREMENT"):
                        accountsPayableInvoices.Type = AccountsPayableInvoicesType.Eprocurement;
                        break;
                    case ("TRAVEL"):
                        accountsPayableInvoices.Type = AccountsPayableInvoicesType.Travel;
                        break;
                    default:
                        break;
                }
            }
            accountsPayableInvoices.Vendor = new AccountsPayableInvoicesVendorDtoProperty();
            if (!string.IsNullOrEmpty(source.VendorId))
            {
                string vendorGuid = string.Empty;
                if (vendorGuidCollection != null && vendorGuidCollection.Any() && vendorGuidCollection.TryGetValue(source.VendorId, out vendorGuid))
                {
                    var addressId = !string.IsNullOrEmpty(source.VoucherAddressId) && source.VoucherUseAltAddress ? source.VoucherAddressId : string.Empty;
                    var existingVendor = new AccountsPayableInvoicesExistingVendorDtoProperty();
                    existingVendor.Vendor = new GuidObject2(vendorGuid);
                    if (!(string.IsNullOrEmpty(addressId)))
                    {
                        try
                        {
                            var addressGuid = string.Empty;
                            if (addressGuidCollection != null && addressGuidCollection.Any())
                                addressGuidCollection.TryGetValue(addressId, out addressGuid);
                            if (!(string.IsNullOrWhiteSpace(addressGuid)))
                            {
                                existingVendor.AlternativeVendorAddress = new GuidObject2(addressGuid);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Do nothing if invalid address ID.  Just leave address ID off the results.
                            logger.Error(ex, "Unable to get vendor address.");
                        }
                    }
                    accountsPayableInvoices.Vendor.ExistingVendor = existingVendor;
                }
                else
                {
                    //since there is already vendor.Id, the lack of guid for it should throw an error. 
                    IntegrationApiExceptionAddError(string.Format("Missing GUID for vendor id '{0}'.", source.VendorId), "GUID.Not.Found",
                        source.Guid, source.Id);
                }
            }
            else
            {
                if (!source.VoucherMiscName.Any())
                {
                    IntegrationApiExceptionAddError("Missing vendor information.", "Bad.Data", source.Guid, source.Id);
                }
                source.VendorId = string.Empty;
            }
            if (string.IsNullOrEmpty(source.VendorId) || !string.IsNullOrEmpty(source.VoucherMiscType) || (source.VoucherUseAltAddress && string.IsNullOrEmpty(source.VoucherAddressId)))
            {
                var manualVendorDetails = new AccountsPayableInvoicesManualVendorDetailsDtoProperty();
                // Type
                if (source.VoucherMiscType != null)
                {
                    if (source.VoucherMiscType.ToLower() == "person")
                    {
                        manualVendorDetails.Type = AccountsPayableInvoicesVendorType.Person;
                    }
                    else
                    {
                        manualVendorDetails.Type = AccountsPayableInvoicesVendorType.Organization;
                    }
                }
                if (string.IsNullOrEmpty(source.VendorId) || (source.VoucherUseAltAddress && string.IsNullOrEmpty(source.VoucherAddressId)))
                {
                    // Misc. Name
                    string name = string.Empty;
                    foreach (var vouName in source.VoucherMiscName)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            name = string.Concat(name, " ");
                        }
                        name = string.Concat(name, vouName);
                    }
                    manualVendorDetails.Name = name;
                    // Misc. Address Information
                    if (source.VoucherMiscAddress.Any())
                    {
                        manualVendorDetails.AddressLines = source.VoucherMiscAddress;
                        var place = await BuildAddressPlaceDtoAsync(source);
                        if ((place != null) && (place.Country != null))
                            manualVendorDetails.Place = place;
                    }
                }
                accountsPayableInvoices.Vendor.ManualVendorDetails = manualVendorDetails;
            }
            //display submittedBy
            if (!string.IsNullOrEmpty(source.SubmittedBy))
            {

                if (personGuidCollection == null)
                {
                    IntegrationApiExceptionAddError(
                    string.Format("Missing GUID for account details submitted by id '{0}': ", source.SubmittedBy), "GUID.Not.Found",
                    source.Guid, source.Id);
                }
                else
                {
                    var personGuid = string.Empty;
                    if (personGuidCollection.TryGetValue(source.SubmittedBy, out personGuid))
                    {
                        accountsPayableInvoices.SubmittedBy = new GuidObject2(personGuid);
                    }
                    else
                    {
                        IntegrationApiExceptionAddError( string.Format("Missing GUID for account details submitted by id '{0}': ", source.SubmittedBy), "GUID.Not.Found",
                        source.Guid, source.Id);
                    }
                }
            }
            if ((source.VoucherReferenceNo != null) && (source.VoucherReferenceNo.Any()))
                accountsPayableInvoices.ReferenceNumber = source.VoucherReferenceNo.FirstOrDefault();

            accountsPayableInvoices.VendorInvoiceNumber = source.InvoiceNumber;
            accountsPayableInvoices.TransactionDate = source.Date;
            accountsPayableInvoices.VendorInvoiceDate = source.InvoiceDate;
            accountsPayableInvoices.VoidDate = source.VoucherVoidGlTranDate;

            var processState = AccountsPayableInvoicesProcessState.NotSet;
            switch (source.Status)
            {
                case VoucherStatus.InProgress:
                    processState = AccountsPayableInvoicesProcessState.Inprogress;
                    break;
                case VoucherStatus.Outstanding:
                    processState = AccountsPayableInvoicesProcessState.Outstanding;
                    break;
                case VoucherStatus.Voided:
                    processState = AccountsPayableInvoicesProcessState.Voided;
                    break;
                case VoucherStatus.Paid:
                    processState = AccountsPayableInvoicesProcessState.Paid;
                    break;
                case VoucherStatus.NotApproved:
                    processState = AccountsPayableInvoicesProcessState.Notapproved;
                    break;
                case VoucherStatus.Reconciled:
                    processState = AccountsPayableInvoicesProcessState.Reconciled;
                    break;
                default:
                    break;

            }
            if (processState != AccountsPayableInvoicesProcessState.NotSet)
                accountsPayableInvoices.ProcessState = processState;

            accountsPayableInvoices.PaymentStatus = (source.VoucherPayFlag == "Y") ?
                AccountsPayableInvoicesPaymentStatus.Nohold : AccountsPayableInvoicesPaymentStatus.Hold;

            if (source.VoucherInvoiceAmt != null)
            {
                var vendorBilledAmount = new Amount2DtoProperty
                {
                    Currency = currency,
                    Value = source.VoucherInvoiceAmt
                };

                accountsPayableInvoices.VendorBilledAmount = vendorBilledAmount;
            }

            if (source.VoucherDiscAmt != null)
            {
                var invoiceDiscountAmount = new Amount2DtoProperty
                {
                    Currency = currency,
                    Value = source.VoucherDiscAmt
                };
                accountsPayableInvoices.InvoiceDiscountAmount = invoiceDiscountAmount;
            }
            if (source.VoucherTaxes != null && source.VoucherTaxes.Any())
            {
                var accountsPayableInvoicesTaxes = new List<AccountsPayableInvoicesTaxesDtoProperty>();
                foreach (var voucherTax in source.VoucherTaxes)
                {
                    var accountsPayableInvoicesTax = new AccountsPayableInvoicesTaxesDtoProperty();                    
                    try
                    {
                        var taxCodeGuid = await _referenceDataRepository.GetCommerceTaxCodeGuidAsync(voucherTax.TaxCode);
                        if (!string.IsNullOrEmpty(taxCodeGuid))
                        {
                            accountsPayableInvoicesTax.TaxCode = new GuidObject2(taxCodeGuid);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                            source.Guid, source.Id);
                    }
                    var vendorAmount = new Amount2DtoProperty
                    {
                        Currency = currency,
                        Value = voucherTax.TaxAmount
                    };

                    accountsPayableInvoicesTax.VendorAmount = vendorAmount;

                    accountsPayableInvoicesTaxes.Add(accountsPayableInvoicesTax);
                }

                if (accountsPayableInvoicesTaxes.Any())
                    accountsPayableInvoices.Taxes = accountsPayableInvoicesTaxes;
            }

            if (source.VoucherNet.HasValue)
            {
                accountsPayableInvoices.InvoiceType = (source.VoucherNet < 0) ? AccountsPayableInvoicesInvoiceType.Creditinvoice
                    : AccountsPayableInvoicesInvoiceType.Invoice;
            }
            var apTypeGuid = string.Empty;
            if (!string.IsNullOrWhiteSpace(source.ApType))
            {            
                try
                {
                    apTypeGuid = await _colleagueFinanceReferenceDataRepository.GetAccountsPayableSourceGuidAsync(source.ApType);
                    if (!string.IsNullOrEmpty(apTypeGuid))
                    {
                        var payment = new AccountsPayableInvoicesPaymentDtoProperty();
                        payment.Source = new GuidObject2(apTypeGuid);
                        payment.PaymentDueOn = source.DueDate;
                        if (!string.IsNullOrEmpty(source.VoucherVendorTerms))
                        {                          
                            try
                            {
                                var vendorTermGuid = await _colleagueFinanceReferenceDataRepository.GetVendorTermGuidAsync(source.VoucherVendorTerms);
                                if (!string.IsNullOrEmpty(vendorTermGuid))
                                {
                                    payment.PaymentTerms = new GuidObject2(vendorTermGuid);
                                }
                            }
                            catch (RepositoryException ex)
                            {
                                IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                    source.Guid, source.Id);
                            }
                        }
                        accountsPayableInvoices.Payment = payment;
                    }
                    
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                        source.Guid, source.Id);
                }                
            }                

            if (!(string.IsNullOrWhiteSpace(source.Comments)))
                accountsPayableInvoices.InvoiceComment = source.Comments;
            
            if (source.LineItems != null && source.LineItems.Any())
            {
                var accountsPayableInvoicesLineItems = new List<AccountsPayableInvoicesLineItemDtoProperty2>();
                foreach (var lineItem in source.LineItems)
                {
                    var accountsPayableInvoicesLineItem = new AccountsPayableInvoicesLineItemDtoProperty2();

                    if (!string.IsNullOrEmpty(lineItem.PurchaseOrderId))
                    {
                        var poGuid = string.Empty;
                        if (poGuidCollection == null)
                        {                            
                            IntegrationApiExceptionAddError(string.Format("GUID not found for purchase order id '{0}'", lineItem.PurchaseOrderId), "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            poGuidCollection.TryGetValue(lineItem.PurchaseOrderId, out poGuid);
                            if (string.IsNullOrEmpty(poGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("GUID not found for purchase order id '{0}'", lineItem.PurchaseOrderId), "GUID.Not.Found", source.Guid, source.Id);
                            }
                            else
                            {
                                accountsPayableInvoicesLineItem.ReferenceDocument = new LineItemReferenceDocumentDtoProperty2()
                                {
                                    PurchaseOrder = new GuidObject2(poGuid)
                                };
                            }
                        }
                        
                    }
                    else if (!string.IsNullOrEmpty(source.BlanketPurchaseOrderId))
                    {
                        var bpoGuid = string.Empty;
                        if (bpoGuidCollection == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("GUID not found for bpo id '{0}'", source.BlanketPurchaseOrderId), "GUID.Not.Found", source.Guid, source.Id);
                        }
                        else
                        {
                            bpoGuidCollection.TryGetValue(source.BlanketPurchaseOrderId, out bpoGuid);
                            if (string.IsNullOrEmpty(bpoGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("GUID not found for bpo id '{0}'", source.BlanketPurchaseOrderId), "GUID.Not.Found", source.Guid, source.Id);
                            }
                            else
                            {
                                accountsPayableInvoicesLineItem.ReferenceDocument = new LineItemReferenceDocumentDtoProperty2()
                                {
                                    BlanketPurchaseOrder = new GuidObject2(bpoGuid)
                                };
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(source.RecurringVoucherId))
                    {
                        accountsPayableInvoicesLineItem.ReferenceDocument = new LineItemReferenceDocumentDtoProperty2()
                        {
                            RecurringVoucher = source.RecurringVoucherId
                        };

                    }
                    //for BPO, we ae not displaying the referenceDocumentLineItemNumber property
                    if (accountsPayableInvoicesLineItem.ReferenceDocument != null && accountsPayableInvoicesLineItem.ReferenceDocument.BlanketPurchaseOrder == null)
                        accountsPayableInvoicesLineItem.ReferenceDocumentLineItemNumber = lineItem.Id;

                    accountsPayableInvoicesLineItem.LineItemNumber = lineItem.Id;

                    accountsPayableInvoicesLineItem.Description = lineItem.Description;

                    if (!string.IsNullOrEmpty(lineItem.CommodityCode))
                    {                       
                        try
                        {
                            var commodityCodeGuid = await _colleagueFinanceReferenceDataRepository.GetCommodityCodeGuidAsync(lineItem.CommodityCode);
                            if (!string.IsNullOrEmpty(commodityCodeGuid))
                            {
                                accountsPayableInvoicesLineItem.CommodityCode = new GuidObject2(commodityCodeGuid);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                source.Guid, source.Id);
                        }                       
                    }

                    if (!string.IsNullOrEmpty(lineItem.FixedAssetsFlag))
                    {
                        try
                        {
                            var fixedAssetFlag = await _colleagueFinanceReferenceDataRepository.GetFxaTransferFlagGuidAsync(lineItem.FixedAssetsFlag);
                            if (!string.IsNullOrEmpty(fixedAssetFlag))
                            {
                                accountsPayableInvoicesLineItem.FixedAssetDesignation = new GuidObject2(fixedAssetFlag);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                source.Guid, source.Id);
                        }
                    }

                    accountsPayableInvoicesLineItem.Quantity = lineItem.Quantity;

                    if (!(string.IsNullOrEmpty(lineItem.UnitOfIssue)))
                    {                      
                        try
                        {
                            var commodityUnitTypeGuid = await _colleagueFinanceReferenceDataRepository.GetCommodityUnitTypeGuidAsync(lineItem.UnitOfIssue);
                            if (!string.IsNullOrEmpty(commodityUnitTypeGuid))
                            {
                                accountsPayableInvoicesLineItem.UnitofMeasure = new GuidObject2(commodityUnitTypeGuid);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                source.Guid, source.Id);
                        }
                    }

                    accountsPayableInvoicesLineItem.UnitPrice = new Amount2DtoProperty()
                    {
                        Value = lineItem.Price,
                        Currency = currency
                    };                    

                    if ((lineItem.AccountsPayableLineItemTaxes != null) && (lineItem.AccountsPayableLineItemTaxes.Any()))
                    {
                        var accountsPayableInvoicesLineItemTaxes = new List<AccountsPayableInvoicesTaxesDtoProperty>();

                        //need to combine similar taxcodes
                        var lineItemTaxesTuple = lineItem.AccountsPayableLineItemTaxes
                            .GroupBy(l => l.TaxCode)
                            .Select(cl => new Tuple<string, decimal?>(
                                   cl.First().TaxCode,
                                   cl.Sum(c => c.TaxAmount)
                                )).ToList().OrderBy(co => co.Item1);

                        foreach (var lineItemTax in lineItemTaxesTuple)
                        {
                            var accountsPayableInvoicesTax = new AccountsPayableInvoicesTaxesDtoProperty();                            
                            try
                            {
                                var taxCodeGuid = await _referenceDataRepository.GetCommerceTaxCodeGuidAsync(lineItemTax.Item1);
                                if (!string.IsNullOrEmpty(taxCodeGuid))
                                {
                                    accountsPayableInvoicesTax.TaxCode = new GuidObject2(taxCodeGuid);
                                }
                            }
                            catch (RepositoryException ex)
                            {
                                IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                    source.Guid, source.Id);
                            }                            
                            accountsPayableInvoicesTax.VendorAmount = new Amount2DtoProperty()
                            {
                                Currency = currency,
                                Value = lineItemTax.Item2
                            };

                            // get tax code rate
                            var allTaxCodeRatesEntities = await GetAllCommerceTaxCodeRatesAsync(bypassCache);
                            if (allTaxCodeRatesEntities != null)
                            {
                                var taxRateEntities = allTaxCodeRatesEntities.Where(r => r.Code == lineItemTax.Item1 && r.ApTaxEffectiveDate <= source.Date).OrderByDescending(date => date.ApTaxEffectiveDate);
                                if (taxRateEntities != null && taxRateEntities.Any())
                                {
                                    if (!string.IsNullOrEmpty(taxRateEntities.FirstOrDefault().Guid))
                                    {
                                        var taxRateDtos = new List<LineItemTaxCodeRateDtoProperty>();
                                        var taxRateDto = new LineItemTaxCodeRateDtoProperty();
                                        taxRateDto.Rate = new GuidObject2(taxRateEntities.FirstOrDefault().Guid);
                                        taxRateDto.Amount = new Amount2DtoProperty()
                                        {
                                            Currency = currency,
                                            Value = lineItemTax.Item2
                                        };
                                        taxRateDtos.Add(taxRateDto);
                                        accountsPayableInvoicesTax.TaxCodeRates = taxRateDtos;
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError(string.Format("GUID not found for tax rate id '{0}'", taxRateEntities.FirstOrDefault().Code), "GUID.Not.Found", source.Guid, source.Id);
                                    }
                                }                                
                            }

                            accountsPayableInvoicesLineItemTaxes.Add(accountsPayableInvoicesTax);
                        }

                        if (accountsPayableInvoicesLineItemTaxes.Any())
                            accountsPayableInvoicesLineItem.Taxes = accountsPayableInvoicesLineItemTaxes;
                    }

                                                                                                   
                    if (lineItem.TradeDiscountAmount.HasValue && lineItem.TradeDiscountAmount > 0)
                    {
                        var accountsPayableInvoicesLineItemDiscount = new AccountsPayableInvoicesDiscountDtoProperty();

                        accountsPayableInvoicesLineItemDiscount.Amount = new Amount2DtoProperty()
                        {
                            Currency = currency,
                            Value = lineItem.TradeDiscountAmount.Value
                        };                     
                        accountsPayableInvoicesLineItem.Discount = accountsPayableInvoicesLineItemDiscount;
                    }
                   
                    accountsPayableInvoicesLineItem.PaymentStatus = (source.VoucherPayFlag == "Y") ?
                        AccountsPayableInvoicesPaymentStatus.Nohold : AccountsPayableInvoicesPaymentStatus.Hold;

                    if (!(string.IsNullOrWhiteSpace(lineItem.Comments)))
                    {
                        accountsPayableInvoicesLineItem.Comment = lineItem.Comments;
                    }
                   
                    switch (source.Status)
                    {
                        case VoucherStatus.InProgress:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Open;
                            break;
                        case VoucherStatus.Outstanding:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Open;
                            break;
                        case VoucherStatus.Voided:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Closed;
                            break;
                        case VoucherStatus.Paid:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Closed;
                            break;
                        case VoucherStatus.NotApproved:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Open;
                            break;
                        case VoucherStatus.Reconciled:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Closed;
                            break;
                        case VoucherStatus.Cancelled:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Closed;
                            break;
                        default:
                            accountsPayableInvoicesLineItem.Status = AccountsPayableInvoicesStatus.Closed;
                            break;
                    }

                    var accountsPayableInvoicesLineItemAccountDetails = new List<AccountsPayableInvoicesAccountDetailDtoProperty>();


                    foreach (var glDistribution in lineItem.GlDistributions)
                    {
                        if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                        {
                            var accountsPayableInvoicesAccountDetail = new AccountsPayableInvoicesAccountDetailDtoProperty();

                          
                            string acctNumber = glDistribution.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                            acctNumber = GetFormattedGlAccount(acctNumber, GlConfig);
                             if (!string.IsNullOrEmpty(glDistribution.ProjectId))
                            {
                                acctNumber = ConvertAccountingstringToIncludeProjectRefNo(glDistribution.ProjectId, acctNumber);
                            }
                            accountsPayableInvoicesAccountDetail.AccountingString = acctNumber;

                            var accountDetailAllocation = new AccountsPayableInvoicesAllocationDtoProperty();

                            var accountsPayableInvoicesAllocated = new AccountsPayableInvoicesAllocatedDtoProperty();
                            accountsPayableInvoicesAllocated.Amount = new Amount2DtoProperty()
                            {

                                Currency = currency,
                                Value = glDistribution.Amount
                            };

                            accountsPayableInvoicesAllocated.Quantity = glDistribution.Quantity; // lineItem.Quantity;
                            accountsPayableInvoicesAllocated.Percentage = glDistribution.Percent;
                            accountDetailAllocation.Allocated = accountsPayableInvoicesAllocated;

                            //not used: accountDetailAllocation.AdditionalAmount = new AmountDtoProperty();
                            //not used: accountDetailAllocation.DiscountAmount = new AmountDtoProperty();
                            if ((lineItem.AccountsPayableLineItemTaxes != null) && (lineItem.AccountsPayableLineItemTaxes.Any()))
                            {
                                var glTax = lineItem.AccountsPayableLineItemTaxes.Where(lit => lit.LineGlNumber == glDistribution.GlAccountNumber)
                                    .Sum(c => c.TaxAmount);

                                if (glTax != 0)
                                {
                                    accountDetailAllocation.TaxAmount = new Amount2DtoProperty()
                                    {
                                        Value = glTax,
                                        Currency = currency
                                    };
                                }
                            }
                            accountsPayableInvoicesAccountDetail.Allocation = accountDetailAllocation;


                            if (!string.IsNullOrWhiteSpace(source.ApType))
                            {
                                //if the guid is missing, the exception would have thrown by now from code above.
                                if (!string.IsNullOrEmpty(apTypeGuid))
                                {
                                    accountsPayableInvoicesAccountDetail.Source = new GuidObject2(apTypeGuid);
                                }
                            }

                            if (!(string.IsNullOrWhiteSpace(lineItem.TaxFormCode)))
                            {
                                try
                                {
                                    var taxBoxeGuid = await _referenceDataRepository.GetBoxCodesGuidAsync(lineItem.TaxFormCode);
                                    if (!string.IsNullOrEmpty(taxBoxeGuid))
                                    {
                                        accountsPayableInvoicesAccountDetail.TaxFormComponent = new GuidObject2(taxBoxeGuid);
                                    }
                                }
                                catch (RepositoryException ex)
                                {
                                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                        source.Guid, source.Id);
                                }

                            }

                            if (!string.IsNullOrEmpty(source.VoucherRequestor))
                            {
                                if (personGuidCollection == null)
                                {
                                    IntegrationApiExceptionAddError(
                                    string.Format("Missing GUID for account details submitted by id '{0}': ", source.VoucherRequestor), "GUID.Not.Found",
                                    source.Guid, source.Id);
                                }
                                else
                                {
                                    var personGuid = string.Empty;
                                    if (personGuidCollection.TryGetValue(source.VoucherRequestor, out personGuid))
                                    {
                                        accountsPayableInvoicesAccountDetail.SubmittedBy = new GuidObject2(personGuid);
                                    }
                                    else
                                    {
                                        IntegrationApiExceptionAddError(
                                            string.Format("Missing GUID for account details submitted by id '{0}': ", source.VoucherRequestor), "GUID.Not.Found",
                                            source.Guid, source.Id);
                                    }
                                }
                            }
                            accountsPayableInvoicesLineItemAccountDetails.Add(accountsPayableInvoicesAccountDetail);
                        }
                    }
                    if (accountsPayableInvoicesLineItemAccountDetails.Any())
                        accountsPayableInvoicesLineItem.AccountDetails = accountsPayableInvoicesLineItemAccountDetails;

                    accountsPayableInvoicesLineItems.Add(accountsPayableInvoicesLineItem);

                }
                if (accountsPayableInvoicesLineItems.Any())
                    accountsPayableInvoices.LineItems = accountsPayableInvoicesLineItems;
            }

            return accountsPayableInvoices;
        }

        /// <summary>
        /// Build a AddressPlaces DTO object from an AccountsPayableInvoices entity
        /// </summary>
        /// <param name="source">AccountsPayableInvoices entity object</param>
        /// <returns>An AddressPlace object <see cref="Dtos.AddressPlace"/> in EEDM format</returns>
        private async Task<Dtos.AddressPlace> BuildAddressPlaceDtoAsync(Domain.ColleagueFinance.Entities.AccountsPayableInvoices source, bool bypassCache = false)
        {
            var addressDto = new Dtos.AddressPlace();
            Dtos.AddressCountry addressCountry = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;

            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(source.VoucherMiscCountry))
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.VoucherMiscCountry);
            else
            {
                if (!string.IsNullOrEmpty(source.VoucherMiscState))
                {
                    //var states = (await _referenceDataRepository.GetStateCodesAsync()).FirstOrDefault(x => x.Code == address.State);
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.VoucherMiscState);
                    if (states != null)
                    {
                        if (!string.IsNullOrEmpty(states.CountryCode))
                        {
                            country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == states.CountryCode);
                        }
                    }
                }
                if (country == null)
                {
                    var hostCountry = await GetHostCountryAsync();
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(source.VoucherMiscCountry))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + source.VoucherMiscCountry + " for voucher Id '" + source.Id + "'");
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + (await GetHostCountryAsync())  +" for voucher Id '" + source.Id + "'");
            }
            //need to check to make sure ISO code is there.
            if (string.IsNullOrEmpty(country.IsoAlpha3Code))
                throw new ArgumentException("Unable to locate ISO country code for " + country.Code + " for voucher Id '" + source.Id + "'");

            switch (country.IsoAlpha3Code)
            {
                case "USA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.USA;
                    addressCountry.PostalTitle = "UNITED STATES OF AMERICA";
                    break;
                case "CAN":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.CAN;
                    addressCountry.PostalTitle = "CANADA";
                    break;
                case "AUS":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.AUS;
                    addressCountry.PostalTitle = "AUSTRALIA";
                    break;
                case "BRA":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.BRA;
                    addressCountry.PostalTitle = "BRAZIL";
                    break;
                case "MEX":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.MEX;
                    addressCountry.PostalTitle = "MEXICO";
                    break;
                case "NLD":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.NLD;
                    addressCountry.PostalTitle = "NETHERLANDS";
                    break;
                case "GBR":
                    addressCountry.Code = Dtos.EnumProperties.IsoCode.GBR;
                    addressCountry.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                    break;
                default:
                    try
                    {
                        addressCountry.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                    }
                    catch (Exception ex)
                    {
                        throw new ColleagueWebApiException(string.Concat(ex.Message, "For the Country: '", source.VoucherMiscCountry, "' .ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountry.PostalTitle = country.Description.ToUpper();
                    break;
            }

            if (!string.IsNullOrEmpty(source.VoucherMiscState))
            {
                //var states = (await _referenceDataRepository.GetStateCodesAsync()).FirstOrDefault(x => x.Code == address.State);
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == source.VoucherMiscState);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
            }
            if (region != null)
            {
                addressCountry.Region = region;
            }
            if (!string.IsNullOrEmpty(source.VoucherMiscCity))
            {
                addressCountry.Locality = source.VoucherMiscCity;
            }

            addressCountry.PostalCode = (string.IsNullOrEmpty(source.VoucherMiscZip)) 
                ? null : source.VoucherMiscZip;
            
            if (country != null)
                addressCountry.Title = country.Description;

            if ((!string.IsNullOrEmpty(addressCountry.Locality)
                || !string.IsNullOrEmpty(addressCountry.PostalCode)
                || addressCountry.Region != null
                || addressCountry.SubRegion != null
                || !string.IsNullOrEmpty(source.VoucherMiscCountry)))
            {
                addressDto = new Dtos.AddressPlace() { Country = addressCountry };
            }
            return addressDto;
        }

        #endregion

        /// <summary>
        /// Get all CommerceTaxCode Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.Base.Entities.CommerceTaxCode>> GetAllCommerceTaxCodesAsync(bool bypassCache)
        {
            return _commerceTaxCodes ?? (_commerceTaxCodes = await _referenceDataRepository.GetCommerceTaxCodesAsync(bypassCache));
        }


        /// <summary>
        /// Get all CommerceTaxCodeRates Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.Base.Entities.CommerceTaxCodeRate>> GetAllCommerceTaxCodeRatesAsync(bool bypassCache)
        {
            return _commerceTaxCodeRates ?? (_commerceTaxCodeRates = await _referenceDataRepository.GetCommerceTaxCodeRatesAsync(bypassCache));
        }
        /// <summary>
        /// Get all AccountsPayableSources Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources>> GetAllAccountsPayableSourcesAsync(bool bypassCache)
        {
            return _accountsPayableSources ?? (_accountsPayableSources = await _colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache));
        }

        /// <summary>
        /// Get all  tax-form-components Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<BoxCodes>> GetAllBoxCodesAsync(bool bypassCache)
        {
            return _boxCodes ?? (_boxCodes = await _referenceDataRepository.GetAllBoxCodesAsync(bypassCache));
        }

        /// <summary>
        /// Get all CommodityCode Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode>> GetAllCommodityCodesAsync(bool bypassCache)
        {
            return _commodityCodes ?? (_commodityCodes = await _colleagueFinanceReferenceDataRepository.GetCommodityCodesAsync(bypassCache));
        }

        /// Get all FixedAssetDesignations Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="FixedAssetDesignations"> FixedAssetDesignationsFixedAssetDesignations entity objects</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FxaTransferFlags>> GetFixedAssetDesignationsAsync(bool bypassCache)
        {
            if (_fxaTransferFlags == null)
            {
                _fxaTransferFlags = await _colleagueFinanceReferenceDataRepository.GetFxaTransferFlagsAsync(bypassCache);
            }
            return _fxaTransferFlags;
        }

        /// <summary>
        /// Get all CommodityUnitType Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType>> GetAllCommodityUnitTypesAsync(bool bypassCache)
        {
            return _commodityUnitTypes ?? (_commodityUnitTypes = await _colleagueFinanceReferenceDataRepository.GetCommodityUnitTypesAsync(bypassCache));
        }

        /// <summary>
        /// Get all VendorTerms Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm>> GetAllVendorTermsAsync(bool bypassCache)
        {
            return _vendorTerms ?? (_vendorTerms = await _colleagueFinanceReferenceDataRepository.GetVendorTermsAsync(bypassCache));
        }

        /// <summary>
        /// Get GeneralLedgerAccountStructureEntity domain object
        /// </summary>
        /// <returns>GeneralLedgerAccountStructureEntity domain object</returns>
        private async Task<Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure> GetGeneralLedgerAccountStructure()
        {
            return _glAccountStructure ?? (_glAccountStructure = await _generalLedgerConfigurationRepository.GetAccountStructureAsync());
        }

        /// <summary>
        /// Get Host Country
        /// </summary>
        /// <returns>HostCountry</returns>
        private async Task<string> GetHostCountryAsync()
        {
            return _hostCountry ?? (_hostCountry = await _addressRepository.GetHostCountryAsync()); 
        }

        /// <summary>
        /// Check the GL's amounts and make sure they have funds avialable before proceeding.
        /// </summary>
        /// <param name="api"></param>
        /// <param name="ApiId"></param>
        /// <returns></returns>
        private async Task<List<Domain.ColleagueFinance.Entities.FundsAvailable>> checkFunds2(AccountsPayableInvoices2 api, string ApiId = "")
        {
            var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            //check if Accounting string has funds
            int lineCount = 0;
            var docSubmittedById = string.Empty;
            if (api.SubmittedBy != null && !string.IsNullOrEmpty(api.SubmittedBy.Id))
                docSubmittedById = await _personRepository.GetPersonIdFromGuidAsync(api.SubmittedBy.Id);
            else
                docSubmittedById = CurrentUser.PersonId;
            var budgetOvrCheckTuple = new List<Tuple<string, bool>>();
            foreach (var lineItems in api.LineItems)
            {
                int detailCount = 0;
                decimal? budgetAmount = 0;
                lineCount++;
                List<string> accountingStringList = new List<string>();
                foreach (var details in lineItems.AccountDetails)
                {
                    detailCount++;
                    var submittedBy = (details.SubmittedBy != null) ? details.SubmittedBy.Id :
                                    (api.SubmittedBy != null) ? api.SubmittedBy.Id : "";
                    var submittedById = (!string.IsNullOrEmpty(submittedBy)) ? await _personRepository.GetPersonIdFromGuidAsync(submittedBy) : "";

                    if (details.Allocation != null && details.Allocation.Allocated != null &&
                            details.Allocation.Allocated.Amount != null && details.Allocation.Allocated.Amount.Value != null
                            && details.Allocation.Allocated.Amount.Value.HasValue)
                    {
                        budgetAmount = details.Allocation.Allocated.Amount.Value.Value;
                    }
                    //if there is no allocated.amount, we see if there is percent and we calculate the amount
                    else if (details.Allocation != null && details.Allocation.Allocated != null &&
                            details.Allocation.Allocated.Percentage != null && details.Allocation.Allocated.Percentage.HasValue && lineItems.UnitPrice != null && lineItems.UnitPrice.Value != null && lineItems.UnitPrice.Value.HasValue)
                    {
                        budgetAmount = lineItems.UnitPrice.Value.Value * details.Allocation.Allocated.Percentage / 100;
                    }
                    // if there is no allocated amount or percentage, then use the quantity to calculate the amount for budget check
                    else if (details.Allocation != null && details.Allocation.Allocated != null &&
                            details.Allocation.Allocated.Quantity != null && details.Allocation.Allocated.Quantity.HasValue && lineItems.UnitPrice != null && lineItems.UnitPrice.Value != null && lineItems.UnitPrice.Value.HasValue && lineItems.Quantity != null && lineItems.Quantity.HasValue)
                    {
                        budgetAmount = lineItems.UnitPrice.Value.Value * details.Allocation.Allocated.Quantity / lineItems.Quantity;
                    }
                    if (budgetAmount != null && budgetAmount.HasValue && lineItems.UnitPrice != null && lineItems.UnitPrice.Currency != null)
                    {
                        string PosID = lineCount.ToString() + "." + detailCount.ToString();
                        if (details.SubmittedBy != null)
                            PosID = PosID + ".DS";
                        var budgetCheckOverrideFlag = (details.BudgetCheck == AccountsPayableInvoicesAccountBudgetCheck.Override) ? true : false;
                        budgetOvrCheckTuple.Add(new Tuple<string, bool>(details.AccountingString, budgetCheckOverrideFlag));
                        fundsAvailable.Add(new Domain.ColleagueFinance.Entities.FundsAvailable(details.AccountingString)
                        {
                            Sequence = PosID,
                            SubmittedBy = submittedById,
                            Amount = (decimal)budgetAmount,
                            ItemId = lineItems.LineItemNumber ?? lineItems.LineItemNumber,
                            TransactionDate = api.TransactionDate,
                            CurrencyCode = lineItems.UnitPrice.Currency.ToString()
                        });
                    }
                    

                    var accountingString = accountingStringList.Find(x => x.Equals(details.AccountingString));
                    if (string.IsNullOrWhiteSpace(accountingString))
                    {
                        accountingStringList.Add(details.AccountingString);
                    }
                    else
                    {
                        throw new ColleagueWebApiException("A line item has two account details with the same GL number " + accountingString + " this is not allowed");
                    }
                }
            }
            if (fundsAvailable != null && fundsAvailable.Any())
            {
                if (string.IsNullOrEmpty(ApiId))
                {
                    ApiId = "NEW";
                }

                var availableFunds = await _accountFundAvailableRepository.CheckAvailableFundsAsync(fundsAvailable, "", ApiId,"", docSubmittedById);
                if (availableFunds != null)
                {
                    //introducing a new var to concat all the error messages from budget check process so that rather than just showing the first error message, we will show them all
                    string budgetMessages = string.Empty;
                    foreach (var availableFund in availableFunds)
                    {
                        if (availableFund.AvailableStatus == FundsAvailableStatus.NotAvailable)
                        {
                            budgetMessages = string.Concat(budgetMessages, string.Format("The accounting string " + availableFund.AccountString + " does not have funds available. {0}. ", availableFund.OverrideMessage));
                        }
                        //if we get a override and if the budgetcheck flag is not set to override then thrown an exception
                        if (availableFund.AvailableStatus == FundsAvailableStatus.Override)
                        {
                            var budOverCheck = budgetOvrCheckTuple.FirstOrDefault(acct => acct.Item1 == availableFund.AccountString);
                            if (budOverCheck != null && budOverCheck.Item2 == false)
                            {
                                budgetMessages = string.Concat(budgetMessages, string.Format("The accounting string " + availableFund.AccountString + " does not have funds available. BudgetCheck flag not set to override. {0}. ", availableFund.OverrideMessage));
                            }
                            else
                            {
                                overrideAvailable.Add(availableFund);
                            }
                        }                  
                    }
                    if (!string.IsNullOrEmpty(budgetMessages))
                    {
                        throw new ArgumentException(budgetMessages);
                    }
                }
            }

            return overrideAvailable;
        }

        /// <summary>
        /// Helper method to validate Accounts Payable Invoices PUT/POST.
        /// </summary>
        /// <param name="accountsPayableInvoices">accounts Payable Invoices DTO object of type <see cref="Dtos.AccountsPayableInvoices2"/></param>
        /// 
        private void ValidateAccountsPayableInvoices2(Dtos.AccountsPayableInvoices2 accountsPayableInvoices)
        {
            if (accountsPayableInvoices.Vendor == null)
            {
                throw new ArgumentNullException("accountsPayableInvoices.vendor", "The vendor is required when submitting an accounts payable invoice. ");
            }

            if ((accountsPayableInvoices.TransactionDate == DateTime.MinValue))
            {
                throw new ArgumentNullException("accountsPayableInvoices.TransactionDate", "The transactionDate is a required field");
            }

            var existingVendor = (accountsPayableInvoices.Vendor != null && accountsPayableInvoices.Vendor.ExistingVendor != null) ?
                accountsPayableInvoices.Vendor.ExistingVendor : null;
            var manualVendor = (accountsPayableInvoices.Vendor != null && accountsPayableInvoices.Vendor.ManualVendorDetails != null) ?
                accountsPayableInvoices.Vendor.ManualVendorDetails : null;

            if ((existingVendor == null || existingVendor.Vendor == null || string.IsNullOrEmpty(existingVendor.Vendor.Id)) &&
                (manualVendor == null || string.IsNullOrEmpty(manualVendor.Name)))
                throw new ArgumentNullException("accountsPayableInvoices", "Must provide either existing vendor or manual vendor details for accountsPayableInvoices. ");

            if ((manualVendor != null) && (manualVendor.Place != null))
            {
                if (manualVendor.Place.Country == null)
                {
                    throw new ArgumentNullException("accountsPayableInvoices", "When providing a manual vendor detail with a place, then the country is required.");
                }
                if ( manualVendor.Place.Country.Code == null)
                {
                    throw new ArgumentNullException("accountsPayableInvoices", "When providing a manual vendor detail with a country, then the code is required.");
                }
            }


            var defaultCurrency = new CurrencyIsoCode?();
            
            if (accountsPayableInvoices.ProcessState == null || accountsPayableInvoices.ProcessState == AccountsPayableInvoicesProcessState.NotSet)
            {
                throw new ArgumentNullException("accountsPayableInvoices.ProcessState", "The processState is required when submitting an accounts payable invoice. ");
            }
            if (accountsPayableInvoices.PaymentStatus == null || accountsPayableInvoices.PaymentStatus == AccountsPayableInvoicesPaymentStatus.NotSet)
            {
                throw new ArgumentNullException("accountsPayableInvoices.PaymentStatus", "The paymentStatus is required when submitting an accounts payable invoice. ");
            }
            if (accountsPayableInvoices.Payment == null)
            {
                throw new ArgumentNullException("accountsPayableInvoices.Payment", "The payment is required when submitting an accounts payable invoice. ");
            }
            if (accountsPayableInvoices.Payment != null)
            {
                if (!accountsPayableInvoices.Payment.PaymentDueOn.HasValue)
                {
                    throw new ArgumentNullException("accountsPayableInvoices.Payment", "The paymentDueOn are required when submitting an accounts payable invoice payment. ");
                }
                if (accountsPayableInvoices.Payment.Source != null && string.IsNullOrEmpty(accountsPayableInvoices.Payment.Source.Id))
                {
                    throw new ArgumentNullException("accountsPayableInvoices.Payment.Source", "The source id is required when submitting a Payment Source. ");
                }
                if (accountsPayableInvoices.Payment.PaymentTerms != null && string.IsNullOrEmpty(accountsPayableInvoices.Payment.PaymentTerms.Id))
                {
                    throw new ArgumentNullException("accountsPayableInvoices.Payment.PaymentTerms", "The payment term id is required when submitting a Payment Term. ");
                }
            }
            if (accountsPayableInvoices.VendorBilledAmount != null && (!accountsPayableInvoices.VendorBilledAmount.Value.HasValue || accountsPayableInvoices.VendorBilledAmount.Currency == null))
            {
                throw new ArgumentNullException("accountsPayableInvoices.VendorBilledAmount", "The vendor billed amount value and currency are required when submitting a vendor billed amount. ");
            }
            if (accountsPayableInvoices.VendorBilledAmount != null) { defaultCurrency = checkCurrency(defaultCurrency, accountsPayableInvoices.VendorBilledAmount.Currency); }

           
            if (accountsPayableInvoices.InvoiceDiscountAmount != null && (!accountsPayableInvoices.InvoiceDiscountAmount.Value.HasValue || accountsPayableInvoices.InvoiceDiscountAmount.Currency == null))
            {
                throw new ArgumentNullException("accountsPayableInvoices.InvoiceDiscountAmount", "The invoice discount amount value and currency are required when submitting a invoice discount amount. ");
            }

            if (accountsPayableInvoices.VoidDate != null && accountsPayableInvoices.VoidDate < accountsPayableInvoices.TransactionDate)
            {
                throw new ArgumentNullException("accountsPayableInvoices.VoidDate", "Void date cannot be before transaction date");
            }

            if (accountsPayableInvoices.InvoiceDiscountAmount != null)
            { defaultCurrency = checkCurrency(defaultCurrency, accountsPayableInvoices.InvoiceDiscountAmount.Currency); }
            if (accountsPayableInvoices.Taxes != null && accountsPayableInvoices.Taxes.Any())
            {
                foreach (var tax in accountsPayableInvoices.Taxes)
                {
                    if (tax.TaxCode != null && string.IsNullOrEmpty(tax.TaxCode.Id))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.Taxes.TaxCode", "The tax code id is required when submitting Taxes. ");
                    }
                    if (tax.VendorAmount != null && (!tax.VendorAmount.Value.HasValue || tax.VendorAmount.Currency == null))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.Taxes.VendorAmount.", "The vendor amount value and currency are required when submitting a taxes vendor amount. ");
                    }
                    if (tax.VendorAmount != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, tax.VendorAmount.Currency);
                    }
                    if (tax.TaxCodeRates != null && tax.TaxCodeRates.Any())
                    {
                        foreach (var TaxRate in tax.TaxCodeRates)
                        {
                            if (TaxRate.Rate == null)
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.taxCodeRates.Rate", "The taxCodeRates.rate is required when submitting a line item with taxCodeRates.");
                            }
                            if (TaxRate.Rate != null && string.IsNullOrEmpty(TaxRate.Rate.Id))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.taxCodeRates.Rate", "The taxCodeRates.rate.id is required when submitting a line item with taxCodeRates. ");
                            }
                            if (TaxRate.Rate != null && (!TaxRate.Amount.Value.HasValue || TaxRate.Amount.Currency == null))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.taxCodeRates.Vmount", "The amount value and currency are required when submitting a line item taxCodeRate amount. ");
                            }
                            if (TaxRate.Amount != null)
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, TaxRate.Amount.Currency);
                            }
                        }
                    }
                    
                }
            }
            if (accountsPayableInvoices.LineItems == null || !accountsPayableInvoices.LineItems.Any())
            {
                throw new ArgumentNullException("accountsPayableInvoices.LineItems.", "At least one line item must be provided when submitting an accounts-payable-invoice. ");
            }

            if (accountsPayableInvoices.LineItems != null && accountsPayableInvoices.LineItems.Any())
            {
                foreach (var lineItem in accountsPayableInvoices.LineItems)
                {
                    if (lineItem.CommodityCode != null && string.IsNullOrEmpty(lineItem.CommodityCode.Id))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.CommodityCode", "The commodity code id is required when submitting a commodity code. ");
                    }
                    if (lineItem.FixedAssetDesignation != null && string.IsNullOrEmpty(lineItem.FixedAssetDesignation.Id))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.FixedAssetDesignation", "The fixedAssetDesignation id is required when submitting a fixedAssetDesignation. ");
                    }
                    if (lineItem.UnitofMeasure != null && string.IsNullOrEmpty(lineItem.UnitofMeasure.Id))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.UnitofMeasure", "The UnitofMeasure id is required when submitting a UnitofMeasure. ");
                    }
                    if (lineItem.UnitPrice != null && (!lineItem.UnitPrice.Value.HasValue || lineItem.UnitPrice.Currency == null))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.Taxes.UnitPrice.", "Both the unit price amount value and currency are required when submitting a line item unit price. ");
                    }
                    if (lineItem.ReferenceDocument != null)
                    {
                        if (lineItem.ReferenceDocument.PurchasingArrangement != null && !string.IsNullOrEmpty(lineItem.ReferenceDocument.PurchasingArrangement.Id))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.ReferenceDocument.PurchasingArrangement", "ReferenceDocument.PurchasingArrangement is not supported. ");
                        }
                        if (!string.IsNullOrEmpty(lineItem.ReferenceDocument.RecurringVoucher))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.ReferenceDocument.RecurringVoucher", "ReferenceDocument.RecurringVoucher is not supported. ");
                        }
                       
                    }
                   
                    if (lineItem.UnitPrice != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, lineItem.UnitPrice.Currency);
                    }
                    if (lineItem.VendorBilledUnitPrice != null && (!lineItem.VendorBilledUnitPrice.Value.HasValue || lineItem.VendorBilledUnitPrice.Currency == null))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.VendorBilledUnitPrice.UnitPrice.", "The vendor billed unit price amount value and currency are required when submitting a line item vendor billed unit prioe. ");
                    }
                    if (lineItem.VendorBilledUnitPrice != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, lineItem.VendorBilledUnitPrice.Currency);
                    }
                    if (lineItem.AdditionalAmount != null && (!lineItem.AdditionalAmount.Value.HasValue || lineItem.AdditionalAmount.Currency == null))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.AdditionalAmount.UnitPrice", "The additional amount value and currency are required when submitting a line item additional price. ");
                    }
                    if (lineItem.AdditionalAmount != null)
                    {
                        defaultCurrency = checkCurrency(defaultCurrency, lineItem.AdditionalAmount.Currency);
                    }
                    if (lineItem.Taxes != null && lineItem.Taxes.Any())
                    {
                        foreach (var lineItemTaxes in lineItem.Taxes)
                        {
                            if (lineItemTaxes.TaxCode == null)
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.TaxCode", "The Taxes.TaxCode is required when submitting a line item Tax Code. ");
                            }
                            if (lineItemTaxes.TaxCode != null && string.IsNullOrEmpty(lineItemTaxes.TaxCode.Id))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.TaxCode", "The Taxes.TaxCode id is required when submitting a line item Tax Code. ");
                            }
                            if (lineItemTaxes.VendorAmount != null && (!lineItemTaxes.VendorAmount.Value.HasValue || lineItemTaxes.VendorAmount.Currency == null))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.Taxes.VendorAmount", "The VendorAmount value and currency are required when submitting a line item tax vendorAmount. ");
                            }
                            if (lineItemTaxes.VendorAmount != null)
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, lineItemTaxes.VendorAmount.Currency);
                            }
                        }
                    }

                    if (lineItem.Discount != null)
                    {
                        if (lineItem.Discount.Amount != null && (!lineItem.Discount.Amount.Value.HasValue || lineItem.Discount.Amount.Currency == null))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.Discount.Amount", "The discount amount value and currency are required when submitting a line item discount amount. ");
                        }
                        if (lineItem.Discount.Amount != null)
                        {
                            defaultCurrency = checkCurrency(defaultCurrency, lineItem.Discount.Amount.Currency);
                        }
                        if ((lineItem.Discount.Amount == null) &&  (!lineItem.Discount.Percent.HasValue))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.Discount", "Either discount amount or percentage are required when submitting a line item discount amount. ");
                        }
                        if ((lineItem.Discount.Amount != null) && (lineItem.Discount.Percent.HasValue))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.Discount", "Either discount amount or percentage are required when submitting a line item discount amount. Can not provide both.");
                        }
                        if ((lineItem.Discount.Amount == null) && (lineItem.Discount.Percent.HasValue) && (lineItem.Discount.Percent < 0 || lineItem.Discount.Percent > 100))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.Discount", "The discount percent should be a number greater than zero and less than or equal to 100.");
                        }
                        if (lineItem.Discount.Amount != null && lineItem.Discount.Amount.Value.HasValue && lineItem.Discount.Amount.Value < 0)
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.Discount.Amount", "The discount amount value cannot be negative. ");
                        }

                    }
                    if (lineItem.AccountDetails == null)
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.AccountDetails", "The accountDetails are required when submitting a line item. ");
                    }
                    var lineTaxForm = string.Empty;
                    foreach (var accountDetail in lineItem.AccountDetails)
                    {
                        if (accountDetail.Allocation != null)
                        {
                            var allocation = accountDetail.Allocation;
                            if (allocation.Allocated != null)
                            {
                                var allocated = allocation.Allocated;
                                if (allocated.Amount != null && (!allocated.Amount.Value.HasValue || allocated.Amount.Currency == null))
                                {
                                    throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetail.Allocation.Allocated",
                                        "The Allocation.Allocated value and currency are required when submitting a line item AccountDetail.Allocation.Allocated. ");
                                }
                                if (allocated.Amount != null)
                                {
                                    defaultCurrency = checkCurrency(defaultCurrency, allocated.Amount.Currency);
                                }
                            }
                            else
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.allocation.allocated", "The allocated is required when submitting a line item account detail. ");
                            }
                            if (allocation.TaxAmount != null && (!allocation.TaxAmount.Value.HasValue || allocation.TaxAmount.Currency == null))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetail.Allocation.TaxAmount",
                                    "The tax amount value and currency are required when submitting a line item account detail allocation tax amount. ");
                            }
                            if (allocation.TaxAmount != null)
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, allocation.TaxAmount.Currency);
                            }
                            if (allocation.AdditionalAmount != null && (!allocation.AdditionalAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetail.Allocation.AdditionalAmount",
                                    "The additional amount value and currency are required when submitting a line item account detail allocation additional amount. ");
                            }
                            if (allocation.AdditionalAmount != null)
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, allocation.AdditionalAmount.Currency);
                            }
                            if (allocation.DiscountAmount != null && (!allocation.DiscountAmount.Value.HasValue || allocation.DiscountAmount.Currency == null))
                            {
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.DiscountAmount.Allocation.AdditionalAmount",
                                    "The discount amount value and currency are required when submitting a line item account detail allocation discount amount. ");
                            }
                            if (allocation.DiscountAmount != null)
                            {
                                defaultCurrency = checkCurrency(defaultCurrency, allocation.DiscountAmount.Currency);
                            }

                        }
                        if (accountDetail.Source != null && string.IsNullOrEmpty(accountDetail.Source.Id))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.Source", "The Source id is required when submitting a line item account detail source. ");
                        }
                        if (accountDetail.TaxFormComponent != null && string.IsNullOrEmpty(accountDetail.TaxFormComponent.Id))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.taxFormComponent", "The taxFormComponent id is required when submitting a line item account detail taxFormComponent.");
                        }

                        if (accountDetail.TaxFormComponent != null && !string.IsNullOrEmpty(accountDetail.TaxFormComponent.Id))
                        {
                            //check the guid in each account details to make sure they are the same.
                            if (string.IsNullOrEmpty(lineTaxForm)) // this is the first taxform
                                lineTaxForm = accountDetail.TaxFormComponent.Id;
                            else if (lineTaxForm != accountDetail.TaxFormComponent.Id)
                                throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.taxFormComponent", "The taxFormComponents must be identical for each account detail for the line item : " + lineItem.Description);
                        }

                        if (accountDetail.SubmittedBy != null && string.IsNullOrEmpty(accountDetail.SubmittedBy.Id))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.SubmittedBy", "The SubmittedBy id is required when submitting a line item account detail SubmittedBy. ");
                        }
                        if (string.IsNullOrEmpty(accountDetail.AccountingString))
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.AccountingString", "The AccountingString id is required when submitting a line item account detail AccountingString. ");
                        }
                        if (accountDetail.Allocation == null)
                        {
                            throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.Allocation", "The Allocation is required when submitting a line item account detail. ");
                        }
                        //we are using the API user for override check if submittedBy is not there. so this check is not needed.
                        //if (accountDetail.BudgetCheck == AccountsPayableInvoicesAccountBudgetCheck.Override && (accountDetail.SubmittedBy == null || string.IsNullOrEmpty(accountDetail.SubmittedBy.Id)))
                        //{
                        //    throw new ArgumentNullException("accountsPayableInvoices.LineItems.AccountDetails.SubmittedBy", "The SubmittedBy id is required when budget override is requested. ");
                        //}
                    }
                    
                    if (string.IsNullOrEmpty(lineItem.Description))
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.Description", "The Description is required when submitting a line item. ");
                    }
                    if (lineItem.Quantity != null && lineItem.Quantity.HasValue && lineItem.Quantity.Value <= 0)
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.Quantity", "The Quantity must be greater than zero when submitting a line item.");
                    }
                    if (lineItem.Quantity == null || !lineItem.Quantity.HasValue)
                    {
                        // Default the line item quantity to allocated quantity when it's not included with the PUT/POST request.
                        var totalQuantity = lineItem.AccountDetails.Sum(ad => ad.Allocation != null && ad.Allocation.Allocated != null && ad.Allocation.Allocated.Quantity.HasValue ? ad.Allocation.Allocated.Quantity : 1);
                        lineItem.Quantity = totalQuantity;                   
                    }
                    if (lineItem.UnitPrice == null)
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.UnitPrice", "The UnitPrice is required when submitting a line item. ");
                    }
                    if (lineItem.PaymentStatus == null)
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.PaymentStatus", "The PaymentStatus is required when submitting a line item. ");
                    }
                    if (lineItem.Status == null)
                    {
                        throw new ArgumentNullException("accountsPayableInvoices.LineItems.Status", "The Status is required when submitting a line item. ");
                    }
                }
            }
            //if one line item has BPO as reference doc then all of them should have it as well
            var bpoRefDoc = accountsPayableInvoices.LineItems.Where(line => line.ReferenceDocument != null && line.ReferenceDocument.BlanketPurchaseOrder != null && !string.IsNullOrEmpty(line.ReferenceDocument.BlanketPurchaseOrder.Id));
            if (bpoRefDoc != null && bpoRefDoc.Any())
            {
                if (bpoRefDoc.Count() != accountsPayableInvoices.LineItems.Count)
                {
                    throw new ArgumentNullException("accountsPayableInvoices.LineItems.ReferenceDocument", "All line items in the voucher need to be related to Blanket Purchase Order when paying a Blanket Purchase Order. ");
                }
            }
        }

        private CurrencyIsoCode? checkCurrency(CurrencyIsoCode? defaultValue, CurrencyIsoCode? newValue)
        {
            if (defaultValue != null && defaultValue != newValue && newValue != null)
            {
                throw new ArgumentException("accountsPayableInvoices.Currency", "All currency codes in the request must be the same and cannot differ.");
            }
            CurrencyIsoCode? cc = newValue == null ? defaultValue : newValue;
            return cc;
        }

       private string ConvertAccountingString(int GLCompCount, string accountingString)
        {
            if (string.IsNullOrEmpty(accountingString))
                return string.Empty;

            //if GL number contains an Astrisks and if there more then one we are
            // assuming that the delimiter is an astrisk, So we'll convert that delimiter to something
            // else while perserving the project code if present.

            if ((accountingString.Split('*').Length - 1) > 1)
            {
                int CountAstrisks = accountingString.Split('*').Length - 1;
                if ((GLCompCount - 1) < CountAstrisks)
                {
                    int lastIndex = accountingString.LastIndexOf('*');
                    accountingString = accountingString.Substring(0, lastIndex).Replace("*", "")
                          + accountingString.Substring(lastIndex);
                }
                else
                {
                    accountingString = Regex.Replace(accountingString, "[^0-9a-zA-Z]", "");
                }
            }
            var glsplit = accountingString.Split('*');
            accountingString = glsplit[0].Replace("-", "_");

            var tempAccountingString = glsplit[0].Replace("_", "");
            if (tempAccountingString.Length <= 14) { accountingString = tempAccountingString; }

            if (glsplit.Count() > 1)
            {
                accountingString = ConvertAccountingStringWithProjectRefNoToProjectId(glsplit[1], accountingString);
            }

            return accountingString;
        }

        /// <summary>
        /// Gets accounting string with project ref no.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="accountingString"></param>
        /// <returns></returns>
        private string ConvertAccountingstringToIncludeProjectRefNo(string projectId, string accountingString)
        {
            if (_projectReferenceIds != null && _projectReferenceIds.Any())
            {
                var projectRefId = string.Empty;
                if (_projectReferenceIds.TryGetValue(projectId, out projectRefId))
                {
                    return string.Concat(accountingString, "*", projectRefId);
                }
            }
            return accountingString;
        }

        /// <summary>
        /// Gets accounting string with project id.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="accountingString"></param>
        /// <returns></returns>
        private string ConvertAccountingStringWithProjectRefNoToProjectId(string projectRefNo, string accountingString)
        {
            if (_projectReferenceIds != null && _projectReferenceIds.Any())
            {
                var projectId = string.Empty;
                projectId = _projectReferenceIds.FirstOrDefault(x => x.Value == projectRefNo).Key;
                {
                    return string.Concat(accountingString, "*", projectId);
                }

            }
            return accountingString;
        }

        private string GetFormattedGlAccount(string accountNumber, GeneralLedgerAccountStructure GlConfig)
        {
            string formattedGlAccount = string.Empty;
            string tempGlNo = string.Empty;
            formattedGlAccount = Regex.Replace(accountNumber, "[^0-9a-zA-Z]", "");

            int startLoc = 0;
            int x = 0, glCount = GlConfig.MajorComponents.Count;

            foreach (var glMajor in GlConfig.MajorComponents)
            {
                try
                {
                    x++;
                    if (x < glCount) { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength) + GlConfig.glDelimiter; }
                    else { tempGlNo = tempGlNo + formattedGlAccount.Substring(startLoc, glMajor.ComponentLength); }
                    startLoc += glMajor.ComponentLength;
                }
                catch (ArgumentOutOfRangeException aex)
                {
                    throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", accountNumber));
                }
            }
            formattedGlAccount = tempGlNo;

            return formattedGlAccount;
        }
    }
}