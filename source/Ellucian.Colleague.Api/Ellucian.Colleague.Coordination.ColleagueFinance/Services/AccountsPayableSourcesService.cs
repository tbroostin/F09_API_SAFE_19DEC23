//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class AccountsPayableSourcesService : BaseCoordinationService, IAccountsPayableSourcesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AccountsPayableSourcesService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Gets all accounts-payable-sources
        /// </summary>
        /// <returns>Collection of AccountsPayableSources DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountsPayableSources>> GetAccountsPayableSourcesAsync(bool bypassCache = false)
        {
            var accountsPayableSourcesCollection = new List<Ellucian.Colleague.Dtos.AccountsPayableSources>();

            var accountsPayableSourcesEntities = await _referenceDataRepository.GetAccountsPayableSourcesAsync(bypassCache);
            if (accountsPayableSourcesEntities != null && accountsPayableSourcesEntities.Any())
            {
                foreach (var accountsPayableSources in accountsPayableSourcesEntities)
                {
                    accountsPayableSourcesCollection.Add(ConvertAccountsPayableSourcesEntityToDto(accountsPayableSources));
                }
            }
            return accountsPayableSourcesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 8.0</remarks>
        /// <summary>
        /// Get a AccountsPayableSources from its GUID
        /// </summary>
        /// <returns>AccountsPayableSources DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountsPayableSources> GetAccountsPayableSourcesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAccountsPayableSourcesEntityToDto((await _referenceDataRepository.GetAccountsPayableSourcesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("accounts-payable-sources not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AccountsPayableSources domain entity to its corresponding AccountsPayableSources DTO
        /// </summary>
        /// <param name="source">AccountsPayableSources domain entity</param>
        /// <returns>AccountsPayableSources DTO</returns>
        private Ellucian.Colleague.Dtos.AccountsPayableSources ConvertAccountsPayableSourcesEntityToDto(Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableSources source)
        {
            var accountsPayableSources = new Ellucian.Colleague.Dtos.AccountsPayableSources();

            accountsPayableSources.Id = source.Guid;
            accountsPayableSources.Code = source.Code;
            accountsPayableSources.Title = source.Description;
            accountsPayableSources.Description = null;
            if (!string.IsNullOrEmpty(source.directDeposit) && source.directDeposit.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                accountsPayableSources.DirectDeposit = DirectDeposit.Enabled;
            }
            else
            {
                accountsPayableSources.DirectDeposit = DirectDeposit.Disabled;
            }

            return accountsPayableSources;
        }


    }
}
