// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates an <see cref="Attachment"/> from an <see cref="Domain.Base.Entities.Attachment"/>
    /// </summary>
    public class AttachmentEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.Attachment, Dtos.Base.Attachment> 
    {
        /// <summary>
        /// Instantiates a new <see cref="AttachmentEntityAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public AttachmentEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
            AddMappingDependency<Domain.Base.Entities.AttachmentStatus, Dtos.Base.AttachmentStatus>();
        }
    }
}