// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping Pilot phone Dto to Entity
    /// </summary>
    public class PilotPhoneDtoAdapter : AutoMapperAdapter<Dtos.Base.PilotPhoneNumber, Domain.Base.Entities.PilotPhoneNumber>
    {
        /// <summary>
        /// Initializes a new instance of the PilotPhoneDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PilotPhoneDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }
    }
}
