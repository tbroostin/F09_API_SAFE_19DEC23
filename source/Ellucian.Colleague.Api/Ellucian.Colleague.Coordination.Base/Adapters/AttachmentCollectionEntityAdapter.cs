// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates an <see cref="AttachmentCollection"/> from an <see cref="Domain.Base.Entities.AttachmentCollection"/>
    /// </summary>
    public class AttachmentCollectionEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.AttachmentCollection, Dtos.Base.AttachmentCollection> 
    {
        /// <summary>
        /// Instantiates a new <see cref="AttachmentCollectionEntityAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public AttachmentCollectionEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
            AddMappingDependency<Domain.Base.Entities.AttachmentCollectionStatus, Dtos.Base.AttachmentCollectionStatus>();
            AddMappingDependency<Domain.Base.Entities.AttachmentAction, Dtos.Base.AttachmentAction>();
            AddMappingDependency<Domain.Base.Entities.AttachmentCollectionIdentity, Dtos.Base.AttachmentCollectionIdentity>();
            AddMappingDependency<Domain.Base.Entities.AttachmentCollectionIdentityType, Dtos.Base.AttachmentCollectionIdentityType>();
            AddMappingDependency<Domain.Base.Entities.AttachmentOwnerAction, Dtos.Base.AttachmentOwnerAction>();
        }
    }
}