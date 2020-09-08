// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IPurchaseOrderService interface
    /// </summary>
    [RegisterType]
    public class PurchaseOrderService : BaseCoordinationService, IPurchaseOrderService
    {
        private IPurchaseOrderRepository purchaseOrderRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository;
        private IBuyerRepository buyerRepository;
        private IReferenceDataRepository referenceDataRepository;
        private readonly IVendorsRepository vendorsRepository;
        private readonly IPersonRepository personRepository;
        private readonly IConfigurationRepository configurationRepository;
        private readonly IAccountFundsAvailableRepository accountFundAvailableRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;
        IDictionary<string, string> _projectReferenceIds = null;
        private IStaffRepository staffRepository;
        private IProcurementsUtilityService procurementsUtilityService;

        // Constructor to initialize the private attributes
        public PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IBuyerRepository buyerRepository,
            IVendorsRepository vendorsRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IAccountFundsAvailableRepository accountFundAvailableRepository,
            IPersonRepository personRepository,
            IRoleRepository roleRepository,
            IStaffRepository staffRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IProcurementsUtilityService procurementsUtilityService,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.purchaseOrderRepository = purchaseOrderRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.vendorsRepository = vendorsRepository;
            this.buyerRepository = buyerRepository;
            this.accountFundAvailableRepository = accountFundAvailableRepository;
            this.personRepository = personRepository;
            this.configurationRepository = configurationRepository;
            this.staffRepository = staffRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
            this.procurementsUtilityService = procurementsUtilityService;
        }


        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> _commodityUnitType = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType>> GetCommodityUnitTypesAsync(bool bypassCache)
        {
            if (_commodityUnitType == null)
            {
                _commodityUnitType = await colleagueFinanceReferenceDataRepository.GetCommodityUnitTypesAsync(bypassCache);
            }
            return _commodityUnitType;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> _vendorTerm = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm>> GetVendorTermsAsync(bool bypassCache)
        {
            if (_vendorTerm == null)
            {
                _vendorTerm = await colleagueFinanceReferenceDataRepository.GetVendorTermsAsync(bypassCache);
            }
            return _vendorTerm;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> _accountsPayableSources;

        /// <summary>
        /// Get all AccountsPayableSources Entity Objects
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources>> GetAllAccountsPayableSourcesAsync(bool bypassCache)
        {
            //await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(false);
            return _accountsPayableSources ?? (_accountsPayableSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache));
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType> _freeOnBoardType = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool bypassCache)
        {
            if (_freeOnBoardType == null)
            {
                _freeOnBoardType = await colleagueFinanceReferenceDataRepository.GetFreeOnBoardTypesAsync(bypassCache);
            }
            return _freeOnBoardType;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod> _ShippingMethods = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod>> GetShippingMethodsAsync(bool bypassCache)
        {
            if (_ShippingMethods == null)
            {
                _ShippingMethods = await colleagueFinanceReferenceDataRepository.GetShippingMethodsAsync(bypassCache);
            }
            return _ShippingMethods;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination> _shipToDestination = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination>> GetShipToDestinationsAsync(bool bypassCache)
        {
            if (_shipToDestination == null)
            {
                _shipToDestination = await colleagueFinanceReferenceDataRepository.GetShipToDestinationsAsync(bypassCache);
            }
            return _shipToDestination;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> _commodityCode = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode>> GetCommodityCodesAsync(bool bypassCache)
        {
            if (_commodityCode == null)
            {
                _commodityCode = await colleagueFinanceReferenceDataRepository.GetCommodityCodesAsync(bypassCache);
            }
            return _commodityCode;
        }

        private IEnumerable<Domain.Base.Entities.CommerceTaxCode> _commerceTaxCode = null;
        private async Task<IEnumerable<Domain.Base.Entities.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache)
        {
            if (_commerceTaxCode == null)
            {
                _commerceTaxCode = await referenceDataRepository.GetCommerceTaxCodesAsync(bypassCache);
            }
            return _commerceTaxCode;
        }

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<State> _states = null;
        private async Task<IEnumerable<State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }
        
        private IEnumerable<BoxCodes> _boxCodes = null;
        private async Task<IEnumerable<BoxCodes>> GetAllBoxCodesAsync(bool bypassCache)
        {
            if (_boxCodes == null)
            {
                _boxCodes = await referenceDataRepository.GetAllBoxCodesAsync(bypassCache);
            }
            return _boxCodes;
        }

        /// <summary>
        /// Returns the purchase order selected by the user
        /// </summary>
        /// <param name="id">ID for the requested purchase order</param>
        /// <returns><see cref="Dtos.ColleagueFinance.PurchaseOrder">Purchase Order DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrder> GetPurchaseOrderAsync(string id)
        {
            // Check the permission code to view a purchase order.
            CheckPurchaseOrderViewPermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // Get the purchase order domain entity from the repository
            var purchaseOrderDomainEntity = await purchaseOrderRepository.GetPurchaseOrderAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

            if (purchaseOrderDomainEntity == null)
            {
                throw new ArgumentNullException("purchaseOrderDomainEntity", "purchaseOrderDomainEntity cannot be null.");
            }

            await AssignGlDescription(glConfiguration, purchaseOrderDomainEntity);

            // Convert the purchase order and all its child objects into DTOs
            var purchaseOrderDtoAdapter = new PurchaseOrderEntityToDtoAdapter(_adapterRegistry, logger);
            var purchaseOrderDto = purchaseOrderDtoAdapter.MapToType(purchaseOrderDomainEntity, glConfiguration.MajorComponentStartPositions);

            if (purchaseOrderDto.LineItems == null || purchaseOrderDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access purchase order.");
            }

            return purchaseOrderDto;
        }

        private async Task AssignGlDescription(GeneralLedgerAccountStructure glAccountStructure, PurchaseOrder requisitionDomainEntity)
        {
            List<string> glAccountNumbers = new List<string>();
            foreach (var lineItem in requisitionDomainEntity.LineItems)
            {
                foreach (var glDististribution in lineItem.GlDistributions)
                {
                    if (!glDististribution.Masked)
                    {
                        glAccountNumbers.Add(glDististribution.GlAccountNumber);
                    }
                }
            }
            if (glAccountNumbers.Any())
            {
                var glAccountDescriptionsDictionary = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(glAccountNumbers, glAccountStructure);
                foreach (var lineItem in requisitionDomainEntity.LineItems)
                {
                    foreach (var glDististribution in lineItem.GlDistributions)
                    {
                        if (!glDististribution.Masked)
                        {
                            string description = "";
                            if (!string.IsNullOrEmpty(glDististribution.GlAccountNumber))
                            {
                                glAccountDescriptionsDictionary.TryGetValue(glDististribution.GlAccountNumber, out description);
                            }
                            glDististribution.GlAccountDescription = description;
                        }
                    }
                }
            }
        }

        #region EEDM Purchase orders V11

        #region EEDM Get/GET ALL V11

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get Purchase Orders data.
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>List of <see cref="Dtos.PurchaseOrders">PurchaseOrders</see></returns>
        public async Task<Tuple<IEnumerable<PurchaseOrders2>, int>> GetPurchaseOrdersAsync(int offset, int limit, PurchaseOrders2 criteriaObject, bool bypassCache = false)
        {
            CheckViewPurchaseOrderPermission();

            var purchaseOrdersCollection = new List<Ellucian.Colleague.Dtos.PurchaseOrders2>();
            GeneralLedgerAccountStructure glConfiguration = null;

            try
            {
                glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError(e.Message, "Bad.Data");
            }

            Tuple<IEnumerable<PurchaseOrder>, int> purchaseOrderEntities = null;

            try
            {

                purchaseOrderEntities = await purchaseOrderRepository.GetPurchaseOrdersAsync(offset, limit, criteriaObject.OrderNumber);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError(e.Message, "Bad.Data");
            }

            if ((purchaseOrderEntities == null) || (purchaseOrderEntities.Item1 != null))
            {
                new Tuple<IEnumerable<Dtos.PurchaseOrders2>, int>(purchaseOrdersCollection, 0);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }


            var totalRecords = purchaseOrderEntities.Item2;


            var projectIds = purchaseOrderEntities.Item1
                         .SelectMany(t => t.LineItems)
                         .Where(i => i.GlDistributions != null)
                         .SelectMany(p => p.GlDistributions)
                         .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                         .Select(pj => pj.ProjectId)
                         .ToList()
                         .Distinct();
            if (projectIds != null && projectIds.Any())
            {
                _projectReferenceIds = await purchaseOrderRepository.GetProjectReferenceIds(projectIds.ToArray());
            }

            foreach (var purchaseOrderEntity in purchaseOrderEntities.Item1)
            {
                try
                {
                    if (purchaseOrderEntity.Guid != null)
                    {
                        var accountsPayableInvoiceDto = await this.ConvertPurchaseOrderEntityToDtoAsync(purchaseOrderEntity, glConfiguration, bypassCache);
                        purchaseOrdersCollection.Add(accountsPayableInvoiceDto);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Concat(ex.Message, "  Purchase Order: '", purchaseOrderEntity.Number, "', Entity: 'PURCHASE.ORDERS'"), "Bad.Data", purchaseOrderEntity.Guid, purchaseOrderEntity.Id);
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.PurchaseOrders2>, int>(purchaseOrdersCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get PurchaseOrder data from a Guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.PurchaseOrders">A PurchaseOrders DTO</see></returns>
        public async Task<PurchaseOrders2> GetPurchaseOrdersByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Purchase Order.");
            }
            CheckViewPurchaseOrderPermission();

            //var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            GeneralLedgerAccountStructure glConfiguration = null;

            try
            {
                glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError(e.Message, "Bad.Data");
            }

            PurchaseOrder purchaseOrder = null;

            try
            {
                purchaseOrder = await purchaseOrderRepository.GetPurchaseOrdersByGuidAsync(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Purchase Order not found for GUID: {0} ", guid));
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError(e.Message, "Bad.Data");
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException(string.Format("Purchase Order not found for GUID: {0} ", guid));
            }
            if (purchaseOrder.LineItems != null && purchaseOrder.LineItems.Any())
            {
                var projectIds = purchaseOrder.LineItems
                   .Where(i => i.GlDistributions != null)
                   .SelectMany(p => p.GlDistributions)
                   .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                   .Select(pj => pj.ProjectId)
                   .ToList()
                   .Distinct();
                if (projectIds != null && projectIds.Any())
                {
                    _projectReferenceIds = await purchaseOrderRepository.GetProjectReferenceIds(projectIds.ToArray());
                }
            }
            var purchaseOrderDTO = await ConvertPurchaseOrderEntityToDtoAsync(purchaseOrder, glConfiguration);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return purchaseOrderDTO;
        }

        /// <summary>
        /// Returns the list of Purchase Order summary object for the user
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Purchase Order Summary DTOs</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId)
        {
            List<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderSummary> purchaseOrderSummaryDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderSummary>();

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(personId);

            //check if personId has staff record            
            await CheckStaffRecordAsync(personId);

            // Check the permission code to view a purchase order.
            CheckPurchaseOrderViewPermission();

            // Get the list of purchase order summary domain entity from the repository
            var purchaseOrderSummaryDomainEntities = await purchaseOrderRepository.GetPurchaseOrderSummaryByPersonIdAsync(personId);

            if (purchaseOrderSummaryDomainEntities == null || !purchaseOrderSummaryDomainEntities.Any())
            {
                return purchaseOrderSummaryDtos;
            }

            //sorting
            var sortOrderSequence = new List<string> { PurchaseOrderStatus.InProgress.ToString(), PurchaseOrderStatus.NotApproved.ToString(), PurchaseOrderStatus.Outstanding.ToString(), PurchaseOrderStatus.Accepted.ToString(), PurchaseOrderStatus.Backordered.ToString(), PurchaseOrderStatus.Invoiced.ToString(), PurchaseOrderStatus.Paid.ToString(), PurchaseOrderStatus.Reconciled.ToString(), PurchaseOrderStatus.Closed.ToString(), PurchaseOrderStatus.Voided.ToString() };
            purchaseOrderSummaryDomainEntities = purchaseOrderSummaryDomainEntities.OrderBy(item => sortOrderSequence.IndexOf(item.Status.ToString())).ThenByDescending(x => int.Parse(x.Id));


            // Convert the purchase order summary and all its child objects into DTOs
            var purchaseOrderSummaryDtoAdapter = new PurchaseOrderSummaryEntityDtoAdapter(_adapterRegistry, logger);
            foreach (var purchaseOrderDomainEntity in purchaseOrderSummaryDomainEntities)
            {
                purchaseOrderSummaryDtos.Add(purchaseOrderSummaryDtoAdapter.MapToType(purchaseOrderDomainEntity));
            }

            return purchaseOrderSummaryDtos;
        }


        /// <summary>
        /// Determine if personId is has staff record
        /// </summary>
        /// <param name="personId">ID of person from data</param>
        /// <returns></returns>
        private async Task CheckStaffRecordAsync(string personId)
        {
            try
            {
                var staffRecord = await staffRepository.GetAsync(personId);
                if (staffRecord == null)
                {
                    var message = string.Format("{0} does not have staff record.", CurrentUser.PersonId);
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
            }
            catch (KeyNotFoundException)
            {
                var message = string.Format("{0} does not have staff record.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Converts an PurchaseOrder domain entity to its corresponding PurchaseOrder DTO
        /// </summary>
        /// <param name="source">PurchaseOrder domain entity</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.PurchaseOrders">PurchaseOrders</see></returns>
        private async Task<Dtos.PurchaseOrders2> ConvertPurchaseOrderEntityToDtoAsync(Domain.ColleagueFinance.Entities.PurchaseOrder source,
            GeneralLedgerAccountStructure glConfiguration, bool bypassCache = false)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("A source is required to obtain a Purchase Order.");
                return null;
            }
            var purchaseOrder = new Ellucian.Colleague.Dtos.PurchaseOrders2();

            var currency = GetCurrencyIsoCode(source.CurrencyCode, source.HostCountry);

            purchaseOrder.Id = source.Guid;
            purchaseOrder.OrderNumber = source.Number;

            if (!string.IsNullOrEmpty(source.Type))
            {
                string upperType = source.Type.ToUpper();
                switch (upperType)
                {
                    case ("PROCUREMENT"):
                        purchaseOrder.Type = PurchaseOrdersTypes.Procurement;
                        break;
                    case ("EPROCUREMENT"):
                        purchaseOrder.Type = PurchaseOrdersTypes.Eprocurement;
                        break;
                    case ("TRAVEL"):
                        purchaseOrder.Type = PurchaseOrdersTypes.Travel;
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(source.SubmittedBy))
            {
                string submittedByGuid = string.Empty;
                try
                {
                    submittedByGuid = await personRepository.GetPersonGuidFromIdAsync(source.SubmittedBy);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
                if (string.IsNullOrEmpty(submittedByGuid))
                {
                    string message = string.Concat("Missing Submitted By information for purchase order: ", source.Id, " Guid: ", source.Guid, " Submitted By: ", source.SubmittedBy);
                    IntegrationApiExceptionAddError(message, "Bad.Data", source.Guid, source.Id);
                }
                else
                {
                    purchaseOrder.SubmittedBy = new GuidObject2(submittedByGuid);
                }
            }

            if ((source.ReferenceNo != null) && (source.ReferenceNo.Any()))
            {
                purchaseOrder.ReferenceNumbers = source.ReferenceNo;
            }
            purchaseOrder.OrderedOn = source.Date;
            purchaseOrder.TransactionDate = (source.MaintenanceDate != null && source.MaintenanceDate.HasValue)
                ? Convert.ToDateTime(source.MaintenanceDate) : source.Date;

            if ((source.DeliveryDate != null && source.DeliveryDate.HasValue))
                purchaseOrder.DeliveredBy = Convert.ToDateTime(source.DeliveryDate);

            purchaseOrder.StatusDate = source.StatusDate;
            if ((source.VoidGlTranDate != null && source.VoidGlTranDate.HasValue))
                purchaseOrder.StatusDate = Convert.ToDateTime(source.VoidGlTranDate);

            switch (source.Status)
            {
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Accepted):
                    purchaseOrder.Status = PurchaseOrdersStatus.Accepted; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Backordered):
                    purchaseOrder.Status = PurchaseOrdersStatus.Backordered; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Closed):
                    purchaseOrder.Status = PurchaseOrdersStatus.Closed; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress):
                    purchaseOrder.Status = PurchaseOrdersStatus.InProgress; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Invoiced):
                    purchaseOrder.Status = PurchaseOrdersStatus.Invoiced; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.NotApproved):
                    purchaseOrder.Status = PurchaseOrdersStatus.Notapproved; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Outstanding):
                    purchaseOrder.Status = PurchaseOrdersStatus.Outstanding; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Paid):
                    purchaseOrder.Status = PurchaseOrdersStatus.Paid; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Reconciled):
                    purchaseOrder.Status = PurchaseOrdersStatus.Reconciled; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Voided):
                    purchaseOrder.Status = PurchaseOrdersStatus.Voided; break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(source.Buyer))
            {
                try
                {
                    var buyerGuid = await buyerRepository.GetBuyerGuidFromIdAsync(source.Buyer);
                    if (string.IsNullOrEmpty(buyerGuid))
                    {
                        var message = string.Concat("Missing buyer information for purchase order: '", source.Id, "' Buyer: '", source.Buyer, "'");

                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        purchaseOrder.Buyer = new GuidObject2(buyerGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            var initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty();
            initiator.Name = source.InitiatorName;
            if (!string.IsNullOrEmpty(source.DefaultInitiator))
            {
                try
                {
                    var initiatorGuid = await personRepository.GetPersonGuidFromIdAsync(source.DefaultInitiator);
                    if (string.IsNullOrEmpty(initiatorGuid))
                    {
                        string message = string.Concat("Missing initiator information for purchase order: '", source.Id, "'  Initiator: '", source.DefaultInitiator, "'");
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        initiator.Detail = new GuidObject2(initiatorGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }

            }
            if ((!string.IsNullOrEmpty(initiator.Name)) || (initiator.Detail != null))
            {
                purchaseOrder.Initiator = initiator;
            }

            var purchaseOrdersShipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty();
            if (!string.IsNullOrEmpty(source.ShipToCode))
            {
                try
                {
                    var shipToDestinationGuid = await colleagueFinanceReferenceDataRepository.GetShipToDestinationGuidAsync(source.ShipToCode);
                    if (string.IsNullOrEmpty(shipToDestinationGuid))
                    {
                        string message = string.Concat("Missing shipping information for purchase order: '", source.Id, "'  ShipToCode: '", source.ShipToCode, "'");
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        purchaseOrdersShipping.ShipTo = new GuidObject2(shipToDestinationGuid);
                        purchaseOrder.Shipping = purchaseOrdersShipping;
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.Fob))
            {
                try
                {
                    var freeOnBoardTypeGuid = await colleagueFinanceReferenceDataRepository.GetFreeOnBoardTypeGuidAsync(source.Fob);
                    if (string.IsNullOrEmpty(freeOnBoardTypeGuid))
                    {
                        string message = string.Concat("Missing FreeOnBoard information for purchase order: '", source.Id, "'  FreeOnBoard: '", source.Fob, "'");
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        purchaseOrdersShipping.FreeOnBoard = new GuidObject2(freeOnBoardTypeGuid);
                        purchaseOrder.Shipping = purchaseOrdersShipping;
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.ShipViaCode))
            {
                try
                {
                    var shippingMethodGuid = await colleagueFinanceReferenceDataRepository.GetShippingMethodGuidAsync(source.ShipViaCode);
                    if (string.IsNullOrEmpty(shippingMethodGuid))
                    {
                        string message = string.Concat("Missing Shipping Method information for purchase order: '", source.Id, "'  ShippingMethod: '", source.ShipViaCode, "'");
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        purchaseOrder.ShippingMethod = new GuidObject2(shippingMethodGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            try
            {
                purchaseOrder.OverrideShippingDestination = await BuildOverrideShippingDestinationDtoAsync(source, bypassCache);

            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.Guid, source.Id);
            }

            var purchaseOrderVendor = new PurchaseOrdersVendorDtoProperty2();


            if (!string.IsNullOrEmpty(source.VendorId))
            {
                purchaseOrderVendor.ExistingVendor = new PurchaseOrdersExistingVendorDtoProperty();

                try
                {
                   var vendorGuid = await vendorsRepository.GetVendorGuidFromIdAsync(source.VendorId);
                    if (string.IsNullOrEmpty(vendorGuid))
                    {
                        var message = string.Concat("Missing vendor GUID for purchase order: ", source.Id,  " Vendor ID: ", source.VendorId);
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);

                    }
                    purchaseOrderVendor.ExistingVendor.Vendor = new GuidObject2(vendorGuid);

                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }

                try
                { 
             
                    if (!string.IsNullOrEmpty(source.VendorAddressId) && (source.AltAddressFlag == true))
                    {
                        var vendorAddressGuid = await purchaseOrderRepository.GetGuidFromIdAsync(source.VendorAddressId, "ADDRESS");
                        if (string.IsNullOrEmpty(vendorAddressGuid))
                        {
                            var message = string.Concat("Missing GUID for existing vendor address: ", source.Id,  " Vendor ID: ", source.VendorId);
                            IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);

                        }
                        purchaseOrderVendor.ExistingVendor.AlternativeVendorAddress = new GuidObject2(vendorAddressGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            if ((purchaseOrderVendor.ExistingVendor == null) || (purchaseOrderVendor.ExistingVendor.AlternativeVendorAddress == null && source.AltAddressFlag == true))
            {
                try
                {
                    purchaseOrderVendor.ManualVendorDetails = await BuildManualVendorDetailsDtoAsync(source, bypassCache);
                }
               
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data", source.Guid, source.Id);
                }
            }

            if (purchaseOrderVendor.ExistingVendor != null || purchaseOrderVendor.ManualVendorDetails != null)
            {
                purchaseOrder.Vendor = purchaseOrderVendor;
            }
            else
            {
                IntegrationApiExceptionAddError("Vendor is required.", "Bad.Data", source.Guid, source.Id);
            }

            if (!string.IsNullOrEmpty(source.VendorTerms))
            {
                try
                {
                    var vendorTermGuid = await colleagueFinanceReferenceDataRepository.GetVendorTermGuidAsync(source.VendorTerms);

                    if (string.IsNullOrEmpty(vendorTermGuid))
                    {
                        var message = string.Concat("Missing vendor term information.  Vendor Term: ", source.VendorTerms);
                        IntegrationApiExceptionAddError(message, "Bad.Data", source.Guid, source.Id);
                    }
                    else {
                        purchaseOrder.PaymentTerms = new GuidObject2(vendorTermGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }

            if (!string.IsNullOrEmpty(source.ApType))
            {
                try
                {
                    var apTypeGuid = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourceGuidAsync(source.ApType);

                    if (string.IsNullOrEmpty(apTypeGuid))
                    {
                        var message = string.Concat("Missing accounts payable source information.  Ap Type: ", source.ApType);
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", source.Guid, source.Id);
                    }
                    else
                    {
                        purchaseOrder.PaymentSource = new GuidObject2(apTypeGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", source.Guid, source.Id);
                }
            }


            var purchaseOrdersComments = new List<PurchaseOrdersCommentsDtoProperty>();
            if (!string.IsNullOrEmpty(source.Comments))
            {
                var purchaseOrdersComment = new PurchaseOrdersCommentsDtoProperty();
                purchaseOrdersComment.Comment = source.Comments;  //PoPrintedComments
                purchaseOrdersComment.Type = CommentTypes.Printed;
                purchaseOrdersComments.Add(purchaseOrdersComment);
            }
            if (!string.IsNullOrEmpty(source.InternalComments))
            {
                var purchaseOrdersComment = new PurchaseOrdersCommentsDtoProperty();
                purchaseOrdersComment.Comment = source.InternalComments; //PoComments
                purchaseOrdersComment.Type = CommentTypes.NotPrinted;
                purchaseOrdersComments.Add(purchaseOrdersComment);
            }
            if (purchaseOrdersComments != null && purchaseOrdersComments.Any())
            {
                purchaseOrder.Comments = purchaseOrdersComments;
            }

            var lineItems = new List<PurchaseOrdersLineItemsDtoProperty>();
            foreach (var sourceLineItem in source.LineItems)
            {
                var lineItem = await BuildPurchaseOrderLineItem(sourceLineItem, source.Id, source.Guid, glConfiguration, currency, bypassCache);
                if (lineItem != null)
                {
                    lineItems.Add(lineItem);
                }
            }
            if (lineItems != null && lineItems.Any())
            {
                purchaseOrder.LineItems = lineItems;
            }

            return purchaseOrder;
        }

        #endregion

        #region V11 PUT/POST

        /// <summary>
        /// EEDM V11 Put purchase order
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        public async Task<PurchaseOrders2> PutPurchaseOrdersAsync(string guid, PurchaseOrders2 purchaseOrder)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException("purchaseOrder", "Must provide a purchaseOrder for update");
            if (string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrder", "Must provide a guid for purchaseOrder update");
            
            // verify the user has the permission to create a PurchaseOrders
            CheckUpdatePurchaseOrderPermission();

            ValidatePurchaseOrder(purchaseOrder);

            if (purchaseOrder.PaymentSource == null)
            {
                IntegrationApiExceptionAddError("PaymentSource cannot be unset.", "Validation.Exception");
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            // get the ID associated with the incoming guid
            var purchaseOrderId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(purchaseOrder.Id);

           var overRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(purchaseOrderId))
            {

                GeneralLedgerAccountStructure glConfiguration = null;

                try
                {
                    glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "", purchaseOrder.Id);
                    throw IntegrationApiException;
                }

                purchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                #region  Check Funds Available

                try
                {
                    // Don't check funds availability if we are closing the purchase order
                    if (purchaseOrder.Status != PurchaseOrdersStatus.Closed && purchaseOrder.Status != PurchaseOrdersStatus.Voided)
                        overRideGLs = await CheckFunds(purchaseOrder, purchaseOrderId);

                    if ((purchaseOrder.LineItems) != null && (purchaseOrder.LineItems.Any()))
                    {
                        var projectRefNos = purchaseOrder.LineItems
                       .Where(i => i.AccountDetail != null)
                       .SelectMany(p => p.AccountDetail)
                       .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                       .Select(pj => pj.AccountingString.Split('*')[1])
                       .ToList()
                       .Distinct();

                        if (projectRefNos != null && projectRefNos.Any())
                        {
                            _projectReferenceIds = await purchaseOrderRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                        }
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message);
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                #endregion

                #region create domain entity from request
                PurchaseOrder purchaseOrderEntity = null;
                try
                {
                    // map the DTO to entities
                    purchaseOrderEntity = await ConvertPurchaseOrdersDtoToEntityAsync(purchaseOrder.Id, purchaseOrder, glConfiguration.MajorComponents.Count);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Record not created.  Error extracting request." + ex.Message, "Global.Internal.Error",
                       purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null
                      );
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                #endregion

                #region Create record from domain entity

                PurchaseOrder updatedPurchaseOrderEntity = null;

                // update the entity in the database
                try
                {
                    updatedPurchaseOrderEntity = await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrderEntity);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
                {
                    IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                         purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null);
                    throw IntegrationApiException;
                }


                #endregion

                #region Build DTO response

                PurchaseOrders2 dtoPurchaseOrder = null;
                try
                {
                    dtoPurchaseOrder = await this.ConvertPurchaseOrderEntityToDtoAsync(updatedPurchaseOrderEntity, glConfiguration, true);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError("Record created. Error building response." + ex.Message, "Global.Internal.Error",
                         purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null);
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                #endregion


                if (dtoPurchaseOrder.LineItems != null && dtoPurchaseOrder.LineItems.Any() && overRideGLs != null && overRideGLs.Any())
                {
                    int lineCount = 0;
                    foreach (var lineItem in dtoPurchaseOrder.LineItems)
                    {
                        int detailCount = 0;
                        lineCount++;
                        foreach (var detail in lineItem.AccountDetail)
                        {
                            detailCount++;
                            string PosID = lineCount.ToString() + "." + detailCount.ToString();
                            var findOvr = overRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.Sequence == PosID + ".DS");
                            if (findOvr != null)
                            {
                                if (!string.IsNullOrEmpty(findOvr.SubmittedBy) && findOvr.Sequence == PosID + ".DS")
                                {
                                    var submittedByGuid = await personRepository.GetPersonGuidFromIdAsync(findOvr.SubmittedBy);
                                    if (string.IsNullOrEmpty(submittedByGuid))
                                    {
                                        throw new Exception(string.Concat("Process finished but we couldn't return a Submitted By GUID purchase order: ", dtoPurchaseOrder.Id, " Submitted By: ", findOvr.SubmittedBy));
                                    }
                                    detail.SubmittedBy = new GuidObject2(submittedByGuid);
                                }
                                if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                    detail.BudgetCheck = PurchaseOrdersAccountBudgetCheck.Override;
                            }
                        }
                    }
                }

                // return the newly updated DTO
                return dtoPurchaseOrder;
            }

            // perform a create instead
            return await PostPurchaseOrdersAsync(purchaseOrder);
        }

        /// <summary>
        /// Post the purchase order Async
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        public async Task<PurchaseOrders2> PostPurchaseOrdersAsync(PurchaseOrders2 purchaseOrder)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException("purchaseOrders", "Must provide a purchaseOrders for update");
            if (string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrders", "Must provide a guid for purchaseOrders update");

            // verify the user has the permission to create a PurchaseOrders
            CheckUpdatePurchaseOrderPermission();

           
            ValidatePurchaseOrder(purchaseOrder);


            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            purchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            #region  Check Funds Available

            var overRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

            try
            {
                overRideGLs = await CheckFunds(purchaseOrder);


                if ((purchaseOrder.LineItems) != null && (purchaseOrder.LineItems.Any()))
                {
                    var projectRefNos = purchaseOrder.LineItems
                   .Where(i => i.AccountDetail != null)
                   .SelectMany(p => p.AccountDetail)
                   .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                   .Select(pj => pj.AccountingString.Split('*')[1])
                   .ToList()
                   .Distinct();

                    if (projectRefNos != null && projectRefNos.Any())
                    {
                        _projectReferenceIds = await purchaseOrderRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                    }
                }
                
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            #endregion

            #region Create domain entity from request

            PurchaseOrder purchaseOrderEntity = null;
            try
            {
                // map the DTO to entities
                purchaseOrderEntity = await ConvertPurchaseOrdersDtoToEntityAsync(purchaseOrder.Id, purchaseOrder, glConfiguration.MajorComponents.Count);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record not created.  Error extracting request." + ex.Message, "Global.Internal.Error",
                   purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null
                  );
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            
            #endregion

            #region Create record from domain entity

            PurchaseOrder createdPurchaseOrder = null;

            // create the entity in the database
            try
            {
                createdPurchaseOrder = await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrderEntity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)  //catch InvalidOperationException thrown when record already exists.
            {
                IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error",
                     purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null);
                throw IntegrationApiException;
            }


            #endregion

            #region Build DTO response

            PurchaseOrders2 dtoPurchaseOrder = null;
            try
            {
                dtoPurchaseOrder = await this.ConvertPurchaseOrderEntityToDtoAsync(createdPurchaseOrder, glConfiguration, true);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("Record created. Error building response." + ex.Message, "Global.Internal.Error",
                     purchaseOrder != null && !string.IsNullOrEmpty(purchaseOrder.Id) ? purchaseOrder.Id : null);
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            #endregion
            

            if (dtoPurchaseOrder.LineItems != null && dtoPurchaseOrder.LineItems.Any() && overRideGLs != null && overRideGLs.Any())
            {
                int lineCount = 0;
                foreach (var lineItem in dtoPurchaseOrder.LineItems)
                {
                    int detailCount = 0;
                    lineCount++;
                    foreach (var detail in lineItem.AccountDetail)
                    {
                        detailCount++;
                        string PosID = lineCount.ToString() + "." + detailCount.ToString();
                        var findOvr = overRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.Sequence == PosID + ".DS");
                        if (findOvr != null)
                        {
                            if (!string.IsNullOrEmpty(findOvr.SubmittedBy) && findOvr.Sequence == PosID + ".DS")
                            {
                                var submittedByGuid = await personRepository.GetPersonGuidFromIdAsync(findOvr.SubmittedBy);
                                if (string.IsNullOrEmpty(submittedByGuid))
                                {
                                    throw new Exception(string.Concat("Process finsihed by we couldn't return a Submitted By GUID purchase order: ", dtoPurchaseOrder.Id, " Submitted By: ", findOvr.SubmittedBy));
                                }
                                detail.SubmittedBy = new GuidObject2(submittedByGuid);
                            }
                            if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                detail.BudgetCheck = PurchaseOrdersAccountBudgetCheck.Override;
                        }
                    }
                }
            }

            // return the newly created purchaseOrder
            return dtoPurchaseOrder;
        }

        /// <summary>
        /// Check if Funds are avialable for this PUT/POST event.
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <param name="PoId"></param>
        /// <returns></returns>
        private async Task<List<Domain.ColleagueFinance.Entities.FundsAvailable>> CheckFunds(PurchaseOrders2 purchaseOrder, string PoId = "")
        {
            var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            //check if Accounting string has funds
            int lineCount = 0;
            var docSubmittedById = string.Empty;
            if (purchaseOrder.SubmittedBy != null && !string.IsNullOrEmpty(purchaseOrder.SubmittedBy.Id))
                docSubmittedById = await personRepository.GetPersonIdFromGuidAsync(purchaseOrder.SubmittedBy.Id);
            else
                docSubmittedById = CurrentUser.PersonId;
            var budgetOvrCheckTuple = new List<Tuple<string, bool>>();

            foreach (var lineItems in purchaseOrder.LineItems)
            {
                int detailCount = 0;
                lineCount++;
                List<string> accountingStringList = new List<string>();
                if (lineItems.AccountDetail != null && lineItems.AccountDetail.Any())
                {
                    foreach (var details in lineItems.AccountDetail)
                    {
                        detailCount++;
                        var submittedBy = (details.SubmittedBy != null) ? details.SubmittedBy.Id :
                                        (purchaseOrder.SubmittedBy != null) ? purchaseOrder.SubmittedBy.Id : "";
                        var submittedById = (!string.IsNullOrEmpty(submittedBy)) ? await personRepository.GetPersonIdFromGuidAsync(submittedBy) : "";

                        if (details.Allocation != null && details.Allocation.Allocated != null &&
                                details.Allocation.Allocated.Amount != null && details.Allocation.Allocated.Amount.Value != null
                                && details.Allocation.Allocated.Amount.Value.HasValue)
                        {
                            string PosID = lineCount.ToString() + "." + detailCount.ToString();
                            if (details.SubmittedBy != null)
                                PosID = PosID + ".DS";
                            var budgetCheckOverrideFlag = (details.BudgetCheck == PurchaseOrdersAccountBudgetCheck.Override) ? true : false;
                            budgetOvrCheckTuple.Add(new Tuple<string, bool>(details.AccountingString, budgetCheckOverrideFlag));
                            // You don't need to have funds available if you are de-activating the line item or if the line item
                            // status is already paid, invoiced, or reconciled (funds already allocated).
                            if (details.Status != Dtos.EnumProperties.LineItemStatus.Voided 
                                && details.Status != Dtos.EnumProperties.LineItemStatus.Closed
                                && details.Status != Dtos.EnumProperties.LineItemStatus.Invoiced
                                && details.Status != Dtos.EnumProperties.LineItemStatus.Paid
                                && details.Status != Dtos.EnumProperties.LineItemStatus.Reconciled)
                            {
                                fundsAvailable.Add(new Domain.ColleagueFinance.Entities.FundsAvailable(details.AccountingString)
                                {
                                    Sequence = PosID,
                                    SubmittedBy = submittedById,
                                    Amount = details.Allocation.Allocated.Amount.Value.Value,
                                    ItemId = lineItems.LineItemNumber ?? lineItems.LineItemNumber,
                                    TransactionDate = purchaseOrder.OrderedOn
                                });
                            }
                        }

                        var accountingString = accountingStringList.Find(x => x.Equals(details.AccountingString));
                        if (string.IsNullOrWhiteSpace(accountingString))
                        {
                            accountingStringList.Add(details.AccountingString);
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("A line item has two account details with the same GL number " + accountingString + " this is not allowed", "Validation.Exception");
                        }
                    }
                }
            }
            if (fundsAvailable != null && fundsAvailable.Any())
            {
                if (string.IsNullOrEmpty(PoId))
                {
                    PoId = "NEW";
                }

                var availableFunds = await accountFundAvailableRepository.CheckAvailableFundsAsync(fundsAvailable, PoId, "", "", docSubmittedById);
                if (availableFunds != null)
                {
                    foreach (var availableFund in availableFunds)
                    {
                        if (availableFund.AvailableStatus == FundsAvailableStatus.NotAvailable)
                        {
                            IntegrationApiExceptionAddError("The accounting string " + availableFund.AccountString + " does not have funds available", "Validation.Exception");
                        }
                        //if we get a override and if the budgetcheck flag is not set to override then thrown an exception
                        else if (availableFund.AvailableStatus == FundsAvailableStatus.Override )
                        {
                            var budOverCheck = budgetOvrCheckTuple.FirstOrDefault(acct => acct.Item1 == availableFund.AccountString);
                            if (budOverCheck != null && budOverCheck.Item2 == false)
                            {
                                IntegrationApiExceptionAddError("The accounting string " + availableFund.AccountString + " does not have funds available. BudgetCheck flag not set to override.", "Validation.Exception");
                            }
                            else
                            {
                                overrideAvailable.Add(availableFund);
                            }
                        }
                    }
                }
            }

            return overrideAvailable;
        }

        /// <summary>
        /// Convert a DTO to a entity
        /// </summary>
        /// <param name="purchaseOrderGuid"></param>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        private async Task<Domain.ColleagueFinance.Entities.PurchaseOrder> ConvertPurchaseOrdersDtoToEntityAsync(string purchaseOrderGuid, Dtos.PurchaseOrders2 purchaseOrder, int GLCompCount)
        {
            if (purchaseOrder == null || string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrder", "Must provide guid for purchaseOrder");

            if ((purchaseOrder.Vendor != null) && (purchaseOrder.Vendor.ExistingVendor != null) &&
                 (purchaseOrder.Vendor.ExistingVendor.Vendor != null) && (string.IsNullOrEmpty(purchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                IntegrationApiExceptionAddError("Must provide vendor.id for purchaseOrder", "Validation.Exception", purchaseOrder.Id);


            var guid = purchaseOrderGuid;
            string purchaseOrderId = string.Empty;

            try
            {
                purchaseOrderId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(guid);
            }
            catch (Exception)
            {
                // do nothing.. if the purchase order does not exist, it will be a new record.
            }

            var poStatus = PurchaseOrderStatus.InProgress;
            if (purchaseOrder.Status != PurchaseOrdersStatus.NotSet && purchaseOrder.Status != null)
            {
                try
                {
                    poStatus = GetPurchaseOrderStatus(purchaseOrder);
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Invalid purchase order status '{0}'", purchaseOrder.Status.ToString()), "Validation.Exception",
                           purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }

            var date = (purchaseOrder.StatusDate == DateTime.MinValue)
                ? DateTime.Now.Date : purchaseOrder.StatusDate;
            string currency = null;

            if (string.IsNullOrEmpty(purchaseOrderId))
            {
                purchaseOrderId = "NEW";
            }

            if (string.IsNullOrEmpty(purchaseOrder.OrderNumber)) { purchaseOrder.OrderNumber = "new"; }

            var purchaseOrderEntity = new Domain.ColleagueFinance.Entities.PurchaseOrder(
               purchaseOrderId, guid ?? new Guid().ToString(), purchaseOrder.OrderNumber, "test name", poStatus, date, purchaseOrder.OrderedOn);

            if (purchaseOrder.Type != PurchaseOrdersTypes.NotSet)
            {
                switch (purchaseOrder.Type)
                {
                    case (PurchaseOrdersTypes.Procurement):
                        purchaseOrderEntity.Type = "PROCUREMENT"; break;
                    case (PurchaseOrdersTypes.Eprocurement):
                        purchaseOrderEntity.Type = "EPROCUREMENT"; break;
                    case (PurchaseOrdersTypes.Travel):
                        purchaseOrderEntity.Type = "TRAVEL"; break;
                    default:
                        break;
                }
            }

            if (purchaseOrder.SubmittedBy != null)
            {
                try
                {
                    var submittedById = await personRepository.GetPersonIdFromGuidAsync(purchaseOrder.SubmittedBy.Id);
                    if (string.IsNullOrEmpty(submittedById))
                    {
                        IntegrationApiExceptionAddError(string.Format("Person record not found for SubmittedBy.Id '{0}'", purchaseOrder.SubmittedBy.Id), "Validation.Exception",
                            purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                    purchaseOrderEntity.SubmittedBy = submittedById;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Format("Person record not found for SubmittedBy.Id '{0}'", purchaseOrder.SubmittedBy.Id), "Validation.Exception",
                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }
            // Flag to send data to the CTX to bypass the tax forms
            purchaseOrderEntity.bypassTaxForms = CheckBypassTaxformsPermission();
            // Flag to send data to the CTX to bypass approvals
            purchaseOrderEntity.bypassApprovals = CheckBypassApprovalsPermission();

            if (purchaseOrder.TransactionDate != default(DateTime))
                purchaseOrderEntity.MaintenanceDate = purchaseOrder.TransactionDate;

            if ((purchaseOrder.DeliveredBy != default(DateTime)))
                purchaseOrderEntity.DeliveryDate = purchaseOrder.DeliveredBy;

            if (purchaseOrder.Buyer != null)
            {
                try
                {
                    var buyerId = await buyerRepository.GetBuyerIdFromGuidAsync(purchaseOrder.Buyer.Id);
                    if (string.IsNullOrEmpty(buyerId))
                    {
                        IntegrationApiExceptionAddError(string.Format("Buyer not found for Buyer.Id '{0}'", purchaseOrder.Buyer.Id), "Validation.Exception",
                           purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                    else
                    {
                        purchaseOrderEntity.Buyer = buyerId;
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Format("Buyer not found for Buyer.Id '{0}'", purchaseOrder.Buyer.Id), "Validation.Exception",
                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }

            if (purchaseOrder.Initiator != null && purchaseOrder.Initiator.Detail != null)
            {
                try
                {
                    var initiatorId = await personRepository.GetPersonIdFromGuidAsync(purchaseOrder.Initiator.Detail.Id);
                    if (string.IsNullOrEmpty(initiatorId))
                    {
                        IntegrationApiExceptionAddError(string.Format("Initiator detail record not found for Initiator.Detail.Id '{0}'.", purchaseOrder.Initiator.Detail.Id), "Validation.Exception",
                               purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                    else
                    {
                        purchaseOrderEntity.DefaultInitiator = initiatorId;
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Format("Initiator detail record not found for Initiator.Detail.Id '{0}'.", purchaseOrder.Initiator.Detail.Id), "Validation.Exception",
                                purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }

            if (purchaseOrder.Shipping != null)
            {
                if (purchaseOrder.Shipping.ShipTo != null)
                {
                    IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination> shipToDestinations = null;
                    try
                    {
                        shipToDestinations = await GetShipToDestinationsAsync(true);

                        if (shipToDestinations == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve ShipToDestinations.", "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                        else
                        {
                            var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Guid == purchaseOrder.Shipping.ShipTo.Id);
                            if ((shipToDestination == null) || (string.IsNullOrEmpty(shipToDestination.Code)))
                            {
                                IntegrationApiExceptionAddError(string.Format("ShipToDestination record not found for Shipping.ShipTo.Id '{0}' ", purchaseOrder.Shipping.ShipTo.Id), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                purchaseOrderEntity.ShipToCode = shipToDestination.Code;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the ShipToDestination record for Shipping.ShipTo.Id '{0}'. Details: {1}.", purchaseOrder.Shipping.ShipTo.Id, ex.Message), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                }

                if (purchaseOrder.Shipping.FreeOnBoard != null)
                {
                    IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType> freeOnBoardTypes = null;
                    try
                    {
                        freeOnBoardTypes = await GetFreeOnBoardTypesAsync(true);
                        if (freeOnBoardTypes == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve FreeOnBoardTypes.", "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                        else
                        {
                            var freeOnBoardType = freeOnBoardTypes.FirstOrDefault(fob => fob.Guid == purchaseOrder.Shipping.FreeOnBoard.Id);
                            if ((freeOnBoardType == null) || (string.IsNullOrEmpty(freeOnBoardType.Code)))
                            {
                                IntegrationApiExceptionAddError(string.Format("FreeOnBoardTypes record not found for Shipping.FreeOnBoard.Id '{0}'.", purchaseOrder.Shipping.FreeOnBoard.Id), "Validation.Exception",
                                      purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                purchaseOrderEntity.Fob = freeOnBoardType.Code;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the FreeOnBoardTypes record for Shipping.FreeOnBoard.Id '{0}'. Details: {1}.", purchaseOrder.Shipping.FreeOnBoard.Id, ex.Message), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                }
            }

            // Shipping Method
            if (purchaseOrder.ShippingMethod != null)
            {
                IEnumerable<Domain.ColleagueFinance.Entities.ShippingMethod> shippingMethodTypes = null;
                try
                {
                    shippingMethodTypes = await GetShippingMethodsAsync(true);
                    if (shippingMethodTypes != null && !string.IsNullOrEmpty(purchaseOrder.ShippingMethod.Id))
                    {
                        var shippingMethod = shippingMethodTypes.FirstOrDefault(fob => fob.Guid == purchaseOrder.ShippingMethod.Id);
                        if ((shippingMethod == null) || (string.IsNullOrEmpty(shippingMethod.Code)))
                        {
                            IntegrationApiExceptionAddError(string.Format("ShippingMethods record not found for ShippingMethod.Id '{0}'.", purchaseOrder.ShippingMethod.Id), "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                        else
                        {
                            purchaseOrderEntity.ShipViaCode = shippingMethod.Code;
                        }
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the ShippingMethods record for ShippingMethod.Id '{0}'. Details: {1}.", purchaseOrder.ShippingMethod.Id, ex.Message), "Validation.Exception",
                               purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }

            if (purchaseOrder.OverrideShippingDestination != null)
            {
                var overrideShippingDestinationDto = purchaseOrder.OverrideShippingDestination;
                if (!string.IsNullOrEmpty(purchaseOrder.OverrideShippingDestination.Description))
                {
                    purchaseOrderEntity.AltShippingName = purchaseOrder.OverrideShippingDestination.Description;
                }

                if (overrideShippingDestinationDto.AddressLines != null && overrideShippingDestinationDto.AddressLines.Any())
                {
                    purchaseOrderEntity.AltShippingAddress = overrideShippingDestinationDto.AddressLines;
                }

                var place = overrideShippingDestinationDto.Place;
                if (place != null && place.Country != null)
                {
                    purchaseOrderEntity.AltShippingCountry = place.Country.Code.ToString();

                    if (!string.IsNullOrEmpty(place.Country.Locality))
                    {
                        purchaseOrderEntity.AltShippingCity = place.Country.Locality;
                    }
                    if (place.Country.Region != null && !string.IsNullOrEmpty(place.Country.Region.Code))
                    {
                        purchaseOrderEntity.AltShippingState = place.Country.Region.Code.Split('-')[1];
                    }
                    purchaseOrderEntity.AltShippingZip = place.Country.PostalCode;
                }
                if (purchaseOrder.OverrideShippingDestination.Contact != null)
                {
                    if (!string.IsNullOrEmpty(purchaseOrder.OverrideShippingDestination.Contact.Number))
                        purchaseOrderEntity.AltShippingPhone = purchaseOrder.OverrideShippingDestination.Contact.Number;
                    if (!string.IsNullOrEmpty(purchaseOrder.OverrideShippingDestination.Contact.Extension))
                        purchaseOrderEntity.AltShippingPhoneExt = purchaseOrder.OverrideShippingDestination.Contact.Extension;
                }
            }

            if (purchaseOrder.Vendor != null)
            {
                if (purchaseOrder.Vendor.ExistingVendor != null &&
                    purchaseOrder.Vendor.ExistingVendor.Vendor != null && (!string.IsNullOrEmpty(purchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                {
                    string vendorId = string.Empty;
                    try
                    {
                        vendorId = await vendorsRepository.GetVendorIdFromGuidAsync(purchaseOrder.Vendor.ExistingVendor.Vendor.Id);
                        if (!string.IsNullOrEmpty(vendorId))
                            purchaseOrderEntity.VendorId = vendorId;
                        else
                        {
                            IntegrationApiExceptionAddError
                                (string.Format("Vendor record not found for Vendor.ExistingVendor.Vendor.Id '{0}'.", purchaseOrder.Vendor.ExistingVendor.Vendor.Id), "Validation.Exception",
                                      purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError
                             (string.Concat("The vendor id must correspond with a valid vendor record : ", purchaseOrder.Vendor.ExistingVendor.Vendor.Id), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }


                    if (purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress != null &&
                        !string.IsNullOrWhiteSpace(purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id))
                    {
                        string vendorAddressId = string.Empty;
                        try
                        {
                            var alternativeVendorAddress = await referenceDataRepository.GetGuidLookupResultFromGuidAsync(purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id);

                            if (alternativeVendorAddress == null)
                            {
                                IntegrationApiExceptionAddError
                               (string.Format("Address record not found for ExistingVendor.AlternativeVendorAddress '{0}'.", purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id), "Validation.Exception",
                                 purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else if ((alternativeVendorAddress != null) && (!string.Equals(alternativeVendorAddress.Entity, "ADDRESS", StringComparison.OrdinalIgnoreCase)))
                            {
                                IntegrationApiExceptionAddError
                                 (string.Format("ExistingVendor.AlternativeVendorAddress GUID '{0}' has different entity '{1} than expected, ADDRESS.",
                                 purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id, alternativeVendorAddress.Entity), "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else if ((alternativeVendorAddress != null) && (string.IsNullOrEmpty(alternativeVendorAddress.PrimaryKey)))
                            {
                                IntegrationApiExceptionAddError
                                (string.Format("Key not found on Address record for ExistingVendor.AlternativeVendorAddress '{0}'", purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id), "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                purchaseOrderEntity.VendorAddressId = alternativeVendorAddress.PrimaryKey;
                            }

                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the address record for ExistingVendor.AlternativeVendorAddress  '{0}'. Details: {1}.", purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id, ex.Message), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }
                }

                if (purchaseOrder.Vendor.ManualVendorDetails != null)
                {
                    var manualVendorDetailsDto = purchaseOrder.Vendor.ManualVendorDetails;

                    if (manualVendorDetailsDto.Type == ManualVendorType.Organization)
                    { purchaseOrderEntity.MiscIntgCorpPersonFlag = "Organization"; }
                    else if (manualVendorDetailsDto.Type == ManualVendorType.Person)
                    {
                        purchaseOrderEntity.MiscIntgCorpPersonFlag = "Person";
                    }

                    if ((purchaseOrder.Vendor.ExistingVendor == null ||
                        purchaseOrder.Vendor.ExistingVendor.Vendor == null || string.IsNullOrEmpty(purchaseOrder.Vendor.ExistingVendor.Vendor.Id)) &&
                        (manualVendorDetailsDto.Type == null || !manualVendorDetailsDto.Type.HasValue))
                    {
                        IntegrationApiExceptionAddError("Must provide a valid manual vendor type when not supplying a existing vendor for a purchase order.", "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }

                    if (manualVendorDetailsDto.Name != null && manualVendorDetailsDto.Name.Any())
                    {
                        purchaseOrderEntity.MiscName = new List<string>();
                        purchaseOrderEntity.MiscName.Add(manualVendorDetailsDto.Name);
                    }

                    if (manualVendorDetailsDto.AddressLines != null && manualVendorDetailsDto.AddressLines.Any())
                    {
                        purchaseOrderEntity.MiscAddress = manualVendorDetailsDto.AddressLines;
                    }

                    var place = manualVendorDetailsDto.Place;
                    if (place != null && place.Country != null)
                    {
                        purchaseOrderEntity.MiscCountry = place.Country.Code.ToString();

                        if (!string.IsNullOrEmpty(place.Country.Locality))
                        {
                            purchaseOrderEntity.MiscCity = place.Country.Locality;
                        }
                        if (place.Country.Region != null && !string.IsNullOrEmpty(place.Country.Region.Code))
                        {
                            purchaseOrderEntity.MiscState = place.Country.Region.Code.Split('-')[1];
                        }
                        purchaseOrderEntity.MiscZip = place.Country.PostalCode;

                    }
                }
            }

            if (purchaseOrder.ReferenceNumbers != null && purchaseOrder.ReferenceNumbers.Count() > 0)
            {
                purchaseOrderEntity.ReferenceNo = purchaseOrder.ReferenceNumbers;
            }

            if (purchaseOrder.PaymentSource != null)
            {
                var payment = purchaseOrder.PaymentSource;
                if ((payment.Id != null) && (!string.IsNullOrEmpty(payment.Id)))
                {
                    IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> allAccountsPayableSources = null;
                    try
                    {
                        allAccountsPayableSources = await this.GetAllAccountsPayableSourcesAsync(true);
                        if (allAccountsPayableSources == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve AccountsPayableSources.", "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                        else
                        {
                            var accountsPayableSource = allAccountsPayableSources.FirstOrDefault(ap => ap.Guid == payment.Id);
                            if ((accountsPayableSource == null) || (string.IsNullOrEmpty(accountsPayableSource.Code)))
                            {
                                IntegrationApiExceptionAddError(string.Format("AccountsPayableSources record not found for PaymentSource.Id '{0}'.", payment.Id), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                purchaseOrderEntity.ApType = accountsPayableSource.Code;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the AccountsPayableSources record for PaymentSource.Id '{0}'. Details: {1}.", payment.Id, ex.Message), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                }
            }

            if ((purchaseOrder.PaymentTerms != null) && (!string.IsNullOrEmpty(purchaseOrder.PaymentTerms.Id)))
            {
                IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> allVendorTerms = null;
                try
                {
                    allVendorTerms = await this.GetVendorTermsAsync(true);
                    if (allVendorTerms == null)
                    {
                        IntegrationApiExceptionAddError("Unable to retrieve VendorTerms.", "Validation.Exception",
                               purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                    }
                    else
                    {
                        var vendorTerm = allVendorTerms.FirstOrDefault(vnd => vnd.Guid == purchaseOrder.PaymentTerms.Id);
                        if ((vendorTerm == null) || (string.IsNullOrEmpty(vendorTerm.Code)))
                        {

                            IntegrationApiExceptionAddError(string.Format("VendorTerm record not found for PaymentTerms.Id '{0}'.", purchaseOrder.PaymentTerms.Id), "Validation.Exception",
                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                        else
                        {
                            purchaseOrderEntity.VendorTerms = vendorTerm.Code;
                        }
                    }
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the VendorTerm record for  PaymentTerms.Id '{0}'. Details: {1}.", purchaseOrder.PaymentTerms.Id, ex.Message), "Validation.Exception",
                               purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                }
            }

            if (purchaseOrder.Comments != null && purchaseOrder.Comments.Any())
            {
                purchaseOrderEntity.Comments = string.Empty;
                purchaseOrderEntity.InternalComments = string.Empty;
                foreach (var comment in purchaseOrder.Comments)
                {
                    switch (comment.Type)
                    {
                        case CommentTypes.NotPrinted:
                            purchaseOrderEntity.InternalComments = !string.IsNullOrEmpty(purchaseOrderEntity.InternalComments) ?
                                string.Concat(purchaseOrderEntity.InternalComments, " ", comment.Comment) : comment.Comment;
                            break;
                        case CommentTypes.Printed:
                            purchaseOrderEntity.Comments = !string.IsNullOrEmpty(purchaseOrderEntity.Comments) ?
                                string.Concat(purchaseOrderEntity.Comments, " ", comment.Comment) : comment.Comment;
                            break;
                    }
                }
            }

            if ((purchaseOrder.LineItems != null) && (purchaseOrder.LineItems.Any()))
            {

                foreach (var lineItem in purchaseOrder.LineItems)
                {
                    var id = lineItem.LineItemNumber;
                    if (string.IsNullOrEmpty(id))
                        id = "NEW";
                    var description = lineItem.Description;
                    decimal quantity = lineItem.Quantity == null ? 0 : lineItem.Quantity.Value;
                    decimal price = 0;
                    if ((lineItem.UnitPrice != null) && (lineItem.UnitPrice.Value.HasValue))
                    {
                        price = Convert.ToDecimal(lineItem.UnitPrice.Value);
                        currency = lineItem.UnitPrice.Currency.ToString();
                    }

                    decimal extendedPrice = 0;

                    var apLineItem = new LineItem(id, description, quantity, price, extendedPrice);

                    if ((lineItem.CommodityCode != null) && (!string.IsNullOrEmpty(lineItem.CommodityCode.Id)))
                    {
                        IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> allCommodityCodes = null;
                        try
                        {
                            allCommodityCodes = await GetCommodityCodesAsync(true);
                            if (allCommodityCodes == null)
                            {
                                IntegrationApiExceptionAddError("Unable to retrieve CommodityCodes.", "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                var commodityCode = allCommodityCodes.FirstOrDefault(cc => cc.Guid == lineItem.CommodityCode.Id);
                                if ((commodityCode == null) || (string.IsNullOrEmpty(commodityCode.Code)))
                                {
                                    IntegrationApiExceptionAddError(string.Format("CommodityCode record not found for CommodityCode.Id '{0}'.", lineItem.CommodityCode.Id), "Validation.Exception",
                                           purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                                else
                                {
                                    apLineItem.CommodityCode = commodityCode.Code;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the CommodityCode record for  CommodityCode.Id '{0}'. Details: {1}.", lineItem.CommodityCode.Id, ex.Message), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }

                    if ((lineItem.UnitOfMeasure != null) && (!string.IsNullOrEmpty(lineItem.UnitOfMeasure.Id)))
                    {
                        IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> allCommodityUnitTypes = null;
                        try
                        {
                            allCommodityUnitTypes = await GetCommodityUnitTypesAsync(true);
                            if (allCommodityUnitTypes == null)
                            {
                                IntegrationApiExceptionAddError("Unable to retrieve CommodityUnitTypes.", "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                var commodityUnitType = allCommodityUnitTypes.FirstOrDefault(cc => cc.Guid == lineItem.UnitOfMeasure.Id);
                                if ((commodityUnitType == null) || (string.IsNullOrEmpty(commodityUnitType.Code)))
                                {
                                    IntegrationApiExceptionAddError(string.Format("CommodityUnitType record not found for UnitOfMeasure.Id '{0}'.", lineItem.UnitOfMeasure.Id), "Validation.Exception",
                                           purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                                else
                                {
                                    apLineItem.UnitOfIssue = commodityUnitType.Code;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the CommodityUnitType record for UnitOfMeasure.Id '{0}'. Details: {1}.", lineItem.UnitOfMeasure.Id, ex.Message), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }
                    if (lineItem.Comments != null && lineItem.Comments.Any())
                    {
                        string comment = string.Empty;
                        foreach (var com in lineItem.Comments)
                        {
                            comment = !string.IsNullOrEmpty(comment) ?
                                string.Concat(comment, " ", com.Comment) : com.Comment;
                        }
                        apLineItem.Comments = comment;
                    }

                    if (lineItem.TradeDiscount != null && lineItem.TradeDiscount.Amount != null)
                    {
                        apLineItem.TradeDiscountAmount = lineItem.TradeDiscount.Amount.Value;
                        currency = lineItem.TradeDiscount.Amount.Currency.ToString();
                    }

                    if (lineItem.TradeDiscount != null && lineItem.TradeDiscount.Percent != null)
                    {
                        apLineItem.TradeDiscountPercentage = lineItem.TradeDiscount.Percent;
                    }

                    if (!(string.IsNullOrWhiteSpace(lineItem.PartNumber)))
                    {
                        apLineItem.VendorPart = lineItem.PartNumber;
                    }
                    // Line items have both expected date and desired date.  We want to use expected date.
                    // apLineItem.DesiredDate = lineItem.DesiredDate;
                    apLineItem.ExpectedDeliveryDate = lineItem.DesiredDate;


                    var lineItemTaxCodes = new List<string>();
                    if ((lineItem.TaxCodes != null) && (lineItem.TaxCodes.Any()))
                    {

                        IEnumerable<Domain.Base.Entities.CommerceTaxCode> allCommerceTaxCodes = null;
                        try
                        {
                            allCommerceTaxCodes = await GetCommerceTaxCodesAsync(true);
                            if (allCommerceTaxCodes == null)
                            {
                                IntegrationApiExceptionAddError("Unable to retrieve CommerceTaxCodes.", "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {

                                foreach (var lineItemTax in lineItem.TaxCodes)
                                {
                                    if (lineItemTax != null && lineItemTax.Id != null && !string.IsNullOrEmpty(lineItemTax.Id))
                                    {
                                        var taxCode = allCommerceTaxCodes.FirstOrDefault(tax => tax.Guid == lineItemTax.Id);
                                        if ((taxCode == null) || (string.IsNullOrEmpty(taxCode.Code)))
                                        {
                                            IntegrationApiExceptionAddError(string.Format("CommerceTaxCodes record not found for TaxCode.Id '{0}'.", lineItemTax.Id), "Validation.Exception",
                                                   purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                        }
                                        else if ((taxCode.AppurEntryFlag == false) || (taxCode.UseTaxFlag == true))
                                        {
                                             IntegrationApiExceptionAddError(string.Concat("Tax Code '", taxCode.Code, "' is not permitted for use on AP/PUR documents."), "Validation.Exception",
                                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                        }
                                        else
                                        {
                                            lineItemTaxCodes.Add(taxCode.Code);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the CommerceTaxCodes record. Details: {0}.", ex.Message), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }

                    if (lineItem.Reference != null && !string.IsNullOrEmpty(lineItem.Reference.Document.Requisition.Id))
                    {
                        try
                        {
                            var requisitionGuidLookup = await referenceDataRepository.GetGuidLookupResultFromGuidAsync(lineItem.Reference.Document.Requisition.Id);

                            if (requisitionGuidLookup == null)
                            {
                                IntegrationApiExceptionAddError
                               (string.Format("Requisition record not found for Reference.Document.Requisition.Id '{0}'", lineItem.Reference.Document.Requisition.Id), "Validation.Exception",
                                 purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else if ((requisitionGuidLookup != null) && (!string.Equals(requisitionGuidLookup.Entity, "REQUISITIONS", StringComparison.OrdinalIgnoreCase)))
                            {
                                IntegrationApiExceptionAddError
                                 (string.Format("Reference.Document.Requisition.Id '{0}' has different entity '{1} than expected, REQUISITIONS.",
                                 lineItem.Reference.Document.Requisition.Id, requisitionGuidLookup.Entity), "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else if ((requisitionGuidLookup != null) && (string.IsNullOrEmpty(requisitionGuidLookup.PrimaryKey)))
                            {
                                IntegrationApiExceptionAddError
                                (string.Format("ID not found on Requisition record for Reference.Document.Requisition.Id '{0}'", lineItem.Reference.Document.Requisition.Id), "Validation.Exception",
                                  purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                            else
                            {
                                apLineItem.RequisitionId = requisitionGuidLookup.PrimaryKey;
                            }

                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred retrieving the requisition record for lineItem.Reference.Document.Requisition.Id  '{0}'. Details: {1}.", purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id, ex.Message), "Validation.Exception",
                                    purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                        }
                    }

                    if (lineItem.AccountDetail != null && lineItem.AccountDetail.Any())
                    {
                        if (lineItem.AccountDetail[0].StatusDate != null)
                        {
                            apLineItem.StatusDate = lineItem.AccountDetail[0].StatusDate;
                        }
                        var crntDetails = lineItem.AccountDetail[0].Status;
                        var crntDetailsDate = lineItem.AccountDetail[0].StatusDate;
                        foreach (var accountDetails in lineItem.AccountDetail)
                        {

                            if (accountDetails.Status != Dtos.EnumProperties.LineItemStatus.NotSet)
                            {
                                var status = accountDetails.Status;
                                switch (status)
                                {
                                    case (Dtos.EnumProperties.LineItemStatus.Accepted):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Accepted; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Backordered):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Backordered; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Closed):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Closed; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Invoiced):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Invoiced; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Outstanding):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Paid):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Paid; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Reconciled):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Reconciled; break;
                                    case (Dtos.EnumProperties.LineItemStatus.Voided):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Voided; break;
                                    case (Dtos.EnumProperties.LineItemStatus.AwaitingReceipt):
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Hold; break;
                                    default:
                                        apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding; break;
                                }
                                if (crntDetails != accountDetails.Status)
                                {
                                    IntegrationApiExceptionAddError("The LineItem accountDetails have conflicting status.Cannot have different status on the same LineItem.", "Validation.Exception",
                                     purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                                if (crntDetailsDate != null && crntDetailsDate.HasValue && accountDetails.StatusDate != null && accountDetails.StatusDate.HasValue && crntDetailsDate != accountDetails.StatusDate)
                                {
                                    IntegrationApiExceptionAddError("The LineItem accountDetails have conflicting status date. Cannot have different status date on the same LineItem.", "Validation.Exception",
                                     purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                            }

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
                                    currency = allocated.Amount.Currency.ToString();
                                }

                                if (allocated.Percentage.HasValue)
                                    distributionPercent = Convert.ToDecimal(allocated.Percentage);
                            }

                            string accountingString = string.Empty;
                            try
                            {
                                accountingString = ConvertAccountingString(GLCompCount, accountDetails.AccountingString);
                            }
                            catch (Exception ex)
                            {
                                IntegrationApiExceptionAddError(string.Format("An unexpected error occurred extracting the accounting string '{0}'. Detail {1}.", accountDetails.AccountingString, ex.Message), "Validation.Exception",
                                           purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }


                            var glDistribution = new LineItemGlDistribution(accountingString, distributionQuantity, distributionAmount, distributionPercent);

                            if (accountDetails.SubmittedBy != null && !string.IsNullOrEmpty(accountDetails.SubmittedBy.Id))
                            {
                                try
                                {
                                    var submittedById = await personRepository.GetPersonIdFromGuidAsync(accountDetails.SubmittedBy.Id);
                                    if (string.IsNullOrEmpty(submittedById))
                                    {
                                        IntegrationApiExceptionAddError(string.Format("Person record not found for Line Items Account Details SubmittedBy '{0}'.", accountDetails.SubmittedBy.Id), "Validation.Exception",
                                            purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                    }
                                    else
                                    {
                                        glDistribution.SubmittedBy = submittedById;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Person record not found for Line Items Account Details SubmittedBy '{0}'", accountDetails.SubmittedBy.Id), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                            }

                            apLineItem.AddGlDistribution(glDistribution);

                            //validate taxForm Component
                            if ((accountDetails.TaxFormComponent != null) && (!string.IsNullOrEmpty(accountDetails.TaxFormComponent.Id)))
                            {
                                var taxBoxEntities = await GetAllBoxCodesAsync(false);
                                if (taxBoxEntities == null)
                                {
                                    IntegrationApiExceptionAddError("An error occurred extracting all tax - form - components", "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }

                                var taxBox = taxBoxEntities.FirstOrDefault(c => c.Guid == accountDetails.TaxFormComponent.Id);
                                if (taxBox == null)
                                {
                                    IntegrationApiExceptionAddError(string.Format("Unable to determine tax form components represented by guid:  '{0}'", accountDetails.TaxFormComponent.Id), "Validation.Exception",
                                       purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                                }
                                else
                                {
                                    apLineItem.TaxFormCode = taxBox.Code;
                                    apLineItem.TaxForm = taxBox.TaxCode;

                                }
                            }
                        }

                        // LineItem.Status property new for 11.1.0
                        if (lineItem.Status !=  null && lineItem.Status != Dtos.EnumProperties.LineItemStatus.NotSet)
                        {
                            var status = lineItem.Status;
                            switch (status)
                            {
                                case (Dtos.EnumProperties.LineItemStatus.Accepted):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Accepted; break;
                                case (Dtos.EnumProperties.LineItemStatus.Backordered):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Backordered; break;
                                case (Dtos.EnumProperties.LineItemStatus.Closed):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Closed; break;
                                case (Dtos.EnumProperties.LineItemStatus.Invoiced):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Invoiced; break;
                                case (Dtos.EnumProperties.LineItemStatus.Outstanding):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding; break;
                                case (Dtos.EnumProperties.LineItemStatus.Paid):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Paid; break;
                                case (Dtos.EnumProperties.LineItemStatus.Reconciled):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Reconciled; break;
                                case (Dtos.EnumProperties.LineItemStatus.Voided):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Voided; break;
                                case (Dtos.EnumProperties.LineItemStatus.AwaitingReceipt):
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Hold; break;
                                default:
                                    apLineItem.LineItemStatus = Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding; break;
                            }
                            if (crntDetails != null && crntDetails != Dtos.EnumProperties.LineItemStatus.NotSet && crntDetails != lineItem.Status)
                            {
                                IntegrationApiExceptionAddError("The lineItem status and accountDetails status are not the same.  Cannot have different status within the same LineItem.", "Validation.Exception",
                                 purchaseOrder.Id, purchaseOrderId != "NEW" ? purchaseOrderId : null);
                            }
                        }

                        if (lineItemTaxCodes != null && lineItemTaxCodes.Any())
                        {
                            foreach (var lineItemTaxCode in lineItemTaxCodes)
                            {
                                apLineItem.AddTax(new LineItemTax(lineItemTaxCode, 0));
                            }
                        }
                    }
                    purchaseOrderEntity.AddLineItem(apLineItem);
                }
                if (!string.IsNullOrEmpty(currency))
                    purchaseOrderEntity.CurrencyCode = currency;
            }
            return purchaseOrderEntity;
        }

        private PurchaseOrderStatus GetPurchaseOrderStatus(PurchaseOrders2 purchaseOrder)
        {
            PurchaseOrderStatus poStatus;
            switch (purchaseOrder.Status)
            {
                case PurchaseOrdersStatus.InProgress:
                    poStatus = PurchaseOrderStatus.InProgress; //"U";
                    break;
                case PurchaseOrdersStatus.Notapproved:
                    poStatus = PurchaseOrderStatus.NotApproved; //"N";
                    break;
                case PurchaseOrdersStatus.Outstanding:
                    poStatus = PurchaseOrderStatus.Outstanding; //"O";
                    break;
                case PurchaseOrdersStatus.Accepted:
                    poStatus = PurchaseOrderStatus.Accepted; //"A";
                    break;
                case PurchaseOrdersStatus.Backordered:
                    poStatus = PurchaseOrderStatus.Backordered; //"B";
                    break;
                case PurchaseOrdersStatus.Invoiced:
                    poStatus = PurchaseOrderStatus.Invoiced; //"I";
                    break;
                case PurchaseOrdersStatus.Paid:
                    poStatus = PurchaseOrderStatus.Paid; //"P";
                    break;
                case PurchaseOrdersStatus.Reconciled:
                    poStatus = PurchaseOrderStatus.Reconciled; //"R";
                    break;
                case PurchaseOrdersStatus.Voided:
                    poStatus = PurchaseOrderStatus.Voided; //"V";
                    break;
                case PurchaseOrdersStatus.Closed:
                    poStatus = PurchaseOrderStatus.Closed; //"C";
                    break;

                default:
                    // if we get here, we have corrupt data.
                    throw new ApplicationException("Invalid purchase order status for purchase order: " + purchaseOrder.Status.ToString());
            }

            return poStatus;
        }

        /// <summary>
        /// Helper method to validate Purchase Order PUT/POST.
        /// </summary>
        /// <param name="purchaseOrdersDto">Purchase Order DTO object of type <see cref="Dtos.PurchaseOrders2"/></param>
        private void ValidatePurchaseOrder(Dtos.PurchaseOrders2 purchaseOrdersDto)
        {
            var defaultCurrency = new CurrencyIsoCode?();
            if (purchaseOrdersDto.Vendor == null)
            {
                IntegrationApiExceptionAddError("The Vendor is required when submitting a purchase order.", "Missing.Required.Property");
            }
            if (purchaseOrdersDto.Vendor != null && purchaseOrdersDto.Vendor.ExistingVendor != null
                && purchaseOrdersDto.Vendor.ExistingVendor.Vendor != null && string.IsNullOrEmpty(purchaseOrdersDto.Vendor.ExistingVendor.Vendor.Id))
            {
                IntegrationApiExceptionAddError("The vendor id is required when submitting a purchase order with an existing vendor.", "Missing.Required.Property");
            }
            if (purchaseOrdersDto.Vendor != null 
                && (purchaseOrdersDto.Vendor.ExistingVendor == null 
                || purchaseOrdersDto.Vendor.ExistingVendor.Vendor == null 
                || string.IsNullOrEmpty(purchaseOrdersDto.Vendor.ExistingVendor.Vendor.Id))
                && (purchaseOrdersDto.Vendor.ManualVendorDetails == null
                || string.IsNullOrEmpty(purchaseOrdersDto.Vendor.ManualVendorDetails.Name)))
            {
                IntegrationApiExceptionAddError("Either vendor.existingVendor.vendor.id or vendor.manualVendorDetails.name is required when submitting a purchase order.", "Missing.Required.Property");
            }
            if (purchaseOrdersDto.OrderedOn == default(DateTime))
            {
                IntegrationApiExceptionAddError("OrderedOn is a required field.", "Missing.Required.Property");
            }

            if (purchaseOrdersDto.TransactionDate == default(DateTime))
            {
                IntegrationApiExceptionAddError("TransactionDate is a required field", "Missing.Required.Property");
            }

            if (purchaseOrdersDto.OrderedOn > purchaseOrdersDto.TransactionDate)
            {
                IntegrationApiExceptionAddError("TransactionDate cannot before OrderedOn date.", "Validation.Exception");
            }

            if (purchaseOrdersDto.DeliveredBy != default(DateTime) && purchaseOrdersDto.OrderedOn > purchaseOrdersDto.DeliveredBy)
            {
                IntegrationApiExceptionAddError("DeliveredBy date cannot be before the OrderedOn date.", "Validation.Exception");
            }
            if (purchaseOrdersDto.StatusDate != default(DateTime) && purchaseOrdersDto.OrderedOn > purchaseOrdersDto.StatusDate && purchaseOrdersDto.Status == PurchaseOrdersStatus.Voided)
            {
                IntegrationApiExceptionAddError("StatusDate date cannot be before the OrderedOn date when Voiding the purchase order.", "Validation.Exception");
            }

            if (purchaseOrdersDto.OverrideShippingDestination != null && purchaseOrdersDto.OverrideShippingDestination.Place != null)
            {
                if (purchaseOrdersDto.OverrideShippingDestination.Place.Country != null && purchaseOrdersDto.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.CAN && purchaseOrdersDto.OverrideShippingDestination.Place.Country.Code.Value != IsoCode.USA)
                {
                     IntegrationApiExceptionAddError("Country code can only be CAN or USA.", "Validation.Exception");
                }
                if (purchaseOrdersDto.OverrideShippingDestination.Contact != null && !string.IsNullOrEmpty(purchaseOrdersDto.OverrideShippingDestination.Contact.Extension) && purchaseOrdersDto.OverrideShippingDestination.Contact.Extension.Length > 4)
                {
                     IntegrationApiExceptionAddError("The Extension cannot be greater then 4 in length.", "Validation.Exception");
                }
            }
            if (purchaseOrdersDto.Vendor != null && purchaseOrdersDto.Vendor.ManualVendorDetails != null && purchaseOrdersDto.Vendor.ManualVendorDetails.Place != null)
            {
                if (purchaseOrdersDto.Vendor.ManualVendorDetails.Place.Country != null &&
                    purchaseOrdersDto.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.CAN && purchaseOrdersDto.Vendor.ManualVendorDetails.Place.Country.Code.Value != IsoCode.USA)
                {
                   IntegrationApiExceptionAddError("Country code can only be CAN or USA.", "Validation.Exception");
                }
            }
            if (purchaseOrdersDto.PaymentSource != null && string.IsNullOrEmpty(purchaseOrdersDto.PaymentSource.Id))
            {
                IntegrationApiExceptionAddError("PaymentSource id is required when submitting a PaymentSource.", "Validation.Exception");
            }
            if (purchaseOrdersDto.Comments != null)
            {
                foreach (var comments in purchaseOrdersDto.Comments)
                {
                    if (comments.Comment == null)
                    {
                         IntegrationApiExceptionAddError("Comments required a comment.", "Validation.Exception");
                    }
                    if ((comments.Type == CommentTypes.NotSet) || (comments.Type == null))
                    {
                        IntegrationApiExceptionAddError("Type is required for a comment.", "Validation.Exception");
                    }
                }
            }
            if (purchaseOrdersDto.Buyer != null && purchaseOrdersDto.Buyer.Id == null)
            {
                IntegrationApiExceptionAddError("Buyer Id is required for a Buyer object.", "Validation.Exception");
            }
            if (purchaseOrdersDto.Initiator != null && purchaseOrdersDto.Initiator.Detail != null && string.IsNullOrWhiteSpace(purchaseOrdersDto.Initiator.Detail.Id))
            {
                IntegrationApiExceptionAddError("The Initiator detail Id is required for an Initiator detail object.", "Validation.Exception");
            }
            if (purchaseOrdersDto.PaymentTerms != null && string.IsNullOrWhiteSpace(purchaseOrdersDto.PaymentTerms.Id))
            {
                IntegrationApiExceptionAddError("The PaymentTerms Id is required for a PaymentTerms object.", "Validation.Exception");
            }
            if (purchaseOrdersDto.Classification != null && string.IsNullOrWhiteSpace(purchaseOrdersDto.Classification.Id))
            {
                IntegrationApiExceptionAddError("The Classification Id is required for a Classification object.", "Validation.Exception");
            }
            if (purchaseOrdersDto.SubmittedBy != null && string.IsNullOrWhiteSpace(purchaseOrdersDto.SubmittedBy.Id))
            {
                IntegrationApiExceptionAddError("The SubmittedBy Id is required for a SubmittedBy object.", "Validation.Exception");
            }
            if (purchaseOrdersDto.ShippingMethod != null && string.IsNullOrWhiteSpace(purchaseOrdersDto.ShippingMethod.Id))
            {
                IntegrationApiExceptionAddError("The ShippingMethod Id is required for a ShippingMethod object.", "Validation.Exception");
            }

            if (purchaseOrdersDto.LineItems == null)
            {
                IntegrationApiExceptionAddError("At least one line item must be provided when submitting a purchase order.", "Missing.Required.Property");
            }
            if (purchaseOrdersDto.LineItems != null)
            {
                foreach (var lineItem in purchaseOrdersDto.LineItems)
                {
                    if (lineItem.CommodityCode != null && string.IsNullOrWhiteSpace(lineItem.CommodityCode.Id))
                    {
                        IntegrationApiExceptionAddError("The commodity code id is required when submitting a commodity code.", "Validation.Exception");
                    }
                    if (lineItem.UnitOfMeasure != null && string.IsNullOrWhiteSpace(lineItem.UnitOfMeasure.Id))
                    {
                        IntegrationApiExceptionAddError("The UnitofMeasure id is required when submitting a UnitofMeasure.", "Validation.Exception");
                    }
                    if (lineItem.UnitPrice != null && (!lineItem.UnitPrice.Value.HasValue || lineItem.UnitPrice.Currency == null))
                    {
                         IntegrationApiExceptionAddError("Both the unit price amount value and currency are required when submitting a line item unit price.", "Validation.Exception");
                    }
                    if (lineItem.UnitPrice != null)
                    {
                        defaultCurrency = CheckCurrency(defaultCurrency, lineItem.UnitPrice.Currency);
                    }
                    if (lineItem.AdditionalAmount != null && (!lineItem.AdditionalAmount.Value.HasValue || lineItem.AdditionalAmount.Currency == null))
                    {
                        IntegrationApiExceptionAddError("The additional amount value and currency are required when submitting a line item additional price.", "Validation.Exception");
                    }
                    if (lineItem.AdditionalAmount != null)
                    {
                        defaultCurrency = CheckCurrency(defaultCurrency, lineItem.AdditionalAmount.Currency);
                    }
                    if (lineItem.TaxCodes != null)
                    {
                        foreach (var lineItemTaxes in lineItem.TaxCodes)
                        {
                            if (lineItemTaxes.Id == null)
                            {
                                IntegrationApiExceptionAddError("The Taxes.TaxCode is required when submitting a line item Tax Code.", "Validation.Exception");
                            }
                        }
                    }

                    if (lineItem.TradeDiscount != null)
                    {
                        if (lineItem.TradeDiscount.Amount != null && lineItem.TradeDiscount.Percent != null)
                        {
                            IntegrationApiExceptionAddError("TradeDiscount cannot contain both an Amount and Percentage.", "Validation.Exception");
                        }
                        if (lineItem.TradeDiscount.Amount != null && (!lineItem.TradeDiscount.Amount.Value.HasValue || lineItem.TradeDiscount.Amount.Currency == null))
                        {
                            IntegrationApiExceptionAddError("TradeDiscount amount requires both an Amount and Currency.", "Validation.Exception");
                        }
                        if (lineItem.AdditionalAmount != null)
                        {
                            defaultCurrency = CheckCurrency(defaultCurrency, lineItem.AdditionalAmount.Currency);
                        }
                    }

                    try
                    {
                        if (lineItem.Reference != null)
                        {
                            var referenceDoc = lineItem.Reference;
                            // Check to see if the reference line item differ, If they do then we have to make sure that the are the same Item number
                            if (referenceDoc.lineItemNumber != lineItem.LineItemNumber)
                            {
                                if (!string.IsNullOrEmpty(referenceDoc.lineItemNumber) && string.IsNullOrEmpty(lineItem.LineItemNumber))
                                {
                                    lineItem.LineItemNumber = referenceDoc.lineItemNumber;
                                }
                                else
                                {
                                    lineItem.Reference.lineItemNumber = lineItem.LineItemNumber;
                                    referenceDoc.lineItemNumber = lineItem.LineItemNumber;
                                }
                            }

                            if (referenceDoc.Document != null && referenceDoc.Document.PurchasingArrangement != null)
                            {
                                IntegrationApiExceptionAddError("The Document of Purchasing Arrangement is not acceptable in this system.", "Validation.Exception");
                            }

                            if (referenceDoc.Document != null && referenceDoc.Document.Requisition.Id == null)
                            {
                                IntegrationApiExceptionAddError("The requisition ID is a required field.", "Validation.Exception");
                            }

                            if (string.IsNullOrEmpty(referenceDoc.lineItemNumber))
                            {
                                IntegrationApiExceptionAddError("The Line number is a required field for a reference to requisitions.", "Validation.Exception");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError("An unexpected error occurred: " + ex.Message, "Validation.Exception");
                    }

                    if (lineItem != null && lineItem.AccountDetail != null)
                    {
                        var lineTaxForm = string.Empty;
                        foreach (var accountDetail in lineItem.AccountDetail)
                        {
                            if (accountDetail.AccountingString == null)
                            {
                                IntegrationApiExceptionAddError("The AccountingString is required when submitting a line item account detail.", "Validation.Exception");
                            }
                            if (accountDetail.Allocation == null)
                            {
                                IntegrationApiExceptionAddError("The Allocation is required when submitting a line item account detail.", "Validation.Exception");
                            }
                            else
                            {
                                var allocation = accountDetail.Allocation;

                                var allocated = allocation.Allocated;
                                if (allocated.Amount != null && (!allocated.Amount.Value.HasValue || allocated.Amount.Currency == null))
                                {
                                    IntegrationApiExceptionAddError("The Allocation.Allocated value and currency are required when submitting a line item AccountDetail.Allocation.Allocated.", "Validation.Exception");

                                }
                                if (allocated.Amount != null)
                                {
                                    defaultCurrency = CheckCurrency(defaultCurrency, allocated.Amount.Currency);
                                }

                                if (allocation.TaxAmount != null && (!allocation.TaxAmount.Value.HasValue || allocation.TaxAmount.Currency == null))
                                {
                                     IntegrationApiExceptionAddError("The tax amount value and currency are required when submitting a line item account detail allocation tax amount.", "Validation.Exception");
                                }
                                if (allocation.TaxAmount != null)
                                {
                                    defaultCurrency = CheckCurrency(defaultCurrency, allocation.TaxAmount.Currency);
                                }
                                if (allocation.AdditionalAmount != null && (!allocation.AdditionalAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    IntegrationApiExceptionAddError("The additional amount value and currency are required when submitting a line item account detail allocation additional amount.", "Validation.Exception");
                                }
                                if (allocation.AdditionalAmount != null)
                                {
                                    defaultCurrency = CheckCurrency(defaultCurrency, allocation.AdditionalAmount.Currency);
                                }
                                if (allocation.DiscountAmount != null && (!allocation.DiscountAmount.Value.HasValue || allocation.AdditionalAmount.Currency == null))
                                {
                                    IntegrationApiExceptionAddError("The discount amount value and currency are required when submitting a line item account detail allocation discount amount.", "Validation.Exception");
                                }
                                if (allocation.DiscountAmount != null)
                                {
                                    defaultCurrency = CheckCurrency(defaultCurrency, allocation.DiscountAmount.Currency);
                                }
                            }
                            if (accountDetail.TaxFormComponent != null && string.IsNullOrEmpty(accountDetail.TaxFormComponent.Id))
                            {
                                IntegrationApiExceptionAddError("The taxFormComponent id is required when submitting a line item account detail taxFormComponent.", "Validation.Exception");
                            }
                            else
                            {
                                if (accountDetail.TaxFormComponent != null && !string.IsNullOrEmpty(accountDetail.TaxFormComponent.Id))
                                {
                                    //check the guid in each account details to make sure they are the same.
                                    if (string.IsNullOrEmpty(lineTaxForm)) // this is the first taxform
                                        lineTaxForm = accountDetail.TaxFormComponent.Id;
                                    else if (lineTaxForm != accountDetail.TaxFormComponent.Id)
                                        IntegrationApiExceptionAddError(string.Format("The taxFormComponents must be identical for each account detail for the line item '{0}'.", lineItem.Description), "Validation.Exception");
                                }
                            }
                            if (accountDetail.SubmittedBy != null && string.IsNullOrEmpty(accountDetail.SubmittedBy.Id))
                            {
                                IntegrationApiExceptionAddError("The SubmittedBy id is required when submitting a line item account detail SubmittedBy.", "Validation.Exception");
                            }
                            if (string.IsNullOrEmpty(accountDetail.AccountingString))
                            {
                                IntegrationApiExceptionAddError("The AccountingString id is required when submitting a line item account detail AccountingString.", "Validation.Exception");
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(lineItem.Description))
                    {
                        IntegrationApiExceptionAddError("The Description is required when submitting a line item.", "Validation.Exception");
                    }
                    if (lineItem.Quantity == 0)
                    {
                        IntegrationApiExceptionAddError("The Quantity is required when submitting a line item.", "Validation.Exception");
                    }
                    if (lineItem.UnitPrice == null)
                    {
                        IntegrationApiExceptionAddError("The UnitPrice is required when submitting a line item.", "Validation.Exception");
                    }
                    else
                    {
                        if (lineItem.UnitPrice != null && lineItem.UnitPrice.Currency != null)
                        {
                            defaultCurrency = CheckCurrency(defaultCurrency, lineItem.UnitPrice.Currency);
                        }
                        else
                        {
                            // throw new ArgumentNullException("purchaseOrders.LineItems.UnitPrice", "The UnitPrice currency is a required when submitting a line item.");
                            IntegrationApiExceptionAddError("The UnitPrice currency is a required when submitting a line item.", "Validation.Exception");
                        }
                        if (!lineItem.UnitPrice.Value.HasValue)
                        {
                            //throw new ArgumentNullException("purchaseOrders.LineItems.UnitPrice", "The UnitPrice value is required when submitting a line item.");
                            IntegrationApiExceptionAddError("The UnitPrice value is required when submitting a line item.", "Validation.Exception");
                        }
                    }
                    if (lineItem.PartNumber != null && lineItem.PartNumber.Length > 11)
                    {
                        // throw new ArgumentNullException("purchaseOrders.LineItems.PartNumber", "The PartNumber cannot exceed 11 characters in length.");
                        IntegrationApiExceptionAddError("The PartNumber cannot exceed 11 characters in length.", "Validation.Exception");
                    }
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
        }


        private CurrencyIsoCode? CheckCurrency(CurrencyIsoCode? defaultValue, CurrencyIsoCode? newValue)
        {
            if (defaultValue != null && defaultValue != newValue && newValue != null)
            {
                IntegrationApiExceptionAddError("All currency codes in the request must be the same and cannot differ.", "Validation.Exception");
            }
            CurrencyIsoCode? cc = newValue == null ? defaultValue : newValue;
            return cc;
        }

        #endregion
       
        #endregion

        #region shared methods
       
        /// <summary>
        ///  Get Currency ISO Code
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="hostCountry"></param>
        /// <returns><see cref="CurrencyIsoCode"></returns>
        private CurrencyIsoCode GetCurrencyIsoCode(string currencyCode, string hostCountry = "USA")
        {
            var currency = CurrencyIsoCode.USD;

            try
            {
                if (!(string.IsNullOrEmpty(currencyCode)))
                {
                    currency = (Dtos.EnumProperties.CurrencyIsoCode)Enum.Parse(typeof(Dtos.EnumProperties.CurrencyIsoCode), currencyCode);
                }
                else
                {
                    currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                        CurrencyIsoCode.USD;
                }
            }
            catch (Exception)
            {
                currency = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                    CurrencyIsoCode.USD;
            }

            return currency;
        }

        /// <summary>
        /// Get Purchase OrderType
        /// </summary>
        /// <param name="sourceStatus"></param>
        /// <returns><see cref="PurchaseOrdersStatus"></returns>
        private PurchaseOrdersTypes ConvertPurchaseOrderTypes(string type)
        {
            var purchaseOrdersType = PurchaseOrdersTypes.NotSet;

            if (type == null)
                return purchaseOrdersType;

            string upperType = type.ToUpper();
            switch (upperType)
            {
                case ("PROCUREMENT"):
                    purchaseOrdersType = PurchaseOrdersTypes.Procurement; break;
                case ("EPROCUREMENT"):
                    purchaseOrdersType = PurchaseOrdersTypes.Eprocurement; break;
                case ("TRAVEL"):
                    purchaseOrdersType = PurchaseOrdersTypes.Travel; break;
                default:
                    break;
            }
            return purchaseOrdersType;
        }

        /// <summary>
        ///  BuildPurchaseOrderLineItem
        /// </summary>
        /// <param name="sourceLineItem"></param>
        /// <param name="sourceId"></param>
        /// <param name="sourceGuid"></param>
        /// <param name="currency"></param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="PurchaseOrdersLineItemsDtoProperty"></returns>
        private async Task<PurchaseOrdersLineItemsDtoProperty> BuildPurchaseOrderLineItem(Domain.ColleagueFinance.Entities.LineItem sourceLineItem, string sourceId, string sourceGuid,
             GeneralLedgerAccountStructure GlConfig, CurrencyIsoCode currency = CurrencyIsoCode.USD, bool bypassCache = false)
        {
            if (sourceLineItem == null)
            {
                IntegrationApiExceptionAddError(string.Concat("Unable to retrieve Purchase Order line item: ", sourceId, " Guid: ", sourceGuid), "Bad.Data");
                return null;
            }
            var lineItem = new PurchaseOrdersLineItemsDtoProperty
            {
                LineItemNumber = sourceLineItem.Id,
                Description = sourceLineItem.Description
            };

            if (!string.IsNullOrEmpty(sourceLineItem.CommodityCode))
            {
                try
                {
                    var commodityCodeGuid = await colleagueFinanceReferenceDataRepository.GetCommodityCodeGuidAsync(sourceLineItem.CommodityCode);

                    if (string.IsNullOrEmpty(commodityCodeGuid))
                    {
                        var message = string.Concat("Missing commodity code information.  CommodityCode: ", sourceLineItem.CommodityCode);
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", sourceGuid, sourceId);
                    }
                    else
                    {
                        lineItem.CommodityCode = new GuidObject2(commodityCodeGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", sourceGuid, sourceId);
                }
            }
            if (!(string.IsNullOrWhiteSpace(sourceLineItem.VendorPart)))
            {
                lineItem.PartNumber = sourceLineItem.VendorPart;
            }

            lineItem.DesiredDate = sourceLineItem.ExpectedDeliveryDate;
            lineItem.Quantity = sourceLineItem.Quantity;

            if (!string.IsNullOrEmpty(sourceLineItem.UnitOfIssue))
            {
                try
                {
                    var unitTypeGuid = await colleagueFinanceReferenceDataRepository.GetCommodityUnitTypeGuidAsync(sourceLineItem.UnitOfIssue);

                    if (string.IsNullOrEmpty(unitTypeGuid))
                    {
                        var message = string.Concat("Missing commodity code unit type information.  UnitOfIssue: ", sourceLineItem.UnitOfIssue);
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", sourceGuid, sourceId);
                    }
                    else
                    {
                        lineItem.UnitOfMeasure = new GuidObject2(unitTypeGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", sourceGuid, sourceId);
                }
            }

            lineItem.UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty()
            {
                Value = sourceLineItem.Price,
                Currency = currency
            };

            var lineItemTaxCodes = new List<GuidObject2>();

            if ((sourceLineItem.LineItemTaxes != null) && (sourceLineItem.LineItemTaxes.Any()))
            {
                var taxCodesEntities = await GetCommerceTaxCodesAsync(bypassCache);
                if (taxCodesEntities == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve commodity tax codes", "GUID.Not.Found", sourceGuid, sourceId);
                }
                else
                {
                    var lineItemTaxesTuple = sourceLineItem.LineItemTaxes
                        .GroupBy(l => l.TaxCode)
                        .Select(cl => new Tuple<string, decimal?>(
                               cl.First().TaxCode,
                               cl.Sum(c => c.TaxAmount)
                            )).ToList();

                    foreach (var lineItemTax in lineItemTaxesTuple)
                    {
                        var taxCode = taxCodesEntities.FirstOrDefault(tax => tax.Code == lineItemTax.Item1);
                        if (taxCode != null)
                        {
                            lineItemTaxCodes.Add(new GuidObject2(taxCode.Guid));
                        }
                    }
                }
            }
            if (lineItemTaxCodes != null && lineItemTaxCodes.Any())
            {
                lineItem.TaxCodes = lineItemTaxCodes;
            }

            if (sourceLineItem.TradeDiscountAmount != null && sourceLineItem.TradeDiscountAmount.HasValue)
            {
                var tradeDiscountDtoProperty = new TradeDiscountDtoProperty();

                tradeDiscountDtoProperty.Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = sourceLineItem.TradeDiscountAmount,
                    Currency = currency
                };

                lineItem.TradeDiscount = tradeDiscountDtoProperty;
            }
            else if (sourceLineItem.TradeDiscountPercentage != null && sourceLineItem.TradeDiscountPercentage.HasValue)
            {
                lineItem.TradeDiscount = new TradeDiscountDtoProperty()
                {
                    Percent = sourceLineItem.TradeDiscountPercentage
                };
            }

            if (!string.IsNullOrEmpty(sourceLineItem.Comments))
            {
                var lineItemComments = new List<PurchaseOrdersCommentsDtoProperty>();

                lineItemComments.Add(new PurchaseOrdersCommentsDtoProperty()
                {
                    Comment = sourceLineItem.Comments,
                    Type = CommentTypes.NotPrinted
                });

                lineItem.Comments = lineItemComments;
            }

            if (!string.IsNullOrEmpty(sourceLineItem.RequisitionId))
            {
                lineItem.Reference = new PurchaseOrdersReferenceDtoProperty();
                lineItem.Reference.Document = new PurchaseOrdersDocumentDtoProperty();
                try
                {
                    var reqGuid = await purchaseOrderRepository.GetGuidFromIdAsync(sourceLineItem.RequisitionId, "REQUISITIONS");
                    if (string.IsNullOrEmpty(reqGuid))
                    {
                        var message = string.Concat("Missing requisition GUID.  RequisitionId: ", sourceLineItem.RequisitionId);
                        IntegrationApiExceptionAddError(message, "GUID.Not.Found", sourceGuid, sourceId);
                    }
                    else
                    {
                        lineItem.Reference.Document.Requisition = new GuidObject2(reqGuid);
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "Bad.Data", sourceGuid, sourceId);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "Bad.Data", sourceGuid, sourceId);
                }

                lineItem.Reference.lineItemNumber = lineItem.LineItemNumber;
            }


            var accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>();
            int sequenceNumber = 0;
            foreach (var glDistribution in sourceLineItem.GlDistributions)
            {
                if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                {
                    var accountDetail = new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty();

                    sequenceNumber++;
                    accountDetail.SequenceNumber = sequenceNumber;

                    string acctNumber = string.Empty;
                    try
                    {
                        var acctNumberInitial = glDistribution.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                        acctNumber = GetFormattedGlAccount(acctNumberInitial, GlConfig);
                        if (!string.IsNullOrEmpty(glDistribution.ProjectId))
                        {
                            acctNumber = ConvertAccountingstringToIncludeProjectRefNo(glDistribution.ProjectId, acctNumber);
                        }
                    }

                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "Bad.Data", sourceGuid, sourceId);
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "Bad.Data", sourceGuid, sourceId);
                    }

                    accountDetail.AccountingString = acctNumber;

                    var allocation = new Dtos.DtoProperties.PurchaseOrdersAllocationDtoProperty();

                    var allocated = new Dtos.DtoProperties.PurchaseOrdersAllocatedDtoProperty();

                    allocated.Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Value = glDistribution.Amount,
                        Currency = currency
                    };

                    allocated.Percentage = glDistribution.Percent;
                    allocated.Quantity = glDistribution.Quantity;

                    allocation.Allocated = allocated;

                    if ((sourceLineItem.LineItemTaxes != null) && (sourceLineItem.LineItemTaxes.Any()))
                    {
                        var glTax = sourceLineItem.LineItemTaxes.Where(lit => lit.LineGlNumber == glDistribution.GlAccountNumber)
                            .Sum(c => c.TaxAmount);

                        if (glTax != 0)
                        {
                            allocation.TaxAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                                Value = glTax,
                                Currency = currency
                            };
                        }
                    }
                    accountDetail.Allocation = allocation;

                    if (!(string.IsNullOrWhiteSpace(sourceLineItem.TaxFormCode)))
                    {
                        var taxBoxes = await GetAllBoxCodesAsync(bypassCache);
                        if (taxBoxes != null)
                        {
                            var taxBox = taxBoxes.FirstOrDefault(aps => aps.Code == sourceLineItem.TaxFormCode);
                            if (taxBox != null)
                            {
                                accountDetail.TaxFormComponent = new GuidObject2(taxBox.Guid);
                            }
                        }
                    }

                    if (sourceLineItem.StatusDate != null && sourceLineItem.StatusDate.HasValue)
                    {
                        accountDetail.StatusDate = sourceLineItem.StatusDate;
                    }

                    if (sourceLineItem.LineItemStatus != null)
                    {
                        var status = this.ConvertLineItemStatus(sourceLineItem.LineItemStatus);
                        // Account Details status cannot contain awaitingReceipt enumeration
                        if (status != Dtos.EnumProperties.LineItemStatus.AwaitingReceipt)
                        {
                            accountDetail.Status = status;
                        }
                    }
                    accountDetails.Add(accountDetail);
                }
            }
            if (accountDetails != null && accountDetails.Any())
            {
                lineItem.AccountDetail = accountDetails;
            }

            // New Line Item status at line item level for v11.1.0
            if (sourceLineItem.LineItemStatus != null)
            {
                lineItem.Status = this.ConvertLineItemStatus(sourceLineItem.LineItemStatus);
            }

            return lineItem;
        }

        /// <summary>
        /// GetPurchaseOrderStatus enumeration from PurchaseOrderStatus domian enum
        /// </summary>
        /// <param name="sourceStatus"></param>
        /// <returns><see cref="PurchaseOrdersStatus"></returns>
        private PurchaseOrdersStatus ConvertPurchaseOrderStatus(Domain.ColleagueFinance.Entities.PurchaseOrderStatus? sourceStatus)
        {
            var purchaseOrdersStatus = PurchaseOrdersStatus.NotSet;

            if (sourceStatus == null)
                return purchaseOrdersStatus;

            switch (sourceStatus)
            {
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Accepted):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Accepted; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Backordered):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Backordered; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Closed):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Closed; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress):
                    purchaseOrdersStatus = PurchaseOrdersStatus.InProgress; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Invoiced):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Invoiced; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.NotApproved):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Notapproved; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Outstanding):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Outstanding; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Paid):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Paid; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Reconciled):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Reconciled; break;
                case (Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Voided):
                    purchaseOrdersStatus = PurchaseOrdersStatus.Voided; break;
                default:
                    break;
            }
            return purchaseOrdersStatus;
        }

        /// <summary>
        /// GetPurchaseOrderStatus enumeration from PurchaseOrderStatus domian enum
        /// </summary>
        /// <param name="sourceStatus"></param>
        /// <returns><see cref="PurchaseOrdersStatus"></returns>
        private Dtos.EnumProperties.LineItemStatus ConvertLineItemStatus(Domain.ColleagueFinance.Entities.LineItemStatus? sourceStatus)
        {
            var lineItemStatus = Dtos.EnumProperties.LineItemStatus.NotSet;

            if (sourceStatus == null)
                return lineItemStatus;

            switch (sourceStatus)
            {
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Accepted):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Accepted; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Backordered):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Backordered; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Closed):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Closed; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Invoiced):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Invoiced; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Outstanding; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Paid):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Paid; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Reconciled):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Reconciled; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Voided):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Voided; break;
                case (Domain.ColleagueFinance.Entities.LineItemStatus.Hold):
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.AwaitingReceipt; break;
                default:
                    lineItemStatus = Dtos.EnumProperties.LineItemStatus.Outstanding; break;
            }
            return lineItemStatus;
        }

        /// <summary>
        /// Build an OverrideShippingDestinationDtoProperty DTO object from a PurchaseOrder entity
        /// </summary>
        /// <param name="source">PurchaseOrder Entity Object</param>
        /// <returns>An <see cref="OverrideShippingDestinationDtoProperty"> OverrideShippingDestinationDtoProperty object <see cref="OverrideShippingDestinationDtoProperty"/> in EEDM format</returns>
        private async Task<OverrideShippingDestinationDtoProperty> BuildOverrideShippingDestinationDtoAsync(Domain.ColleagueFinance.Entities.PurchaseOrder source,
            bool bypassCache = false)
        {
            var overrideShippingDestinationDto = new Dtos.OverrideShippingDestinationDtoProperty();

            if (!string.IsNullOrEmpty(source.AltShippingName))
            {
                overrideShippingDestinationDto.Description = source.AltShippingName;
            }

            if (source.AltShippingAddress != null && source.AltShippingAddress.Any())
            {
                overrideShippingDestinationDto.AddressLines = source.AltShippingAddress;
            }

            overrideShippingDestinationDto.Place = await BuildAddressPlace(source.MiscCountry,
                source.AltShippingCity, source.AltShippingState, source.AltShippingZip,
                source.HostCountry, source.AltShippingCountry, bypassCache);

            if (!string.IsNullOrEmpty(source.AltShippingPhone) || !string.IsNullOrEmpty(source.AltShippingPhoneExt))
            {
                var contact = new PhoneDtoProperty();
                if (!string.IsNullOrEmpty(source.AltShippingPhone))
                    contact.Number = source.AltShippingPhone;
                if (!string.IsNullOrEmpty(source.AltShippingPhoneExt))
                    contact.Extension = source.AltShippingPhoneExt;
                overrideShippingDestinationDto.Contact = contact;
            }

            if (overrideShippingDestinationDto != null &&
               (overrideShippingDestinationDto.AddressLines != null || overrideShippingDestinationDto.Contact != null
               || overrideShippingDestinationDto.Place != null || !string.IsNullOrEmpty(overrideShippingDestinationDto.Description)))
            {
                return overrideShippingDestinationDto;
            }
            return null;
        }

        /// <summary>
        /// Build an ManualVendorDetailsDtoProperty DTO object from a PurchaseOrder entity
        /// </summary>
        /// <param name="source">PurchaseOrder Entity Object</param>
        /// <returns>An <see cref="ManualVendorDetailsDtoProperty"> ManualVendorDetailsDtoProperty object <see cref="ManualVendorDetailsDtoProperty"/> in EEDM format</returns>
        private async Task<ManualVendorDetailsDtoProperty> BuildManualVendorDetailsDtoAsync(Domain.ColleagueFinance.Entities.PurchaseOrder source,
            bool bypassCache = false)
        {
            var manualVendorDetailsDto = new Dtos.ManualVendorDetailsDtoProperty();

            if (source.MiscName != null && source.MiscName.Any())
            {
                manualVendorDetailsDto.Name = source.MiscName.FirstOrDefault();
            }

            if (source.MiscAddress != null && source.MiscAddress.Any())
            {
                manualVendorDetailsDto.AddressLines = source.MiscAddress;
            }

            switch (source.MiscIntgCorpPersonFlag)
            {
                case "Organization":
                    manualVendorDetailsDto.Type = ManualVendorType.Organization;
                    break;
                case "Person":
                    manualVendorDetailsDto.Type = ManualVendorType.Person;
                    break;
            }

            manualVendorDetailsDto.Place = await BuildAddressPlace(source.MiscCountry,
                source.MiscCity, source.MiscState, source.MiscZip,
                source.HostCountry, "", bypassCache);

            if (manualVendorDetailsDto != null &&
               (manualVendorDetailsDto.AddressLines != null || !(string.IsNullOrWhiteSpace(manualVendorDetailsDto.Name))
               || manualVendorDetailsDto.Place != null))
            {
                return manualVendorDetailsDto;
            }
            return null;
        }

        /// <summary>
        /// Build an AddressPlace DTO from address components
        /// </summary>
        /// <param name="addressCountry"></param>
        /// <param name="addressCity"></param>
        /// <param name="addressState"></param>
        /// <param name="addressZip"></param>
        /// <param name="hostCountry"></param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="AddressPlace"></returns>
        private async Task<AddressPlace> BuildAddressPlace( string addressCountry, string addressCity,
            string addressState, string addressZip, string hostCountry,string intgAltShipCountry = "",  bool bypassCache = false)
        {
            var addressCountryDto = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;
            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(intgAltShipCountry))
            {

                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == intgAltShipCountry
                    || x.IsoCode == intgAltShipCountry);
                if (country == null)
                {
                    try
                    {
                        if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        {
                            country = await referenceDataRepository.GetCountryFromIsoAlpha3CodeAsync("USA");
                        }
                        else
                        {
                            country = await referenceDataRepository.GetCountryFromIsoAlpha3CodeAsync("CAN");
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "Bad.Data");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(addressCountry))
            {
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressCountry);
                if (country == null)
                {
                    IntegrationApiExceptionAddError("Unable to locate country code for " + addressCountry, "Bad.Data");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(addressState))
                {
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
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
                    try
                    {
                        if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        {
                            country = await referenceDataRepository.GetCountryFromIsoAlpha3CodeAsync("USA");
                        }
                        else
                        {
                            country = await referenceDataRepository.GetCountryFromIsoAlpha3CodeAsync("CAN");
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex, "Bad.Data");
                    }
                }
            }

            //need to check to make sure ISO code is there.
            if (country != null && string.IsNullOrEmpty(country.IsoAlpha3Code))
                IntegrationApiExceptionAddError("ISO country code missing for country: '" + country.Code + "'", "Bad.Data");
            else
            {
                switch (country.IsoAlpha3Code)
                {
                    case "USA":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.USA;
                        addressCountryDto.PostalTitle = "UNITED STATES OF AMERICA";
                        break;
                    case "CAN":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.CAN;
                        addressCountryDto.PostalTitle = "CANADA";
                        break;
                    case "AUS":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.AUS;
                        addressCountryDto.PostalTitle = "AUSTRALIA";
                        break;
                    case "BRA":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.BRA;
                        addressCountryDto.PostalTitle = "BRAZIL";
                        break;
                    case "MEX":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.MEX;
                        addressCountryDto.PostalTitle = "MEXICO";
                        break;
                    case "NLD":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.NLD;
                        addressCountryDto.PostalTitle = "NETHERLANDS";
                        break;
                    case "GBR":
                        addressCountryDto.Code = Dtos.EnumProperties.IsoCode.GBR;
                        addressCountryDto.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                        break;
                    default:
                        try
                        {
                            addressCountryDto.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                            addressCountryDto.PostalTitle = country.Description.ToUpper();
                        }
                        catch (Exception ex)
                        {
                            IntegrationApiExceptionAddError(string.Concat(ex.Message, "For the Country: '", addressCountry, "' ISOCode not found: '", country.IsoAlpha3Code, "'"), "Bad.Data");
                        }                       
                        break;
                }
            }
            if (!string.IsNullOrEmpty(addressState))
            {
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
                else
                {
                    IntegrationApiExceptionAddError(string.Concat("Description not found for for the state: '", addressState, "' or an error has occurred retrieving that value."), "Bad.Data");
                }
            }
            
            if (region != null)
            {
                addressCountryDto.Region = region;
            }

            if (!string.IsNullOrEmpty(addressCity))
            {
                addressCountryDto.Locality = addressCity;
            }
            addressCountryDto.PostalCode = addressZip;

            if (country != null)
                addressCountryDto.Title = country.Description;

            if ((addressCountryDto != null) &&
                (!string.IsNullOrEmpty(addressCountryDto.Locality) || !string.IsNullOrEmpty(addressCountryDto.PostalCode) || addressCountryDto.Region != null))
            {
                return new AddressPlace()
                {
                    Country = addressCountryDto
                };
            }

            return null;
        }
       
        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information related to purchase orders that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewPurchaseOrderPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewPurchaseOrders);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Purchase Orders.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a UPDATE operation. This API will integrate information related to purchase orders that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdatePurchaseOrderPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.UpdatePurchaseOrders);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update Purchase Orders.");
            }
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
                catch (ArgumentOutOfRangeException)
                {
                    throw new InvalidOperationException(string.Format("Invalid GL account number: {0}", accountNumber));
                }
            }
            formattedGlAccount = tempGlNo;

            return formattedGlAccount;
        }

        #endregion

        /// <summary>
        /// Create/Update a purchase order.
        /// </summary>
        /// <param name="purchaseOrderCreateUpdateRequest">The purchase order create update request DTO.</param>        
        /// <returns>The purchase order create update response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse> CreateUpdatePurchaseOrderAsync(Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest purchaseOrderCreateUpdateRequest)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse response = new Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse();
            if (purchaseOrderCreateUpdateRequest == null)
            {
                throw new ArgumentNullException("purchaseOrderCreateRequest", "Must provide a purchaseOrderCreateRequest object");
            }

            if (string.IsNullOrEmpty(purchaseOrderCreateUpdateRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            if (purchaseOrderCreateUpdateRequest.PurchaseOrder == null)
            {
                throw new ArgumentNullException("purchasOrder", "PurchasOrder must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(purchaseOrderCreateUpdateRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(purchaseOrderCreateUpdateRequest.PersonId);

            //Change to create or update permission, after creating new permission.            
            CheckPurchaseOrderCreateUpdatePermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            //Convert DTO to domain entity            
            var purchaseOrderCreateUpdateRequestEntity = ConvertCreateUpdateRequestDtoToEntity(purchaseOrderCreateUpdateRequest, glConfiguration);
            Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateResponse responseEntity = null;

            //Check if purchase order sent for modify exist in colleague for user.            
            if (!string.IsNullOrEmpty(purchaseOrderCreateUpdateRequest.PurchaseOrder.Id))
            {
                // Get the ID for the person who is logged in, and use the ID to get their GL access level.
                var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
                if (generalLedgerUser == null)
                {
                    throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
                }

                // Get the purchase order domain entity from the repository
                var originalPurchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync(purchaseOrderCreateUpdateRequest.PurchaseOrder.Id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);
                if (originalPurchaseOrder == null)
                {
                    var message = string.Format("{0} Purchase Order number doesn't exist for modify.", purchaseOrderCreateUpdateRequest.PurchaseOrder.Number);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                responseEntity = await purchaseOrderRepository.UpdatePurchaseOrderAsync(purchaseOrderCreateUpdateRequestEntity, originalPurchaseOrder);
            }
            else
            {
                responseEntity = await purchaseOrderRepository.CreatePurchaseOrderAsync(purchaseOrderCreateUpdateRequestEntity);

            }

            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateResponse, Dtos.ColleagueFinance.PurchaseOrderCreateUpdateResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }
            return response;
        }

        /// <summary>
        /// Void a purchase order.
        /// </summary>
        /// <param name="purchaseOrderVoidRequest">The purchase order void request DTO.</param>        
        /// <returns>The purchase order void response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidResponse> VoidPurchaseOrderAsync(Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidRequest purchaseOrderVoidRequest)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderVoidResponse response = new Dtos.ColleagueFinance.PurchaseOrderVoidResponse();
            if (purchaseOrderVoidRequest == null)
            {
                throw new ArgumentNullException("purchaseOrderVoidRequest", "Must provide a purchaseOrderVoidRequest object");
            }

            if (string.IsNullOrEmpty(purchaseOrderVoidRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }

            if (string.IsNullOrEmpty(purchaseOrderVoidRequest.PurchaseOrderId))
            {
                throw new ArgumentNullException("PurchaseOrderId", "PurchaseOrder Id must be specified.");
            }

            if (string.IsNullOrEmpty(purchaseOrderVoidRequest.ConfirmationEmailAddresses))
            {
                throw new ArgumentNullException("confirmationEmailAddresses", "confirmationEmailAddresses must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(purchaseOrderVoidRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(purchaseOrderVoidRequest.PersonId);

            //Change to create or update permission, after creating new permission.            
            CheckPurchaseOrderCreateUpdatePermission();

            //Convert DTO to domain entity            
            var purchaseOrderVoidRequestEntity = ConvertVoidRequestDtoToEntity(purchaseOrderVoidRequest);
            Domain.ColleagueFinance.Entities.PurchaseOrderVoidResponse responseEntity = null;

            responseEntity = await purchaseOrderRepository.VoidPurchaseOrderAsync(purchaseOrderVoidRequestEntity);


            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderVoidResponse, Dtos.ColleagueFinance.PurchaseOrderVoidResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }
            return response;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckPurchaseOrderViewPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewPurchaseOrder) || HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view purchase orders.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows bypass of tax forms update on a purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private bool CheckBypassTaxformsPermission()
        {
            return HasPermission(ColleagueFinancePermissionCodes.ByPassTaxForms);
        }

        /// <summary>
        /// Permission code that allows bypass of approvals on a purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private bool CheckBypassApprovalsPermission()
        {
            return HasPermission(ColleagueFinancePermissionCodes.ByPassPurchaseOrderApproval);
        }

        /// <summary>
        /// convert create/Update PO Request to Entity
        /// </summary>
        /// <param name="requisitionCreateUpdateRequest"></param>
        /// <param name="glAccountStructure"></param>
        /// <returns>PurchaseOrderCreateUpdateRequest</returns>
        private  Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateRequest ConvertCreateUpdateRequestDtoToEntity(Dtos.ColleagueFinance.PurchaseOrderCreateUpdateRequest requisitionCreateUpdateRequest, GeneralLedgerAccountStructure glAccountStructure)
        {
            Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateRequest createUpdateRequestEntity = new Domain.ColleagueFinance.Entities.PurchaseOrderCreateUpdateRequest();
            createUpdateRequestEntity.PersonId = requisitionCreateUpdateRequest.PersonId;
            createUpdateRequestEntity.ConfEmailAddresses = requisitionCreateUpdateRequest.ConfEmailAddresses;
            createUpdateRequestEntity.InitiatorInitials = requisitionCreateUpdateRequest.InitiatorInitials;
            createUpdateRequestEntity.IsPersonVendor = requisitionCreateUpdateRequest.IsPersonVendor;
            createUpdateRequestEntity.PurchaseOrder = ConvertPurchaseOrderDtoToEntity(requisitionCreateUpdateRequest.PurchaseOrder, glAccountStructure);
            return createUpdateRequestEntity;
        }

        /// <summary>
        /// Permission code that allows a WRITE operation on a purchase Order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckPurchaseOrderCreateUpdatePermission()
        {

            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to create or modify Purchase Orders.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Convert Purchase order Dto to Entity
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <param name="glAccountStructure"></param>
        /// <returns>PurchaseOrder Entity</returns>
        private PurchaseOrder ConvertPurchaseOrderDtoToEntity(Dtos.ColleagueFinance.PurchaseOrder purchaseOrder, GeneralLedgerAccountStructure glAccountStructure)
        {

            if (purchaseOrder == null)
                throw new ArgumentNullException("PurchaseOrder", "Must provide PurchaseOrder object");

            var poStatus = ConvertPurchaseOrderStatusDtoToEntity(purchaseOrder.Status);
            var purchaseOrderId = !(string.IsNullOrEmpty(purchaseOrder.Id)) ? purchaseOrder.Id.Trim() : "NEW";
            var purchaseOrderNumber = !(string.IsNullOrEmpty(purchaseOrder.Number)) ? purchaseOrder.Number : "new";

            var purchaseOrderEntity = new Domain.ColleagueFinance.Entities.PurchaseOrder(
                purchaseOrderId, "NEW", purchaseOrderNumber, purchaseOrder.VendorName, poStatus, purchaseOrder.StatusDate, purchaseOrder.Date);

            if (!(string.IsNullOrWhiteSpace(purchaseOrder.ShipToCode)))
            {
                purchaseOrderEntity.ShipToCode = purchaseOrder.ShipToCode.ToUpper();
            }
            if ((purchaseOrder.Approvers != null) && (purchaseOrder.Approvers.Any()))
            {
                foreach (var approver in purchaseOrder.Approvers)
                {
                    if (approver != null && !string.IsNullOrEmpty(approver.ApproverId))
                    {
                        // Approver Initials needs to be uppercase
                        var approverEntity = new Domain.ColleagueFinance.Entities.Approver(approver.ApproverId.ToUpper());
                        purchaseOrderEntity.AddApprover(approverEntity);
                    }
                }
            }
            if ((purchaseOrder.LineItems != null) && (purchaseOrder.LineItems.Any()))
            {
                foreach (var lineItem in purchaseOrder.LineItems)
                {
                    if (lineItem != null)
                    {
                        var description = !(string.IsNullOrEmpty(lineItem.Description)) ? lineItem.Description.Trim() : string.Empty;
                        //description = description.Length <= 25 ? description : description.Substring(0, 25);
                        decimal quantity = lineItem.Quantity;

                        var lineItemId = !(string.IsNullOrEmpty(lineItem.Id)) ? lineItem.Id.Trim() : "NEW";

                        var apLineItem = new Domain.ColleagueFinance.Entities.LineItem(lineItemId, description, quantity, lineItem.Price, lineItem.ExtendedPrice);
                        apLineItem.VendorPart = !(string.IsNullOrEmpty(lineItem.VendorPart)) ? lineItem.VendorPart.Trim() : string.Empty;
                        apLineItem.UnitOfIssue = !(string.IsNullOrEmpty(lineItem.UnitOfIssue)) ? lineItem.UnitOfIssue.Trim() : string.Empty;
                        apLineItem.Comments = !(string.IsNullOrEmpty(lineItem.Comments)) ? lineItem.Comments.Trim() : string.Empty;
                        apLineItem.TradeDiscountPercentage = lineItem.TradeDiscountPercentage;
                        apLineItem.TradeDiscountAmount = lineItem.TradeDiscountAmount;
                        apLineItem.FixedAssetsFlag = !(string.IsNullOrEmpty(lineItem.FixedAssetsFlag)) ? lineItem.FixedAssetsFlag.Trim() : string.Empty;

                        apLineItem.CommodityCode = lineItem.CommodityCode;
                        apLineItem.ExpectedDeliveryDate = lineItem.ExpectedDeliveryDate;
                        apLineItem.TaxForm = lineItem.TaxForm;
                        apLineItem.TaxFormCode = lineItem.TaxFormCode;
                        apLineItem.TaxFormLocation = lineItem.TaxFormLocation;

                        foreach (var glAccount in lineItem.GlDistributions)
                        {
                            string glAccountNo = !(string.IsNullOrEmpty(glAccount.FormattedGlAccount)) ? glAccount.FormattedGlAccount : "MASKED";
                            var internalGlAccountNo = GlAccountUtility.ConvertGlAccountToInternalFormat(glAccountNo, glAccountStructure.MajorComponentStartPositions);

                            apLineItem.AddGlDistributionForSave(new Domain.ColleagueFinance.Entities.LineItemGlDistribution(internalGlAccountNo, glAccount.Quantity, glAccount.Amount)
                            {
                                ProjectNumber = glAccount.ProjectNumber
                            });
                        }
                        if (purchaseOrderId.Equals("NEW"))
                        {
                            //add tax codes to the entity
                            if (lineItem.ReqLineItemTaxCodes != null && lineItem.ReqLineItemTaxCodes.Any())
                            {
                                foreach (var taxcode in lineItem.ReqLineItemTaxCodes)
                                {
                                    if (taxcode != null && !string.IsNullOrEmpty(taxcode.TaxReqTaxCode))
                                    {
                                        apLineItem.AddReqTax(new Domain.ColleagueFinance.Entities.LineItemReqTax(taxcode.TaxReqTaxCode));
                                    }
                                }
                            }
                        }
                        else
                        {
                            //add tax codes to the entity
                            if (lineItem.ReqLineItemTaxCodes != null && lineItem.ReqLineItemTaxCodes.Any())
                            {
                                foreach (var taxcode in lineItem.ReqLineItemTaxCodes)
                                {
                                    if (!string.IsNullOrEmpty(taxcode.TaxReqTaxCode))
                                    {
                                        apLineItem.AddReqTax(new Domain.ColleagueFinance.Entities.LineItemReqTax(taxcode.TaxReqTaxCode));
                                    }
                                }
                            }
                        }
                        purchaseOrderEntity.AddLineItem(apLineItem);
                    }
                }
            }
            purchaseOrderEntity.VendorId = purchaseOrder.VendorId;
            if (!(string.IsNullOrWhiteSpace(purchaseOrder.ApType)))
            {
                purchaseOrderEntity.ApType = purchaseOrder.ApType.ToUpper();
            }
            if ((purchaseOrder.Approvers != null) && (purchaseOrder.Approvers.Any()))
            {
                foreach (var approver in purchaseOrder.Approvers)
                {
                    if (!string.IsNullOrEmpty(approver.ApproverId))
                    {
                        var approverEntity = new Domain.ColleagueFinance.Entities.Approver(approver.ApproverId);
                        purchaseOrderEntity.AddApprover(approverEntity);
                    }
                }
            }

            purchaseOrderEntity.Comments = purchaseOrder.Comments;
            purchaseOrderEntity.InternalComments = purchaseOrder.InternalComments;
            purchaseOrderEntity.DeliveryDate = purchaseOrder.DeliveryDate;
            purchaseOrderEntity.DefaultCommodityCode = purchaseOrder.DefaultCommodityCode;
            purchaseOrderEntity.CommodityCode = purchaseOrder.DefaultCommodityCode;
            return purchaseOrderEntity;

        }

        /// <summary>
        /// Converts a PurchaseOrderStatus DTO enumeration to a PurchaseOrderStatus domain enum 
        /// </summary>
        /// <param name="sourceStatus">PurchaseOrderStatus DTO enumeration</param>
        /// <returns><see cref="RequisitionsStatus">PurchaseOrderStatus domain enum</returns>
        private  Domain.ColleagueFinance.Entities.PurchaseOrderStatus ConvertPurchaseOrderStatusDtoToEntity(Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus sourceStatus)
        {
            var purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress;

            switch (sourceStatus)
            {
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.InProgress):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.NotApproved):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.NotApproved; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Outstanding):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Outstanding; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Accepted):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Accepted; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Backordered):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Backordered; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Invoiced):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Invoiced; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Paid):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Paid; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Reconciled):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Reconciled; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Voided):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Voided; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrderStatus.Closed):
                    purchaseOrderStatus = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Closed; break;
               
                default:
                    break;
            }
            return purchaseOrderStatus;
        }

        /// <summary>
        /// convert create/Update PO Request to Entity
        /// </summary>
        /// <param name="requisitionCreateUpdateRequest"></param>
        /// <param name="glAccountStructure"></param>
        /// <returns>PurchaseOrderCreateUpdateRequest</returns>
        private static Domain.ColleagueFinance.Entities.PurchaseOrderVoidRequest ConvertVoidRequestDtoToEntity(Dtos.ColleagueFinance.PurchaseOrderVoidRequest purchaseOrderVoidRequest)
        {
            Domain.ColleagueFinance.Entities.PurchaseOrderVoidRequest voidRequestEntity = new Domain.ColleagueFinance.Entities.PurchaseOrderVoidRequest();
            voidRequestEntity.PersonId = purchaseOrderVoidRequest.PersonId;
            voidRequestEntity.PurchaseOrderId = purchaseOrderVoidRequest.PurchaseOrderId;
            voidRequestEntity.ConfirmationEmailAddresses = purchaseOrderVoidRequest.ConfirmationEmailAddresses;
            voidRequestEntity.InternalComments = purchaseOrderVoidRequest.InternalComments;
            
            return voidRequestEntity;
        }
    }
}