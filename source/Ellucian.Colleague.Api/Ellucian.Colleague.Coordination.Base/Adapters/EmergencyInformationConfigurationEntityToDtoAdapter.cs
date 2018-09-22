// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping Emergency Information Configuration Entity to Dto
    /// </summary>
    public class EmergencyInformationConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the EmergencyInformationConfigurationEntityToDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public EmergencyInformationConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Override the automapping to account for renamed property
        /// </summary>
        /// <param name="source">The source Emergency Information Configuration entity</param>
        /// <returns>The Emergency Information Configuration dto</returns>
        public override Dtos.Base.EmergencyInformationConfiguration MapToType(Domain.Base.Entities.EmergencyInformationConfiguration source)
        {
            return new Dtos.Base.EmergencyInformationConfiguration()
            {
                AllowOptOut = source.AllowOptOut,
                HideHealthConditions = source.HideHealthConditions,
                RequireContactToConfirm = source.RequireContact
            };
        }
    }
}
