// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A proxy access permission definition
    /// </summary>
    [Serializable]
    public class ProxyAccessPermission
    {
        private string _id;
        private string _proxySubjectId;
        private string _proxyUserId;
        private string _proxyWorkflowCode;
        private DateTime _startDate;

        /// <summary>
        /// The ID of the proxy access permission record
        /// </summary>
        public string Id
        {
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("ID already defined for proxy access permission.");
                }
                if (!string.IsNullOrEmpty(value))
                {
                    _id = value;
                }
            }
        }

        /// <summary>
        /// The ID of the user for whom access is being granted or revoked
        /// </summary>
        public string ProxySubjectId { get { return _proxySubjectId; } }

        /// <summary>
        /// The ID of the user to whom access is being granted or revoked
        /// </summary>
        public string ProxyUserId { get { return _proxyUserId; } }

        /// <summary>
        /// The workflow for which the user's access is being granted or revoked
        /// </summary>
        public string ProxyWorkflowCode { get { return _proxyWorkflowCode; } }

        /// <summary>
        /// Date on which proxy access starts
        /// </summary>
        public DateTime StartDate { get { return _startDate; } }

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
        public DateTime EffectiveDate { get { return EndDate.HasValue && EndDate.Value <= DateTime.Today ? EndDate.Value : StartDate; } }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyAccessPermission"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="proxySubjectId"></param>
        /// <param name="proxyUserId"></param>
        /// <param name="workflowCode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="useEmployeeGroups"></param>
        public ProxyAccessPermission(string id, string proxySubjectId, string proxyUserId, string workflowCode, DateTime startDate, DateTime? endDate, bool useEmployeeGroups = false)
        {
            if (string.IsNullOrEmpty(proxySubjectId))
            {
                throw new ArgumentNullException("proxySubjectId", "Proxy Subject User ID must have a value.");
            }
            if (string.IsNullOrEmpty(proxyUserId))
            {
                throw new ArgumentNullException("proxyUserId", "Proxy User ID must have a value.");
            }
            if (string.IsNullOrEmpty(workflowCode))
            {
                throw new ArgumentNullException("workflowCode", "Workflow code must have a value.");
            }
            if (startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate", "Start date must be specified.");
            }

            _id = id;
            _proxySubjectId = proxySubjectId;
            _proxyUserId = proxyUserId;
            _proxyWorkflowCode = workflowCode;
            _startDate = startDate;
            EndDate = endDate;

            if(useEmployeeGroups) // For Employee Proxy
            {
                IsGranted = (EndDate.HasValue && EndDate <= DateTime.Today) ? false : true;
            }
            else
            {
                IsGranted = StartDate <= DateTime.Today && (!EndDate.HasValue || EndDate.Value > DateTime.Today);
            }
        }       
    }
}
