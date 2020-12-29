// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

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
    /// This class implements the IAccountsPayableTypeService interface
    /// </summary>
    [RegisterType]
    public class AccountsPayableTypeService : BaseCoordinationService, IAccountsPayableTypeService
    {
        private IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceRepository;

        // This constructor initializes the private attributes.
        public AccountsPayableTypeService(IColleagueFinanceReferenceDataRepository colleagueFinanceReferenceRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.colleagueFinanceReferenceRepository = colleagueFinanceReferenceRepository;
        }

        /// <summary>
        /// Returns a list of AP type codes.
        /// </summary>
        /// <returns>A list of AP type codes</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableType>> GetAccountsPayableTypesAsync()
        {
            // Get the domain entities
            var ApTypeDomainEntities = await colleagueFinanceReferenceRepository.GetAccountsPayableTypeCodesAsync();

            // Create the adapter to convert AP type domain entities to DTOs.
            var ApTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountsPayableType, Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableType>();
            var ApTypeDtos = new List<Ellucian.Colleague.Dtos.ColleagueFinance.AccountsPayableType>();

            if (ApTypeDomainEntities != null && ApTypeDomainEntities.Any())
            {
                //sort the entities on code, then by description
                ApTypeDomainEntities = ApTypeDomainEntities.OrderBy(x => x.Code);
                foreach (var type in ApTypeDomainEntities)
                {
                    ApTypeDtos.Add(ApTypeDtoAdapter.MapToType(type));
                }
            }
            return ApTypeDtos;
        }
    }
}