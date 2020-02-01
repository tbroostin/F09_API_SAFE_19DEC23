// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IAccountsPayableTaxService interface
    /// </summary>
    [RegisterType]
    public class AccountsPayableTaxService : BaseCoordinationService, IAccountsPayableTaxService
    {
        private IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceRepository;

        // This constructor initializes the private attributes.
        public AccountsPayableTaxService(IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.colleagueFinanceReferenceRepository = colleagueFinanceReferenceRepository;
        }

        /// <summary>
        /// Returns a list of Accounts Payable Tax codes.
        /// </summary>
        /// <returns>A list of Accounts Payable Tax codes</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableTax>> GetAccountsPayableTaxesAsync()
        {
            // Get the domain entities
            var accountsPayableTaxDomainEntities = await colleagueFinanceReferenceRepository.GetAccountsPayableTaxCodesAsync();

            // Create the adapter to convert AccountsPayable tax domain entities to DTOs.
            var accountsPayableTaxDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableTax, Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableTax>();
            var accountsPayableTaxDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableTax>();
            if (accountsPayableTaxDomainEntities != null && accountsPayableTaxDomainEntities.Any())
            {
                //sort the entities on code, then by description
                accountsPayableTaxDomainEntities = accountsPayableTaxDomainEntities.OrderBy(x => x.Code).ToList();
                foreach (var tax in accountsPayableTaxDomainEntities)
                {
                    accountsPayableTaxDtos.Add(accountsPayableTaxDtoAdapter.MapToType(tax));
                }
            }
            return accountsPayableTaxDtos;
        }
    }
};