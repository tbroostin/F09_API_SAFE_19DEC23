// Copyright 2019 Ellucian Company L.P. and its affiliates..

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class TaxFormBoxCodesService : BaseCoordinationService, ITaxFormBoxCodesService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public TaxFormBoxCodesService(IReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ICountryRepository countryRepository,
            IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets all box codes configured for tax forms
        /// </summary>        
        /// <returns>Collection of <see cref="Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes">boxcodes</see> objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>> GetAllTaxFormBoxCodesAsync()
        {
            var boxCodesList = new List<Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>();

            List<Domain.Base.Entities.BoxCodes> boxCodesEntities = null;

            boxCodesEntities = (await _referenceDataRepository.GetAllBoxCodesAsync()).ToList();
            // Create the adapter to convert BoxCodes domain entities to DTOs.
            var boxCodesDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.BoxCodes, Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>();

            if (boxCodesEntities != null && boxCodesEntities.Any())
            {
                //sort the entities by code
                boxCodesEntities = boxCodesEntities.OrderBy(x => x.Code).ToList();
                foreach (var tax in boxCodesEntities)
                {
                    boxCodesList.Add(boxCodesDtoAdapter.MapToType(tax));
                }
            }

            return boxCodesList;
        }
    }
}