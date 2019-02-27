// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

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
        IDictionary<string, string> _projectReferenceIds = null;

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
        }


        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> _commodityUnitType = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType>> GetCommodityUnitTypesAsync(bool bypassCache)
        {
            if (_commodityUnitType == null)
            {
                _commodityUnitType = await colleagueFinanceReferenceDataRepository.GetCommodityUnitTypesAsync(false);
            }
            return _commodityUnitType;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> _vendorTerm = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm>> GetVendorTermsAsync(bool bypassCache)
        {
            if (_vendorTerm == null)
            {
                _vendorTerm = await colleagueFinanceReferenceDataRepository.GetVendorTermsAsync(false);
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
            await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(false);
            return _accountsPayableSources ?? (_accountsPayableSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(false));
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType> _freeOnBoardType = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool bypassCache)
        {
            if (_freeOnBoardType == null)
            {
                _freeOnBoardType = await colleagueFinanceReferenceDataRepository.GetFreeOnBoardTypesAsync(false);
            }
            return _freeOnBoardType;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination> _shipToDestination = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination>> GetShipToDestinationsAsync(bool bypassCache)
        {
            if (_shipToDestination == null)
            {
                _shipToDestination = await colleagueFinanceReferenceDataRepository.GetShipToDestinationsAsync(false);
            }
            return _shipToDestination;
        }

        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> _commodityCode = null;
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode>> GetCommodityCodesAsync(bool bypassCache)
        {
            if (_commodityCode == null)
            {
                _commodityCode = await colleagueFinanceReferenceDataRepository.GetCommodityCodesAsync(false);
            }
            return _commodityCode;
        }

        private IEnumerable<Domain.Base.Entities.CommerceTaxCode> _commerceTaxCode = null;
        private async Task<IEnumerable<Domain.Base.Entities.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache)
        {
            if (_commerceTaxCode == null)
            {
                _commerceTaxCode = await referenceDataRepository.GetCommerceTaxCodesAsync(false);
            }
            return _commerceTaxCode;
        }

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await referenceDataRepository.GetCountryCodesAsync(false);
            }
            return _countries;
        }
            
        private IEnumerable<State> _states = null;
        private async Task<IEnumerable<State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await referenceDataRepository.GetStateCodesAsync(false);
            }
            return _states;
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

            // Convert the purchase order and all its child objects into DTOs
            var purchaseOrderDtoAdapter = new PurchaseOrderEntityToDtoAdapter(_adapterRegistry, logger);
            var purchaseOrderDto = purchaseOrderDtoAdapter.MapToType(purchaseOrderDomainEntity, glConfiguration.MajorComponentStartPositions);

            if (purchaseOrderDto.LineItems == null || purchaseOrderDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access purchase order.");
            }

            return purchaseOrderDto;
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
        public async Task<Tuple<IEnumerable<PurchaseOrders2>, int>> GetPurchaseOrdersAsync2(int offset, int limit, bool bypassCache = false)
        {
            CheckViewPurchaseOrderPermission();

            var purchaseOrdersCollection = new List<Ellucian.Colleague.Dtos.PurchaseOrders2>();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            var purchaseOrderEntities = await purchaseOrderRepository.GetPurchaseOrdersAsync(offset, limit);
            var totalRecords = purchaseOrderEntities.Item2;
            if (purchaseOrderEntities.Item1 != null)
            {
                try
                {
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
                                var accountsPayableInvoiceDto = await this.ConvertPurchaseOrderEntityToDtoAsync2(purchaseOrderEntity, glConfiguration, bypassCache);
                                purchaseOrdersCollection.Add(accountsPayableInvoiceDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Concat(ex.Message, "  Purchase Order: '", purchaseOrderEntity.Number, "', Entity: 'PURCHASE.ORDERS', Record ID: '", purchaseOrderEntity.Id, "'"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return new Tuple<IEnumerable<Dtos.PurchaseOrders2>, int>(purchaseOrdersCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get PurchaseOrder data from a Guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.PurchaseOrders">A PurchaseOrders DTO</see></returns>
        public async Task<PurchaseOrders2> GetPurchaseOrdersByGuidAsync2(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Purchase Order.");
            }
            CheckViewPurchaseOrderPermission();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            try
            {
                var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrdersByGuidAsync(guid);
                if (purchaseOrder == null)
                {
                    throw new KeyNotFoundException(string.Format("Purchase Order not found for guid: {0} ", guid));
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
                return await ConvertPurchaseOrderEntityToDtoAsync2(purchaseOrder, glConfiguration);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No Purchase Order was found for guid '{0}'.", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Format("No Purchase Order was found for guid '{0}'.", guid), ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException(string.Format("No Purchase Order was found for guid '{0}'.", guid), ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Converts an PurchaseOrder domain entity to its corresponding PurchaseOrder DTO
        /// </summary>
        /// <param name="source">PurchaseOrder domain entity</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.PurchaseOrders">PurchaseOrders</see></returns>
        private async Task<Dtos.PurchaseOrders2> ConvertPurchaseOrderEntityToDtoAsync2(Domain.ColleagueFinance.Entities.PurchaseOrder source, 
            GeneralLedgerAccountStructure glConfiguration, bool bypassCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source is required to obtain a Purchase Order.");
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
                var submittedByGuid = await personRepository.GetPersonGuidFromIdAsync(source.SubmittedBy);
                if (string.IsNullOrEmpty(submittedByGuid))
                {
                    throw new Exception(string.Concat("Missing Submitted By information for purchase order: ", source.Id, " Guid: ", source.Guid, " Submitted By: ", source.SubmittedBy));
                }
                purchaseOrder.SubmittedBy = new GuidObject2(submittedByGuid);
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
                var buyerGuid = await buyerRepository.GetBuyerGuidFromIdAsync(source.Buyer);
                if (string.IsNullOrEmpty(buyerGuid))
                {
                    throw new Exception(string.Concat("Missing buyer information for purchase order: ", source.Id, " Guid: ", source.Guid, " Buyer: ", source.Buyer));
                }
                purchaseOrder.Buyer = new GuidObject2(buyerGuid);
            }

            var initiator = new Dtos.DtoProperties.PurchaseOrdersInitiatorDtoProperty();
            initiator.Name = source.InitiatorName;
            if (!string.IsNullOrEmpty(source.DefaultInitiator))
            {
                var initiatorGuid = await personRepository.GetPersonGuidFromIdAsync(source.DefaultInitiator);
                if (string.IsNullOrEmpty(initiatorGuid))
                {
                    throw new KeyNotFoundException(string.Concat("Missing initiator information for purchase order: ", source.Id, " Guid: ", source.Guid, " Initiator: ", source.DefaultInitiator));
                }
                initiator.Detail = new GuidObject2(initiatorGuid);
            }
            if ((!string.IsNullOrEmpty(initiator.Name)) || (initiator.Detail != null))
            {
                purchaseOrder.Initiator = initiator;
            }

            var purchaseOrdersShipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty();
            if (!string.IsNullOrEmpty(source.ShipToCode))
            {
                var shipToDestinations = await GetShipToDestinationsAsync(bypassCache);
                if (shipToDestinations == null)
                {
                    throw new Exception("Unable to retrieve ShipToDestination");
                }
                var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Code == source.ShipToCode);
                if (shipToDestination == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to retrieve ShipToDestination for: ", source.ShipToCode));
                }
                purchaseOrdersShipping.ShipTo = new GuidObject2(shipToDestination.Guid);
                purchaseOrder.Shipping = purchaseOrdersShipping;
            }
            if (!string.IsNullOrEmpty(source.Fob))
            {
                var freeOnBoardTypes = await GetFreeOnBoardTypesAsync(bypassCache);
                if (freeOnBoardTypes == null)
                {
                    throw new Exception("Unable to retrieve FreeOnBoardTypes");
                }
                var freeOnBoardType = freeOnBoardTypes.FirstOrDefault(fob => fob.Code == source.Fob);
                if (freeOnBoardType == null)
                {
                    throw new KeyNotFoundException(string.Concat("Unable to retrieve FreeOnBoardTypes for: ", source.Fob));
                }
                purchaseOrdersShipping.FreeOnBoard = new GuidObject2(freeOnBoardType.Guid);
                purchaseOrder.Shipping = purchaseOrdersShipping;
            }


            purchaseOrder.OverrideShippingDestination = await BuildOverrideShippingDestinationDtoAsync(source, bypassCache);

            var purchaseOrderVendor = new PurchaseOrdersVendorDtoProperty2();
            

            if (!string.IsNullOrEmpty(source.VendorId))
            {
                purchaseOrderVendor.ExistingVendor = new PurchaseOrdersExistingVendorDtoProperty();
                var vendorGuid = await vendorsRepository.GetVendorGuidFromIdAsync(source.VendorId);
                if (string.IsNullOrEmpty(vendorGuid))
                {
                    throw new Exception(string.Concat("Missing vendor GUID for purchase order: ", source.Id, " Guid: ", source.Guid, " Vendor ID: ", source.VendorId));
                }
                
                purchaseOrderVendor.ExistingVendor.Vendor = new GuidObject2(vendorGuid);

                if (!string.IsNullOrEmpty(source.VendorAddressId) && (source.AltAddressFlag == true))
                {
                    var vendorAddressGuid = await purchaseOrderRepository.GetGuidFromIdAsync(source.VendorAddressId,"ADDRESS");
                    if (string.IsNullOrEmpty(vendorAddressGuid))
                    {
                        throw new Exception(string.Concat("Missing GUID for existin vendor address: ", source.Id, " Guid: ", source.Guid, " Vendor ID: ", source.VendorId));
                    }
                    purchaseOrderVendor.ExistingVendor.AlternativeVendorAddress = new GuidObject2(vendorAddressGuid);
                }
            } 

            if ((purchaseOrderVendor.ExistingVendor == null) || (purchaseOrderVendor.ExistingVendor.AlternativeVendorAddress == null && source.AltAddressFlag == true) )
                purchaseOrderVendor.ManualVendorDetails = await BuildManualVendorDetailsDtoAsync(source, bypassCache);

            if (purchaseOrderVendor.ExistingVendor != null || purchaseOrderVendor.ManualVendorDetails != null)
            {
                purchaseOrder.Vendor = purchaseOrderVendor;
            }
            else
            {
                throw new Exception(string.Concat("Vendor is required: ", source.Id, " Guid: ", source.Guid));
            }

            if (!string.IsNullOrEmpty(source.VendorTerms))
            {
                var vendorTerms = await GetVendorTermsAsync(bypassCache);
                if (vendorTerms == null)
                    throw new Exception("Unable to retrieve vendor terms");
                var vendorTerm = vendorTerms.FirstOrDefault(vt => vt.Code == source.VendorTerms);
                if (vendorTerm == null)
                {
                    throw new KeyNotFoundException(string.Concat("Missing vendor term information for purchase order: ", source.Id, " Guid: ", source.Guid, " Vendor Term: ", source.VendorTerms));
                }
                purchaseOrder.PaymentTerms = new GuidObject2(vendorTerm.Guid);
            }


            if (!string.IsNullOrEmpty(source.ApType))
            {
                var accountsPayableSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(false);
                if (accountsPayableSources == null)
                    throw new Exception("Unable to retrieve accounts payable sources");
                var accountsPayableSource = accountsPayableSources.FirstOrDefault(aps => aps.Code == source.ApType);
                if (accountsPayableSource == null)
                {
                    throw new KeyNotFoundException(string.Concat("Missing accounts payable source information for purchase order: ", source.Id, " Guid: ", source.Guid, " Ap Type: ", source.ApType));
                }
                purchaseOrder.PaymentSource = new GuidObject2(accountsPayableSource.Guid);
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
        /// EEDM V10 Put purchase order
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        public async Task<PurchaseOrders2> PutPurchaseOrdersAsync2(string guid, PurchaseOrders2 purchaseOrder)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException("purchaseOrder", "Must provide a purchaseOrder for update");
            if (string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrder", "Must provide a guid for purchaseOrder update");

            // get the person ID associated with the incoming guid
            var purchaseOrderId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(purchaseOrder.Id);
            List<Domain.ColleagueFinance.Entities.FundsAvailable> OverRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(purchaseOrderId))
            {
                // verify the user has the permission to create a PurchaseOrders
                CheckUpdatePurchaseOrderPermission();
                var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

                purchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // Don't check funds availability if we are closing the purchase order
                    if (purchaseOrder.Status != PurchaseOrdersStatus.Closed && purchaseOrder.Status != PurchaseOrdersStatus.Voided)
                        OverRideGLs = await checkFunds2(purchaseOrder, purchaseOrderId);

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
                    // map the DTO to entities
                    var purchaseOrderEntity
                    = await ConvertPurchaseOrdersDtoToEntityAsync2(purchaseOrder.Id, purchaseOrder, glConfiguration.MajorComponents.Count);

                    // update the entity in the database
                    var updatedPurchaseOrderEntity =
                        await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrderEntity);


                    var dtoPurchaseOrder = await this.ConvertPurchaseOrderEntityToDtoAsync2(updatedPurchaseOrderEntity, glConfiguration, true);


                    if (dtoPurchaseOrder.LineItems != null && dtoPurchaseOrder.LineItems.Any() && OverRideGLs != null && OverRideGLs.Any())
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
                                var findOvr = OverRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.Sequence == PosID + ".DS");
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
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }

            // perform a create instead
            return await PostPurchaseOrdersAsync2(purchaseOrder);
        }

        /// <summary>
        /// Post the purchase order Async
        /// </summary>
        /// <param name="purchaseOrder"></param>
        /// <returns></returns>
        public async Task<PurchaseOrders2> PostPurchaseOrdersAsync2(PurchaseOrders2 purchaseOrder)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException("purchaseOrders", "Must provide a purchaseOrders for update");
            if (string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrders", "Must provide a guid for purchaseOrders update");


            Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrder createdPurchaseOrder = null;

            List<Domain.ColleagueFinance.Entities.FundsAvailable> OverRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

            // verify the user has the permission to create a PurchaseOrders
            CheckUpdatePurchaseOrderPermission();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            purchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                OverRideGLs = await checkFunds2(purchaseOrder);
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
                var purchaseOrderEntity
                         = await ConvertPurchaseOrdersDtoToEntityAsync2(purchaseOrder.Id, purchaseOrder, glConfiguration.MajorComponents.Count);

                // create a PurchaseOrder entity in the database
                createdPurchaseOrder =
                    await purchaseOrderRepository.UpdatePurchaseOrdersAsync(purchaseOrderEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            var dtoPurchaseOrder = await this.ConvertPurchaseOrderEntityToDtoAsync2(createdPurchaseOrder, glConfiguration, true);


            if (dtoPurchaseOrder.LineItems != null && dtoPurchaseOrder.LineItems.Any() && OverRideGLs != null && OverRideGLs.Any())
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
                        var findOvr = OverRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.Sequence == PosID + ".DS");
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
        private async Task<List<Domain.ColleagueFinance.Entities.FundsAvailable>> checkFunds2(PurchaseOrders2 purchaseOrder, string PoId = "")
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
                            if (details.Status != PurchaseOrdersStatus.Voided 
                                && details.Status != PurchaseOrdersStatus.Closed 
                                && details.Status != PurchaseOrdersStatus.Notapproved
                                && details.Status != PurchaseOrdersStatus.Invoiced
                                && details.Status != PurchaseOrdersStatus.Paid
                                && details.Status != PurchaseOrdersStatus.Reconciled)
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
                            throw new Exception("A line item has two account details with the same GL number " + accountingString + " this is not allowed");
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
                            throw new ArgumentException("The accounting string " + availableFund.AccountString + " does not have funds available");
                        }
                        //if we get a override and if the budgetcheck flag is not set to override then thrown an exception
                        if (availableFund.AvailableStatus == FundsAvailableStatus.Override )
                        {
                            var budOverCheck = budgetOvrCheckTuple.FirstOrDefault(acct => acct.Item1 == availableFund.AccountString);
                            if (budOverCheck != null && budOverCheck.Item2 == false)
                            {
                                throw new ArgumentException("The accounting string " + availableFund.AccountString + " does not have funds available. BudgetCheck flag not set to override.");
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
        private async Task<Domain.ColleagueFinance.Entities.PurchaseOrder> ConvertPurchaseOrdersDtoToEntityAsync2(string purchaseOrderGuid, Dtos.PurchaseOrders2 purchaseOrder, int GLCompCount)
        {
            if (purchaseOrder == null || string.IsNullOrEmpty(purchaseOrder.Id))
                throw new ArgumentNullException("purchaseOrder", "Must provide guid for purchaseOrder");

            if ((purchaseOrder.Vendor != null) && (purchaseOrder.Vendor.ExistingVendor != null) && 
                 (purchaseOrder.Vendor.ExistingVendor.Vendor != null) && (string.IsNullOrEmpty(purchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                throw new ArgumentNullException("purchaseOrder", "Must provide vendor.id for purchaseOrder");

            var poStatus = PurchaseOrderStatus.InProgress;
            if (purchaseOrder.Status != PurchaseOrdersStatus.NotSet && purchaseOrder.Status != null)
            {
                poStatus = GetPurchaseOrderStatus(purchaseOrder);
            }

            var guid = purchaseOrderGuid;
            var purchaseOrderId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(guid);
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
                var submittedById = await personRepository.GetPersonIdFromGuidAsync(purchaseOrder.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    throw new Exception(string.Concat("SubmittedBy GUID is not found: ", purchaseOrder.Id, " Guid: ", purchaseOrder.SubmittedBy.Id));
                }
                purchaseOrderEntity.SubmittedBy = submittedById;
            }
            // New flag to send data to the CTX to bypass the tax forms
            purchaseOrderEntity.bypassTaxForms = CheckBypassTaxformsPermission();
            // New flag to send data to the CTX to bypass approvals
            purchaseOrderEntity.bypassApprovals = CheckBypassApprovalsPermission();

            if (purchaseOrder.TransactionDate != default(DateTime))
                purchaseOrderEntity.MaintenanceDate = purchaseOrder.TransactionDate;

            if ((purchaseOrder.DeliveredBy != default(DateTime)))
                purchaseOrderEntity.DeliveryDate = purchaseOrder.DeliveredBy;

            if (purchaseOrder.Buyer != null)
            {
                var buyerId = await buyerRepository.GetBuyerIdFromGuidAsync(purchaseOrder.Buyer.Id);
                if (string.IsNullOrEmpty(buyerId))
                {
                    throw new Exception(string.Concat("Missing buyer information for purchase order: ", purchaseOrderEntity.Id, " Guid: ", guid, " Buyer: ", purchaseOrder.Buyer.Id));
                }
                purchaseOrderEntity.Buyer = buyerId;
            }

            if (purchaseOrder.Initiator != null && purchaseOrder.Initiator.Detail != null)
            {
                var initiatorId = await personRepository.GetPersonIdFromGuidAsync(purchaseOrder.Initiator.Detail.Id);
                if (string.IsNullOrEmpty(initiatorId))
                {
                    throw new Exception(string.Concat("GUID for initiator detail ID could not find colleague ID: ", purchaseOrderEntity.Id, " Guid: ", guid, " Buyer: ", purchaseOrder.Initiator.Detail.Id));
                }
                purchaseOrderEntity.DefaultInitiator = initiatorId;
            }

            if (purchaseOrder.Shipping != null)
            {
                if (purchaseOrder.Shipping.ShipTo != null)
                {
                    var shipToDestinations = await GetShipToDestinationsAsync(true);
                    if (shipToDestinations == null)
                    {
                        throw new Exception("Unable to retrieve ShipToDestination");
                    }
                    var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Guid == purchaseOrder.Shipping.ShipTo.Id);
                    if (shipToDestination == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to retrieve ShipToDestination for: ", purchaseOrder.Shipping.ShipTo.Id));
                    }
                    purchaseOrderEntity.ShipToCode = shipToDestination.Code;
                }

                if (purchaseOrder.Shipping.FreeOnBoard != null)
                {
                    var freeOnBoardTypes = await GetFreeOnBoardTypesAsync(true);
                    if (freeOnBoardTypes == null)
                    {
                        throw new Exception("Unable to retrieve FreeOnBoardTypes");
                    }
                    var freeOnBoardType = freeOnBoardTypes.FirstOrDefault(fob => fob.Guid == purchaseOrder.Shipping.FreeOnBoard.Id);
                    if (freeOnBoardType == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to retrieve FreeOnBoardTypes for: ", purchaseOrder.Shipping.FreeOnBoard.Id));
                    }
                    purchaseOrderEntity.Fob = freeOnBoardType.Code;
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
            }

            if (purchaseOrder.Vendor != null)
            {
                if (purchaseOrder.Vendor.ExistingVendor != null &&
                    purchaseOrder.Vendor.ExistingVendor.Vendor != null && (!string.IsNullOrEmpty(purchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                {
                    string vendorId = string.Empty;
                    try
                    {
                        vendorId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(purchaseOrder.Vendor.ExistingVendor.Vendor.Id);
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException
                            (string.Concat("The vendor id must correspond with a valid vendor record : ", purchaseOrder.Vendor.ExistingVendor.Vendor.Id));
                    }
                    if (!string.IsNullOrEmpty(vendorId))
                        purchaseOrderEntity.VendorId = vendorId;
                    else
                    {
                        throw new ArgumentNullException("PurchaseOrder", "Must provide valid vendor.ExistingVendor.id for purchaseOrder");
                    }

                    if (purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress != null &&
                        !string.IsNullOrWhiteSpace(purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id))
                    {
                        string vendorAddressId = string.Empty;
                        try
                        {
                            vendorAddressId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException
                                (string.Concat("The vendor alternate address id must correspond with a valid address record : ", purchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id));
                        }
                        if (!string.IsNullOrEmpty(vendorAddressId))
                            purchaseOrderEntity.VendorAddressId = vendorAddressId;
                        else
                        {
                            throw new ArgumentNullException("PurchaseOrder", "Must provide valid vendor.ExistingVendor.alternativeVendorAddress.id for purchaseOrder");
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
                        purchaseOrder.Vendor.ExistingVendor.Vendor == null) && 
                        !manualVendorDetailsDto.Type.HasValue)
                    {
                        throw new ArgumentNullException("PurchaseOrder", "Must provide a valid manual vendor type when not supplying a existing vendor for a purchase order");
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
                    var purchaseOrderSource = (await this.GetAllAccountsPayableSourcesAsync(true)).FirstOrDefault(ap => ap.Guid == payment.Id);
                    if (purchaseOrderSource == null)
                        throw new KeyNotFoundException("PurchaseOrder Source not found for guid: " + payment.Id);
                    purchaseOrderEntity.ApType = purchaseOrderSource.Code;
                }
            }

            if ((purchaseOrder.PaymentTerms != null) && (!string.IsNullOrEmpty(purchaseOrder.PaymentTerms.Id)))
            {
                var vendorTerms = (await this.GetVendorTermsAsync(true)).FirstOrDefault(ap => ap.Guid == purchaseOrder.PaymentTerms.Id);
                if (vendorTerms == null)
                    throw new KeyNotFoundException("PaymentTerm not found for guid: " + purchaseOrder.PaymentTerms.Id);
                purchaseOrderEntity.VendorTerms = vendorTerms.Code;
            }

            if (purchaseOrder.Comments != null && purchaseOrder.Comments.Any())
            {
                foreach (var comment in purchaseOrder.Comments)
                {
                    switch (comment.Type)
                    {
                        case CommentTypes.NotPrinted:
                            purchaseOrderEntity.InternalComments = comment.Comment;
                            break;
                        case CommentTypes.Printed:
                            purchaseOrderEntity.Comments = comment.Comment;
                            break;
                    }
                }
            }

            if ((purchaseOrder.LineItems != null) && (purchaseOrder.LineItems.Any()))
            {
                var allCommodityCodes = (await GetCommodityCodesAsync(true));
                if ((allCommodityCodes == null) || (!allCommodityCodes.Any()))
                {
                    throw new Exception("An error occurred extracting all commodity codes");
                }

                var allCommodityUnitTypes = (await this.GetCommodityUnitTypesAsync(true));
                if ((allCommodityUnitTypes == null) || (!allCommodityUnitTypes.Any()))
                {
                    throw new Exception("An error occurred extracting all commodity unit types");
                }

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
                        var commodityCode = allCommodityCodes.FirstOrDefault(c => c.Guid == lineItem.CommodityCode.Id);
                        if (commodityCode == null)
                        {
                            throw new Exception("Unable to determine commodity code represented by guid: " + lineItem.CommodityCode.Id);
                        }
                        apLineItem.CommodityCode = commodityCode.Code;
                    }

                    if ((lineItem.UnitOfMeasure != null) && (!string.IsNullOrEmpty(lineItem.UnitOfMeasure.Id)))
                    {
                        var commodityUnitType = allCommodityUnitTypes.FirstOrDefault(c => c.Guid == lineItem.UnitOfMeasure.Id);
                        if (commodityUnitType == null)
                        {
                            throw new Exception("Unable to determine commodity unit type represented by guid: " + lineItem.UnitOfMeasure.Id);
                        }
                        apLineItem.UnitOfIssue = commodityUnitType.Code;

                    }
                    if (lineItem.Comments != null && lineItem.Comments.Any())
                    {
                        string comment = "";
                        foreach (var com in lineItem.Comments)
                        {
                            comment = string.Concat(comment, com.Comment);
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
                    apLineItem.DesiredDate = lineItem.DesiredDate;


                    var lineItemTaxCodes = new List<string>();
                    if ((lineItem.TaxCodes != null) && (lineItem.TaxCodes.Any()))
                    {
                        var taxCodesEntities = await GetCommerceTaxCodesAsync(true);

                        if (taxCodesEntities == null)
                        {
                            throw new Exception("An error occurred extracting all commerce tax codes");
                        }
                        foreach (var lineItemTax in lineItem.TaxCodes)
                        {
                            if (lineItemTax != null && lineItemTax.Id != null && !string.IsNullOrEmpty(lineItemTax.Id))
                            {
                                var taxCode = taxCodesEntities.FirstOrDefault(tax => tax.Guid == lineItemTax.Id);

                                if (taxCode == null)
                                {
                                    throw new Exception(string.Concat("Tax code not found for guid:'", lineItemTax.Id, "'"));
                                }
                                if ((taxCode.AppurEntryFlag == false) || (taxCode.UseTaxFlag == true))
                                {
                                    throw new ArgumentException(string.Concat("Tax Code '", taxCode.Code, "' is not permitted for use on AP/PUR documents."));
                                }

                                lineItemTaxCodes.Add(taxCode.Code);
                            }
                        }                    
                    }

                    if (lineItem.Reference != null && !string.IsNullOrEmpty(lineItem.Reference.Document.Requisition.Id))
                    {

                        string reqId = string.Empty;
                        try
                        {
                            reqId = await purchaseOrderRepository.GetPurchaseOrdersIdFromGuidAsync(lineItem.Reference.Document.Requisition.Id);
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException
                                (string.Concat("The requisition id must correspond with a valid requisition record : ", lineItem.Reference.Document.Requisition.Id));
                        }
                        if (!string.IsNullOrEmpty(reqId))
                            apLineItem.RequisitionId = reqId;
                        else
                        {
                            throw new ArgumentNullException("PurchaseOrder", "A reference Document must have a valid Requisition ID");
                        }
                    }

                    if (lineItem.AccountDetail != null && lineItem.AccountDetail.Any())
                    {
                        if (lineItem.AccountDetail[0].StatusDate != null)
                        {
                            apLineItem.StatusDate = lineItem.AccountDetail[0].StatusDate;
                        }
                        var crntDetails = lineItem.AccountDetail[0].Status;
                        foreach (var accountDetails in lineItem.AccountDetail)
                        {

                            if (accountDetails.Status != PurchaseOrdersStatus.NotSet)
                            {
                                var status = accountDetails.Status;
                                switch (status)
                                {
                                    case (PurchaseOrdersStatus.Accepted):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Accepted; break;
                                    case (PurchaseOrdersStatus.Backordered):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Backordered; break;
                                    case (PurchaseOrdersStatus.Closed):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Closed; break;
                                    case (PurchaseOrdersStatus.InProgress):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress; break;
                                    case (PurchaseOrdersStatus.Invoiced):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Invoiced; break;
                                    case (PurchaseOrdersStatus.Notapproved):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.NotApproved; break;
                                    case (PurchaseOrdersStatus.Outstanding):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Outstanding; break;
                                    case (PurchaseOrdersStatus.Paid):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Paid; break;
                                    case (PurchaseOrdersStatus.Reconciled):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Reconciled; break;
                                    case (PurchaseOrdersStatus.Voided):
                                        apLineItem.Status = Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Voided; break;
                                    default:
                                        break;
                                }
                                if (crntDetails != accountDetails.Status)
                                {
                                    throw new Exception("The LineItem accountDetails have conflicting status. Cannot have different status on the same LineItem");
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
                            
                            string accountingString = ConvertAccountingString(GLCompCount, accountDetails.AccountingString);
                            var glDistribution = new LineItemGlDistribution(accountingString, distributionQuantity, distributionAmount, distributionPercent);
                            
                            if (accountDetails.SubmittedBy != null && !string.IsNullOrEmpty(accountDetails.SubmittedBy.Id))
                            {
                                var submittedById = await personRepository.GetPersonIdFromGuidAsync(accountDetails.SubmittedBy.Id);
                                if (string.IsNullOrEmpty(submittedById))
                                {
                                    throw new Exception(string.Concat("Line Items Account Details SubmittedBy GUID is not found: ", purchaseOrder.Id, " Guid: ", accountDetails.SubmittedBy.Id));
                                }
                                glDistribution.SubmittedBy = submittedById;
                            }

                            apLineItem.AddGlDistribution(glDistribution);
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

        #endregion
        #endregion

        #region shared methods
        /// <summary>
        ///  Get Currency ISO Code
        /// </summary>
        /// <param name="currencyCode"></param>
        /// <param name="hostCountry"></param>
        /// <returns><see cref="CurrencyIsoCode"></returns>
        private static CurrencyIsoCode GetCurrencyIsoCode(string currencyCode, string hostCountry = "USA")
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
                throw new ArgumentNullException(string.Concat("Unable to retrieve Purchase Order line item: ", sourceId, " Guid: ", sourceGuid));
            }
            var lineItem = new PurchaseOrdersLineItemsDtoProperty();

            lineItem.LineItemNumber = sourceLineItem.Id;
            lineItem.Description = sourceLineItem.Description;

            if (!string.IsNullOrEmpty(sourceLineItem.CommodityCode))
            {
                var commodityCodes = await GetCommodityCodesAsync(bypassCache);
                if (commodityCodes == null)
                    throw new Exception("Unable to retrieve commodity codes");
                var commodityCode = commodityCodes.FirstOrDefault(cc => cc.Code == sourceLineItem.CommodityCode);
                if (commodityCode == null)
                {
                    throw new KeyNotFoundException(string.Concat("Missing commodity code information for purchase order: ", sourceId, " Guid: ", sourceGuid, " Commodity Code: ", sourceLineItem.CommodityCode));
                }
                lineItem.CommodityCode = new GuidObject2(commodityCode.Guid);
            }
            if (!(string.IsNullOrWhiteSpace(sourceLineItem.VendorPart)))
            {
                lineItem.PartNumber = sourceLineItem.VendorPart;
            }
            lineItem.DesiredDate = sourceLineItem.DesiredDate;
            lineItem.Quantity = sourceLineItem.Quantity;

            if (!string.IsNullOrEmpty(sourceLineItem.UnitOfIssue))
            {
                var commodityCodeUnitTypes = await GetCommodityUnitTypesAsync(bypassCache);
                if (commodityCodeUnitTypes == null)
                {
                    throw new Exception("Unable to retrieve commodity code unit types");
                }
                var commodityCodeUnitType = commodityCodeUnitTypes.FirstOrDefault(cct => cct.Code == sourceLineItem.UnitOfIssue);
                if (commodityCodeUnitType == null)
                {
                    throw new KeyNotFoundException(string.Concat("Missing commodity code unit types information for purchase order: ", sourceId, " Guid: ", sourceGuid,
                        " Commodity Code Unit Type: ", sourceLineItem.UnitOfIssue));
                }
                lineItem.UnitOfMeasure = new GuidObject2(commodityCodeUnitType.Guid);
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
                    throw new Exception("Unable to retrieve commodity tax codes");
                }
                var lineItemTaxesTuple = sourceLineItem.LineItemTaxes
                    .GroupBy(l => l.TaxCode)
                    .Select(cl => new Tuple<string, decimal>(
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

            try
            {
                if (!string.IsNullOrEmpty(sourceLineItem.RequisitionId))
                {
                    lineItem.Reference = new PurchaseOrdersReferenceDtoProperty();
                    lineItem.Reference.Document = new PurchaseOrdersDocumentDtoProperty();
                    var ReqGuid = new GuidObject2(await purchaseOrderRepository.GetGuidFromIdAsync(sourceLineItem.RequisitionId, "REQUISITIONS"));
                    lineItem.Reference.Document.Requisition = ReqGuid;

                    lineItem.Reference.lineItemNumber = lineItem.LineItemNumber;
                }
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            
            

            var accountDetails = new List<Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty>();
            int sequenceNumber = 0;
            foreach (var glDistribution in sourceLineItem.GlDistributions)
            {
                if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                {
                    var accountDetail = new Dtos.DtoProperties.PurchaseOrdersAccountDetailDtoProperty();
                    //var acctNumber = glDistribution.GlAccountNumber.Replace("_", "-");
                    sequenceNumber++;
                    accountDetail.SequenceNumber = sequenceNumber;
                    //accountDetail.AccountingString =
                    //    string.IsNullOrEmpty(glDistribution.ProjectId) ?
                    //        acctNumber : string.Concat(acctNumber, '*', glDistribution.ProjectId);
                    string acctNumber = glDistribution.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                    acctNumber = GetFormattedGlAccount(acctNumber, GlConfig);
                    if (!string.IsNullOrEmpty(glDistribution.ProjectId))
                    {
                        acctNumber = ConvertAccountingstringToIncludeProjectRefNo(glDistribution.ProjectId, acctNumber);
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

                    //accountDetail.BudgetCheck = PurchaseOrdersAccountBudgetCheck.NotSet;

                    if (sourceLineItem.StatusDate != null && sourceLineItem.StatusDate.HasValue)
                    {
                        accountDetail.StatusDate = sourceLineItem.StatusDate;
                    }

                    if (sourceLineItem.Status != null)
                    {
                        accountDetail.Status = this.ConvertPurchaseOrderStatus(sourceLineItem.Status);

                    }
                    accountDetails.Add(accountDetail);
                }
            }
            if (accountDetails != null && accountDetails.Any())
            {
                lineItem.AccountDetail = accountDetails;
            }

            return lineItem;
        }

        /// <summary>
        /// GetPurchaseOrderStatus
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

            if (!string.IsNullOrEmpty(source.AltShippingPhone))
            {
                overrideShippingDestinationDto.Contact = new PhoneDtoProperty()
                {
                    Number = source.AltShippingPhone,
                    Extension = source.AltShippingPhoneExt
                };
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
                    // var hostCountry = addressHostCountry;
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            else if(!string.IsNullOrEmpty(addressCountry))
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressCountry);
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
                   // var hostCountry = addressHostCountry;
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(addressCountry))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
            }

            //need to check to make sure ISO code is there.
            if (country != null && string.IsNullOrEmpty(country.IsoAlpha3Code))
                throw new ArgumentException("Unable to locate ISO country code for " + country.Code);

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
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Concat(ex.Message, "For the Country: '", addressCountry, "' .ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountryDto.PostalTitle = country.Description.ToUpper();
                    break;
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
                    throw new ArgumentException(string.Concat("Description not found for for the state: '", addressState, "' or an error has occurred retrieving that value. "));
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

            if (addressCountryDto != null
                && (!string.IsNullOrEmpty(addressCountryDto.Locality)
                || !string.IsNullOrEmpty(addressCountryDto.PostalCode)
                || addressCountryDto.Region != null
                ))
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
        /// Permission code that allows a READ operation on a purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckPurchaseOrderViewPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewPurchaseOrder);

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
    }
}