/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayCycleService : BaseCoordinationService, IPayCycleService
    {
        private readonly IPayCycleRepository payCycleRepository;
        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public PayCycleService(
            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IPayCycleRepository payCycleRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            this.payCycleRepository = payCycleRepository;
        }

        /// <summary>
        /// Get all Pay Cycles
        /// </summary>
        /// <param name="lookbackDate">A optional date which is used to filter previous pay periods with end dates prior to this date.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.HumanResources.PayCycle>> GetPayCyclesAsync(DateTime? lookbackDate = null)
        {
            var payCycleEntities = await payCycleRepository.GetPayCyclesAsync(lookbackDate);
            var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayCycle, Ellucian.Colleague.Dtos.HumanResources.PayCycle>();
            return payCycleEntities.Select(payCycle => entityToDtoAdapter.MapToType(payCycle));
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-cycles
        /// </summary>
        /// <returns>Collection of PayCycles DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayCycles>> GetPayCyclesAsync(bool bypassCache = false)
        {
            var payCyclesCollection = new List<Ellucian.Colleague.Dtos.PayCycles>();

            var payCyclesEntities = await _referenceDataRepository.GetPayCyclesAsync(bypassCache);
            if (payCyclesEntities != null && payCyclesEntities.Any())
            {
                foreach (var payCycles in payCyclesEntities)
                {
                    payCyclesCollection.Add(ConvertPayCyclesEntityToDto(payCycles));
                }
            }
            return payCyclesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayCycles from its GUID
        /// </summary>
        /// <returns>PayCycles DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayCycles> GetPayCyclesByGuidAsync(string guid)
        {
            try
            {
                return ConvertPayCyclesEntityToDto((await _referenceDataRepository.GetPayCyclesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("pay-cycles not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("pay-cycles not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayCycle2 domain entity to its corresponding PayCycles DTO
        /// </summary>
        /// <param name="source">PayCycle2 domain entity</param>
        /// <returns>PayCycles DTO</returns>
        private Ellucian.Colleague.Dtos.PayCycles ConvertPayCyclesEntityToDto(PayCycle2 source)
        {
            var payCycles = new Ellucian.Colleague.Dtos.PayCycles();

            payCycles.Id = source.Guid;
            payCycles.Code = source.Code;
            payCycles.Title = source.Description;
            payCycles.Description = null;

            return payCycles;
        }
    }
}
