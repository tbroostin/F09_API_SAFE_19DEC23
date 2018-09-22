// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IGlObjectCodeService interface.
    /// </summary>
    [RegisterType]
    public class GlObjectCodeService : BaseCoordinationService, IGlObjectCodeService
    {
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGlObjectCodeRepository glObjectCodeRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;

        // This constructor initializes the private attributes.
        public GlObjectCodeService(IGlObjectCodeRepository glObjectCodeRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.glObjectCodeRepository = glObjectCodeRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
        }

        /// <summary>
        /// Returns the GL object code DTOs that are associated with the user logged into self-service for the 
        /// specified fiscal year. We want the GL object codes corresponding to the filtered cost centers
        /// for the user, so we need to use the cost center filter criteria.
        /// </summary>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <returns>List of GL object code DTOs for the fiscal year.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.GlObjectCode>> QueryGlObjectCodesAsync(CostCenterQueryCriteria criteria)
        {
            // The query criteria can be empty, but it cannot be null.
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Filter component criteria must be specified.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("GL class configuration is not set up.");
            }

            // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ApplicationException("No GL user definition available.");
            }

            // If the user does not have any expense or revenue accounts assigned, return an empty list of DTOs.
            var glObjectCodeDtos = new List<Dtos.ColleagueFinance.GlObjectCode>();

            // For the object view, we will need all five types of accounts, not just revenue and expense types. 
            // If the user does not have any accounts assigned, return an empty list of DTOs.
            if ((generalLedgerUser.AllAccounts != null && generalLedgerUser.AllAccounts.Any()))
            {
                // Create the adapter to convert GL object code domain entities to DTOs.
                var glObjectCodeAdapter = new GlObjectCodeEntityToDtoAdapter(_adapterRegistry, logger);

                // If the user has GL accounts assigned, convert the filter criteria DTO
                // into a domain entity, and pass it into the GL object code repository.
                var costCenterCriteriaAdapter = new CostCenterQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
                var queryCriteriaEntity = costCenterCriteriaAdapter.MapToType(criteria);

                var glObjectCodes = await glObjectCodeRepository.GetGlObjectCodesAsync(generalLedgerUser, glAccountStructure, glClassConfiguration, queryCriteriaEntity, CurrentUser.PersonId);

                // Assign the GL account descriptions to the GL account entities.
                var allGlAccountEntities = glObjectCodes.SelectMany(x => x.GlAccounts)
                    .Union(glObjectCodes.SelectMany(x => x.Pools.Select(y => y.Umbrella)))
                    .Union(glObjectCodes.SelectMany(x => x.Pools.SelectMany(y => y.Poolees))).ToList();

                // Get the descriptions for the GL accounts returned from the object repository.
                var glAccountDescriptions = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(allGlAccountEntities.Select(x => x.GlAccountNumber).ToList(), glAccountStructure);

                foreach (var glAccountEntity in allGlAccountEntities)
                {
                    string description = "";
                    if (!string.IsNullOrEmpty(glAccountEntity.GlAccountNumber))
                    {
                        glAccountDescriptions.TryGetValue(glAccountEntity.GlAccountNumber, out description);
                    }

                    glAccountEntity.GlAccountDescription = description ?? string.Empty;
                }

                // Convert the domain entities into DTOs
                foreach (var entity in glObjectCodes)
                {
                    if (entity != null)
                    {
                        glObjectCodeDtos.Add(glObjectCodeAdapter.MapToType(entity, glAccountStructure.MajorComponentStartPositions));
                    }
                }
            }

            return glObjectCodeDtos;
        }

        /// <summary>
        /// Get a list of fiscal years for the GL object code view.
        /// </summary>
        /// <returns>A list of fiscal years available to filter data.</returns>
        public async Task<IEnumerable<string>> GetFiscalYearsAsync()
        {
            var fiscalYearConfiguration = await generalLedgerConfigurationRepository.GetFiscalYearConfigurationAsync();
            if (fiscalYearConfiguration == null)
            {
                throw new ApplicationException("Fiscal year configuration is not set up.");
            }

            return await generalLedgerConfigurationRepository.GetAllFiscalYearsAsync(fiscalYearConfiguration.FiscalYearForToday);
        }
    }
}
