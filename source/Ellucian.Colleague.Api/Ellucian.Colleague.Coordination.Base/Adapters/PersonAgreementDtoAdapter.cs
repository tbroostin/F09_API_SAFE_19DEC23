// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping PersonAgreement Dto to Entity
    /// </summary>
    public class PersonAgreementDtoAdapter : AutoMapperAdapter<Dtos.Base.PersonAgreement, Domain.Base.Entities.PersonAgreement>
    {
        /// <summary>
        /// Initializes a new instance of the PersonAgreementDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonAgreementDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.PersonAgreementStatus, Domain.Base.Entities.PersonAgreementStatus>();
        }

        /// <summary>
        /// Maps a PersonAgreement dto to the corresponding entity
        /// </summary>
        /// <param name="source">The <see cref="PersonAgreement"/> Dto</param>
        /// <returns>The corresponding <see cref="PersonAgreement"/> entity</returns>
        public override Domain.Base.Entities.PersonAgreement MapToType(Dtos.Base.PersonAgreement source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var status = source.Status.HasValue ? ((source.Status == Dtos.Base.PersonAgreementStatus.Accepted) ? Domain.Base.Entities.PersonAgreementStatus.Accepted : Domain.Base.Entities.PersonAgreementStatus.Declined) : (Domain.Base.Entities.PersonAgreementStatus?)null;
            var result = new Domain.Base.Entities.PersonAgreement(source.Id, source.PersonId, source.AgreementCode, source.AgreementPeriodCode, source.PersonCanDeclineAgreement, source.Title, source.DueDate, source.Text, status, source.ActionTimestamp);
            return result;
        }

    }
}
