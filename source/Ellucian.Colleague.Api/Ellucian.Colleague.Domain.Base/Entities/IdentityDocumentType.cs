// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// CitizenshipStatus
    /// </summary>
    [Serializable]
    public class IdentityDocumentType : GuidCodeItem
    {

        private IdentityDocumentTypeCategory _identityDocumentTypeCategory;
        public IdentityDocumentTypeCategory IdentityDocumentTypeCategory { get { return _identityDocumentTypeCategory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDocumentType"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the IdentityDocumentType</param>
        /// <param name="description">Description or Title of the IdentityDocumentType</param>
        /// <param name="identityDocumentTypeCategory">Identity Document Type Category of the IdentityDocumentType</param>
        public IdentityDocumentType(string guid, string code, string description, IdentityDocumentTypeCategory identityDocumentTypeCategory)
            : base (guid, code, description)
        {
            _identityDocumentTypeCategory = identityDocumentTypeCategory;
        }
    }
}
