// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Linq;
using System.Text.RegularExpressions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IBlanketPurchaseOrderService interface
    /// </summary>
    [RegisterType]
    public class BlanketPurchaseOrderService : BaseCoordinationService, IBlanketPurchaseOrderService
    {
        private IBlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
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
        public BlanketPurchaseOrderService(IBlanketPurchaseOrderRepository blanketPurchaseOrderRepository,
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
            this.blanketPurchaseOrderRepository = blanketPurchaseOrderRepository;
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
        /// Returns the DTO for the specified blanket purchase order
        /// </summary>
        /// <param name="id">ID of the requested blanket purchase order</param>
        /// <returns>Blanket purchase order DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string id)
        {
            // Check the permission code to view a blanket purchase order.
            CheckViewBlanketPurchaseOrderPermission();

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

            // Get the blanket purchase order domain entity from the repository
            var blanketPurchaseOrderDomainEntity = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

            if (blanketPurchaseOrderDomainEntity == null)
            {
                throw new ArgumentNullException("blanketPurchaseOrderDomainEntity", "blanketPurchaseOrderDomainEntity cannot be null.");
            }

            // Convert the blanket purchase order and all its child objects into DTOs
            var blanketPurchaseOrderDtoAdapter = new BlanketPurchaseOrderEntityToDtoAdapter(_adapterRegistry, logger);
            var bpoDto = blanketPurchaseOrderDtoAdapter.MapToType(blanketPurchaseOrderDomainEntity, glConfiguration.MajorComponentStartPositions);

            if (bpoDto.GlDistributions == null || bpoDto.GlDistributions.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access blanket purchase order.");
            }

            return bpoDto;
        }

        /// <summary>
        /// EEDM Get all purchase orders by paging V16.0.0
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.BlanketPurchaseOrders>, int>> GetBlanketPurchaseOrdersAsync(int offset, int limit, Dtos.BlanketPurchaseOrders criteriaObj, bool bypassCache = false)
        {
            CheckViewBlanketPurchaseOrdersPermission();

            int totalRecords = 0;
            var orderNumber = (criteriaObj != null && !string.IsNullOrEmpty(criteriaObj.OrderNumber)) ? criteriaObj.OrderNumber : string.Empty;
            var blanketPurchaseOrdersCollection = new List<Ellucian.Colleague.Dtos.BlanketPurchaseOrders>();

            try
            {
                var blanketPurchaseOrderEntities = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersAsync(offset, limit, orderNumber);

                totalRecords = blanketPurchaseOrderEntities.Item2;
                if (blanketPurchaseOrderEntities.Item1 != null && blanketPurchaseOrderEntities.Item1.Any())
                {
                    // Get the GL Configuration to get the name of the full GL account access role
                    // and also provides the information to format the GL accounts
                    var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
                    if (glConfiguration == null)
                    {
                        throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
                    }

                    var projectIds = blanketPurchaseOrderEntities.Item1
                                 .SelectMany(t => t.GlDistributions)
                                 .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                                 .Select(pj => pj.ProjectId)
                                 .ToList()
                                 .Distinct();
                    if (projectIds != null && projectIds.Any())
                    {
                        _projectReferenceIds = await blanketPurchaseOrderRepository.GetProjectReferenceIds(projectIds.ToArray());
                    }
                    foreach (var blanketPurchaseOrderEntity in blanketPurchaseOrderEntities.Item1)
                    {
                        if (blanketPurchaseOrderEntity.Guid != null)
                        {
                            var blanketPurchaseOrdersDto = await ConvertBlanketPurchaseOrderEntityToDtoAsync(blanketPurchaseOrderEntity, glConfiguration, bypassCache);
                            blanketPurchaseOrdersCollection.Add(blanketPurchaseOrdersDto);
                        }
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
            return new Tuple<IEnumerable<Dtos.BlanketPurchaseOrders>, int>(blanketPurchaseOrdersCollection, totalRecords);
        }

        /// <summary>
        /// EEDM Get Purchase order by GUID V16.0.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> GetBlanketPurchaseOrdersByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Blanket Purchase Order.");
            }
            CheckViewBlanketPurchaseOrdersPermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            var blanketPurchaseOrder = new BlanketPurchaseOrders();
            try
            {
                var blanketPurchaseOrderEntity = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersByGuidAsync(guid);
                if (blanketPurchaseOrderEntity == null)
                {
                    throw new KeyNotFoundException(string.Format("blanket-purchase-orders not found for GUID: {0} ", guid));
                }
                if (blanketPurchaseOrderEntity.GlDistributions != null && blanketPurchaseOrderEntity.GlDistributions.Any())
                {
                    var projectIds = blanketPurchaseOrderEntity.GlDistributions
                       .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                       .Select(pj => pj.ProjectId)
                       .ToList()
                       .Distinct();
                    if (projectIds != null && projectIds.Any())
                    {
                        _projectReferenceIds = await blanketPurchaseOrderRepository.GetProjectReferenceIds(projectIds.ToArray());
                    }
                }
                blanketPurchaseOrder = await ConvertBlanketPurchaseOrderEntityToDtoAsync(blanketPurchaseOrderEntity, glConfiguration);
            }
            catch (KeyNotFoundException ex)
            {
                //IntegrationApiExceptionAddError(string.Format("Blanket Purchase Order was not found for guid '{0}'.", guid), httpStatusCode: System.Net.HttpStatusCode.NotFound);
                throw ex;
            }
            catch (InvalidOperationException)
            {
                IntegrationApiExceptionAddError(string.Format("blanket-purchase-orders not found for GUID '{0}'.", guid), httpStatusCode: System.Net.HttpStatusCode.NotFound);
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
            return blanketPurchaseOrder;
        }

        /// <summary>
        /// EEDM Put blanket purchase orders V16.0.0
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="blanketPurchaseOrders"></param>
        /// <returns></returns>
        public async Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> PutBlanketPurchaseOrdersAsync(string guid, Ellucian.Colleague.Dtos.BlanketPurchaseOrders blanketPurchaseOrder)
        {
            if (blanketPurchaseOrder == null)
                throw new ArgumentNullException("blanketPurchaseOrder", "Must provide a blanketPurchaseOrder for update");
            if (string.IsNullOrEmpty(blanketPurchaseOrder.Id))
                throw new ArgumentNullException("blanketPurchaseOrder", "Must provide a guid for blanketPurchaseOrder update");

            // get the person ID associated with the incoming guid
            var blanketPurchaseOrderId = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersIdFromGuidAsync(blanketPurchaseOrder.Id);
            List<Domain.ColleagueFinance.Entities.FundsAvailable> OverRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(blanketPurchaseOrderId))
            {
                // verify the user has the permission to create a PurchaseOrders
                CheckUpdateBlanketPurchaseOrderPermission();
                var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

                blanketPurchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // Don't check funds availability if we are closing the purchase order
                    if (blanketPurchaseOrder.Status != BlanketPurchaseOrdersStatus.Closed && blanketPurchaseOrder.Status != BlanketPurchaseOrdersStatus.Voided)
                        OverRideGLs = await checkFunds2(blanketPurchaseOrder, blanketPurchaseOrderId);

                    if ((blanketPurchaseOrder.OrderDetails) != null && (blanketPurchaseOrder.OrderDetails.Any()))
                    {
                        var projectRefNos = blanketPurchaseOrder.OrderDetails
                       .Where(i => i.AccountDetails != null)
                       .SelectMany(p => p.AccountDetails)
                       .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                       .Select(pj => pj.AccountingString.Split('*')[1])
                       .ToList()
                       .Distinct();

                        if (projectRefNos != null && projectRefNos.Any())
                        {
                            _projectReferenceIds = await blanketPurchaseOrderRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                        }
                    }
                    // map the DTO to entities
                    var blanketPurchaseOrderEntity
                    = await ConvertBlanketPurchaseOrdersDtoToEntityAsync(blanketPurchaseOrder.Id, blanketPurchaseOrder, glConfiguration.MajorComponents.Count);

                    if (blanketPurchaseOrder.PaymentSource == null)
                    {
                        IntegrationApiExceptionAddError("PaymentSource cannot be unset.", "Validation.Exception", blanketPurchaseOrderEntity.Guid, blanketPurchaseOrderEntity.Id);
                    }
                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }

                    // update the entity in the database
                    var updatedPurchaseOrderEntity =
                        await blanketPurchaseOrderRepository.UpdateBlanketPurchaseOrdersAsync(blanketPurchaseOrderEntity);

                    var dtoPurchaseOrder = await this.ConvertBlanketPurchaseOrderEntityToDtoAsync(updatedPurchaseOrderEntity, glConfiguration, true);

                    if (dtoPurchaseOrder.OrderDetails != null && dtoPurchaseOrder.OrderDetails.Any() && OverRideGLs != null && OverRideGLs.Any())
                    {
                        int lineCount = 0;
                        foreach (var lineItem in dtoPurchaseOrder.OrderDetails)
                        {
                            int detailCount = 0;
                            lineCount++;
                            foreach (var detail in lineItem.AccountDetails)
                            {
                                detailCount++;
                                string PosID = lineCount.ToString() + "." + detailCount.ToString();
                                var findOvr = OverRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.AccountString == detail.AccountingString);
                                if (findOvr != null)
                                {
                                    if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                        detail.BudgetCheck = PurchaseOrdersAccountBudgetCheck.Override;
                                }
                            }
                        }
                    }

                    if (IntegrationApiException != null)
                    {
                        throw IntegrationApiException;
                    }

                    // return the newly updated DTO
                    return dtoPurchaseOrder;

                }
                catch (IntegrationApiException ex)
                {
                    throw ex;
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, ex.Source);
                    throw IntegrationApiException;
                }
            }

            // perform a create instead
            return await PostBlanketPurchaseOrdersAsync(blanketPurchaseOrder);
        }

        /// <summary>
        /// EEDM Post blanket purchase order V16.0.0
        /// </summary>
        /// <param name="blanketPurchaseOrders"></param>
        /// <returns></returns>
        public async Task<Ellucian.Colleague.Dtos.BlanketPurchaseOrders> PostBlanketPurchaseOrdersAsync(Ellucian.Colleague.Dtos.BlanketPurchaseOrders blanketPurchaseOrder)
        {
            if (blanketPurchaseOrder == null)
                throw new ArgumentNullException("blanketPurchaseOrders", "Must provide a blanketPurchaseOrders for update");
            if (string.IsNullOrEmpty(blanketPurchaseOrder.Id))
                throw new ArgumentNullException("blanketPurchaseOrders", "Must provide a guid for blanketPurchaseOrders update");


            Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrder createdBlanketPurchaseOrder = null;

            List<Domain.ColleagueFinance.Entities.FundsAvailable> OverRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();

            // verify the user has the permission to create a PurchaseOrders
            CheckUpdateBlanketPurchaseOrderPermission();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            blanketPurchaseOrderRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                OverRideGLs = await checkFunds2(blanketPurchaseOrder);
                if ((blanketPurchaseOrder.OrderDetails) != null && (blanketPurchaseOrder.OrderDetails.Any()))
                {
                    var projectRefNos = blanketPurchaseOrder.OrderDetails
                   .Where(i => i.AccountDetails != null)
                   .SelectMany(p => p.AccountDetails)
                   .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                   .Select(pj => pj.AccountingString.Split('*')[1])
                   .ToList()
                   .Distinct();

                    if (projectRefNos != null && projectRefNos.Any())
                    {
                        _projectReferenceIds = await blanketPurchaseOrderRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                    }
                }
                var blanketPurchaseOrderEntity
                         = await ConvertBlanketPurchaseOrdersDtoToEntityAsync(blanketPurchaseOrder.Id, blanketPurchaseOrder, glConfiguration.MajorComponents.Count);

                // create a PurchaseOrder entity in the database
                createdBlanketPurchaseOrder =
                    await blanketPurchaseOrderRepository.CreateBlanketPurchaseOrdersAsync(blanketPurchaseOrderEntity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, ex.Source);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            var dtoPurchaseOrder = await this.ConvertBlanketPurchaseOrderEntityToDtoAsync(createdBlanketPurchaseOrder, glConfiguration, true);


            if (dtoPurchaseOrder.OrderDetails != null && dtoPurchaseOrder.OrderDetails.Any() && OverRideGLs != null && OverRideGLs.Any())
            {
                int lineCount = 0;
                foreach (var lineItem in dtoPurchaseOrder.OrderDetails)
                {
                    int detailCount = 0;
                    lineCount++;
                    foreach (var detail in lineItem.AccountDetails)
                    {
                        detailCount++;
                        string PosID = lineCount.ToString() + "." + detailCount.ToString();
                        var findOvr = OverRideGLs.FirstOrDefault(a => a.Sequence == PosID || a.AccountString == detail.AccountingString);
                        if (findOvr != null)
                        {
                            if (findOvr.AvailableStatus == FundsAvailableStatus.Override)
                                detail.BudgetCheck = PurchaseOrdersAccountBudgetCheck.Override;
                        }
                    }
                }
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            // return the newly created blanketPurchaseOrder
            return dtoPurchaseOrder;
        }

        /// <summary>
        /// Check if Funds are avialable for this PUT/POST event.
        /// </summary>
        /// <param name="blanketPurchaseOrder"></param>
        /// <param name="PoId"></param>
        /// <returns></returns>
        private async Task<List<Domain.ColleagueFinance.Entities.FundsAvailable>> checkFunds2(BlanketPurchaseOrders blanketPurchaseOrder, string PoId = "")
        {
            var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            //check if Accounting string has funds
            int lineCount = 0;
            var docSubmittedById = string.Empty;
            if (blanketPurchaseOrder.SubmittedBy != null && !string.IsNullOrEmpty(blanketPurchaseOrder.SubmittedBy.Id))
                docSubmittedById = await personRepository.GetPersonIdFromGuidAsync(blanketPurchaseOrder.SubmittedBy.Id);
            else
                docSubmittedById = CurrentUser.PersonId;
            var budgetOvrCheckTuple = new List<Tuple<string, bool>>();

            foreach (var detail in blanketPurchaseOrder.OrderDetails)
            {
                int detailCount = 0;
                lineCount++;
                List<string> accountingStringList = new List<string>();
                if (detail.AccountDetails != null && detail.AccountDetails.Any())
                {
                    foreach (var details in detail.AccountDetails)
                    {
                        detailCount++;
                        var submittedBy = (blanketPurchaseOrder.SubmittedBy != null) ? blanketPurchaseOrder.SubmittedBy.Id : "";
                        var submittedById = (!string.IsNullOrEmpty(submittedBy)) ? await personRepository.GetPersonIdFromGuidAsync(submittedBy) : "";

                        if (details.Allocation != null && details.Allocation.Allocated != null &&
                                details.Allocation.Allocated.Amount != null && details.Allocation.Allocated.Amount.Value != null
                                && details.Allocation.Allocated.Amount.Value.HasValue)
                        {
                            string PosID = lineCount.ToString() + "." + detailCount.ToString();
                            var budgetCheckOverrideFlag = (details.BudgetCheck == PurchaseOrdersAccountBudgetCheck.Override) ? true : false;
                            budgetOvrCheckTuple.Add(new Tuple<string, bool>(details.AccountingString, budgetCheckOverrideFlag));
                            fundsAvailable.Add(new Domain.ColleagueFinance.Entities.FundsAvailable(details.AccountingString)
                            {
                                Sequence = PosID,
                                SubmittedBy = submittedById,
                                Amount = details.Allocation.Allocated.Amount.Value.Value,
                                TransactionDate = blanketPurchaseOrder.OrderedOn
                            });
                        }

                        var accountingString = accountingStringList.Find(x => x.Equals(details.AccountingString));
                        if (string.IsNullOrWhiteSpace(accountingString))
                        {
                            accountingStringList.Add(details.AccountingString);
                        }
                        else
                        {
                            throw new Exception("A order details has two account details with the same GL number '" + accountingString + "' this is not allowed");
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
                            throw new ArgumentException("The accounting string '" + availableFund.AccountString + "' does not have funds available");
                        }
                        //if we get a override and if the budgetcheck flag is not set to override then thrown an exception
                        if (availableFund.AvailableStatus == FundsAvailableStatus.Override)
                        {
                            var budOverCheck = budgetOvrCheckTuple.FirstOrDefault(acct => acct.Item1 == availableFund.AccountString);
                            if (budOverCheck != null && budOverCheck.Item2 == false)
                            {
                                throw new ArgumentException("The accounting string '" + availableFund.AccountString + "' does not have funds available. BudgetCheck flag not set to override.");
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
        /// <param name="blanketPurchaseOrderGuid"></param>
        /// <param name="blanketPurchaseOrder"></param>
        /// <returns></returns>
        private async Task<Domain.ColleagueFinance.Entities.BlanketPurchaseOrder> ConvertBlanketPurchaseOrdersDtoToEntityAsync(string blanketPurchaseOrderGuid, Dtos.BlanketPurchaseOrders blanketPurchaseOrder, int GLCompCount)
        {
            if (blanketPurchaseOrder == null || string.IsNullOrEmpty(blanketPurchaseOrder.Id))
                throw new ArgumentNullException("blanketPurchaseOrder", "Must provide guid for blanketPurchaseOrder");

            if ((blanketPurchaseOrder.Vendor != null) && (blanketPurchaseOrder.Vendor.ExistingVendor != null) &&
                 (blanketPurchaseOrder.Vendor.ExistingVendor.Vendor != null) && (string.IsNullOrEmpty(blanketPurchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                throw new ArgumentNullException("blanketPurchaseOrder", "Must provide vendor.id for blanketPurchaseOrder");

            var poStatus = BlanketPurchaseOrderStatus.NotApproved;
            if (blanketPurchaseOrder.Status != BlanketPurchaseOrdersStatus.NotSet && blanketPurchaseOrder.Status != null)
            {
                switch (blanketPurchaseOrder.Status)
                {
                    case BlanketPurchaseOrdersStatus.Notapproved:
                        poStatus = BlanketPurchaseOrderStatus.NotApproved; //"N";
                        break;
                    case BlanketPurchaseOrdersStatus.Outstanding:
                        poStatus = BlanketPurchaseOrderStatus.Outstanding; //"O";
                        break;

                    case BlanketPurchaseOrdersStatus.Voided:
                        poStatus = BlanketPurchaseOrderStatus.Voided; //"V";
                        break;
                    case BlanketPurchaseOrdersStatus.Closed:
                        poStatus = BlanketPurchaseOrderStatus.Closed; //"C";
                        break;

                    default:
                        // if we get here, we have an invalid status.
                        IntegrationApiExceptionAddError(string.Format("The blanket purchase order status of '{0}' is not supported", blanketPurchaseOrder.Status.ToString()));
                        break;
                }
            }

            var guid = blanketPurchaseOrderGuid;
            var blanketPurchaseOrderId = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersIdFromGuidAsync(guid);
            var date = DateTime.Now.Date;
            string currency = null;

            if (string.IsNullOrEmpty(blanketPurchaseOrderId))
            {
                blanketPurchaseOrderId = "NEW";
            }

            if (string.IsNullOrEmpty(blanketPurchaseOrder.OrderNumber)) { blanketPurchaseOrder.OrderNumber = "new"; }

            var blanketPurchaseOrderEntity = new Domain.ColleagueFinance.Entities.BlanketPurchaseOrder(
                   blanketPurchaseOrderId, guid ?? new Guid().ToString(), blanketPurchaseOrder.OrderNumber, "test name", poStatus, date, blanketPurchaseOrder.OrderedOn);
            
            if (blanketPurchaseOrder.SubmittedBy != null)
            {
                var submittedById = await personRepository.GetPersonIdFromGuidAsync(blanketPurchaseOrder.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    IntegrationApiExceptionAddError(string.Format("SubmittedBy information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                        "blanketPurchaseOrder.submittedBy.id", guid, blanketPurchaseOrderEntity.Id);
                }
                else
                {
                    blanketPurchaseOrderEntity.SubmittedBy = submittedById;
                }
            }
            // New flag to send data to the CTX to bypass approvals
            blanketPurchaseOrderEntity.bypassApprovals = CheckBypassApprovalsPermission();

            if (blanketPurchaseOrder.TransactionDate != default(DateTime))
                blanketPurchaseOrderEntity.MaintenanceDate = blanketPurchaseOrder.TransactionDate;
            
            blanketPurchaseOrderEntity.ExpirationDate = blanketPurchaseOrder.ExpireDate;

            if (blanketPurchaseOrder.Buyer != null)
            {
                var buyerId = await buyerRepository.GetBuyerIdFromGuidAsync(blanketPurchaseOrder.Buyer.Id);
                if (string.IsNullOrEmpty(buyerId))
                {
                    IntegrationApiExceptionAddError(string.Format("Buyer information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                        "blanketPurchaseOrder.buyer.id", guid, blanketPurchaseOrderEntity.Id);
                }
                else
                {
                    blanketPurchaseOrderEntity.Buyer = buyerId;
                }
            }

            if (blanketPurchaseOrder.Initiator != null && blanketPurchaseOrder.Initiator.Detail != null)
            {
                var initiatorId = await personRepository.GetPersonIdFromGuidAsync(blanketPurchaseOrder.Initiator.Detail.Id);
                if (string.IsNullOrEmpty(initiatorId))
                {
                    IntegrationApiExceptionAddError(string.Format("Initiator information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                        "blanketPurchaseOrder.initiator.id", guid, blanketPurchaseOrderEntity.Id);
                }
                else
                {
                    blanketPurchaseOrderEntity.DefaultInitiator = initiatorId;
                }
            }

            if (blanketPurchaseOrder.Shipping != null)
            {
                if (blanketPurchaseOrder.Shipping.ShipTo != null)
                {
                    var shipToDestinations = await GetShipToDestinationsAsync(true);
                    if (shipToDestinations == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Ship to destinations reference data is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.shipping.shipTo.id", guid, blanketPurchaseOrderEntity.Id);
                    }
                    else
                    {
                        var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Guid == blanketPurchaseOrder.Shipping.ShipTo.Id);
                        if (shipToDestination == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Ship to destination information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                "blanketPurchaseOrder.shipping.ShipTo.id", guid, blanketPurchaseOrderEntity.Id);
                        }
                        else
                        {
                            blanketPurchaseOrderEntity.ShipToCode = shipToDestination.Code;
                        }
                    }
                }

                if (blanketPurchaseOrder.Shipping.FreeOnBoard != null)
                {
                    var freeOnBoardTypes = await GetFreeOnBoardTypesAsync(true);
                    if (freeOnBoardTypes == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Free on board types reference data is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.shipping.freeOnBoard.id", guid, blanketPurchaseOrderEntity.Id);
                    }
                    else
                    {
                        var freeOnBoardType = freeOnBoardTypes.FirstOrDefault(fob => fob.Guid == blanketPurchaseOrder.Shipping.FreeOnBoard.Id);
                        if (freeOnBoardType == null)
                        {
                            IntegrationApiExceptionAddError(string.Format("Free on board information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                "blanketPurchaseOrder.shipping.freeOnBoard.id", guid, blanketPurchaseOrderEntity.Id);
                        }
                        else
                        {
                            blanketPurchaseOrderEntity.Fob = freeOnBoardType.Code;
                        }
                    }
                }
            }
            if (blanketPurchaseOrder.OverrideShippingDestination != null)
            {
                var overrideShippingDestinationDto = blanketPurchaseOrder.OverrideShippingDestination;
                if (!string.IsNullOrEmpty(blanketPurchaseOrder.OverrideShippingDestination.Description))
                {
                    blanketPurchaseOrderEntity.AltShippingName = blanketPurchaseOrder.OverrideShippingDestination.Description;
                }

                if (overrideShippingDestinationDto.AddressLines != null && overrideShippingDestinationDto.AddressLines.Any())
                {
                    blanketPurchaseOrderEntity.AltShippingAddress = overrideShippingDestinationDto.AddressLines;
                }

                var place = overrideShippingDestinationDto.Place;
                if (place != null && place.Country != null)
                {
                    blanketPurchaseOrderEntity.AltShippingCountry = place.Country.Code.ToString();

                    if (!string.IsNullOrEmpty(place.Country.Locality))
                    {
                        blanketPurchaseOrderEntity.AltShippingCity = place.Country.Locality;
                    }
                    if (place.Country.Region != null && !string.IsNullOrEmpty(place.Country.Region.Code))
                    {
                        blanketPurchaseOrderEntity.AltShippingState = place.Country.Region.Code.Split('-')[1];
                    }
                    blanketPurchaseOrderEntity.AltShippingZip = place.Country.PostalCode;
                }
                if (blanketPurchaseOrder.OverrideShippingDestination.Contact != null)
                {
                    if (!string.IsNullOrEmpty(blanketPurchaseOrder.OverrideShippingDestination.Contact.Number))
                    {
                        blanketPurchaseOrderEntity.AltShippingPhone = blanketPurchaseOrder.OverrideShippingDestination.Contact.Number;
                        blanketPurchaseOrderEntity.AltShippingPhoneExt = blanketPurchaseOrder.OverrideShippingDestination.Contact.Extension;
                    }
                }
            }

            if (blanketPurchaseOrder.Vendor != null)
            {
                if (blanketPurchaseOrder.Vendor.ExistingVendor != null &&
                    blanketPurchaseOrder.Vendor.ExistingVendor.Vendor != null && (!string.IsNullOrEmpty(blanketPurchaseOrder.Vendor.ExistingVendor.Vendor.Id)))
                {
                    string vendorId = string.Empty;
                    try
                    {
                        vendorId = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersIdFromGuidAsync(blanketPurchaseOrder.Vendor.ExistingVendor.Vendor.Id);
                    }
                    catch (ArgumentException)
                    {
                        IntegrationApiExceptionAddError(string.Format("Vendor information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.vendor.existingVendor.id", guid, blanketPurchaseOrderEntity.Id);
                    }
                    if (!string.IsNullOrEmpty(vendorId))
                        blanketPurchaseOrderEntity.VendorId = vendorId;
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Vendor information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.vendor.existingVendor.id", guid, blanketPurchaseOrderEntity.Id);
                    }

                    if (blanketPurchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress != null &&
                        !string.IsNullOrWhiteSpace(blanketPurchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id))
                    {
                        string vendorAddressId = string.Empty;
                        try
                        {
                            vendorAddressId = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersIdFromGuidAsync(blanketPurchaseOrder.Vendor.ExistingVendor.AlternativeVendorAddress.Id);
                        }
                        catch (ArgumentException)
                        {
                            IntegrationApiExceptionAddError(string.Format("Vendor alternative address information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                "blanketPurchaseOrder.vendor.existingVendor.alternativeVendorAddress.id", guid, blanketPurchaseOrderEntity.Id);
                        }
                        if (!string.IsNullOrEmpty(vendorAddressId))
                            blanketPurchaseOrderEntity.VendorAddressId = vendorAddressId;
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Vendor alternative address information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                "blanketPurchaseOrder.vendor.existingVendor.alternativeVendorAddress.id", guid, blanketPurchaseOrderEntity.Id);
                        }
                    }
                }

                if (blanketPurchaseOrder.Vendor.ManualVendorDetails != null)
                {
                    var manualVendorDetailsDto = blanketPurchaseOrder.Vendor.ManualVendorDetails;

                    if (manualVendorDetailsDto.Type == ManualVendorType.Organization)
                    { blanketPurchaseOrderEntity.MiscIntgCorpPersonFlag = "Organization"; }
                    else if (manualVendorDetailsDto.Type == ManualVendorType.Person)
                    {
                        blanketPurchaseOrderEntity.MiscIntgCorpPersonFlag = "Person";
                    }

                    if ((blanketPurchaseOrder.Vendor.ExistingVendor == null ||
                        blanketPurchaseOrder.Vendor.ExistingVendor.Vendor == null) ||
                        string.IsNullOrEmpty(blanketPurchaseOrder.Vendor.ExistingVendor.Vendor.Id))
                    {
                        IntegrationApiExceptionAddError(string.Format("An existing vendor is required for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.vendor.existingVendor.vendor.id", guid, blanketPurchaseOrderEntity.Id);
                    }

                    if (manualVendorDetailsDto.Name != null && manualVendorDetailsDto.Name.Any())
                    {
                        blanketPurchaseOrderEntity.MiscName = new List<string>();
                        blanketPurchaseOrderEntity.MiscName.Add(manualVendorDetailsDto.Name);
                    }

                    if (manualVendorDetailsDto.AddressLines != null && manualVendorDetailsDto.AddressLines.Any())
                    {
                        blanketPurchaseOrderEntity.MiscAddress = manualVendorDetailsDto.AddressLines;
                    }

                    var place = manualVendorDetailsDto.Place;
                    if (place != null && place.Country != null)
                    {
                        blanketPurchaseOrderEntity.MiscCountry = place.Country.Code.ToString();

                        if (!string.IsNullOrEmpty(place.Country.Locality))
                        {
                            blanketPurchaseOrderEntity.MiscCity = place.Country.Locality;
                        }
                        if (place.Country.Region != null && !string.IsNullOrEmpty(place.Country.Region.Code))
                        {
                            blanketPurchaseOrderEntity.MiscState = place.Country.Region.Code.Split('-')[1];
                        }
                        blanketPurchaseOrderEntity.MiscZip = place.Country.PostalCode;

                    }
                }
            }

            if (blanketPurchaseOrder.ReferenceNumbers != null && blanketPurchaseOrder.ReferenceNumbers.Count() > 0)
            {
                blanketPurchaseOrderEntity.ReferenceNo = blanketPurchaseOrder.ReferenceNumbers;
            }

            if (blanketPurchaseOrder.PaymentSource != null)
            {
                var payment = blanketPurchaseOrder.PaymentSource;
                if (string.IsNullOrEmpty(payment.Id))
                {
                    IntegrationApiExceptionAddError("PaymentSource id is required when submitting a PaymentSource.", "Validation.Exception", guid, blanketPurchaseOrderEntity.Id);
                }
                if ((payment.Id != null) && (!string.IsNullOrEmpty(payment.Id)))
                {
                    var blanketPurchaseOrderSource = (await this.GetAllAccountsPayableSourcesAsync(true)).FirstOrDefault(ap => ap.Guid == payment.Id);
                    if (blanketPurchaseOrderSource == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Payment source information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                            "blanketPurchaseOrder.paymentSource.id", guid, blanketPurchaseOrderEntity.Id);
                    }
                    else
                    {
                        blanketPurchaseOrderEntity.ApType = blanketPurchaseOrderSource.Code;
                    }
                }
            }

            if ((blanketPurchaseOrder.PaymentTerms != null) && (!string.IsNullOrEmpty(blanketPurchaseOrder.PaymentTerms.Id)))
            {
                var vendorTerms = (await this.GetVendorTermsAsync(true)).FirstOrDefault(ap => ap.Guid == blanketPurchaseOrder.PaymentTerms.Id);
                if (vendorTerms == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Payment terms information is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                        "blanketPurchaseOrder.paymentTerms.id", guid, blanketPurchaseOrderEntity.Id);
                }
                else
                {
                    blanketPurchaseOrderEntity.VendorTerms = vendorTerms.Code;
                }
            }

            if (blanketPurchaseOrder.Comments != null && blanketPurchaseOrder.Comments.Any())
            {
                blanketPurchaseOrderEntity.Comments = string.Empty;
                blanketPurchaseOrderEntity.InternalComments = string.Empty;
                foreach (var comment in blanketPurchaseOrder.Comments)
                {
                    switch (comment.Type)
                    {
                        case CommentTypes.NotPrinted:
                            blanketPurchaseOrderEntity.InternalComments = !string.IsNullOrEmpty(blanketPurchaseOrderEntity.InternalComments) ?
                                string.Concat(blanketPurchaseOrderEntity.InternalComments, " ", comment.Comment) : comment.Comment;
                            break;
                        case CommentTypes.Printed:
                            blanketPurchaseOrderEntity.Comments = !string.IsNullOrEmpty(blanketPurchaseOrderEntity.Comments) ?
                                string.Concat(blanketPurchaseOrderEntity.Comments, " ", comment.Comment) : comment.Comment;
                            break;
                    }
                }
            }

            if ((blanketPurchaseOrder.OrderDetails != null) && (blanketPurchaseOrder.OrderDetails.Any()))
            {
                decimal totalAmount = 0;
                foreach (var orderDetail in blanketPurchaseOrder.OrderDetails)
                {
                    blanketPurchaseOrderEntity.Description = orderDetail.Description;
                    if ((orderDetail.Amount != null) && (orderDetail.Amount.Value != null && orderDetail.Amount.Value.HasValue))
                    {
                        totalAmount += Convert.ToDecimal(orderDetail.Amount.Value);
                        currency = orderDetail.Amount.Currency.ToString();
                    }
                    if (orderDetail.AdditionalAmount != null && orderDetail.AdditionalAmount.Value != null && orderDetail.AdditionalAmount.Value.HasValue)
                    {
                        totalAmount += Convert.ToDecimal(orderDetail.AdditionalAmount.Value);
                    }
                    if (orderDetail.DiscountAmount != null && orderDetail.DiscountAmount.Value != null && orderDetail.DiscountAmount.Value.HasValue)
                    {
                        totalAmount -= Convert.ToDecimal(orderDetail.DiscountAmount.Value);
                    }

                    if ((orderDetail.CommodityCode != null) && (!string.IsNullOrEmpty(orderDetail.CommodityCode.Id)))
                    {
                        var allCommodityCodes = (await GetCommodityCodesAsync(true));
                        if ((allCommodityCodes == null) || (!allCommodityCodes.Any()))
                        {
                            IntegrationApiExceptionAddError(string.Format("An error occurred extracting all commodity codes for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                "blanketPurchaseOrder.orderDetails.commodityCode.id", guid, blanketPurchaseOrderEntity.Id);
                        }
                        else
                        {
                            var commodityCode = allCommodityCodes.FirstOrDefault(c => c.Guid == orderDetail.CommodityCode.Id);
                            if (commodityCode == null)
                            {
                                IntegrationApiExceptionAddError(string.Format("Commodity code is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                    "blanketPurchaseOrder.orderDetails.commodityCode.id", guid, blanketPurchaseOrderEntity.Id);
                            }
                            else
                            {
                                blanketPurchaseOrderEntity.CommodityCode = commodityCode.Code;
                            }
                        }
                    }
                    
                    if (orderDetail.Comments != null && orderDetail.Comments.Any())
                    {
                        foreach (var comment in orderDetail.Comments)
                        {
                            switch (comment.Type)
                            {
                                case CommentTypes.NotPrinted:
                                    blanketPurchaseOrderEntity.InternalComments = !string.IsNullOrEmpty(blanketPurchaseOrderEntity.InternalComments) ?
                                        string.Concat(blanketPurchaseOrderEntity.InternalComments, " ", comment.Comment) : comment.Comment;
                                    break;
                                case CommentTypes.Printed:
                                    blanketPurchaseOrderEntity.Comments = !string.IsNullOrEmpty(blanketPurchaseOrderEntity.Comments) ?
                                        string.Concat(blanketPurchaseOrderEntity.Comments, " ", comment.Comment) : comment.Comment;
                                    break;
                            }
                        }
                    }

                    if (orderDetail.ReferenceRequisitions != null)
                    {
                        foreach (var referenceRequisition in orderDetail.ReferenceRequisitions)
                        {
                            string reqId = string.Empty;
                            try
                            {
                                reqId = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrdersIdFromGuidAsync(referenceRequisition.Requisition.Id);
                            }
                            catch (ArgumentException)
                            {
                                IntegrationApiExceptionAddError(string.Format("Reference requisition is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                    "blanketPurchaseOrder.orderDetails.referenceRequisitions.requisition.id", guid, blanketPurchaseOrderEntity.Id);
                            }
                            if (!string.IsNullOrEmpty(reqId))
                                blanketPurchaseOrderEntity.AddRequisition(reqId);
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("Reference requisition is invalid for blanket purchase order '{0}'", blanketPurchaseOrder.OrderNumber),
                                    "blanketPurchaseOrder.orderDetails.referenceRequisitions.requisition.id", guid, blanketPurchaseOrderEntity.Id);
                            }
                        }
                    }

                    if (orderDetail.AccountDetails != null && orderDetail.AccountDetails.Any())
                    {
                        foreach (var accountDetails in orderDetail.AccountDetails)
                        {
                            decimal distributionAmount = 0;
                            decimal distributionPercent = 0;
                            var allocated = accountDetails.Allocation != null ? accountDetails.Allocation.Allocated : null;
                            if (allocated != null)
                            {
                                if (allocated.Amount != null && allocated.Amount.Value != null && allocated.Amount.Value.HasValue)
                                {
                                    distributionAmount = Convert.ToDecimal(allocated.Amount.Value);
                                    currency = allocated.Amount.Currency.ToString();
                                }
                                if (allocated.Percentage.HasValue)
                                {
                                    distributionPercent = Convert.ToDecimal(allocated.Percentage);
                                }
                            }
                            var additionalAmount = accountDetails.Allocation != null ? accountDetails.Allocation.AdditionalAmount : null;
                            if (additionalAmount != null && additionalAmount.Value != null && additionalAmount.Value.HasValue)
                            {
                                distributionAmount += Convert.ToDecimal(additionalAmount.Value);
                                currency = additionalAmount.Currency.ToString();
                            }
                            var discountAmount = accountDetails.Allocation != null ? accountDetails.Allocation.DiscountAmount : null;
                            if (discountAmount != null && discountAmount.Value != null && discountAmount.Value.HasValue)
                            {
                                distributionAmount -= Convert.ToDecimal(discountAmount.Value);
                                currency = discountAmount.Currency.ToString();
                            }

                            string accountingString = ConvertAccountingString(GLCompCount, accountDetails.AccountingString);
                            var glDistribution = new BlanketPurchaseOrderGlDistribution(accountingString, distributionAmount);
                            glDistribution.Percentage = distributionPercent;

                            blanketPurchaseOrderEntity.AddGlDistribution(glDistribution);
                        }
                    }
                }
                blanketPurchaseOrderEntity.Amount = totalAmount;
                if (!string.IsNullOrEmpty(currency))
                    blanketPurchaseOrderEntity.CurrencyCode = currency;
            }
            return blanketPurchaseOrderEntity;
        }

        /// <summary>
        /// Converts an PurchaseOrder domain entity to its corresponding PurchaseOrder DTO
        /// </summary>
        /// <param name="source">PurchaseOrder domain entity</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.BlanketPurchaseOrders">BlanketPurchaseOrders</see></returns>
        private async Task<Dtos.BlanketPurchaseOrders> ConvertBlanketPurchaseOrderEntityToDtoAsync(Domain.ColleagueFinance.Entities.BlanketPurchaseOrder source,
            GeneralLedgerAccountStructure glConfiguration, bool bypassCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("blanketPurchaseOrder", "A blanket purchase order entity is required to obtain a Blanket Purchase Order.");
            }
            var blanketPurchaseOrder = new Ellucian.Colleague.Dtos.BlanketPurchaseOrders();

            var currency = GetCurrencyIsoCode(source.CurrencyCode, source.HostCountry);

            blanketPurchaseOrder.Id = source.Guid;
            blanketPurchaseOrder.OrderNumber = source.Number;

            if (!string.IsNullOrEmpty(source.SubmittedBy))
            {
                try
                {
                    var submittedByGuid = await personRepository.GetPersonGuidFromIdAsync(source.SubmittedBy);
                    if (string.IsNullOrEmpty(submittedByGuid))
                    {
                        IntegrationApiExceptionAddError(
                            string.Concat("Missing Submitted By information for blanket purchase order: '", source.Number, "'"),
                            "blanketPurchaseOrder.submittedBy", source.Guid, source.Id);
                    }
                    else
                    {
                        blanketPurchaseOrder.SubmittedBy = new GuidObject2(submittedByGuid);
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Missing Submitted By information for blanket purchase order: '",source.Number, "'"),
                        "blanketPurchaseOrder.submittedBy", source.Guid, source.Id);
                }
            }

            if ((source.ReferenceNo != null) && (source.ReferenceNo.Any()))
            {
                blanketPurchaseOrder.ReferenceNumbers = source.ReferenceNo;
            }
            blanketPurchaseOrder.OrderedOn = source.Date;
            blanketPurchaseOrder.TransactionDate = (source.MaintenanceDate != null && source.MaintenanceDate.HasValue)
                ? Convert.ToDateTime(source.MaintenanceDate) : source.Date;

            blanketPurchaseOrder.ExpireDate = source.ExpirationDate;

            switch (source.Status)
            {
                case (Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Closed):
                    blanketPurchaseOrder.Status = BlanketPurchaseOrdersStatus.Closed; break;
                case (Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.InProgress):
                    blanketPurchaseOrder.Status = BlanketPurchaseOrdersStatus.Notapproved; break;
                case (Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.NotApproved):
                    blanketPurchaseOrder.Status = BlanketPurchaseOrdersStatus.Notapproved; break;
                case (Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Outstanding):
                    blanketPurchaseOrder.Status = BlanketPurchaseOrdersStatus.Outstanding; break;
                case (Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Voided):
                    blanketPurchaseOrder.Status = BlanketPurchaseOrdersStatus.Voided; break;
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
                        throw new Exception(string.Concat("Missing buyer information for blanket purchase order: '", source.Number, "'"));
                    }
                    blanketPurchaseOrder.Buyer = new GuidObject2(buyerGuid);
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Missing buyer information for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.buyer", source.Guid, source.Id);
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
                        throw new Exception(string.Concat("Missing initiator information for blanket purchase order: '", source.Number, "'"));
                    }
                    initiator.Detail = new GuidObject2(initiatorGuid);
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Missing initiator information for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.initiator", source.Guid, source.Id);
                }
            }
            if ((!string.IsNullOrEmpty(initiator.Name)) || (initiator.Detail != null))
            {
                blanketPurchaseOrder.Initiator = initiator;
            }

            var purchaseOrdersShipping = new Dtos.DtoProperties.PurchaseOrdersShippingDtoProperty();
            if (!string.IsNullOrEmpty(source.ShipToCode))
            {
                try
                {
                    var shipToDestinations = await GetShipToDestinationsAsync(bypassCache);
                    if (shipToDestinations == null)
                    {
                        throw new Exception("Unable to retrieve ShipToDestinations");
                    }
                    var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Code == source.ShipToCode);
                    if (shipToDestination == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to retrieve ShipToDestination for: ", source.ShipToCode));
                    }
                    purchaseOrdersShipping.ShipTo = new GuidObject2(shipToDestination.Guid);
                    blanketPurchaseOrder.Shipping = purchaseOrdersShipping;
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Unable to retrieve ShipToDestination for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.shipping.shipTo", source.Guid, source.Id);
                }
            }
            if (!string.IsNullOrEmpty(source.Fob))
            {
                try
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
                    blanketPurchaseOrder.Shipping = purchaseOrdersShipping;
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Unable to retrieve FreeOnBoardTypes for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.shipping.freeOnBoard", source.Guid, source.Id);
                }
            }


            blanketPurchaseOrder.OverrideShippingDestination = await BuildOverrideShippingDestinationDtoAsync(source, bypassCache);

            var blanketPurchaseOrderVendor = new BlanketPurchaseOrdersVendor();


            if (!string.IsNullOrEmpty(source.VendorId))
            {
                blanketPurchaseOrderVendor.ExistingVendor = new PurchaseOrdersExistingVendorDtoProperty();
                try
                {
                    var vendorGuid = await vendorsRepository.GetVendorGuidFromIdAsync(source.VendorId);
                    if (string.IsNullOrEmpty(vendorGuid))
                    {
                        IntegrationApiExceptionAddError(
                            string.Concat("Missing Missing vendor GUID for blanket purchase order: '", source.Number, "'"),
                            "blanketPurchaseOrder.vendor.existingVendor.vendor.id", source.Guid, source.Id);
                    }

                    blanketPurchaseOrderVendor.ExistingVendor.Vendor = new GuidObject2(vendorGuid);

                    if (!string.IsNullOrEmpty(source.VendorAddressId) && (source.AltAddressFlag == true))
                    {
                        var vendorAddressGuid = await blanketPurchaseOrderRepository.GetGuidFromIdAsync(source.VendorAddressId, "ADDRESS");
                        if (string.IsNullOrEmpty(vendorAddressGuid))
                        {
                            IntegrationApiExceptionAddError(
                                string.Concat("Missing GUID for existing vendor address for blanket purchase order: '", source.Number, "'"),
                                "blanketPurchaseOrder.vendor.existingVendor.alternativeVendorAddress.id", source.Guid, source.Id);
                        }
                        blanketPurchaseOrderVendor.ExistingVendor.AlternativeVendorAddress = new GuidObject2(vendorAddressGuid);
                    }
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Missing existing vendor data for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.vendor.existingVendor", source.Guid, source.Id);
                }
            }

            if ((blanketPurchaseOrderVendor.ExistingVendor == null) || (blanketPurchaseOrderVendor.ExistingVendor.AlternativeVendorAddress == null && source.AltAddressFlag == true))
                blanketPurchaseOrderVendor.ManualVendorDetails = await BuildManualVendorDetailsDtoAsync(source, bypassCache);

            if (blanketPurchaseOrderVendor.ExistingVendor != null || blanketPurchaseOrderVendor.ManualVendorDetails != null)
            {
                blanketPurchaseOrder.Vendor = blanketPurchaseOrderVendor;
            }
            else
            {
                IntegrationApiExceptionAddError(
                        string.Concat("Missing vendor data.  existingVendor or manualVendorDetails is required for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.vendor", source.Guid, source.Id);
            }

            if (!string.IsNullOrEmpty(source.VendorTerms))
            {
                try
                {
                    var vendorTerms = await GetVendorTermsAsync(bypassCache);
                    if (vendorTerms == null)
                        throw new Exception("Unable to retrieve vendor terms");
                    var vendorTerm = vendorTerms.FirstOrDefault(vt => vt.Code == source.VendorTerms);
                    if (vendorTerm == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Missing vendor term information for blanket purchase order: ", source.Id, " Guid: ", source.Guid, " Vendor Term: ", source.VendorTerms));
                    }
                    blanketPurchaseOrder.PaymentTerms = new GuidObject2(vendorTerm.Guid);
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Unable to retrieve Vendor PaymentTerms for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.paymentTerms", source.Guid, source.Id);
                }
            }


            if (!string.IsNullOrEmpty(source.ApType))
            {
                try
                {
                    var accountsPayableSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(false);
                    if (accountsPayableSources == null)
                        throw new Exception("Unable to retrieve accounts payable sources");
                    var accountsPayableSource = accountsPayableSources.FirstOrDefault(aps => aps.Code == source.ApType);
                    if (accountsPayableSource == null)
                    {
                        IntegrationApiExceptionAddError(
                        string.Concat("Unable to retrieve accounts payable source information for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.paymentSource.id", source.Guid, source.Id);
                    }
                    blanketPurchaseOrder.PaymentSource = new GuidObject2(accountsPayableSource.Guid);
                }
                catch
                {
                    IntegrationApiExceptionAddError(
                        string.Concat("Unable to retrieve accounts payable source information for blanket purchase order: '", source.Number, "'"),
                        "blanketPurchaseOrder.paymentSource", source.Guid, source.Id);
                }
            }


            var purchaseOrdersComments = new List<BlanketPurchaseOrdersComments>();
            if (!string.IsNullOrEmpty(source.Comments))
            {
                var purchaseOrdersComment = new BlanketPurchaseOrdersComments();
                purchaseOrdersComment.Comment = source.Comments;  //PoPrintedComments
                purchaseOrdersComment.Type = CommentTypes.Printed;
                purchaseOrdersComments.Add(purchaseOrdersComment);
            }
            if (!string.IsNullOrEmpty(source.InternalComments))
            {
                var purchaseOrdersComment = new BlanketPurchaseOrdersComments();
                purchaseOrdersComment.Comment = source.InternalComments; //PoComments
                purchaseOrdersComment.Type = CommentTypes.NotPrinted;
                purchaseOrdersComments.Add(purchaseOrdersComment);
            }
            if (purchaseOrdersComments != null && purchaseOrdersComments.Any())
            {
                blanketPurchaseOrder.Comments = purchaseOrdersComments;
            }

            var orderDetails = new List<BlanketPurchaseOrdersOrderdetails>();
            var orderDetailObject = await BuildBlanketPurchaseOrderDetails(source, source.Id, source.Guid, glConfiguration, currency, bypassCache);
            if (orderDetailObject != null)
            {
                orderDetails.Add(orderDetailObject);
            }

            if (orderDetails != null && orderDetails.Any())
            {
                blanketPurchaseOrder.OrderDetails = orderDetails;
            }
            return blanketPurchaseOrder;
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
        private async Task<BlanketPurchaseOrdersOrderdetails> BuildBlanketPurchaseOrderDetails(BlanketPurchaseOrder blanketPurchaseOrder, string sourceId, string sourceGuid,
             GeneralLedgerAccountStructure GlConfig, CurrencyIsoCode currency = CurrencyIsoCode.USD, bool bypassCache = false)
        {
            var detail = new BlanketPurchaseOrdersOrderdetails();
            if (blanketPurchaseOrder != null)
            {
                detail.OrderDetailNumber = blanketPurchaseOrder.Id;
                detail.Description = blanketPurchaseOrder.Description;

                if (!string.IsNullOrEmpty(blanketPurchaseOrder.CommodityCode))
                {
                    try
                    {
                        var commodityCodes = await GetCommodityCodesAsync(bypassCache);
                        if (commodityCodes == null)
                        {
                            throw new Exception("Unable to retrieve commodity codes");
                        }
                        var commodityCode = commodityCodes.FirstOrDefault(cc => cc.Code == blanketPurchaseOrder.CommodityCode);
                        if (commodityCode == null)
                        {
                            IntegrationApiExceptionAddError(
                                string.Concat("Unable to retrieve commodity code information for blanket purchase order: '", blanketPurchaseOrder.Number, "'"),
                                "blanketPurchaseOrder.orderDetails.commodityCode", sourceGuid, sourceId);
                        }
                        else
                        {
                            detail.CommodityCode = new GuidObject2(commodityCode.Guid);
                        }
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError(
                            string.Concat("Unable to retrieve commodity code information for blanket purchase order: '", blanketPurchaseOrder.Number, "'"),
                            "blanketPurchaseOrder.orderDetails.commodityCode", sourceGuid, sourceId);
                    }
                }

                detail.Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                {
                    Value = blanketPurchaseOrder.Amount,
                    Currency = currency
                };

                if (blanketPurchaseOrder.Requisitions != null && blanketPurchaseOrder.Requisitions.Any())
                {
                    detail.ReferenceRequisitions = new List<BlanketPurchaseOrdersReferenceDtoProperty>();
                    foreach (var requisitionId in blanketPurchaseOrder.Requisitions)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(requisitionId))
                            {
                                var reference = new BlanketPurchaseOrdersReferenceDtoProperty();
                                var ReqGuid = new GuidObject2(await blanketPurchaseOrderRepository.GetGuidFromIdAsync(requisitionId, "REQUISITIONS"));
                                reference.Requisition = ReqGuid;

                                detail.ReferenceRequisitions.Add(reference);
                            }
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(
                                string.Concat("Unable to retrieve requisition information for blanket purchase order: '", blanketPurchaseOrder.Number, "'"),
                                "blanketPurchaseOrder.orderDetails.referenceRequisitions.requisition.id", sourceGuid, sourceId);
                        }
                    }
                }

                var accountDetails = new List<BlanketPurchaseOrdersAccountDetailDtoProperty>();
                foreach (var glDistribution in blanketPurchaseOrder.GlDistributions)
                {
                    if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                    {
                        var accountDetail = new BlanketPurchaseOrdersAccountDetailDtoProperty();
                        //var acctNumber = glDistribution.GlAccountNumber.Replace("_", "-");
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

                        var allocation = new BlanketPurchaseOrdersAllocationDtoProperty();

                        var allocated = new BlanketPurchaseOrdersAllocatedDtoProperty();

                        allocated.Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Value = glDistribution.EncumberedAmount,
                            Currency = currency
                        };
                        allocated.Percentage = glDistribution.Percentage;

                        allocation.Allocated = allocated;
                        accountDetail.Allocation = allocation;
                        accountDetails.Add(accountDetail);
                    }
                }
                if (accountDetails != null && accountDetails.Any())
                {
                    detail.AccountDetails = accountDetails;
                }
            }

            return detail;
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
        /// Build an OverrideShippingDestinationDtoProperty DTO object from a PurchaseOrder entity
        /// </summary>
        /// <param name="source">PurchaseOrder Entity Object</param>
        /// <returns>An <see cref="OverrideShippingDestinationDtoProperty"> OverrideShippingDestinationDtoProperty object <see cref="OverrideShippingDestinationDtoProperty"/> in EEDM format</returns>
        private async Task<OverrideShippingDestinationDtoProperty> BuildOverrideShippingDestinationDtoAsync(Domain.ColleagueFinance.Entities.BlanketPurchaseOrder source,
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
        private async Task<ManualVendorDetailsDtoProperty> BuildManualVendorDetailsDtoAsync(Domain.ColleagueFinance.Entities.BlanketPurchaseOrder source,
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
        private async Task<AddressPlace> BuildAddressPlace(string addressCountry, string addressCity,
            string addressState, string addressZip, string hostCountry, string intgAltShipCountry = "", bool bypassCache = false)
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
            else if (!string.IsNullOrEmpty(addressCountry))
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
            // todo srm put if statement around zip assignment
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
        /// Permission code that allows a READ operation on a blanket purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBlanketPurchaseOrderPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrder);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view blanket purchase orders.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a READ operation on a blanket purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBlanketPurchaseOrdersPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrders) || HasPermission(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders);

            if (!hasPermission)
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view blanket-purchase-orders.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' does not have permission to view blanket purchase orders.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Permission code that allows a UPDATE/CREATE operation on a blanket purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdateBlanketPurchaseOrderPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.UpdateBlanketPurchaseOrders);

            if (!hasPermission)
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update blanket-purchase-orders.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' does not have permission to create or update blanket purchase orders.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Permission code that allows bypass of approvals on a purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private bool CheckBypassApprovalsPermission()
        {
            return HasPermission(ColleagueFinancePermissionCodes.ByPassBlanketPurchaseOrderApproval);
        }
    }
}

