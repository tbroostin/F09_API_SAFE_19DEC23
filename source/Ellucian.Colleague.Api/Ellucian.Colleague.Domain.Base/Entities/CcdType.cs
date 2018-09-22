// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Certification Level attribute  of CCD Types
    /// </summary>
    [Serializable]
    public class CcdType : CodeItem
    {
        /// <summary>
        /// The Credential Level
        /// </summary>
        public string CredentialLevel { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code"> code</param>
        /// <param name="description"> description</param>
        public CcdType(string code, string description, string credentialLevel)
            : base(code, description)
        {
            CredentialLevel = credentialLevel;
        }
    }
}
