/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter to convert a Timestamp DTO to a Timestamp Domain Entity
    /// </summary>
    [RegisterType]
    public class TimestampDtoToEntityAdapter : AutoMapperAdapter<Dtos.Base.Timestamp, Domain.Base.Entities.Timestamp>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public TimestampDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Maps the Timestamp DTO to a Timestamp Domain Entity.
        /// </summary>
        /// <param name="Source">The Timestamp DTO. If null, method returns null</param>
        /// <returns></returns>
        public override Domain.Base.Entities.Timestamp MapToType(Dtos.Base.Timestamp Source)
        {
            if (Source == null) return null;

            return new Domain.Base.Entities.Timestamp(Source.AddOperator, Source.AddDateTime, Source.ChangeOperator, Source.ChangeDateTime);
        }
    }
}
