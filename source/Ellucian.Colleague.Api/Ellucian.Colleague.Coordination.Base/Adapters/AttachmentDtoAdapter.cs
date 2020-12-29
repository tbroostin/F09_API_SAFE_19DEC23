// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a <see cref="Attachment"/> from a <see cref="Dtos.Base.Attachment"/>
    /// </summary>
    public class AttachmentDtoAdapter : AutoMapperAdapter<Dtos.Base.Attachment, Attachment>
    {
        /// <summary>
        /// Instantiates a new <see cref="AttachmentDtoAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public AttachmentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Base.AttachmentStatus, AttachmentStatus>();
        }

        /// <summary>
        /// Maps an <see cref="Attachment"/> from an <see cref="Dtos.Base.Attachment"/>
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Base.Attachment"/> to map</param>
        /// <returns>The mapped <see cref="Attachment"/></returns>
        public override Attachment MapToType(Dtos.Base.Attachment source)
        {
            return new Attachment(source.Id, source.CollectionId, source.Name,
                source.ContentType, source.Size, source.Owner)
            {
                CreatedAt = source.CreatedAt,
                CreatedBy = source.CreatedBy,
                DeletedAt = source.DeletedAt,
                DeletedBy = source.DeletedBy,                
                ModifiedAt = source.ModifiedAt,
                ModifiedBy = source.ModifiedBy,
                Status = ConvertStatusDtoToEntity(source.Status),
                TagOne = source.TagOne
            };
        }

        // Convert attachment DTO status to entity
        private AttachmentStatus ConvertStatusDtoToEntity(Dtos.Base.AttachmentStatus status)
        {
            switch (status)
            {
                case Dtos.Base.AttachmentStatus.Active:
                    return AttachmentStatus.Active;
                case Dtos.Base.AttachmentStatus.Deleted:
                    return AttachmentStatus.Deleted;
                default:
                    return AttachmentStatus.Active;
            }
        }
    }
}