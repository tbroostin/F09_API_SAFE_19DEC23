// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Fixed Asset transfer flag for a procurement lineitem
    /// </summary>
    [Serializable]
    public class FixedAssetsFlag : CodeItem
    {
        /// <summary>
        /// FixedAsset Transfer flag constructor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public FixedAssetsFlag(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}
