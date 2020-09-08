// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
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
using System.Linq;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IRequisitionService interface
    /// </summary>
    [RegisterType]
    public class RequisitionService : BaseCoordinationService, IRequisitionService
    {
        private IRequisitionRepository requisitionRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository;
        private IAccountFundsAvailableRepository accountFundsAvailableRepository;
        private IBuyerRepository buyerRepository;
        private IReferenceDataRepository referenceDataRepository;
        private IVendorsRepository vendorsRepository;
        private IPersonRepository personsRepository;
        private IConfigurationRepository configurationRepository;
        private IAddressRepository addressRepository;
        private IStaffRepository staffRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;
        private IProcurementsUtilityService procurementsUtilityService;

        private IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> _accountsPayableSources = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType> _commodityUnitType = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> _vendorTerm = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType> _freeOnBoardType = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination> _shipToDestination = null;
        private IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode> _commodityCode = null;
        private IEnumerable<Domain.Base.Entities.CommerceTaxCode> _commerceTaxCode = null;
        private IEnumerable<Domain.Base.Entities.State> _states = null;
        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        IDictionary<string, string> _projectReferenceIds = null;

        // This constructor initializes the private attributes
        public RequisitionService(IRequisitionRepository requisitionRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceDataRepository,
            IAccountFundsAvailableRepository accountFundsAvailableRepository,
            IReferenceDataRepository referenceDataRepository,
            IBuyerRepository buyerRepository,
            IPersonRepository personRepository,
            IVendorsRepository vendorsRepository,
            IAddressRepository addressRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IStaffRepository staffRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IProcurementsUtilityService procurementsUtilityService,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.requisitionRepository = requisitionRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.colleagueFinanceReferenceDataRepository = colleagueFinanceReferenceDataRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.vendorsRepository = vendorsRepository;
            this.addressRepository = addressRepository;
            this.accountFundsAvailableRepository = accountFundsAvailableRepository;
            this.buyerRepository = buyerRepository;
            this.personsRepository = personRepository;
            this.configurationRepository = configurationRepository;
            this.staffRepository = staffRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
            this.procurementsUtilityService = procurementsUtilityService;
        }

        /// <summary>
        /// Returns the requisition selected by the user
        /// </summary>
        /// <param name="id">ID for the requested requisition</param>        
        /// <returns>Requisition DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.Requisition> GetRequisitionAsync(string id)
        {
            // Check the permission code to view a requisition.
            CheckRequisitionViewPermission();

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

            // Get the requisition domain entity from the repository
            var requisitionDomainEntity = await requisitionRepository.GetRequisitionAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

            if (requisitionDomainEntity == null)
            {
                throw new ArgumentNullException("requisitionDomainEntity", "requisitionDomainEntity cannot be null.");
            }

            await AssignGlDescription(glConfiguration, requisitionDomainEntity);

            // Convert the requisition and all its child objects into DTOs
            var requisitionDtoAdapter = new RequisitionEntityToDtoAdapter(_adapterRegistry, logger);
            var requisitionDto = requisitionDtoAdapter.MapToType(requisitionDomainEntity, glConfiguration.MajorComponentStartPositions);

            // Throw an exception if there are no line items being returned since access to the document
            // is governed by access to the GL numbers on the line items, and a line item will not be returned
            // if the user does not have access to at least one of the line items.
            if (requisitionDto.LineItems == null || requisitionDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access requisition.");
            }

            return requisitionDto;
        }

        /// <summary>
        /// Returns the list of requisition summary object for the user
        /// </summary>
        /// <param name="id">Person ID</param>
        /// <returns>Requisition Summary DTOs</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionSummary>> GetRequisitionsSummaryByPersonIdAsync(string personId)
        {
            List<Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionSummary> requistionSummaryDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionSummary>();

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(personId);

            //check if personId has staff record            
            await CheckStaffRecordAsync(personId);

            // Check the permission code to view a requisition.
            CheckRequisitionViewPermission();

            // Get the list of requisition summary domain entity from the repository
            var requisitionSummaryDomainEntities = await requisitionRepository.GetRequisitionsSummaryByPersonIdAsync(personId);

            if (requisitionSummaryDomainEntities == null || !requisitionSummaryDomainEntities.Any())
            {
                return requistionSummaryDtos;
            }

            //sorting
            var sortOrderSequence = new List<string> { RequisitionStatus.InProgress.ToString(), RequisitionStatus.NotApproved.ToString(), RequisitionStatus.Outstanding.ToString(), RequisitionStatus.PoCreated.ToString() };
            requisitionSummaryDomainEntities = requisitionSummaryDomainEntities.OrderBy(item => sortOrderSequence.IndexOf(item.Status.ToString())).ThenByDescending(x => int.Parse(x.Id));

            // Convert the requisition summary and all its child objects into DTOs
            var requisitionSummaryDtoAdapter = new RequisitionSummaryEntityDtoAdapter(_adapterRegistry, logger);
            foreach (var requisitionDomainEntity in requisitionSummaryDomainEntities)
            {
                requistionSummaryDtos.Add(requisitionSummaryDtoAdapter.MapToType(requisitionDomainEntity));
            }

            return requistionSummaryDtos;
        }

        /// <summary>
        /// Returns the requisition selected by the user
        /// </summary>
        /// <param name="id">ID for the requested requisition</param>        
        /// <returns>Modify Requisition DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.ModifyRequisition> GetRequisitionForModifyWithLineItemDefaultsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Requisition ID must be specified.");
            }

            Ellucian.Colleague.Dtos.ColleagueFinance.ModifyRequisition modifyRequisitionDto = new Ellucian.Colleague.Dtos.ColleagueFinance.ModifyRequisition();
            var requisitionDto = await GetRequisitionAsync(id);

            if (requisitionDto == null)
            {
                throw new ArgumentNullException("requisitionDto", "requisition cannot be null.");
            }

            modifyRequisitionDto.Requisition = requisitionDto;
            var defaultLineItemAdditionalDetails = await procurementsUtilityService.GetNewLineItemDefaultAdditionalInformation(requisitionDto.CommodityCode, requisitionDto.VendorId, requisitionDto.ApType);
            modifyRequisitionDto.DefaultLineItemAdditionalDetails = defaultLineItemAdditionalDetails;
            return modifyRequisitionDto;
        }

        /// <summary>
        /// Create/Update a requisition.
        /// </summary>
        /// <param name="requisitionCreateUpdateRequest">The requisition create update request DTO.</param>        
        /// <returns>The requisition create update response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionCreateUpdateResponse> CreateUpdateRequisitionAsync(Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionCreateUpdateRequest requisitionCreateUpdateRequest)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionCreateUpdateResponse response = new Dtos.ColleagueFinance.RequisitionCreateUpdateResponse();
            if (requisitionCreateUpdateRequest == null)
            {
                throw new ArgumentNullException("requisitionCreateRequest", "Must provide a requisitionCreateRequest object");
            }

            if (string.IsNullOrEmpty(requisitionCreateUpdateRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            if (requisitionCreateUpdateRequest.Requisition == null)
            {
                throw new ArgumentNullException("requisition", "Requisition must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(requisitionCreateUpdateRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(requisitionCreateUpdateRequest.PersonId);

            //Change to create or update permission, after creating new permission.            
            CheckRequisitionCreateUpdatePermission();

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
            var requisitionCreateUpdateRequestEntity = ConvertCreateUpdateRequestDtoToEntity(requisitionCreateUpdateRequest, glConfiguration);
            RequisitionCreateUpdateResponse responseEntity = null;

            //Check if requisition sent for modify exist in colleague for user.            
            if (!string.IsNullOrEmpty(requisitionCreateUpdateRequest.Requisition.Id))
            {
                // Get the ID for the person who is logged in, and use the ID to get their GL access level.
                var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
                if (generalLedgerUser == null)
                {
                    throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
                }

                // Get the requisition domain entity from the repository
                var originalRequisition = await requisitionRepository.GetRequisitionAsync(requisitionCreateUpdateRequest.Requisition.Id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);
                if (originalRequisition == null)
                {
                    var message = string.Format("{0} requisition number doesn't exist for modify.", requisitionCreateUpdateRequest.Requisition.Number);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                responseEntity = await requisitionRepository.UpdateRequisitionsAsync(requisitionCreateUpdateRequestEntity, originalRequisition);
            }
            else
            {
                responseEntity = await requisitionRepository.CreateRequisitionsAsync(requisitionCreateUpdateRequestEntity);
            }

            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.RequisitionCreateUpdateResponse, Dtos.ColleagueFinance.RequisitionCreateUpdateResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }
            return response;
        }

        /// <summary>
        /// Delete a requisition.
        /// </summary>
        /// <param name="requisitionDeleteRequest">The requisition delete request DTO.</param>        
        /// <returns>The requisition delete response DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteResponse> DeleteRequisitionsAsync(Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteRequest requisitionDeleteRequest)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionDeleteResponse response = new Dtos.ColleagueFinance.RequisitionDeleteResponse();
            if (requisitionDeleteRequest == null)
            {
                throw new ArgumentNullException("requisitionDeleteRequest", "Must provide a requisitionDeleteRequest object");
            }

            if (string.IsNullOrEmpty(requisitionDeleteRequest.PersonId))
            {
                throw new ArgumentNullException("personId", "Person ID must be specified.");
            }
            if (string.IsNullOrEmpty(requisitionDeleteRequest.RequisitionId))
            {
                throw new ArgumentNullException("requisitionId", "Requisition Id must be specified.");
            }
            if (string.IsNullOrEmpty(requisitionDeleteRequest.ConfirmationEmailAddresses))
            {
                throw new ArgumentNullException("confirmationEmailAddresses", "confirmationEmailAddresses must be specified.");
            }
            // check if personId passed is same currentuser
            CheckIfUserIsSelf(requisitionDeleteRequest.PersonId);

            //check if personId has staff record
            await CheckStaffRecordAsync(requisitionDeleteRequest.PersonId);

            //Check for Delete requisition Permission
            CheckRequisitionDeletePermission();

            //Convert DTO to domain entity            
            var requisitionDeleteRequestEntity = ConvertDeleteRequestDtoToEntity(requisitionDeleteRequest);
            RequisitionDeleteResponse responseEntity = null;
            
            responseEntity = await requisitionRepository.DeleteRequisitionsAsync(requisitionDeleteRequestEntity);
            
            var createResponseAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.RequisitionDeleteResponse, Dtos.ColleagueFinance.RequisitionDeleteResponse>();

            if (responseEntity != null)
            {
                response = createResponseAdapter.MapToType(responseEntity);
            }

            return response;
        }


        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get Requisition data.
        /// </summary>
        /// <param name="offset">Paging offset</param>
        /// <param name="limit">Paging limit</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>List of <see cref="Dtos.Requisitions">Requisitions</see></returns>
        public async Task<Tuple<IEnumerable<Requisitions>, int>> GetRequisitionsAsync(int offset, int limit, bool bypassCache = false)
        {

            CheckViewRequisitionPermission();

            var requisitionsCollection = new List<Ellucian.Colleague.Dtos.Requisitions>();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            var requisitionEntities = await requisitionRepository.GetRequisitionsAsync(offset, limit);
            var totalRecords = requisitionEntities.Item2;
            try
            {
                var projectIds = requisitionEntities.Item1
                             .SelectMany(t => t.LineItems)
                             .Where(i => i.GlDistributions != null)
                             .SelectMany(p => p.GlDistributions)
                             .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                             .Select(pj => pj.ProjectId)
                             .ToList()
                             .Distinct();
                if (projectIds != null && projectIds.Any())
                {
                    _projectReferenceIds = await requisitionRepository.GetProjectReferenceIds(projectIds.ToArray());
                }

                foreach (var requisitionEntity in requisitionEntities.Item1)
                {
                    if (requisitionEntity.Guid != null)
                    {


                        var requisitionDto = await this.ConvertRequisitionEntityToDtoAsync(requisitionEntity, glConfiguration, bypassCache);
                        requisitionsCollection.Add(requisitionDto);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return new Tuple<IEnumerable<Dtos.Requisitions>, int>(requisitionsCollection, totalRecords);
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get Requisition data from a Guid
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <returns><see cref="Dtos.Requisitions">A Requisitions DTO</see></returns>
        public async Task<Requisitions> GetRequisitionsByGuidAsync(string guid, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "A GUID is required to obtain a Requisition.");
            }
            CheckViewRequisitionPermission();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            try
            {
                var requisitionData = await requisitionRepository.GetRequisitionsByGuidAsync(guid);
                if (requisitionData == null)
                {
                    throw new KeyNotFoundException(string.Format("Requisition not found for guid: {0} ", guid));
                }

                if (requisitionData.Status == RequisitionStatus.InProgress)
                {
                    throw new ArgumentException("Requisitions at a current status of 'Unfinished' cannot be viewed or modified");
                }
                var projectIds = requisitionData.LineItems
                        .Where(i => i.GlDistributions != null)
                        .SelectMany(p => p.GlDistributions)
                        .Where(p => !(string.IsNullOrEmpty(p.ProjectId)))
                        .Select(pj => pj.ProjectId)
                        .ToList()
                        .Distinct();
                if (projectIds != null && projectIds.Any())
                {
                    _projectReferenceIds = await requisitionRepository.GetProjectReferenceIds(projectIds.ToArray());
                }
                return await ConvertRequisitionEntityToDtoAsync(requisitionData, glConfiguration);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("No requisitions was found for guid  " + guid, ex);
            }
            catch (RepositoryException ex)
            {
                throw new RepositoryException("No requisitions was found for guid  " + guid, ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("No requisitions was found for guid  " + guid, ex);
            }
        }

        /// <summary>
        /// Delete a requisition from the database
        /// </summary>
        /// <param name="id">The requested requisition GUID</param>
        /// <returns></returns>
        public async Task DeleteRequisitionAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Must provide a requisition guid for deletion.");
            }

            CheckDeleteRequisitionPermission();
            try
            {
                var requisitionData = await requisitionRepository.GetRequisitionsByGuidAsync(guid);
                if (requisitionData == null)
                {
                    throw new KeyNotFoundException();
                }

                await requisitionRepository.DeleteRequisitionAsync(guid);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("Requisitions not found for guid: '{0}'.", guid));
            }
        }

        /// <summary>
        /// Update a requisition in the database from a DTO.
        /// </summary>
        /// <param name="requisitions">The <see cref="Requisitions">requisitions</see> DTO to update.</param>
        /// <returns>The newly updated <see cref="Requisitions">requisitions</see></returns>
        public async Task<Requisitions> UpdateRequisitionsAsync(Requisitions requisition)
        {
            if (requisition == null)
                throw new ArgumentNullException("requisition", "Must provide a requisition for update");
            if (string.IsNullOrEmpty(requisition.Id))
                throw new ArgumentNullException("requisition", "Must provide a guid for requisition update");

            // get the ID associated with the incoming guid
            var requisitionId = await requisitionRepository.GetRequisitionsIdFromGuidAsync(requisition.Id);

            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            var overRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            // verify the GUID exists to perform an update.  If not, perform a create instead

            // verify the user has the permission to update a requisitions
            CheckUpdateRequisitionPermission();

            requisitionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                overRideGLs = await CheckFunds(requisition, requisitionId);
                if ((requisition.LineItems) != null && (requisition.LineItems.Any()))
                {
                    var projectRefNos = requisition.LineItems
                   .Where(i => i.AccountDetail != null)
                   .SelectMany(p => p.AccountDetail)
                   .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                   .Select(pj => pj.AccountingString.Split('*')[1])
                   .ToList()
                   .Distinct();

                    if (projectRefNos != null && projectRefNos.Any())
                    {
                        _projectReferenceIds = await requisitionRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                    }
                }
                // map the DTO to entities
                var requisitionEntity
                = await ConvertRequisitionsDtoToEntityAsync(requisitionId, requisition, glConfiguration.MajorComponents.Count, true);

                 // update the entity in the database
                    var updatedRequisitionEntity =
                    await requisitionRepository.UpdateRequisitionAsync(requisitionEntity);


                var dtoRequisition = await this.ConvertRequisitionEntityToDtoAsync(updatedRequisitionEntity, glConfiguration, true);

                if (dtoRequisition.LineItems != null && dtoRequisition.LineItems.Any() && overRideGLs != null && overRideGLs.Any())
                {
                    int lineCount = 0;
                    foreach (var lineItem in dtoRequisition.LineItems)
                    {
                        int detailCount = 0;
                        lineCount++;
                        foreach (var detail in lineItem.AccountDetail)
                        {
                            detailCount++;
                            var posID = lineCount.ToString() + "." + detailCount.ToString();
                            var findOvr = overRideGLs.FirstOrDefault(a => a.Sequence == posID || a.Sequence == posID + ".DS");
                            if ((findOvr != null) && (findOvr.AvailableStatus == FundsAvailableStatus.Override))
                                detail.BudgetCheck = AccountBudgetCheck.Override;
                        }
                    }
                }
                // return the newly updated DTO
                return dtoRequisition;
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

        /// <summary>
        /// Create a Requisition in the database from a DTO.
        /// </summary>
        /// <param name="requisitions">The <see cref="Requisitions">requisitions</see> to create in the database.</param>
        /// <returns>The newly created <see cref="Requisitions">requisitions</see></returns>
        public async Task<Requisitions> CreateRequisitionsAsync(Requisitions requisition)
        {
            if (requisition == null)
                throw new ArgumentNullException("requisitions", "Must provide a requisitions for create");
            if (string.IsNullOrEmpty(requisition.Id))
                throw new ArgumentNullException("requisitions", "Must provide a guid for requisitions create");
            if (requisition.Id != Guid.Empty.ToString())
                throw new ArgumentNullException("requisitions", "Must provide a nil guid for requisitions create");

            Colleague.Domain.ColleagueFinance.Entities.Requisition createdRequisition = null;

            var overRideGLs = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();

            // verify the user has the permission to create a requisitions
            CheckUpdateRequisitionPermission();

            requisitionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                overRideGLs = await CheckFunds(requisition);

                if ((requisition.LineItems) != null && (requisition.LineItems.Any()))
                {
                    var projectRefNos = requisition.LineItems
                      .Where(i => i.AccountDetail != null)
                      .SelectMany(p => p.AccountDetail)
                      .Where(p => (!string.IsNullOrEmpty(p.AccountingString)) && (p.AccountingString.Split('*').Count() > 1))
                      .Select(pj => pj.AccountingString.Split('*')[1])
                      .ToList()
                      .Distinct();

                    if (projectRefNos != null && projectRefNos.Any())
                    {
                        _projectReferenceIds = await requisitionRepository.GetProjectIdsFromReferenceNo(projectRefNos.ToArray());
                    }
                }

                var requisitionEntity
                         = await ConvertRequisitionsDtoToEntityAsync(requisition.Id, requisition, glConfiguration.MajorComponents.Count, true);

                // create a requisition entity in the database
                createdRequisition =
                    await requisitionRepository.CreateRequisitionAsync(requisitionEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            var dtoRequisition = await this.ConvertRequisitionEntityToDtoAsync(createdRequisition, glConfiguration, true);

            if (dtoRequisition.LineItems != null && dtoRequisition.LineItems.Any() && overRideGLs != null && overRideGLs.Any())
            {
                int lineCount = 0;
                foreach (var lineItem in dtoRequisition.LineItems)
                {
                    int detailCount = 0;
                    lineCount++;
                    foreach (var detail in lineItem.AccountDetail)
                    {
                        detailCount++;
                        var posID = lineCount.ToString() + "." + detailCount.ToString();
                        var findOvr = overRideGLs.FirstOrDefault(a => a.Sequence == posID || a.Sequence == posID + ".DS");
                        if ((findOvr != null) && (findOvr.AvailableStatus == FundsAvailableStatus.Override))
                            detail.BudgetCheck = AccountBudgetCheck.Override;
                    }
                }
            }

            // return the newly created requisition
            return dtoRequisition;
        }

        #region private methods

        /// <summary>
        /// Convert a requisitions DTO object into a requisition domain entity object
        /// </summary>
        /// <param name="guid">requisition guid</param>
        /// <param name="requisition">requisitions DTO</param>
        /// <returns>requisition domain entity object</returns>
        private async Task<Domain.ColleagueFinance.Entities.Requisition> ConvertRequisitionsDtoToEntityAsync(string requisitionId, Requisitions requisition, int GLCompCount, bool bypassCache = true)
        {

            if (requisition == null || string.IsNullOrEmpty(requisition.Id))
                throw new ArgumentNullException("Requisitions", "Must provide guid for Requisitions");

            var existingVendor = (requisition.Vendor != null && requisition.Vendor.ExistingVendor != null) ?
                requisition.Vendor.ExistingVendor : null;
            var manualVendor = (requisition.Vendor != null && requisition.Vendor.ManualVendorDetails != null) ?
                requisition.Vendor.ManualVendorDetails : null;                            

            if ((requisition.RequestedOn == null) || (requisition.RequestedOn == DateTime.MinValue))
                throw new ArgumentNullException("Requisitions", "Must provide RequestedOn date for Requisitions");

            if ((requisition.TransactionDate == null) || (requisition.TransactionDate == DateTime.MinValue))
                throw new ArgumentNullException("Requisitions", "Must provide TransactionDate for Requisitions");

            if ((requisition.Initiator == null) || (string.IsNullOrWhiteSpace(requisition.Initiator.Name)))
                throw new ArgumentNullException("Requisitions", "Must provide Initiator name for Requisitions");

            if ((requisition.LineItems == null) || (!requisition.LineItems.Any()))
                throw new ArgumentNullException("Requisitions", "Must provide Line Item for Requisitions");

            var reqStatus = RequisitionStatus.InProgress;
            if (requisition.Status != null)
            {
                reqStatus = GetRequisitionStatus(requisition);
            }

            var date = (requisition.TransactionDate == DateTime.MinValue) ? DateTime.Now.Date
                : requisition.TransactionDate;
            string currency = null;

            if (string.IsNullOrEmpty(requisitionId))
            {
                requisitionId = "NEW";
            }

            var requisitionEntity = new Domain.ColleagueFinance.Entities.Requisition(
                requisitionId, requisition.Id ?? new Guid().ToString(), requisition.RequisitionNumber, "", reqStatus, date, requisition.RequestedOn);

            if ((requisition.SubmittedBy != null) && (!string.IsNullOrWhiteSpace(requisition.SubmittedBy.Id)))
            {
                var submittedById = await personsRepository.GetPersonIdFromGuidAsync(requisition.SubmittedBy.Id);
                if (string.IsNullOrEmpty(submittedById))
                {
                    throw new Exception(string.Concat("submittedBy.id is not found: Requistion ID", requisitionId, " Submitted By: ", requisition.SubmittedBy.Id));
                }
                requisitionEntity.IntgSubmittedBy = submittedById;
            }
            // New flag to send data to the CTX to bypass the tax forms
            requisitionEntity.bypassTaxForms = CheckBypassTaxformsPermission();
            // New flag to send data to the CTX to bypass approvals
            requisitionEntity.bypassApprovals = CheckBypassApprovalsPermission();
            if (requisition.TransactionDate != default(DateTime))
                requisitionEntity.MaintenanceDate = requisition.TransactionDate;

            if ((requisition.DeliveredBy != default(DateTime)))
                requisitionEntity.DeliveryDate = requisition.DeliveredBy;

            if ((requisition.Buyer != null) && (!string.IsNullOrWhiteSpace(requisition.Buyer.Id)))
            {
                var buyerId = await buyerRepository.GetBuyerIdFromGuidAsync(requisition.Buyer.Id);
                if (string.IsNullOrEmpty(buyerId))
                {
                    throw new Exception(string.Concat("Missing buyer information for Requisition ID: ", requisitionId, " Guid: ", requisitionEntity.Guid, " Buyer: ", requisition.Buyer.Id));
                }
                requisitionEntity.Buyer = buyerId;
            }

            if ((requisition.Initiator != null) && (requisition.Initiator.Detail != null) && !(string.IsNullOrWhiteSpace(requisition.Initiator.Detail.Id)))
            {
                var initiatorId = await personsRepository.GetPersonIdFromGuidAsync(requisition.Initiator.Detail.Id);
                if (string.IsNullOrEmpty(initiatorId))
                {
                    throw new Exception(string.Concat("Colleague ID not found for initiator.detail.id.  Requisition ID ", requisitionId, " Guid: ", requisitionEntity.Guid, " Initiator: ", requisition.Initiator.Detail.Id));
                }
                requisitionEntity.DefaultInitiator = initiatorId;
            }

            if (requisition.PaymentSource != null) 
            {
                if (string.IsNullOrWhiteSpace(requisition.PaymentSource.Id))
                {
                    throw new Exception("PaymentSource id is required when submitting a PaymentSource.");
                }
                else
                {
                    var accountsPayableSources = await this.GetAllAccountsPayableSourcesAsync(bypassCache);
                    if (accountsPayableSources == null)
                    {
                        throw new Exception("Unable to retrieve AccountsPayableSources");
                    }

                    var accountsPayableSource = accountsPayableSources.FirstOrDefault(ap => ap.Guid == requisition.PaymentSource.Id);
                    if (accountsPayableSource == null)
                        throw new KeyNotFoundException("AccountsPayableSources not found for guid: " + requisition.PaymentSource.Id);
                    requisitionEntity.ApType = accountsPayableSource.Code;
                }
            }

            if (requisition.Shipping != null)
            {
                if ((requisition.Shipping.ShipTo != null) && !(string.IsNullOrWhiteSpace(requisition.Shipping.ShipTo.Id)))
                {
                    var shipToDestinations = await GetShipToDestinationsAsync(bypassCache);
                    if (shipToDestinations == null)
                    {
                        throw new Exception("Unable to retrieve ShipToDestination");
                    }
                    var shipToDestination = shipToDestinations.FirstOrDefault(stc => stc.Guid == requisition.Shipping.ShipTo.Id);
                    if (shipToDestination == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to retrieve ShipToDestination for: ", requisition.Shipping.ShipTo.Id));
                    }
                    requisitionEntity.ShipToCode = shipToDestination.Code;
                }

                if ((requisition.Shipping.FreeOnBoard != null) && !(string.IsNullOrWhiteSpace(requisition.Shipping.FreeOnBoard.Id)))
                {
                    var freeOnBoardTypes = await GetFreeOnBoardTypesAsync(bypassCache);
                    if (freeOnBoardTypes == null)
                    {
                        throw new Exception("Unable to retrieve FreeOnBoardTypes");
                    }
                    var freeOnBoardType = freeOnBoardTypes.FirstOrDefault(fob => fob.Guid == requisition.Shipping.FreeOnBoard.Id);
                    if (freeOnBoardType == null)
                    {
                        throw new KeyNotFoundException(string.Concat("Unable to retrieve FreeOnBoardTypes for: ", requisition.Shipping.FreeOnBoard.Id));
                    }
                    requisitionEntity.Fob = freeOnBoardType.Code;
                }
            }
            if (requisition.OverrideShippingDestination != null)
            {
                var overrideShippingDestinationDto = requisition.OverrideShippingDestination;
                if (!string.IsNullOrEmpty(requisition.OverrideShippingDestination.Description))
                {
                    requisitionEntity.AltShippingName = requisition.OverrideShippingDestination.Description;
                }

                if (overrideShippingDestinationDto.AddressLines != null && overrideShippingDestinationDto.AddressLines.Any())
                {
                    requisitionEntity.AltShippingAddress = overrideShippingDestinationDto.AddressLines;
                }

                var place = overrideShippingDestinationDto.Place;
                if (place != null && place.Country != null)
                {
                    requisitionEntity.IntgAltShipCountry = place.Country.Code.ToString();

                    if (!string.IsNullOrEmpty(place.Country.Locality))
                    {
                        requisitionEntity.AltShippingCity = place.Country.Locality;
                    }
                    if (place.Country.Region != null && !string.IsNullOrEmpty(place.Country.Region.Code))
                    {
                        requisitionEntity.AltShippingState = place.Country.Region.Code.Split('-')[1];
                    }
                    requisitionEntity.AltShippingZip = place.Country.PostalCode;
                }
                if (requisition.OverrideShippingDestination.Contact != null)
                {
                    if (!string.IsNullOrEmpty(requisition.OverrideShippingDestination.Contact.Number))
                        requisitionEntity.AltShippingPhone = requisition.OverrideShippingDestination.Contact.Number;
                    if (!string.IsNullOrEmpty(requisition.OverrideShippingDestination.Contact.Extension))
                        requisitionEntity.AltShippingPhoneExt = requisition.OverrideShippingDestination.Contact.Extension;
                }
            }

            string vendorId = string.Empty;
            if (existingVendor != null && existingVendor.Vendor != null && !string.IsNullOrEmpty(existingVendor.Vendor.Id))
            {

                try
                {
                    vendorId = await requisitionRepository.GetRequisitionsIdFromGuidAsync(existingVendor.Vendor.Id);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException
                        (string.Concat("The vendor id must correspond with a valid vendor record : ", existingVendor.Vendor.Id));
                }
                if (string.IsNullOrEmpty(vendorId))
                {
                    throw new ArgumentNullException("requisition", "Must provide valid vendor.existingVendor.vendor.id for requisition");
                }
                requisitionEntity.VendorId = vendorId;
            }
            string alternativeVendorAddressId = string.Empty;
            if ((existingVendor != null && existingVendor.AlternativeVendorAddress != null) && (!string.IsNullOrEmpty(existingVendor.AlternativeVendorAddress.Id)))
            {
                try
                {
                    alternativeVendorAddressId = await this.addressRepository.GetAddressFromGuidAsync(existingVendor.AlternativeVendorAddress.Id);
                }
                catch (Exception)
                {
                    throw new ArgumentException
                        (string.Concat("The vendor.existingVendor.alternativeVendorAddress.id must correspond with a valid address record : ", existingVendor.AlternativeVendorAddress.Id));
                }
                if (string.IsNullOrEmpty(alternativeVendorAddressId))
                {
                    throw new ArgumentNullException("requisition", "Must provide valid vendor.existingVendor.alternativeVendorAddress.id for requisition");
                }
                requisitionEntity.VendorAlternativeAddressId = alternativeVendorAddressId;
            }

            if (manualVendor != null)
            {
                if (manualVendor.Type.HasValue)
                {
                    requisitionEntity.IntgCorpPerIndicator = manualVendor.Type.ToString();
                }

                if (manualVendor.Name != null && manualVendor.Name.Any())
                {
                    requisitionEntity.MiscName = new List<string>();
                    requisitionEntity.MiscName.Add(manualVendor.Name);
                }

                if (manualVendor.AddressLines != null && manualVendor.AddressLines.Any())
                {
                    requisitionEntity.MiscAddress = manualVendor.AddressLines;
                }
                var place = manualVendor.Place;
                if (place != null)
                {
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
                        // var countryEntity = (await GetAllCountriesAsync(false)).Where(cc => cc.IsoAlpha3Code == place.Country.Code.ToString()).FirstOrDefault();
                        if (countryEntity == null)
                        {
                            throw new ArgumentException(string.Format("Country Code of '{0}' cannot be found in the COUNTRIES table (ISO.ALPHA3.CODE) field in Colleague. ", place.Country.Code.ToString()), "manualVendor.place.country.code");
                        }
                        requisitionEntity.MiscCountry = countryEntity.Code;
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
                        requisitionEntity.MiscState = stateEntity.Code;
                    }
                    if (place.Country != null && !string.IsNullOrEmpty(place.Country.Locality))
                        requisitionEntity.MiscCity = place.Country.Locality;


                    if (place.Country != null && !string.IsNullOrEmpty(place.Country.PostalCode))
                        requisitionEntity.MiscZip = place.Country.PostalCode;
                }
            }

            if ((requisition.PaymentTerms != null) && (!string.IsNullOrEmpty(requisition.PaymentTerms.Id)))
            {
                var vendorTerms = await this.GetVendorTermsAsync(bypassCache);
                if (vendorTerms == null)
                {
                    throw new Exception("Unable to retrieve Vendor Terms");
                }
                var vendorTerm = vendorTerms.FirstOrDefault(ap => ap.Guid == requisition.PaymentTerms.Id);
                if (vendorTerm == null)
                {
                    throw new KeyNotFoundException("PaymentTerm not found for guid: " + requisition.PaymentTerms.Id);
                }
                requisitionEntity.VendorTerms = vendorTerm.Code;
            }

            if (requisition.Comments != null && requisition.Comments.Any())
            {
                requisitionEntity.Comments = string.Empty;
                requisitionEntity.InternalComments = string.Empty;
                foreach (var comment in requisition.Comments)
                {
                    switch (comment.Type)
                    {
                        case CommentTypes.NotPrinted:
                            requisitionEntity.InternalComments = !string.IsNullOrEmpty(requisitionEntity.InternalComments) ?
                                string.Concat(requisitionEntity.InternalComments, " ", comment.Comment) : comment.Comment;
                            break;
                        case CommentTypes.Printed:
                            requisitionEntity.Comments = !string.IsNullOrEmpty(requisitionEntity.Comments) ?
                                string.Concat(requisitionEntity.Comments, " ", comment.Comment) : comment.Comment;
                            break;
                    }
                }

            }

            if ((requisition.LineItems != null) && (requisition.LineItems.Any()))
            {
                foreach (var lineItem in requisition.LineItems)
                {
                    if ((lineItem.AccountDetail == null) || (!lineItem.AccountDetail.Any()))
                    {
                        throw new Exception("lineItems.AccountDetail is required.");
                    }
                    if ((lineItem.UnitPrice == null) || (!lineItem.UnitPrice.Value.HasValue))
                    {
                        throw new Exception("lineItems.unitPrice is required.");
                    }
                    if (string.IsNullOrWhiteSpace(lineItem.Description))
                    {
                        throw new Exception("lineItems.description is required.");
                    }
                    if (lineItem.Quantity == null || !lineItem.Quantity.HasValue)
                    {
                        throw new Exception("lineItems.quantity is required.");
                    }

                    var id = lineItem.LineItemNumber;
                    if (string.IsNullOrEmpty(id))
                        id = "NEW";
                    var description = !(string.IsNullOrEmpty(lineItem.Description)) ? lineItem.Description.Trim() : string.Empty;
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
                        var allCommodityCodes = (await GetCommodityCodesAsync(bypassCache));
                        if ((allCommodityCodes == null) || (!allCommodityCodes.Any()))
                        {
                            throw new Exception("An error occurred extracting all commodity codes");
                        }
                        var commodityCode = allCommodityCodes.FirstOrDefault(c => c.Guid == lineItem.CommodityCode.Id);
                        if (commodityCode == null)
                        {
                            throw new Exception("Unable to determine commodity code represented by guid: " + lineItem.CommodityCode.Id);
                        }
                        apLineItem.CommodityCode = commodityCode.Code;
                    }


                    if ((lineItem.UnitOfMeasure != null) && (!string.IsNullOrEmpty(lineItem.UnitOfMeasure.Id)))
                    {
                        var allCommodityUnitTypes = (await this.GetCommodityUnitTypesAsync(bypassCache));
                        if ((allCommodityUnitTypes == null) || (!allCommodityUnitTypes.Any()))
                        {
                            throw new Exception("An error occurred extracting all commodity unit types");
                        }
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
                    if ((lineItem.Taxes != null) && (lineItem.Taxes.Any()))
                    {
                        var taxCodesEntities = await GetCommerceTaxCodesAsync(true);
                        if (taxCodesEntities != null)
                        {
                            foreach (var lineItemTax in lineItem.Taxes)
                            {
                                if (lineItemTax != null && lineItemTax.Id != null && !string.IsNullOrEmpty(lineItemTax.Id))
                                {
                                    var taxCode = taxCodesEntities.FirstOrDefault(tax => tax.Guid == lineItemTax.Id);
                                    if (taxCode != null)
                                    {
                                        lineItemTaxCodes.Add(taxCode.Code);
                                    }
                                    else
                                    {
                                        throw new Exception("Unable to determine tax code represented by guid: " + lineItemTax.Id);
                                    }
                                }
                            }
                        }
                    }

                    if (lineItem.AccountDetail != null && lineItem.AccountDetail.Any())
                    {
                        foreach (var accountDetails in lineItem.AccountDetail)
                        {
                            if (string.IsNullOrWhiteSpace(accountDetails.AccountingString))
                            {
                                throw new Exception("lineItem.accountDetails.accountingString is required.");
                            }
                            if (accountDetails.Allocation == null)
                            {
                                throw new Exception("lineItem.accountDetails.allocation is required.");
                            }
                            decimal distributionQuantity = 0;
                            decimal distributionAmount = 0;
                            decimal distributionPercent = 0;

                            if (accountDetails.Allocation.Allocated == null)
                            {
                                throw new Exception("lineItem.accountDetails.allocation.allocated is required.");
                            }
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
                            /*
                            var accountingString = accountDetails.AccountingString;
                            if (!string.IsNullOrEmpty(accountingString))
                            {
                                accountingString = accountingString.Replace("-", "_");
                                var Glsplit = accountingString.Split('*');

                                var tempAccountingString = Glsplit[0].Replace("_", "");
                                if (tempAccountingString.Length <= 14)
                                {
                                    accountingString = tempAccountingString;
                                    if (Glsplit.Count() > 1)
                                    {

                                        accountingString = ConvertAccountingStringWithProjectRefNoToProjectId(Glsplit[1], accountingString);
                                    }
                                }
                            }
                           */
                            string accountingString = ConvertAccountingString(GLCompCount, accountDetails.AccountingString);

                            var glDistribution = new LineItemGlDistribution(accountingString, distributionQuantity, distributionAmount, distributionPercent);

                            apLineItem.AddGlDistribution(glDistribution);

                        }

                        if (lineItemTaxCodes != null && lineItemTaxCodes.Any())
                        {
                            foreach (var lineItemTaxCode in lineItemTaxCodes)
                            {
                                if (!string.IsNullOrEmpty(lineItemTaxCode))
                                    apLineItem.AddTax(new LineItemTax(lineItemTaxCode, 0));
                            }
                        }
                    }
                    if (apLineItem != null && requisitionEntity.LineItems.Where(x => x.Id == apLineItem.Id).Count() > 0 && apLineItem.Id != "NEW")
                        throw new ArgumentException("The requisition payload conatins one or more duplicate lineItemNumber");

                    requisitionEntity.AddLineItem(apLineItem);
                }
                if (!string.IsNullOrEmpty(currency))
                    requisitionEntity.CurrencyCode = currency;
            }
            return requisitionEntity;

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

        private static RequisitionStatus GetRequisitionStatus(Requisitions requisition)
        {
            RequisitionStatus reqStatus;
            switch (requisition.Status)
            {
                case RequisitionsStatus.Cancelled:
                    throw new ArgumentException("Requisitions", "Cancelled status not supported for Requisitions");

                case RequisitionsStatus.Inprogress:
                    reqStatus = RequisitionStatus.InProgress; //"U";
                    break;
                case RequisitionsStatus.Notapproved:
                    reqStatus = RequisitionStatus.NotApproved; //"N";
                    break;
                case RequisitionsStatus.Outstanding:
                    reqStatus = RequisitionStatus.Outstanding; //"O";
                    break;
                case RequisitionsStatus.Purchaseordercreated:
                    reqStatus = RequisitionStatus.PoCreated; //"P";
                    break;

                default:
                    // if we get here, we have corrupt data.
                    throw new ApplicationException("Invalid Requisitions status: " + requisition.Status.ToString());
            }

            return reqStatus;
        }

        /// Check if Funds are avialable for this PUT/POST event.
        /// </summary>
        /// <param name="requisition"></param>
        /// <param name="requisitionId"></param>
        /// <returns></returns>
        private async Task<List<Domain.ColleagueFinance.Entities.FundsAvailable>> CheckFunds(Requisitions requisition, string requisitionId = "")
        {
            var fundsAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            var overrideAvailable = new List<Domain.ColleagueFinance.Entities.FundsAvailable>();
            //check if Accounting string has funds
            int lineCount = 0;
            var submittedById = string.Empty;
            if (requisition.SubmittedBy != null && !string.IsNullOrEmpty(requisition.SubmittedBy.Id))
                submittedById = await personsRepository.GetPersonIdFromGuidAsync(requisition.SubmittedBy.Id);
            else
                submittedById = CurrentUser.PersonId;
            var budgetOvrCheckTuple = new List<Tuple<string, bool>>();
            if (requisition.LineItems != null)
            {
                foreach (var lineItems in requisition.LineItems)
                {
                    int detailCount = 0;
                    lineCount++;
                    var accountingStringList = new List<string>();
                    foreach (var details in lineItems.AccountDetail)
                    {
                        detailCount++;

                        if (details.Allocation != null && details.Allocation.Allocated != null &&
                                details.Allocation.Allocated.Amount != null && details.Allocation.Allocated.Amount.Value != null
                                && details.Allocation.Allocated.Amount.Value.HasValue)
                        {
                            string PosID = lineCount.ToString() + "." + detailCount.ToString();
                            if (submittedById != null)
                                PosID = PosID + ".DS";
                            var budgetCheckOverrideFlag = (details.BudgetCheck == AccountBudgetCheck.Override) ? true : false;
                            budgetOvrCheckTuple.Add(new Tuple<string, bool>(details.AccountingString, budgetCheckOverrideFlag));
                            fundsAvailable.Add(new Domain.ColleagueFinance.Entities.FundsAvailable(details.AccountingString)
                            {
                                Sequence = PosID,
                                SubmittedBy = submittedById,
                                Amount = details.Allocation.Allocated.Amount.Value.Value,
                                ItemId = lineItems.LineItemNumber ?? lineItems.LineItemNumber,
                                TransactionDate = requisition.TransactionDate,
                                CurrencyCode = details.Allocation.Allocated.Amount.Currency.ToString()
                            });
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
                if (string.IsNullOrEmpty(requisitionId))
                {
                    requisitionId = "NEW";
                }

                var availableFunds = await accountFundsAvailableRepository.CheckAvailableFundsAsync(fundsAvailable, "", "", "", submittedById, requisitionId);
                if (availableFunds != null)
                {
                    foreach (var availableFund in availableFunds)
                    {
                        if (availableFund.AvailableStatus == FundsAvailableStatus.NotAvailable)
                        {
                            throw new ArgumentException("The accounting string " + availableFund.AccountString + " does not have funds available");
                        }
                        if (availableFund.AvailableStatus == FundsAvailableStatus.Override)
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
        /// Converts an requisition domain entity to its corresponding Requisition DTO
        /// </summary>
        /// <param name="source">requisition domain entity</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="Dtos.Requisitions">Requisitions</see></returns>
        private async Task<Dtos.Requisitions> ConvertRequisitionEntityToDtoAsync(Domain.ColleagueFinance.Entities.Requisition source,
            GeneralLedgerAccountStructure GlConfig, bool bypassCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source is required to obtain a Requisition.");
            }
            try
            {
                var requisition = new Ellucian.Colleague.Dtos.Requisitions();

                var currency = GetCurrencyIsoCode(source.CurrencyCode, source.HostCountry);

                requisition.Id = source.Guid;
                requisition.RequisitionNumber = source.Number;

                requisition.Status = ConvertRequisitionStatus(source.Status);

                if (!string.IsNullOrEmpty(source.IntgSubmittedBy))
                {
                    var submittedByGuid = await this.personsRepository.GetPersonGuidFromIdAsync(source.IntgSubmittedBy);
                    if (string.IsNullOrEmpty(submittedByGuid))
                    {
                        throw new Exception(string.Concat("Missing Submitted By information for requisition: ", source.Id, " Guid: ", source.Guid, " Submitted By: ", source.IntgSubmittedBy));
                    }
                    requisition.SubmittedBy = new GuidObject2(submittedByGuid);
                }

                requisition.RequestedOn = source.Date;

                requisition.TransactionDate = (source.MaintenanceDate != null && source.MaintenanceDate.HasValue)
                    ? Convert.ToDateTime(source.MaintenanceDate) : source.Date;

                if ((source.DesiredDate != null && source.DesiredDate.HasValue))
                    requisition.DeliveredBy = Convert.ToDateTime(source.DesiredDate);

                requisition.Status = ConvertRequisitionStatus(source.Status);

                if (!string.IsNullOrEmpty(source.Buyer))
                {
                    var buyerGuid = await buyerRepository.GetBuyerGuidFromIdAsync(source.Buyer);
                    if (string.IsNullOrEmpty(buyerGuid))
                    {
                        throw new Exception(string.Concat("Missing buyer information for requisition: ", source.Id, " Guid: ", source.Guid, " Buyer: ", source.Buyer));
                    }
                    requisition.Buyer = new GuidObject2(buyerGuid);
                }

                var initiator = new Dtos.DtoProperties.InitiatorDtoProperty();
                initiator.Name = source.InitiatorName;
                if (!string.IsNullOrEmpty(source.DefaultInitiator))
                {
                    var initiatorGuid = await personsRepository.GetPersonGuidFromIdAsync(source.DefaultInitiator);
                    if (string.IsNullOrEmpty(initiatorGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("Missing initiator information for requisition: ", source.Id, " Guid: ", source.Guid, " Initiator: ", source.DefaultInitiator));
                    }
                    initiator.Detail = new GuidObject2(initiatorGuid);
                }
                if ((!string.IsNullOrEmpty(initiator.Name)) || (initiator.Detail != null))
                {
                    requisition.Initiator = initiator;
                }

                var requisitionsShipping = new Dtos.DtoProperties.ShippingDtoProperty();
                if (!string.IsNullOrEmpty(source.ShipToCode))
                {
                    var shipToDestinations = await colleagueFinanceReferenceDataRepository.GetShipToDestinationGuidAsync(source.ShipToCode);
                    if (!string.IsNullOrEmpty(shipToDestinations))
                    {
                        requisitionsShipping.ShipTo = new GuidObject2(shipToDestinations);
                        requisition.Shipping = requisitionsShipping;
                    }
                }
                if (!string.IsNullOrEmpty(source.Fob))
                {
                    var freeOnBoardTypes = await colleagueFinanceReferenceDataRepository.GetFreeOnBoardTypeGuidAsync(source.Fob);
                    if (!string.IsNullOrEmpty(freeOnBoardTypes))
                    {
                        requisitionsShipping.FreeOnBoard = new GuidObject2(freeOnBoardTypes);
                        requisition.Shipping = requisitionsShipping;
                    }
                }

                requisition.OverrideShippingDestination = await BuildOverrideShippingDestinationDtoAsync(source, bypassCache);

                var requisitionVendor = new VendorDtoProperty();

                if (!string.IsNullOrEmpty(source.VendorId))
                {
                    var vendorGuid = await vendorsRepository.GetVendorGuidFromIdAsync(source.VendorId);
                    if (string.IsNullOrEmpty(vendorGuid))
                    {
                        throw new Exception(string.Concat("Missing vendor information for requisition: ", source.Id, " Guid: ", source.Guid, " Vendor ID: ", source.VendorId));
                    }
                    var existingVendor = new ExistingVendorDetailsDtoProperty();
                    existingVendor.Vendor = new GuidObject2(vendorGuid);

                    var addressId = source.VendorAlternativeAddressId;
                    if (!string.IsNullOrEmpty(addressId))
                    {
                        try
                        {
                            var addressGuid = await this.addressRepository.GetAddressGuidFromIdAsync(addressId);
                            if (string.IsNullOrEmpty(addressGuid))
                            {
                                throw new KeyNotFoundException(string.Concat("Unable to retrieve AlternativeVendorAddress for requisition: ", source.Id, " Guid: ", source.Guid, " AlternativeVendorAddress: ", source.VendorAlternativeAddressId));
                            }
                            existingVendor.AlternativeVendorAddress = new GuidObject2(addressGuid);
                        }
                        catch
                        {
                            // Do nothing if invalid address ID.  Just leave address ID off the results.
                        }
                    }
                    requisitionVendor.ExistingVendor = existingVendor;
                }
                if ((requisitionVendor.ExistingVendor == null) || (requisitionVendor.ExistingVendor.AlternativeVendorAddress == null && source.UseAltAddress == true))
                {
                    requisitionVendor.ManualVendorDetails = await BuildManualVendorDetailsDtoAsync(source, bypassCache);
                }

                if (requisitionVendor.ExistingVendor != null || requisitionVendor.ManualVendorDetails != null)
                {
                    requisition.Vendor = requisitionVendor;
                }


                if (!string.IsNullOrEmpty(source.VendorTerms))
                {
                    var vendorTerms = await colleagueFinanceReferenceDataRepository.GetVendorTermGuidAsync(source.VendorTerms);
                    if (!string.IsNullOrEmpty(vendorTerms))
                    {
                        requisition.PaymentTerms = new GuidObject2(vendorTerms);
                    }
                }

                if (!string.IsNullOrEmpty(source.ApType))
                {
                    var apSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourceGuidAsync(source.ApType);
                    if (!string.IsNullOrEmpty(apSources))
                    {
                        requisition.PaymentSource = new GuidObject2(apSources);
                    }
                }

                var requisitionsComments = new List<CommentsDtoProperty>();
                if (!string.IsNullOrEmpty(source.Comments))
                {
                    var requisitionsComment = new CommentsDtoProperty();
                    requisitionsComment.Comment = source.Comments;  //PoPrintedComments
                    requisitionsComment.Type = CommentTypes.Printed;
                    requisitionsComments.Add(requisitionsComment);
                }
                if (!string.IsNullOrEmpty(source.InternalComments))
                {
                    var requisitionsComment = new CommentsDtoProperty();
                    requisitionsComment.Comment = source.InternalComments; //PoComments
                    requisitionsComment.Type = CommentTypes.NotPrinted;
                    requisitionsComments.Add(requisitionsComment);
                }
                if (requisitionsComments != null && requisitionsComments.Any())
                {
                    requisition.Comments = requisitionsComments;
                }

                var lineItems = new List<RequisitionsLineItemsDtoProperty>();
                foreach (var sourceLineItem in source.LineItems)
                {
                    var lineItem = await BuildRequisitionsLineItem(sourceLineItem, source.Id,
                        source.Guid, GlConfig, currency, bypassCache);
                    if (lineItem != null)
                    {
                        lineItems.Add(lineItem);
                    }
                }
                if (lineItems != null && lineItems.Any())
                {
                    requisition.LineItems = lineItems;
                }
                return requisition;
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat(ex.Message, "  Entity: 'REQUISITIONS', Record ID: '", source.Id, "'"));
            }
        }

        /// <summary>
        /// Converts a RequisitionsStatus domain enumeration to a RequisitionsStatus DTO enum 
        /// </summary>
        /// <param name="sourceStatus">RequisitionsStatus domain enumeration</param>
        /// <returns><see cref="RequisitionsStatus">RequisitionsStatus DTO enum</returns>
        private RequisitionsStatus ConvertRequisitionStatus(Domain.ColleagueFinance.Entities.RequisitionStatus? sourceStatus)
        {
            var requisitionStatus = RequisitionsStatus.NotSet;

            if (sourceStatus == null)
                return requisitionStatus;

            switch (sourceStatus)
            {
                case (Domain.ColleagueFinance.Entities.RequisitionStatus.InProgress):
                    requisitionStatus = RequisitionsStatus.Inprogress; break;
                case (Domain.ColleagueFinance.Entities.RequisitionStatus.NotApproved):
                    requisitionStatus = RequisitionsStatus.Notapproved; break;
                case (Domain.ColleagueFinance.Entities.RequisitionStatus.Outstanding):
                    requisitionStatus = RequisitionsStatus.Outstanding; break;
                case (Domain.ColleagueFinance.Entities.RequisitionStatus.PoCreated):
                    requisitionStatus = RequisitionsStatus.Purchaseordercreated; break;
                default:
                    break;
            }
            return requisitionStatus;
        }

        /// <summary>
        ///  Build a RequisitionsLineItemsDtoProperty from a LineItem domain entity
        /// </summary>
        /// <param name="sourceLineItem"></param>
        /// <param name="sourceId"></param>
        /// <param name="sourceGuid"></param>
        /// <param name="currency"></param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns><see cref="RequisitionsLineItemsDtoProperty"></returns>
        private async Task<RequisitionsLineItemsDtoProperty> BuildRequisitionsLineItem(Domain.ColleagueFinance.Entities.LineItem sourceLineItem, string sourceId, string sourceGuid,
            GeneralLedgerAccountStructure GlConfig, CurrencyIsoCode currency = CurrencyIsoCode.USD, bool bypassCache = false)
        {

            if (sourceLineItem == null)
            {
                throw new ArgumentNullException(string.Concat("Unable to retrieve Requisition line item: ", sourceId, " Guid: ", sourceGuid));
            }

            try
            {
                var lineItem = new RequisitionsLineItemsDtoProperty();

                lineItem.LineItemNumber = sourceLineItem.Id;
                lineItem.Description = !(string.IsNullOrEmpty(sourceLineItem.Description)) ? sourceLineItem.Description.Trim() : string.Empty;

                if (!string.IsNullOrEmpty(sourceLineItem.CommodityCode))
                {
                    var commodityCodes = await colleagueFinanceReferenceDataRepository.GetCommodityCodeGuidAsync(sourceLineItem.CommodityCode);
                    if (!string.IsNullOrEmpty(commodityCodes))
                    {
                        lineItem.CommodityCode = new GuidObject2(commodityCodes);
                    }
                }
                if (!(string.IsNullOrWhiteSpace(sourceLineItem.VendorPart)))
                {
                    lineItem.PartNumber = sourceLineItem.VendorPart;
                }
                lineItem.DesiredDate = sourceLineItem.DesiredDate;
                lineItem.Quantity = sourceLineItem.Quantity;

                if (!string.IsNullOrEmpty(sourceLineItem.UnitOfIssue))
                {
                    var commodityCodeUnitTypes = await colleagueFinanceReferenceDataRepository.GetCommodityUnitTypeGuidAsync(sourceLineItem.UnitOfIssue);
                    if (!string.IsNullOrEmpty(commodityCodeUnitTypes))
                    {
                        lineItem.UnitOfMeasure = new GuidObject2(commodityCodeUnitTypes);
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
                    var lineItemTaxesTuple = sourceLineItem.LineItemTaxes
                        .GroupBy(l => l.TaxCode)
                        .Select(cl => new Tuple<string, decimal?>(
                               cl.First().TaxCode,
                               cl.Sum(c => c.TaxAmount)
                            )).ToList();

                    foreach (var lineItemTax in lineItemTaxesTuple)
                    {
                        var taxCode = await referenceDataRepository.GetCommerceTaxCodeGuidAsync(lineItemTax.Item1);
                        if (!string.IsNullOrEmpty(taxCode))
                        {
                            lineItemTaxCodes.Add(new GuidObject2(taxCode));
                        }
                    }
                }
                if (lineItemTaxCodes != null && lineItemTaxCodes.Any())
                {
                    lineItem.Taxes = lineItemTaxCodes;
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
                    var lineItemComments = new List<CommentsDtoProperty>();

                    lineItemComments.Add(new CommentsDtoProperty()
                    {
                        Comment = sourceLineItem.Comments,
                        Type = CommentTypes.NotPrinted
                    });

                    lineItem.Comments = lineItemComments;
                }


                var accountDetails = new List<Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty>();

                foreach (var glDistribution in sourceLineItem.GlDistributions)
                {
                    if (!string.IsNullOrEmpty(glDistribution.GlAccountNumber))
                    {
                        var accountDetail = new Dtos.DtoProperties.RequisitionsAccountDetailDtoProperty();
                        /*var acctNumber = glDistribution.GlAccountNumber.Replace("_", "-");


                        accountDetail.AccountingString =
                            string.IsNullOrEmpty(glDistribution.ProjectId) ?
                                acctNumber : string.Concat(acctNumber, '*', glDistribution.ProjectId);
                       */
                        /*
                         *  string accountingString = transactionDetail.GlAccount.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                         accountingString = GetFormattedGlAccount(accountingString, GlConfig);
                         */

                        string acctNumber = glDistribution.GetFormattedGlAccount(GlConfig.MajorComponentStartPositions);
                        acctNumber = GetFormattedGlAccount(acctNumber, GlConfig);
                        //var acctNumber = glDistribution.GlAccountNumber.Replace("_", "-");
                        if (!string.IsNullOrEmpty(glDistribution.ProjectId))
                        {
                            acctNumber = ConvertAccountingstringToIncludeProjectRefNo(glDistribution.ProjectId, acctNumber);
                        }
                        accountDetail.AccountingString = acctNumber;

                        var allocation = new Dtos.DtoProperties.RequisitionsAllocationDtoProperty();

                        var allocated = new Dtos.DtoProperties.RequisitionsAllocatedDtoProperty();

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

                        accountDetails.Add(accountDetail);
                    }
                }
                if (accountDetails != null && accountDetails.Any())
                {
                    lineItem.AccountDetail = accountDetails;
                }

                return lineItem;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Build an ManualVendorDetailsDtoProperty DTO object from a Requisition entity
        /// </summary>
        /// <param name="source">Requisition Entity Object</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>An <see cref="ManualVendorDetailsDtoProperty"> ManualVendorDetailsDtoProperty object <see cref="ManualVendorDetailsDtoProperty"/> in EEDM format</returns>
        private async Task<ManualVendorDetailsDtoProperty> BuildManualVendorDetailsDtoAsync(Domain.ColleagueFinance.Entities.Requisition source,
            bool bypassCache = false)
        {
            var manualVendorDetailsDto = new Dtos.ManualVendorDetailsDtoProperty();
            manualVendorDetailsDto.Type = ConvertCorpPersonIndicator(source.IntgCorpPerIndicator);
            if (source.MiscName != null && source.MiscName.Any())
            {
                //manualVendorDetailsDto.Name = source.MiscName.FirstOrDefault();
                string name = string.Empty;
                foreach (var vouName in source.MiscName)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = string.Concat(name, " ");
                    }
                    name = string.Concat(name, vouName);
                }
                manualVendorDetailsDto.Name = name;
            }

            if (source.MiscAddress != null && source.MiscAddress.Any())
            {
                manualVendorDetailsDto.AddressLines = source.MiscAddress;
            }

            manualVendorDetailsDto.Place = await BuildAddressPlace(source.MiscCountry,
                source.MiscCity, source.MiscState, source.MiscZip,
                source.HostCountry, bypassCache);

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
            string addressState, string addressZip, string hostCountry, bool bypassCache = false)
        {
            var addressCountryDto = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;
            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(addressCountry))
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
            if (!string.IsNullOrEmpty(addressZip))
            {
                addressCountryDto.PostalCode = addressZip;
            }

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
        /// Build an OverrideShippingDestinationDtoProperty DTO object from a Requisition entity
        /// </summary>
        /// <param name="source">Requisition Entity Object</param>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>An <see cref="OverrideShippingDestinationDtoProperty"> OverrideShippingDestinationDtoProperty object <see cref="OverrideShippingDestinationDtoProperty"/> in EEDM format</returns>
        private async Task<OverrideShippingDestinationDtoProperty> BuildOverrideShippingDestinationDtoAsync(Domain.ColleagueFinance.Entities.Requisition source,
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

            var country = string.IsNullOrEmpty(source.IntgAltShipCountry) ?
                    source.MiscCountry : source.IntgAltShipCountry;

            overrideShippingDestinationDto.Place = await BuildAddressPlace(country,
                source.AltShippingCity, source.AltShippingState, source.AltShippingZip,
                source.HostCountry, bypassCache);

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
        /// ConvertCorpPersonIndicator
        /// </summary>
        /// <param name="corpPersonIndicator">string to convert</param>
        /// <returns><see cref="ManualVendorType"></returns>
        private ManualVendorType? ConvertCorpPersonIndicator(string corpPersonIndicator)
        {
            ManualVendorType? manualVendorType = null; ;

            if (string.IsNullOrEmpty(corpPersonIndicator))
                return manualVendorType;

            switch (corpPersonIndicator.ToLower())
            {
                case ("person"):
                    manualVendorType = ManualVendorType.Person; break;
                case ("organization"):
                    manualVendorType = ManualVendorType.Organization; break;
                default:
                    break;
            }
            return manualVendorType;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate information that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewRequisitionPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewRequisitions);
            if (!hasPermission)
            {
                hasPermission = HasPermission(ColleagueFinancePermissionCodes.UpdateRequisitions);
            }
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Requisitions.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a UPDATE operation. This API will integrate information that 
        /// could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdateRequisitionPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.UpdateRequisitions);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update Requisitions.");
            }
        }

        /// <summary>
        /// Permissions code that allows an external system to do a DELETE operation. 
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckDeleteRequisitionPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.DeleteRequisitions);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to delete Requisitions.");
            }
        }

        /// <summary>
        /// Get all CommodityUnitType Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="CommodityUnitType"> CommodityUnitType entity object</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityUnitType>> GetCommodityUnitTypesAsync(bool bypassCache)
        {
            if (_commodityUnitType == null)
            {
                _commodityUnitType = await colleagueFinanceReferenceDataRepository.GetCommodityUnitTypesAsync(bypassCache);
            }
            return _commodityUnitType;
        }

        /// <summary>
        /// Get all VendorTerm Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="VendorTerm"> VendorTerm entity object</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm>> GetVendorTermsAsync(bool bypassCache)
        {
            if (_vendorTerm == null)
            {
                _vendorTerm = await colleagueFinanceReferenceDataRepository.GetVendorTermsAsync(bypassCache);
            }
            return _vendorTerm;
        }

        /// <summary>
        /// Get all AccountsPayableSources Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="AccountsPayableSources"> AccountsPayableSources entity object</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources>> GetAllAccountsPayableSourcesAsync(bool bypassCache)
        {
            await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache);
            return _accountsPayableSources ?? (_accountsPayableSources = await colleagueFinanceReferenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache));
        }

        /// <summary>
        /// Get all FreeOnBoardType Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="FreeOnBoardType"> FreeOnBoardType entity objects</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool bypassCache)
        {
            if (_freeOnBoardType == null)
            {
                _freeOnBoardType = await colleagueFinanceReferenceDataRepository.GetFreeOnBoardTypesAsync(bypassCache);
            }
            return _freeOnBoardType;
        }

        /// <summary>
        /// Get all ShipToDestination Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="ShipToDestination"> ShipToDestination entity objects</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.ShipToDestination>> GetShipToDestinationsAsync(bool bypassCache)
        {
            if (_shipToDestination == null)
            {
                _shipToDestination = await colleagueFinanceReferenceDataRepository.GetShipToDestinationsAsync(bypassCache);
            }
            return _shipToDestination;
        }

        /// <summary>
        /// Get all CommodityCode Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="CommodityCode"> CommodityCode entity objects</returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.CommodityCode>> GetCommodityCodesAsync(bool bypassCache)
        {
            if (_commodityCode == null)
            {
                _commodityCode = await colleagueFinanceReferenceDataRepository.GetCommodityCodesAsync(bypassCache);
            }
            return _commodityCode;
        }

        /// <summary>
        /// Get all CommerceTaxCode Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="CommerceTaxCode"> CommerceTaxCode entity objects</returns>
        private async Task<IEnumerable<Domain.Base.Entities.CommerceTaxCode>> GetCommerceTaxCodesAsync(bool bypassCache)
        {
            if (_commerceTaxCode == null)
            {
                _commerceTaxCode = await referenceDataRepository.GetCommerceTaxCodesAsync(bypassCache);
            }
            return _commerceTaxCode;
        }

        /// <summary>
        /// Get all Country Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="Country"> Country entity objects</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        /// <summary>
        /// Get all State Entity Objects
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag.  If set to true, will requery cached items</param>
        /// <returns>A collection of <see cref="State"> State entity objects</returns>
        private async Task<IEnumerable<State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
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

        /// <summary>
        /// Permission code that allows a READ operation on a requisition.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckRequisitionViewPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewRequisition) || HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view requisitions.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a WRITE operation on a requisition.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckRequisitionCreateUpdatePermission()
        {

            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to create or modify requisitions.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a DELETE operation on a requisition.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckRequisitionDeletePermission()
        {

            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.DeleteRequisition);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to delete requisitions.", CurrentUser.PersonId);
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
            return HasPermission(ColleagueFinancePermissionCodes.ByPassRequisitionApproval);
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
        private static Requisition ConvertRequisitionDtoToEntity(Dtos.ColleagueFinance.Requisition requisition, GeneralLedgerAccountStructure glAccountStructure)
        {

            if (requisition == null)
                throw new ArgumentNullException("Requisition", "Must provide Requisition object");

            var reqStatus = ConvertRequisitionStatusDtoToEntity(requisition.Status);
            var requisitionId = !(string.IsNullOrEmpty(requisition.Id)) ? requisition.Id.Trim() : "NEW";
            var requisitionNumber = !(string.IsNullOrEmpty(requisition.Number)) ? requisition.Number : string.Empty;

            var requisitionEntity = new Domain.ColleagueFinance.Entities.Requisition(
                requisitionId, "NEW", requisitionNumber, requisition.VendorName, reqStatus, requisition.StatusDate, requisition.Date);


            if (!(string.IsNullOrWhiteSpace(requisition.ShipToCode)))
            {
                requisitionEntity.ShipToCode = requisition.ShipToCode.ToUpper();
            }
            if ((requisition.Approvers != null) && (requisition.Approvers.Any()))
            {
                foreach (var approver in requisition.Approvers)
                {
                    if (approver != null && !string.IsNullOrEmpty(approver.ApproverId))
                    {
                        // Approver Initials needs to be uppercase
                        var approverEntity = new Approver(approver.ApproverId.ToUpper());
                        requisitionEntity.AddApprover(approverEntity);
                    }
                }
            }
            if ((requisition.LineItems != null) && (requisition.LineItems.Any()))
            {
                foreach (var lineItem in requisition.LineItems)
                {
                    if (lineItem != null)
                    {
                        // the requisition line item description is no longer restricted to a 25 character string
                        var description = !(string.IsNullOrEmpty(lineItem.Description)) ? lineItem.Description.Trim() : string.Empty;

                        decimal quantity = lineItem.Quantity;

                        var lineItemId = !(string.IsNullOrEmpty(lineItem.Id)) ? lineItem.Id.Trim() : "NEW";

                        var apLineItem = new LineItem(lineItemId, description, quantity, lineItem.Price, lineItem.ExtendedPrice);
                        apLineItem.VendorPart = !(string.IsNullOrEmpty(lineItem.VendorPart)) ? lineItem.VendorPart.Trim() : string.Empty;
                        apLineItem.UnitOfIssue = !(string.IsNullOrEmpty(lineItem.UnitOfIssue)) ? lineItem.UnitOfIssue.Trim() : string.Empty;
                        apLineItem.Comments = !(string.IsNullOrEmpty(lineItem.Comments)) ? lineItem.Comments.Trim() : string.Empty;
                        apLineItem.TradeDiscountPercentage = lineItem.TradeDiscountPercentage;
                        apLineItem.TradeDiscountAmount = lineItem.TradeDiscountAmount;
                        apLineItem.FixedAssetsFlag = !(string.IsNullOrEmpty(lineItem.FixedAssetsFlag)) ? lineItem.FixedAssetsFlag.Trim() : string.Empty;

                        apLineItem.CommodityCode = lineItem.CommodityCode;
                        apLineItem.DesiredDate = lineItem.DesiredDate;
                        apLineItem.TaxForm = lineItem.TaxForm;
                        apLineItem.TaxFormCode = lineItem.TaxFormCode;
                        apLineItem.TaxFormLocation = lineItem.TaxFormLocation;

                        foreach (var glAccount in lineItem.GlDistributions)
                        {
                            string glAccountNo = !(string.IsNullOrEmpty(glAccount.FormattedGlAccount)) ? glAccount.FormattedGlAccount : "MASKED";
                            var internalGlAccountNo = GlAccountUtility.ConvertGlAccountToInternalFormat(glAccountNo, glAccountStructure.MajorComponentStartPositions);

                            apLineItem.AddGlDistributionForSave(new LineItemGlDistribution(internalGlAccountNo, glAccount.Quantity, glAccount.Amount)
                            {
                                ProjectNumber = glAccount.ProjectNumber
                            });
                        }
                        if (requisitionId.Equals("NEW"))
                        {
                            //add tax codes to the entity
                            if (lineItem.ReqLineItemTaxCodes != null && lineItem.ReqLineItemTaxCodes.Any())
                            {
                                foreach (var taxcode in lineItem.ReqLineItemTaxCodes)
                                {
                                    if (taxcode != null && !string.IsNullOrEmpty(taxcode.TaxReqTaxCode))
                                    {
                                        apLineItem.AddReqTax(new LineItemReqTax(taxcode.TaxReqTaxCode));
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
                                        apLineItem.AddReqTax(new LineItemReqTax(taxcode.TaxReqTaxCode));
                                    }
                                }
                            }

                        }
                        requisitionEntity.AddLineItem(apLineItem);
                    }
                }
            }
            requisitionEntity.VendorId = requisition.VendorId;
            if (!(string.IsNullOrWhiteSpace(requisition.ApType)))
            {
                requisitionEntity.ApType = requisition.ApType.ToUpper();
            }
            if ((requisition.Approvers != null) && (requisition.Approvers.Any()))
            {
                foreach (var approver in requisition.Approvers)
                {
                    if (!string.IsNullOrEmpty(approver.ApproverId))
                    {
                        var approverEntity = new Approver(approver.ApproverId);
                        requisitionEntity.AddApprover(approverEntity);
                    }
                }
            }

            requisitionEntity.Comments = requisition.Comments;
            requisitionEntity.InternalComments = requisition.InternalComments;
            requisitionEntity.DesiredDate = requisition.DesiredDate;
            return requisitionEntity;

        }

        private async Task AssignGlDescription(GeneralLedgerAccountStructure glAccountStructure, Requisition requisitionDomainEntity)
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

        private static RequisitionCreateUpdateRequest ConvertCreateUpdateRequestDtoToEntity(Dtos.ColleagueFinance.RequisitionCreateUpdateRequest requisitionCreateUpdateRequest, GeneralLedgerAccountStructure glAccountStructure)
        {
            RequisitionCreateUpdateRequest createUpdateRequestEntity = new RequisitionCreateUpdateRequest();
            createUpdateRequestEntity.PersonId = requisitionCreateUpdateRequest.PersonId;
            createUpdateRequestEntity.ConfEmailAddresses = requisitionCreateUpdateRequest.ConfEmailAddresses;
            createUpdateRequestEntity.InitiatorInitials = requisitionCreateUpdateRequest.InitiatorInitials;
            createUpdateRequestEntity.IsPersonVendor = requisitionCreateUpdateRequest.IsPersonVendor;
            createUpdateRequestEntity.Requisition = ConvertRequisitionDtoToEntity(requisitionCreateUpdateRequest.Requisition, glAccountStructure);
            return createUpdateRequestEntity;
        }

        private static RequisitionDeleteRequest ConvertDeleteRequestDtoToEntity(Dtos.ColleagueFinance.RequisitionDeleteRequest requisitionDeleteRequest)
        {
            RequisitionDeleteRequest deleteRequestEntity = new RequisitionDeleteRequest();
            deleteRequestEntity.PersonId = requisitionDeleteRequest.PersonId;
            deleteRequestEntity.RequisitionId = requisitionDeleteRequest.RequisitionId;
            deleteRequestEntity.ConfirmationEmailAddresses = requisitionDeleteRequest.ConfirmationEmailAddresses;
            
            return deleteRequestEntity;
        }

        /// <summary>
        /// Converts a RequisitionsStatus DTO enumeration to a RequisitionsStatus domain enum 
        /// </summary>
        /// <param name="sourceStatus">RequisitionsStatus DTO enumeration</param>
        /// <returns><see cref="RequisitionsStatus">RequisitionsStatus domain enum</returns>
        private static RequisitionStatus ConvertRequisitionStatusDtoToEntity(Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionStatus sourceStatus)
        {
            var requisitionStatus = RequisitionStatus.InProgress;

            switch (sourceStatus)
            {
                case (Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionStatus.InProgress):
                    requisitionStatus = RequisitionStatus.InProgress; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionStatus.NotApproved):
                    requisitionStatus = RequisitionStatus.NotApproved; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionStatus.Outstanding):
                    requisitionStatus = RequisitionStatus.Outstanding; break;
                case (Ellucian.Colleague.Dtos.ColleagueFinance.RequisitionStatus.PoCreated):
                    requisitionStatus = RequisitionStatus.PoCreated; break;
                default:
                    break;
            }
            return requisitionStatus;
        }

        #endregion
    }
}
