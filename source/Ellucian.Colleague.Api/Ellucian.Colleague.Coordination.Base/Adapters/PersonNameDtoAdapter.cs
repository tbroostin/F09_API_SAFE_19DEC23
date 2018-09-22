// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a <see cref="Domain.Base.Entities.PersonName"/> from a <see cref="Dtos.Base.PersonName"/>
    /// </summary>
    public class PersonNameDtoAdapter : AutoMapperAdapter<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>
    {
        /// <summary>
        /// Instantiates a new <see cref="PersonNameAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonNameDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger){}

        /// <summary>
        /// Maps a <see cref="Domain.Base.Entities.PersonName"/> from a <see cref="Dtos.Base.PersonName"/>
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Base.PersonName"/> to map</param>
        /// <returns>The mapped <see cref="Domain.Base.Entities.PersonName"/></returns>
        public override Domain.Base.Entities.PersonName MapToType(Dtos.Base.PersonName source)
        {
            return new Domain.Base.Entities.PersonName(source.GivenName, source.MiddleName, source.FamilyName);
        }
    }
}
