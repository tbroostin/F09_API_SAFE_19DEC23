// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// GeographicAreaType
    /// </summary>
    [Serializable]
    public class PrivacyStatus : GuidCodeItem
    {

        private PrivacyStatusType _privacyStatusType;
        public PrivacyStatusType PrivacyStatusType { get { return _privacyStatusType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivacyStatus"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the PrivacyStatus</param>
        /// <param name="description">Description or Title of the PrivacyStatus</param>
        /// <param name="privacyStatus">Privacy status type of PrivacyStatus</param>
        public PrivacyStatus(string guid, string code, string description, PrivacyStatusType privacyStatusType)
            : base (guid, code, description)
        {
            _privacyStatusType = privacyStatusType;
        }
    }
}
