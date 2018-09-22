// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// GL Source Codes valcode
    /// </summary>
    [Serializable]
    public class GlSourceCodes : GuidCodeItem
    {

        /// <summary>
        /// Store the special processing number 3
        /// </summary>
        public string GlSourceCodeProcess3 { set; get; }

        /// <summary>
        /// gl Source codes
        /// </summary>
        /// <param name="guid">Guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public GlSourceCodes(string guid, string code, string description, string sp3)
            : base(guid, code, description)
        {
            GlSourceCodeProcess3 = sp3;
        }
    }
}
