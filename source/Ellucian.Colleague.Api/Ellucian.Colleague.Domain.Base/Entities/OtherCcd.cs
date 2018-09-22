// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// CCD Academic Credential
    /// </summary>
    [Serializable]
    public class OtherCcd : GuidCodeItem
    {

        /// <summary>
        /// The Credential Level
        /// </summary>
        public string CredentialLevel { get; set; }

        /// <summary>
        /// The Ccd Type
        /// </summary>
        public string CredentialTypeID { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OtherCcd"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the CCD</param>
        /// <param name="description">Description or Title of the CCD</param>
        public OtherCcd(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}
