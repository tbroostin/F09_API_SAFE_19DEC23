// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Class for Account Components
    /// </summary>
    [Serializable]
    public class AccountComponents : GuidCodeItem
    {
        /// <summary>
        /// Account Components
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public AccountComponents(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}