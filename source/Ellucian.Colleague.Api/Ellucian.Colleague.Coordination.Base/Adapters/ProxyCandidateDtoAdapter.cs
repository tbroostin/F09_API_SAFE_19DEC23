// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a ProxyCandidate entity from the corresponding dto
    /// </summary>
    public class ProxyCandidateDtoAdapter : AutoMapperAdapter<Dtos.Base.ProxyCandidate, Domain.Base.Entities.ProxyCandidate>
    {
        /// <summary>
        /// Initializes a new instance of the ProxyCandidateDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public ProxyCandidateDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a ProxyCandidate dto to the corresponding entity
        /// </summary>
        /// <param name="source">The <see cref="ProxyCandidate"/> Dto</param>
        /// <returns>The corresponding <see cref="ProxyCandidate"/> entity</returns>
        public override Domain.Base.Entities.ProxyCandidate MapToType(Dtos.Base.ProxyCandidate source)
        {
            List<Domain.Base.Entities.PersonMatchResult> matches = null;
            if (source.ProxyMatchResults != null && source.ProxyMatchResults.Any())
            {
                var mapper = adapterRegistry.GetAdapter<Dtos.Base.PersonMatchResult, Domain.Base.Entities.PersonMatchResult>();
                matches = source.ProxyMatchResults.Select(x => mapper.MapToType(x)).ToList();
            }
            var result = new Domain.Base.Entities.ProxyCandidate(source.ProxySubject,
                source.RelationType,
                source.GrantedPermissions,
                source.FirstName,
                source.LastName,
                source.EmailAddress,
                matches)
                {
                    BirthDate = source.BirthDate,
                    EmailType = source.EmailType,
                    FormerFirstName = source.FormerFirstName,
                    FormerLastName = source.FormerLastName,
                    FormerMiddleName = source.FormerMiddleName,
                    Gender = source.Gender,
                    MiddleName = source.MiddleName,
                    Phone = source.Phone,
                    PhoneExtension = source.PhoneExtension,
                    PhoneType = source.PhoneType,
                    Prefix = source.Prefix,
                    Id = source.Id,
                    GovernmentId = source.GovernmentId,
                    Suffix = source.Suffix,
                };
            return result;
        }

    }
}
