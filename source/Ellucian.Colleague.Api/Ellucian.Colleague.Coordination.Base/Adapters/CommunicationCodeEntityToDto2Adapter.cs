/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter converts CommunicationCode domain entities to CommunicationCode2 DTOs
    /// </summary>
    public class CommunicationCodeEntityToDto2Adapter : AutoMapperAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode2>
    {
        /// <summary>
        /// Constructor adds a mapping dependency for CommunicationCodeHyperlink Domain to CommunicationCodeHyperlink DTO
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public CommunicationCodeEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.CommunicationCodeHyperlink, Dtos.Base.CommunicationCodeHyperlink>();
        }
    }
}
