// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// GL Account
    /// </summary>
    [Serializable]
    public class GeneralLedgerAccount : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralLedgerAccount"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the GeneralLedgerAccount item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public GeneralLedgerAccount(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}
