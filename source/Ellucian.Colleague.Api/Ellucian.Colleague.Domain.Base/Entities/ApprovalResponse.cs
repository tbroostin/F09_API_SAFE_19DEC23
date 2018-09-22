// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class ApprovalResponse
    {
        // Private fields
        private string _Id;
        private readonly string _DocumentId;
        private readonly string _PersonId;
        private readonly string _UserId;
        private readonly DateTimeOffset _Received;
        private readonly bool _IsApproved;

        /// <summary>
        /// ID of approval response
        /// </summary>
        public string Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Approval response ID cannot be changed.");
                }
            }
        }

        /// <summary>
        /// ID of approval document
        /// </summary>
        public string DocumentId { get { return _DocumentId; } }

        /// <summary>
        /// ID for whom approval applies
        /// </summary>
        public string PersonId { get { return _PersonId; } }

        /// <summary>
        /// Login ID of user who gave the approval
        /// </summary>
        public string UserId { get { return _UserId; } }

        /// <summary>
        /// Date/time at which the approval was received
        /// </summary>
        public DateTimeOffset Received { get { return _Received; } }

        /// <summary>
        /// Approval indicator
        /// </summary>
        public bool IsApproved { get { return _IsApproved; } }

        /// <summary>
        /// Approval response constructor
        /// </summary>
        /// <param name="id">Approval response ID</param>
        /// <param name="documentId">ID of approval document</param>
        /// <param name="personId">ID for whom approval applies</param>
        /// <param name="userId">Login ID of user giving approval</param>
        /// <param name="received">Date/time when approval was received</param>
        /// <param name="isApproved">Approval indicator</param>
        public ApprovalResponse(string id, string documentId, string personId, string userId, DateTimeOffset received, bool isApproved)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId","Approval response document ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Approval response person ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId","Approval response user ID cannot be null or empty");
            }

            _Id = id;
            _DocumentId = documentId;
            _PersonId = personId;
            _UserId = userId;
            _Received = received;
            _IsApproved = isApproved;
        }
    }
}
