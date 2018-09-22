//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class BargainingUnitsService : BaseCoordinationService, IBargainingUnitsService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public BargainingUnitsService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all employee classifications
        /// </summary>
        /// <returns>Collection of BargainingUnits DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BargainingUnit>> GetBargainingUnitsAsync(bool bypassCache = false)
        {
            var bargainingUnitCollection = new List<Ellucian.Colleague.Dtos.BargainingUnit>();

            var bargainingUnitEntities = await _referenceDataRepository.GetBargainingUnitsAsync(bypassCache);
            if (bargainingUnitEntities != null && bargainingUnitEntities.Count() > 0)
            {
                foreach (var bargainingUnit in bargainingUnitEntities)
                {
                    bargainingUnitCollection.Add(ConvertBargainingUnitEntityToDto(bargainingUnit));
                }
            }
            return bargainingUnitCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a bargaining unit from its GUID
        /// </summary>
        /// <returns>BargainingUnit DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.BargainingUnit> GetBargainingUnitsByGuidAsync(string guid)
        {
            try
            {
                return ConvertBargainingUnitEntityToDto((await _referenceDataRepository.GetBargainingUnitsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Bargaining unit not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BargainingUnit domain entity to its corresponding BargainingUnit DTO
        /// </summary>
        /// <param name="source">BargainingUnit domain entity</param>
        /// <returns>EmployeeClassification DTO</returns>
        private Ellucian.Colleague.Dtos.BargainingUnit ConvertBargainingUnitEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.BargainingUnit source)
        {
            var bargainingUnit = new Ellucian.Colleague.Dtos.BargainingUnit();

            bargainingUnit.Id = source.Guid;
            bargainingUnit.Code = source.Code;
            bargainingUnit.Title = source.Description;
            bargainingUnit.Description = null;

            return bargainingUnit;
        }

    }
   
}