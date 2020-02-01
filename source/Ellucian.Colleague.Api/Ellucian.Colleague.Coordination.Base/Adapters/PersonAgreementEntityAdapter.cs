// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for mapping PersonAgreement Entity to DTO
    /// </summary>
    public class PersonAgreementEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.PersonAgreement, Dtos.Base.PersonAgreement>
    {
        /// <summary>
        /// Initializes a new instance of the PersonAgreementDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonAgreementEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.PersonAgreementStatus, Dtos.Base.PersonAgreementStatus>();
        }
    }
}
