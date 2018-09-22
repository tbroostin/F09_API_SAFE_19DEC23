// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a ProxyCandidate dto from the corresponding entity
    /// </summary>
    public class ProxyCandidateEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.ProxyCandidate, Dtos.Base.ProxyCandidate>
    {
        /// <summary>
        /// Initializes a new instance of the ProxyCandidateDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public ProxyCandidateEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a ProxyCandidate dto to the corresponding entity
        /// </summary>
        /// <param name="source">The <see cref="ProxyCandidate"/> entity</param>
        /// <returns>The corresponding <see cref="ProxyCandidate"/> Dto</returns>
        public override Dtos.Base.ProxyCandidate MapToType(Domain.Base.Entities.ProxyCandidate source)
        {
            List<Dtos.Base.PersonMatchResult> matches = null;
            if (source.ProxyMatchResults != null && source.ProxyMatchResults.Any())
            {
                var mapper = new AutoMapperAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>(adapterRegistry, logger);
                matches = source.ProxyMatchResults.Select(x => mapper.MapToType(x)).ToList();
            }
            var result = new Dtos.Base.ProxyCandidate()
            {
                ProxySubject = source.ProxySubject,
                RelationType = source.RelationType,
                GrantedPermissions = source.GrantedPermissions,
                FirstName = source.FirstName,
                LastName = source.LastName,
                EmailAddress = source.EmailAddress,
                ProxyMatchResults = matches,
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
