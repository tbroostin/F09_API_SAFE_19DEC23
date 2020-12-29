//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class TaxFormsService : BaseCoordinationService, ITaxFormsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _financeReferenceDataRepository;

        public TaxFormsService(
            IColleagueFinanceReferenceDataRepository financeReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _financeReferenceDataRepository = financeReferenceDataRepository;
        }

        #region GET Methods
        public async Task<IEnumerable<TaxForm>> GetTaxFormsAsync()
        {
            var taxFormEntities =  await _financeReferenceDataRepository.GetTaxFormsAsync();

            //sort the entities
            taxFormEntities = taxFormEntities.OrderBy(tf => tf.Code);

            // Get the right adapter for the type mapping
            var taxFormAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.TaxForm, TaxForm>();
            // Map the entity to the DTO
            var taxFormDtos = new List<TaxForm>();
            if (taxFormEntities != null && taxFormEntities.Any())
            {
                foreach (var taxform in taxFormEntities)
                {
                    taxFormDtos.Add(taxFormAdapter.MapToType(taxform));
                }
            }
            return taxFormDtos;
        }
        #endregion  

    }
}