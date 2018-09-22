// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter converts ProxyPermissionAssignment DTOs to ProxyPermissionAssignment domain entities
    /// </summary>
    public class ProxyPermissionAssignmentDtoAdapter : BaseAdapter<ProxyPermissionAssignment, Domain.Base.Entities.ProxyPermissionAssignment>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ProxyPermissionAssignmentDtoAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ProxyPermissionAssignmentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }


        public override Domain.Base.Entities.ProxyPermissionAssignment MapToType(ProxyPermissionAssignment source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A proxy permission assignment must be provided.");
            }
            var permissions = new List<Domain.Base.Entities.ProxyAccessPermission>();

            if (source.Permissions != null && source.Permissions.Any())
            {
                foreach (var access in source.Permissions)
                {
                    DateTime? endDate = null;
                    if (!access.IsGranted)
                    {
                        endDate = DateTime.Today;
                    }
                    var perm = new Domain.Base.Entities.ProxyAccessPermission(access.Id, access.ProxySubjectId, access.ProxyUserId, access.ProxyWorkflowCode, access.StartDate)
                    {
                        EndDate = endDate,
                        ApprovalEmailDocumentId = access.ApprovalEmailDocumentId,
                        DisclosureReleaseDocumentId = access.DisclosureReleaseDocumentId
                    };
                    permissions.Add(perm);
                }
            }
            var entity = new Domain.Base.Entities.ProxyPermissionAssignment(source.ProxySubjectId, permissions, source.IsReauthorizing);
            entity.ProxySubjectApprovalDocumentText.AddRange(source.ProxySubjectApprovalDocumentText);
            entity.ProxyEmailAddress = source.ProxyEmailAddress;
            entity.ProxyEmailType = source.ProxyEmailType;

            return entity;
        }
    }
}
