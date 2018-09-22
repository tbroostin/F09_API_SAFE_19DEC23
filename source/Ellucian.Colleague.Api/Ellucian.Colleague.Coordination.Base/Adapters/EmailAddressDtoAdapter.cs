// Copyright 2015 Ellucian Company L.P. and its affiliates.
using AutoMapper;
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
    /// Adapter for mapping Email Address Dto to Entity
    /// </summary>
    public class EmailAddressDtoAdapter : AutoMapperAdapter<Dtos.Base.EmailAddress, Ellucian.Colleague.Domain.Base.Entities.EmailAddress>
    {
        /// <summary>
        /// Initializes a new instance of the EmailAddressDtoAdapter class
        /// Should only be called once per application domain.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public EmailAddressDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps an EmailAddress Dto to the corresponding Entity
        /// </summary>
        /// <param name="Source">The EmailAddress Dto</param>
        /// <returns>The corresponding EmailAddress Entity</returns>
        public override Domain.Base.Entities.EmailAddress MapToType(Dtos.Base.EmailAddress Source)
        {
            Domain.Base.Entities.EmailAddress emailAddressEntity = new Domain.Base.Entities.EmailAddress(Source.Value, Source.TypeCode);
            emailAddressEntity.IsPreferred = Source.IsPreferred;
            return emailAddressEntity;
        }
    }
}
