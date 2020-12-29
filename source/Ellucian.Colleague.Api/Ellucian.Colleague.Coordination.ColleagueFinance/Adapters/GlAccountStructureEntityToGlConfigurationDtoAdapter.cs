// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    ///  Adapter for mapping the cost center GL account entity to a DTO.
    /// </summary>
    public class GlAccountStructureEntityToGlConfigurationDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure, Ellucian.Colleague.Dtos.ColleagueFinance.GeneralLedgerConfiguration>
    {
        public GlAccountStructureEntityToGlConfigurationDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)

            : base(adapterRegistry, logger)
        {
            // The General Ledger Component entity does not need a custom adapter, so add the mapping depency for it
            // and this adapter will map tht entity also
            AddMappingDependency<Domain.ColleagueFinance.Entities.GeneralLedgerComponent, Dtos.ColleagueFinance.GeneralLedgerComponent>();

        }

        /// <summary>
        /// Convert paramters from the GL account structure domain entity into the GL Configuration DTO.
        /// </summary>
        /// <param name="Source">The GL account structure domain entity.</param>
        /// <returns>A General Ledger Configuration DTO.</returns>
        public Dtos.ColleagueFinance.GeneralLedgerConfiguration MapToType(Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure Source)
        {
            var glConfigurationDto = new GeneralLedgerConfiguration();
            glConfigurationDto.MajorComponents = new List<Dtos.ColleagueFinance.GeneralLedgerComponent>();
            glConfigurationDto.SubComponents = new List<Dtos.ColleagueFinance.GeneralLedgerComponent>();

            var glComponentAdapter = adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.GeneralLedgerComponent, Dtos.ColleagueFinance.GeneralLedgerComponent>();

            foreach (var component in Source.MajorComponents)
            {
                var majorComponentDto = glComponentAdapter.MapToType(component);
                glConfigurationDto.MajorComponents.Add(majorComponentDto);
            }
            foreach (var component in Source.Subcomponents)
            {
                var subComponentDto = glComponentAdapter.MapToType(component);
                glConfigurationDto.SubComponents.Add(subComponentDto);
            }

            return glConfigurationDto;
        }
    }
}
