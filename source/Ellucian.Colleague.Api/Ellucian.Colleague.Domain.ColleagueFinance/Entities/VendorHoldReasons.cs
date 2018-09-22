// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Class for VendorHoldReasons
    /// </summary>
    [Serializable]
    public class VendorHoldReasons : GuidCodeItem
    {

         /// <summary>
        /// Vendor Hold Reasons
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public VendorHoldReasons(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}

