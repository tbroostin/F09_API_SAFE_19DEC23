// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A proxy access permission definition
    /// </summary>
    public class ProxyAccessPermission
    {
        /// <summary>
        /// The ID of the proxy access permission record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The ID of the user for whom access is being granted or revoked
        /// </summary>
        public string ProxySubjectId { get; set; }

        /// <summary>
        /// The ID of the user to whom access is being granted or revoked
        /// </summary>
        public string ProxyUserId { get; set; }

        /// <summary>
        /// The workflow for which the user's access is being granted or revoked
        /// </summary>
        public string ProxyWorkflowCode { get; set; }

        /// <summary>
        /// Date on which proxy access starts
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Date on which proxy access ends
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Date by which reauthorization is required before access is revoked
        /// </summary>
        public DateTime? ReauthorizationDate { get; set; }

        /// <summary>
        /// Date on which proxy access was most recently updated
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// ID of the associated disclosure release response
        /// </summary>
        public string DisclosureReleaseDocumentId { get; set; }

        /// <summary>
        /// ID of the associated approval email response
        /// </summary>
        public string ApprovalEmailDocumentId { get; set; }

        /// <summary>
        /// Flag indicating whether access is being granted or revoked
        /// </summary>
        public bool IsGranted { get; set; }
    }
}
