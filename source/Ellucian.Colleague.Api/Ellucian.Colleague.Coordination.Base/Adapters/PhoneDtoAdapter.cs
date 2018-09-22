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
    /// Adapter for mapping Phone Dto to Entity
    /// </summary>
    public class PhoneDtoAdapter : AutoMapperAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>
    {
        /// <summary>
        /// Initializes a new instance of the PhoneDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PhoneDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a Phone Dto to the corresponding Entity
        /// </summary>
        /// <param name="Source">The Phone Dto</param>
        /// <returns>The corresponding Phone Entity</returns>
        public override Domain.Base.Entities.Phone MapToType(Dtos.Base.Phone Source)
        {
            Domain.Base.Entities.Phone phoneEntity = new Domain.Base.Entities.Phone(Source.Number, Source.TypeCode, Source.Extension);
            return phoneEntity;
        }
    }
}
